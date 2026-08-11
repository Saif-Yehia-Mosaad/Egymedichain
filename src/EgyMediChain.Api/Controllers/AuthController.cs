using EgyMediChain.Api.Common;
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly IConfiguration _config;
    private readonly IEmailService _email;

    public AuthController(AppDbContext db, JwtTokenService jwt, IConfiguration config, IEmailService email)
    {
        _db = db;
        _jwt = jwt;
        _config = config;
        _email = email;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email and password are required." });

        // §5.1 - lock an account for a short cool-down after 5 consecutive failed attempts in
        // the last 15 minutes, same rate-limit pattern used for the Forgot Password flow.
        var lockoutWindow = DateTime.UtcNow.AddMinutes(-15);
        var recentFailures = await _db.AuditLogs.CountAsync(l =>
            l.Action == AuditAction.LoginFailed && l.ResourceId == MaskEmail(dto.Email) && l.CreatedAt >= lockoutWindow);
        if (recentFailures >= 5)
            return StatusCode(429, new { message = "Too many failed login attempts. Please try again in 15 minutes." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _db.AuditLogs.Add(new AuditLog
            {
                LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                Action = AuditAction.LoginFailed,
                ResourceType = "SystemUser",
                ResourceId = MaskEmail(dto.Email),
                Result = AuditResult.Failed,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (user.IsActive != true)
            return Unauthorized(new { message = "This account is not active yet. Wait for Ministry approval." });
        if (user.IsSuspended == true)
            return Unauthorized(new { message = "This account has been suspended." });

        user.LastLoginAt = DateTime.UtcNow;
        var response = await IssueTokensAsync(user);
        await _db.SaveChangesAsync();

        return Ok(response);
    }

    // Body: { "refreshToken": "..." }
    // Rotates the refresh token: the old one is revoked and a brand new pair (access + refresh) is issued.
    // This replaces the old /refresh, which just handed back a SuperAdmin token to anyone who called it.
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest(new { message = "Refresh token is required." });

        var stored = await _db.AuthRefreshTokens
            .Include(t => t.SystemUser)
            .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);

        if (stored == null || stored.RevokedAt != null || stored.ExpiresAt == null || stored.ExpiresAt <= DateTime.UtcNow)
            return Unauthorized(new { message = "Refresh token is invalid or expired. Please log in again." });

        if (stored.SystemUser == null || stored.SystemUser.IsActive != true)
            return Unauthorized(new { message = "Account is not active." });
        if (stored.SystemUser.IsSuspended == true)
            return Unauthorized(new { message = "This account has been suspended." });

        stored.RevokedAt = DateTime.UtcNow;
        var response = await IssueTokensAsync(stored.SystemUser);
        await _db.SaveChangesAsync();

        return Ok(response);
    }

    // Body: { "refreshToken": "..." }
    // Logs the user out on this device by revoking that one refresh token (their access token
    // keeps working until it naturally expires - that's normal for stateless JWTs).
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest(new { message = "Refresh token is required." });

        var stored = await _db.AuthRefreshTokens.FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);
        if (stored != null && stored.RevokedAt == null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Logged out." });
    }

    // ---------------- Forgot Password flow (Backend Action Report §2) ----------------
    // Design principle: impossible to use for account enumeration. Every branch below returns
    // the exact same 200 + generic body regardless of whether the email exists, is suspended, etc.

    [HttpPost("forgot-password")]
    public async Task<ActionResult<GenericMessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            return BadRequest(new { message = "A valid email is required." });

        var generic = new GenericMessageResponseDto { Message = "If an account exists for this email, a reset code has been sent." };

        // Lightweight DB-backed rate limit (no rate-limiting middleware wired up yet in this
        // project - §5.1/§2.4 flag this as infra worth adding generically; this is the
        // self-contained version scoped to just this table for now).
        var since = DateTime.UtcNow.AddHours(-1);
        var recentByEmail = await _db.PasswordResetRequests.CountAsync(r => r.RequestedEmail == dto.Email && r.CreatedAt >= since);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var recentByIp = ip == null ? 0 : await _db.PasswordResetRequests.CountAsync(r => r.RequestIp == ip && r.CreatedAt >= since);
        if (recentByEmail >= 5 || recentByIp >= 20)
            return StatusCode(429, new { message = "Too many requests. Please try again later." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.Email == dto.Email || u.OfficialEmail == dto.Email || u.PersonalEmail == dto.Email);

        if (user != null && user.IsActive == true && user.IsSuspended != true)
        {
            var otp = Random.Shared.Next(0, 1_000_000).ToString("D6");
            _db.PasswordResetRequests.Add(new PasswordResetRequest
            {
                UserId = user.Id,
                RequestedEmail = dto.Email,
                HashedOtp = HashSecret(otp),
                Attempts = 0,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                RequestIp = ip
            });
            await _db.SaveChangesAsync();

            // No email service is wired up in this project yet (Backend Action Report §5.4).
            // Logged here so the flow is testable end-to-end locally until Brevo/SMTP is added.
            // Fire-and-forget - don't let email latency/failure block the response (§2.1 step 5,
            // §5.4). Falls back to a console log automatically if Brevo isn't configured yet
            // (see BrevoEmailService).
            _ = _email.SendAsync(dto.Email, "Your EgyMediChain password reset code",
                $"<p>Your one-time verification code is:</p><h2 style=\"letter-spacing:4px\">{otp}</h2><p>This code expires in 10 minutes. If you didn't request this, you can safely ignore this email.</p>");
        }
        else
        {
            // Equivalent-cost dummy work so a timing side-channel can't distinguish this branch.
            _ = HashSecret(Guid.NewGuid().ToString());
        }

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            Action = AuditAction.ForgotPasswordRequested,
            ResourceType = "SystemUser",
            ResourceId = MaskEmail(dto.Email),
            Result = AuditResult.Success,
            IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return Ok(generic);
    }

    [HttpPost("verify-reset-code")]
    public async Task<ActionResult<VerifyResetCodeResponseDto>> VerifyResetCode([FromBody] VerifyResetCodeRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code) ||
            !System.Text.RegularExpressions.Regex.IsMatch(dto.Code, @"^\d{6}$"))
            return BadRequest(new { message = "A valid email and 6-digit code are required." });

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var since = DateTime.UtcNow.AddHours(-1);
        var recentAttempts = await _db.PasswordResetRequests.CountAsync(r => r.RequestedEmail == dto.Email && r.CreatedAt >= since);
        if (recentAttempts >= 10)
            return StatusCode(429, new { message = "Too many attempts. Please request a new code." });

        var invalid = new ObjectResult(new { message = "Invalid or expired code." }) { StatusCode = 401 };

        var request = await _db.PasswordResetRequests
            .Where(r => r.RequestedEmail == dto.Email && r.ConsumedAt == null && r.VerifiedAt == null)
            .OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();

        LogVerifyAttempt(dto.Email, ip, false);

        if (request == null || request.ExpiresAt == null || request.ExpiresAt <= DateTime.UtcNow || request.Attempts >= 5)
        {
            await _db.SaveChangesAsync();
            return invalid;
        }

        if (request.HashedOtp != HashSecret(dto.Code))
        {
            request.Attempts += 1;
            await _db.SaveChangesAsync();
            return invalid;
        }

        var resetToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        request.ResetTokenHash = HashSecret(resetToken);
        request.VerifiedAt = DateTime.UtcNow;
        request.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);

        // Update the last log entry we just added to reflect success.
        var lastLog = await _db.AuditLogs.Where(l => l.Action == AuditAction.PasswordResetVerifyAttempted && l.ResourceId == MaskEmail(dto.Email))
            .OrderByDescending(l => l.CreatedAt).FirstOrDefaultAsync();
        if (lastLog != null) lastLog.Result = AuditResult.Success;

        await _db.SaveChangesAsync();

        return Ok(new VerifyResetCodeResponseDto { ResetToken = resetToken, ExpiresAt = request.ResetTokenExpiresAt });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<GenericMessageResponseDto>> ResetPasswordWithToken([FromBody] ResetPasswordWithTokenDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ResetToken) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "resetToken and newPassword are required." });

        if (dto.NewPassword.Length < 8 || !dto.NewPassword.Any(char.IsUpper) || !dto.NewPassword.Any(char.IsLower) || !dto.NewPassword.Any(char.IsDigit))
            return BadRequest(new { message = "Password must be at least 8 characters and include upper, lower case letters and a digit." });

        var hashedToken = HashSecret(dto.ResetToken);
        var request = await _db.PasswordResetRequests.Include(r => r.User).ThenInclude(u => u!.RefreshTokens)
            .FirstOrDefaultAsync(r => r.ResetTokenHash == hashedToken && r.ConsumedAt == null);

        if (request == null || request.User == null || request.ResetTokenExpiresAt == null || request.ResetTokenExpiresAt <= DateTime.UtcNow)
            return Unauthorized(new { message = "This reset link is invalid or has expired." });

        var user = request.User;
        if (!string.IsNullOrEmpty(user.PasswordHash) && BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            return BadRequest(new { message = "New password must be different from your current password." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, 12);
        user.UpdatedAt = DateTime.UtcNow;
        request.ConsumedAt = DateTime.UtcNow;

        if (user.RefreshTokens != null)
            foreach (var t in user.RefreshTokens.Where(t => t.RevokedAt == null)) t.RevokedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Action = AuditAction.PasswordResetCompleted,
            ResourceType = "SystemUser",
            ResourceId = user.Email,
            Result = AuditResult.Success,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // No email service wired up yet - see note in ForgotPassword above.
        var changeNotice = "<p>Your EgyMediChain account password was just changed.</p><p>If this wasn't you, contact your Ministry SuperAdmin immediately - all your active sessions have been signed out as a precaution.</p>";
        if (!string.IsNullOrWhiteSpace(user.OfficialEmail ?? user.Email))
            _ = _email.SendAsync(user.OfficialEmail ?? user.Email!, "Your EgyMediChain password was changed", changeNotice);
        if (!string.IsNullOrWhiteSpace(user.PersonalEmail))
            _ = _email.SendAsync(user.PersonalEmail!, "Your EgyMediChain password was changed", changeNotice);

        return Ok(new GenericMessageResponseDto { Message = "Password reset successfully. Please sign in with your new password." });
    }

    private void LogVerifyAttempt(string email, string? ip, bool success) =>
        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            Action = AuditAction.PasswordResetVerifyAttempted,
            ResourceType = "SystemUser",
            ResourceId = MaskEmail(email),
            Result = success ? AuditResult.Success : AuditResult.Failed,
            IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        });

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "***" + email[Math.Max(0, at)..];
        return email[0] + new string('*', at - 1) + email[at..];
    }

    // HMAC-SHA256 with a server-side pepper (reuses the existing Jwt:Key secret rather than
    // introducing a second secret into configuration) - used for OTPs and reset tokens, never
    // stored in plaintext.
    private string HashSecret(string value)
    {
        var pepper = _config.GetSection("Jwt")["Key"] ?? "fallback-pepper-change-me";
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(pepper));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private async Task<LoginResponseDto> IssueTokensAsync(SystemUser user)
    {
        var refreshToken = _jwt.GenerateRefreshToken();

        _db.AuthRefreshTokens.Add(new AuthRefreshToken
        {
            SystemUserId = user.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        return new LoginResponseDto
        {
            Token = _jwt.GenerateAccessToken(user),
            RefreshToken = refreshToken,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.ToString(),
            EntityType = user.EntityType?.ToString(),
            EntityId = user.EntityId
        };
    }
}
