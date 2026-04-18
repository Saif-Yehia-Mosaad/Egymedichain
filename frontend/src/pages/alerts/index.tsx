import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { AlertTriangle, Bell, CheckCircle, ShieldAlert, Package, Clock, Filter } from 'lucide-react';
import { alertsApi } from '../../api/endpoints';
import { Card, CardHeader, Button, Badge, Skeleton, EmptyState } from '../../components/ui';
import { fmtDate, cn } from '../../utils';
import type { Alert, AlertSeverity, AlertType } from '../../types';

const MOCK_ALERTS: Alert[] = [
  { id: 1, type: 'LowStock', severity: 'High', message: 'Amoxicillin 500mg stock at Nasr City Pharmacy is critically low (320 units, threshold: 500)', isResolved: false, createdAt: new Date(Date.now() - 3600000).toISOString() },
  { id: 2, type: 'SuspiciousTransfer', severity: 'Critical', message: 'Suspicious transfer pattern detected: Batch B-2024-0033 transferred 5 times in 2 hours', isResolved: false, transactionId: 4, createdAt: new Date(Date.now() - 7200000).toISOString() },
  { id: 3, type: 'ExpiryWarning', severity: 'Medium', message: 'Metformin 850mg Batch B-2024-0011 expires in 12 days — Cairo Central Warehouse', isResolved: false, createdAt: new Date(Date.now() - 86400000).toISOString() },
  { id: 4, type: 'CounterfeitDetected', severity: 'Critical', message: 'Counterfeit medicine detected at QR scan: SN-00291-EGM — Alexandria Pharmacy #12', isResolved: false, createdAt: new Date(Date.now() - 86400000 * 2).toISOString() },
  { id: 5, type: 'LowStock', severity: 'Medium', message: 'Atorvastatin 20mg stock low at Giza Medical Depot (85 units, threshold: 200)', isResolved: true, createdAt: new Date(Date.now() - 86400000 * 3).toISOString(), resolvedAt: new Date(Date.now() - 86400000 * 2).toISOString() },
];

const TYPE_ICON: Record<AlertType, React.ReactNode> = {
  LowStock: <Package size={15} />,
  ExpiryWarning: <Clock size={15} />,
  SuspiciousTransfer: <AlertTriangle size={15} />,
  CounterfeitDetected: <ShieldAlert size={15} />,
  SystemAlert: <Bell size={15} />,
};

const SEVERITY_COLOR: Record<AlertSeverity, { badge: 'red' | 'orange' | 'blue' | 'gray'; bg: string; border: string; icon: string }> = {
  Critical: { badge: 'red', bg: 'bg-red-50', border: 'border-red-200', icon: 'text-red-500' },
  High: { badge: 'orange', bg: 'bg-orange-50', border: 'border-orange-200', icon: 'text-orange-500' },
  Medium: { badge: 'blue', bg: 'bg-blue-50', border: 'border-blue-200', icon: 'text-blue-500' },
  Low: { badge: 'gray', bg: 'bg-slate-50', border: 'border-slate-200', icon: 'text-slate-400' },
};

type SevFilter = 'all' | AlertSeverity;
type StatusFilter = 'all' | 'active' | 'resolved';

