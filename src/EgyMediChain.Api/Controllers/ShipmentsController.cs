using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// National-level shipments view. FactoryDashboardController and Warehouse/PharmacyDashboardController
// already have their own /shipments endpoints, but those are scoped to one entity - this is the
// Ministry's system-wide summary (what the National Dashboard needs), which didn't exist before.
[ApiController]
[Route("api/shipments")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class ShipmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ShipmentsController(AppDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<ActionResult<ShipmentsSummaryDto>> GetSummary()
    {
        var scope = GetMinistryEntityScope();
        var query = _db.Shipments.AsQueryable();

        query = scope switch
        {
            "Factory" => query.Where(s => s.SourceFactoryId != null),
            "Warehouse" => query.Where(s => s.SourceWarehouseId != null || s.DestinationWarehouseId != null),
            "Pharmacy" => query.Where(s => s.DestinationPharmacyId != null),
            _ => query
        };

        return Ok(new ShipmentsSummaryDto
        {
            TotalShipments = await query.CountAsync(),
            InTransit = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.InTransit),
            Delivered = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Delivered),
            PartiallyReceived = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.PartiallyReceived),
            Rejected = await query.CountAsync(s => s.ShipmentStatus == ShipmentStatus.Rejected),
            FactoryToWarehouse = await query.CountAsync(s => s.ShipmentType == EgyMediChain.Domain.Enums.ShipmentType.FactoryToWarehouse),
            WarehouseToPharmacy = await query.CountAsync(s => s.ShipmentType == EgyMediChain.Domain.Enums.ShipmentType.WarehouseToPharmacy)
        });
    }

    private string? GetMinistryEntityScope()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role != "MinistryAdmin" && role != "MinistryViewer") return null;

        var scope = User.FindFirst("entityType")?.Value;
        if (string.IsNullOrEmpty(scope) || scope == "Ministry") return null;
        return scope;
    }
}