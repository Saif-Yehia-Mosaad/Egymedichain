import { useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Search, Edit, Eye, ChevronDown, CheckCircle, XCircle, ArrowLeft } from 'lucide-react';
import { medicinesApi } from '../../api/endpoints';
import { Card, CardHeader, Button, Badge, Input, Select, Skeleton, EmptyState } from '../../components/ui';
import { fmtDate, cn } from '../../utils';
import type { Medicine } from '../../types';

const MOCK_MEDICINES: Medicine[] = [
  { id: 1, name: 'Amoxicillin 500mg', activeIngredient: 'Amoxicillin', dosageForm: 'Capsule', strength: '500mg', registrationNumber: 'EG-MED-2024-001', manufacturerId: 1, manufacturerName: 'EgyPharma Co.', isActive: true, createdAt: '2024-01-15', batchCount: 12, totalUnits: 240000 },
  { id: 2, name: 'Paracetamol 1g', activeIngredient: 'Paracetamol', dosageForm: 'Tablet', strength: '1000mg', registrationNumber: 'EG-MED-2024-002', manufacturerId: 2, manufacturerName: 'Delta Pharma', isActive: true, createdAt: '2024-01-20', batchCount: 8, totalUnits: 180000 },
  { id: 3, name: 'Metformin 850mg', activeIngredient: 'Metformin HCl', dosageForm: 'Tablet', strength: '850mg', registrationNumber: 'EG-MED-2023-088', manufacturerId: 1, manufacturerName: 'EgyPharma Co.', isActive: true, createdAt: '2023-11-10', batchCount: 15, totalUnits: 320000 },
  { id: 4, name: 'Atorvastatin 20mg', activeIngredient: 'Atorvastatin', dosageForm: 'Tablet', strength: '20mg', registrationNumber: 'EG-MED-2023-044', manufacturerId: 3, manufacturerName: 'PharmaCairo', isActive: false, createdAt: '2023-08-01', batchCount: 5, totalUnits: 90000 },
  { id: 5, name: 'Omeprazole 20mg', activeIngredient: 'Omeprazole', dosageForm: 'Capsule', strength: '20mg', registrationNumber: 'EG-MED-2024-015', manufacturerId: 2, manufacturerName: 'Delta Pharma', isActive: true, createdAt: '2024-02-05', batchCount: 9, totalUnits: 150000 },
];

