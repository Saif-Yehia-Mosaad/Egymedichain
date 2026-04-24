import { useQuery } from '@tanstack/react-query';
import {
  AreaChart, Area, BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, PieChart, Pie, Cell,
} from 'recharts';
import {
  Package, AlertTriangle, ArrowLeftRight, ShieldX,
  Store, Warehouse, TrendingUp, Activity,
} from 'lucide-react';
import { analyticsApi } from '../../api/endpoints';
import { Card, CardHeader, Skeleton, SkeletonCard, Badge } from '../../components/ui';
import { fmtNumber, fmtDate } from '../../utils';
import type { DashboardStats } from '../../types';

// ─── Mock data shown while API isn't connected ─────────────────
const MOCK: DashboardStats = {
  serializedUnits: 8420000,
  lowStockMedicines: 27,
  transfersToday: 1284,
  counterfeitBlocks: 143,
  activePharmacies: 61200,
  activeWarehouses: 892,
  stockByGovernorate: [
    { name: 'Cairo', coverage: 88 },
    { name: 'Alexandria', coverage: 71 },
    { name: 'Giza', coverage: 65 },
    { name: 'Aswan', coverage: 42 },
    { name: 'Assiut', coverage: 38 },
    { name: 'Luxor', coverage: 55 },
  ],
  transferTrend: Array.from({ length: 14 }, (_, i) => ({
    date: new Date(Date.now() - (13 - i) * 86400000).toLocaleDateString('en', { month: 'short', day: 'numeric' }),
    count: Math.floor(900 + Math.random() * 600),
  })),
  alertsByType: [
    { type: 'Low Stock', count: 27 },
    { type: 'Expiry', count: 14 },
    { type: 'Suspicious', count: 8 },
    { type: 'Counterfeit', count: 3 },
  ],
  recentActivity: [
    { id: 1, type: 'transfer', message: 'Batch MED-2024-0892 transferred from Cairo Warehouse → Nasr City Pharmacy', timestamp: new Date(Date.now() - 180000).toISOString(), severity: undefined },
    { id: 2, type: 'alert', message: 'Low stock alert: Amoxicillin 500mg at Alexandria Central', timestamp: new Date(Date.now() - 600000).toISOString(), severity: 'High' },
    { id: 3, type: 'registration', message: 'New medicine registered: Paracetamol 1g by EgyPharma', timestamp: new Date(Date.now() - 3600000).toISOString(), severity: undefined },
    { id: 4, type: 'verification', message: 'QR verified: SN-00284-EGM at Giza Pharmacy #442', timestamp: new Date(Date.now() - 7200000).toISOString(), severity: undefined },
    { id: 5, type: 'alert', message: 'Suspicious transfer pattern detected: Batch B-2024-0033', timestamp: new Date(Date.now() - 10800000).toISOString(), severity: 'Critical' },
  ],
};

const PIE_COLORS = ['#f97316', '#22c55e', '#3b82f6', '#ef4444'];

