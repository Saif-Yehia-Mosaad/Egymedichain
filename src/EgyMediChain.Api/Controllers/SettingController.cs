using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Backend Action Report §3 - previously the frontend persisted everything to localStorage only
// (settings didn't follow the user across devices and were invisible to admins for support).
[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    private int? CurrentUserId()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    // Self-scoped by the JWT's own subject claim, never by a client-supplied id - this is the
    // IDOR protection the report calls out as the main risk here (§3.1).
    [HttpGet("me")]
    public async Task<ActionResult<UserPreferencesDto>> GetMyPreferences()
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();

        var user = await _db.SystemUsers.FindAsync(userId.Value);
        if (user == null) return NotFound();

        var prefs = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

        return Ok(new UserPreferencesDto
        {
            Notifications = new NotificationPreferencesDto
            {
                EmailAlerts = prefs?.EmailAlerts ?? true,
                PushNotifications = prefs?.PushNotifications ?? true,
                CriticalAlerts = prefs?.CriticalAlerts ?? true,
                WeeklyReports = prefs?.WeeklyReports ?? false
            },
            AvatarUrl = prefs?.AvatarUrl,
            Profile = new UserProfileSummaryDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.ToString()
            }
        });
    }

    [HttpPut("me")]
    [HttpPatch("me")]
    public async Task<ActionResult<UserPreferencesDto>> UpdateMyPreferences([FromBody] UpdateUserPreferencesDto? dto)
    {
        var userId = CurrentUserId();
        if (userId == null) return Unauthorized();

        var user = await _db.SystemUsers.FindAsync(userId.Value);
        if (user == null) return NotFound();

        if (dto?.AvatarUrl != null && (dto.AvatarUrl.Length > 500 || !Uri.IsWellFormedUriString(dto.AvatarUrl, UriKind.Absolute)))
            return BadRequest(new { message = "avatarUrl must be a valid absolute URL under 500 characters." });

        var prefs = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (prefs == null)
        {
            prefs = new UserPreference { UserId = userId };
            _db.UserPreferences.Add(prefs);
        }

        if (dto?.Notifications != null)
        {
            prefs.EmailAlerts = dto.Notifications.EmailAlerts;
            prefs.PushNotifications = dto.Notifications.PushNotifications;
            prefs.CriticalAlerts = dto.Notifications.CriticalAlerts;
            prefs.WeeklyReports = dto.Notifications.WeeklyReports;
        }
        if (dto?.AvatarUrl != null) prefs.AvatarUrl = dto.AvatarUrl;
        prefs.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new UserPreferencesDto
        {
            Notifications = new NotificationPreferencesDto
            {
                EmailAlerts = prefs.EmailAlerts,
                PushNotifications = prefs.PushNotifications,
                CriticalAlerts = prefs.CriticalAlerts,
                WeeklyReports = prefs.WeeklyReports
            },
            AvatarUrl = prefs.AvatarUrl,
            Profile = new UserProfileSummaryDto { Id = user.Id, FullName = user.FullName, Email = user.Email, Role = user.Role?.ToString() }
        });
    }
}

// Ministry-wide policy config - SuperAdmin only (Backend Action Report §3.2). High blast radius
// (SessionTimeoutMinutes affects every active session going forward), so every write is audited.
[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = "SuperAdmin")]
public class SystemSettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SystemSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("system")]
    public async Task<ActionResult<SystemConfigDto>> GetSystemConfig()
    {
        var cfg = await _db.SystemConfigurations.FirstOrDefaultAsync();
        return Ok(new SystemConfigDto
        {
            TwoFactorRequired = cfg?.TwoFactorRequired ?? false,
            SessionTimeoutMinutes = cfg?.SessionTimeoutMinutes ?? 60
        });
    }

    [HttpPut("system")]
    public async Task<ActionResult<SystemConfigDto>> UpdateSystemConfig([FromBody] SystemConfigDto? dto)
    {
        if (dto == null) return BadRequest(new { message = "Request body is required." });
        if (dto.SessionTimeoutMinutes < 5 || dto.SessionTimeoutMinutes > 480)
            return BadRequest(new { message = "sessionTimeoutMinutes must be between 5 and 480." });

        var cfg = await _db.SystemConfigurations.FirstOrDefaultAsync();
        var old = cfg == null ? null : $"2FA={cfg.TwoFactorRequired}, Timeout={cfg.SessionTimeoutMinutes}";
        if (cfg == null)
        {
            cfg = new SystemConfiguration();
            _db.SystemConfigurations.Add(cfg);
        }
        cfg.TwoFactorRequired = dto.TwoFactorRequired;
        cfg.SessionTimeoutMinutes = dto.SessionTimeoutMinutes;
        cfg.UpdatedAt = DateTime.UtcNow;

        var callerEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = callerEmail ?? "SuperAdmin",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.SystemConfigUpdated,
            ResourceType = "SystemConfiguration",
            ResourceId = "system",
            OldValue = old,
            NewValue = $"2FA={cfg.TwoFactorRequired}, Timeout={cfg.SessionTimeoutMinutes}",
            Result = AuditResult.Success,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(dto);
    }
}