export function AlertsPage() {
  const [sevFilter, setSevFilter] = useState<SevFilter>('all');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('active');
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['alerts'],
    queryFn: () => alertsApi.list({ pageSize: 50 }),
    retry: false,
  });

  const alerts: Alert[] = data?.data ?? MOCK_ALERTS;

  const resolveMutation = useMutation({
    mutationFn: alertsApi.resolve,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['alerts'] }),
  });

  const filtered = alerts.filter((a) => {
    const matchSev = sevFilter === 'all' || a.severity === sevFilter;
    const matchStatus = statusFilter === 'all' ? true : statusFilter === 'active' ? !a.isResolved : a.isResolved;
    return matchSev && matchStatus;
  });

  const counts = {
    critical: alerts.filter((a) => a.severity === 'Critical' && !a.isResolved).length,
    high: alerts.filter((a) => a.severity === 'High' && !a.isResolved).length,
    active: alerts.filter((a) => !a.isResolved).length,
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-black text-slate-800">Alerts & Notifications</h1>
          <p className="text-slate-400 text-sm mt-0.5">System-generated alerts requiring attention</p>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          {counts.critical > 0 && (
            <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-red-50 border border-red-200 animate-pulse">
              <ShieldAlert size={14} className="text-red-500" />
              <span className="text-sm font-bold text-red-700">{counts.critical} Critical</span>
            </div>
          )}
          {counts.high > 0 && (
            <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-orange-50 border border-orange-200">
              <AlertTriangle size={14} className="text-orange-500" />
              <span className="text-sm font-semibold text-orange-700">{counts.high} High</span>
            </div>
          )}
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3 items-center">
        {/* Status */}
        <div className="flex gap-1.5">
          {(['all', 'active', 'resolved'] as StatusFilter[]).map((s) => (
            <button key={s} onClick={() => setStatusFilter(s)}
              className={cn('px-3 py-2 text-sm rounded-xl font-medium transition-all capitalize',
                statusFilter === s ? 'bg-slate-800 text-white' : 'bg-white border border-slate-200 text-slate-600 hover:bg-slate-50')}>
              {s}
            </button>
          ))}
        </div>
        <div className="w-px h-6 bg-slate-200" />
        {/* Severity */}
        <div className="flex gap-1.5">
          <Filter size={14} className="text-slate-400 self-center" />
          {(['all', 'Critical', 'High', 'Medium', 'Low'] as SevFilter[]).map((s) => (
            <button key={s} onClick={() => setSevFilter(s)}
              className={cn('px-3 py-1.5 text-xs rounded-xl font-semibold transition-all',
                sevFilter === s
                  ? s === 'Critical' ? 'bg-red-500 text-white' : s === 'High' ? 'bg-orange-400 text-white' : s === 'Medium' ? 'bg-blue-500 text-white' : 'bg-slate-700 text-white'
                  : 'bg-white border border-slate-200 text-slate-500 hover:bg-slate-50')}>
              {s}
            </button>
          ))}
        </div>
      </div>

      {/* Alert cards */}
      <div className="space-y-3">
        {isLoading ? (
          Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="rounded-2xl border border-slate-200 bg-white p-5 space-y-2">
              <Skeleton className="h-4 w-1/4" />
              <Skeleton className="h-3 w-full" />
              <Skeleton className="h-3 w-2/3" />
            </div>
          ))
        ) : filtered.length === 0 ? (
          <EmptyState icon={<CheckCircle size={48} />} title={statusFilter === 'active' ? 'No active alerts' : 'No alerts found'} description="All clear — no alerts match the current filters" />
        ) : (
          filtered.map((alert) => {
            const cfg = SEVERITY_COLOR[alert.severity];
            return (
              <div
                key={alert.id}
                className={cn(
                  'rounded-2xl border p-5 transition-all',
                  alert.isResolved ? 'opacity-60 bg-slate-50 border-slate-200' : `${cfg.bg} ${cfg.border}`,
                  !alert.isResolved && alert.severity === 'Critical' && 'ring-1 ring-red-300'
                )}
              >
                <div className="flex items-start gap-3">
                  <div className={cn('flex-shrink-0 mt-0.5', cfg.icon)}>
                    {TYPE_ICON[alert.type]}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap mb-1.5">
                      <Badge color={cfg.badge}>{alert.severity}</Badge>
                      <span className="text-xs text-slate-400 bg-white/60 px-2 py-0.5 rounded-lg font-medium">
                        {alert.type.replace(/([A-Z])/g, ' $1').trim()}
                      </span>
                      {alert.isResolved && <Badge color="green"><CheckCircle size={9} className="inline mr-1" />Resolved</Badge>}
                    </div>
                    <p className="text-sm text-slate-700 leading-relaxed">{alert.message}</p>
                    <div className="flex items-center gap-4 mt-2 text-xs text-slate-400">
                      <span>{fmtDate(alert.createdAt, 'relative')}</span>
                      {alert.resolvedAt && <span>Resolved: {fmtDate(alert.resolvedAt, 'datetime')}</span>}
                    </div>
                  </div>
                  {!alert.isResolved && (
                    <Button
                      size="sm"
                      variant="ghost"
                      leftIcon={<CheckCircle size={12} />}
                      isLoading={resolveMutation.isPending}
                      onClick={() => resolveMutation.mutate(alert.id)}
                      className="flex-shrink-0 whitespace-nowrap"
                    >
                      Resolve
                    </Button>
                  )}
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
}
