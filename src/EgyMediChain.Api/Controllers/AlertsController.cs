using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer,FactoryUser,WarehouseUser,PharmacyUser")]
public class AlertsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AlertsController(AppDbContext db) => _db = db;

    [HttpGet("counts")]
    public async Task<ActionResult<AlertsCountsDto>> GetCounts()
    {
        var scope = GetMinistryEntityScope();
        var alertsQuery = _db.Alerts.AsQueryable();
        if (scope != null)
            alertsQuery = alertsQuery.Where(a => a.EntityType != null && a.EntityType.ToString() == scope);

        return Ok(new AlertsCountsDto
        {
            OpenAlerts = await alertsQuery.CountAsync(a => a.AlertStatus == AlertStatus.Open),
            PublicScanLogs = scope == null ? await _db.PublicVerificationScans.CountAsync() : 0,
            RecallAlerts = await alertsQuery.CountAsync(a => a.AlertType == AlertType.Recall)
        });
    }

    // ---------------- Open Alerts ----------------
    // Default pageSize raised from 5 -> 50 (Alerts_Backend_Gaps_Report.md, item 3): the frontend
    // currently loads everything once and paginates/filters client-side, so a tiny default was
    // silently truncating the dashboard to 5 rows. Cap stays at 200 to avoid an unbounded query.
    [HttpGet]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? severity, [FromQuery] string? entityType,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        var query = _db.Alerts.Include(a => a.Batch).AsQueryable();

        // Explicit filter from the frontend's dropdown - only narrows further within whatever
        // this account's Ministry scope already allows.
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType != null && a.EntityType.ToString() == entityType);

        var scope = GetMinistryEntityScope();
        if (scope != null)
            query = query.Where(a => a.EntityType != null && a.EntityType.ToString() == scope);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.AlertStatus != null && a.AlertStatus.ToString() == status);
        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(a => a.Severity != null && a.Severity.ToString() == severity);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // severity/status filters added server-side (Alerts_Backend_Gaps_Report.md, item 4) - previously
    // this only supported page/pageSize, forcing the frontend to filter locally.
    [HttpGet("recalls")]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetRecalls(
        [FromQuery] string? severity, [FromQuery] string? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        var query = _db.Alerts.Include(a => a.Batch).Where(a => a.AlertType == AlertType.Recall);

        var scope = GetMinistryEntityScope();
        if (scope != null)
            query = query.Where(a => a.EntityType != null && a.EntityType.ToString() == scope);

        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(a => a.Severity != null && a.Severity.ToString() == severity);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.AlertStatus != null && a.AlertStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();
        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlertListItemDto>> GetById(int id)
    {
        var a = await _db.Alerts.Include(x => x.Batch).FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound(new { message = "Alert not found." });
        if (!IsAllowedAlertEntityType(a.EntityType)) return Forbid403();

        return Ok(new AlertListItemDto
        {
            Id = a.Id,
            AlertCode = a.AlertCode,
            AlertType = a.AlertType?.ToString(),
            Severity = a.Severity?.ToString(),
            EntityType = a.EntityType?.ToString(),
            EntityName = a.EntityName,
            BatchNumber = a.Batch?.BatchNumber,
            Message = a.Message,
            AlertStatus = a.AlertStatus?.ToString(),
            CreatedAt = a.CreatedAt
        });
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAlertStatusDto? dto)
    {
        var a = await _db.Alerts.FindAsync(id);
        if (a == null) return NotFound(new { message = "Alert not found." });
        if (!IsAllowedAlertEntityType(a.EntityType)) return Forbid403();

        var old = a.AlertStatus?.ToString();
        var newStatus = (dto?.Status ?? "UnderReview") switch
        {
            "Resolved" => AlertStatus.Resolved,
            "Dismissed" => AlertStatus.Dismissed,
            "Open" => AlertStatus.Open,
            _ => AlertStatus.UnderReview
        };
        a.AlertStatus = newStatus;
        if (newStatus is AlertStatus.Resolved or AlertStatus.Dismissed) a.ResolvedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = newStatus == AlertStatus.Resolved ? AuditAction.ResolveAlert : AuditAction.DismissAlert,
            ResourceType = "Alert",
            ResourceId = a.AlertCode,
            OldValue = old,
            NewValue = newStatus.ToString(),
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Alert status updated.", status = newStatus.ToString() });
    }

    // Bulk status update for multiple alerts at once (Alerts_Backend_Gaps_Report.md, item 7).
    [HttpPost("bulk-status")]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkUpdateAlertStatusDto? dto)
    {
        if (dto?.AlertIds == null || dto.AlertIds.Count == 0)
            return BadRequest(new { message = "At least one alertId is required." });

        var newStatus = (dto.Status ?? "UnderReview") switch
        {
            "Resolved" => AlertStatus.Resolved,
            "Dismissed" => AlertStatus.Dismissed,
            "Open" => AlertStatus.Open,
            _ => AlertStatus.UnderReview
        };

        var alerts = await _db.Alerts.Where(a => dto.AlertIds.Contains(a.Id)).ToListAsync();
        var allowed = alerts.Where(a => IsAllowedAlertEntityType(a.EntityType)).ToList();

        foreach (var a in allowed)
        {
            a.AlertStatus = newStatus;
            if (newStatus is AlertStatus.Resolved or AlertStatus.Dismissed) a.ResolvedAt = DateTime.UtcNow;

            _db.AuditLogs.Add(new AuditLog
            {
                LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                UserDisplayName = "Dr. Saif",
                Role = SystemRole.SuperAdmin,
                Action = newStatus == AlertStatus.Resolved ? AuditAction.ResolveAlert : AuditAction.DismissAlert,
                ResourceType = "Alert",
                ResourceId = a.AlertCode,
                NewValue = newStatus.ToString(),
                IpAddress = "127.0.0.1",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"{allowed.Count} alert(s) updated.", updatedCount = allowed.Count, skippedCount = alerts.Count - allowed.Count });
    }

    // Ministry only - narrower than the rest of this controller (Factory/Warehouse/Pharmacy users
    // can view/update their own alerts but shouldn't be able to delete them). Only lets you delete
    // alerts that are already Resolved or Dismissed, so an active/open alert can never be erased.
    [Authorize(Roles = "SuperAdmin,MinistryAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Alerts.FindAsync(id);
        if (a == null) return NotFound(new { message = "Alert not found." });
        if (!IsAllowedAlertEntityType(a.EntityType)) return Forbid403();

        if (a.AlertStatus != AlertStatus.Resolved && a.AlertStatus != AlertStatus.Dismissed)
            return BadRequest(new { message = "Only Resolved or Dismissed alerts can be deleted." });

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.DeleteAlert,
            ResourceType = "Alert",
            ResourceId = a.AlertCode,
            OldValue = a.AlertStatus?.ToString(),
            NewValue = "Deleted",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        _db.Alerts.Remove(a);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Alert deleted." });
    }

    // ---------------- Public Scan Logs ----------------
    [HttpGet("public-scans")]
    public async Task<ActionResult<PagedResult<ScanListItemDto>>> GetPublicScans(
        [FromQuery] string? result, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
        var query = _db.PublicVerificationScans.AsQueryable();
        if (!string.IsNullOrWhiteSpace(result))
            query = query.Where(s => s.VerificationResult != null && s.VerificationResult.ToString() == result);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.ScannedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize)
            .Select(s => new ScanListItemDto
            {
                Id = s.Id,
                ScanCode = s.ScanCode,
                ScannedGTIN = s.ScannedGTIN,
                ScannedSerialNumber = s.ScannedSerialNumber,
                ScannedBatchNumber = s.ScannedBatchNumber,
                ProductName = s.ProductName,
                VerificationResult = s.VerificationResult.ToString(),
                Reason = s.Reason,
                Governorate = s.Governorate,
                City = s.City,
                ScannedAt = s.ScannedAt
            }).ToListAsync();

        return Ok(new PagedResult<ScanListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("public-scans/{id:int}")]
    public async Task<ActionResult<ScanDetailsDto>> GetScanDetails(int id)
    {
        var s = await _db.PublicVerificationScans
            .Include(x => x.UnitCode).ThenInclude(u => u!.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(x => x.UnitCode).ThenInclude(u => u!.Batch).ThenInclude(b => b!.Factory)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (s == null) return NotFound(new { message = "Scan not found." });

        var dto = new ScanDetailsDto
        {
            Id = s.Id,
            ScannedGTIN = s.ScannedGTIN,
            ScannedSerialNumber = s.ScannedSerialNumber,
            ScannedBatchNumber = s.ScannedBatchNumber,
            VerificationResult = s.VerificationResult?.ToString(),
            Reason = s.Reason,
            Governorate = s.Governorate,
            City = s.City,
            ScannedAt = s.ScannedAt,
            UnitCodeId = s.UnitCodeId
        };

        if (s.UnitCode != null)
        {
            dto.ProductName = s.UnitCode.Batch?.MedicineProduct?.ProductName;
            dto.BatchNumber = s.UnitCode.Batch?.BatchNumber;
            dto.FactoryName = s.UnitCode.Batch?.Factory?.OfficialFactoryName;
            dto.UnitStatus = s.UnitCode.UnitStatus?.ToString();
            dto.ScanCount = s.UnitCode.ScanCount;
            dto.FirstScannedAt = s.UnitCode.FirstScannedAt;
        }

        return Ok(dto);
    }

    [HttpPost("public-scans/{id:int}/create-alert")]
    public async Task<IActionResult> CreateAlertFromScan(int id, [FromBody] CreateAlertFromScanDto? dto)
    {
        var s = await _db.PublicVerificationScans.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound(new { message = "Scan not found." });

        var severity = (dto?.Severity ?? "High") switch
        {
            "Critical" => AlertSeverity.Critical,
            "Medium" => AlertSeverity.Medium,
            "Low" => AlertSeverity.Low,
            _ => AlertSeverity.High
        };

        var alert = new Alert
        {
            AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AlertType = AlertType.SuspiciousScan,
            Severity = severity,
            EntityType = EntityKind.Pharmacy,
            EntityName = "Public Scan",
            Message = dto?.Message ?? $"Suspicious scan reported for batch {s.ScannedBatchNumber}.",
            AlertStatus = AlertStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
        _db.Alerts.Add(alert);

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.CreateAlert,
            ResourceType = "PublicVerificationScan",
            ResourceId = s.ScanCode,
            OldValue = null,
            NewValue = "AlertCreated",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Alert created from scan.", alertCode = alert.AlertCode });
    }

    // Only restricts MinistryAdmin/MinistryViewer accounts that were created with an EntityScope
    // (see AdminController.AddMinistryAdmin). SuperAdmin and the operational roles (FactoryUser/
    // WarehouseUser/PharmacyUser) are left exactly as before - unscoped for this controller.
    private string? GetMinistryEntityScope()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "MinistryAdmin" && role != "MinistryViewer") return null;

        var scope = User.FindFirst("entityType")?.Value;
        if (string.IsNullOrEmpty(scope) || scope == "Ministry") return null;
        return scope;
    }

    private bool IsAllowedAlertEntityType(EntityKind? type)
    {
        var scope = GetMinistryEntityScope();
        if (scope == null) return true;
        return type != null && type.ToString() == scope;
    }

    private ObjectResult Forbid403() => new(new { message = "This account's Ministry scope doesn't include this entity type." }) { StatusCode = 403 };
}

