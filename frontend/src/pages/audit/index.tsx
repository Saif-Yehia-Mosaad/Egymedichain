import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Search, Download } from 'lucide-react';
import { auditApi } from '../../api/endpoints';
import { Card, CardHeader, Badge, Button, Skeleton, EmptyState } from '../../components/ui';
import { fmtDate, cn } from '../../utils';
import type { AuditLog } from '../../types';

const MOCK_AUDIT: AuditLog[] = [
  { id: 1, userId: 2, userEmail: 'mfr1@egypharma.eg', action: 'CREATE', tableName: 'Medicines', recordId: 8, oldValues: undefined, newValues: '{"name":"Omeprazole 20mg"}', ipAddress: '192.168.1.10', createdAt: new Date(Date.now() - 3600000).toISOString() },
  { id: 2, userId: 1, userEmail: 'admin@moh.gov.eg', action: 'UPDATE', tableName: 'Manufacturers', recordId: 3, oldValues: '{"isApproved":false}', newValues: '{"isApproved":true}', ipAddress: '10.0.0.5', createdAt: new Date(Date.now() - 7200000).toISOString() },
  { id: 3, userId: 3, userEmail: 'wh1@cairohub.eg', action: 'CREATE', tableName: 'Transactions', recordId: 892, oldValues: undefined, newValues: '{"quantity":2000}', ipAddress: '172.16.0.8', createdAt: new Date(Date.now() - 86400000).toISOString() },
  { id: 4, userId: 4, userEmail: 'ph1@nasrcity.eg', action: 'READ', tableName: 'MedicineUnits', recordId: 12840, oldValues: undefined, newValues: undefined, ipAddress: '192.168.2.15', createdAt: new Date(Date.now() - 86400000 * 2).toISOString() },
  { id: 5, userId: 1, userEmail: 'admin@moh.gov.eg', action: 'DELETE', tableName: 'Users', recordId: 7, oldValues: '{"email":"test@test.eg"}', newValues: undefined, ipAddress: '10.0.0.5', createdAt: new Date(Date.now() - 86400000 * 3).toISOString() },
  { id: 6, userId: 2, userEmail: 'mfr1@egypharma.eg', action: 'CREATE', tableName: 'Batches', recordId: 15, oldValues: undefined, newValues: '{"batchNumber":"B-2024-0015"}', ipAddress: '192.168.1.10', createdAt: new Date(Date.now() - 86400000 * 4).toISOString() },
];

const ACTION_COLOR: Record<string, 'green' | 'blue' | 'orange' | 'red' | 'gray'> = {
  CREATE: 'green',
  UPDATE: 'blue',
  READ: 'gray',
  DELETE: 'red',
};

const TABLES = ['All', 'Medicines', 'Batches', 'Transactions', 'Users', 'Manufacturers', 'MedicineUnits'];
const ACTIONS = ['All', 'CREATE', 'UPDATE', 'DELETE', 'READ'];

