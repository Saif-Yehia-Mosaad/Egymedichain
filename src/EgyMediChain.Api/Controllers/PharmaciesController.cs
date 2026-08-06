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
[Route("api/pharmacies")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
[RequireMinistryScope("Pharmacy")]
public class PharmaciesController : ControllerBase
{
    private readonly AppDbContext _db;
    public PharmaciesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<PharmacyListItemDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var query = _db.Pharmacies.Include(p => p.DefaultWarehouse).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.OfficialPharmacyName != null && p.OfficialPharmacyName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.PharmacyStatus != null && p.PharmacyStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderBy(p => p.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 5 : pageSize)
            .Select(p => new PharmacyListItemDto
            {
                Id = p.Id,
                PharmacyName = p.OfficialPharmacyName,
                PharmacyType = p.PharmacyType,
                Governorate = p.Governorate,
                City = p.City,
                DefaultWarehouse = p.DefaultWarehouse != null ? p.DefaultWarehouse.OfficialWarehouseName : null,
                HasColdStorage = p.HasColdStorage,
                LicenseExpiryDate = p.LicenseExpiryDate,
                PharmacyStatus = p.PharmacyStatus.ToString(),
                CreatedAt = p.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<PharmacyListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PharmacyProfileDto>> GetById(int id)
    {
        var p = await _db.Pharmacies.Include(x => x.DefaultWarehouse).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound(new { message = "Pharmacy not found." });

        return Ok(new PharmacyProfileDto
        {
            Id = p.Id,
            OfficialPharmacyName = p.OfficialPharmacyName,
            PharmacyType = p.PharmacyType,
            Governorate = p.Governorate,
            City = p.City,
            DistrictArea = p.DistrictArea,
            FullAddress = p.FullAddress,
            DefaultWarehouse = p.DefaultWarehouse?.OfficialWarehouseName,
            HasColdStorage = p.HasColdStorage,
            PharmacyLicenseNumber = p.PharmacyLicenseNumber,
            LicenseIssueDate = p.LicenseIssueDate,
            LicenseExpiryDate = p.LicenseExpiryDate,
            PharmacistSyndicateId = p.PharmacistSyndicateId,
            PharmacyStatus = p.PharmacyStatus?.ToString(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id, [FromBody] EntityActionDto? dto)
    {
        var p = await _db.Pharmacies.FindAsync(id);
        if (p == null) return NotFound(new { message = "Pharmacy not found." });
        var old = p.PharmacyStatus?.ToString();
        p.PharmacyStatus = FacilityStatus.Suspended;
        p.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SuspendEntity, "Pharmacy", p.PharmacyLicenseNumber, old, "Suspended", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Pharmacy suspended.", status = "Suspended" });
    }

    [HttpPost("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id, [FromBody] EntityActionDto? dto)
    {
        var p = await _db.Pharmacies.FindAsync(id);
        if (p == null) return NotFound(new { message = "Pharmacy not found." });
        var old = p.PharmacyStatus?.ToString();
        p.PharmacyStatus = FacilityStatus.Active;
        p.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.ReactivateEntity, "Pharmacy", p.PharmacyLicenseNumber, old, "Active", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Pharmacy reactivated.", status = "Active" });
    }

    [HttpPost("{id:int}/set-inactive")]
    public async Task<IActionResult> SetInactive(int id, [FromBody] EntityActionDto? dto)
    {
        var p = await _db.Pharmacies.FindAsync(id);
        if (p == null) return NotFound(new { message = "Pharmacy not found." });
        var old = p.PharmacyStatus?.ToString();
        p.PharmacyStatus = FacilityStatus.Inactive;
        p.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SetInactiveEntity, "Pharmacy", p.PharmacyLicenseNumber, old, "Inactive", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Pharmacy set to inactive.", status = "Inactive" });
    }

    [HttpGet("{id:int}/inventory")]
    public async Task<ActionResult<PagedResult<InventoryDistributionItemDto>>> GetInventory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var ph = await _db.Pharmacies.FindAsync(id);
        if (ph == null) return NotFound(new { message = "Pharmacy not found." });

        var query = _db.InventoryStocks.Where(i => i.HolderType == "Pharmacy" && i.HolderName == ph.OfficialPharmacyName);
        var total = await query.CountAsync();
        var items = await query.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(i => new InventoryDistributionItemDto
            {
                HolderType = i.HolderType,
                HolderName = i.HolderName,
                TotalReceivedQuantity = i.TotalReceivedQuantity,
                AvailableQuantity = i.AvailableQuantity,
                ReservedQuantity = i.ReservedQuantity,
                QuarantinedQuantity = i.QuarantinedQuantity,
                InventoryStatus = i.InventoryStatus.ToString(),
                LastUpdated = i.LastUpdated
            }).ToListAsync();
        return Ok(new PagedResult<InventoryDistributionItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}/shipments")]
    public async Task<ActionResult<PagedResult<ShipmentSummaryItemDto>>> GetShipments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var ph = await _db.Pharmacies.FindAsync(id);
        if (ph == null) return NotFound(new { message = "Pharmacy not found." });

        var query = _db.Shipments.Where(s => s.Destination == ph.OfficialPharmacyName);
        var total = await query.CountAsync();
        var items = await query.Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => new ShipmentSummaryItemDto
            {
                TransferCode = s.TransferCode,
                ShipmentType = s.ShipmentType.ToString(),
                Source = s.Source,
                Destination = s.Destination,
                ExpectedQuantity = s.ExpectedQuantity,
                ReceivedQuantity = s.ReceivedQuantity,
                ShipmentStatus = s.ShipmentStatus.ToString(),
                DispatchDate = s.DispatchDate,
                ReceivedDate = s.ReceivedDate
            }).ToListAsync();
        return Ok(new PagedResult<ShipmentSummaryItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}/registration-request")]
    public async Task<ActionResult<EntityRegistrationRequestRefDto>> GetRegistrationRequest(int id)
    {
        var r = await _db.RegistrationRequests
            .Where(x => x.PharmacyId == id)
            .OrderByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "No registration request found for this pharmacy." });

        return Ok(new EntityRegistrationRequestRefDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            SubmittedAt = r.SubmittedAt
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _db.Pharmacies.Include(p => p.DefaultWarehouse).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.OfficialPharmacyName != null && p.OfficialPharmacyName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.PharmacyStatus != null && p.PharmacyStatus.ToString() == status);

        var rows = await query.OrderBy(p => p.Id).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,PharmacyName,PharmacyType,Governorate,City,DefaultWarehouse,HasColdStorage,LicenseExpiryDate,Status,CreatedAt");
        foreach (var p in rows)
        {
            sb.AppendLine(string.Join(",",
                p.Id,
                Csv(p.OfficialPharmacyName), Csv(p.PharmacyType), Csv(p.Governorate), Csv(p.City),
                Csv(p.DefaultWarehouse?.OfficialWarehouseName), p.HasColdStorage,
                p.LicenseExpiryDate?.ToString("yyyy-MM-dd"), Csv(p.PharmacyStatus?.ToString()), p.CreatedAt?.ToString("yyyy-MM-dd")));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"pharmacies-{DateTime.UtcNow:yyyyMMdd}.csv");
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
        NewValue = string.IsNullOrWhiteSpace(reason) ? newVal : $"{newVal} (Reason: {reason})",
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

