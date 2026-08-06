using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/factories")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
[RequireMinistryScope("Factory")]
public class FactoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public FactoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<FactoryListItemDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var query = _db.Factories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.OfficialFactoryName != null && f.OfficialFactoryName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(f => f.FactoryStatus != null && f.FactoryStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderBy(f => f.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 5 : pageSize)
            .Select(f => new FactoryListItemDto
            {
                Id = f.Id,
                FactoryName = f.OfficialFactoryName,
                LegalCompanyName = f.LegalCompanyName,
                Governorate = f.Governorate,
                City = f.City,
                LicenseExpiryDate = f.LicenseExpiryDate,
                HasColdStorage = f.HasColdStorage,
                HasQualityControlLab = f.HasQualityControlLab,
                FactoryStatus = f.FactoryStatus.ToString(),
                TotalBatches = f.TotalBatches,
                CreatedAt = f.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<FactoryListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FactoryProfileDto>> GetById(int id)
    {
        var f = await _db.Factories.FirstOrDefaultAsync(x => x.Id == id);
        if (f == null) return NotFound(new { message = "Factory not found." });

        return Ok(new FactoryProfileDto
        {
            Id = f.Id,
            OfficialFactoryName = f.OfficialFactoryName,
            LegalCompanyName = f.LegalCompanyName,
            DosageFormsProduced = f.DosageFormsProduced,
            Governorate = f.Governorate,
            City = f.City,
            DistrictArea = f.DistrictArea,
            FullAddress = f.FullAddress,
            FactoryLicenseNumber = f.FactoryLicenseNumber,
            TechnicalOperatingLicenseNumber = f.TechnicalOperatingLicenseNumber,
            CommercialRegistrationNumber = f.CommercialRegistrationNumber,
            TaxCardNumber = f.TaxCardNumber,
            LicenseIssueDate = f.LicenseIssueDate,
            LicenseExpiryDate = f.LicenseExpiryDate,
            HasQualityControlLab = f.HasQualityControlLab,
            HasFinishedGoodsStore = f.HasFinishedGoodsStore,
            HasColdStorage = f.HasColdStorage,
            HasQuarantineArea = f.HasQuarantineArea,
            FactoryStatus = f.FactoryStatus?.ToString(),
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        });
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id, [FromBody] EntityActionDto? dto)
    {
        var f = await _db.Factories.FindAsync(id);
        if (f == null) return NotFound(new { message = "Factory not found." });
        var old = f.FactoryStatus?.ToString();
        f.FactoryStatus = FacilityStatus.Suspended;
        f.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SuspendEntity, "Factory", f.FactoryLicenseNumber, old, "Suspended", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Factory suspended.", status = "Suspended" });
    }

    [HttpPost("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id, [FromBody] EntityActionDto? dto)
    {
        var f = await _db.Factories.FindAsync(id);
        if (f == null) return NotFound(new { message = "Factory not found." });
        var old = f.FactoryStatus?.ToString();
        f.FactoryStatus = FacilityStatus.Active;
        f.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.ReactivateEntity, "Factory", f.FactoryLicenseNumber, old, "Active", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Factory reactivated.", status = "Active" });
    }

    [HttpPost("{id:int}/set-inactive")]
    public async Task<IActionResult> SetInactive(int id, [FromBody] EntityActionDto? dto)
    {
        var f = await _db.Factories.FindAsync(id);
        if (f == null) return NotFound(new { message = "Factory not found." });
        var old = f.FactoryStatus?.ToString();
        f.FactoryStatus = FacilityStatus.Inactive;
        f.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SetInactiveEntity, "Factory", f.FactoryLicenseNumber, old, "Inactive", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Factory set to inactive.", status = "Inactive" });
    }

    [HttpGet("{id:int}/batches")]
    public async Task<ActionResult<PagedResult<BatchListItemDto>>> GetBatches(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).Where(b => b.FactoryId == id);
        var total = await query.CountAsync();
        var items = await query.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(b => new BatchListItemDto
            {
                Id = b.Id,
                ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                GTIN = b.MedicineProduct != null ? b.MedicineProduct.GTIN : null,
                DosageForm = b.MedicineProduct != null ? b.MedicineProduct.DosageForm : null,
                Strength = b.MedicineProduct != null ? b.MedicineProduct.Strength : null,
                BatchNumber = b.BatchNumber,
                FactoryName = b.Factory != null ? b.Factory.OfficialFactoryName : null,
                Quantity = b.Quantity,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus.ToString(),
                SupplyChainStage = b.SupplyChainStage.ToString(),
                CurrentLocation = b.CurrentLocation,
                OpenAlerts = b.OpenAlertsCount
            }).ToListAsync();
        return Ok(new PagedResult<BatchListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}/registration-request")]
    public async Task<ActionResult<EntityRegistrationRequestRefDto>> GetRegistrationRequest(int id)
    {
        var r = await _db.RegistrationRequests
            .Where(x => x.FactoryId == id)
            .OrderByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "No registration request found for this factory." });

        return Ok(new EntityRegistrationRequestRefDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            SubmittedAt = r.SubmittedAt
        });
    }

    // Same filters as GetAll (search/status), returns a CSV file (opens fine in Excel).
    // No paging - exports everything matching the filters.
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _db.Factories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.OfficialFactoryName != null && f.OfficialFactoryName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(f => f.FactoryStatus != null && f.FactoryStatus.ToString() == status);

        var rows = await query.OrderBy(f => f.Id).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,FactoryName,LegalCompanyName,Governorate,City,LicenseExpiryDate,HasColdStorage,HasQualityControlLab,Status,TotalBatches,CreatedAt");
        foreach (var f in rows)
        {
            sb.AppendLine(string.Join(",",
                f.Id,
                Csv(f.OfficialFactoryName), Csv(f.LegalCompanyName), Csv(f.Governorate), Csv(f.City),
                f.LicenseExpiryDate?.ToString("yyyy-MM-dd"), f.HasColdStorage, f.HasQualityControlLab,
                Csv(f.FactoryStatus?.ToString()), f.TotalBatches, f.CreatedAt?.ToString("yyyy-MM-dd")));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"factories-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Contains(',') || value.Contains('"')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static AuditLog Log(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal, string? reason = null) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
        UserDisplayName = "Dr. Saif",
        Role = SystemRole.SuperAdmin,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        // Reason is appended here since AuditLog has no dedicated Reason column
        // (EntitiesManagement-Backend-Gaps.md, item 3).
        NewValue = string.IsNullOrWhiteSpace(reason) ? newVal : $"{newVal} (Reason: {reason})",
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

