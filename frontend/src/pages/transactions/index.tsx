import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, ArrowRight, CheckCircle, Clock, XCircle, Hash, X } from 'lucide-react';
import { transactionsApi } from '../../api/endpoints';
import { Card, CardHeader, Button, Input, Select, Badge, Skeleton, EmptyState } from '../../components/ui';
import { fmtDate, cn } from '../../utils';
import type { Transaction, TransactionStatus } from '../../types';

const MOCK_TRANSACTIONS: Transaction[] = [
  { id: 1, transactionRef: 'TXN-2024-0892', batchId: 1, batchNumber: 'B-2024-0892', medicineName: 'Amoxicillin 500mg', quantity: 2000, fromNodeId: 1, fromNodeType: 'Manufacturer', fromNodeName: 'EgyPharma Co.', toNodeId: 2, toNodeType: 'Warehouse', toNodeName: 'Cairo Central Warehouse', status: 'Completed', transactionHash: 'a3f8e2...c9d1', timestamp: new Date(Date.now() - 3600000 * 2).toISOString() },
  { id: 2, transactionRef: 'TXN-2024-0891', batchId: 2, batchNumber: 'B-2024-0891', medicineName: 'Paracetamol 1g', quantity: 500, fromNodeId: 2, fromNodeType: 'Warehouse', fromNodeName: 'Cairo Central Warehouse', toNodeId: 3, toNodeType: 'Pharmacy', toNodeName: 'Nasr City Pharmacy #1', status: 'Completed', transactionHash: 'b7c2a1...f4e3', timestamp: new Date(Date.now() - 3600000 * 5).toISOString() },
  { id: 3, transactionRef: 'TXN-2024-0890', batchId: 3, batchNumber: 'B-2024-0890', medicineName: 'Metformin 850mg', quantity: 1000, fromNodeId: 2, fromNodeType: 'Warehouse', fromNodeName: 'Alexandria North Hub', toNodeId: 4, toNodeType: 'Pharmacy', toNodeName: 'Sidi Gaber Pharmacy', status: 'Pending', transactionHash: 'c1d4e5...a2b9', timestamp: new Date(Date.now() - 3600000 * 8).toISOString() },
  { id: 4, transactionRef: 'TXN-2024-0889', batchId: 1, batchNumber: 'B-2024-0888', medicineName: 'Atorvastatin 20mg', quantity: 300, fromNodeId: 1, fromNodeType: 'Manufacturer', fromNodeName: 'Delta Pharma', toNodeId: 5, toNodeType: 'Warehouse', toNodeName: 'Giza Medical Depot', status: 'Failed', transactionHash: 'd5f6a7...b8c0', timestamp: new Date(Date.now() - 3600000 * 24).toISOString() },
];

const txnSchema = z.object({
  batchId: z.number().min(1),
  quantity: z.number().min(1),
  fromNodeType: z.enum(['Manufacturer', 'Warehouse', 'Pharmacy']),
  fromNodeId: z.number().min(1),
  toNodeType: z.enum(['Manufacturer', 'Warehouse', 'Pharmacy']),
  toNodeId: z.number().min(1),
  notes: z.string().optional(),
});
type TxnForm = z.infer<typeof txnSchema>;

const statusConfig: Record<TransactionStatus, { color: 'green' | 'orange' | 'red' | 'gray'; icon: React.ReactNode }> = {
  Completed: { color: 'green', icon: <CheckCircle size={14} /> },
  Pending: { color: 'orange', icon: <Clock size={14} /> },
  Failed: { color: 'red', icon: <XCircle size={14} /> },
  Cancelled: { color: 'gray', icon: <XCircle size={14} /> },
};

const NODE_TYPES = [{ value: 'Manufacturer', label: 'Manufacturer' }, { value: 'Warehouse', label: 'Warehouse' }, { value: 'Pharmacy', label: 'Pharmacy' }];
const NODE_OPTIONS = [{ value: '1', label: 'Node 1' }, { value: '2', label: 'Node 2' }, { value: '3', label: 'Node 3' }];

