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
[Route("api/warehouses")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
[RequireMinistryScope("Warehouse")]
public class WarehousesController : ControllerBase
{
    private readonly AppDbContext _db;
    public WarehousesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<WarehouseListItemDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var query = _db.Warehouses.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.OfficialWarehouseName != null && w.OfficialWarehouseName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.WarehouseStatus != null && w.WarehouseStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderBy(w => w.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 5 : pageSize)
            .Select(w => new WarehouseListItemDto
            {
                Id = w.Id,
                WarehouseName = w.OfficialWarehouseName,
                WarehouseType = w.WarehouseType,
                Governorate = w.Governorate,
                City = w.City,
                LicenseExpiryDate = w.LicenseExpiryDate,
                HasColdStorage = w.HasColdStorage,
                HasQuarantineArea = w.HasQuarantineArea,
                HasDeliveryService = w.HasDeliveryService,
                WarehouseStatus = w.WarehouseStatus.ToString(),
                CreatedAt = w.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<WarehouseListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseProfileDto>> GetById(int id)
    {
        var w = await _db.Warehouses.FirstOrDefaultAsync(x => x.Id == id);
        if (w == null) return NotFound(new { message = "Warehouse not found." });

        return Ok(new WarehouseProfileDto
        {
            Id = w.Id,
            OfficialWarehouseName = w.OfficialWarehouseName,
            WarehouseType = w.WarehouseType,
            Governorate = w.Governorate,
            City = w.City,
            DistrictArea = w.DistrictArea,
            FullAddress = w.FullAddress,
            WarehouseLicenseNumber = w.WarehouseLicenseNumber,
            LicenseIssueDate = w.LicenseIssueDate,
            LicenseExpiryDate = w.LicenseExpiryDate,
            HasColdStorage = w.HasColdStorage,
            HasQuarantineArea = w.HasQuarantineArea,
            HasDeliveryService = w.HasDeliveryService,
            WarehouseStatus = w.WarehouseStatus?.ToString(),
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt
        });
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id, [FromBody] EntityActionDto? dto)
    {
        var w = await _db.Warehouses.FindAsync(id);
        if (w == null) return NotFound(new { message = "Warehouse not found." });
        var old = w.WarehouseStatus?.ToString();
        w.WarehouseStatus = FacilityStatus.Suspended;
        w.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SuspendEntity, "Warehouse", w.WarehouseLicenseNumber, old, "Suspended", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Warehouse suspended.", status = "Suspended" });
    }

    [HttpPost("{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id, [FromBody] EntityActionDto? dto)
    {
        var w = await _db.Warehouses.FindAsync(id);
        if (w == null) return NotFound(new { message = "Warehouse not found." });
        var old = w.WarehouseStatus?.ToString();
        w.WarehouseStatus = FacilityStatus.Active;
        w.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.ReactivateEntity, "Warehouse", w.WarehouseLicenseNumber, old, "Active", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Warehouse reactivated.", status = "Active" });
    }

    [HttpPost("{id:int}/set-inactive")]
    public async Task<IActionResult> SetInactive(int id, [FromBody] EntityActionDto? dto)
    {
        var w = await _db.Warehouses.FindAsync(id);
        if (w == null) return NotFound(new { message = "Warehouse not found." });
        var old = w.WarehouseStatus?.ToString();
        w.WarehouseStatus = FacilityStatus.Inactive;
        w.UpdatedAt = DateTime.UtcNow;
        _db.AuditLogs.Add(Log(AuditAction.SetInactiveEntity, "Warehouse", w.WarehouseLicenseNumber, old, "Inactive", dto?.Reason));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Warehouse set to inactive.", status = "Inactive" });
    }

    [HttpGet("{id:int}/inventory")]
    public async Task<ActionResult<PagedResult<InventoryDistributionItemDto>>> GetInventory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var wh = await _db.Warehouses.FindAsync(id);
        if (wh == null) return NotFound(new { message = "Warehouse not found." });

        var query = _db.InventoryStocks.Where(i => i.HolderType == "Warehouse" && i.HolderName == wh.OfficialWarehouseName);
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
        var wh = await _db.Warehouses.FindAsync(id);
        if (wh == null) return NotFound(new { message = "Warehouse not found." });

        var query = _db.Shipments.Where(s => s.Source == wh.OfficialWarehouseName || s.Destination == wh.OfficialWarehouseName);
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
            .Where(x => x.WarehouseId == id)
            .OrderByDescending(x => x.SubmittedAt)
            .FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "No registration request found for this warehouse." });

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
        var query = _db.Warehouses.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.OfficialWarehouseName != null && w.OfficialWarehouseName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.WarehouseStatus != null && w.WarehouseStatus.ToString() == status);

        var rows = await query.OrderBy(w => w.Id).ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,WarehouseName,WarehouseType,Governorate,City,LicenseExpiryDate,HasColdStorage,HasQuarantineArea,HasDeliveryService,Status,CreatedAt");
        foreach (var w in rows)
        {
            sb.AppendLine(string.Join(",",
                w.Id,
                Csv(w.OfficialWarehouseName), Csv(w.WarehouseType), Csv(w.Governorate), Csv(w.City),
                w.LicenseExpiryDate?.ToString("yyyy-MM-dd"), w.HasColdStorage, w.HasQuarantineArea, w.HasDeliveryService,
                Csv(w.WarehouseStatus?.ToString()), w.CreatedAt?.ToString("yyyy-MM-dd")));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"warehouses-{DateTime.UtcNow:yyyyMMdd}.csv");
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

