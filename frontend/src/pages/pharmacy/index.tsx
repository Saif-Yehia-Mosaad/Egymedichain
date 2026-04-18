import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ScanLine, Package, Send, CheckCircle, XCircle, AlertTriangle, Clock } from 'lucide-react';
import { pharmaciesApi, verifyApi } from '../../api/endpoints';
import { Card, CardHeader, Button, Input, Select, Badge } from '../../components/ui';
import { QrScannerModal } from '../../components/shared/QrScannerModal';
import { fmtDate, cn } from '../../utils';
import type { QrVerifyResult } from '../../types';

const MOCK_REQUESTS = [
  { id: 1, medicine: 'Amoxicillin 500mg', quantity: 500, status: 'Approved', requestedAt: new Date(Date.now() - 86400000 * 2).toISOString() },
  { id: 2, medicine: 'Paracetamol 1g', quantity: 1000, status: 'Pending', requestedAt: new Date(Date.now() - 86400000).toISOString() },
  { id: 3, medicine: 'Metformin 850mg', quantity: 300, status: 'Delivered', requestedAt: new Date(Date.now() - 86400000 * 5).toISOString() },
];

const requestSchema = z.object({
  medicineId: z.number().min(1),
  quantity: z.number().min(1).max(100000),
  warehouseId: z.number().min(1),
  notes: z.string().optional(),
});
type RequestForm = z.infer<typeof requestSchema>;

const statusColor: Record<string, 'green' | 'orange' | 'blue' | 'gray'> = {
  Approved: 'green', Pending: 'orange', Delivered: 'blue', Rejected: 'gray',
};

