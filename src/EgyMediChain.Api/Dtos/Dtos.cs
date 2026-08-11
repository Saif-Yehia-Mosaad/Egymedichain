using EgyMediChain.Api.Common;
using EgyMediChain.Domain.Enums;

namespace EgyMediChain.Api.Dtos;

// ---------------- Auth ----------------
public class LoginRequestDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class LoginResponseDto
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public int? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
}

public class RefreshRequestDto
{
    public string? RefreshToken { get; set; }
}

// ---------------- Overview ----------------
public class OverviewCardsDto
{
    public int PendingRequests { get; set; }
    public int ActiveFactories { get; set; }
    public int ActiveWarehouses { get; set; }
    public int ActivePharmacies { get; set; }
    // Totals added so the Entities Management screen doesn't need 3 extra "?pageSize=1"
    // requests just to read totalCount (EntitiesManagement-Backend-Gaps.md, item 4).
    public int TotalFactories { get; set; }
    public int TotalWarehouses { get; set; }
    public int TotalPharmacies { get; set; }
    public int ActiveBatches { get; set; }
    public int ShipmentsInTransit { get; set; }
    public int OpenAlerts { get; set; }
    public int SuspiciousPublicScans { get; set; }
}

public class OverviewDto
{
    public OverviewCardsDto? Cards { get; set; }
    public List<RecentRegistrationRequestDto>? RecentRegistrationRequests { get; set; }
    public List<RecentAlertDto>? RecentAlerts { get; set; }
    public List<RecentBatchActivityDto>? RecentBatchActivity { get; set; }
}

public class RecentRegistrationRequestDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    [DocumentedEnum(typeof(RegistrationStatus))]
    public string? RegistrationStatus { get; set; }
}

public class RecentAlertDto
{
    public int Id { get; set; }
    public string? AlertCode { get; set; }
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? AlertStatus { get; set; }
}

public class RecentBatchActivityDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    [DocumentedEnum(typeof(BatchStatus))]
    public string? BatchStatus { get; set; }
    public string? SupplyChainStage { get; set; }
    public DateTime? LastUpdated { get; set; }
}

// ---------------- Registration Requests ----------------
public class RegistrationRequestListItemDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? RepresentativeName { get; set; }
    // Same person/value as RepresentativeName - added because the frontend's "Submitted By"
    // column reads more naturally from a field with this name. Both are always equal.
    public string? SubmittedBy { get; set; }
    public string? Email { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool? EmailConfirmed { get; set; }
    public string? DocumentsOverallStatus { get; set; }
    [DocumentedEnum(typeof(RegistrationStatus))]
    public string? RegistrationStatus { get; set; }
}

// GET /api/registration-requests/counts - typed replacement for the previously untyped `object`
// response (Alerts_Backend_Gaps_Report.md, item 1, mentioned as a shared issue).
public class RegistrationRequestsCountsDto
{
    public int PendingReview { get; set; }
    public int NeedsMoreDocuments { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}

// PUT /api/admin/users/{id} - Edit Staff (Backend Action Report �1.3).
// NationalId and password fields are deliberately excluded - see AdminController for the
// explicit 400 rejection if a client sends them anyway.
public class UpdateMinistryAdminDto
{
    public string? FullName { get; set; }
    public string? MobileNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? OfficialEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Role { get; set; }
    public string? Department { get; set; }
    public string? Facility { get; set; }
    public string? Status { get; set; } // Active | Inactive | Suspended
    public string? Qualification { get; set; }
    public string? JobGrade { get; set; }
    public DateTime? HireDate { get; set; }
    public string? InsuranceNumber { get; set; }

