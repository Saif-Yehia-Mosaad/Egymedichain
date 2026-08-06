namespace EgyMediChain.Domain.Enums;

public enum SystemRole
{
    SuperAdmin,
    MinistryAdmin,
    MinistryViewer,
    FactoryUser,
    WarehouseUser,
    PharmacyUser
}

public enum EntityKind
{
    Factory,
    Warehouse,
    Pharmacy,
    Ministry
}

public enum RegistrationStatus
{
    Pending,
    UnderReview,
    NeedsMoreDocuments,
    Approved,
    Rejected,
    Cancelled
}

public enum DocumentStatus
{
    UnderReview,
    Complete,
    NeedsReplacement,
    Rejected
}

public enum FacilityStatus
{
    PendingReview,
    Active,
    Suspended,
    Inactive,
    Rejected
}

public enum BatchStatus
{
    // Ministry-facing (existing)
    InProduction,
    InSupplyChain,
    InWarehouse,
    InPharmacy,
    Quarantined,
    Recalled,
    Available,
    // Factory operational lifecycle
    Draft,
    Registered,
    CodesGenerated,
    ReadyForWarehouseDispatch,
    PartiallyDispatched,
    FullyDispatched,
    Expired,
    Cancelled
}

public enum SupplyChainStage
{
    AtFactory,
    InTransit,
    Stored,
    Available,
    Quarantined,
    Recalled
}

public enum UnitStatus
{
    Generated,
    InWarehouse,
    InPharmacy,
    Blocked,
    Recalled,
    Suspicious
}

public enum ShipmentType
{
    FactoryToWarehouse,
    WarehouseToPharmacy,
    WarehouseToWarehouse
}

public enum ShipmentStatus
{
    Pending,
    InTransit,
    PendingInspection,
    Delivered,
    Received,
    PartiallyReceived,
    Rejected,
    Cancelled
}

public enum InventoryStatus
{
    Active,
    Quarantined,
    Recalled,
    Blocked
}

public enum AlertType
{
    ColdChainIssue,
    QuantityMismatch,
    SuspiciousScan,
    LicenseExpiry,
    BlockedUnitScan,
    ComplianceIssue,
    Recall,
    DuplicateSerial,
    DamagedPackage,
    ExpiredBatch,
    DocumentMissing
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AlertStatus
{
    Open,
    UnderReview,
    Resolved,
    Dismissed
}

public enum VerificationResult
{
    Authentic,
    NotFound,
    DuplicateScan,
    Recalled,
    Expired,
    Blocked,
    Suspicious
}
public enum AuditResult
{
    Success,
    Failed,
    Warning
}
public enum AuditAction
{
    ApproveRegistration,
    RejectRegistration,
    UpdateDocumentStatus,
    SuspendEntity,
    ReactivateEntity,
    SetInactiveEntity,
    FreezeBatch,
    CreateRecallAlert,
    RevokeUserSessions,
    CreateAdmin,
    ResolveAlert,
    DismissAlert,
    CreateAlert,
    ApproveDocument,
    RejectDocument,
    // Factory operational
    CreateBatch,
    GenerateCodes,
    MarkBatchReadyForDispatch,
    CancelDraftBatch,
    DispatchShipment,
    // Warehouse / Pharmacy operational
    ReceiveShipment,
    RejectShipment,
    QuarantineStock,
    // Deletions (added for cleanup / delete endpoints)
    DeleteRegistrationRequest,
    DeleteUser,
    DeleteDraftBatch,
    DeleteAlert,
    // Added for the Staff Management gap fixes
    ResetUserPassword,
    SuspendUser,
    // Added for Edit Staff + Forgot Password (Backend Action Report)
    StaffMemberUpdated,
    ForgotPasswordRequested,
    PasswordResetVerifyAttempted,
    PasswordResetCompleted,
    SystemConfigUpdated,
    LoginFailed,
    LoginLockedOut
}