// ─── Medicines List ────────────────────────────────────────────
export function MedicinesPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');

  const { data, isLoading } = useQuery({
    queryKey: ['medicines', { search, statusFilter }],
    queryFn: () => medicinesApi.list({ search, isActive: statusFilter === 'all' ? undefined : statusFilter === 'active' }),
    retry: false,
  });

  const medicines = data?.data ?? MOCK_MEDICINES;
  const filtered = medicines.filter((m) => {
    const matchSearch = !search || m.name.toLowerCase().includes(search.toLowerCase()) || m.registrationNumber.includes(search);
    const matchStatus = statusFilter === 'all' || (statusFilter === 'active' ? m.isActive : !m.isActive);
    return matchSearch && matchStatus;
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-slate-800">Medicines Registry</h1>
          <p className="text-slate-400 text-sm mt-0.5">All registered pharmaceutical products</p>
        </div>
        <Button variant="gradient" leftIcon={<Plus size={15} />} onClick={() => navigate('/medicines/new')}>
          Register Medicine
        </Button>
      </div>

      <div className="flex items-center gap-3 flex-wrap">
        <div className="relative flex-1 min-w-48 max-w-72">
          <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Search by name or reg. number…"
            className="w-full pl-9 pr-4 py-2.5 text-sm bg-white border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-400/30 focus:border-emerald-400" />
        </div>
        <div className="flex gap-1.5">
          {(['all', 'active', 'inactive'] as const).map((s) => (
            <button key={s} onClick={() => setStatusFilter(s)}
              className={cn('px-3 py-2 text-sm rounded-xl font-medium transition-all capitalize',
                statusFilter === s ? 'bg-emerald-500 text-white shadow-md shadow-emerald-200' : 'bg-white border border-slate-200 text-slate-600 hover:bg-slate-50')}>
              {s}
            </button>
          ))}
        </div>
      </div>

      <Card className="overflow-hidden p-0">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-100 bg-slate-50/80">
                <th className="text-left px-6 py-3.5 text-xs font-bold text-slate-500 uppercase tracking-wider">Medicine</th>
                <th className="text-left px-4 py-3.5 text-xs font-bold text-slate-500 uppercase tracking-wider">Reg. Number</th>
                <th className="text-left px-4 py-3.5 text-xs font-bold text-slate-500 uppercase tracking-wider">Manufacturer</th>
                <th className="text-left px-4 py-3.5 text-xs font-bold text-slate-500 uppercase tracking-wider">Batches</th>
                <th className="text-left px-4 py-3.5 text-xs font-bold text-slate-500 uppercase tracking-wider">Status</th>
                <th className="text-left px-4 py-3.5 text-xs font-bold text-slate-500 uppercase tracking-wider">Registered</th>
                <th className="px-4 py-3.5" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {isLoading ? Array.from({ length: 5 }).map((_, i) => (
                <tr key={i}>{Array.from({ length: 7 }).map((_, j) => <td key={j} className="px-6 py-4"><Skeleton className="h-4 w-full" /></td>)}</tr>
              )) : filtered.length === 0 ? (
                <tr><td colSpan={7}><EmptyState title="No medicines found" description="Try adjusting your search or filters" /></td></tr>
              ) : filtered.map((m) => (
                <tr key={m.id} className="hover:bg-emerald-50/30 transition-colors group cursor-pointer" onClick={() => navigate(`/medicines/${m.id}`)}>
                  <td className="px-6 py-4">
                    <p className="text-sm font-bold text-slate-800 group-hover:text-emerald-700 transition-colors">{m.name}</p>
                    <p className="text-xs text-slate-400 mt-0.5">{m.activeIngredient} · {m.dosageForm}</p>
                  </td>
                  <td className="px-4 py-4"><code className="text-xs bg-slate-100 px-2 py-1 rounded-lg font-mono text-slate-600">{m.registrationNumber}</code></td>
                  <td className="px-4 py-4 text-sm text-slate-600">{m.manufacturerName}</td>
                  <td className="px-4 py-4 text-sm text-slate-600">{m.batchCount ?? '—'}</td>
                  <td className="px-4 py-4">
                    <Badge color={m.isActive ? 'green' : 'gray'}>
                      {m.isActive ? <><CheckCircle size={10} className="inline mr-1" />Active</> : <><XCircle size={10} className="inline mr-1" />Inactive</>}
                    </Badge>
                  </td>
                  <td className="px-4 py-4 text-sm text-slate-400">{fmtDate(m.createdAt)}</td>
                  <td className="px-4 py-4">
                    <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                      <button onClick={(e) => { e.stopPropagation(); navigate(`/medicines/${m.id}`); }} className="p-1.5 rounded-lg hover:bg-slate-100 text-slate-400 hover:text-slate-600"><Eye size={14} /></button>
                      <button onClick={(e) => { e.stopPropagation(); navigate(`/medicines/${m.id}/edit`); }} className="p-1.5 rounded-lg hover:bg-slate-100 text-slate-400 hover:text-slate-600"><Edit size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}

// ─── Medicine Form ─────────────────────────────────────────────
const medicineSchema = z.object({
  name: z.string().min(2, 'Required'),
  activeIngredient: z.string().min(2, 'Required'),
  dosageForm: z.string().min(1, 'Required'),
  strength: z.string().min(1, 'Required'),
  registrationNumber: z.string().min(5, 'Required'),
  manufacturerId: z.number().min(1, 'Required'),
});
type MedicineFormValues = z.infer<typeof medicineSchema>;

export function MedicineFormPage() {
  const { id } = useParams<{ id?: string }>();
  const isEdit = !!id && id !== 'new';
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [apiError, setApiError] = useState<string | null>(null);

  const { data: existing } = useQuery({
    queryKey: ['medicine', id],
    queryFn: () => medicinesApi.getById(Number(id)),
    enabled: isEdit,
    retry: false,
  });

  const { register, handleSubmit, setValue, formState: { errors, isSubmitting } } = useForm<MedicineFormValues>({
    resolver: zodResolver(medicineSchema),
    values: existing ? {
      name: existing.name, activeIngredient: existing.activeIngredient,
      dosageForm: existing.dosageForm, strength: existing.strength,
      registrationNumber: existing.registrationNumber, manufacturerId: existing.manufacturerId,
    } : undefined,
  });

  const mutation = useMutation({
    mutationFn: (data: MedicineFormValues) =>
      isEdit ? medicinesApi.update(Number(id), data) : medicinesApi.create(data),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['medicines'] }); navigate('/medicines'); },
    onError: (e: { message?: string }) => setApiError(e.message ?? 'Failed to save'),
  });

  const DOSAGE_FORMS = ['Tablet', 'Capsule', 'Syrup', 'Injection', 'Cream', 'Drops', 'Inhaler', 'Powder', 'Solution'];

  return (
    <div className="max-w-2xl space-y-6">
      <div className="flex items-center gap-4">
        <button onClick={() => navigate('/medicines')} className="w-9 h-9 flex items-center justify-center rounded-xl bg-white border border-slate-200 text-slate-500 hover:bg-slate-50 transition-colors"><ArrowLeft size={16} /></button>
        <div>
          <h1 className="text-2xl font-black text-slate-800">{isEdit ? 'Edit Medicine' : 'Register New Medicine'}</h1>
          <p className="text-slate-400 text-sm mt-0.5">Fill in the pharmaceutical product details</p>
        </div>
      </div>
      <Card>
        <form onSubmit={handleSubmit((d) => { setApiError(null); mutation.mutate(d); })} className="space-y-5">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
            <div className="sm:col-span-2"><Input label="Medicine Name" placeholder="e.g. Amoxicillin 500mg" error={errors.name?.message} {...register('name')} /></div>
            <Input label="Active Ingredient" placeholder="e.g. Amoxicillin Trihydrate" error={errors.activeIngredient?.message} {...register('activeIngredient')} />
            <Input label="Strength" placeholder="e.g. 500mg" error={errors.strength?.message} {...register('strength')} />
            <Select label="Dosage Form" options={[{ value: '', label: 'Select form…' }, ...DOSAGE_FORMS.map((f) => ({ value: f, label: f }))]} error={errors.dosageForm?.message} {...register('dosageForm')} />
            <Input label="Registration Number" placeholder="EG-MED-YYYY-XXX" error={errors.registrationNumber?.message} {...register('registrationNumber')} />
            <div className="sm:col-span-2">
              <Select label="Manufacturer"
                options={[{ value: '', label: 'Select manufacturer…' }, { value: '1', label: 'EgyPharma Co.' }, { value: '2', label: 'Delta Pharma' }, { value: '3', label: 'PharmaCairo' }]}
                error={errors.manufacturerId?.message}
                onChange={(e) => setValue('manufacturerId', Number(e.target.value))}
              />
            </div>
          </div>
          {apiError && <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">{apiError}</div>}
          <div className="flex gap-3 pt-2">
            <Button type="submit" variant="gradient" isLoading={isSubmitting}>{isEdit ? 'Update Medicine' : 'Register Medicine'}</Button>
            <Button type="button" variant="ghost" onClick={() => navigate('/medicines')}>Cancel</Button>
          </div>
        </form>
      </Card>
    </div>
  );
}

// ─── Medicine Detail ───────────────────────────────────────────
export function MedicineDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [batchesOpen, setBatchesOpen] = useState(false);

  const { data: medicine, isLoading } = useQuery({
    queryKey: ['medicine', id],
    queryFn: () => medicinesApi.getById(Number(id)),
    retry: false,
  });

  const { data: batches } = useQuery({
    queryKey: ['medicine-batches', id],
    queryFn: () => medicinesApi.getBatches(Number(id)),
    enabled: batchesOpen,
    retry: false,
  });

  const m = medicine ?? MOCK_MEDICINES.find((x) => x.id === Number(id));

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-4">
        <button onClick={() => navigate('/medicines')} className="w-9 h-9 flex items-center justify-center rounded-xl bg-white border border-slate-200 text-slate-500 hover:bg-slate-50"><ArrowLeft size={16} /></button>
        <div className="flex-1">{isLoading ? <Skeleton className="h-7 w-64" /> : <h1 className="text-2xl font-black text-slate-800">{m?.name}</h1>}</div>
        <Button variant="ghost" leftIcon={<Edit size={14} />} onClick={() => navigate(`/medicines/${id}/edit`)}>Edit</Button>
      </div>

      {m && (
        <>
          <Card>
            <CardHeader title="Medicine Details" />
            <div className="grid grid-cols-2 gap-4">
              {([['Active Ingredient', m.activeIngredient], ['Dosage Form', m.dosageForm], ['Strength', m.strength], ['Reg. Number', m.registrationNumber], ['Manufacturer', m.manufacturerName], ['Status', m.isActive ? 'Active' : 'Inactive'], ['Registered', fmtDate(m.createdAt)], ['Total Batches', String(m.batchCount ?? '—')]] as [string, string][]).map(([label, value]) => (
                <div key={label}>
                  <p className="text-xs text-slate-400 font-medium mb-0.5">{label}</p>
                  <p className="text-sm font-semibold text-slate-800">{value}</p>
                </div>
              ))}
            </div>
          </Card>

          <Card onClick={() => setBatchesOpen((v) => !v)}>
            <div className="flex items-center justify-between">
              <div>
                <p className="font-bold text-slate-800">Production Batches</p>
                <p className="text-xs text-slate-400 mt-0.5">Click to {batchesOpen ? 'hide' : 'load'} batch records</p>
              </div>
              <ChevronDown size={18} className={cn('text-slate-400 transition-transform', batchesOpen && 'rotate-180')} />
            </div>
            {batchesOpen && (
              <div className="mt-5 space-y-2" onClick={(e) => e.stopPropagation()}>
                {batches?.map((b) => (
                  <div key={b.id} className="flex items-center gap-4 px-4 py-3 rounded-xl bg-slate-50 border border-slate-100">
                    <code className="text-xs font-mono text-slate-600">{b.batchNumber}</code>
                    <span className="text-xs text-slate-400">Exp: {fmtDate(b.expiryDate)}</span>
                    <span className="text-xs text-slate-500 ml-auto">Qty: {b.totalQuantity.toLocaleString()}</span>
                    <Badge color={b.isExpired ? 'red' : 'green'}>{b.isExpired ? 'Expired' : 'Valid'}</Badge>
                  </div>
                )) ?? <p className="text-sm text-slate-400 text-center py-4">No batches available offline</p>}
              </div>
            )}
          </Card>
        </>
      )}
    </div>
  );
}

export { MedicineFormPage as MedicineEditPage };
void Link;