    // Present only so the controller can detect and reject a misuse of this endpoint (�1.5) -
    // these are never applied even if sent.
    public string? NationalId { get; set; }
    public string? Password { get; set; }
    public string? TemporaryPassword { get; set; }
}

public class GenericMessageResponseDto
{
    public string? Message { get; set; }
}

// ---- Forgot Password flow (Backend Action Report �2) ----
public class ForgotPasswordRequestDto
{
    public string? Email { get; set; }
}

public class VerifyResetCodeRequestDto
{
    public string? Email { get; set; }
    public string? Code { get; set; }
}

public class VerifyResetCodeResponseDto
{
    public string? ResetToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ResetPasswordWithTokenDto
{
    public string? ResetToken { get; set; }
    public string? NewPassword { get; set; }
}

// ---- Settings (Backend Action Report �3) ----
public class NotificationPreferencesDto
{
    public bool EmailAlerts { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool CriticalAlerts { get; set; } = true;
    public bool WeeklyReports { get; set; }
}

public class UpdateUserPreferencesDto
{
    public NotificationPreferencesDto? Notifications { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UserProfileSummaryDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}

public class UserPreferencesDto
{
    public NotificationPreferencesDto? Notifications { get; set; }
    public UserProfileSummaryDto? Profile { get; set; }
    public string? AvatarUrl { get; set; }
}

public class SystemConfigDto
{
    public bool TwoFactorRequired { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 60;
}

// ---- Reports (Backend Action Report �4) ----
public class GenerateReportRequestDto
{
    public string? ReportType { get; set; } // BatchTraceability | RecallSummary | InventorySnapshot | AuditTrailExport | StaffDirectory
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Format { get; set; } // only "Csv" is implemented for now - see ReportsController
}

public class ReportJobDto
{
    public Guid JobId { get; set; }
    public string? Status { get; set; }
    public DateTime? RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

// GET /api/shipments/summary (national) - typed replacement for the previously untyped `object`
// response (Remaining Gaps - "no response schema in the Swagger (just 200: OK)").
public class ShipmentsSummaryDto
{
    public int TotalShipments { get; set; }
    public int InTransit { get; set; }
    public int Delivered { get; set; }
    public int PartiallyReceived { get; set; }
    public int Rejected { get; set; }
    public int FactoryToWarehouse { get; set; }
    public int WarehouseToPharmacy { get; set; }
}

// GET /api/warehouse-dashboard/{warehouseId}/shipments/summary - typed replacement (same gap,
// warehouse-scoped variant).
public class WarehouseShipmentsSummaryDto
{
    public int IncomingShipments { get; set; }
    public int PendingInspection { get; set; }
    public int Received { get; set; }
    public int PartiallyReceived { get; set; }
    public int Rejected { get; set; }
}

public class AccountInfoDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalIdMasked { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
}

public class EntityInfoDto
{
    // Common address fields
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? Status { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Factory
    public string? FactoryCode { get; set; }
    public string? OfficialFactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? DosageFormsProduced { get; set; }
    public string? FactoryLicenseNumber { get; set; }
    public string? TechnicalOperatingLicenseNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? TaxCardNumber { get; set; }
    public bool? HasQualityControlLab { get; set; }
    public bool? HasFinishedGoodsStore { get; set; }

    // Warehouse
    public string? WarehouseCode { get; set; }
    public string? OfficialWarehouseName { get; set; }
    public string? WarehouseType { get; set; }
    public string? WarehouseLicenseNumber { get; set; }
    public bool? HasDeliveryService { get; set; }

    // Pharmacy
    public string? PharmacyCode { get; set; }
    public string? OfficialPharmacyName { get; set; }
    public string? PharmacyType { get; set; }
    public string? DefaultWarehouseName { get; set; }
    public string? PharmacyLicenseNumber { get; set; }
    public string? PharmacistSyndicateId { get; set; }

    // Shared
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
}

public class FactoryDetailsDto
{
    public int? EstablishedYear { get; set; }
    public int? TotalProductionLines { get; set; }
    public string? MainProductionTypes { get; set; }
    public bool? ColdChainCapable { get; set; }
    public string? StorageTypes { get; set; }
    public string? QualityCertificates { get; set; }
    public string? Description { get; set; }
}

public class RegistrationInfoDto
{
    public string? RegistrationRequestNo { get; set; }
    public DateTime? SubmittedAt { get; set; }
    [DocumentedEnum(typeof(RegistrationStatus))]
    public string? RegistrationStatus { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class LicenseItemDto
{
    public int Id { get; set; }
    public string? LicenseType { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; }
    public string? FileUrl { get; set; }
}

public class FactoryProfileFullDto
{
    public int Id { get; set; }
    public string? FactoryName { get; set; }
    public string? FactoryCode { get; set; }
    public string? FactoryStatus { get; set; }
    [DocumentedEnum(typeof(RegistrationStatus))]
    public string? RegistrationStatus { get; set; }
    public DateTime? MemberSince { get; set; }
    public AccountInfoDto? Account { get; set; }
    public EntityInfoDto? Entity { get; set; }
    public FactoryDetailsDto? FactoryDetails { get; set; }
    public RegistrationInfoDto? RegistrationInfo { get; set; }
    public List<LicenseItemDto>? Licenses { get; set; }
    public List<DocumentItemDto>? Documents { get; set; }
}

public class DocumentItemDto
{
    public int Id { get; set; }
    public string? DocumentType { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public DateTime? UploadedAt { get; set; }
    public string? DocumentStatus { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class RegistrationRequestDetailsDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public DateTime? SubmittedAt { get; set; }
    [DocumentedEnum(typeof(RegistrationStatus))]
    public string? RegistrationStatus { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? InspectionScheduledDate { get; set; }
    public string? InspectorNotes { get; set; }
    public AccountInfoDto? Account { get; set; }
    public EntityInfoDto? Entity { get; set; }
    public List<DocumentItemDto>? Documents { get; set; }
    // Flat label/value list - built from Account + Entity so the frontend's ReviewRequestModal
    // can just iterate one array instead of knowing every field in EntityInfoDto ahead of time.
    public List<FieldItemDto>? Fields { get; set; }
}

public class FieldItemDto
{
    public string? Label { get; set; }
    public string? Value { get; set; }
}

public class ApproveRequestDto
{
    public string? AdminNotes { get; set; }
}

public class RequestInspectionDto
{
    public DateTime? ScheduledDate { get; set; }
    public string? InspectorNotes { get; set; }
}

// Used by GET /api/factories/{id}/registration-request (and the Warehouse/Pharmacy equivalents),
// and by GET /api/registration-requests/by-entity/{entityType}/{entityId} (same shape, unified lookup -
// see EntitiesManagement-Backend-Gaps.md item 1).
public class EntityRegistrationRequestRefDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    [DocumentedEnum(typeof(RegistrationStatus))]
    public string? RegistrationStatus { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class RejectRequestDto
{
    public string? RejectionReason { get; set; }
}

public class RequestMoreDocumentsDto
{
    public string? AdminNotes { get; set; }
    public List<int>? DocumentIdsNeedingReplacement { get; set; }
}

public class DocumentStatusUpdateDto
{
    public string? Status { get; set; } // Complete / NeedsReplacement / Rejected
    public string? RejectionReason { get; set; }
}

// ---------------- Entities Management ----------------
public class FactoryListItemDto
{
    public int Id { get; set; }
    public string? FactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQualityControlLab { get; set; }
    public string? FactoryStatus { get; set; }
    public int? TotalBatches { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class FactoryProfileDto
{
    public int Id { get; set; }
    public string? OfficialFactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? DosageFormsProduced { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? FactoryLicenseNumber { get; set; }
    public string? TechnicalOperatingLicenseNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? TaxCardNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasQualityControlLab { get; set; }
    public bool? HasFinishedGoodsStore { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public string? FactoryStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WarehouseListItemDto
{
    public int Id { get; set; }
    public string? WarehouseName { get; set; }
    public string? WarehouseType { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public bool? HasDeliveryService { get; set; }
    public string? WarehouseStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class WarehouseProfileDto
{
    public int Id { get; set; }
    public string? OfficialWarehouseName { get; set; }
    public string? WarehouseType { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? WarehouseLicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public bool? HasDeliveryService { get; set; }
    public string? WarehouseStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PharmacyListItemDto
{
    public int Id { get; set; }
    public string? PharmacyName { get; set; }
    public string? PharmacyType { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DefaultWarehouse { get; set; }
    public bool? HasColdStorage { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? PharmacyStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class PharmacyProfileDto
{
    public int Id { get; set; }
    public string? OfficialPharmacyName { get; set; }
    public string? PharmacyType { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? DefaultWarehouse { get; set; }
    public bool? HasColdStorage { get; set; }
    public string? PharmacyLicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? PharmacistSyndicateId { get; set; }
    public string? PharmacyStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class EntityActionDto
{
    public string? Reason { get; set; }
}

// ---------------- Medicine & Batch Monitoring ----------------
public class BatchListItemDto
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    [DocumentedEnum(typeof(BatchStatus))]
    public string? BatchStatus { get; set; }
    public string? SupplyChainStage { get; set; }
    public string? CurrentLocation { get; set; }
    public int? OpenAlerts { get; set; }
    public long? UnitCodesCount { get; set; }
    public bool? AvailableForDispatch { get; set; }
    // Added so dispatch validation (cold-chain check) doesn't need an extra GET .../{id} call
    // per row just to read one boolean (Backend_Action_Report_Factory_Portal, item 1.6).
    public bool? RequiresColdChain { get; set; }
    public DateTime? UpdatedAt { get; set; } // for the "Last Update" column
}

// PUT/PATCH .../batches/{batchId} - only allowed while BatchStatus == Draft (Backend_Action_Report_Factory_Portal, item 1.1)
public class UpdateBatchDto
{
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? BatchNumber { get; set; }
    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

// GET .../batches/{batchId}/unit-codes (Backend_Action_Report_Factory_Portal, item 1.2)
public class UnitCodeListItemDto
{
    public int Id { get; set; }
    public string? GTIN { get; set; }
    public string? SerialNumber { get; set; }
    public string? CodeValueHash { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? UnitStatus { get; set; }
}

// GET .../batches/summary - typed replacement for the previously untyped `object` response
// (MedicineBatchDashboard-Backend-Gaps.md, item 1). Field names match what was already being
// emitted, just now documented in the Swagger schema.
public class BatchesSummaryDto
{
    public int TotalBatches { get; set; }
    public int InProduction { get; set; }
    public int InSupplyChain { get; set; }
    public int InWarehouses { get; set; }
    public int InPharmacies { get; set; }
    public int Quarantined { get; set; }
    public int Recalled { get; set; }
    public int OpenAlerts { get; set; }
}

public class ProductInfoDto
{
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? ProductStatus { get; set; }
}

public class BatchInfoDto
{
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    [DocumentedEnum(typeof(BatchStatus))]
    public string? BatchStatus { get; set; }
    public string? SupplyChainStage { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UnitCodesSummaryDto
{
    public long? TotalUnitCodes { get; set; }
    public long? GeneratedCount { get; set; }
    public long? InWarehouseCount { get; set; }
    public long? InPharmacyCount { get; set; }
    public long? SuspiciousCount { get; set; }
    public long? BlockedCount { get; set; }
    public long? RecalledCount { get; set; }
    public long? ScanCountTotal { get; set; }
}

public class ShipmentSummaryItemDto
{
    public string? TransferCode { get; set; }
    public string? ShipmentType { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public long? ExpectedQuantity { get; set; }
    public long? ReceivedQuantity { get; set; }
    [DocumentedEnum(typeof(ShipmentStatus))]
    public string? ShipmentStatus { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class InventoryDistributionItemDto
{
    public string? HolderType { get; set; }
    public string? HolderName { get; set; }
    public long? TotalReceivedQuantity { get; set; }
    public long? AvailableQuantity { get; set; }
    public long? ReservedQuantity { get; set; }
    public long? QuarantinedQuantity { get; set; }
    [DocumentedEnum(typeof(InventoryStatus))]
    public string? InventoryStatus { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public class RelatedAlertItemDto
{
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    public string? Message { get; set; }
    public string? AlertStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class BatchDetailsDto
{
    public int Id { get; set; }
    public ProductInfoDto? ProductInfo { get; set; }
    public BatchInfoDto? BatchInfo { get; set; }
    public UnitCodesSummaryDto? UnitCodesSummary { get; set; }
    public List<ShipmentSummaryItemDto>? Shipments { get; set; }
    public List<InventoryDistributionItemDto>? InventoryDistribution { get; set; }
    public List<RelatedAlertItemDto>? RelatedAlerts { get; set; }
}

public class CreateRecallAlertDto
{
    public string? Message { get; set; }
}

// ---------------- Alerts & Public Scans ----------------
public class AlertListItemDto
{
    public int Id { get; set; }
    public string? AlertCode { get; set; }
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public string? ShipmentTransferCode { get; set; }
    public string? Message { get; set; }
    public string? AlertStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class AlertDetailsDto
{
    public int Id { get; set; }
    public string? AlertCode { get; set; }
    public string? AlertType { get; set; }
    public string? Severity { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? Message { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public int? BatchId { get; set; }
    public string? ShipmentTransferCode { get; set; }
    public int? ShipmentId { get; set; }
    public string? AlertStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    // "Impact on this Batch" panel - only meaningful for Recall / ComplianceIssue alerts
    public string? ImpactedBatchStatus { get; set; }
    public string? ImpactedUnitCodesStatus { get; set; }
    public string? ImpactedInventoryStatus { get; set; }
    public bool? BatchDispatchBlocked { get; set; }
}

public class UpdateAlertStatusDto
{
    public string? Status { get; set; } // UnderReview / Resolved / Dismissed
}

public class ScanListItemDto
{
    public int Id { get; set; }
    public string? ScanCode { get; set; }
    public string? ScannedGTIN { get; set; }
    public string? ScannedSerialNumber { get; set; }
    public string? ScannedBatchNumber { get; set; }
    public string? ProductName { get; set; }
    public string? VerificationResult { get; set; }
    public string? Reason { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? ScannedAt { get; set; }
}

public class ScanDetailsDto
{
    public int Id { get; set; }
    public string? ScannedGTIN { get; set; }
    public string? ScannedSerialNumber { get; set; }
    public string? ScannedBatchNumber { get; set; }
    public string? VerificationResult { get; set; }
    public string? Reason { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? ScannedAt { get; set; }

    public int? UnitCodeId { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public string? UnitStatus { get; set; }
    public int? ScanCount { get; set; }
    public DateTime? FirstScannedAt { get; set; }
}

public class CreateAlertFromScanDto
{
    public string? Message { get; set; }
    public string? Severity { get; set; }
}

// GET /api/alerts/counts - typed replacement for the previously untyped `object` response
// (Alerts_Backend_Gaps_Report.md, item 1).
public class AlertsCountsDto
{
    public int OpenAlerts { get; set; }
    public int PublicScanLogs { get; set; }
    public int RecallAlerts { get; set; }
}

// GET /api/factory-dashboard/{factoryId}/alerts/counts (Backend_Action_Report_Factory_Portal, item 1.5)
public class FactoryAlertsCountsDto
{
    public int Open { get; set; }
    public int UnderReview { get; set; }
    public int Resolved { get; set; }
    public int Dismissed { get; set; }
    public int Total { get; set; }
}

// POST /api/alerts/bulk-status (Alerts_Backend_Gaps_Report.md, item 7)
public class BulkUpdateAlertStatusDto
{
    public List<int>? AlertIds { get; set; }
    public string? Status { get; set; } // UnderReview / Resolved / Dismissed
}

// ---------------- Admin & Audit ----------------
public class SystemUserListItemDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? Role { get; set; }
    [DocumentedEnum(typeof(EntityKind))]
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // ---- Staff Management fields (staff-management-backend-gaps.md, items 3-4) ----
    public string? OfficialEmail { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Department { get; set; }
    public string? Facility { get; set; }
    // Tri-state: "Active" | "Inactive" | "Suspended", derived from IsActive + IsSuspended.
    public string? Status { get; set; }
    public string? NationalIdMasked { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Qualification { get; set; }
    public string? JobGrade { get; set; }
    public DateTime? HireDate { get; set; }
    public string? InsuranceNumber { get; set; }
}

// POST /api/admin/users/{id}/reset-password (staff-management-backend-gaps.md, item 1)
public class ResetPasswordDto
{
    public string? Password { get; set; }
    public bool? ForcePasswordReset { get; set; }
    public string? SendCredentialsTo { get; set; } // "personalEmail" (never the official/ministry inbox)
}

public class SystemUsersSummaryDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int ActiveSessions { get; set; }
}

public class AddMinistryAdminDto
{
    public string? FullName { get; set; }
    // Kept for backward compatibility - if OfficialEmail isn't sent, this is used as both
    // the login email and OfficialEmail.
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalId { get; set; }
    public string? Role { get; set; } // MinistryAdmin / MinistryViewer / SuperAdmin
    public string? TemporaryPassword { get; set; }
    public bool? SendResetLink { get; set; }

    // Optional. "Factory" | "Warehouse" | "Pharmacy" | null (or "Ministry") = full access.
    // Only meaningful when Role is MinistryAdmin or MinistryViewer - a SuperAdmin is always
    // unscoped regardless of what's sent here.
    public string? EntityScope { get; set; }

    // ---- Staff Management HR/identity fields (staff-management-backend-gaps.md, item 2) ----
    public string? PersonalEmail { get; set; }
    public string? OfficialEmail { get; set; }
    public string? Department { get; set; }
    public string? Facility { get; set; }
    public string? Status { get; set; } // active | inactive | suspended
    public DateTime? DateOfBirth { get; set; }
    public string? Qualification { get; set; }
    public string? JobGrade { get; set; }
    public DateTime? HireDate { get; set; }
    public string? InsuranceNumber { get; set; }
    public bool? ForcePasswordReset { get; set; }
    public string? SendCredentialsTo { get; set; } // "personalEmail"
}

public class AuditLogListItemDto
{
    public int Id { get; set; }
    public string? LogCode { get; set; }
    public string? User { get; set; }
    public string? Role { get; set; }
    public string? Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Result { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }
}

// ---------------- Common ----------------
public class PagedResult<T>
{
    public List<T>? Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

// ---------------- Shared: richer Shipment/Inventory items for operational dashboards ----------------
public class ShipmentListItemDto
{
    public int Id { get; set; }
    public string? TransferCode { get; set; }
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? BatchNumber { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public long? ExpectedQuantity { get; set; }
    public long? ReceivedQuantity { get; set; }
    public bool? RequiresColdChain { get; set; }
    [DocumentedEnum(typeof(ShipmentStatus))]
    public string? ShipmentStatus { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class ShipmentDetailsDto
{
    public int Id { get; set; }
    public string? TransferCode { get; set; }
    public string? ShipmentType { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public string? ProductName { get; set; }
    public string? BatchNumber { get; set; }
    public long? ExpectedQuantity { get; set; }
    public long? ReceivedQuantity { get; set; }
    [DocumentedEnum(typeof(ShipmentStatus))]
    public string? ShipmentStatus { get; set; }
    public bool? RequiresColdChain { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
    [DocumentedEnum(typeof(InspectionResult))]
    public string? InspectionResult { get; set; }
}

public class InventoryStockListItemDto
{
    public int Id { get; set; }
    // Numeric batch id, added so Inventory-originated issue reports can populate
    // ReportIssueDto.batchId the same way Shipment-originated ones already do
    // (Frontend Integration Audit, Remaining Gaps - "ReportIssueDto.batchId from Inventory").
    public int? BatchId { get; set; }
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public string? BatchNumber { get; set; }
    public string? FactoryName { get; set; }
    public long? TotalReceivedQuantity { get; set; }
    public long? AvailableQuantity { get; set; }
    public long? ReservedQuantity { get; set; }
    public long? QuarantinedQuantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? RequiresColdChain { get; set; }
    [DocumentedEnum(typeof(InventoryStatus))]
    public string? InventoryStatus { get; set; }
}

public class InventoryStockDetailsDto
{
    public int Id { get; set; }
    public ProductInfoDto? ProductInfo { get; set; }
    public BatchInfoDto? BatchInfo { get; set; }
    public InventoryDistributionItemDto? WarehouseInventory { get; set; }
    public List<ShipmentSummaryItemDto>? RelatedShipments { get; set; }
}

public class OperationalProfileDto
{
    public AccountInfoDto? Account { get; set; }
    public EntityInfoDto? Entity { get; set; }
    public List<DocumentItemDto>? Documents { get; set; }
}

public class ReportIssueDto
{
    public string? AlertType { get; set; } // ComplianceIssue / ColdChainIssue / QuantityMismatch / DamagedPackage
    public string? Message { get; set; }
    public int? BatchId { get; set; }
    public int? ShipmentId { get; set; }
}

// ---------------- Factory Dashboard ----------------
public class FactoryOverviewCardsDto
{
    public int TotalBatches { get; set; }
    public int ReadyForDispatch { get; set; }
    public long UnitCodesGenerated { get; set; }
    public int ShipmentsInTransit { get; set; }
    public int ReceivedByWarehouses { get; set; }
    public int OpenAlerts { get; set; }
}

public class FactoryOverviewDto
{
    public FactoryOverviewCardsDto? Cards { get; set; }
    public List<BatchListItemDto>? RecentBatches { get; set; }
    public List<ShipmentListItemDto>? RecentShipments { get; set; }
    public List<AlertListItemDto>? OpenAlerts { get; set; }
}

public class CreateBatchDto
{
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? BatchNumber { get; set; }
    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public bool? SaveAsDraft { get; set; }
}

// GET /api/factory-dashboard/{factoryId}/warehouses (Backend_Action_Report_Factory_Portal, item 1.4)
// Deliberately thin - just enough for a dispatch-destination dropdown (name + cold-storage flag).
public class WarehouseOptionDto
{
    public int Id { get; set; }
    public string? WarehouseName { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public bool? HasColdStorage { get; set; }
    public string? WarehouseStatus { get; set; }
}

// GET /api/warehouse-dashboard/{warehouseId}/pharmacies - thin, dispatch-destination-only
// (Frontend Integration Audit, Remaining Gaps - Warehouse→Pharmacy directory).
public class PharmacyOptionDto
{
    public int Id { get; set; }
    public string? PharmacyName { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? PharmacyStatus { get; set; }
}

public class CreateDispatchDto
{
    public int? BatchId { get; set; }
    public long? DispatchQuantity { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? Notes { get; set; }
}

// ---------------- Warehouse Dashboard ----------------
public class WarehouseOverviewCardsDto
{
    public int IncomingShipments { get; set; }
    public int PendingInspection { get; set; }
    public int ReadyStockSkus { get; set; }
    public long TotalStockUnits { get; set; }
    public int OutgoingShipments { get; set; }
    public int OpenAlerts { get; set; }
}

public class WarehouseInventorySummaryCardsDto
{
    public int TotalProductsSkus { get; set; }
    public long TotalReceivedUnits { get; set; }
    public long AvailableUnits { get; set; }
    public long ReservedUnits { get; set; }
    public long QuarantinedUnits { get; set; }
    public long ExpiringSoonUnits { get; set; } // within 90 days
}

public class FactoryShipmentsSummaryDto
{
    public int TotalShipments { get; set; }
    public int InTransit { get; set; }
    public int Received { get; set; }
    public int PartiallyReceived { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}

public class ProductStockSummaryItemDto
{
    public string? ProductName { get; set; }
    public string? Strength { get; set; }
    public string? DosageForm { get; set; }
    public long? TotalStock { get; set; }
    public long? AvailableStock { get; set; }
    public long? InTransit { get; set; }
    public long? ExpiringSoon { get; set; }
}

public class WarehouseOverviewDto
{
    public WarehouseOverviewCardsDto? Cards { get; set; }
    public List<ShipmentListItemDto>? RecentIncomingShipments { get; set; }
    public List<ShipmentListItemDto>? RecentOutgoingShipments { get; set; }
    public List<ProductStockSummaryItemDto>? TopProducts { get; set; }
    public List<AlertListItemDto>? RecentAlerts { get; set; }
}

public class ReceiveShipmentDto
{
    public long? ReceivedQuantity { get; set; }
    [DocumentedEnum(typeof(InspectionResult))]
    public string? InspectionResult { get; set; } // Accepted / PartiallyAccepted / Rejected
    public string? Notes { get; set; }
}

public class DispatchToPharmacyDto
{
    public long? DispatchQuantity { get; set; }
    public int? TargetPharmacyId { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? Notes { get; set; }
}

public class MoveToQuarantineDto
{
    public long? QuarantineQuantity { get; set; }
    public string? Reason { get; set; }
}

// ---------------- Pharmacy Dashboard ----------------
public class PharmacyOverviewCardsDto
{
    public int IncomingShipments { get; set; }
    public int PendingReceiving { get; set; }
    public long CurrentStock { get; set; }
    public long? ColdChainStock { get; set; }
    public int OpenAlerts { get; set; }
    public long RecalledStock { get; set; }
}

public class PharmacyOverviewDto
{
    public PharmacyOverviewCardsDto? Cards { get; set; }
    public List<ShipmentListItemDto>? RecentIncomingShipments { get; set; }
    public List<InventoryStockListItemDto>? CurrentStockSummary { get; set; }
    public List<AlertListItemDto>? RecentAlerts { get; set; }
}