export function TransactionsPage() {
  const [showForm, setShowForm] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['transactions'],
    queryFn: () => transactionsApi.list({ pageSize: 20 }),
    retry: false,
  });

  const transactions: Transaction[] = data?.data ?? MOCK_TRANSACTIONS;

  const { register, handleSubmit, reset, setValue, formState: { errors, isSubmitting } } = useForm<TxnForm>({
    resolver: zodResolver(txnSchema),
    defaultValues: { fromNodeType: 'Manufacturer', toNodeType: 'Warehouse' },
  });

  const mutation = useMutation({
    mutationFn: transactionsApi.create,
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['transactions'] }); reset(); setShowForm(false); },
    onError: (e: { message?: string }) => setApiError(e.message ?? 'Failed to create transaction'),
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-slate-800">Transactions</h1>
          <p className="text-slate-400 text-sm mt-0.5">Immutable supply chain transfer log</p>
        </div>
        <Button variant="gradient" leftIcon={<Plus size={15} />} onClick={() => setShowForm(true)}>New Transfer</Button>
      </div>

      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-slate-900/50 backdrop-blur-sm" onClick={() => setShowForm(false)} />
          <div className="relative bg-white rounded-3xl shadow-2xl w-full max-w-lg z-10 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100 sticky top-0 bg-white z-10">
              <p className="font-bold text-slate-800">Record Transfer</p>
              <button onClick={() => setShowForm(false)} className="w-7 h-7 flex items-center justify-center rounded-lg bg-slate-100 text-slate-500"><X size={14} /></button>
            </div>
            <form
              onSubmit={handleSubmit((d) => { setApiError(null); mutation.mutate(d); })}
              className="p-6 space-y-4"
            >
              <Input label="Batch ID" type="number" placeholder="Batch ID" error={errors.batchId?.message} {...register('batchId', { valueAsNumber: true })} />
              <Input label="Quantity" type="number" placeholder="Number of units" error={errors.quantity?.message} {...register('quantity', { valueAsNumber: true })} />
              <div className="grid grid-cols-2 gap-3">
                <Select label="From Node Type" options={NODE_TYPES} onChange={(e) => setValue('fromNodeType', e.target.value as TxnForm['fromNodeType'])} />
                <Select label="From Node" options={NODE_OPTIONS} error={errors.fromNodeId?.message} onChange={(e) => setValue('fromNodeId', Number(e.target.value))} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <Select label="To Node Type" options={NODE_TYPES} onChange={(e) => setValue('toNodeType', e.target.value as TxnForm['toNodeType'])} />
                <Select label="To Node" options={NODE_OPTIONS} error={errors.toNodeId?.message} onChange={(e) => setValue('toNodeId', Number(e.target.value))} />
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-sm font-medium text-slate-700">Notes</label>
                <textarea className="w-full rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-emerald-400/40" rows={2} {...register('notes')} />
              </div>
              {apiError && <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">{apiError}</div>}
              <div className="flex gap-3">
                <Button type="submit" variant="gradient" isLoading={isSubmitting}>Record Transfer</Button>
                <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
              </div>
            </form>
          </div>
        </div>
      )}

      <Card>
        <CardHeader title="Transfer History" subtitle="All supply chain movements — cryptographically signed" />
        {isLoading ? (
          <div className="space-y-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="flex gap-4">
                <div className="flex flex-col items-center"><Skeleton className="w-8 h-8 rounded-full" /><div className="w-px flex-1 bg-slate-100 mt-2" /></div>
                <div className="flex-1 pb-6 space-y-2"><Skeleton className="h-4 w-3/4" /><Skeleton className="h-3 w-1/2" /></div>
              </div>
            ))}
          </div>
        ) : transactions.length === 0 ? (
          <EmptyState title="No transactions yet" description="Transfer records will appear here" />
        ) : (
          <div className="space-y-0">
            {transactions.map((txn, idx) => {
              const cfg = statusConfig[txn.status] ?? statusConfig.Pending;
              const isLast = idx === transactions.length - 1;
              return (
                <div key={txn.id} className="flex gap-4 group">
                  <div className="flex flex-col items-center flex-shrink-0">
                    <div className={cn('w-8 h-8 rounded-full flex items-center justify-center text-white shadow-sm transition-transform group-hover:scale-110',
                      txn.status === 'Completed' ? 'bg-emerald-500' : txn.status === 'Pending' ? 'bg-orange-400' : 'bg-red-400')}>
                      {cfg.icon}
                    </div>
                    {!isLast && <div className="w-px flex-1 bg-slate-100 mt-1 mb-1 min-h-[20px]" />}
                  </div>
                  <div className={cn('flex-1 pb-5', isLast && 'pb-0')}>
                    <div className="flex items-start justify-between gap-3">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 flex-wrap">
                          <code className="text-xs bg-slate-100 px-2 py-0.5 rounded-lg font-mono text-slate-600">{txn.transactionRef}</code>
                          <Badge color={cfg.color}>{txn.status}</Badge>
                        </div>
                        <p className="text-sm font-bold text-slate-800 mt-1.5">{txn.medicineName}</p>
                        <div className="flex items-center gap-2 mt-1 text-xs text-slate-500">
                          <span className="font-medium">{txn.fromNodeName}</span>
                          <ArrowRight size={11} className="text-slate-400" />
                          <span className="font-medium">{txn.toNodeName}</span>
                          <span className="text-slate-300">·</span>
                          <span>{txn.quantity.toLocaleString()} units</span>
                        </div>
                        <div className="flex items-center gap-1.5 mt-1.5 text-[11px] text-slate-400">
                          <Hash size={9} />
                          <code className="font-mono">{txn.transactionHash}</code>
                          <span className="text-slate-300">·</span>
                          <span>{fmtDate(txn.timestamp, 'datetime')}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </Card>
    </div>
  );
}