export function DashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['dashboard'],
    queryFn: analyticsApi.getDashboard,
    retry: false,
    staleTime: 60_000,
  });

  const stats = data ?? MOCK;

  const kpis = [
    { label: 'Serialized Units', value: fmtNumber(stats.serializedUnits), icon: <Package size={20} />, color: 'from-emerald-400 to-emerald-600', bg: 'from-emerald-50 to-emerald-100/50', trend: '+12%' },
    { label: 'Low Stock Medicines', value: fmtNumber(stats.lowStockMedicines), icon: <AlertTriangle size={20} />, color: 'from-orange-400 to-orange-600', bg: 'from-orange-50 to-orange-100/50', trend: '−3 today' },
    { label: 'Transfers Today', value: fmtNumber(stats.transfersToday), icon: <ArrowLeftRight size={20} />, color: 'from-blue-400 to-blue-600', bg: 'from-blue-50 to-blue-100/50', trend: '+8%' },
    { label: 'Counterfeit Blocks', value: fmtNumber(stats.counterfeitBlocks), icon: <ShieldX size={20} />, color: 'from-red-400 to-red-600', bg: 'from-red-50 to-red-100/50', trend: 'This month' },
    { label: 'Active Pharmacies', value: fmtNumber(stats.activePharmacies), icon: <Store size={20} />, color: 'from-violet-400 to-violet-600', bg: 'from-violet-50 to-violet-100/50', trend: '98.2% online' },
    { label: 'Active Warehouses', value: fmtNumber(stats.activeWarehouses), icon: <Warehouse size={20} />, color: 'from-cyan-400 to-cyan-600', bg: 'from-cyan-50 to-cyan-100/50', trend: 'All regions' },
  ];

  const activityIcon: Record<string, React.ReactNode> = {
    transfer: <ArrowLeftRight size={13} className="text-blue-500" />,
    alert: <AlertTriangle size={13} className="text-orange-500" />,
    registration: <Package size={13} className="text-emerald-500" />,
    verification: <ShieldX size={13} className="text-violet-500" />,
  };

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-slate-800">Ministry Dashboard</h1>
          <p className="text-slate-400 text-sm mt-0.5">National pharmaceutical supply chain overview</p>
        </div>
        <div className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-emerald-50 border border-emerald-200">
          <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
          <span className="text-xs font-semibold text-emerald-700">Live</span>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-6 gap-3 lg:gap-4">
        {isLoading
          ? Array.from({ length: 6 }).map((_, i) => <SkeletonCard key={i} />)
          : kpis.map((kpi) => (
            <div key={kpi.label} className={`rounded-2xl bg-gradient-to-br ${kpi.bg} border border-white p-5 shadow-sm hover:shadow-md transition-shadow`}>
              <div className={`inline-flex items-center justify-center w-9 h-9 rounded-xl bg-gradient-to-br ${kpi.color} text-white mb-3 shadow-lg`}>
                {kpi.icon}
              </div>
              <p className="text-2xl font-black text-slate-800">{kpi.value}</p>
              <p className="text-xs text-slate-500 font-medium mt-0.5">{kpi.label}</p>
              <p className="text-[11px] text-slate-400 mt-1">{kpi.trend}</p>
            </div>
          ))}
      </div>

      {/* Charts row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <Card className="lg:col-span-2">
          <CardHeader title="Transfer Activity" subtitle="Daily transfers — last 14 days" action={
            <Badge color="blue"><TrendingUp size={10} className="inline mr-1" />Trending up</Badge>
          } />
          {isLoading ? <Skeleton className="h-52" /> : (
            <ResponsiveContainer width="100%" height={200}>
              <AreaChart data={stats.transferTrend} margin={{ top: 5, right: 5, left: -30, bottom: 0 }}>
                <defs>
                  <linearGradient id="tGrad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#22c55e" stopOpacity={0.3} />
                    <stop offset="95%" stopColor="#22c55e" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                <XAxis dataKey="date" tick={{ fontSize: 10, fill: '#94a3b8' }} tickLine={false} />
                <YAxis tick={{ fontSize: 10, fill: '#94a3b8' }} tickLine={false} axisLine={false} />
                <Tooltip contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 20px rgba(0,0,0,0.1)', fontSize: 12 }} />
                <Area type="monotone" dataKey="count" stroke="#22c55e" strokeWidth={2} fill="url(#tGrad)" dot={false} />
              </AreaChart>
            </ResponsiveContainer>
          )}
        </Card>

        {/* Alerts by type */}
        <Card>
          <CardHeader title="Alerts by Type" subtitle="Active alerts breakdown" />
          {isLoading ? <Skeleton className="h-52" /> : (
            <ResponsiveContainer width="100%" height={160}>
              <PieChart>
                <Pie data={stats.alertsByType} dataKey="count" nameKey="type" cx="50%" cy="50%" innerRadius={45} outerRadius={70} paddingAngle={3}>
                  {stats.alertsByType.map((_, i) => (
                    <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip contentStyle={{ borderRadius: '12px', border: 'none', fontSize: 12 }} />
              </PieChart>
            </ResponsiveContainer>
          )}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {stats.alertsByType.map((a, i) => (
              <div key={a.type} className="flex items-center gap-1.5">
                <span className="w-2 h-2 rounded-full flex-shrink-0" style={{ background: PIE_COLORS[i % PIE_COLORS.length] }} />
                <span className="text-[11px] text-slate-500 truncate">{a.type}</span>
                <span className="text-[11px] font-bold text-slate-700 ml-auto">{a.count}</span>
              </div>
            ))}
          </div>
        </Card>
      </div>

      {/* Stock by Governorate + Activity Feed */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader title="Stock Coverage by Governorate" subtitle="% of pharmacies adequately stocked" />
          {isLoading ? <Skeleton className="h-48" /> : (
            <ResponsiveContainer width="100%" height={200}>
              <BarChart data={stats.stockByGovernorate} margin={{ top: 5, right: 5, left: -30, bottom: 0 }}>
                <defs>
                  <linearGradient id="bGrad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.9} />
                    <stop offset="95%" stopColor="#22c55e" stopOpacity={0.9} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                <XAxis dataKey="name" tick={{ fontSize: 10, fill: '#94a3b8' }} tickLine={false} />
                <YAxis tick={{ fontSize: 10, fill: '#94a3b8' }} tickLine={false} axisLine={false} domain={[0, 100]} />
                <Tooltip formatter={(v) => [`${v}%`, 'Coverage']} contentStyle={{ borderRadius: '12px', border: 'none', fontSize: 12 }} />
                <Bar dataKey="coverage" fill="url(#bGrad)" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </Card>

        {/* Activity feed */}
        <Card>
          <CardHeader title="Recent Activity" subtitle="Real-time system events" action={
            <span className="flex items-center gap-1.5 text-xs text-emerald-600 font-semibold">
              <Activity size={12} /> Live
            </span>
          } />
          <div className="space-y-3">
            {isLoading
              ? Array.from({ length: 5 }).map((_, i) => <div key={i} className="flex gap-3"><Skeleton className="w-6 h-6 rounded-lg" /><div className="flex-1 space-y-1.5"><Skeleton className="h-3 w-full" /><Skeleton className="h-2 w-1/3" /></div></div>)
              : stats.recentActivity.map((item) => (
                <div key={item.id} className="flex items-start gap-3 group">
                  <div className="flex-shrink-0 w-6 h-6 rounded-lg bg-slate-100 flex items-center justify-center group-hover:bg-slate-200 transition-colors">
                    {activityIcon[item.type]}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs text-slate-700 leading-relaxed">{item.message}</p>
                    <p className="text-[11px] text-slate-400 mt-0.5">{fmtDate(item.timestamp, 'relative')}</p>
                  </div>
                  {item.severity && (
                    <Badge color={item.severity === 'Critical' ? 'red' : 'orange'} className="flex-shrink-0 text-[10px]">
                      {item.severity}
                    </Badge>
                  )}
                </div>
              ))}
          </div>
        </Card>
      </div>
    </div>
  );
}
