import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Warehouse as WarehouseIcon, MapPin, X } from 'lucide-react';
import { RadialBarChart, RadialBar, ResponsiveContainer, Tooltip } from 'recharts';
import { warehousesApi } from '../../api/endpoints';
import { Card, Button, Input, Select, Badge, Skeleton, EmptyState } from '../../components/ui';
import { fmtNumber, stockPercent, cn } from '../../utils';
import type { Warehouse } from '../../types';

const MOCK_WAREHOUSES: Warehouse[] = [
  { id: 1, name: 'Cairo Central Warehouse', region: 'Greater Cairo', governorate: 'Cairo', capacity: 500000, licenseNumber: 'WH-CAI-001', currentOccupancy: 312000, isActive: true, createdAt: '2022-01-10' },
  { id: 2, name: 'Alexandria North Hub', region: 'Northern Coast', governorate: 'Alexandria', capacity: 250000, licenseNumber: 'WH-ALX-002', currentOccupancy: 198000, isActive: true, createdAt: '2022-03-15' },
  { id: 3, name: 'Giza Medical Depot', region: 'Greater Cairo', governorate: 'Giza', capacity: 180000, licenseNumber: 'WH-GIZ-003', currentOccupancy: 72000, isActive: true, createdAt: '2022-06-20' },
  { id: 4, name: 'Assiut Regional Store', region: 'Upper Egypt', governorate: 'Assiut', capacity: 120000, licenseNumber: 'WH-AST-004', currentOccupancy: 45000, isActive: false, createdAt: '2023-01-05' },
  { id: 5, name: 'Port Said Hub', region: 'Canal Zone', governorate: 'Port Said', capacity: 90000, licenseNumber: 'WH-POS-005', currentOccupancy: 81000, isActive: true, createdAt: '2023-08-12' },
];

const GOVERNORATES = ['Cairo', 'Alexandria', 'Giza', 'Assiut', 'Port Said', 'Aswan', 'Luxor', 'Mansoura', 'Tanta', 'Zagazig'];

const warehouseSchema = z.object({
  name: z.string().min(3, 'Required'),
  region: z.string().min(2, 'Required'),
  governorate: z.string().min(1, 'Required'),
  capacity: z.number().min(1000, 'Min 1,000 units'),
  licenseNumber: z.string().min(5, 'Required'),
});
type WHForm = z.infer<typeof warehouseSchema>;

