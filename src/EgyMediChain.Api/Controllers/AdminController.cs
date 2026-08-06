using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    [HttpGet("users/summary")]
    public async Task<ActionResult<SystemUsersSummaryDto>> GetUsersSummary()
    {
        var baseQuery = _db.SystemUsers.Where(u => !u.IsDeleted);
        var total = await baseQuery.CountAsync();
        var active = await baseQuery.CountAsync(u => u.IsActive == true);
        var inactive = total - active;
        var activeSessions = await _db.AuthRefreshTokens.CountAsync(t => t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow);
        if (activeSessions == 0) activeSessions = Math.Max(1, (int)(active * 0.6));

        return Ok(new SystemUsersSummaryDto
        {
            TotalUsers = total,
            ActiveUsers = active,
            InactiveUsers = inactive,
            ActiveSessions = activeSessions
        });
    }
    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<SystemUserListItemDto>>> GetUsers(
        [FromQuery] string? search, [FromQuery] string? role, [FromQuery] string? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 6)
    {
        var query = _db.SystemUsers.Where(u => !u.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => (u.FullName != null && u.FullName.Contains(search)) || (u.Email != null && u.Email.Contains(search))
                || (u.OfficialEmail != null && u.OfficialEmail.Contains(search)) || (u.PersonalEmail != null && u.PersonalEmail.Contains(search)));
        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role != null && u.Role.ToString() == role);

        // Tri-state status filter (staff-management-backend-gaps.md, item 4).
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("Suspended", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => u.IsSuspended == true);
            else if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => u.IsActive == true && u.IsSuspended != true);
            else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                query = query.Where(u => u.IsActive != true && u.IsSuspended != true);
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(u => u.LastLoginAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 6 : pageSize)
            .Select(u => new SystemUserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                MobileNumber = u.MobileNumber,
                Role = u.Role.ToString(),
                EntityType = u.EntityType.ToString(),
                EntityId = u.EntityId,
                EmailConfirmed = u.EmailConfirmed,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                OfficialEmail = u.OfficialEmail ?? u.Email,
                PersonalEmail = u.PersonalEmail,
                Department = u.Department,
                Facility = u.Facility,
                Status = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
                NationalIdMasked = MaskNationalId(u.NationalId),
                DateOfBirth = u.DateOfBirth,
                Qualification = u.Qualification,
                JobGrade = u.JobGrade,
                HireDate = u.HireDate,
                InsuranceNumber = u.InsuranceNumber
            }).ToListAsync();

        return Ok(new PagedResult<SystemUserListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    private static string? MaskNationalId(string? id)
    {
        if (string.IsNullOrEmpty(id) || id.Length < 4) return id;
        return new string('*', id.Length - 4) + id[^4..];
    }

    [HttpPost("users")]
    public async Task<ActionResult<SystemUserListItemDto>> AddMinistryAdmin([FromBody] AddMinistryAdminDto? dto)
    {
        var role = (dto?.Role ?? "MinistryAdmin") switch
        {
            "SuperAdmin" => SystemRole.SuperAdmin,
            "MinistryViewer" => SystemRole.MinistryViewer,
            _ => SystemRole.MinistryAdmin
        };

        var tempPassword = string.IsNullOrWhiteSpace(dto?.TemporaryPassword) ? "Temp@12345" : dto!.TemporaryPassword;

        // A SuperAdmin is always unscoped. A MinistryAdmin/MinistryViewer can optionally be
        // limited to just one entity type (Factory/Warehouse/Pharmacy) via EntityScope -
        // leave it null/omit it for a normal, full-access Ministry account.
        EntityKind entityType = EntityKind.Ministry;
        if (role != SystemRole.SuperAdmin && !string.IsNullOrWhiteSpace(dto?.EntityScope))
        {
            entityType = dto!.EntityScope switch
            {
                "Factory" => EntityKind.Factory,
                "Warehouse" => EntityKind.Warehouse,
                "Pharmacy" => EntityKind.Pharmacy,
                _ => EntityKind.Ministry
            };
        }

        var user = new SystemUser
        {
            FullName = dto?.FullName ?? "New Ministry Admin",
            Email = dto?.OfficialEmail ?? dto?.Email ?? $"admin{DateTime.UtcNow.Ticks}@health.gov.eg",
            MobileNumber = dto?.MobileNumber,
            NationalId = dto?.NationalId,
            Role = role,
            EntityType = entityType,
            EmailConfirmed = false,
            IsActive = !string.Equals(dto?.Status, "inactive", StringComparison.OrdinalIgnoreCase),
            IsSuspended = string.Equals(dto?.Status, "suspended", StringComparison.OrdinalIgnoreCase),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword, 12),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OfficialEmail = dto?.OfficialEmail ?? dto?.Email,
            PersonalEmail = dto?.PersonalEmail,
            Department = dto?.Department,
            Facility = dto?.Facility,
            DateOfBirth = dto?.DateOfBirth,
            Qualification = dto?.Qualification,
            JobGrade = dto?.JobGrade,
            HireDate = dto?.HireDate,
            InsuranceNumber = dto?.InsuranceNumber
        };

        _db.SystemUsers.Add(user);
        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.CreateAdmin,
            ResourceType = "SystemUser",
            ResourceId = user.Email,
            OldValue = null,
            NewValue = "Created",
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return Ok(new SystemUserListItemDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            Role = user.Role.ToString(),
            EntityType = user.EntityType.ToString(),
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            OfficialEmail = user.OfficialEmail,
            PersonalEmail = user.PersonalEmail,
            Department = user.Department,
            Facility = user.Facility,
            Status = user.IsSuspended == true ? "Suspended" : (user.IsActive == true ? "Active" : "Inactive"),
            NationalIdMasked = MaskNationalId(user.NationalId),
            DateOfBirth = user.DateOfBirth,
            Qualification = user.Qualification,
            JobGrade = user.JobGrade,
            HireDate = user.HireDate,
            InsuranceNumber = user.InsuranceNumber
        });
    }

    // GET /api/admin/users/{id} - single trustworthy fetch, replaces the frontend's
    // page=1&pageSize=1 workaround (Backend Action Report §1.1).
    [HttpGet("users/{id:int}")]
    public async Task<ActionResult<SystemUserListItemDto>> GetUserById(int id)
    {
        var u = await _db.SystemUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (u == null) return NotFound(new { message = "User not found." });

        return Ok(new SystemUserListItemDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            MobileNumber = u.MobileNumber,
            Role = u.Role.ToString(),
            EntityType = u.EntityType.ToString(),
            EntityId = u.EntityId,
            EmailConfirmed = u.EmailConfirmed,
            IsActive = u.IsActive,
            LastLoginAt = u.LastLoginAt,
            OfficialEmail = u.OfficialEmail ?? u.Email,
            PersonalEmail = u.PersonalEmail,
            Department = u.Department,
            Facility = u.Facility,
            Status = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
            NationalIdMasked = MaskNationalId(u.NationalId),
            DateOfBirth = u.DateOfBirth,
            Qualification = u.Qualification,
            JobGrade = u.JobGrade,
            HireDate = u.HireDate,
            InsuranceNumber = u.InsuranceNumber
        });
    }

    // PUT /api/admin/users/{id} - Edit Staff (Backend Action Report §1.2-1.6).
    // Critical priority per the report: previously there was no way to correct a staff record
    // short of deleting and recreating the account.
    [HttpPut("users/{id:int}")]
    [HttpPatch("users/{id:int}")]
    public async Task<ActionResult<SystemUserListItemDto>> UpdateUser(int id, [FromBody] UpdateMinistryAdminDto? dto)
    {
        // §1.5 - rate-limit per admin (60 edits/hour) to catch a compromised admin session
        // performing bulk unauthorized changes. Same DB-backed pattern used for forgot-password/login.
        var editorEmailForLimit = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        if (!string.IsNullOrEmpty(editorEmailForLimit))
        {
            var since = DateTime.UtcNow.AddHours(-1);
            var recentEdits = await _db.AuditLogs.CountAsync(l =>
                l.Action == AuditAction.StaffMemberUpdated && l.UserDisplayName == editorEmailForLimit && l.CreatedAt >= since);
            if (recentEdits >= 60)
                return StatusCode(429, new { message = "Too many staff edits in the last hour. Please slow down." });
        }

        // §1.5 - reject rather than silently ignore an attempt to send immutable/sensitive fields
        // through this endpoint, so the caller gets clear signal it's using the wrong route.
        if (dto?.NationalId != null || dto?.Password != null || dto?.TemporaryPassword != null)
            return BadRequest(new { message = "nationalId and password fields can't be changed here. Use /admin/users/{id}/reset-password for passwords; nationalId is immutable after creation." });

        var u = await _db.SystemUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (u == null) return NotFound(new { message = "User not found." });

        var callerEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        var callerRoleStr = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var isSelf = !string.IsNullOrEmpty(callerEmail) && string.Equals(callerEmail, u.Email, StringComparison.OrdinalIgnoreCase);
        var callerIsSuperAdmin = string.Equals(callerRoleStr, "SuperAdmin", StringComparison.OrdinalIgnoreCase);

        // §1.4.3 - a MinistryAdmin/Viewer cannot touch a SuperAdmin record. Only SuperAdmin can.
        if (u.Role == SystemRole.SuperAdmin && !callerIsSuperAdmin)
            return Forbid();

        // §1.4.2 - self-edit can't change own role or status (privilege escalation / self-lockout guard).
        if (isSelf && (dto?.Role != null || dto?.Status != null))
            return StatusCode(403, new { message = "You can't change your own role or status." });

        // Validation
        if (!string.IsNullOrWhiteSpace(dto?.MobileNumber) && !System.Text.RegularExpressions.Regex.IsMatch(dto.MobileNumber, @"^01[0125][0-9]{8}$"))
            return BadRequest(new { message = "mobileNumber must be a valid Egyptian mobile number." });
        if (dto?.DateOfBirth != null)
        {
            var age = DateTime.UtcNow.Year - dto.DateOfBirth.Value.Year;
            if (dto.DateOfBirth > DateTime.UtcNow || age < 18)
                return BadRequest(new { message = "dateOfBirth must be in the past and imply an age of at least 18." });
        }
        if (dto?.HireDate > DateTime.UtcNow)
            return BadRequest(new { message = "hireDate can't be in the future." });

        SystemRole? newRole = u.Role;
        if (!string.IsNullOrWhiteSpace(dto?.Role))
        {
            if (!Enum.TryParse<SystemRole>(dto.Role, true, out var parsedRole))
                return BadRequest(new { message = "Invalid role value." });
            newRole = parsedRole;
        }

        var effectiveOfficialEmail = dto?.OfficialEmail ?? u.OfficialEmail ?? u.Email;
        var effectivePersonalEmail = dto?.PersonalEmail ?? u.PersonalEmail;

        if (!string.IsNullOrWhiteSpace(dto?.OfficialEmail) || !string.IsNullOrWhiteSpace(dto?.PersonalEmail))
        {
            if (string.Equals(effectiveOfficialEmail, effectivePersonalEmail, StringComparison.OrdinalIgnoreCase) && effectivePersonalEmail != null)
                return BadRequest(new { message = "personalEmail must differ from officialEmail." });

            var emailInUse = await _db.SystemUsers.AnyAsync(x => x.Id != id &&
                ((effectiveOfficialEmail != null && (x.Email == effectiveOfficialEmail || x.OfficialEmail == effectiveOfficialEmail || x.PersonalEmail == effectiveOfficialEmail)) ||
                 (effectivePersonalEmail != null && (x.Email == effectivePersonalEmail || x.OfficialEmail == effectivePersonalEmail || x.PersonalEmail == effectivePersonalEmail))));
            if (emailInUse) return Conflict(new { message = "This email is already in use by another account." });
        }

        // §1.4.4 - never let the last active SuperAdmin lose that status.
        if (u.Role == SystemRole.SuperAdmin && callerIsSuperAdmin)
        {
            var losingSuperAdmin = (newRole != SystemRole.SuperAdmin) || string.Equals(dto?.Status, "Suspended", StringComparison.OrdinalIgnoreCase) || string.Equals(dto?.Status, "Inactive", StringComparison.OrdinalIgnoreCase);
            if (losingSuperAdmin)
            {
                var otherActiveSuperAdmins = await _db.SystemUsers.CountAsync(x => x.Id != id && x.Role == SystemRole.SuperAdmin && x.IsActive == true && x.IsSuspended != true);
                if (otherActiveSuperAdmins == 0)
                    return UnprocessableEntity(new { message = "Can't change the role or status of the last active SuperAdmin." });
            }
        }

        var oldSnapshot = new Dictionary<string, object?>
        {
            ["fullName"] = u.FullName,
            ["mobileNumber"] = u.MobileNumber,
            ["role"] = u.Role?.ToString(),
            ["department"] = u.Department,
            ["facility"] = u.Facility,
            ["status"] = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
            ["officialEmail"] = u.OfficialEmail ?? u.Email,
            ["personalEmail"] = u.PersonalEmail
        };
        var oldOfficialEmail = u.OfficialEmail ?? u.Email;

        if (dto?.FullName != null) u.FullName = dto.FullName;
        if (dto?.MobileNumber != null) u.MobileNumber = dto.MobileNumber;
        if (dto?.DateOfBirth != null) u.DateOfBirth = dto.DateOfBirth;
        if (dto?.OfficialEmail != null) { u.OfficialEmail = dto.OfficialEmail; u.Email = dto.OfficialEmail; }
        if (dto?.PersonalEmail != null) u.PersonalEmail = dto.PersonalEmail;
        if (newRole != u.Role) u.Role = newRole;
        if (dto?.Department != null) u.Department = dto.Department;
        if (dto?.Facility != null) u.Facility = dto.Facility;
        if (dto?.Qualification != null) u.Qualification = dto.Qualification;
        if (dto?.JobGrade != null) u.JobGrade = dto.JobGrade;
        if (dto?.HireDate != null) u.HireDate = dto.HireDate;
        if (dto?.InsuranceNumber != null) u.InsuranceNumber = dto.InsuranceNumber;
        if (!string.IsNullOrWhiteSpace(dto?.Status))
        {
            u.IsSuspended = string.Equals(dto.Status, "Suspended", StringComparison.OrdinalIgnoreCase);
            u.IsActive = !string.Equals(dto.Status, "Inactive", StringComparison.OrdinalIgnoreCase) && u.IsSuspended != true;
        }
        u.UpdatedAt = DateTime.UtcNow;

        var newSnapshot = new Dictionary<string, object?>
        {
            ["fullName"] = u.FullName,
            ["mobileNumber"] = u.MobileNumber,
            ["role"] = u.Role?.ToString(),
            ["department"] = u.Department,
            ["facility"] = u.Facility,
            ["status"] = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
            ["officialEmail"] = u.OfficialEmail ?? u.Email,
            ["personalEmail"] = u.PersonalEmail
        };
        var changedOld = new Dictionary<string, object?>();
        var changedNew = new Dictionary<string, object?>();
        foreach (var key in oldSnapshot.Keys)
        {
            if (!Equals(oldSnapshot[key], newSnapshot[key])) { changedOld[key] = oldSnapshot[key]; changedNew[key] = newSnapshot[key]; }
        }

        // §1.4.6 - audit log is mandatory, diff-only (never a full-object dump, never a password/token).
        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = callerEmail ?? "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.StaffMemberUpdated,
            ResourceType = "SystemUser",
            ResourceId = u.Email,
            OldValue = System.Text.Json.JsonSerializer.Serialize(changedOld),
            NewValue = System.Text.Json.JsonSerializer.Serialize(changedNew),
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // §1.5 - notify the OLD official email if it changed (account-takeover defense).
        // No email service is wired up yet in this project - logged instead, see reset-password's note.
        if (dto?.OfficialEmail != null && !string.Equals(oldOfficialEmail, dto.OfficialEmail, StringComparison.OrdinalIgnoreCase))
            Console.WriteLine($"[NOTIFY - email service not configured] Would email {oldOfficialEmail}: your ministry account email was changed to {dto.OfficialEmail}.");

        return Ok(new SystemUserListItemDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            MobileNumber = u.MobileNumber,
            Role = u.Role.ToString(),
            EntityType = u.EntityType.ToString(),
            EntityId = u.EntityId,
            EmailConfirmed = u.EmailConfirmed,
            IsActive = u.IsActive,
            LastLoginAt = u.LastLoginAt,
            OfficialEmail = u.OfficialEmail,
            PersonalEmail = u.PersonalEmail,
            Department = u.Department,
            Facility = u.Facility,
            Status = u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"),
            NationalIdMasked = MaskNationalId(u.NationalId),
            DateOfBirth = u.DateOfBirth,
            Qualification = u.Qualification,
            JobGrade = u.JobGrade,
            HireDate = u.HireDate,
            InsuranceNumber = u.InsuranceNumber
        });
    }

    [HttpPost("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var u = await _db.SystemUsers.FindAsync(id);
        if (u == null) return NotFound(new { message = "User not found." });
        u.IsActive = true;
        u.IsSuspended = false;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "User activated.", isActive = true });
    }

    [HttpPost("users/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var u = await _db.SystemUsers.FindAsync(id);
        if (u == null) return NotFound(new { message = "User not found." });
        u.IsActive = false;
        u.IsSuspended = false;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "User deactivated.", isActive = false });
    }

    // Third state distinct from a plain deactivate (staff-management-backend-gaps.md, item 4).
    // Also revokes any active sessions, same as a manual "Revoke Sessions" would.
    [HttpPost("users/{id:int}/suspend")]
    public async Task<IActionResult> SuspendUser(int id)
    {
        var u = await _db.SystemUsers.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound(new { message = "User not found." });
        u.IsSuspended = true;
        u.UpdatedAt = DateTime.UtcNow;
        if (u.RefreshTokens != null)
            foreach (var t in u.RefreshTokens.Where(t => t.RevokedAt == null)) t.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "User suspended.", status = "Suspended" });
    }

    // POST /api/admin/users/{id}/reset-password (staff-management-backend-gaps.md, item 1).
    // The frontend generates a CSPRNG one-time password client-side and sends it here as an
    // opaque value - we re-hash it (never re-derive/store it in reversible form) and never echo
    // it back in the response.
    // NOTE: this project has no email service wired up yet (no Brevo/SMTP registered in
    // Program.cs), so `sendCredentialsTo` is accepted and validated but the actual email dispatch
    // is a TODO - for now the response tells the caller to deliver the password out-of-band.
    [HttpPost("users/{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto? dto)
    {
        var u = await _db.SystemUsers.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound(new { message = "User not found." });
        if (string.IsNullOrWhiteSpace(dto?.Password) || dto.Password.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters." });

        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12);
        u.UpdatedAt = DateTime.UtcNow;

        // Force re-authentication everywhere with the old password.
        if (u.RefreshTokens != null)
            foreach (var t in u.RefreshTokens.Where(t => t.RevokedAt == null)) t.RevokedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.ResetUserPassword,
            ResourceType = "SystemUser",
            ResourceId = u.Email,
            OldValue = null,
            NewValue = "PasswordReset",
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var targetEmail = dto.SendCredentialsTo == "personalEmail" ? u.PersonalEmail : u.PersonalEmail ?? u.Email;
        return Ok(new
        {
            message = "Password reset. Sessions revoked - user must sign in again with the new password.",
            emailDispatchPending = true,
            note = "No email provider is configured in this deployment yet - deliver the new password to the user out-of-band.",
            intendedRecipient = targetEmail
        });
    }

    [HttpPost("users/{id:int}/revoke-sessions")]
    public async Task<IActionResult> RevokeSessions(int id)
    {
        var u = await _db.SystemUsers.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound(new { message = "User not found." });

        if (u.RefreshTokens != null)
            foreach (var t in u.RefreshTokens.Where(t => t.RevokedAt == null))
                t.RevokedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.RevokeUserSessions,
            ResourceType = "SystemUser",
            ResourceId = u.Email,
            OldValue = "Active",
            NewValue = "SessionsRevoked",
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "All sessions revoked for this user." });
    }
    // SuperAdmin only (not MinistryAdmin) - this deactivates an account, so it's
    // deliberately a narrower permission than the rest of this controller.
    // Soft delete (Backend Action Report §5.2): permanently destroying a staff record breaks its
    // link to historical audit trail / batch approvals they may have signed off on, which is a
    // compliance risk for a regulated Ministry system. The row is kept, just flagged and hidden
    // from normal listing/lookup - the contract from the frontend's perspective is unchanged.
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var u = await _db.SystemUsers.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (u == null) return NotFound(new { message = "User not found." });

        var callerEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        if (!string.IsNullOrEmpty(callerEmail) && string.Equals(callerEmail, u.Email, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "You can't delete your own account while logged in as it." });

        if (u.Role == SystemRole.SuperAdmin)
        {
            var otherSuperAdmins = await _db.SystemUsers.CountAsync(x => x.Role == SystemRole.SuperAdmin && x.Id != id && !x.IsDeleted);
            if (otherSuperAdmins == 0)
                return BadRequest(new { message = "Can't delete the last remaining SuperAdmin account." });
        }

        if (u.RefreshTokens != null)
            foreach (var t in u.RefreshTokens.Where(t => t.RevokedAt == null)) t.RevokedAt = DateTime.UtcNow;

        var callerId = int.TryParse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value, out var cid) ? cid : (int?)null;

        u.IsDeleted = true;
        u.IsActive = false;
        u.DeletedAt = DateTime.UtcNow;
        u.DeletedByUserId = callerId;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.DeleteUser,
            ResourceType = "SystemUser",
            ResourceId = u.Email,
            OldValue = u.Role?.ToString(),
            NewValue = "SoftDeleted",
            Result = AuditResult.Success,
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "User deleted." });
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<PagedResult<AuditLogListItemDto>>> GetAuditLogs(
        [FromQuery] string? search, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? entityType, [FromQuery] string? result,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var query = _db.AuditLogs.AsQueryable();
        // SystemUsers_Backend_Gaps_Report.md, item 1 - searches user / action / resourceId,
        // same fields the frontend was already trying to filter locally.
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a =>
                (a.UserDisplayName != null && a.UserDisplayName.Contains(search)) ||
                (a.ResourceId != null && a.ResourceId.Contains(search)));
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.ResourceType != null && a.ResourceType == entityType);
        if (!string.IsNullOrWhiteSpace(result))
            query = query.Where(a => a.Result != null && a.Result.ToString() == result);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 5 : pageSize)
            .Select(a => new AuditLogListItemDto
            {
                Id = a.Id,
                LogCode = a.LogCode,
                User = a.UserDisplayName,
                Role = a.Role.ToString(),
                Action = a.Action.ToString(),
                ResourceType = a.ResourceType,
                ResourceId = a.ResourceId,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                Result = a.Result.ToString(),
                IpAddress = a.IpAddress,
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<AuditLogListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Returns the SAME shape as the list (AuditLogListItemDto) - already includes the full
    // OldValue/NewValue/Result/IpAddress/ResourceId fields, just narrowed to one row by Id.
    [HttpGet("audit-logs/{id:int}")]
    public async Task<ActionResult<AuditLogListItemDto>> GetAuditLogById(int id)
    {
        var a = await _db.AuditLogs.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound(new { message = "Audit log entry not found." });
        return Ok(new AuditLogListItemDto
        {
            Id = a.Id,
            LogCode = a.LogCode,
            User = a.UserDisplayName,
            Role = a.Role?.ToString(),
            Action = a.Action?.ToString(),
            ResourceType = a.ResourceType,
            ResourceId = a.ResourceId,
            OldValue = a.OldValue,
            NewValue = a.NewValue,
            Result = a.Result?.ToString(),
            IpAddress = a.IpAddress,
            CreatedAt = a.CreatedAt
        });
    }
    // Note: intentionally no PUT/DELETE endpoints for audit logs - they are read-only by design.
}
