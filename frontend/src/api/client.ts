import axios, { AxiosInstance, AxiosError } from 'axios';
import type { ApiError } from '../types';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'https://localhost:7001/api';

// ─── Token Storage ────────────────────────────────────────────
export const tokenStore = {
  getAccess: () => localStorage.getItem('egm_access_token'),
  getRefresh: () => localStorage.getItem('egm_refresh_token'),
  setTokens: (access: string, refresh: string) => {
    localStorage.setItem('egm_access_token', access);
    localStorage.setItem('egm_refresh_token', refresh);
  },
  clear: () => {
    localStorage.removeItem('egm_access_token');
    localStorage.removeItem('egm_refresh_token');
    localStorage.removeItem('egm_user');
  },
};

// ─── Axios Instance ───────────────────────────────────────────
export const apiClient: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
});

// ─── Request Interceptor — attach JWT ────────────────────────
apiClient.interceptors.request.use((config) => {
  const token = tokenStore.getAccess();
  if (token) config.headers['Authorization'] = `Bearer ${token}`;
  return config;
});

// ─── Response Interceptor — handle 401, format errors ────────
let isRefreshing = false;
let failedQueue: { resolve: (v: unknown) => void; reject: (e: unknown) => void }[] = [];

const processQueue = (error: unknown, token: string | null) => {
  failedQueue.forEach((prom) => (error ? prom.reject(error) : prom.resolve(token)));
  failedQueue = [];
};

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as typeof error.config & { _retry?: boolean };
    if (error.response?.status === 401 && !original?._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          if (original) original.headers['Authorization'] = `Bearer ${token}`;
          return apiClient(original!);
        });
      }
      original!._retry = true;
      isRefreshing = true;
      const refreshToken = tokenStore.getRefresh();
      if (!refreshToken) {
        tokenStore.clear();
        window.location.href = '/#/login';
        return Promise.reject(error);
      }
      try {
        const { data } = await axios.post(`${BASE_URL}/auth/refresh`, { refreshToken });
        tokenStore.setTokens(data.accessToken, data.refreshToken);
        processQueue(null, data.accessToken);
        return apiClient(original!);
      } catch (refreshError) {
        processQueue(refreshError, null);
        tokenStore.clear();
        window.location.href = '/#/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }
    const apiErr: ApiError = {
      message:
        (error.response?.data as Record<string, string>)?.message ??
        error.message ??
        'An unexpected error occurred',
      errors: (error.response?.data as ApiError)?.errors,
      statusCode: error.response?.status ?? 0,
    };
    return Promise.reject(apiErr);
  }
);
