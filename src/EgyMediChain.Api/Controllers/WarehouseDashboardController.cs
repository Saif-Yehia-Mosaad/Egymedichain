using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Operational portal for a single Warehouse (role: WarehouseUser). Scoped by {warehouseId} in
// the route, same pattern as the Factory dashboard.
[ApiController]
[Route("api/warehouse-dashboard/{warehouseId:int}")]
[Authorize(Roles = "WarehouseUser,SuperAdmin,MinistryAdmin")]
[ValidateEntityOwnership("warehouseId")]
public class WarehouseDashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public WarehouseDashboardController(AppDbContext db) => _db = db;

    private async Task<Warehouse?> GetWarehouseAsync(int warehouseId) => await _db.Warehouses.FindAsync(warehouseId);
    private static bool IsActive(Warehouse w) => w.WarehouseStatus == FacilityStatus.Active;

    // ---------------- Overview ----------------
    [HttpGet("overview")]
    public async Task<ActionResult<WarehouseOverviewDto>> GetOverview(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var incomingQuery = _db.Shipments.Where(s => s.DestinationWarehouseId == warehouseId);
        var outgoingQuery = _db.Shipments.Where(s => s.SourceWarehouseId == warehouseId);
        var inventoryQuery = _db.InventoryStocks.Where(i => i.WarehouseId == warehouseId);

        var readyStockSkus = await inventoryQuery.Where(i => i.InventoryStatus == InventoryStatus.Active && i.AvailableQuantity > 0)
            .Select(i => i.Batch!.MedicineProductId).Distinct().CountAsync();

        var cards = new WarehouseOverviewCardsDto
        {
            IncomingShipments = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit || s.ShipmentStatus == ShipmentStatus.PendingInspection),
            PendingInspection = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PendingInspection),
            ReadyStockSkus = readyStockSkus,
            TotalStockUnits = await inventoryQuery.SumAsync(i => i.AvailableQuantity ?? 0),
            OutgoingShipments = await outgoingQuery.CountAsync(),
            OpenAlerts = await _db.Alerts.CountAsync(a => a.EntityType == EntityKind.Warehouse && a.EntityName == warehouse.OfficialWarehouseName && a.AlertStatus == AlertStatus.Open)
        };

        var recentIncoming = await incomingQuery.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .OrderByDescending(s => s.DispatchDate).Take(5)
            .Select(s => ToShipmentListItem(s)).ToListAsync();

        var recentOutgoing = await outgoingQuery.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .OrderByDescending(s => s.DispatchDate).Take(5)
            .Select(s => ToShipmentListItem(s)).ToListAsync();

        var soonCutoff = DateTime.UtcNow.AddDays(90);
        var topProducts = await inventoryQuery.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Where(i => i.Batch != null && i.Batch.MedicineProduct != null)
            .GroupBy(i => new { i.Batch!.MedicineProduct!.ProductName, i.Batch.MedicineProduct.Strength, i.Batch.MedicineProduct.DosageForm })
            .Select(g => new ProductStockSummaryItemDto
            {
                ProductName = g.Key.ProductName,
                Strength = g.Key.Strength,
                DosageForm = g.Key.DosageForm,
                TotalStock = g.Sum(i => i.TotalReceivedQuantity ?? 0),
                AvailableStock = g.Sum(i => i.AvailableQuantity ?? 0),
                InTransit = g.Sum(i => i.ReservedQuantity ?? 0),
                ExpiringSoon = g.Where(i => i.Batch!.ExpiryDate != null && i.Batch.ExpiryDate <= soonCutoff).Sum(i => i.AvailableQuantity ?? 0)
            })
            .OrderByDescending(p => p.TotalStock)
            .Take(5)
            .ToListAsync();

        var recentAlerts = await _db.Alerts.Where(a => a.EntityType == EntityKind.Warehouse && a.EntityName == warehouse.OfficialWarehouseName)
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

        return Ok(new WarehouseOverviewDto { Cards = cards, RecentIncomingShipments = recentIncoming, RecentOutgoingShipments = recentOutgoing, TopProducts = topProducts, RecentAlerts = recentAlerts });
    }

    // ---------------- Inventory Summary Cards ----------------
    [HttpGet("inventory/summary")]
    public async Task<ActionResult<WarehouseInventorySummaryCardsDto>> GetInventorySummary(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var inventoryQuery = _db.InventoryStocks.Include(i => i.Batch).Where(i => i.WarehouseId == warehouseId);
        var soonCutoff = DateTime.UtcNow.AddDays(90);

        return Ok(new WarehouseInventorySummaryCardsDto
        {
            TotalProductsSkus = await inventoryQuery.Select(i => i.Batch!.MedicineProductId).Distinct().CountAsync(),
            TotalReceivedUnits = await inventoryQuery.SumAsync(i => i.TotalReceivedQuantity ?? 0),
            AvailableUnits = await inventoryQuery.SumAsync(i => i.AvailableQuantity ?? 0),
            ReservedUnits = await inventoryQuery.SumAsync(i => i.ReservedQuantity ?? 0),
            QuarantinedUnits = await inventoryQuery.SumAsync(i => i.QuarantinedQuantity ?? 0),
            ExpiringSoonUnits = await inventoryQuery.Where(i => i.Batch != null && i.Batch.ExpiryDate != null && i.Batch.ExpiryDate <= soonCutoff).SumAsync(i => i.AvailableQuantity ?? 0)
        });
    }

    // ---------------- Shipments Summary Cards ----------------
    [HttpGet("shipments/summary")]
    public async Task<ActionResult<WarehouseShipmentsSummaryDto>> GetShipmentsSummary(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var incomingQuery = _db.Shipments.Where(s => s.DestinationWarehouseId == warehouseId);
        return Ok(new WarehouseShipmentsSummaryDto
        {
            IncomingShipments = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit),
            PendingInspection = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PendingInspection),
            Received = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Received),
            PartiallyReceived = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PartiallyReceived),
            Rejected = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Rejected)
        });
    }

    // ---------------- Shipments ----------------
    [HttpGet("shipments/incoming")]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetIncoming(int warehouseId,
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct).Where(s => s.DestinationWarehouseId == warehouseId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.ShipmentStatus != null && s.ShipmentStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.DispatchDate)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => ToShipmentListItem(s)).ToListAsync();
        return Ok(new PagedResult<ShipmentListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("shipments/outgoing")]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetOutgoing(int warehouseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct).Where(s => s.SourceWarehouseId == warehouseId);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.DispatchDate)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => ToShipmentListItem(s)).ToListAsync();
        return Ok(new PagedResult<ShipmentListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("shipments/{shipmentId:int}")]
    public async Task<ActionResult<ShipmentDetailsDto>> GetShipmentDetails(int warehouseId, int shipmentId)
    {
        var s = await _db.Shipments.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(x => x.Id == shipmentId && (x.DestinationWarehouseId == warehouseId || x.SourceWarehouseId == warehouseId));
        if (s == null) return NotFound(new { message = "Shipment not found for this warehouse." });

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

    [HttpPost("shipments/{shipmentId:int}/receive")]
    public async Task<IActionResult> ReceiveShipment(int warehouseId, int shipmentId, [FromBody] ReceiveShipmentDto? dto)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });
        if (!IsActive(warehouse)) return Conflict(new { message = "This warehouse is not Active. Operational actions are disabled." });

        var shipment = await _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.DestinationWarehouseId == warehouseId);
        if (shipment == null) return NotFound(new { message = "Shipment not found for this warehouse." });

        if (shipment.RequiresColdChain == true && warehouse.HasColdStorage != true)
            return Conflict(new { message = "This batch requires cold storage. This warehouse is not marked as cold storage capable." });

        var expected = shipment.ExpectedQuantity ?? 0;
        var inspection = dto?.InspectionResult ?? "Accepted";
        long received = inspection == "Rejected" ? 0 : (dto?.ReceivedQuantity ?? expected);

        shipment.ReceivedQuantity = received;
        shipment.InspectionResult = inspection;
        shipment.Notes = dto?.Notes ?? shipment.Notes;
        shipment.ReceivedDate = DateTime.UtcNow;

        if (inspection == "Rejected" || received == 0)
        {
            shipment.ShipmentStatus = ShipmentStatus.Rejected;
            _db.Alerts.Add(NewAlert(AlertType.ComplianceIssue, AlertSeverity.High, EntityKind.Warehouse, warehouse.OfficialWarehouseName, shipment.BatchId, shipment.Id,
                $"Shipment {shipment.TransferCode} was rejected on receipt at {warehouse.OfficialWarehouseName}."));
            _db.AuditLogs.Add(NewLog(AuditAction.RejectShipment, "Shipment", shipment.TransferCode, "InTransit", "Rejected"));
        }
        else if (received < expected)
        {
            shipment.ShipmentStatus = ShipmentStatus.PartiallyReceived;
            UpsertInventory(warehouseId, shipment.BatchId, warehouse.OfficialWarehouseName, received);
            _db.Alerts.Add(NewAlert(AlertType.QuantityMismatch, AlertSeverity.Medium, EntityKind.Warehouse, warehouse.OfficialWarehouseName, shipment.BatchId, shipment.Id,
                $"Received quantity ({received}) does not match expected quantity ({expected}) for shipment {shipment.TransferCode}."));
            _db.AuditLogs.Add(NewLog(AuditAction.ReceiveShipment, "Shipment", shipment.TransferCode, "InTransit", "PartiallyReceived"));
        }
        else
        {
            shipment.ShipmentStatus = ShipmentStatus.Received;
            UpsertInventory(warehouseId, shipment.BatchId, warehouse.OfficialWarehouseName, received);
            _db.AuditLogs.Add(NewLog(AuditAction.ReceiveShipment, "Shipment", shipment.TransferCode, "InTransit", "Received"));
        }

        if (shipment.Batch != null && shipment.ShipmentStatus != ShipmentStatus.Rejected)
        {
            shipment.Batch.SupplyChainStage = SupplyChainStage.Stored;
            shipment.Batch.CurrentLocation = warehouse.OfficialWarehouseName;
            shipment.Batch.BatchStatus = BatchStatus.InWarehouse;
            shipment.Batch.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Shipment {shipment.ShipmentStatus}.", status = shipment.ShipmentStatus.ToString(), receivedQuantity = received });
    }

    private void UpsertInventory(int warehouseId, int? batchId, string? holderName, long receivedQty)
    {
        var stock = _db.InventoryStocks.FirstOrDefault(i => i.WarehouseId == warehouseId && i.BatchId == batchId);
        if (stock == null)
        {
            _db.InventoryStocks.Add(new InventoryStock
            {
                BatchId = batchId,
                HolderType = "Warehouse",
                HolderName = holderName,
                WarehouseId = warehouseId,
                TotalReceivedQuantity = receivedQty,
                AvailableQuantity = receivedQty,
                ReservedQuantity = 0,
                QuarantinedQuantity = 0,
                InventoryStatus = InventoryStatus.Active,
                LastUpdated = DateTime.UtcNow
            });
        }
        else
        {
            stock.TotalReceivedQuantity = (stock.TotalReceivedQuantity ?? 0) + receivedQty;
            stock.AvailableQuantity = (stock.AvailableQuantity ?? 0) + receivedQty;
            stock.LastUpdated = DateTime.UtcNow;
        }
    }

    // ---------------- Inventory & Dispatch ----------------
    [HttpGet("inventory")]
    public async Task<ActionResult<PagedResult<InventoryStockListItemDto>>> GetInventory(int warehouseId,
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .Where(i => i.WarehouseId == warehouseId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => (i.Batch != null && i.Batch.BatchNumber != null && i.Batch.BatchNumber.Contains(search)) ||
                                      (i.Batch != null && i.Batch.MedicineProduct != null && i.Batch.MedicineProduct.ProductName != null && i.Batch.MedicineProduct.ProductName.Contains(search)));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.InventoryStatus != null && i.InventoryStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.LastUpdated)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(i => ToInventoryListItem(i)).ToListAsync();

        return Ok(new PagedResult<InventoryStockListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("inventory/{inventoryId:int}")]
    public async Task<ActionResult<InventoryStockDetailsDto>> GetInventoryDetails(int warehouseId, int inventoryId)
    {
        var stock = await _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .FirstOrDefaultAsync(i => i.Id == inventoryId && i.WarehouseId == warehouseId);
        if (stock == null) return NotFound(new { message = "Stock record not found for this warehouse." });

        var shipments = await _db.Shipments.Where(s => s.BatchId == stock.BatchId && (s.DestinationWarehouseId == warehouseId || s.SourceWarehouseId == warehouseId))
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

        return Ok(new InventoryStockDetailsDto
        {
            Id = stock.Id,
            ProductInfo = new ProductInfoDto
            {
                ProductName = stock.Batch?.MedicineProduct?.ProductName,
                GTIN = stock.Batch?.MedicineProduct?.GTIN,
                DosageForm = stock.Batch?.MedicineProduct?.DosageForm,
                Strength = stock.Batch?.MedicineProduct?.Strength,
                RequiresColdChain = stock.Batch?.MedicineProduct?.RequiresColdChain,
                ProductStatus = stock.Batch?.MedicineProduct?.ProductStatus
            },
            BatchInfo = new BatchInfoDto
            {
                BatchNumber = stock.Batch?.BatchNumber,
                FactoryName = stock.Batch?.Factory?.OfficialFactoryName,
                ManufacturingDate = stock.Batch?.ManufacturingDate,
                ExpiryDate = stock.Batch?.ExpiryDate,
                BatchStatus = stock.Batch?.BatchStatus?.ToString(),
                SupplyChainStage = stock.Batch?.SupplyChainStage?.ToString()
            },
            WarehouseInventory = new InventoryDistributionItemDto
            {
                HolderType = stock.HolderType,
                HolderName = stock.HolderName,
                TotalReceivedQuantity = stock.TotalReceivedQuantity,
                AvailableQuantity = stock.AvailableQuantity,
                ReservedQuantity = stock.ReservedQuantity,
                QuarantinedQuantity = stock.QuarantinedQuantity,
                InventoryStatus = stock.InventoryStatus?.ToString(),
                LastUpdated = stock.LastUpdated
            },
            RelatedShipments = shipments
        });
    }

    [HttpPost("inventory/{inventoryId:int}/dispatch-to-pharmacy")]
    public async Task<IActionResult> DispatchToPharmacy(int warehouseId, int inventoryId, [FromBody] DispatchToPharmacyDto? dto)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });
        if (!IsActive(warehouse)) return Conflict(new { message = "This warehouse is not Active. Operational actions are disabled." });

        var stock = await _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(i => i.Id == inventoryId && i.WarehouseId == warehouseId);
        if (stock == null) return NotFound(new { message = "Stock record not found for this warehouse." });

        if (stock.Batch?.BatchStatus is BatchStatus.Quarantined or BatchStatus.Recalled or BatchStatus.Expired or BatchStatus.Cancelled ||
            stock.InventoryStatus is InventoryStatus.Blocked or InventoryStatus.Recalled or InventoryStatus.Quarantined)
            return Conflict(new { message = "This batch/stock is not available for dispatch (quarantined, recalled or blocked)." });

        var pharmacy = await _db.Pharmacies.FindAsync(dto?.TargetPharmacyId ?? 0);
        if (pharmacy == null || pharmacy.PharmacyStatus != FacilityStatus.Active)
            return BadRequest(new { message = "Please select an active target pharmacy." });

        if (stock.Batch?.MedicineProduct?.RequiresColdChain == true && pharmacy.HasColdStorage != true)
            return Conflict(new { message = "This batch requires cold storage. Please select a pharmacy with cold storage capability." });

        var dispatchQty = dto?.DispatchQuantity ?? 0;
        if (dispatchQty <= 0 || dispatchQty > (stock.AvailableQuantity ?? 0))
            return BadRequest(new { message = $"Dispatch quantity must be between 1 and the available quantity ({stock.AvailableQuantity ?? 0})." });

        var shipment = new Shipment
        {
            TransferCode = $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}",
            BatchId = stock.BatchId,
            ShipmentType = ShipmentType.WarehouseToPharmacy,
            Source = warehouse.OfficialWarehouseName,
            Destination = pharmacy.OfficialPharmacyName,
            SourceWarehouseId = warehouseId,
            DestinationPharmacyId = pharmacy.Id,
            ExpectedQuantity = dispatchQty,
            ShipmentStatus = ShipmentStatus.InTransit,
            RequiresColdChain = stock.Batch?.MedicineProduct?.RequiresColdChain ?? false,
            Notes = dto?.Notes,
            DispatchDate = dto?.DispatchDate ?? DateTime.UtcNow
        };
        _db.Shipments.Add(shipment);

        stock.AvailableQuantity = (stock.AvailableQuantity ?? 0) - dispatchQty;
        stock.ReservedQuantity = (stock.ReservedQuantity ?? 0) + dispatchQty;
        stock.LastUpdated = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(AuditAction.DispatchShipment, "Shipment", shipment.TransferCode, "Active", "InTransit"));
        await _db.SaveChangesAsync();

        return Ok(new { message = "Shipment dispatched to pharmacy.", transferCode = shipment.TransferCode });
    }

    [HttpPost("inventory/{inventoryId:int}/move-to-quarantine")]
    public async Task<IActionResult> MoveToQuarantine(int warehouseId, int inventoryId, [FromBody] MoveToQuarantineDto? dto)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });
        if (!IsActive(warehouse)) return Conflict(new { message = "This warehouse is not Active. Operational actions are disabled." });
        if (warehouse.HasQuarantineArea != true)
            return Conflict(new { message = "This warehouse does not have a quarantine area. Use Report Issue instead." });

        var stock = await _db.InventoryStocks.FirstOrDefaultAsync(i => i.Id == inventoryId && i.WarehouseId == warehouseId);
        if (stock == null) return NotFound(new { message = "Stock record not found for this warehouse." });

        var qty = dto?.QuarantineQuantity ?? (stock.AvailableQuantity ?? 0);
        if (qty <= 0 || qty > (stock.AvailableQuantity ?? 0))
            return BadRequest(new { message = $"Quarantine quantity must be between 1 and the available quantity ({stock.AvailableQuantity ?? 0})." });

        stock.AvailableQuantity = (stock.AvailableQuantity ?? 0) - qty;
        stock.QuarantinedQuantity = (stock.QuarantinedQuantity ?? 0) + qty;
        stock.InventoryStatus = InventoryStatus.Quarantined;
        stock.LastUpdated = DateTime.UtcNow;

        _db.Alerts.Add(NewAlert(AlertType.ComplianceIssue, AlertSeverity.Medium, EntityKind.Warehouse, warehouse.OfficialWarehouseName, stock.BatchId, null,
            dto?.Reason ?? $"{qty} units moved to quarantine at {warehouse.OfficialWarehouseName}."));
        _db.AuditLogs.Add(NewLog(AuditAction.QuarantineStock, "InventoryStock", stock.Id.ToString(), "Active", "Quarantined"));

        await _db.SaveChangesAsync();
        return Ok(new { message = "Stock moved to quarantine.", quarantinedQuantity = stock.QuarantinedQuantity });
    }

    // ---------------- Report Issue ----------------
    [HttpPost("report-issue")]
    public async Task<IActionResult> ReportIssue(int warehouseId, [FromBody] ReportIssueDto? dto)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });
        if (!IsActive(warehouse)) return Conflict(new { message = "This warehouse is not Active. Operational actions are disabled." });

        var alertType = (dto?.AlertType ?? "ComplianceIssue") switch
        {
            "ColdChainIssue" => AlertType.ColdChainIssue,
            "QuantityMismatch" => AlertType.QuantityMismatch,
            "DamagedPackage" => AlertType.DamagedPackage,
            _ => AlertType.ComplianceIssue
        };

        var alert = NewAlert(alertType, AlertSeverity.Medium, EntityKind.Warehouse, warehouse.OfficialWarehouseName, dto?.BatchId, dto?.ShipmentId,
            dto?.Message ?? "Issue reported by warehouse.");
        _db.Alerts.Add(alert);
        _db.AuditLogs.Add(NewLog(AuditAction.CreateAlert, "Alert", alert.AlertCode, null, "Open"));

        await _db.SaveChangesAsync();
        return Ok(new { message = "Issue reported to the Ministry.", alertCode = alert.AlertCode });
    }

    // ---------------- Alerts (view-only) ----------------
    [HttpGet("alerts")]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetAlerts(int warehouseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var query = _db.Alerts.Include(a => a.Batch).Where(a => a.EntityType == EntityKind.Warehouse && a.EntityName == warehouse.OfficialWarehouseName);
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
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                Message = a.Message,
                AlertStatus = a.AlertStatus.ToString(),
                CreatedAt = a.CreatedAt
            }).ToListAsync();

        return Ok(new PagedResult<AlertListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Scoped alert-status counts, independent of list pagination (Frontend Integration Audit,
    // Remaining Gaps - Factory already had this; Warehouse didn't).
    [HttpGet("alerts/counts")]
    public async Task<ActionResult<FactoryAlertsCountsDto>> GetAlertsCounts(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var query = _db.Alerts.Where(a => a.EntityType == EntityKind.Warehouse && a.EntityName == warehouse.OfficialWarehouseName);
        return Ok(new FactoryAlertsCountsDto
        {
            Open = await query.CountAsync(a => a.AlertStatus == AlertStatus.Open),
            UnderReview = await query.CountAsync(a => a.AlertStatus == AlertStatus.UnderReview),
            Resolved = await query.CountAsync(a => a.AlertStatus == AlertStatus.Resolved),
            Dismissed = await query.CountAsync(a => a.AlertStatus == AlertStatus.Dismissed),
            Total = await query.CountAsync()
        });
    }

    // Alert detail fetch, scoped to this warehouse (Remaining Gaps - only Factory had this before;
    // Warehouse/Pharmacy were opening alert details from the already-loaded list row).
    [HttpGet("alerts/{alertId:int}")]
    public async Task<ActionResult<AlertDetailsDto>> GetAlertDetails(int warehouseId, int alertId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var a = await _db.Alerts.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct).Include(x => x.Shipment)
            .FirstOrDefaultAsync(x => x.Id == alertId && x.EntityType == EntityKind.Warehouse && x.EntityName == warehouse.OfficialWarehouseName);
        if (a == null) return NotFound(new { message = "Alert not found for this warehouse." });

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

    // Warehouse-scoped pharmacy directory, for dispatch destination selection - mirrors the
    // Factory→Warehouse directory (Remaining Gaps: "Warehouse→Pharmacy directory ... still calls
    // the Admin-tagged GET /api/pharmacies").
    [HttpGet("pharmacies")]
    public async Task<ActionResult<PagedResult<PharmacyOptionDto>>> GetPharmaciesForDispatch(int warehouseId,
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var query = _db.Pharmacies.Where(p => p.PharmacyStatus == FacilityStatus.Active);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.OfficialPharmacyName != null && p.OfficialPharmacyName.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderBy(p => p.OfficialPharmacyName)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 50 : pageSize)
            .Select(p => new PharmacyOptionDto
            {
                Id = p.Id,
                PharmacyName = p.OfficialPharmacyName,
                Governorate = p.Governorate,
                City = p.City,
                PharmacyStatus = p.PharmacyStatus.ToString()
            }).ToListAsync();

        return Ok(new PagedResult<PharmacyOptionDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // ---------------- Profile ----------------
    [HttpGet("profile")]
    public async Task<ActionResult<OperationalProfileDto>> GetProfile(int warehouseId)
    {
        var warehouse = await GetWarehouseAsync(warehouseId);
        if (warehouse == null) return NotFound(new { message = "Warehouse not found." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.EntityType == EntityKind.Warehouse && u.EntityId == warehouseId);
        var docs = await _db.EntityDocuments
            .Where(d => d.RegistrationRequest != null && d.RegistrationRequest.WarehouseId == warehouseId)
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

        return Ok(new OperationalProfileDto
        {
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
                WarehouseCode = warehouse.WarehouseCode,
                OfficialWarehouseName = warehouse.OfficialWarehouseName,
                WarehouseType = warehouse.WarehouseType,
                Governorate = warehouse.Governorate,
                City = warehouse.City,
                DistrictArea = warehouse.DistrictArea,
                FullAddress = warehouse.FullAddress,
                Phone = warehouse.Phone,
                Email = warehouse.Email,
                WarehouseLicenseNumber = warehouse.WarehouseLicenseNumber,
                HasColdStorage = warehouse.HasColdStorage,
                HasQuarantineArea = warehouse.HasQuarantineArea,
                HasDeliveryService = warehouse.HasDeliveryService,
                LicenseIssueDate = warehouse.LicenseIssueDate,
                LicenseExpiryDate = warehouse.LicenseExpiryDate,
                Status = warehouse.WarehouseStatus?.ToString()
            },
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

    private static InventoryStockListItemDto ToInventoryListItem(InventoryStock i) => new()
    {
        Id = i.Id,
        BatchId = i.BatchId,
        ProductName = i.Batch?.MedicineProduct?.ProductName,
        GTIN = i.Batch?.MedicineProduct?.GTIN,
        DosageForm = i.Batch?.MedicineProduct?.DosageForm,
        Strength = i.Batch?.MedicineProduct?.Strength,
        BatchNumber = i.Batch?.BatchNumber,
        FactoryName = i.Batch?.Factory?.OfficialFactoryName,
        TotalReceivedQuantity = i.TotalReceivedQuantity,
        AvailableQuantity = i.AvailableQuantity,
        ReservedQuantity = i.ReservedQuantity,
        QuarantinedQuantity = i.QuarantinedQuantity,
        ExpiryDate = i.Batch?.ExpiryDate,
        RequiresColdChain = i.Batch?.MedicineProduct?.RequiresColdChain,
        InventoryStatus = i.InventoryStatus?.ToString()
    };

    private static Alert NewAlert(AlertType type, AlertSeverity severity, EntityKind entityKind, string? entityName, int? batchId, int? shipmentId, string message) => new()
    {
        AlertCode = $"ALERT-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
        AlertType = type,
        Severity = severity,
        EntityType = entityKind,
        EntityName = entityName,
        BatchId = batchId,
        ShipmentId = shipmentId,
        Message = message,
        AlertStatus = AlertStatus.Open,
        CreatedAt = DateTime.UtcNow
    };

    private static AuditLog NewLog(AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
        UserDisplayName = "Warehouse User",
        Role = SystemRole.WarehouseUser,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = newVal,
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}
