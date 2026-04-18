import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { AlertTriangle, Clock, Package, Search, Filter } from 'lucide-react';
import { inventoryApi } from '../../api/endpoints';
import { Card, CardHeader, Badge, Skeleton, EmptyState, ProgressBar } from '../../components/ui';
import { fmtDate, fmtNumber, isExpiringSoon, stockPercent, cn } from '../../utils';
import type { InventoryItem } from '../../types';

const MOCK_INVENTORY: InventoryItem[] = [
  { id: 1, nodeId: 1, nodeType: 'Pharmacy', nodeName: 'Nasr City Pharmacy #1', medicineId: 1, medicineName: 'Amoxicillin 500mg', quantity: 320, minThreshold: 500, capacity: 2000, expiryDate: '2025-03-15', isLowStock: true, isExpiringSoon: false, updatedAt: new Date().toISOString() },
  { id: 2, nodeId: 1, nodeType: 'Pharmacy', nodeName: 'Nasr City Pharmacy #1', medicineId: 2, medicineName: 'Paracetamol 1g', quantity: 1800, minThreshold: 400, capacity: 3000, expiryDate: '2025-12-01', isLowStock: false, isExpiringSoon: false, updatedAt: new Date().toISOString() },
  { id: 3, nodeId: 2, nodeType: 'Warehouse', nodeName: 'Cairo Central Warehouse', medicineId: 3, medicineName: 'Metformin 850mg', quantity: 15000, minThreshold: 2000, capacity: 50000, expiryDate: '2025-01-28', isLowStock: false, isExpiringSoon: true, updatedAt: new Date().toISOString() },
  { id: 4, nodeId: 2, nodeType: 'Warehouse', nodeName: 'Cairo Central Warehouse', medicineId: 1, medicineName: 'Amoxicillin 500mg', quantity: 85, minThreshold: 1000, capacity: 20000, expiryDate: '2025-11-20', isLowStock: true, isExpiringSoon: false, updatedAt: new Date().toISOString() },
  { id: 5, nodeId: 3, nodeType: 'Pharmacy', nodeName: 'Alexandria Pharmacy #7', medicineId: 5, medicineName: 'Omeprazole 20mg', quantity: 2200, minThreshold: 300, capacity: 4000, expiryDate: '2026-06-10', isLowStock: false, isExpiringSoon: false, updatedAt: new Date().toISOString() },
  { id: 6, nodeId: 3, nodeType: 'Pharmacy', nodeName: 'Alexandria Pharmacy #7', medicineId: 4, medicineName: 'Atorvastatin 20mg', quantity: 120, minThreshold: 200, capacity: 1500, expiryDate: '2025-02-10', isLowStock: true, isExpiringSoon: true, updatedAt: new Date().toISOString() },
];

type FilterType = 'all' | 'lowStock' | 'expiring';

