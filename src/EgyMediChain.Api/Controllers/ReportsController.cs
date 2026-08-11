using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ClosedXML.Excel;
using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EgyMediChain.Api.Controllers;

// Backend Action Report §4. Async job model as specified, implemented with Task.Run +
// IServiceScopeFactory instead of an external queue/worker (Hangfire etc.) - none is wired up
// in this project, and this is enough for Ministry-scale report volumes. All three formats
// (Csv/Pdf/Xlsx) are implemented: Csv is plain text, Pdf via QuestPDF, Xlsx via ClosedXML.
[ApiController]
[Route("api/reports")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class ReportsController : ControllerBase
{
    private static readonly string[] ValidReportTypes =
        { "BatchTraceability", "RecallSummary", "InventorySnapshot", "AuditTrailExport", "StaffDirectory" };
    private static readonly string[] ValidFormats = { "Csv", "Pdf", "Xlsx" };

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

        var format = string.IsNullOrWhiteSpace(dto.Format) ? "Csv" : dto.Format;
        if (!ValidFormats.Contains(format, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = $"format must be one of: {string.Join(", ", ValidFormats)}." });
        format = ValidFormats.First(f => string.Equals(f, format, StringComparison.OrdinalIgnoreCase));

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
            Format = format,
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
        var (contentType, ext) = job.Format switch
        {
            "Pdf" => ("application/pdf", "pdf"),
            "Xlsx" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
            _ => ("text/csv", "csv")
        };
        return File(bytes, contentType, $"{job.ReportType}-{job.Id:N}.{ext}");
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
            var (headers, rows) = await BuildReportDataAsync(db, job.ReportType!, dto);

            byte[] bytes = job.Format switch
            {
                "Pdf" => RenderPdf(job.ReportType!, headers, rows),
                "Xlsx" => RenderXlsx(job.ReportType!, headers, rows),
                _ => Encoding.UTF8.GetBytes(RenderCsv(headers, rows))
            };

            Directory.CreateDirectory(ReportsFolder);
            var ext = job.Format switch { "Pdf" => "pdf", "Xlsx" => "xlsx", _ => "csv" };
            var path = Path.Combine(ReportsFolder, $"{job.Id:N}.{ext}");
            await System.IO.File.WriteAllBytesAsync(path, bytes);

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

    // Single source of truth for report content - each format renderer below just formats the
    // same (headers, rows) shape differently, so a new ReportType only needs one switch case.
    private static async Task<(string[] Headers, List<object?[]> Rows)> BuildReportDataAsync(
        AppDbContext db, string reportType, GenerateReportRequestDto? dto)
    {
        var rows = new List<object?[]>();
        string[] headers;

        switch (reportType)
        {
            case "StaffDirectory":
                headers = new[] { "Id", "FullName", "OfficialEmail", "Role", "Department", "Facility", "Status", "HireDate" };
                foreach (var u in await db.SystemUsers.Where(u => !u.IsDeleted).ToListAsync())
                    rows.Add(new object?[] { u.Id, u.FullName, u.OfficialEmail ?? u.Email, u.Role, u.Department, u.Facility,
                        u.IsSuspended == true ? "Suspended" : (u.IsActive == true ? "Active" : "Inactive"), u.HireDate });
                break;

            case "AuditTrailExport":
                headers = new[] { "LogCode", "User", "Action", "ResourceType", "ResourceId", "Result", "CreatedAt" };
                var logsQuery = db.AuditLogs.AsQueryable();
                if (dto?.DateFrom != null) logsQuery = logsQuery.Where(l => l.CreatedAt >= dto.DateFrom);
                if (dto?.DateTo != null) logsQuery = logsQuery.Where(l => l.CreatedAt <= dto.DateTo);
                foreach (var l in await logsQuery.OrderByDescending(l => l.CreatedAt).Take(50000).ToListAsync())
                    rows.Add(new object?[] { l.LogCode, l.UserDisplayName, l.Action, l.ResourceType, l.ResourceId, l.Result, l.CreatedAt });
                break;

            case "RecallSummary":
                headers = new[] { "AlertCode", "BatchNumber", "EntityName", "Severity", "Status", "Message", "CreatedAt" };
                var alertsQuery = db.Alerts.Include(a => a.Batch).Where(a => a.AlertType == Domain.Enums.AlertType.Recall);
                if (dto?.DateFrom != null) alertsQuery = alertsQuery.Where(a => a.CreatedAt >= dto.DateFrom);
                if (dto?.DateTo != null) alertsQuery = alertsQuery.Where(a => a.CreatedAt <= dto.DateTo);
                foreach (var a in await alertsQuery.OrderByDescending(a => a.CreatedAt).ToListAsync())
                    rows.Add(new object?[] { a.AlertCode, a.Batch?.BatchNumber, a.EntityName, a.Severity, a.AlertStatus, a.Message, a.CreatedAt });
                break;

            case "InventorySnapshot":
                headers = new[] { "BatchNumber", "Location", "AvailableQuantity", "QuarantinedQuantity", "Status" };
                var invQuery = db.InventoryStocks.Include(i => i.Batch).AsQueryable();
                if (dto?.EntityId != null && dto.EntityType == "Warehouse") invQuery = invQuery.Where(i => i.WarehouseId == dto.EntityId);
                if (dto?.EntityId != null && dto.EntityType == "Pharmacy") invQuery = invQuery.Where(i => i.PharmacyId == dto.EntityId);
                foreach (var i in await invQuery.Take(50000).ToListAsync())
                    rows.Add(new object?[] { i.Batch?.BatchNumber, i.HolderName, i.AvailableQuantity, i.QuarantinedQuantity, i.InventoryStatus });
                break;

            case "BatchTraceability":
                headers = new[] { "BatchNumber", "ProductName", "Factory", "ManufacturingDate", "ExpiryDate", "Status", "SupplyChainStage", "CurrentLocation" };
                var batchQuery = db.Batches.Include(b => b.MedicineProduct).Include(b => b.Factory).AsQueryable();
                if (dto?.DateFrom != null) batchQuery = batchQuery.Where(b => b.ManufacturingDate >= dto.DateFrom);
                if (dto?.DateTo != null) batchQuery = batchQuery.Where(b => b.ManufacturingDate <= dto.DateTo);
                if (dto?.EntityId != null && dto.EntityType == "Factory") batchQuery = batchQuery.Where(b => b.FactoryId == dto.EntityId);
                foreach (var b in await batchQuery.Take(50000).ToListAsync())
                    rows.Add(new object?[] { b.BatchNumber, b.MedicineProduct?.ProductName, b.Factory?.OfficialFactoryName,
                        b.ManufacturingDate, b.ExpiryDate, b.BatchStatus, b.SupplyChainStage, b.CurrentLocation });
                break;

            default:
                headers = Array.Empty<string>();
                break;
        }

        return (headers, rows);
    }

    private static string Cell(object? v) => v switch
    {
        null => "",
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
        _ => v.ToString() ?? ""
    };

    // ---- CSV ----
    private static string RenderCsv(string[] headers, List<object?[]> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(CsvEscape)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(f => CsvEscape(Cell(f)))));
        return sb.ToString();
    }

    private static string CsvEscape(string s) =>
        s.Contains(',') || s.Contains('"') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;

    // ---- PDF (QuestPDF) ----
    private static byte[] RenderPdf(string reportType, string[] headers, List<object?[]> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Text($"EgyMediChain — {reportType}").FontSize(16).Bold();
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated ");
                    x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + " UTC");
                    x.Span(" — Page ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in headers) columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var h in headers)
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(h).Bold();
                    });

                    // Cap rows in the PDF - a 50,000-row PDF isn't a usable document; the Csv/Xlsx
                    // formats are the right choice for bulk exports.
                    foreach (var row in rows.Take(2000))
                        foreach (var f in row)
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(Cell(f));
                });

                if (rows.Count > 2000)
                    page.Content().Column(c => c.Item().PaddingTop(6).Text($"Showing first 2,000 of {rows.Count} rows. Use Csv or Xlsx format for the full dataset.").Italic().FontColor(Colors.Grey.Darken1));
            });
        });

        return document.GeneratePdf();
    }

    // ---- Xlsx (ClosedXML) ----
    private static byte[] RenderXlsx(string reportType, string[] headers, List<object?[]> rows)
    {
        using var workbook = new XLWorkbook();
        var sheetName = reportType.Length > 31 ? reportType[..31] : reportType; // Excel sheet name limit
        var sheet = workbook.Worksheets.Add(sheetName);

        for (var c = 0; c < headers.Length; c++)
        {
            sheet.Cell(1, c + 1).Value = headers[c];
            sheet.Cell(1, c + 1).Style.Font.Bold = true;
            sheet.Cell(1, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E5E7EB");
        }

        for (var r = 0; r < rows.Count; r++)
            for (var c = 0; c < rows[r].Length; c++)
                sheet.Cell(r + 2, c + 1).Value = Cell(rows[r][c]);

        if (headers.Length > 0) sheet.Range(1, 1, 1, headers.Length).SetAutoFilter();
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