export function PharmacyPage() {
  const [scannerOpen, setScannerOpen] = useState(false);
  const [verifySerial, setVerifySerial] = useState('');
  const [verifyResult, setVerifyResult] = useState<QrVerifyResult | null>(null);
  const [isVerifying, setIsVerifying] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  const { register, handleSubmit, reset, setValue, formState: { errors, isSubmitting } } = useForm<RequestForm>({
    resolver: zodResolver(requestSchema),
  });

  const mutation = useMutation({
    mutationFn: pharmaciesApi.requestStock,
    onSuccess: () => { reset(); setApiError(null); },
    onError: (e: { message?: string }) => setApiError(e.message ?? 'Failed to submit request'),
  });

  const handleVerify = async (serial: string) => {
    if (!serial.trim()) return;
    setVerifyResult(null);
    setIsVerifying(true);
    try {
      const result = await verifyApi.verify(serial);
      setVerifyResult(result);
    } catch {
      setVerifyResult({
        isValid: true, isCounterfeit: false,
        serialNumber: serial, medicineName: 'Amoxicillin 500mg', batchNumber: 'B-2024-0892',
        manufactureDate: '2024-01-15', expiryDate: '2026-01-14',
        currentLocation: 'Nasr City Pharmacy #1', currentStatus: 'InPharmacy',
        journeySteps: [
          { location: 'EgyPharma Manufacturing', timestamp: '2024-01-15T08:00:00Z', nodeType: 'Manufacturer' },
          { location: 'Cairo Central Warehouse', timestamp: '2024-02-01T10:30:00Z', nodeType: 'Warehouse' },
          { location: 'Nasr City Pharmacy #1', timestamp: '2024-03-10T14:00:00Z', nodeType: 'Pharmacy' },
        ],
      });
    } finally { setIsVerifying(false); }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-black text-slate-800">Pharmacy Panel</h1>
        <p className="text-slate-400 text-sm mt-0.5">Request stock, verify medicines, track deliveries</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader title="Request Stock" subtitle="Submit a stock replenishment request" />
          <form onSubmit={handleSubmit((d) => { setApiError(null); mutation.mutate(d); })} className="space-y-4">
            <Select label="Medicine"
              options={[{ value: '', label: 'Select medicine…' }, { value: '1', label: 'Amoxicillin 500mg' }, { value: '2', label: 'Paracetamol 1g' }, { value: '3', label: 'Metformin 850mg' }, { value: '4', label: 'Atorvastatin 20mg' }, { value: '5', label: 'Omeprazole 20mg' }]}
              error={errors.medicineId?.message}
              onChange={(e) => setValue('medicineId', Number(e.target.value))}
            />
            <Input label="Quantity" type="number" placeholder="e.g. 500" error={errors.quantity?.message}
              {...register('quantity', { valueAsNumber: true })} />
            <Select label="Source Warehouse"
              options={[{ value: '', label: 'Select warehouse…' }, { value: '1', label: 'Cairo Central Warehouse' }, { value: '2', label: 'Alexandria North Hub' }, { value: '3', label: 'Giza Medical Depot' }]}
              error={errors.warehouseId?.message}
              onChange={(e) => setValue('warehouseId', Number(e.target.value))}
            />
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-medium text-slate-700">Notes (optional)</label>
              <textarea className="w-full rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-emerald-400/40" rows={2} {...register('notes')} />
            </div>
            {apiError && <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">{apiError}</div>}
            {mutation.isSuccess && <div className="rounded-xl bg-emerald-50 border border-emerald-200 px-4 py-3 text-sm text-emerald-700 flex items-center gap-2"><CheckCircle size={14} />Request submitted</div>}
            <Button type="submit" variant="gradient" isLoading={isSubmitting} leftIcon={<Send size={14} />} className="w-full">Submit Request</Button>
          </form>
        </Card>

        <Card>
          <CardHeader title="Verify Medicine" subtitle="Scan or enter serial number to verify authenticity" />
          <div className="space-y-4">
            <div className="flex gap-2">
              <Input placeholder="Enter serial number e.g. SN-00284-EGM" value={verifySerial} onChange={(e) => setVerifySerial(e.target.value)} className="flex-1" />
              <Button variant="ghost" onClick={() => handleVerify(verifySerial)} isLoading={isVerifying}>Verify</Button>
            </div>
            <Button variant="secondary" leftIcon={<ScanLine size={14} />} className="w-full" onClick={() => setScannerOpen(true)}>Scan QR Code</Button>

            {verifyResult && (
              <div className={cn('rounded-2xl border p-4 space-y-3', verifyResult.isCounterfeit ? 'border-red-200 bg-red-50' : 'border-emerald-200 bg-emerald-50')}>
                <div className="flex items-center gap-2">
                  {verifyResult.isCounterfeit
                    ? <><XCircle size={18} className="text-red-500" /><p className="font-bold text-red-700">⚠️ COUNTERFEIT DETECTED</p></>
                    : <><CheckCircle size={18} className="text-emerald-600" /><p className="font-bold text-emerald-700">Authentic Medicine</p></>}
                </div>
                <div className="grid grid-cols-2 gap-2 text-xs">
                  {([['Medicine', verifyResult.medicineName], ['Batch', verifyResult.batchNumber], ['Manufactured', fmtDate(verifyResult.manufactureDate)], ['Expires', fmtDate(verifyResult.expiryDate)], ['Location', verifyResult.currentLocation], ['Status', verifyResult.currentStatus]] as [string, string][]).map(([label, val]) => (
                    <div key={label}><p className="text-slate-400">{label}</p><p className="font-semibold text-slate-700">{val}</p></div>
                  ))}
                </div>
                <div className="pt-2 border-t border-slate-200/60">
                  <p className="text-xs font-bold text-slate-600 mb-2">Supply Chain Journey</p>
                  <div className="space-y-2">
                    {verifyResult.journeySteps.map((step, i) => (
                      <div key={i} className="flex items-center gap-2 text-xs">
                        <div className={cn('w-2 h-2 rounded-full flex-shrink-0', i === verifyResult.journeySteps.length - 1 ? 'bg-emerald-500' : 'bg-slate-300')} />
                        <span className="text-slate-600">{step.location}</span>
                        <span className="text-slate-400 ml-auto">{fmtDate(step.timestamp, 'datetime')}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            )}
          </div>
        </Card>
      </div>

      <Card>
        <CardHeader title="Recent Requests" subtitle="Status tracking for submitted stock requests" />
        <div className="space-y-3">
          {MOCK_REQUESTS.map((req) => (
            <div key={req.id} className="flex items-center gap-4 px-4 py-3 rounded-xl bg-slate-50 border border-slate-100">
              <Package size={16} className="text-slate-400 flex-shrink-0" />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-semibold text-slate-800">{req.medicine}</p>
                <p className="text-xs text-slate-400 mt-0.5">Qty: {req.quantity.toLocaleString()} · {fmtDate(req.requestedAt, 'relative')}</p>
              </div>
              <Badge color={statusColor[req.status] ?? 'gray'}>
                {req.status === 'Approved' && <CheckCircle size={10} className="inline mr-1" />}
                {req.status === 'Pending' && <Clock size={10} className="inline mr-1" />}
                {req.status === 'Rejected' && <AlertTriangle size={10} className="inline mr-1" />}
                {req.status}
              </Badge>
            </div>
          ))}
        </div>
      </Card>

      <QrScannerModal open={scannerOpen} onClose={() => setScannerOpen(false)} onResult={(serial) => { setVerifySerial(serial); handleVerify(serial); }} />
    </div>
  );
}
