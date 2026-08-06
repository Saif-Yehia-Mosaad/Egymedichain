using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

// Backend Action Report §4. Async job model as specified, but implemented with Task.Run +
// IServiceScopeFactory instead of an external queue/worker (Hangfire etc.) - none is wired up
// in this project yet, and this is enough for Ministry-scale report volumes. Only CSV is
// implemented for now; Pdf/Xlsx need a rendering library this project doesn't have yet
// (QuestPDF / ClosedXML) - requesting them returns 400 rather than faking the format.
[ApiController]
[Route("api/reports")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class ReportsController : ControllerBase
{
    private static readonly string[] ValidReportTypes =
        { "BatchTraceability", "RecallSummary", "InventorySnapshot", "AuditTrailExport", "StaffDirectory" };

    private readonly AppDbContext _db;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportsController(AppDbContext db, IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _scopeFactory = scopeFactory;
    }

    private static string ReportsFolder => Path.Combine(Directory.GetCurrentDirectory(), "GeneratedReports");

    [HttpPost("generate")]
    public async Task<ActionResult<ReportJobDto>> Generate([FromBody] GenerateReportRequestDto? dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ReportType) || !ValidReportTypes.Contains(dto.ReportType))
            return BadRequest(new { message = $"reportType must be one of: {string.Join(", ", ValidReportTypes)}." });

        if (!string.Equals(dto.Format, "Csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only the Csv format is currently implemented. Pdf/Xlsx need a rendering library this deployment doesn't have yet." });

        if (dto.DateFrom != null && dto.DateTo != null && dto.DateFrom > dto.DateTo)
            return BadRequest(new { message = "dateFrom must be before dateTo." });
        if (dto.DateFrom != null && dto.DateTo != null && (dto.DateTo.Value - dto.DateFrom.Value).TotalDays > 730)
            return BadRequest(new { message = "Date range can't exceed 2 years." });

        var userId = int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var uid) ? uid : (int?)null;

        var job = new ReportJob
        {
            Id = Guid.NewGuid(),
            RequestedByUserId = userId,
            ReportType = dto.ReportType,
            Format = "Csv",
            ParametersJson = System.Text.Json.JsonSerializer.Serialize(dto),
            Status = "Queued",
            RequestedAt = DateTime.UtcNow
        };
        _db.ReportJobs.Add(job);
        await _db.SaveChangesAsync();

        // Fire-and-forget on a new DI scope (the request's own AppDbContext/scope will be
        // disposed as soon as this action returns 202, so the background work needs its own).
        _ = Task.Run(() => RunJobAsync(job.Id));

        return AcceptedAtAction(nameof(GetStatus), new { jobId = job.Id },
            new ReportJobDto { JobId = job.Id, Status = job.Status, RequestedAt = job.RequestedAt });
    }

    [HttpGet("{jobId:guid}/status")]
    public async Task<ActionResult<ReportJobDto>> GetStatus(Guid jobId)
    {
        var job = await _db.ReportJobs.FindAsync(jobId);
        if (job == null) return NotFound(new { message = "Report job not found." });
        if (!IsOwnerOrSuperAdmin(job)) return StatusCode(403, new { message = "You didn't request this report." });

        return Ok(new ReportJobDto
        {
            JobId = job.Id,
            Status = job.Status,
            RequestedAt = job.RequestedAt,
            CompletedAt = job.CompletedAt,
            ErrorMessage = job.ErrorMessage
        });
    }

    [HttpGet("{jobId:guid}/download")]
    public async Task<IActionResult> Download(Guid jobId)
    {
        var job = await _db.ReportJobs.FindAsync(jobId);
        if (job == null) return NotFound(new { message = "Report job not found." });
        if (!IsOwnerOrSuperAdmin(job)) return StatusCode(403, new { message = "You didn't request this report." });

        if (job.Status != "Completed" || string.IsNullOrEmpty(job.FilePath))
            return Conflict(new { message = $"Report is not ready yet (status: {job.Status}). Keep polling /status." });
        if (!System.IO.File.Exists(job.FilePath))
            return NotFound(new { message = "Report file is no longer available (it may have expired)." });

        var bytes = await System.IO.File.ReadAllBytesAsync(job.FilePath);
        return File(bytes, "text/csv", $"{job.ReportType}-{job.Id:N}.csv");
    }

    private bool IsOwnerOrSuperAdmin(ReportJob job)
    {
        var userId = int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var uid) ? uid : (int?)null;
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        return isSuperAdmin || (userId != null && userId == job.RequestedByUserId);
    }

    // Runs on its own DI scope/DbContext instance - the HTTP request that queued this job has
    // already returned a 202 by the time this executes.
    private async Task RunJobAsync(Guid jobId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var job = await db.ReportJobs.FindAsync(jobId);
        if (job == null) return;

        try
        {
            job.Status = "Processing";
            await db.SaveChangesAsync();

            var dto = System.Text.Json.JsonSerializer.Deserialize<GenerateReportRequestDto>(job.ParametersJson ?? "{}");
            var csv = await BuildCsvAsync(db, job.ReportType!, dto);

            Directory.CreateDirectory(ReportsFolder);
            var path = Path.Combine(ReportsFolder, $"{job.Id:N}.csv");
            await System.IO.File.WriteAllTextAsync(path, csv, Encoding.UTF8);

            job.FilePath = path;
            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
        }
        await db.SaveChangesAsync();
    }

    private static async Task<string> BuildCsvAsync(AppDbContext db, string reportType, GenerateReportRequestDto? dto)
    {
        var sb = new StringBuilder();
        switch (reportType)
        {
            case "StaffDirectory":
                sb.AppendLine("Id,FullName,OfficialEmail,Role,Department,Facility,Status,HireDate");
                var staff = await db.SystemUsers.Where(u => !u.IsDeleted).ToListAsync();
                foreach (var u in staff)
                    sb.AppendLine(Csv(u.Id, u.FullName, u.OfficialEmail ?? u.Email, u.Role, u.Department, u.Facility,
                        u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"), u.HireDate));
                break;

            case "AuditTrailExport":
                var logsQuery = db.AuditLogs.AsQueryable();
                if (dto?.DateFrom != null) logsQuery = logsQuery.Where(l => l.CreatedAt >= dto.DateFrom);
                if (dto?.DateTo != null) logsQuery = logsQuery.Where(l => l.CreatedAt <= dto.DateTo);
                sb.AppendLine("LogCode,User,Action,ResourceType,ResourceId,Result,CreatedAt");
                foreach (var l in await logsQuery.OrderByDescending(l => l.CreatedAt).Take(50000).ToListAsync())
                    sb.AppendLine(Csv(l.LogCode, l.UserDisplayName, l.Action, l.ResourceType, l.ResourceId, l.Result, l.CreatedAt));
                break;

            case "RecallSummary":
                var alertsQuery = db.Alerts.Include(a => a.Batch).Where(a => a.AlertType == Domain.Enums.AlertType.Recall);
                if (dto?.DateFrom != null) alertsQuery = alertsQuery.Where(a => a.CreatedAt >= dto.DateFrom);
                if (dto?.DateTo != null) alertsQuery = alertsQuery.Where(a => a.CreatedAt <= dto.DateTo);
                sb.AppendLine("AlertCode,BatchNumber,EntityName,Severity,Status,Message,CreatedAt");
                foreach (var a in await alertsQuery.OrderByDescending(a => a.CreatedAt).ToListAsync())
                    sb.AppendLine(Csv(a.AlertCode, a.Batch?.BatchNumber, a.EntityName, a.Severity, a.AlertStatus, a.Message, a.CreatedAt));
                break;

            case "InventorySnapshot":
                var invQuery = db.InventoryStocks.Include(i => i.Batch).AsQueryable();
                if (dto?.EntityId != null && dto.EntityType == "Warehouse") invQuery = invQuery.Where(i => i.WarehouseId == dto.EntityId);
                if (dto?.EntityId != null && dto.EntityType == "Pharmacy") invQuery = invQuery.Where(i => i.PharmacyId == dto.EntityId);
                sb.AppendLine("BatchNumber,Location,AvailableQuantity,QuarantinedQuantity,Status");
                foreach (var i in await invQuery.Take(50000).ToListAsync())
                    sb.AppendLine(Csv(i.Batch?.BatchNumber, i.HolderName, i.AvailableQuantity, i.QuarantinedQuantity, i.InventoryStatus));
                break;

            case "BatchTraceability":
                var batchQuery = db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).AsQueryable();
                if (dto?.DateFrom != null) batchQuery = batchQuery.Where(b => b.ManufacturingDate >= dto.DateFrom);
                if (dto?.DateTo != null) batchQuery = batchQuery.Where(b => b.ManufacturingDate <= dto.DateTo);
                if (dto?.EntityId != null && dto.EntityType == "Factory") batchQuery = batchQuery.Where(b => b.FactoryId == dto.EntityId);
                sb.AppendLine("BatchNumber,ProductName,Factory,ManufacturingDate,ExpiryDate,Status,SupplyChainStage,CurrentLocation");
                foreach (var b in await batchQuery.Take(50000).ToListAsync())
                    sb.AppendLine(Csv(b.BatchNumber, b.MedicineProduct?.ProductName, b.Factory?.OfficialFactoryName,
                        b.ManufacturingDate, b.ExpiryDate, b.BatchStatus, b.SupplyChainStage, b.CurrentLocation));
                break;
        }
        return sb.ToString();
    }

    // Minimal CSV field escaping (wraps in quotes, doubles embedded quotes) - good enough for
    // the plain identifiers/dates/enums this report deals with.
    private static string Csv(params object?[] fields) =>
        string.Join(",", fields.Select(f =>
        {
            var s = f?.ToString() ?? "";
            return s.Contains(',') || s.Contains('"') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
        }));
}
