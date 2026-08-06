using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/overview")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class OverviewController : ControllerBase
{
    private readonly AppDbContext _db;
    public OverviewController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<OverviewDto>> Get()
    {
        // null = full Ministry access (SuperAdmin, or a MinistryAdmin/MinistryViewer created
        // without an EntityScope). "Factory"/"Warehouse"/"Pharmacy" = this account was created
        // scoped to just that entity type (see AdminController.AddMinistryAdmin) and the whole
        // dashboard below only shows numbers relevant to that type.
        var scope = GetMinistryEntityScope();

        var cards = new OverviewCardsDto
        {
            PendingRequests = await _db.RegistrationRequests.CountAsync(r =>
                r.RegistrationStatus == RegistrationStatus.Pending &&
                (scope == null || (r.EntityType != null && r.EntityType.ToString() == scope))),

            ActiveFactories = (scope == null || scope == "Factory")
                ? await _db.Factories.CountAsync(f => f.FactoryStatus == FacilityStatus.Active) : 0,

            ActiveWarehouses = (scope == null || scope == "Warehouse")
                ? await _db.Warehouses.CountAsync(w => w.WarehouseStatus == FacilityStatus.Active) : 0,

            ActivePharmacies = (scope == null || scope == "Pharmacy")
                ? await _db.Pharmacies.CountAsync(p => p.PharmacyStatus == FacilityStatus.Active) : 0,

            // Totals (EntitiesManagement-Backend-Gaps.md, item 4) - so the Entities Management
            // screen doesn't need 3 extra "?pageSize=1" requests just to read totalCount.
            TotalFactories = (scope == null || scope == "Factory") ? await _db.Factories.CountAsync() : 0,
            TotalWarehouses = (scope == null || scope == "Warehouse") ? await _db.Warehouses.CountAsync() : 0,
            TotalPharmacies = (scope == null || scope == "Pharmacy") ? await _db.Pharmacies.CountAsync() : 0,

            // Batches are produced by factories, so this card only means something for an
            // unscoped account or one scoped to Factory.
            ActiveBatches = (scope == null || scope == "Factory")
                ? await _db.Batches.CountAsync(b => b.BatchStatus != BatchStatus.Recalled) : 0,

            ShipmentsInTransit = scope switch
            {
                null => await _db.Shipments.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit),
                "Factory" => await _db.Shipments.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit && s.SourceFactoryId != null),
                "Warehouse" => await _db.Shipments.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit && (s.SourceWarehouseId != null || s.DestinationWarehouseId != null)),
                "Pharmacy" => await _db.Shipments.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit && s.DestinationPharmacyId != null),
                _ => 0
            },

            OpenAlerts = await _db.Alerts.CountAsync(a =>
                a.AlertStatus == AlertStatus.Open &&
                (scope == null || (a.EntityType != null && a.EntityType.ToString() == scope))),

            // Public scans aren't tied to one entity type in this schema, so they're only shown
            // to unscoped Ministry accounts.
            SuspiciousPublicScans = scope == null
                ? await _db.PublicVerificationScans.CountAsync(s => s.VerificationResult == VerificationResult.Suspicious) : 0
        };

        var recentRequestsQuery = _db.RegistrationRequests.AsQueryable();
        if (scope != null) recentRequestsQuery = recentRequestsQuery.Where(r => r.EntityType != null && r.EntityType.ToString() == scope);

        var recentRequests = await recentRequestsQuery
            .OrderByDescending(r => r.SubmittedAt)
            .Take(5)
            .Select(r => new RecentRegistrationRequestDto
            {
                Id = r.Id,
                RequestCode = r.RequestCode,
                EntityType = r.EntityType.ToString(),
                EntityName = r.EntityName,
                SubmittedBy = r.RepresentativeName,
                SubmittedAt = r.SubmittedAt,
                RegistrationStatus = r.RegistrationStatus.ToString()
            }).ToListAsync();

        var recentAlertsQuery = _db.Alerts.Include(a => a.Batch).AsQueryable();
        if (scope != null) recentAlertsQuery = recentAlertsQuery.Where(a => a.EntityType != null && a.EntityType.ToString() == scope);

        var recentAlerts = await recentAlertsQuery
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecentAlertDto
            {
                Id = a.Id,
                AlertCode = a.AlertCode,
                AlertType = a.AlertType.ToString(),
                Severity = a.Severity.ToString(),
                EntityType = a.EntityType.ToString(),
                BatchNumber = a.Batch != null ? a.Batch.BatchNumber : null,
                CreatedAt = a.CreatedAt,
                AlertStatus = a.AlertStatus.ToString()
            }).ToListAsync();

        // Recent batch activity is inherently a factory-side feed (batches are produced by
        // factories), so a Warehouse/Pharmacy-scoped account just gets an empty list here.
        var recentBatches = (scope == null || scope == "Factory")
            ? await _db.Batches
                .Include(b => b.MedicineProduct)
                .Include(b => b.Factory)
                .OrderByDescending(b => b.UpdatedAt)
                .Take(5)
                .Select(b => new RecentBatchActivityDto
                {
                    Id = b.Id,
                    ProductName = b.MedicineProduct != null ? b.MedicineProduct.ProductName : null,
                    BatchNumber = b.BatchNumber,
                    FactoryName = b.Factory != null ? b.Factory.OfficialFactoryName : null,
                    BatchStatus = b.BatchStatus.ToString(),
                    SupplyChainStage = b.SupplyChainStage.ToString(),
                    LastUpdated = b.UpdatedAt
                }).ToListAsync()
            : new List<RecentBatchActivityDto>();

        return Ok(new OverviewDto
        {
            Cards = cards,
            RecentRegistrationRequests = recentRequests,
            RecentAlerts = recentAlerts,
            RecentBatchActivity = recentBatches
        });
    }

    // Only MinistryAdmin/MinistryViewer accounts created with an EntityScope are restricted here.
    // SuperAdmin always sees the full, unscoped dashboard.
    private string? GetMinistryEntityScope()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "MinistryAdmin" && role != "MinistryViewer") return null;

        var scope = User.FindFirst("entityType")?.Value;
        if (string.IsNullOrEmpty(scope) || scope == "Ministry") return null;
        return scope;
    }
}
