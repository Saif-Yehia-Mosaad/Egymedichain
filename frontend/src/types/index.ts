// ─── Auth & Users ────────────────────────────────────────────
export type UserRole = 'Ministry' | 'Manufacturer' | 'Warehouse' | 'Pharmacy' | 'Consumer';

export interface AuthUser {
  id: number;
  email: string;
  name: string;
  role: UserRole;
  entityId: number;
  entityName: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface LoginPayload { email: string; password: string; }
export interface ForgotPasswordPayload { email: string; }
export interface VerifyCodePayload { email: string; code: string; }
export interface ResetPasswordPayload { email: string; token: string; newPassword: string; confirmPassword: string; }

// ─── Medicine ────────────────────────────────────────────────
export interface Medicine {
  id: number;
  name: string;
  activeIngredient: string;
  dosageForm: string;
  strength: string;
  registrationNumber: string;
  manufacturerId: number;
  manufacturerName: string;
  isActive: boolean;
  createdAt: string;
  batchCount?: number;
  totalUnits?: number;
}

export interface MedicineFormData {
  name: string;
  activeIngredient: string;
  dosageForm: string;
  strength: string;
  registrationNumber: string;
  manufacturerId: number;
}

// ─── Batch ───────────────────────────────────────────────────
export interface Batch {
  id: number;
  batchNumber: string;
  medicineId: number;
  medicineName: string;
  manufactureDate: string;
  expiryDate: string;
  totalQuantity: number;
  remainingQuantity: number;
  isExpired: boolean;
  importShipmentId?: number;
}

// ─── Inventory ───────────────────────────────────────────────
export type NodeType = 'Manufacturer' | 'Warehouse' | 'Pharmacy';

export interface InventoryItem {
  id: number;
  nodeId: number;
  nodeType: NodeType;
  nodeName: string;
  medicineId: number;
  medicineName: string;
  quantity: number;
  minThreshold: number;
  capacity?: number;
  expiryDate?: string;
  isLowStock: boolean;
  isExpiringSoon: boolean;
  updatedAt: string;
}

// ─── Transaction ─────────────────────────────────────────────
export type TransactionStatus = 'Pending' | 'Completed' | 'Failed' | 'Cancelled';

export interface Transaction {
  id: number;
  transactionRef: string;
  batchId: number;
  batchNumber: string;
  medicineName: string;
  quantity: number;
  fromNodeId: number;
  fromNodeType: NodeType;
  fromNodeName: string;
  toNodeId: number;
  toNodeType: NodeType;
  toNodeName: string;
  status: TransactionStatus;
  transactionHash: string;
  timestamp: string;
  notes?: string;
}

export interface TransactionFormData {
  batchId: number;
  quantity: number;
  fromNodeId: number;
  fromNodeType: NodeType;
  toNodeId: number;
  toNodeType: NodeType;
  notes?: string;
}

// ─── Alert ───────────────────────────────────────────────────
export type AlertType = 'LowStock' | 'ExpiryWarning' | 'SuspiciousTransfer' | 'CounterfeitDetected' | 'SystemAlert';
export type AlertSeverity = 'Low' | 'Medium' | 'High' | 'Critical';

export interface Alert {
  id: number;
  type: AlertType;
  severity: AlertSeverity;
  message: string;
  isResolved: boolean;
  transactionId?: number;
  createdAt: string;
  resolvedAt?: string;
}

// ─── Warehouse ───────────────────────────────────────────────
export interface Warehouse {
  id: number;
  name: string;
  region: string;
  governorate: string;
  capacity: number;
  licenseNumber: string;
  currentOccupancy: number;
  isActive: boolean;
  createdAt: string;
}

// ─── Pharmacy ────────────────────────────────────────────────
export interface Pharmacy {
  id: number;
  name: string;
  syndicateNumber: string;
  governorate: string;
  region: string;
  isActive: boolean;
  licenseNumber: string;
  createdAt: string;
}

// ─── User Management ─────────────────────────────────────────
export interface SystemUser {
  id: number;
  email: string;
  name: string;
  phone: string;
  role: UserRole;
  entityId: number;
  entityName: string;
  isActive: boolean;
  createdAt: string;
}

// ─── Audit Log ───────────────────────────────────────────────
export interface AuditLog {
  id: number;
  userId: number;
  userEmail: string;
  action: string;
  tableName: string;
  recordId: number;
  oldValues?: string;
  newValues?: string;
  ipAddress: string;
  createdAt: string;
}

// ─── Analytics Dashboard ─────────────────────────────────────
export interface DashboardStats {
  serializedUnits: number;
  lowStockMedicines: number;
  transfersToday: number;
  counterfeitBlocks: number;
  activePharmacies: number;
  activeWarehouses: number;
  stockByGovernorate: { name: string; coverage: number }[];
  transferTrend: { date: string; count: number }[];
  alertsByType: { type: string; count: number }[];
  recentActivity: ActivityItem[];
}

export interface ActivityItem {
  id: number;
  type: 'transfer' | 'alert' | 'registration' | 'verification';
  message: string;
  timestamp: string;
  severity?: AlertSeverity;
}

// ─── QR Verification ─────────────────────────────────────────
export interface QrVerifyResult {
  isValid: boolean;
  isCounterfeit: boolean;
  serialNumber: string;
  medicineName: string;
  batchNumber: string;
  manufactureDate: string;
  expiryDate: string;
  currentLocation: string;
  currentStatus: string;
  journeySteps: { location: string; timestamp: string; nodeType: NodeType }[];
}

// ─── API Pagination ──────────────────────────────────────────
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode: number;
}
