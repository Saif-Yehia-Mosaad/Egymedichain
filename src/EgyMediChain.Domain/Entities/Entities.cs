using EgyMediChain.Domain.Enums;

namespace EgyMediChain.Domain.Entities;

public class SystemUser
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    // Login email - kept as-is for backward compatibility (AuthController.Login queries this).
    // Conceptually equals OfficialEmail once that's set; falls back to whichever was provided first.
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalId { get; set; }
    public SystemRole? Role { get; set; }
    public EntityKind? EntityType { get; set; }
    public int? EntityId { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? IsActive { get; set; }
    // Added for Staff Management's tri-state status model (active/inactive/suspended).
    // IsActive stays the source of truth for login gating; IsSuspended is an additional flag
    // so "Suspended" can be told apart from a plain "Inactive" deactivation.
    public bool? IsSuspended { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // ---- Staff Management HR/identity fields (see staff-management-backend-gaps.md) ----
    public string? PersonalEmail { get; set; }
    public string? OfficialEmail { get; set; }
    public string? Department { get; set; }
    public string? Facility { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Qualification { get; set; }
    public string? JobGrade { get; set; }
    public DateTime? HireDate { get; set; }
    public string? InsuranceNumber { get; set; }

    // Soft delete (Backend Action Report §5.2) - permanently destroying a staff record breaks
    // its link to historical audit trail / approvals they signed off on, which is a compliance
    // risk for a regulated Ministry system. DELETE now sets these instead of removing the row.
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }

    public ICollection<AuthRefreshToken>? RefreshTokens { get; set; }
}

public class AuthRefreshToken
{
    public int Id { get; set; }
    public int? SystemUserId { get; set; }
    public SystemUser? SystemUser { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? CreatedByIp { get; set; }
}

// Self-service Forgot Password flow (Backend Action Report §2). OTP + reset token are both
// stored hashed, never in plaintext - see AuthController for the HMAC hashing.
public class PasswordResetRequest
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public SystemUser? User { get; set; }
    public string? RequestedEmail { get; set; }
    public string? HashedOtp { get; set; }
    public string? ResetTokenHash { get; set; }
    public int Attempts { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ResetTokenExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public string? RequestIp { get; set; }
}

// Async report generation job (Backend Action Report §4). Kept intentionally simple - runs via
// Task.Run + IServiceScopeFactory rather than a full queue/worker (Hangfire etc.), since none is
// wired up in this project yet. Good enough for Ministry-scale data volumes; revisit with a real
// job queue if report generation ever needs to survive an app restart mid-job.
public class ReportJob
{
    public Guid Id { get; set; }
    public int? RequestedByUserId { get; set; }
    public string? ReportType { get; set; }
    public string? Format { get; set; }
    public string? ParametersJson { get; set; }
    public string Status { get; set; } = "Queued"; // Queued | Processing | Completed | Failed
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
public class UserPreference
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public SystemUser? User { get; set; }
    public bool EmailAlerts { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool CriticalAlerts { get; set; } = true;
    public bool WeeklyReports { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Single-row Ministry-wide policy config (Backend Action Report §3.2). SuperAdmin-only.
public class SystemConfiguration
{
    public int Id { get; set; }
    public bool TwoFactorRequired { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 60;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}

public class RegistrationRequest
{
    public int Id { get; set; }
    public string? RequestCode { get; set; } // REQ-0012
    public EntityKind? EntityType { get; set; }
    public string? EntityName { get; set; }
    public string? RepresentativeName { get; set; }
    public string? Email { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool? EmailConfirmed { get; set; }
    public DocumentStatus? DocumentsOverallStatus { get; set; }
    public RegistrationStatus? RegistrationStatus { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    // Added for the "Request Inspection" action - nullable so existing rows are unaffected.
    public DateTime? InspectionScheduledDate { get; set; }
    public string? InspectorNotes { get; set; }
    public DateTime? InspectionRequestedAt { get; set; }

    public int? FactoryId { get; set; }
    public Factory? Factory { get; set; }
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public int? PharmacyId { get; set; }
    public Pharmacy? Pharmacy { get; set; }

    public int? SystemUserId { get; set; }
    public SystemUser? SystemUser { get; set; }

    public ICollection<EntityDocument>? Documents { get; set; }
}

public class EntityDocument
{
    public int Id { get; set; }
    public int? RegistrationRequestId { get; set; }
    public RegistrationRequest? RegistrationRequest { get; set; }
    public string? DocumentType { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public DateTime? UploadedAt { get; set; }
    public DocumentStatus? DocumentStatus { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class Factory
{
    public int Id { get; set; }
    public string? FactoryCode { get; set; } // FAC-2024-021 (display code, distinct from license number)
    public string? OfficialFactoryName { get; set; }
    public string? LegalCompanyName { get; set; }
    public string? DosageFormsProduced { get; set; }

    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

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

    // Factory Details tab
    public int? EstablishedYear { get; set; }
    public int? TotalProductionLines { get; set; }
    public string? MainProductionTypes { get; set; }
    public string? StorageTypes { get; set; }
    public string? QualityCertificates { get; set; }
    public string? Description { get; set; }

    // Registration Info tab (denormalized snapshot of the approval, so the factory portal
    // can show it without joining back through RegistrationRequest)
    public string? RegistrationRequestNo { get; set; }
    public DateTime? RegistrationSubmittedAt { get; set; }
    public DateTime? RegistrationApprovedAt { get; set; }
    public string? RegistrationApprovedBy { get; set; }
    public DateTime? RegistrationExpiryDate { get; set; }
    public string? RegistrationNotes { get; set; }

    public FacilityStatus? FactoryStatus { get; set; }
    public int? TotalBatches { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Batch>? Batches { get; set; }
}

public class Warehouse
{
    public int Id { get; set; }
    public string? WarehouseCode { get; set; } // WH-CAI-001
    public string? OfficialWarehouseName { get; set; }
    public string? WarehouseType { get; set; } // Main / Regional

    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? WarehouseLicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }

    public bool? HasColdStorage { get; set; }
    public bool? HasQuarantineArea { get; set; }
    public bool? HasDeliveryService { get; set; }

    public FacilityStatus? WarehouseStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Pharmacy
{
    public int Id { get; set; }
    public string? PharmacyCode { get; set; } // PH-ALX-001
    public string? OfficialPharmacyName { get; set; }
    public string? PharmacyType { get; set; } // Retail

    public string? Governorate { get; set; }
    public string? City { get; set; }
    public string? DistrictArea { get; set; }
    public string? FullAddress { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public int? DefaultWarehouseId { get; set; }
    public Warehouse? DefaultWarehouse { get; set; }
    public bool? HasColdStorage { get; set; }

    public string? PharmacyLicenseNumber { get; set; }
    public DateTime? LicenseIssueDate { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? PharmacistSyndicateId { get; set; }

    public FacilityStatus? PharmacyStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Generic multi-license row used by the Factory (and, if needed later, Warehouse/Pharmacy)
// Profile > Licenses tab. Kept separate from the single License Number fields above because
// a facility can hold several license types at once (Manufacturing, GMP, Environmental, Fire Safety...).
public class EntityLicense
{
    public int Id { get; set; }
    public EntityKind? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? LicenseType { get; set; } // Manufacturing License / GMP Certificate / Environmental License / Fire Safety License ...
    public string? LicenseNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; } // Active / Expired / Suspended
    public string? FileUrl { get; set; }
}

public class MedicineProduct
{
    public int Id { get; set; }
    public string? ProductName { get; set; }
    public string? GTIN { get; set; }
    public string? DosageForm { get; set; }
    public string? Strength { get; set; }
    public bool? RequiresColdChain { get; set; }
    public string? ProductStatus { get; set; }

    public ICollection<Batch>? Batches { get; set; }
}

public class Batch
{
    public int Id { get; set; }
    public string? BatchNumber { get; set; } // BAT-2024-001

    public int? MedicineProductId { get; set; }
    public MedicineProduct? MedicineProduct { get; set; }

    public int? FactoryId { get; set; }
    public Factory? Factory { get; set; }

    public long? Quantity { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public BatchStatus? BatchStatus { get; set; }
    public SupplyChainStage? SupplyChainStage { get; set; }
    public string? CurrentLocation { get; set; }

    public string? CreatedBy { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Unit Codes Summary (denormalized so we don't need millions of UnitCode rows)
    public long? TotalUnitCodes { get; set; }
    public long? GeneratedUnitCodes { get; set; }
    public long? InWarehouseUnitCodes { get; set; }
    public long? InPharmacyUnitCodes { get; set; }
    public long? SuspiciousUnitCodes { get; set; }
    public long? BlockedUnitCodes { get; set; }
    public long? RecalledUnitCodes { get; set; }
    public long? ScanCountTotal { get; set; }

    public int? OpenAlertsCount { get; set; }

    public ICollection<UnitCode>? UnitCodes { get; set; }
    public ICollection<Shipment>? Shipments { get; set; }
    public ICollection<InventoryStock>? InventoryStocks { get; set; }
    public ICollection<Alert>? Alerts { get; set; }
}

// Sample of individual unit codes kept for scan/alert linking (not exhaustive - see Batch summary fields)
public class UnitCode
{
    public int Id { get; set; }
    public string? UnitCodeValue { get; set; }
    public string? SerialNumber { get; set; }
    public string? GTIN { get; set; }
    public string? CodeValueHash { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }
    public UnitStatus? UnitStatus { get; set; }
    public string? CurrentHolderType { get; set; } // Factory/Warehouse/Pharmacy
    public string? CurrentHolderName { get; set; }
    public int? ScanCount { get; set; }
    public DateTime? FirstScannedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class Shipment
{
    public int Id { get; set; }
    public string? TransferCode { get; set; } // TRF-2034-1101
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }
    public ShipmentType? ShipmentType { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }

    // Structured FKs (populated by the Factory/Warehouse operational dashboards;
    // may be null on older/seed-only rows that only have the display strings above)
    public int? SourceFactoryId { get; set; }
    public int? SourceWarehouseId { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public int? DestinationPharmacyId { get; set; }

    public long? ExpectedQuantity { get; set; }
    public long? ReceivedQuantity { get; set; }
    public ShipmentStatus? ShipmentStatus { get; set; }
    public bool? RequiresColdChain { get; set; }
    public int? DispatchedByUserId { get; set; }
    public int? ReceivedByUserId { get; set; }
    public string? InspectionResult { get; set; } // Accepted / PartiallyAccepted / Rejected
    public string? Notes { get; set; }
    public DateTime? DispatchDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
}

public class InventoryStock
{
    public int Id { get; set; }
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }
    public string? HolderType { get; set; } // Warehouse/Pharmacy
    public string? HolderName { get; set; }
    public int? WarehouseId { get; set; }
    public int? PharmacyId { get; set; }
    public long? TotalReceivedQuantity { get; set; }
    public long? AvailableQuantity { get; set; }
    public long? ReservedQuantity { get; set; }
    public long? QuarantinedQuantity { get; set; }
    public InventoryStatus? InventoryStatus { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public class Alert
{
    public int Id { get; set; }
    public string? AlertCode { get; set; } // ALERT-2024-0091
    public AlertType? AlertType { get; set; }
    public AlertSeverity? Severity { get; set; }
    public EntityKind? EntityType { get; set; }
    public string? EntityName { get; set; }
    public int? BatchId { get; set; }
    public Batch? Batch { get; set; }
    public int? ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }
    public string? Message { get; set; }
    public AlertStatus? AlertStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class PublicVerificationScan
{
    public int Id { get; set; }
    public string? ScanCode { get; set; } // SCAN-2024-15021
    public string? ScannedGTIN { get; set; }
    public string? ScannedSerialNumber { get; set; }
    public string? ScannedBatchNumber { get; set; }
    public int? UnitCodeId { get; set; }
    public UnitCode? UnitCode { get; set; }
    public string? ProductName { get; set; }
    public VerificationResult? VerificationResult { get; set; }
    public string? Reason { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public DateTime? ScannedAt { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public string? LogCode { get; set; } // LOG-2024-55678
    public int? UserId { get; set; }
    public SystemUser? User { get; set; }
    public string? UserDisplayName { get; set; }
    public SystemRole? Role { get; set; }
    public AuditAction? Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public AuditResult? Result { get; set; }

    public string? IpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Factory? SourceFactory { get; set; }

    public Warehouse? SourceWarehouse { get; set; }

    public Warehouse? DestinationWarehouse { get; set; }

    public Pharmacy? DestinationPharmacy { get; set; }
}

