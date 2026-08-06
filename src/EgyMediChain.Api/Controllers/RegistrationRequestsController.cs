using EgyMediChain.Api.Dtos;
using EgyMediChain.Domain.Entities;
using EgyMediChain.Domain.Enums;
using EgyMediChain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgyMediChain.Api.Controllers;

[ApiController]
[Route("api/registration-requests")]
[Authorize(Roles = "SuperAdmin,MinistryAdmin,MinistryViewer")]
public class RegistrationRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    public RegistrationRequestsController(AppDbContext db) => _db = db;

    // Public wizard submission (Pharmacy/Warehouse/Factory registration) - no auth required.
    // This is the endpoint the "Submit Request" button on the registration wizard should call.
    [AllowAnonymous]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<object>> Submit(
        [FromForm] SubmitRegistrationRequestDto dto,
        [FromForm] List<IFormFile>? documents,
        [FromForm] List<string>? documentTypes)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.EntityType))
            return BadRequest(new { message = "Missing required fields." });

        if (!dto.ConfirmInfoCorrect || !dto.AgreeToInspection)
            return BadRequest(new { message = "You must accept both declarations before submitting." });

        if (await _db.SystemUsers.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "An account with this email already exists." });

        EntityKind? entityKind = dto.EntityType switch
        {
            "Factory" => EntityKind.Factory,
            "Warehouse" => EntityKind.Warehouse,
            "Pharmacy" => EntityKind.Pharmacy,
            _ => null
        };
        if (entityKind == null) return BadRequest(new { message = "Invalid entity type." });

        var user = new SystemUser
        {
            FullName = dto.RepresentativeName,
            Email = dto.Email,
            MobileNumber = dto.MobileNumber,
            NationalId = dto.NationalId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                string.IsNullOrWhiteSpace(dto.Password) ? Guid.NewGuid().ToString("N") : dto.Password),
            Role = entityKind switch
            {
                EntityKind.Factory => SystemRole.FactoryUser,
                EntityKind.Warehouse => SystemRole.WarehouseUser,
                _ => SystemRole.PharmacyUser
            },
            EntityType = entityKind,
            EmailConfirmed = false,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.SystemUsers.Add(user);

        Factory? factory = null; Warehouse? warehouse = null; Pharmacy? pharmacy = null;
        string? entityName = null;

        if (entityKind == EntityKind.Factory)
        {
            factory = new Factory
            {
                OfficialFactoryName = dto.OfficialFactoryName,
                LegalCompanyName = dto.LegalCompanyName,
                DosageFormsProduced = dto.DosageFormsProduced,
                Governorate = dto.Governorate,
                City = dto.City,
                DistrictArea = dto.DistrictArea,
                FullAddress = dto.FullAddress,
                FactoryLicenseNumber = dto.FactoryLicenseNumber,
                TechnicalOperatingLicenseNumber = dto.TechnicalOperatingLicenseNumber,
                CommercialRegistrationNumber = dto.CommercialRegistrationNumber,
                TaxCardNumber = dto.TaxCardNumber,
                LicenseIssueDate = dto.LicenseIssueDate,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                HasQualityControlLab = dto.HasQualityControlLab,
                HasFinishedGoodsStore = dto.HasFinishedGoodsStore,
                HasColdStorage = dto.HasColdStorage,
                HasQuarantineArea = dto.HasQuarantineArea,
                FactoryStatus = FacilityStatus.PendingReview,
                CreatedAt = DateTime.UtcNow
            };
            _db.Factories.Add(factory);
            entityName = factory.OfficialFactoryName;
        }
        else if (entityKind == EntityKind.Warehouse)
        {
            warehouse = new Warehouse
            {
                OfficialWarehouseName = dto.OfficialWarehouseName,
                WarehouseType = dto.WarehouseType,
                Governorate = dto.Governorate,
                City = dto.City,
                DistrictArea = dto.DistrictArea,
                FullAddress = dto.FullAddress,
                WarehouseLicenseNumber = dto.WarehouseLicenseNumber,
                LicenseIssueDate = dto.LicenseIssueDate,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                HasColdStorage = dto.HasColdStorage,
                HasQuarantineArea = dto.HasQuarantineArea,
                HasDeliveryService = dto.HasDeliveryService,
                WarehouseStatus = FacilityStatus.PendingReview,
                CreatedAt = DateTime.UtcNow
            };
            _db.Warehouses.Add(warehouse);
            entityName = warehouse.OfficialWarehouseName;
        }
        else
        {
            pharmacy = new Pharmacy
            {
                OfficialPharmacyName = dto.OfficialPharmacyName,
                PharmacyType = dto.PharmacyType,
                Governorate = dto.Governorate,
                City = dto.City,
                DistrictArea = dto.DistrictArea,
                FullAddress = dto.FullAddress,
                DefaultWarehouseId = dto.DefaultWarehouseId,
                HasColdStorage = dto.HasColdStorage,
                PharmacyLicenseNumber = dto.PharmacyLicenseNumber,
                LicenseIssueDate = dto.LicenseIssueDate,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                PharmacistSyndicateId = dto.PharmacistSyndicateId,
                PharmacyStatus = FacilityStatus.PendingReview,
                CreatedAt = DateTime.UtcNow
            };
            _db.Pharmacies.Add(pharmacy);
            entityName = pharmacy.OfficialPharmacyName;
        }

        await _db.SaveChangesAsync(); // need generated Ids before linking

        var requestCode = $"REQ-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1000, 9999)}";

        var request = new RegistrationRequest
        {
            RequestCode = requestCode,
            EntityType = entityKind,
            EntityName = entityName,
            RepresentativeName = dto.RepresentativeName,
            Email = dto.Email,
            SystemUserId = user.Id,
            FactoryId = factory?.Id,
            WarehouseId = warehouse?.Id,
            PharmacyId = pharmacy?.Id,
            SubmittedAt = DateTime.UtcNow,
            EmailConfirmed = false,
            RegistrationStatus = RegistrationStatus.Pending,
            DocumentsOverallStatus = DocumentStatus.UnderReview
        };
        _db.RegistrationRequests.Add(request);
        await _db.SaveChangesAsync();

        if (documents != null && documents.Count > 0)
        {
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", requestCode);
            Directory.CreateDirectory(uploadsRoot);

            for (int i = 0; i < documents.Count; i++)
            {
                var file = documents[i];
                if (file.Length == 0) continue;

                var docType = documentTypes != null && i < documentTypes.Count ? documentTypes[i] : "Document";
                var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
                var fullPath = Path.Combine(uploadsRoot, safeName);

                await using (var stream = new FileStream(fullPath, FileMode.Create))
                    await file.CopyToAsync(stream);

                _db.EntityDocuments.Add(new EntityDocument
                {
                    RegistrationRequestId = request.Id,
                    DocumentType = docType,
                    FileName = file.FileName,
                    FileUrl = $"/uploads/{requestCode}/{safeName}",
                    UploadedAt = DateTime.UtcNow,
                    DocumentStatus = DocumentStatus.UnderReview
                });
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Registration request submitted successfully.", requestCode, requestId = request.Id });
    }

    // status: pending | under-review | needs-more-documents | approved | rejected | cancelled | (empty = all)
    // entityType: Factory | Warehouse | Pharmacy (empty = all, subject to this account's Ministry scope)
    [HttpGet]
    public async Task<ActionResult<PagedResult<RegistrationRequestListItemDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? entityType, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.RegistrationRequests.AsQueryable();

        var scope = GetCallerEntityScope();
        if (scope != null)
            query = query.Where(r => r.EntityType != null && r.EntityType.ToString() == scope);

        // Explicit filter from the frontend's dropdown - only narrows further within whatever
        // this account's Ministry scope already allows (can't be used to bypass the scope above).
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(r => r.EntityType != null && r.EntityType.ToString() == entityType);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Replace("-", "").ToLower();
            query = query.Where(r => r.RegistrationStatus != null &&
                r.RegistrationStatus.ToString()!.ToLower() == normalized);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                (r.EntityName != null && r.EntityName.Contains(search)) ||
                (r.Email != null && r.Email.Contains(search)) ||
                (r.RequestCode != null && r.RequestCode.Contains(search)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.SubmittedAt)
            .Skip(Math.Max(0, (page - 1) * pageSize))
            .Take(pageSize <= 0 ? 10 : pageSize)
            .Select(r => new RegistrationRequestListItemDto
            {
                Id = r.Id,
                RequestCode = r.RequestCode,
                EntityType = r.EntityType.ToString(),
                EntityName = r.EntityName,
                RepresentativeName = r.RepresentativeName,
                SubmittedBy = r.RepresentativeName,
                Email = r.Email,
                SubmittedAt = r.SubmittedAt,
                EmailConfirmed = r.EmailConfirmed,
                DocumentsOverallStatus = r.DocumentsOverallStatus.ToString(),
                RegistrationStatus = r.RegistrationStatus.ToString()
            }).ToListAsync();

        return Ok(new PagedResult<RegistrationRequestListItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
    }

    // Unified lookup, works for all 3 entity types (EntitiesManagement-Backend-Gaps.md, item 1).
    // Replaces the frontend's text-search-then-guess-the-first-result workaround, which was
    // unreliable for Factories and simply didn't exist for Warehouses/Pharmacies.
    [HttpGet("by-entity/{entityType}/{entityId:int}")]
    public async Task<ActionResult<EntityRegistrationRequestRefDto>> GetByEntity(string entityType, int entityId)
    {
        if (!Enum.TryParse<EntityKind>(entityType, true, out var kind))
            return BadRequest(new { message = "entityType must be Factory, Warehouse or Pharmacy." });
        if (!IsAllowedEntityType(kind)) return Forbid403();

        var query = _db.RegistrationRequests.AsQueryable();
        query = kind switch
        {
            EntityKind.Factory => query.Where(r => r.FactoryId == entityId),
            EntityKind.Warehouse => query.Where(r => r.WarehouseId == entityId),
            EntityKind.Pharmacy => query.Where(r => r.PharmacyId == entityId),
            _ => query.Where(r => false)
        };

        var r = await query.OrderByDescending(x => x.SubmittedAt).FirstOrDefaultAsync();
        if (r == null) return NotFound(new { message = "No registration request found for this entity." });

        return Ok(new EntityRegistrationRequestRefDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            SubmittedAt = r.SubmittedAt
        });
    }

    [HttpGet("counts")]
    public async Task<ActionResult<object>> GetCounts()
    {
        var scope = GetCallerEntityScope();
        var baseQuery = _db.RegistrationRequests.AsQueryable();
        if (scope != null)
            baseQuery = baseQuery.Where(r => r.EntityType != null && r.EntityType.ToString() == scope);

        return Ok(new
        {
            PendingReview = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.Pending || r.RegistrationStatus == RegistrationStatus.UnderReview),
            NeedsMoreDocuments = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.NeedsMoreDocuments),
            Approved = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.Approved),
            Rejected = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.Rejected),
            Cancelled = await baseQuery.CountAsync(r => r.RegistrationStatus == RegistrationStatus.Cancelled)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RegistrationRequestDetailsDto>> GetById(int id)
    {
        var r = await _db.RegistrationRequests
            .Include(x => x.Factory)
            .Include(x => x.Warehouse)
            .Include(x => x.Pharmacy).ThenInclude(p => p!.DefaultWarehouse)
            .Include(x => x.SystemUser)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        var entity = new EntityInfoDto();
        if (r.Factory != null)
        {
            var f = r.Factory;
            entity.OfficialFactoryName = f.OfficialFactoryName;
            entity.LegalCompanyName = f.LegalCompanyName;
            entity.DosageFormsProduced = f.DosageFormsProduced;
            entity.Governorate = f.Governorate; entity.City = f.City; entity.DistrictArea = f.DistrictArea; entity.FullAddress = f.FullAddress;
            entity.FactoryLicenseNumber = f.FactoryLicenseNumber;
            entity.TechnicalOperatingLicenseNumber = f.TechnicalOperatingLicenseNumber;
            entity.CommercialRegistrationNumber = f.CommercialRegistrationNumber;
            entity.TaxCardNumber = f.TaxCardNumber;
            entity.LicenseIssueDate = f.LicenseIssueDate; entity.LicenseExpiryDate = f.LicenseExpiryDate;
            entity.HasQualityControlLab = f.HasQualityControlLab; entity.HasFinishedGoodsStore = f.HasFinishedGoodsStore;
            entity.HasColdStorage = f.HasColdStorage; entity.HasQuarantineArea = f.HasQuarantineArea;
            entity.Status = f.FactoryStatus?.ToString();
        }
        else if (r.Warehouse != null)
        {
            var w = r.Warehouse;
            entity.OfficialWarehouseName = w.OfficialWarehouseName;
            entity.WarehouseType = w.WarehouseType;
            entity.Governorate = w.Governorate; entity.City = w.City; entity.DistrictArea = w.DistrictArea; entity.FullAddress = w.FullAddress;
            entity.WarehouseLicenseNumber = w.WarehouseLicenseNumber;
            entity.LicenseIssueDate = w.LicenseIssueDate; entity.LicenseExpiryDate = w.LicenseExpiryDate;
            entity.HasColdStorage = w.HasColdStorage; entity.HasQuarantineArea = w.HasQuarantineArea; entity.HasDeliveryService = w.HasDeliveryService;
            entity.Status = w.WarehouseStatus?.ToString();
        }
        else if (r.Pharmacy != null)
        {
            var p = r.Pharmacy;
            entity.OfficialPharmacyName = p.OfficialPharmacyName;
            entity.PharmacyType = p.PharmacyType;
            entity.Governorate = p.Governorate; entity.City = p.City; entity.DistrictArea = p.DistrictArea; entity.FullAddress = p.FullAddress;
            entity.DefaultWarehouseName = p.DefaultWarehouse?.OfficialWarehouseName;
            entity.HasColdStorage = p.HasColdStorage;
            entity.PharmacyLicenseNumber = p.PharmacyLicenseNumber;
            entity.LicenseIssueDate = p.LicenseIssueDate; entity.LicenseExpiryDate = p.LicenseExpiryDate;
            entity.PharmacistSyndicateId = p.PharmacistSyndicateId;
            entity.Status = p.PharmacyStatus?.ToString();
        }

        var fields = new List<FieldItemDto>
        {
            new() { Label = "Representative Name", Value = r.RepresentativeName },
            new() { Label = "Email", Value = r.SystemUser?.Email ?? r.Email },
            new() { Label = "Mobile Number", Value = r.SystemUser?.MobileNumber },
            new() { Label = "National ID", Value = MaskNationalId(r.SystemUser?.NationalId) },
            new() { Label = "Governorate", Value = entity.Governorate },
            new() { Label = "City", Value = entity.City },
            new() { Label = "District / Area", Value = entity.DistrictArea },
            new() { Label = "Full Address", Value = entity.FullAddress }
        };

        if (r.Factory != null)
        {
            fields.Add(new() { Label = "Official Factory Name", Value = entity.OfficialFactoryName });
            fields.Add(new() { Label = "Legal Company Name", Value = entity.LegalCompanyName });
            fields.Add(new() { Label = "Dosage Forms Produced", Value = entity.DosageFormsProduced });
            fields.Add(new() { Label = "Factory License Number", Value = entity.FactoryLicenseNumber });
            fields.Add(new() { Label = "Technical Operating License Number", Value = entity.TechnicalOperatingLicenseNumber });
            fields.Add(new() { Label = "Commercial Registration Number", Value = entity.CommercialRegistrationNumber });
            fields.Add(new() { Label = "Tax Card Number", Value = entity.TaxCardNumber });
            fields.Add(new() { Label = "Has Quality Control Lab", Value = entity.HasQualityControlLab?.ToString() });
            fields.Add(new() { Label = "Has Finished Goods Store", Value = entity.HasFinishedGoodsStore?.ToString() });
        }
        else if (r.Warehouse != null)
        {
            fields.Add(new() { Label = "Official Warehouse Name", Value = entity.OfficialWarehouseName });
            fields.Add(new() { Label = "Warehouse Type", Value = entity.WarehouseType });
            fields.Add(new() { Label = "Warehouse License Number", Value = entity.WarehouseLicenseNumber });
            fields.Add(new() { Label = "Has Delivery Service", Value = entity.HasDeliveryService?.ToString() });
        }
        else if (r.Pharmacy != null)
        {
            fields.Add(new() { Label = "Official Pharmacy Name", Value = entity.OfficialPharmacyName });
            fields.Add(new() { Label = "Pharmacy Type", Value = entity.PharmacyType });
            fields.Add(new() { Label = "Default Warehouse", Value = entity.DefaultWarehouseName });
            fields.Add(new() { Label = "Pharmacy License Number", Value = entity.PharmacyLicenseNumber });
            fields.Add(new() { Label = "Pharmacist Syndicate ID", Value = entity.PharmacistSyndicateId });
        }

        fields.Add(new() { Label = "Has Cold Storage", Value = entity.HasColdStorage?.ToString() });
        fields.Add(new() { Label = "Has Quarantine Area", Value = entity.HasQuarantineArea?.ToString() });

        var dto = new RegistrationRequestDetailsDto
        {
            Id = r.Id,
            RequestCode = r.RequestCode,
            EntityType = r.EntityType?.ToString(),
            SubmittedAt = r.SubmittedAt,
            RegistrationStatus = r.RegistrationStatus?.ToString(),
            AdminNotes = r.AdminNotes,
            RejectionReason = r.RejectionReason,
            InspectionScheduledDate = r.InspectionScheduledDate,
            InspectorNotes = r.InspectorNotes,
            Account = r.SystemUser == null ? null : new AccountInfoDto
            {
                FullName = r.SystemUser.FullName,
                Email = r.SystemUser.Email,
                MobileNumber = r.SystemUser.MobileNumber,
                NationalIdMasked = MaskNationalId(r.SystemUser.NationalId),
                EmailConfirmed = r.SystemUser.EmailConfirmed,
                IsActive = r.SystemUser.IsActive
            },
            Entity = entity,
            Documents = r.Documents?.Select(d => new DocumentItemDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                UploadedAt = d.UploadedAt,
                DocumentStatus = d.DocumentStatus?.ToString(),
                ReviewedBy = d.ReviewedBy,
                ReviewedAt = d.ReviewedAt,
                RejectionReason = d.RejectionReason
            }).ToList(),
            Fields = fields
        };

        return Ok(dto);
    }

    // Only for requests that never became a live entity (Pending / NeedsMoreDocuments / Rejected /
    // Cancelled). An Approved request is a real Active Factory/Warehouse/Pharmacy - deleting the
    // *request* on its own would leave an orphaned entity/account, so that case is blocked on purpose.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.RegistrationRequests
            .Include(x => x.Factory).Include(x => x.Warehouse).Include(x => x.Pharmacy)
            .Include(x => x.SystemUser).Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        if (r.RegistrationStatus == RegistrationStatus.Approved)
            return BadRequest(new { message = "An approved registration is a live entity - suspend/deactivate it instead of deleting the request." });

        _db.AuditLogs.Add(new AuditLog
        {
            LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserDisplayName = "Dr. Saif",
            Role = SystemRole.SuperAdmin,
            Action = AuditAction.DeleteRegistrationRequest,
            ResourceType = "RegistrationRequest",
            ResourceId = r.RequestCode,
            OldValue = r.RegistrationStatus?.ToString(),
            NewValue = "Deleted",
            IpAddress = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        });

        if (r.Documents != null) _db.EntityDocuments.RemoveRange(r.Documents);
        _db.RegistrationRequests.Remove(r);

        // Only remove the entity/user rows if they never went Active (i.e. they were never
        // approved through any other path either) - defensive check in case status drifted.
        if (r.Factory != null && r.Factory.FactoryStatus != FacilityStatus.Active) _db.Factories.Remove(r.Factory);
        if (r.Warehouse != null && r.Warehouse.WarehouseStatus != FacilityStatus.Active) _db.Warehouses.Remove(r.Warehouse);
        if (r.Pharmacy != null && r.Pharmacy.PharmacyStatus != FacilityStatus.Active) _db.Pharmacies.Remove(r.Pharmacy);
        if (r.SystemUser != null && r.SystemUser.IsActive != true) _db.SystemUsers.Remove(r.SystemUser);

        await _db.SaveChangesAsync();
        return Ok(new { message = "Registration request deleted." });
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveRequestDto? dto)
    {
        var r = await _db.RegistrationRequests
            .Include(x => x.Factory).Include(x => x.Warehouse).Include(x => x.Pharmacy).Include(x => x.SystemUser)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        r.RegistrationStatus = RegistrationStatus.Approved;
        if (!string.IsNullOrWhiteSpace(dto?.AdminNotes)) r.AdminNotes = dto.AdminNotes;
        if (r.Factory != null) r.Factory.FactoryStatus = FacilityStatus.Active;
        if (r.Warehouse != null) r.Warehouse.WarehouseStatus = FacilityStatus.Active;
        if (r.Pharmacy != null) r.Pharmacy.PharmacyStatus = FacilityStatus.Active;
        if (r.SystemUser != null) r.SystemUser.IsActive = true;

        _db.AuditLogs.Add(NewLog(r.SystemUser, AuditAction.ApproveRegistration, "RegistrationRequest", r.RequestCode, "Pending", "Approved"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Registration request approved.", status = "Approved" });
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectRequestDto? dto)
    {
        var r = await _db.RegistrationRequests
            .Include(x => x.Factory).Include(x => x.Warehouse).Include(x => x.Pharmacy).Include(x => x.SystemUser)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        r.RegistrationStatus = RegistrationStatus.Rejected;
        r.RejectionReason = dto?.RejectionReason ?? "Not specified";
        if (r.Factory != null) r.Factory.FactoryStatus = FacilityStatus.Rejected;
        if (r.Warehouse != null) r.Warehouse.WarehouseStatus = FacilityStatus.Rejected;
        if (r.Pharmacy != null) r.Pharmacy.PharmacyStatus = FacilityStatus.Rejected;
        if (r.SystemUser != null) r.SystemUser.IsActive = false;

        _db.AuditLogs.Add(NewLog(r.SystemUser, AuditAction.RejectRegistration, "RegistrationRequest", r.RequestCode, "Pending", "Rejected"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Registration request rejected.", status = "Rejected" });
    }

    // "request-documents" is an alias of "request-more-documents" - same handler, two URLs,
    // so either naming the frontend already built against will work.
    [HttpPost("{id:int}/request-more-documents")]
    [HttpPost("{id:int}/request-documents")]
    public async Task<IActionResult> RequestMoreDocuments(int id, [FromBody] RequestMoreDocumentsDto? dto)
    {
        var r = await _db.RegistrationRequests.Include(x => x.SystemUser).Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        r.RegistrationStatus = RegistrationStatus.NeedsMoreDocuments;
        r.AdminNotes = dto?.AdminNotes;

        if (dto?.DocumentIdsNeedingReplacement != null && r.Documents != null)
        {
            foreach (var docId in dto.DocumentIdsNeedingReplacement)
            {
                var doc = r.Documents.FirstOrDefault(d => d.Id == docId);
                if (doc != null) doc.DocumentStatus = DocumentStatus.NeedsReplacement;
            }
        }

        _db.AuditLogs.Add(NewLog(r.SystemUser, AuditAction.UpdateDocumentStatus, "EntityDocument", r.RequestCode, "UnderReview", "NeedsReplacement"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "More documents requested.", status = "NeedsMoreDocuments" });
    }

    // Doesn't change RegistrationStatus (an inspection can be scheduled at any stage) - just
    // records when/notes, so the frontend can show "Inspection scheduled for <date>" on top of
    // whatever status the request is already in.
    [HttpPost("{id:int}/request-inspection")]
    public async Task<IActionResult> RequestInspection(int id, [FromBody] RequestInspectionDto? dto)
    {
        var r = await _db.RegistrationRequests.Include(x => x.SystemUser).FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound(new { message = "Registration request not found." });
        if (!IsAllowedEntityType(r.EntityType)) return Forbid403();

        r.InspectionScheduledDate = dto?.ScheduledDate;
        r.InspectorNotes = dto?.InspectorNotes;
        r.InspectionRequestedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(NewLog(r.SystemUser, AuditAction.UpdateDocumentStatus, "RegistrationRequest", r.RequestCode, r.RegistrationStatus?.ToString(), "InspectionRequested"));
        await _db.SaveChangesAsync();
        return Ok(new { message = "Inspection requested.", scheduledDate = r.InspectionScheduledDate });
    }

    [HttpPost("documents/{documentId:int}/status")]
    public async Task<IActionResult> UpdateDocumentStatus(int documentId, [FromBody] DocumentStatusUpdateDto? dto)
    {
        var doc = await _db.EntityDocuments.Include(d => d.RegistrationRequest).FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc == null) return NotFound(new { message = "Document not found." });
        if (!IsAllowedEntityType(doc.RegistrationRequest?.EntityType)) return Forbid403();

        doc.DocumentStatus = (dto?.Status ?? "Complete") switch
        {
            "Complete" => DocumentStatus.Complete,
            "NeedsReplacement" => DocumentStatus.NeedsReplacement,
            "Rejected" => DocumentStatus.Rejected,
            _ => DocumentStatus.UnderReview
        };
        doc.RejectionReason = dto?.RejectionReason;
        doc.ReviewedAt = DateTime.UtcNow;
        doc.ReviewedBy = "Dr. Saif";

        await _db.SaveChangesAsync();
        return Ok(new { message = "Document status updated.", status = doc.DocumentStatus.ToString() });
    }

    private string? GetCallerEntityScope()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (role == "SuperAdmin") return null; // unscoped

        var scope = User.FindFirst("entityType")?.Value;
        if (string.IsNullOrEmpty(scope) || scope == "Ministry") return null; // unscoped Ministry account
        return scope; // "Factory" | "Warehouse" | "Pharmacy"
    }

    private bool IsAllowedEntityType(EntityKind? type)
    {
        var scope = GetCallerEntityScope();
        if (scope == null) return true;
        return type != null && type.ToString() == scope;
    }

    private ObjectResult Forbid403() => new(new { message = "This account's Ministry scope doesn't include this entity type." }) { StatusCode = 403 };

    private static string? MaskNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length < 4) return nationalId;
        return $"**** **** {nationalId[^4..]}";
    }

    private static AuditLog NewLog(SystemUser? user, AuditAction action, string resourceType, string? resourceId, string? oldVal, string? newVal) => new()
    {
        LogCode = $"LOG-{DateTime.UtcNow:yyyyMMddHHmmss}",
        UserId = user?.Id,
        UserDisplayName = "Dr. Saif",
        Role = SystemRole.SuperAdmin,
        Action = action,
        ResourceType = resourceType,
        ResourceId = resourceId,
        OldValue = oldVal,
        NewValue = newVal,
        IpAddress = "127.0.0.1",
        CreatedAt = DateTime.UtcNow
    };
}
