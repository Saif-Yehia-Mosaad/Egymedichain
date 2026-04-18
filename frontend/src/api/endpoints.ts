import { apiClient } from './client';
import type {
  AuthUser, AuthTokens, LoginPayload, ForgotPasswordPayload,
  VerifyCodePayload, ResetPasswordPayload, Medicine, MedicineFormData,
  Batch, InventoryItem, Transaction, TransactionFormData, Alert,
  Warehouse, Pharmacy, SystemUser, AuditLog, DashboardStats,
  QrVerifyResult, PaginatedResponse,
} from '../types';

// ─── Auth ─────────────────────────────────────────────────────
export const authApi = {
  login: async (payload: LoginPayload) => {
    const { data } = await apiClient.post<{ user: AuthUser } & AuthTokens>('/auth/login', payload);
    return data;
  },
  register: async (payload: Record<string, unknown>) => {
    const { data } = await apiClient.post('/auth/register', payload);
    return data;
  },
  forgotPassword: async (payload: ForgotPasswordPayload) => {
    const { data } = await apiClient.post('/auth/forgot-password', payload);
    return data;
  },
  verifyCode: async (payload: VerifyCodePayload) => {
    const { data } = await apiClient.post('/auth/verify-reset-code', payload);
    return data;
  },
  resetPassword: async (payload: ResetPasswordPayload) => {
    const { data } = await apiClient.post('/auth/reset-password', payload);
    return data;
  },
  refresh: async (refreshToken: string) => {
    const { data } = await apiClient.post<AuthTokens>('/auth/refresh', { refreshToken });
    return data;
  },
};

// ─── Analytics / Dashboard ───────────────────────────────────
export const analyticsApi = {
  getDashboard: async () => {
    const { data } = await apiClient.get<DashboardStats>('/analytics/dashboard');
    return data;
  },
};

// ─── Medicines ───────────────────────────────────────────────
export const medicinesApi = {
  list: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get<PaginatedResponse<Medicine>>('/medicines', { params });
    return data;
  },
  getById: async (id: number) => {
    const { data } = await apiClient.get<Medicine>(`/medicines/${id}`);
    return data;
  },
  create: async (payload: MedicineFormData) => {
    const { data } = await apiClient.post<Medicine>('/medicines', payload);
    return data;
  },
  update: async (id: number, payload: Partial<MedicineFormData>) => {
    const { data } = await apiClient.put<Medicine>(`/medicines/${id}`, payload);
    return data;
  },
  approve: async (id: number) => {
    const { data } = await apiClient.post(`/medicines/${id}/approve`);
    return data;
  },
  getBatches: async (medicineId: number) => {
    const { data } = await apiClient.get<Batch[]>(`/medicines/${medicineId}/batches`);
    return data;
  },
};

// ─── Batches ─────────────────────────────────────────────────
export const batchesApi = {
  create: async (payload: Record<string, unknown>) => {
    const { data } = await apiClient.post<Batch>('/batches', payload);
    return data;
  },
  getById: async (id: number) => {
    const { data } = await apiClient.get<Batch>(`/batches/${id}`);
    return data;
  },
};

// ─── Inventory ───────────────────────────────────────────────
export const inventoryApi = {
  getByNode: async (nodeId: number, nodeType?: string) => {
    const { data } = await apiClient.get<InventoryItem[]>(`/inventory/${nodeId}`, {
      params: nodeType ? { nodeType } : undefined,
    });
    return data;
  },
  getAll: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get<PaginatedResponse<InventoryItem>>('/inventory', { params });
    return data;
  },
};

// ─── Transactions ────────────────────────────────────────────
export const transactionsApi = {
  list: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get<PaginatedResponse<Transaction>>('/transactions', { params });
    return data;
  },
  create: async (payload: TransactionFormData) => {
    const { data } = await apiClient.post<Transaction>('/transactions', payload);
    return data;
  },
  getById: async (id: number) => {
    const { data } = await apiClient.get<Transaction>(`/transactions/${id}`);
    return data;
  },
};

// ─── Alerts ──────────────────────────────────────────────────
export const alertsApi = {
  list: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get<PaginatedResponse<Alert>>('/alerts', { params });
    return data;
  },
  resolve: async (id: number) => {
    const { data } = await apiClient.post(`/alerts/${id}/resolve`);
    return data;
  },
  broadcast: async (payload: Partial<Alert>) => {
    const { data } = await apiClient.post('/alerts', payload);
    return data;
  },
};

// ─── Warehouses ──────────────────────────────────────────────
export const warehousesApi = {
  list: async () => {
    const { data } = await apiClient.get<Warehouse[]>('/warehouses');
    return data;
  },
  getById: async (id: number) => {
    const { data } = await apiClient.get<Warehouse>(`/warehouses/${id}`);
    return data;
  },
  create: async (payload: Partial<Warehouse>) => {
    const { data } = await apiClient.post<Warehouse>('/warehouses', payload);
    return data;
  },
};

// ─── Pharmacies ──────────────────────────────────────────────
export const pharmaciesApi = {
  list: async () => {
    const { data } = await apiClient.get<Pharmacy[]>('/pharmacies');
    return data;
  },
  requestStock: async (payload: Record<string, unknown>) => {
    const { data } = await apiClient.post('/stock-requests', payload);
    return data;
  },
};

// ─── Users ───────────────────────────────────────────────────
export const usersApi = {
  list: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get<PaginatedResponse<SystemUser>>('/users', { params });
    return data;
  },
  getById: async (id: number) => {
    const { data } = await apiClient.get<SystemUser>(`/users/${id}`);
    return data;
  },
  toggleStatus: async (id: number) => {
    const { data } = await apiClient.post(`/users/${id}/toggle-status`);
    return data;
  },
};

// ─── Audit Logs ──────────────────────────────────────────────
export const auditApi = {
  list: async (params?: Record<string, unknown>) => {
    const { data } = await apiClient.get<PaginatedResponse<AuditLog>>('/audit', { params });
    return data;
  },
  getByBatch: async (batchId: number) => {
    const { data } = await apiClient.get<AuditLog[]>(`/audit/batch/${batchId}`);
    return data;
  },
};

// ─── QR Verification ─────────────────────────────────────────
export const verifyApi = {
  verify: async (serial: string) => {
    const { data } = await apiClient.get<QrVerifyResult>(`/verify/${serial}`);
    return data;
  },
};
