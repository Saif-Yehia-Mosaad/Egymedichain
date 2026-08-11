using EgyMediChain.Api.Dtos;
using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Operational portal for a single Pharmacy (role: PharmacyUser). Scoped by {pharmacyId} in the
// route, same pattern as the Factory/Warehouse dashboards. No dispatch, no QR scan, no manual
// inventory edits - by design (see spec): the pharmacy only receives from a Warehouse and views.
[ApiController]
[Route("api/pharmacy-dashboard/{pharmacyId:int}")]
[Authorize(Roles = "PharmacyUser,SuperAdmin,MinistryAdmin")]
[ValidateEntityOwnership("pharmacyId")]
public class PharmacyDashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public PharmacyDashboardController(AppDbContext db) => _db = db;

    private async Task<Pharmacy?> GetPharmacyAsync(int pharmacyId) => await _db.Pharmacies.FindAsync(pharmacyId);
    private static bool IsActive(Pharmacy p) => p.PharmacyStatus == FacilityStatus.Active;

    // ---------------- Overview ----------------
    [HttpGet("overview")]
    public async Task<ActionResult<PharmacyOverviewDto>> GetOverview(int pharmacyId)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });

        var incomingQuery = _db.Shipments.Where(s => s.DestinationPharmacyId == pharmacyId);
        var inventoryQuery = _db.InventoryStocks.Where(i => i.PharmacyId == pharmacyId);

        var cards = new PharmacyOverviewCardsDto
        {
            IncomingShipments = await incomingQuery.CountAsync(),
            PendingReceiving = await incomingQuery.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit || s.ShipmentStatus == ShipmentStatus.PendingInspection),
            CurrentStock = await inventoryQuery.SumAsync(i => i.AvailableQuantity ?? 0),
            ColdChainStock = pharmacy.HasColdStorage == true
                ? await inventoryQuery.Where(i => i.Batch != null && i.Batch.MedicineProduct != null && i.Batch.MedicineProduct.RequiresColdChain == true).SumAsync(i => i.AvailableQuantity ?? 0)
                : null,
            OpenAlerts = await _db.Alerts.CountAsync(a => a.EntityType == EntityKind.Pharmacy && a.EntityName == pharmacy.OfficialPharmacyName && a.AlertStatus == AlertStatus.Open),
            RecalledStock = await inventoryQuery.Where(i => i.InventoryStatus == InventoryStatus.Recalled).SumAsync(i => i.AvailableQuantity ?? 0)
        };

        var recentIncoming = await incomingQuery.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .OrderByDescending(s => s.DispatchDate).Take(5)
            .Select(s => ToShipmentListItem(s)).ToListAsync();

        var stockSummary = await inventoryQuery.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .OrderByDescending(i => i.LastUpdated).Take(5)
            .Select(i => ToInventoryListItem(i)).ToListAsync();

        var recentAlerts = await _db.Alerts.Where(a => a.EntityType == EntityKind.Pharmacy && a.EntityName == pharmacy.OfficialPharmacyName)
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

        return Ok(new PharmacyOverviewDto { Cards = cards, RecentIncomingShipments = recentIncoming, CurrentStockSummary = stockSummary, RecentAlerts = recentAlerts });
    }

    // ---------------- Shipments (Warehouse -> Pharmacy) ----------------
    [HttpGet("shipments")]
    public async Task<ActionResult<PagedResult<ShipmentListItemDto>>> GetShipments(int pharmacyId,
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct).Where(s => s.DestinationPharmacyId == pharmacyId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.ShipmentStatus != null && s.ShipmentStatus.ToString() == status);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.DispatchDate)
            .Skip(Math.Max(0, (page - 1) * pageSize)).Take(pageSize <= 0 ? 10 : pageSize)
            .Select(s => ToShipmentListItem(s)).ToListAsync();
        return Ok(new PagedResult<ShipmentListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    [HttpGet("shipments/{shipmentId:int}")]
    public async Task<ActionResult<ShipmentDetailsDto>> GetShipmentDetails(int pharmacyId, int shipmentId)
    {
        var s = await _db.Shipments.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(x => x.Id == shipmentId && x.DestinationPharmacyId == pharmacyId);
        if (s == null) return NotFound(new { message = "Shipment not found for this pharmacy." });

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
    public async Task<IActionResult> ReceiveShipment(int pharmacyId, int shipmentId, [FromBody] ReceiveShipmentDto? dto)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });
        if (!IsActive(pharmacy)) return Conflict(new { message = "This pharmacy is not Active. Operational actions are disabled." });

        var shipment = await _db.Shipments.Include(s => s.Batch).ThenInclude(b => b!.MedicineProduct)
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.DestinationPharmacyId == pharmacyId);
        if (shipment == null) return NotFound(new { message = "Shipment not found for this pharmacy." });

        if (shipment.RequiresColdChain == true && pharmacy.HasColdStorage != true)
            return Conflict(new { message = "This batch requires cold storage. This pharmacy is not marked as cold storage capable." });

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
            _db.Alerts.Add(NewAlert(AlertType.ComplianceIssue, AlertSeverity.High, EntityKind.Pharmacy, pharmacy.OfficialPharmacyName, shipment.BatchId, shipment.Id,
                $"Shipment {shipment.TransferCode} was rejected on receipt at {pharmacy.OfficialPharmacyName}."));
            _db.AuditLogs.Add(NewLog(AuditAction.RejectShipment, "Shipment", shipment.TransferCode, "InTransit", "Rejected"));
        }
        else if (received < expected)
        {
            shipment.ShipmentStatus = ShipmentStatus.PartiallyReceived;
            UpsertInventory(pharmacyId, shipment.BatchId, pharmacy.OfficialPharmacyName, received);
            _db.Alerts.Add(NewAlert(AlertType.QuantityMismatch, AlertSeverity.Medium, EntityKind.Pharmacy, pharmacy.OfficialPharmacyName, shipment.BatchId, shipment.Id,
                $"Received quantity ({received}) does not match expected quantity ({expected}) for shipment {shipment.TransferCode}."));
            _db.AuditLogs.Add(NewLog(AuditAction.ReceiveShipment, "Shipment", shipment.TransferCode, "InTransit", "PartiallyReceived"));
        }
        else
        {
            shipment.ShipmentStatus = ShipmentStatus.Received;
            UpsertInventory(pharmacyId, shipment.BatchId, pharmacy.OfficialPharmacyName, received);
            _db.AuditLogs.Add(NewLog(AuditAction.ReceiveShipment, "Shipment", shipment.TransferCode, "InTransit", "Received"));
        }

        if (shipment.Batch != null && shipment.ShipmentStatus != ShipmentStatus.Rejected)
        {
            shipment.Batch.SupplyChainStage = SupplyChainStage.Available;
            shipment.Batch.CurrentLocation = pharmacy.OfficialPharmacyName;
            shipment.Batch.BatchStatus = BatchStatus.InPharmacy;
            shipment.Batch.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Shipment {shipment.ShipmentStatus}.", status = shipment.ShipmentStatus.ToString(), receivedQuantity = received });
    }

    private void UpsertInventory(int pharmacyId, int? batchId, string? holderName, long receivedQty)
    {
        var stock = _db.InventoryStocks.FirstOrDefault(i => i.PharmacyId == pharmacyId && i.BatchId == batchId);
        if (stock == null)
        {
            _db.InventoryStocks.Add(new InventoryStock
            {
                BatchId = batchId,
                HolderType = "Pharmacy",
                HolderName = holderName,
                PharmacyId = pharmacyId,
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

    // ---------------- Inventory ----------------
    [HttpGet("inventory")]
    public async Task<ActionResult<PagedResult<InventoryStockListItemDto>>> GetInventory(int pharmacyId,
        [FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .Where(i => i.PharmacyId == pharmacyId);

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
    public async Task<ActionResult<InventoryStockDetailsDto>> GetInventoryDetails(int pharmacyId, int inventoryId)
    {
        var stock = await _db.InventoryStocks.Include(i => i.Batch).ThenInclude(b => b!.MedicineProduct)
            .Include(i => i.Batch).ThenInclude(b => b!.Factory)
            .FirstOrDefaultAsync(i => i.Id == inventoryId && i.PharmacyId == pharmacyId);
        if (stock == null) return NotFound(new { message = "Stock record not found for this pharmacy." });

        var shipments = await _db.Shipments.Where(s => s.BatchId == stock.BatchId && s.DestinationPharmacyId == pharmacyId)
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

    // ---------------- Report Issue ----------------
    [HttpPost("report-issue")]
    public async Task<IActionResult> ReportIssue(int pharmacyId, [FromBody] ReportIssueDto? dto)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });
        if (!IsActive(pharmacy)) return Conflict(new { message = "This pharmacy is not Active. Operational actions are disabled." });

        var alertType = (dto?.AlertType ?? "ComplianceIssue") switch
        {
            "ColdChainIssue" => AlertType.ColdChainIssue,
            "QuantityMismatch" => AlertType.QuantityMismatch,
            "DamagedPackage" => AlertType.DamagedPackage,
            _ => AlertType.ComplianceIssue
        };

        var alert = NewAlert(alertType, AlertSeverity.Medium, EntityKind.Pharmacy, pharmacy.OfficialPharmacyName, dto?.BatchId, dto?.ShipmentId,
            dto?.Message ?? "Issue reported by pharmacy.");
        _db.Alerts.Add(alert);
        _db.AuditLogs.Add(NewLog(AuditAction.CreateAlert, "Alert", alert.AlertCode, null, "Open"));

        await _db.SaveChangesAsync();
        return Ok(new { message = "Issue reported to the Ministry.", alertCode = alert.AlertCode });
    }

    // ---------------- Alerts (view-only) ----------------
    [HttpGet("alerts")]
    public async Task<ActionResult<PagedResult<AlertListItemDto>>> GetAlerts(int pharmacyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });

        var query = _db.Alerts.Include(a => a.Batch).Where(a => a.EntityType == EntityKind.Pharmacy && a.EntityName == pharmacy.OfficialPharmacyName);
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

    // Scoped alert-status counts, independent of list pagination (Remaining Gaps - Factory
    // already had this; Pharmacy didn't).
    [HttpGet("alerts/counts")]
    public async Task<ActionResult<FactoryAlertsCountsDto>> GetAlertsCounts(int pharmacyId)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });

        var query = _db.Alerts.Where(a => a.EntityType == EntityKind.Pharmacy && a.EntityName == pharmacy.OfficialPharmacyName);
        return Ok(new FactoryAlertsCountsDto
        {
            Open = await query.CountAsync(a => a.AlertStatus == AlertStatus.Open),
            UnderReview = await query.CountAsync(a => a.AlertStatus == AlertStatus.UnderReview),
            Resolved = await query.CountAsync(a => a.AlertStatus == AlertStatus.Resolved),
            Dismissed = await query.CountAsync(a => a.AlertStatus == AlertStatus.Dismissed),
            Total = await query.CountAsync()
        });
    }

    // Alert detail fetch, scoped to this pharmacy (Remaining Gaps - only Factory had this before).
    [HttpGet("alerts/{alertId:int}")]
    public async Task<ActionResult<AlertDetailsDto>> GetAlertDetails(int pharmacyId, int alertId)
    {
        var pharmacy = await GetPharmacyAsync(pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });

        var a = await _db.Alerts.Include(x => x.Batch).ThenInclude(b => b!.MedicineProduct).Include(x => x.Shipment)
            .FirstOrDefaultAsync(x => x.Id == alertId && x.EntityType == EntityKind.Pharmacy && x.EntityName == pharmacy.OfficialPharmacyName);
        if (a == null) return NotFound(new { message = "Alert not found for this pharmacy." });

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
    public async Task<ActionResult<OperationalProfileDto>> GetProfile(int pharmacyId)
    {
        var pharmacy = await _db.Pharmacies.Include(p => p.DefaultWarehouse).FirstOrDefaultAsync(p => p.Id == pharmacyId);
        if (pharmacy == null) return NotFound(new { message = "Pharmacy not found." });

        var user = await _db.SystemUsers.FirstOrDefaultAsync(u => u.EntityType == EntityKind.Pharmacy && u.EntityId == pharmacyId);
        var docs = await _db.EntityDocuments
            .Where(d => d.RegistrationRequest != null && d.RegistrationRequest.PharmacyId == pharmacyId)
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
                PharmacyCode = pharmacy.PharmacyCode,
                OfficialPharmacyName = pharmacy.OfficialPharmacyName,
                PharmacyType = pharmacy.PharmacyType,
                Governorate = pharmacy.Governorate,
                City = pharmacy.City,
                DistrictArea = pharmacy.DistrictArea,
                FullAddress = pharmacy.FullAddress,
                Phone = pharmacy.Phone,
                Email = pharmacy.Email,
                DefaultWarehouseName = pharmacy.DefaultWarehouse?.OfficialWarehouseName,
                HasColdStorage = pharmacy.HasColdStorage,
                PharmacyLicenseNumber = pharmacy.PharmacyLicenseNumber,
                PharmacistSyndicateId = pharmacy.PharmacistSyndicateId,
                LicenseIssueDate = pharmacy.LicenseIssueDate,
                LicenseExpiryDate = pharmacy.LicenseExpiryDate,
                Status = pharmacy.PharmacyStatus?.ToString()
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
        UserDisplayName = "Pharmacy User",
        Role = SystemRole.PharmacyUser,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = newVal,
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}
