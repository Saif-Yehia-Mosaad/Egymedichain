using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Operational portal for a single Factory (role: FactoryUser). Everything is scoped to
// {factoryId} in the route rather than pulled from the JWT, matching the rest of this API's
// "keep it simple, low-friction" style - the frontend gets {factoryId} back from /api/auth/login.
[ApiController]
[Route("api/factory-dashboard/{factoryId:int}")]
[Authorize(Roles = "FactoryUser,SuperAdmin,MinistryAdmin")]
[ValidateEntityOwnership("factoryId")]
public class FactoryDashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public FactoryDashboardController(AppDbContext db) => _db = db;

    private async Task<Factory?> GetFactoryAsync(int factoryId) => await _db.Factories.FindAsync(factoryId);

    private static bool IsActive(Factory f) => f.FactoryStatus == FacilityStatus.Active;

    // ---------------- Overview ----------------
    [HttpGet("overview")]
    public async Task<ActionResult<FactoryOverviewDto>> GetOverview(int factoryId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var batchesQuery = _db.Batches.Where(b => b.FactoryId == factoryId);
        var shipmentsQuery = _db.Shipments.Where(s => s.SourceFactoryId == factoryId);

        var cards = new FactoryOverviewCardsDto
        {
            TotalBatches = await batchesQuery.CountAsync(),
            ReadyForDispatch = await batchesQuery.CountAsync(b => b.BatchStatus == BatchStatus.ReadyForWarehouseDispatch || b.BatchStatus == BatchStatus.PartiallyDispatched),
            UnitCodesGenerated = await batchesQuery.SumAsync(b => b.GeneratedUnitCodes ?? 0),
            ShipmentsInTransit = await shipmentsQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit || s.ShipmentStatus == ShipmentStatus.PendingInspection),
            ReceivedByWarehouses = await shipmentsQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Received || s.ShipmentStatus == ShipmentStatus.PartiallyReceived),
            OpenAlerts = await _db.Alerts.CountAsync(a => a.EntityType == EntityKind.Factory && a.EntityName == factory.OfficialFactoryName && a.AlertStatus == AlertStatus.Open)
        };

        var recentBatches = await batchesQuery.Include(b => b.MedicineProduct).Include(b => b.Factory)
            .OrderByDescending(b => b.UpdatedAt).Take(5)
            .Select(b => new BatchListItemDto
            {
                Id = b.Id,
                ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                GTIN = b.MedicineProduct != null ? b.MedicineProduct.GTIN : null,
                BatchNumber = b.BatchNumber,
                Quantity = b.Quantity,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus.ToString(),
                SupplyChainStage = b.SupplyChainStage.ToString(),
                CurrentLocation = b.CurrentLocation,
                OpenAlerts = b.OpenAlertsCount,
                UnitCodesCount = b.GeneratedUnitCodes,
                AvailableForDispatch = b.BatchStatus == BatchStatus.ReadyForWarehouseDispatch || b.BatchStatus == BatchStatus.PartiallyDispatched
            }).ToListAsync();

        var recentShipments = await shipmentsQuery.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .OrderByDescending(s => s.DispatchDate).Take(5)
            .Select(s => ToShipmentListItem(s)).ToListAsync();

        var openAlerts = await _db.Alerts.Where(a => a.EntityType == EntityKind.Factory && a.EntityName == factory.OfficialFactoryName && a.AlertStatus == AlertStatus.Open)
            .Include(a => a.Batch).OrderByDescending(a => a.CreatedAt).Take(5)
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

        return Ok(new FactoryOverviewDto { Cards = cards, RecentBatches = recentBatches, RecentShipments = recentShipments, OpenAlerts = openAlerts });
    }

    // ---------------- Batch Management ----------------
    [HttpGet("batches")]
    public async Task<ActionResult<PagedResult<BatchListItemDto>>> GetBatches(int factoryId,
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).Where(b => b.FactoryId == factoryId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => (b.BatchNumber != null && b.BatchNumber.Contains(search)) ||
                                      (b.MedicineProduct != null && b.MedicineProduct.ProductName != null && b.MedicineProduct.ProductName.Contains(search)));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(b => b.BatchStatus != null && b.BatchStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(b => b.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(b => new BatchListItemDto
            {
                Id = b.Id,
                ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                GTIN = b.MedicineProduct != null ? b.MedicineProduct.GTIN : null,
                DosageForm = b.MedicineProduct != null ? b.MedicineProduct.DosageForm : null,
                Strength = b.MedicineProduct != null ? b.MedicineProduct.Strength : null,
                BatchNumber = b.BatchNumber,
                Quantity = b.Quantity,
                ExpiryDate = b.ExpiryDate,
                BatchStatus = b.BatchStatus.ToString(),
                SupplyChainStage = b.SupplyChainStage.ToString(),
                CurrentLocation = b.CurrentLocation,
                OpenAlerts = b.OpenAlertsCount,
                UnitCodesCount = b.GeneratedUnitCodes,
                AvailableForDispatch = b.BatchStatus == BatchStatus.ReadyForWarehouseDispatch || b.BatchStatus == BatchStatus.PartiallyDispatched,
                RequiresColdChain = b.MedicineProduct != null ? b.MedicineProduct.RequiresColdChain : null
            }).ToListAsync();

        return Ok(new PagedResult<BatchListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("batches/{batchId:int}")]
    public async Task<ActionResult<BatchDetailsDto>> GetBatchDetails(int factoryId, int batchId)
    {
        var b = await _db.Batches.Include(x => x.MedicineProduct).Include(x => x.Factory)
            .Include(x => x.Shipments).Include(x => x.Alerts)
            .FirstOrDefaultAsync(x => x.Id == batchId && x.FactoryId == factoryId);
        if (b == null) return NotFound(new { message = "Batch not found for this factory." });

        return Ok(new BatchDetailsDto
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
            RelatedAlerts = b.Alerts?.Select(a => new RelatedAlertItemDto
            {
                AlertType = a.AlertType?.ToString(),
                Severity = a.Severity?.ToString(),
                Message = a.Message,
                AlertStatus = a.AlertStatus?.ToString(),
                CreatedAt = a.CreatedAt,
                ResolvedAt = a.ResolvedAt
            }).ToList()
        });
    }

    // Real system-of-record unit codes for a batch (Backend_Action_Report_Factory_Portal, item 1.2).
    // Replaces the frontend's client-side fabricated CSV export - this is the actual generated data.
    [HttpGet("batches/{batchId:int}/unit-codes")]
    public async Task<ActionResult<PagedResult<UnitCodeListItemDto>>> GetUnitCodes(int factoryId, int batchId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });

        if (batch.BatchStatus != BatchStatus.CodesGenerated &&
            batch.BatchStatus != BatchStatus.ReadyForWarehouseDispatch &&
            batch.BatchStatus != BatchStatus.PartiallyDispatched &&
            batch.BatchStatus != BatchStatus.FullyDispatched)
            return Conflict(new { message = "Unit codes are only available once they've been generated for this batch." });

        var query = _db.UnitCodes.Where(u => u.BatchId == batchId);
        var total = await query.CountAsync();
        var items = await query.OrderBy(u => u.Id)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 100 : pageSize)
            .Select(u => new UnitCodeListItemDto
            {
                Id = u.Id,
                GTIN = u.GTIN,
                SerialNumber = u.SerialNumber,
                CodeValueHash = u.CodeValueHash,
                ExpiryDate = u.ExpiryDate,
                UnitStatus = u.UnitStatus.ToString()
            }).ToListAsync();

        return Ok(new PagedResult<UnitCodeListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Factory-scoped warehouse directory for dispatch destination selection
    // (Backend_Action_Report_Factory_Portal, item 1.4). Only Active warehouses, so the frontend
    // can show real names + cold-storage capability instead of a blind numeric ID field.
    [HttpGet("warehouses")]
    public async Task<ActionResult<PagedResult<WarehouseOptionDto>>> GetWarehousesForDispatch(int factoryId,
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _db.Warehouses.Where(w => w.WarehouseStatus == FacilityStatus.Active);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.OfficialWarehouseName != null && w.OfficialWarehouseName.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderBy(w => w.OfficialWarehouseName)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 50 : pageSize)
            .Select(w => new WarehouseOptionDto
            {
                Id = w.Id,
                WarehouseName = w.OfficialWarehouseName,
                Governorate = w.Governorate,
                City = w.City,
                HasColdStorage = w.HasColdStorage,
                WarehouseStatus = w.WarehouseStatus.ToString()
            }).ToListAsync();

        return Ok(new PagedResult<WarehouseOptionDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Alert status counts scoped to this factory, independent of list pagination
    // (Backend_Action_Report_Factory_Portal, item 1.5).
    [HttpGet("alerts/counts")]
    public async Task<ActionResult<FactoryAlertsCountsDto>> GetAlertsCounts(int factoryId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var query = _db.Alerts.Where(a => a.EntityType == EntityKind.Factory && a.EntityName == factory.OfficialFactoryName);
        return Ok(new FactoryAlertsCountsDto
        {
            Open = await query.CountAsync(a => a.AlertStatus == AlertStatus.Open),
            UnderReview = await query.CountAsync(a => a.AlertStatus == AlertStatus.UnderReview),
            Resolved = await query.CountAsync(a => a.AlertStatus == AlertStatus.Resolved),
            Dismissed = await query.CountAsync(a => a.AlertStatus == AlertStatus.Dismissed),
            Total = await query.CountAsync()
        });
    }

    // Edit a Draft batch in place (Backend_Action_Report_Factory_Portal, item 1.1).
    [HttpPut("batches/{batchId:int}")]
    [HttpPatch("batches/{batchId:int}")]
    public async Task<ActionResult<BatchListItemDto>> UpdateBatch(int factoryId, int batchId, [FromBody] UpdateBatchDto? dto)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.Include(b => b.MedicineProduct).FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });
        if (batch.BatchStatus != BatchStatus.Draft)
            return Conflict(new { message = "Only a Draft batch can be edited." });

        if (dto?.ExpiryDate != null && dto.ManufacturingDate != null && dto.ExpiryDate <= dto.ManufacturingDate)
            return BadRequest(new { message = "Expiry date must be after manufacturing date." });
        if (dto?.Quantity != null && dto.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than 0." });

        if (batch.MedicineProduct != null)
        {
            if (dto?.ProductName != null) batch.MedicineProduct.ProductName = dto.ProductName;
            if (dto?.GTIN != null) batch.MedicineProduct.GTIN = dto.GTIN;
            if (dto?.DosageForm != null) batch.MedicineProduct.DosageForm = dto.DosageForm;
            if (dto?.Strength != null) batch.MedicineProduct.Strength = dto.Strength;
            if (dto?.RequiresColdChain != null) batch.MedicineProduct.RequiresColdChain = dto.RequiresColdChain;
        }

        if (dto?.BatchNumber != null) batch.BatchNumber = dto.BatchNumber;
        if (dto?.Quantity != null) batch.Quantity = dto.Quantity;
        if (dto?.ManufacturingDate != null) batch.ManufacturingDate = dto.ManufacturingDate;
        if (dto?.ExpiryDate != null) batch.ExpiryDate = dto.ExpiryDate;
        if (dto?.Notes != null) batch.Notes = dto.Notes;
        batch.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new BatchListItemDto
        {
            Id = batch.Id,
            ProductName = batch.MedicineProduct?.ProductName,
            GTIN = batch.MedicineProduct?.GTIN,
            DosageForm = batch.MedicineProduct?.DosageForm,
            Strength = batch.MedicineProduct?.Strength,
            BatchNumber = batch.BatchNumber,
            Quantity = batch.Quantity,
            ManufacturingDate = batch.ManufacturingDate,
            ExpiryDate = batch.ExpiryDate,
            BatchStatus = batch.BatchStatus.ToString(),
            SupplyChainStage = batch.SupplyChainStage.ToString(),
            RequiresColdChain = batch.MedicineProduct?.RequiresColdChain
        });
    }

    [HttpPost("batches")]
    public async Task<ActionResult<BatchListItemDto>> CreateBatch(int factoryId, [FromBody] CreateBatchDto? dto)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        // Reuse an existing MedicineProduct by GTIN, otherwise create a new one.
        MedicineProduct? product = null;
        if (!string.IsNullOrWhiteSpace(dto?.GTIN))
            product = await _db.MedicineProducts.FirstOrDefaultAsync(p => p.GTIN == dto!.GTIN);

        if (product == null)
        {
            product = new MedicineProduct
            {
                ProductName = dto?.ProductName ?? "Unnamed Product",
                GTIN = dto?.GTIN ?? $"AUTO-{DateTime.UtcNow.Ticks}",
                DosageForm = dto?.DosageForm,
                Strength = dto?.Strength,
                RequiresColdChain = dto?.RequiresColdChain ?? false,
                ProductStatus = "Active"
            };
            _db.MedicineProducts.Add(product);
        }

        var isDraft = dto?.SaveAsDraft == true;
        var batch = new Batch
        {
            MedicineProduct = product,
            FactoryId = factoryId,
            Factory = factory,
            BatchNumber = string.IsNullOrWhiteSpace(dto?.BatchNumber) ? $"BAT-{DateTime.UtcNow:yyyyMMddHHmmss}" : dto!.BatchNumber,
            Quantity = dto?.Quantity,
            ManufacturingDate = dto?.ManufacturingDate,
            ExpiryDate = dto?.ExpiryDate,
            Notes = dto?.Notes,
            BatchStatus = isDraft ? BatchStatus.Draft : BatchStatus.Registered,
            SupplyChainStage = SupplyChainStage.AtFactory,
            CurrentLocation = factory.OfficialFactoryName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TotalUnitCodes = 0,
            GeneratedUnitCodes = 0
        };
        _db.Batches.Add(batch);

        _db.AuditLogs.Add(NewLog(AuditAction.CreateBatch, "Batch", batch.BatchNumber, null, batch.BatchStatus.ToString()));
        await _db.SaveChangesAsync();

        return Ok(new BatchListItemDto
        {
            Id = batch.Id,
            ProductName = product.ProductName,
            GTIN = product.GTIN,
            BatchNumber = batch.BatchNumber,
            Quantity = batch.Quantity,
            ExpiryDate = batch.ExpiryDate,
            BatchStatus = batch.BatchStatus.ToString(),
            SupplyChainStage = batch.SupplyChainStage.ToString()
        });
    }

    [HttpPost("batches/{batchId:int}/generate-codes")]
    public async Task<IActionResult> GenerateCodes(int factoryId, int batchId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });
        if (batch.BatchStatus != BatchStatus.Registered)
            return Conflict(new { message = "Codes can only be generated for a batch with status 'Registered'." });

        var qty = batch.Quantity ?? 0;
        batch.TotalUnitCodes = qty;
        batch.GeneratedUnitCodes = qty;
        batch.BatchStatus = BatchStatus.CodesGenerated;
        batch.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.GenerateCodes, "Batch", batch.BatchNumber, "Registered", "CodesGenerated"));
        await _db.SaveChangesAsync();
        return Ok(new { message = $"{qty} unit codes generated.", status = "CodesGenerated", unitCodesGenerated = qty });
    }

    [HttpPost("batches/{batchId:int}/mark-ready")]
    public async Task<IActionResult> MarkReady(int factoryId, int batchId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });
        if (batch.BatchStatus != BatchStatus.CodesGenerated)
            return Conflict(new { message = "Only a batch with status 'CodesGenerated' can be marked ready for dispatch." });

        batch.BatchStatus = BatchStatus.ReadyForWarehouseDispatch;
        batch.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.MarkBatchReadyForDispatch, "Batch", batch.BatchNumber, "CodesGenerated", "ReadyForWarehouseDispatch"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Batch marked ready for warehouse dispatch.", status = "ReadyForWarehouseDispatch" });
    }

    [HttpPost("batches/{batchId:int}/cancel-draft")]
    public async Task<IActionResult> CancelDraft(int factoryId, int batchId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });
        if (batch.BatchStatus != BatchStatus.Draft)
            return Conflict(new { message = "Only a Draft batch can be cancelled." });

        batch.BatchStatus = BatchStatus.Cancelled;
        batch.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.CancelDraftBatch, "Batch", batch.BatchNumber, "Draft", "Cancelled"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Draft batch cancelled.", status = "Cancelled" });
    }

    // Only Draft or already-Cancelled batches can be hard-deleted - those are the only states
    // guaranteed to have no generated unit codes, shipments, or inventory pointing at them.
    [HttpDelete("batches/{batchId:int}")]
    public async Task<IActionResult> DeleteBatch(int factoryId, int batchId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var batch = await _db.Batches.FirstOrDefaultAsync(b => b.Id == batchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });

        if (batch.BatchStatus != BatchStatus.Draft && batch.BatchStatus != BatchStatus.Cancelled)
            return BadRequest(new { message = "Only a Draft or Cancelled batch can be deleted." });

        _db.AuditLogs.Add(NewLog(AuditAction.DeleteDraftBatch, "Batch", batch.BatchNumber, batch.BatchStatus?.ToString(), "Deleted"));
        _db.Batches.Remove(batch);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Batch deleted." });
    }

    // ---------------- Shipments (Factory -> Warehouse) ----------------
    [HttpGet("shipments/summary")]
    public async Task<ActionResult<FactoryShipmentsSummaryDto>> GetShipmentsSummary(int factoryId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var query = _db.Shipments.Where(s => s.SourceFactoryId == factoryId);
        return Ok(new FactoryShipmentsSummaryDto
        {
            TotalShipments = await query.CountAsync(),
            InTransit = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit),
            Received = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Received),
            PartiallyReceived = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PartiallyReceived),
            Rejected = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Rejected),
            Cancelled = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Cancelled)
        });
    }

    [HttpGet("shipments")]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetShipments(int factoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct).Where(s => s.SourceFactoryId == factoryId);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.DispatchDate)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => ToShipmentListItem(s)).ToListAsync();
        return Ok(new PagedResult<ShipmentListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("shipments/{shipmentId:int}")]
    public async Task<ActionResult<ShipmentDetailsDto>> GetShipmentDetails(int factoryId, int shipmentId)
    {
        var s = await _db.Shipments.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(x => x.Id == shipmentId && x.SourceFactoryId == factoryId);
        if (s == null) return NotFound(new { message = "Shipment not found for this factory." });

        return Ok(new ShipmentDetailsDto
        {
            Id = s.Id,
            TransferCode = s.TransferCode,
            ShipmentType = s.ShipmentType?.ToString(),
            Source = s.Source,
            Destination = s.Destination,
            ProductName = s.Batch?.MedicineProduct?.ProductName,
            BatchNumber = s.Batch?.BatchNumber,
            ExpectedQuantity = s.ExpectedQuantity,
            ReceivedQuantity = s.ReceivedQuantity,
            ShipmentStatus = s.ShipmentStatus?.ToString(),
            RequiresColdChain = s.RequiresColdChain,
            DispatchDate = s.DispatchDate,
            ReceivedDate = s.ReceivedDate,
            Notes = s.Notes,
            InspectionResult = s.InspectionResult
        });
    }

    [HttpPost("shipments")]
    public async Task<IActionResult> CreateDispatch(int factoryId, [FromBody] CreateDispatchDto? dto)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });
        if (!IsActive(factory)) return Conflict(new { message = "This factory is not Active. Operational actions are disabled." });

        var batch = await _db.Batches.Include(b => b.MedicineProduct).FirstOrDefaultAsync(b => b.Id == dto!.BatchId && b.FactoryId == factoryId);
        if (batch == null) return NotFound(new { message = "Batch not found for this factory." });

        if (batch.BatchStatus is BatchStatus.Quarantined or BatchStatus.Recalled or BatchStatus.Expired or BatchStatus.Cancelled)
            return Conflict(new { message = $"This batch cannot be dispatched while its status is {batch.BatchStatus}." });

        if (batch.BatchStatus != BatchStatus.ReadyForWarehouseDispatch && batch.BatchStatus != BatchStatus.PartiallyDispatched)
            return Conflict(new { message = "Batch must be ReadyForWarehouseDispatch (or PartiallyDispatched with remaining quantity) before dispatch." });

        var warehouse = await _db.Warehouses.FindAsync(dto?.DestinationWarehouseId ?? 0);
        if (warehouse == null || warehouse.WarehouseStatus != FacilityStatus.Active)
            return BadRequest(new { message = "Please select an active destination warehouse." });

        if (batch.MedicineProduct?.RequiresColdChain == true && warehouse.HasColdStorage != true)
            return Conflict(new { message = "This batch requires cold storage. Please select a warehouse with cold storage capability." });

        var alreadyDispatched = await _db.Shipments.Where(s => s.BatchId == batch.Id && s.SourceFactoryId == factoryId)
            .SumAsync(s => s.ExpectedQuantity ?? 0);
        var remaining = (batch.Quantity ?? 0) - alreadyDispatched;
        var dispatchQty = dto?.DispatchQuantity ?? 0;
        if (dispatchQty <= 0 || dispatchQty > remaining)
            return BadRequest(new { message = $"Dispatch quantity must be between 1 and the available quantity ({remaining})." });

        var shipment = new Shipment
        {
            TransferCode = $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}",
            BatchId = batch.Id,
            Batch = batch,
            ShipmentType = ShipmentType.FactoryToWarehouse,
            Source = factory.OfficialFactoryName,
            Destination = warehouse.OfficialWarehouseName,
            SourceFactoryId = factoryId,
            DestinationWarehouseId = warehouse.Id,
            ExpectedQuantity = dispatchQty,
            ShipmentStatus = ShipmentStatus.InTransit,
            RequiresColdChain = batch.MedicineProduct?.RequiresColdChain ?? false,
            Notes = dto?.Notes,
            DispatchDate = dto?.DispatchDate ?? DateTime.UtcNow
        };
        _db.Shipments.Add(shipment);

        batch.BatchStatus = (remaining - dispatchQty) <= 0 ? BatchStatus.FullyDispatched : BatchStatus.PartiallyDispatched;
        batch.SupplyChainStage = SupplyChainStage.InTransit;
        batch.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.DispatchShipment, "Shipment", shipment.TransferCode, "ReadyForWarehouseDispatch", batch.BatchStatus.ToString()));
        await _db.SaveChangesAsync();

        return Ok(new { message = "Shipment dispatched to warehouse.", transferCode = shipment.TransferCode, batchStatus = batch.BatchStatus.ToString() });
    }

    // ---------------- Alerts (view-only) ----------------
    [HttpGet("alerts")]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetAlerts(int factoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var query = _db.Alerts.Include(a => a.Batch).ThenInclude(b => b!.MedicineProduct).Include(a => a.Shipment)
            .Where(a => a.EntityType == EntityKind.Factory && a.EntityName == factory.OfficialFactoryName);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(a => new AlertListItemDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                EntityName = a.EntityName,
                ProductName = a.Batch != null && a.Batch.MedicineProduct != null ? a.Batch.MedicineProduct.ProductName : null,
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                ShipmentTransferCode = a.Shipment != null ? a.Shipment.TransferCode : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt,
                ResolvedAt = a.ResolvedAt
            }).ToListAsync();

        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("alerts/{alertId:int}")]
    public async Task<ActionResult<AlertDetailsDto>> GetAlertDetails(int factoryId, int alertId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var a = await _db.Alerts.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct).Include(x => x.Shipment)
            .FirstOrDefaultAsync(x => x.Id == alertId && x.EntityType == EntityKind.Factory && x.EntityName == factory.OfficialFactoryName);
        if (a == null) return NotFound(new { message = "Alert not found for this factory." });

        var isImpactful = a.AlertType is AlertType.Recall or AlertType.ComplianceIssue;
        return Ok(new AlertDetailsDto
        {
            Id = a.Id,
            AlertCode = a.AlertCode,
            AlertType = a.AlertType?.ToString(),
            Severity = a.Severity?.ToString(),
            EntityType = a.EntityType?.ToString(),
            EntityName = a.EntityName,
            Message = a.Message,
            ProductName = a.Batch?.MedicineProduct?.ProductName,
            BatchNumber = a.Batch?.BatchNumber,
            BatchId = a.BatchId,
            ShipmentTransferCode = a.Shipment?.TransferCode,
            ShipmentId = a.ShipmentId,
            AlertStatus = a.AlertStatus?.ToString(),
            CreatedAt = a.CreatedAt,
            ResolvedAt = a.ResolvedAt,
            ImpactedBatchStatus = isImpactful ? a.Batch?.BatchStatus?.ToString() : null,
            ImpactedUnitCodesStatus = a.AlertType == AlertType.Recall ? "Recalled" : a.AlertType == AlertType.ComplianceIssue ? "Blocked" : null,
            ImpactedInventoryStatus = a.AlertType == AlertType.Recall ? "Recalled" : a.AlertType == AlertType.ComplianceIssue ? "Blocked" : null,
            BatchDispatchBlocked = isImpactful
        });
    }

    // ---------------- Profile ----------------
    [HttpGet("profile")]
    public async Task<ActionResult<FactoryProfileFullDto>> GetProfile(int factoryId)
    {
        var factory = await GetFactoryAsync(factoryId);
        if (factory == null) return NotFound(new { message = "Factory not found." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.EntityType == EntityKind.Factory && u.EntityId == factoryId);
        var docs = await _db.EntityDocuments
            .Where(d => d.RegistrationRequest != null && d.RegistrationRequest.FactoryId == factoryId)
            .Select(d => new DocumentItemDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                UploadedAt = d.UploadedAt,
                DocumentStatus = d.DocumentStatus.ToString(),
                ReviewedBy = d.ReviewedBy,
                ReviewedAt = d.ReviewedAt,
                RejectionReason = d.RejectionReason
            }).ToListAsync();

        var licenses = await _db.EntityLicenses
            .Where(l => l.EntityType == EntityKind.Factory && l.EntityId == factoryId)
            .Select(l => new LicenseItemDto
            {
                Id = l.Id,
                LicenseType = l.LicenseType,
                LicenseNumber = l.LicenseNumber,
                IssueDate = l.IssueDate,
                ExpiryDate = l.ExpiryDate,
                Status = l.Status,
                FileUrl = l.FileUrl
            }).ToListAsync();

        return Ok(new FactoryProfileFullDto
        {
            Id = factory.Id,
            FactoryName = factory.OfficialFactoryName,
            FactoryCode = factory.FactoryCode,
            FactoryStatus = factory.FactoryStatus?.ToString(),
            RegistrationStatus = "Approved",
            MemberSince = factory.CreatedAt,
            Account = user == null ? null : new AccountInfoDto
            {
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                NationalIdMasked = MaskNationalId(user.NationalId),
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive
            },
            Entity = new EntityInfoDto
            {
                FactoryCode = factory.FactoryCode,
                OfficialFactoryName = factory.OfficialFactoryName,
                LegalCompanyName = factory.LegalCompanyName,
                DosageFormsProduced = factory.DosageFormsProduced,
                Governorate = factory.Governorate,
                City = factory.City,
                DistrictArea = factory.DistrictArea,
                FullAddress = factory.FullAddress,
                Phone = factory.Phone,
                Email = factory.Email,
                FactoryLicenseNumber = factory.FactoryLicenseNumber,
                TechnicalOperatingLicenseNumber = factory.TechnicalOperatingLicenseNumber,
                CommercialRegistrationNumber = factory.CommercialRegistrationNumber,
                TaxCardNumber = factory.TaxCardNumber,
                HasQualityControlLab = factory.HasQualityControlLab,
                HasFinishedGoodsStore = factory.HasFinishedGoodsStore,
                HasColdStorage = factory.HasColdStorage,
                HasQuarantineArea = factory.HasQuarantineArea,
                LicenseIssueDate = factory.LicenseIssueDate,
                LicenseExpiryDate = factory.LicenseExpiryDate,
                Status = factory.FactoryStatus?.ToString()
            },
            FactoryDetails = new FactoryDetailsDto
            {
                EstablishedYear = factory.EstablishedYear,
                TotalProductionLines = factory.TotalProductionLines,
                MainProductionTypes = factory.MainProductionTypes,
                ColdChainCapable = factory.HasColdStorage,
                StorageTypes = factory.StorageTypes,
                QualityCertificates = factory.QualityCertificates,
                Description = factory.Description
            },
            RegistrationInfo = new RegistrationInfoDto
            {
                RegistrationRequestNo = factory.RegistrationRequestNo,
                SubmittedAt = factory.RegistrationSubmittedAt,
                RegistrationStatus = "Approved",
                ApprovedAt = factory.RegistrationApprovedAt,
                ApprovedBy = factory.RegistrationApprovedBy,
                RegistrationExpiryDate = factory.RegistrationExpiryDate,
                Notes = factory.RegistrationNotes
            },
            Licenses = licenses,
            Documents = docs
        });
    }

    private static string? MaskNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length < 4) return nationalId;
        return $"**** **** {nationalId[^4..]}";
    }

    private static ShipmentListItemDto ToShipmentListItem(Shipment s) => new()
    {
        Id = s.Id,
        TransferCode = s.TransferCode,
        ProductName = s.Batch?.MedicineProduct?.ProductName,
        GTIN = s.Batch?.MedicineProduct?.GTIN,
        BatchNumber = s.Batch?.BatchNumber,
        Source = s.Source,
        Destination = s.Destination,
        ExpectedQuantity = s.ExpectedQuantity,
        ReceivedQuantity = s.ReceivedQuantity,
        RequiresColdChain = s.RequiresColdChain,
        ShipmentStatus = s.ShipmentStatus?.ToString(),
        DispatchDate = s.DispatchDate,
        ReceivedDate = s.ReceivedDate
    };

    private static AuditLog NewLog(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
        UserDisplayName = "Factory User",
        Role = SystemRole.FactoryUser,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = newVal,
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}