export function InventoryPage() {
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<FilterType>('all');

  const { data, isLoading } = useQuery({
    queryKey: ['inventory'],
    queryFn: () => inventoryApi.getAll(),
    retry: false,
  });

  const inventory: InventoryItem[] = data?.data ?? MOCK_INVENTORY;

  const filtered = inventory.filter((item) => {
    const matchSearch = !search || item.medicineName.toLowerCase().includes(search.toLowerCase()) || item.nodeName.toLowerCase().includes(search.toLowerCase());
    const matchFilter =
      filter === 'all' ? true :
      filter === 'lowStock' ? item.isLowStock :
      isExpiringSoon(item.expiryDate ?? '');
    return matchSearch && matchFilter;
  });

  const summary = {
    lowStock: inventory.filter((i) => i.isLowStock).length,
    expiring: inventory.filter((i) => isExpiringSoon(i.expiryDate ?? '')).length,
    total: inventory.length,
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-black text-slate-800">Inventory</h1>
          <p className="text-slate-400 text-sm mt-0.5">Stock levels across all nodes</p>
        </div>
        <div className="flex items-center gap-3 flex-wrap">
          {summary.lowStock > 0 && (
            <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-red-50 border border-red-200">
              <AlertTriangle size={14} className="text-red-500" />
              <span className="text-sm font-semibold text-red-700">{summary.lowStock} Low Stock</span>
            </div>
          )}
          {summary.expiring > 0 && (
            <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-orange-50 border border-orange-200">
              <Clock size={14} className="text-orange-500" />
              <span className="text-sm font-semibold text-orange-700">{summary.expiring} Expiring Soon</span>
            </div>
          )}
        </div>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3 flex-wrap">
        <div className="relative flex-1 min-w-48 max-w-72">
          <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search medicine or node…"
            className="w-full pl-9 pr-4 py-2.5 text-sm bg-white border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-400/30"
          />
        </div>
        <div className="flex gap-1.5">
          {([
            { key: 'all', label: 'All', count: summary.total },
            { key: 'lowStock', label: 'Low Stock', count: summary.lowStock },
            { key: 'expiring', label: 'Expiring', count: summary.expiring },
          ] as const).map((f) => (
            <button
              key={f.key}
              onClick={() => setFilter(f.key)}
              className={cn(
                'flex items-center gap-1.5 px-3 py-2 text-sm rounded-xl font-medium transition-all',
                filter === f.key
                  ? 'bg-emerald-500 text-white shadow-md shadow-emerald-200'
                  : 'bg-white border border-slate-200 text-slate-600 hover:bg-slate-50'
              )}
            >
              <Filter size={11} />
              {f.label}
              <span className={cn('text-xs px-1.5 py-0.5 rounded-full font-bold', filter === f.key ? 'bg-white/20 text-white' : 'bg-slate-100 text-slate-500')}>
                {f.count}
              </span>
            </button>
          ))}
        </div>
      </div>

      {/* Cards grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        {isLoading
          ? Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="rounded-2xl border border-slate-200 bg-white p-5 space-y-3">
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-3 w-1/2" />
              <Skeleton className="h-2 w-full" />
              <Skeleton className="h-3 w-1/3" />
            </div>
          ))
          : filtered.length === 0
          ? <div className="col-span-full"><EmptyState icon={<Package size={48} />} title="No inventory items" description="Try changing the filters" /></div>
          : filtered.map((item) => {
            const pct = stockPercent(item.quantity, item.capacity ?? item.minThreshold * 5);
            const expiring = isExpiringSoon(item.expiryDate ?? '');
            const isLow = item.isLowStock;

            return (
              <div
                key={item.id}
                className={cn(
                  'rounded-2xl border bg-white p-5 transition-all hover:shadow-md',
                  isLow && !expiring && 'border-red-200 shadow-red-100/60 shadow-md ring-1 ring-red-100',
                  expiring && 'border-orange-200 shadow-orange-100/60 shadow-md ring-1 ring-orange-100',
                  !isLow && !expiring && 'border-slate-200'
                )}
              >
                {/* Header */}
                <div className="flex items-start justify-between mb-3">
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-bold text-slate-800 truncate">{item.medicineName}</p>
                    <p className="text-xs text-slate-400 mt-0.5 truncate">{item.nodeName}</p>
                  </div>
                  <div className="flex flex-col gap-1 flex-shrink-0 ml-2">
                    {isLow && <Badge color="red"><AlertTriangle size={9} className="inline mr-0.5" />Low</Badge>}
                    {expiring && <Badge color="orange"><Clock size={9} className="inline mr-0.5" />Expiring</Badge>}
                    {!isLow && !expiring && <Badge color="green">OK</Badge>}
                  </div>
                </div>

                {/* Stock level */}
                <div className="mb-3">
                  <div className="flex items-center justify-between mb-1.5">
                    <span className="text-xs text-slate-400">Stock level</span>
                    <span className="text-xs font-bold text-slate-700">{pct}%</span>
                  </div>
                  <ProgressBar
                    value={pct}
                    max={100}
                    color={isLow ? 'red' : pct < 50 ? 'orange' : 'green'}
                  />
                </div>

                {/* Stats row */}
                <div className="grid grid-cols-3 gap-2">
                  <div className="text-center">
                    <p className="text-base font-black text-slate-800">{fmtNumber(item.quantity)}</p>
                    <p className="text-[10px] text-slate-400">In Stock</p>
                  </div>
                  <div className="text-center border-x border-slate-100">
                    <p className="text-base font-black text-slate-500">{fmtNumber(item.minThreshold)}</p>
                    <p className="text-[10px] text-slate-400">Min Threshold</p>
                  </div>
                  <div className="text-center">
                    <p className="text-base font-black text-slate-500">{fmtNumber(item.capacity)}</p>
                    <p className="text-[10px] text-slate-400">Capacity</p>
                  </div>
                </div>

                {/* Expiry */}
                {item.expiryDate && (
                  <p className={cn('text-xs mt-3 pt-3 border-t border-slate-100', expiring ? 'text-orange-600 font-semibold' : 'text-slate-400')}>
                    <Clock size={10} className="inline mr-1" />
                    Expires: {fmtDate(item.expiryDate)}
                  </p>
                )}
              </div>
            );
          })}
      </div>
    </div>
  );
}
