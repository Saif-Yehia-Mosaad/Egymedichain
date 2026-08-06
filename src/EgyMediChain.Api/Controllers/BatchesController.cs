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
[Route("api/batches")]
// Fixed: this controller has no {factoryId} route param and queries ALL factories' batches
// nationally - it's a Ministry-wide view, not a per-factory one (FactoryUser already has their
// own batches via FactoryDashboardController, which IS properly scoped to their own factory).
// Letting FactoryUser call this too would have let them see every other factory's batches.
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
[RequireMinistryScope("Factory")]
public class BatchesController : ControllerBase
{
    private readonly AppDbContext _db;
    public BatchesController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult<BatchesSummaryDto>> GetSummary()
    {
        return Ok(new BatchesSummaryDto
        {
            TotalBatches = await _db.Batches.CountAsync(),
            InProduction = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.InProduction),
            InSupplyChain = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.InSupplyChain),
            InWarehouses = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.InWarehouse),
            InPharmacies = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.InPharmacy),
            Quarantined = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.Quarantined),
            Recalled = await _db.Batches.CountAsync(b => b.BatchStatus == BatchStatus.Recalled),
            OpenAlerts = await _db.Alerts.CountAsync(a => a.AlertStatus == AlertStatus.Open)
        });
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<BatchListItemDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? factory, [FromQuery] string? batchStatus,
        [FromQuery] string? stage, [FromQuery] string? dosageForm,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b =>
                (b.BatchNumber != null && b.BatchNumber.Contains(search)) ||
                (b.MedicineProduct != null && b.MedicineProduct.ProductName != null && b.MedicineProduct.ProductName.Contains(search)) ||
                (b.MedicineProduct != null && b.MedicineProduct.GTIN != null && b.MedicineProduct.GTIN.Contains(search)));

        if (!string.IsNullOrWhiteSpace(factory))
            query = query.Where(b => b.Factory != null && b.Factory.OfficialFactoryName == factory);

        if (!string.IsNullOrWhiteSpace(batchStatus))
            query = query.Where(b => b.BatchStatus != null && b.BatchStatus.ToString() == batchStatus);

        if (!string.IsNullOrWhiteSpace(stage))
            query = query.Where(b => b.SupplyChainStage != null && b.SupplyChainStage.ToString() == stage);

        // Real server-side filter (MedicineBatchDashboard-Backend-Gaps.md, item 2) - previously
        // only applied client-side on the current page, which silently missed matches on other pages.
        if (!string.IsNullOrWhiteSpace(dosageForm))
            query = query.Where(b => b.MedicineProduct != null && b.MedicineProduct.DosageForm == dosageForm);

        var total = await query.CountAsync();
        var items = await query.OrderBy(b => b.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
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
                OpenAlerts = b.OpenAlertsCount,
                RequiresColdChain = b.MedicineProduct != null ? b.MedicineProduct.RequiresColdChain : null,
                UpdatedAt = b.UpdatedAt
            }).ToListAsync();

        return Ok(new PagedResult<BatchListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BatchDetailsDto>> GetById(int id)
    {
        var b = await _db.Batches
            .Include(x => x.MedicineProduct)
            .Include(x => x.Factory)
            .Include(x => x.Shipments)
            .Include(x => x.InventoryStocks)
            .Include(x => x.Alerts)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b == null) return NotFound(new { message = "Batch not found." });

        var dto = new BatchDetailsDto
        {
            Id = b.Id,
            ProductInfo = new ProductInfoDto
            {
                ProductName = b.MedicineProduct?.ProductName,
                GTIN = b.MedicineProduct?.GTIN,
                DosageForm = b.MedicineProduct?.DosageForm,
                Strength = b.MedicineProduct?.Strength,
                RequiresColdChain = b.MedicineProduct?.RequiresColdChain,
                ProductStatus = b.MedicineProduct?.ProductStatus
            },
            BatchInfo = new BatchInfoDto
            {
                BatchNumber = b.BatchNumber,
                FactoryName = b.Factory?.OfficialFactoryName,
                Quantity = b.Quantity,
                ManufacturingDate = b.ManufacturingDate,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus?.ToString(),
                SupplyChainStage = b.SupplyChainStage?.ToString(),
                CreatedBy = b.CreatedBy,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            },
            UnitCodesSummary = new UnitCodesSummaryDto
            {
                TotalUnitCodes = b.TotalUnitCodes,
                GeneratedCount = b.GeneratedUnitCodes,
                InWarehouseCount = b.InWarehouseUnitCodes,
                InPharmacyCount = b.InPharmacyUnitCodes,
                SuspiciousCount = b.SuspiciousUnitCodes,
                BlockedCount = b.BlockedUnitCodes,
                RecalledCount = b.RecalledUnitCodes,
                ScanCountTotal = b.ScanCountTotal
            },
            Shipments = b.Shipments?.Select(s => new ShipmentSummaryItemDto
            {
                TransferCode = s.TransferCode,
                ShipmentType = s.ShipmentType?.ToString(),
                Source = s.Source,
                Destination = s.Destination,
                ExpectedQuantity = s.ExpectedQuantity,
                ReceivedQuantity = s.ReceivedQuantity,
                ShipmentStatus = s.ShipmentStatus?.ToString(),
                DispatchDate = s.DispatchDate,
                ReceivedDate = s.ReceivedDate
            }).ToList(),
            InventoryDistribution = b.InventoryStocks?.Select(i => new InventoryDistributionItemDto
            {
                HolderType = i.HolderType,
                HolderName = i.HolderName,
                TotalReceivedQuantity = i.TotalReceivedQuantity,
                AvailableQuantity = i.AvailableQuantity,
                ReservedQuantity = i.ReservedQuantity,
                QuarantinedQuantity = i.QuarantinedQuantity,
                InventoryStatus = i.InventoryStatus?.ToString(),
                LastUpdated = i.LastUpdated
            }).ToList(),
            RelatedAlerts = b.Alerts?.Select(a => new RelatedAlertItemDto
            {
                AlertType = a.AlertType?.ToString(),
                Severity = a.Severity?.ToString(),
                Message = a.Message,
                AlertStatus = a.AlertStatus?.ToString(),
                CreatedAt = a.CreatedAt,
                ResolvedAt = a.ResolvedAt
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost("{id:int}/freeze")]
    public async Task<IActionResult> Freeze(int id)
    {
        var b = await _db.Batches.Include(x => x.UnitCodes).Include(x => x.InventoryStocks).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Batch not found." });

        var old = b.BatchStatus?.ToString();
        b.BatchStatus = BatchStatus.Quarantined;
        b.UpdatedAt = DateTime.UtcNow;
        if (b.UnitCodes != null) foreach (var u in b.UnitCodes) u.UnitStatus = UnitStatus.Blocked;
        if (b.InventoryStocks != null) foreach (var i in b.InventoryStocks) i.InventoryStatus = InventoryStatus.Blocked;

        _db.Alerts.Add(new Alert
        {
            AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AlertType = AlertType.ComplianceIssue,
            Severity = AlertSeverity.High,
            EntityType = EntityKind.Factory,
            EntityName = b.Factory?.OfficialFactoryName,
            BatchId = b.Id,
            Message = $"Batch {b.BatchNumber} has been frozen by the Ministry pending investigation.",
            AlertStatus = AlertStatus.Open,
            CreatedAt = DateTime.UtcNow
        });

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.FreezeBatch,
            ResourceType = "Batch",
            ResourceId = b.BatchNumber,
            OldValue = old,
            NewValue = "Quarantined",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Batch frozen (quarantined).", status = "Quarantined" });
    }

    // Reverses a freeze done in error. Restores the status the batch had right before it was
    // frozen (read from the last FreezeBatch audit log entry for this batch), falling back to
    // InSupplyChain if no such log is found. Only works while the batch is still Quarantined -
    // won't touch a batch that's since been Recalled through a separate action.
    [HttpPost("{id:int}/unfreeze")]
    public async Task<IActionResult> Unfreeze(int id)
    {
        var b = await _db.Batches.Include(x => x.UnitCodes).Include(x => x.InventoryStocks).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Batch not found." });
        if (b.BatchStatus != BatchStatus.Quarantined)
            return Conflict(new { message = "Only a Quarantined batch can be unfrozen." });

        var lastFreezeLog = await _db.AuditLogs
            .Where(l => l.Action == AuditAction.FreezeBatch && l.ResourceId == b.BatchNumber)
            .OrderByDescending(l => l.CreatedAt).FirstOrDefaultAsync();

        var restoredStatus = Enum.TryParse<BatchStatus>(lastFreezeLog?.OldValue, out var parsed) ? parsed : BatchStatus.InSupplyChain;

        b.BatchStatus = restoredStatus;
        b.UpdatedAt = DateTime.UtcNow;
        if (b.UnitCodes != null) foreach (var u in b.UnitCodes.Where(u => u.UnitStatus == UnitStatus.Blocked)) u.UnitStatus = UnitStatus.InWarehouse;
        if (b.InventoryStocks != null) foreach (var i in b.InventoryStocks.Where(i => i.InventoryStatus == InventoryStatus.Blocked)) i.InventoryStatus = InventoryStatus.Active;

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.FreezeBatch,
            ResourceType = "Batch",
            ResourceId = b.BatchNumber,
            OldValue = "Quarantined",
            NewValue = restoredStatus.ToString(),
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Batch unfrozen.", status = restoredStatus.ToString() });
    }

    [HttpPost("{id:int}/create-recall-alert")]
    public async Task<IActionResult> CreateRecallAlert(int id, [FromBody] CreateRecallAlertDto? dto)
    {
        var b = await _db.Batches.Include(x => x.UnitCodes).Include(x => x.InventoryStocks).Include(x => x.Factory).FirstOrDefaultAsync(x => x.Id == id);
        if (b == null) return NotFound(new { message = "Batch not found." });

        var old = b.BatchStatus?.ToString();
        b.BatchStatus = BatchStatus.Recalled;
        b.UpdatedAt = DateTime.UtcNow;
        if (b.UnitCodes != null) foreach (var u in b.UnitCodes) u.UnitStatus = UnitStatus.Recalled;
        if (b.InventoryStocks != null) foreach (var i in b.InventoryStocks) i.InventoryStatus = InventoryStatus.Recalled;

        _db.Alerts.Add(new Alert
        {
            AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AlertType = AlertType.Recall,
            Severity = AlertSeverity.Critical,
            EntityType = EntityKind.Factory,
            EntityName = b.Factory?.OfficialFactoryName,
            BatchId = b.Id,
            Message = dto?.Message ?? $"Batch {b.BatchNumber} has been recalled by the Ministry.",
            AlertStatus = AlertStatus.Open,
            CreatedAt = DateTime.UtcNow
        });

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.CreateRecallAlert,
            ResourceType = "Batch",
            ResourceId = b.BatchNumber,
            OldValue = old,
            NewValue = "Recalled",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Recall alert created.", status = "Recalled" });
    }
}

