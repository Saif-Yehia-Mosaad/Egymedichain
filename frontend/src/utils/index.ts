import { clsx, type ClassValue } from 'clsx';
import type { UserRole } from '../types';

// ─── Class merger ─────────────────────────────────────────────
export function cn(...inputs: ClassValue[]) {
  return clsx(inputs);
}

// ─── Role-Based Permissions ───────────────────────────────────
const ROLE_SCREENS: Record<UserRole, string[]> = {
  Ministry: ['dashboard', 'medicines', 'medicine-detail', 'medicine-form', 'inventory', 'warehouses', 'transactions', 'alerts', 'users', 'audit', 'settings'],
  Manufacturer: ['dashboard', 'medicines', 'medicine-detail', 'medicine-form', 'transactions', 'settings'],
  Warehouse: ['dashboard', 'inventory', 'warehouses', 'transactions', 'alerts', 'settings'],
  Pharmacy: ['dashboard', 'pharmacy', 'inventory', 'transactions', 'alerts', 'settings'],
  Consumer: ['verify'],
};

export function canAccess(role: UserRole | undefined, screen: string): boolean {
  if (!role) return false;
  return ROLE_SCREENS[role]?.includes(screen) ?? false;
}

// ─── Number / Date Formatting ─────────────────────────────────
export function fmtNumber(n: number | undefined | null, opts?: Intl.NumberFormatOptions): string {
  if (n == null) return '—';
  return new Intl.NumberFormat('en-EG', opts).format(n);
}

export function fmtDate(iso: string | undefined | null, style: 'date' | 'datetime' | 'relative' = 'date'): string {
  if (!iso) return '—';
  const d = new Date(iso);
  if (isNaN(d.getTime())) return '—';

  if (style === 'relative') {
    const diff = Date.now() - d.getTime();
    const mins = Math.floor(diff / 60_000);
    if (mins < 1) return 'just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    return `${Math.floor(hrs / 24)}d ago`;
  }

  if (style === 'datetime') {
    return d.toLocaleString('en-EG', { dateStyle: 'medium', timeStyle: 'short' });
  }

  return d.toLocaleDateString('en-EG', { dateStyle: 'medium' });
}

export function isExpiringSoon(expiryDate: string, daysThreshold = 45): boolean {
  const diff = new Date(expiryDate).getTime() - Date.now();
  return diff > 0 && diff < daysThreshold * 86_400_000;
}

export function stockPercent(qty: number, capacity: number): number {
  if (!capacity) return 0;
  return Math.min(100, Math.round((qty / capacity) * 100));
}