export function AuditPage() {
  const [search, setSearch] = useState('');
  const [tableFilter, setTableFilter] = useState('All');
  const [actionFilter, setActionFilter] = useState('All');

  const { data, isLoading } = useQuery({
    queryKey: ['audit'],
    queryFn: () => auditApi.list({ pageSize: 50 }),
    retry: false,
  });

  const logs: AuditLog[] = data?.data ?? MOCK_AUDIT;

  const filtered = logs.filter((log) => {
    const matchSearch = !search || log.userEmail.includes(search) || log.tableName.toLowerCase().includes(search.toLowerCase());
    const matchTable = tableFilter === 'All' || log.tableName === tableFilter;
    const matchAction = actionFilter === 'All' || log.action === actionFilter;
    return matchSearch && matchTable && matchAction;
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-slate-800">Audit Logs</h1>
          <p className="text-slate-400 text-sm mt-0.5">Complete immutable activity history</p>
        </div>
        <Button variant="ghost" leftIcon={<Download size={14} />}>Export CSV</Button>
      </div>

      {/* Filters */}
      <div className="space-y-3">
        <div className="relative max-w-72">
          <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by user or table…"
            className="w-full pl-9 pr-4 py-2.5 text-sm bg-white border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-400/30"
          />
        </div>
        <div className="flex flex-wrap gap-3">
          <div className="flex flex-wrap gap-1.5">
            <span className="text-xs text-slate-400 self-center font-medium">Table:</span>
            {TABLES.map((t) => (
              <button key={t} onClick={() => setTableFilter(t)}
                className={cn('px-2.5 py-1 text-xs rounded-lg font-medium transition-all',
                  tableFilter === t ? 'bg-slate-700 text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200')}>
                {t}
              </button>
            ))}
          </div>
          <div className="flex flex-wrap gap-1.5">
            <span className="text-xs text-slate-400 self-center font-medium">Action:</span>
            {ACTIONS.map((a) => (
              <button key={a} onClick={() => setActionFilter(a)}
                className={cn('px-2.5 py-1 text-xs rounded-lg font-semibold transition-all',
                  actionFilter === a ? 'bg-slate-700 text-white' :
                  a === 'CREATE' ? 'bg-emerald-50 text-emerald-700 hover:bg-emerald-100' :
                  a === 'UPDATE' ? 'bg-blue-50 text-blue-700 hover:bg-blue-100' :
                  a === 'DELETE' ? 'bg-red-50 text-red-700 hover:bg-red-100' :
                  'bg-slate-100 text-slate-600 hover:bg-slate-200')}>
                {a}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Table */}
      <Card className="overflow-hidden p-0">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-100 bg-slate-50/80">
                {['#', 'User', 'Action', 'Table', 'Record ID', 'Changes', 'IP Address', 'Timestamp'].map((h) => (
                  <th key={h} className="text-left px-5 py-3.5 text-xs font-bold text-slate-500 uppercase tracking-wider whitespace-nowrap">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {isLoading ? (
                Array.from({ length: 6 }).map((_, i) => (
                  <tr key={i}>
                    {Array.from({ length: 8 }).map((_, j) => (
                      <td key={j} className="px-5 py-4"><Skeleton className="h-3 w-full" /></td>
                    ))}
                  </tr>
                ))
              ) : filtered.length === 0 ? (
                <tr><td colSpan={8}><EmptyState title="No audit records" description="No logs match your current filters" /></td></tr>
              ) : (
                filtered.map((log) => (
                  <tr key={log.id} className="hover:bg-slate-50/60 transition-colors">
                    <td className="px-5 py-3.5 text-xs font-mono text-slate-400">{log.id}</td>
                    <td className="px-5 py-3.5">
                      <p className="text-xs font-semibold text-slate-700">{log.userEmail}</p>
                      <p className="text-[11px] text-slate-400">ID: {log.userId}</p>
                    </td>
                    <td className="px-5 py-3.5">
                      <Badge color={ACTION_COLOR[log.action] ?? 'gray'}>{log.action}</Badge>
                    </td>
                    <td className="px-5 py-3.5">
                      <code className="text-xs bg-slate-100 px-2 py-0.5 rounded font-mono text-slate-600">{log.tableName}</code>
                    </td>
                    <td className="px-5 py-3.5 text-xs font-mono text-slate-500">#{log.recordId}</td>
                    <td className="px-5 py-3.5 max-w-[160px]">
                      {(log.oldValues || log.newValues) ? (
                        <div className="text-[10px] font-mono">
                          {log.oldValues && <p className="text-red-500 truncate">- {log.oldValues}</p>}
                          {log.newValues && <p className="text-emerald-600 truncate">+ {log.newValues}</p>}
                        </div>
                      ) : (
                        <span className="text-xs text-slate-300">—</span>
                      )}
                    </td>
                    <td className="px-5 py-3.5 text-xs font-mono text-slate-400">{log.ipAddress}</td>
                    <td className="px-5 py-3.5 text-xs text-slate-400 whitespace-nowrap">{fmtDate(log.createdAt, 'datetime')}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        {filtered.length > 0 && (
          <div className="px-5 py-3 border-t border-slate-100 bg-slate-50/50">
            <p className="text-xs text-slate-400">Showing {filtered.length} of {logs.length} records</p>
          </div>
        )}
      </Card>
    </div>
  );
}