export function WarehousesPage() {
  const [showForm, setShowForm] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({ queryKey: ['warehouses'], queryFn: warehousesApi.list, retry: false });
  const warehouses: Warehouse[] = data ?? MOCK_WAREHOUSES;

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<WHForm>({
    resolver: zodResolver(warehouseSchema),
  });

  const mutation = useMutation({
    mutationFn: warehousesApi.create,
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['warehouses'] }); reset(); setShowForm(false); },
    onError: (e: { message?: string }) => setApiError(e.message ?? 'Failed to create warehouse'),
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-slate-800">Warehouses</h1>
          <p className="text-slate-400 text-sm mt-0.5">Distribution hubs across Egypt</p>
        </div>
        <Button variant="gradient" leftIcon={<Plus size={15} />} onClick={() => setShowForm(true)}>Add Warehouse</Button>
      </div>

      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-slate-900/50 backdrop-blur-sm" onClick={() => setShowForm(false)} />
          <div className="relative bg-white rounded-3xl shadow-2xl w-full max-w-lg z-10">
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
              <p className="font-bold text-slate-800">Add New Warehouse</p>
              <button onClick={() => setShowForm(false)} className="w-7 h-7 flex items-center justify-center rounded-lg bg-slate-100 text-slate-500"><X size={14} /></button>
            </div>
            <form onSubmit={handleSubmit((d) => { setApiError(null); mutation.mutate(d); })} className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="col-span-2"><Input label="Warehouse Name" placeholder="e.g. Cairo Central Depot" error={errors.name?.message} {...register('name')} /></div>
                <Input label="Region" placeholder="e.g. Greater Cairo" error={errors.region?.message} {...register('region')} />
                <Select label="Governorate" options={[{ value: '', label: 'Select…' }, ...GOVERNORATES.map((g) => ({ value: g, label: g }))]} error={errors.governorate?.message} {...register('governorate')} />
                <Input label="Capacity (units)" type="number" placeholder="500000" error={errors.capacity?.message} {...register('capacity', { valueAsNumber: true })} />
                <Input label="License Number" placeholder="WH-XXX-000" error={errors.licenseNumber?.message} {...register('licenseNumber')} />
              </div>
              {apiError && <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">{apiError}</div>}
              <div className="flex gap-3 pt-2">
                <Button type="submit" variant="gradient" isLoading={isSubmitting}>Create Warehouse</Button>
                <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
              </div>
            </form>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
        {isLoading
          ? Array.from({ length: 4 }).map((_, i) => <div key={i} className="rounded-2xl border border-slate-200 bg-white p-5 space-y-3 h-48"><Skeleton className="h-4 w-3/4" /><Skeleton className="h-3 w-1/2" /><Skeleton className="h-24 w-full" /></div>)
          : warehouses.length === 0
          ? <div className="col-span-full"><EmptyState icon={<WarehouseIcon size={48} />} title="No warehouses found" /></div>
          : warehouses.map((wh) => {
            const pct = stockPercent(wh.currentOccupancy, wh.capacity);
            const radialData = [{ value: pct, fill: pct > 80 ? '#ef4444' : pct > 60 ? '#f97316' : '#22c55e' }];
            return (
              <Card key={wh.id} className={cn('hover:shadow-md transition-shadow', !wh.isActive && 'opacity-60')}>
                <div className="flex items-start justify-between mb-4">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <WarehouseIcon size={15} className="text-emerald-500 flex-shrink-0" />
                      <p className="text-sm font-bold text-slate-800 truncate">{wh.name}</p>
                    </div>
                    <div className="flex items-center gap-1 text-xs text-slate-400">
                      <MapPin size={10} />{wh.governorate} · {wh.region}
                    </div>
                  </div>
                  <Badge color={wh.isActive ? 'green' : 'gray'}>{wh.isActive ? 'Active' : 'Inactive'}</Badge>
                </div>
                <div className="flex items-center gap-4">
                  <div className="w-28 h-28 flex-shrink-0">
                    <ResponsiveContainer width="100%" height="100%">
                      <RadialBarChart cx="50%" cy="50%" innerRadius="60%" outerRadius="90%" data={radialData} startAngle={90} endAngle={-270}>
                        <RadialBar dataKey="value" cornerRadius={8} background={{ fill: '#f1f5f9' }} />
                        <Tooltip formatter={(v) => [`${v}%`, 'Occupancy']} contentStyle={{ borderRadius: '10px', border: 'none', fontSize: 12 }} />
                      </RadialBarChart>
                    </ResponsiveContainer>
                    <p className="text-center text-xs font-bold text-slate-700 -mt-3">{pct}%</p>
                  </div>
                  <div className="flex-1 space-y-3">
                    <div><p className="text-[11px] text-slate-400 mb-0.5">Current Occupancy</p><p className="text-base font-black text-slate-800">{fmtNumber(wh.currentOccupancy)}</p></div>
                    <div><p className="text-[11px] text-slate-400 mb-0.5">Total Capacity</p><p className="text-sm font-bold text-slate-500">{fmtNumber(wh.capacity)}</p></div>
                    <div><p className="text-[11px] text-slate-400 mb-0.5">License</p><code className="text-xs bg-slate-100 px-2 py-0.5 rounded-lg font-mono text-slate-600">{wh.licenseNumber}</code></div>
                  </div>
                </div>
              </Card>
            );
          })}
      </div>
    </div>
  );
}
