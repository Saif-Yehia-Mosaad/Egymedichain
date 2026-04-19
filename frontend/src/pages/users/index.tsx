import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi } from '../../api/endpoints';
import { Button, Badge, Skeleton, EmptyState } from '../../components/ui';
import { fmtDate } from '../../utils';
import type { SystemUser, UserRole } from '../../types';

const MOCK_USERS: SystemUser[] = [
  { id: 1, email: 'admin@moh.gov.eg', name: 'Dr. Amira Hassan', phone: '01001234567', role: 'Ministry', entityId: 0, entityName: 'Ministry of Health', isActive: true, createdAt: '2023-01-10' },
  { id: 2, email: 'mfr1@egypharma.eg', name: 'Eng. Khaled Mansour', phone: '01112345678', role: 'Manufacturer', entityId: 1, entityName: 'EgyPharma Co.', isActive: true, createdAt: '2023-03-15' },
  { id: 3, email: 'wh1@cairohub.eg', name: 'Ibrahim Saleh', phone: '01023456789', role: 'Warehouse', entityId: 2, entityName: 'Cairo Central Warehouse', isActive: true, createdAt: '2023-05-20' },
  { id: 4, email: 'ph1@nasrcity.eg', name: 'Dr. Sara Nabil', phone: '01234567890', role: 'Pharmacy', entityId: 3, entityName: 'Nasr City Pharmacy #1', isActive: true, createdAt: '2023-07-01' },
  { id: 5, email: 'ph2@giza.eg', name: 'Ahmed Fouad', phone: '01098765432', role: 'Pharmacy', entityId: 4, entityName: 'Giza Pharmacy #7', isActive: false, createdAt: '2023-09-12' },
];

const ROLE_COLORS: Record<UserRole, { badge: 'green' | 'blue' | 'orange' | 'purple' | 'gray'; gradient: string }> = {
  Ministry: { badge: 'purple', gradient: 'from-violet-600 to-blue-700' },
  Manufacturer: { badge: 'blue', gradient: 'from-blue-600 to-cyan-700' },
  Warehouse: { badge: 'orange', gradient: 'from-amber-600 to-orange-700' },
  Pharmacy: { badge: 'green', gradient: 'from-emerald-600 to-teal-700' },
  Consumer: { badge: 'gray', gradient: 'from-slate-500 to-slate-700' },
};

export function UsersPage() {
  const [showModal, setShowModal] = useState(false);
  const [newUser, setNewUser] = useState({ firstName: '', lastName: '', email: '', accessTier: 'Warehouse Administrator', node: 'Global Central Hub' });
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({ queryKey: ['users'], queryFn: () => usersApi.list(), retry: false });
  const users: SystemUser[] = data?.data ?? MOCK_USERS;

  const toggleMutation = useMutation({
    mutationFn: usersApi.toggleStatus,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['users'] }),
  });

  const stats = [
    { label: 'Authorized Users', value: '1,102', sub: 'Across all logistics nodes', color: '#191c1e' },
    { label: 'Active Today', value: '847', sub: 'In all global timezones', color: '#006e2f' },
    { label: 'Pending Access', value: '14', sub: 'Awaiting biometric verification', color: '#b45309' },
    { label: 'Revoked (30d)', value: '3', sub: 'Security-triggered revocations', color: '#ba1a1a' },
  ];

  return (
    <>
      <div className="space-y-6">
        <div>
          <h1 style={{ fontFamily: 'Manrope,sans-serif', fontWeight: 800, fontSize: '2rem', color: '#191c1e', letterSpacing: '-0.02em' }}>
            User Ecosystem
          </h1>
          <p className="mt-2" style={{ color: '#3d4a3d', fontSize: '15px' }}>
            Manage access protocols and logistics personnel authority.
          </p>
        </div>
        <button onClick={() => setShowModal(true)}
          className="flex items-center gap-2 px-5 py-2.5 rounded-xl text-white font-bold text-sm"
          style={{ fontFamily: 'Manrope,sans-serif', background: 'linear-gradient(135deg,#006e2f,#0058be)', boxShadow: '0 4px 16px rgba(0,110,47,0.22)' }}>
          <span className="material-symbols-outlined text-[18px]">person_add</span>
          Provision New User
        </button>

        {/* Stats */}
        <div className="grid grid-cols-4 gap-4">
          {stats.map(s => (
            <div key={s.label} className="card-clinical">
              <p className="text-[11px] uppercase tracking-widest font-bold mb-1" style={{ color: '#6d7b6c' }}>{s.label}</p>
              <p className="font-headline text-3xl font-black" style={{ fontFamily: 'Manrope,sans-serif', color: s.color }}>{s.value}</p>
              <p className="text-[12px] mt-1" style={{ color: '#6d7b6c' }}>{s.sub}</p>
            </div>
          ))}
        </div>

        {/* Table */}
        <div className="card-clinical overflow-hidden p-0">
          <table className="w-full">
            <thead>
              <tr style={{ background: '#f7f9fb', borderBottom: '1px solid rgba(188,203,185,0.15)' }}>
                {['User', 'Access Tier', 'Node Assignment', 'Last Active', 'Status', ''].map(h => (
                  <th key={h} className="text-left px-5 py-3.5 text-[11px] font-bold uppercase tracking-widest" style={{ color: '#6d7b6c' }}>{h}</th>
                ))}
              </tr>
            </thead>
            <tbody style={{ borderCollapse: 'collapse' }}>
              {isLoading ? Array.from({ length: 5 }).map((_, i) => (
                <tr key={i}><td colSpan={6} className="px-5 py-4"><Skeleton className="h-8 w-full" /></td></tr>
              )) : users.map(u => {
                const rc = ROLE_COLORS[u.role];
                const initials = u.name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
                return (
                  <tr key={u.id} className="transition-colors" style={{ borderBottom: '1px solid rgba(188,203,185,0.08)' }}>
                    <td className="px-5 py-3">
                      <div className="flex items-center gap-3">
                        <div className={`w-8 h-8 rounded-full bg-gradient-to-br ${rc.gradient} flex items-center justify-center text-white font-black text-[11px]`} style={{ fontFamily: 'Manrope,sans-serif' }}>
                          {initials}
                        </div>
                        <div>
                          <p className="font-semibold text-[13px]" style={{ color: '#191c1e' }}>{u.name}</p>
                          <p className="text-[11px]" style={{ color: '#6d7b6c' }}>{u.email}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-5 py-3">
                      <Badge color={rc.badge}>{u.role}</Badge>
                    </td>
                    <td className="px-5 py-3 text-[13px]" style={{ color: '#6d7b6c' }}>{u.entityName}</td>
                    <td className="px-5 py-3 text-[13px]" style={{ color: '#6d7b6c' }}>{fmtDate(u.createdAt, 'relative')}</td>
                    <td className="px-5 py-3">
                      <div className="flex items-center gap-1.5">
                        <div className="w-2 h-2 rounded-full" style={{ background: u.isActive ? '#006e2f' : '#6d7b6c' }} />
                        <span className="text-[12px] font-medium" style={{ color: u.isActive ? '#006e2f' : '#6d7b6c' }}>
                          {u.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </div>
                    </td>
                    <td className="px-5 py-3">
                      <button onClick={() => toggleMutation.mutate(u.id)}
                        className="text-[12px] font-bold hover:underline"
                        style={{ color: u.isActive ? '#ba1a1a' : '#006e2f' }}>
                        {u.isActive ? 'Revoke' : 'Activate'}
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
          <div className="flex justify-between items-center px-5 py-3" style={{ borderTop: '1px solid rgba(188,203,185,0.1)', background: '#f7f9fb' }}>
            <p className="text-[12px]" style={{ color: '#6d7b6c' }}>Showing 1-15 of 1,102 users</p>
            <div className="flex items-center gap-1">
              {[1, 2, 3].map(n => (
                <button key={n} className="w-8 h-8 rounded-lg flex items-center justify-center text-[13px] font-medium transition-colors"
                  style={{ background: n === 1 ? '#006e2f' : 'transparent', color: n === 1 ? 'white' : '#6d7b6c' }}>
                  {n}
                </button>
              ))}
              <button className="px-3 py-1.5 text-[12px] font-medium" style={{ color: '#6d7b6c' }}>Next →</button>
            </div>
          </div>
        </div>
      </div>

      {/* ─── New Protocol Provisioning Modal ─────────────────── */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center" style={{ background: 'rgba(25,28,30,0.35)', backdropFilter: 'blur(4px)' }}>
          <div className="relative bg-white rounded-2xl w-full max-w-[560px] mx-4" style={{ boxShadow: '0 24px 80px rgba(0,0,0,0.15)' }}>
            {/* Header */}
            <div className="flex items-start justify-between p-8 pb-6">
              <div>
                <h2 style={{ fontFamily: 'Manrope,sans-serif', fontWeight: 900, fontSize: '1.375rem', color: '#191c1e', letterSpacing: '-0.01em' }}>
                  New Protocol Provisioning
                </h2>
                <p className="mt-1 text-sm" style={{ color: '#6d7b6c' }}>Add a new authenticated user to the BioChain ecosystem.</p>
              </div>
              <button onClick={() => setShowModal(false)}
                className="w-8 h-8 flex items-center justify-center rounded-full transition-colors mt-0.5"
                style={{ background: '#f2f4f6', color: '#6d7b6c' }}>
                <span className="material-symbols-outlined text-[18px]">close</span>
              </button>
            </div>

            <div className="px-8 pb-8 space-y-5">
              {/* First + Last Name */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-[11px] font-bold uppercase tracking-widest mb-2" style={{ color: '#6d7b6c' }}>First Name</label>
                  <input
                    value={newUser.firstName}
                    onChange={e => setNewUser(p => ({ ...p, firstName: e.target.value }))}
                    placeholder="e.g. Julian"
                    className="w-full rounded-xl px-4 py-3 text-sm outline-none transition-all"
                    style={{ background: '#f2f4f6', border: '1px solid #eceef0', color: '#191c1e' }}
                    onFocus={e => { e.target.style.background = 'white'; e.target.style.boxShadow = '0 0 0 3px rgba(0,88,190,0.12)'; }}
                    onBlur={e => { e.target.style.background = '#f2f4f6'; e.target.style.boxShadow = 'none'; }}
                  />
                </div>
                <div>
                  <label className="block text-[11px] font-bold uppercase tracking-widest mb-2" style={{ color: '#6d7b6c' }}>Last Name</label>
                  <input
                    value={newUser.lastName}
                    onChange={e => setNewUser(p => ({ ...p, lastName: e.target.value }))}
                    placeholder="e.g. Sterling"
                    className="w-full rounded-xl px-4 py-3 text-sm outline-none transition-all"
                    style={{ background: '#f2f4f6', border: '1px solid #eceef0', color: '#191c1e' }}
                    onFocus={e => { e.target.style.background = 'white'; e.target.style.boxShadow = '0 0 0 3px rgba(0,88,190,0.12)'; }}
                    onBlur={e => { e.target.style.background = '#f2f4f6'; e.target.style.boxShadow = 'none'; }}
                  />
                </div>
              </div>

              {/* Email */}
              <div>
                <label className="block text-[11px] font-bold uppercase tracking-widest mb-2" style={{ color: '#6d7b6c' }}>Authorized Email</label>
                <input
                  value={newUser.email}
                  onChange={e => setNewUser(p => ({ ...p, email: e.target.value }))}
                  placeholder="j.sterling@biochain.io"
                  type="email"
                  className="w-full rounded-xl px-4 py-3 text-sm outline-none transition-all"
                  style={{ background: '#f2f4f6', border: '1px solid #eceef0', color: '#191c1e' }}
                  onFocus={e => { e.target.style.background = 'white'; e.target.style.boxShadow = '0 0 0 3px rgba(0,88,190,0.12)'; }}
                  onBlur={e => { e.target.style.background = '#f2f4f6'; e.target.style.boxShadow = 'none'; }}
                />
              </div>

              {/* Access Tier + Node */}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-[11px] font-bold uppercase tracking-widest mb-2" style={{ color: '#6d7b6c' }}>Access Tier</label>
                  <div className="relative">
                    <select
                      value={newUser.accessTier}
                      onChange={e => setNewUser(p => ({ ...p, accessTier: e.target.value }))}
                      className="w-full rounded-xl px-4 py-3 text-sm outline-none appearance-none cursor-pointer"
                      style={{ background: '#f2f4f6', border: '1px solid #eceef0', color: '#191c1e', paddingRight: '2.5rem' }}>
                      <option>Warehouse Administrator</option>
                      <option>Logistics Lead</option>
                      <option>Pharmacy Admin</option>
                      <option>Auditor</option>
                      <option>Ministry Access</option>
                    </select>
                    <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                      <span className="material-symbols-outlined text-[18px]" style={{ color: '#6d7b6c' }}>expand_more</span>
                    </div>
                  </div>
                </div>
                <div>
                  <label className="block text-[11px] font-bold uppercase tracking-widest mb-2" style={{ color: '#6d7b6c' }}>Node Assignment</label>
                  <div className="relative">
                    <select
                      value={newUser.node}
                      onChange={e => setNewUser(p => ({ ...p, node: e.target.value }))}
                      className="w-full rounded-xl px-4 py-3 text-sm outline-none appearance-none cursor-pointer"
                      style={{ background: '#f2f4f6', border: '1px solid #eceef0', color: '#191c1e', paddingRight: '2.5rem' }}>
                      <option>Global Central Hub</option>
                      <option>North Bio-Hub Alpha</option>
                      <option>Pacific Distribution Center</option>
                      <option>Cairo Central Warehouse</option>
                    </select>
                    <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                      <span className="material-symbols-outlined text-[18px]" style={{ color: '#6d7b6c' }}>expand_more</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Security note */}
              <div className="flex items-start gap-3 p-4 rounded-xl" style={{ background: 'rgba(0,110,47,0.06)', border: '1px solid rgba(0,110,47,0.12)' }}>
                <span className="material-symbols-outlined text-[18px] flex-shrink-0 mt-0.5" style={{ color: '#006e2f' }}>security</span>
                <p className="text-[13px] leading-relaxed" style={{ color: '#3d4a3d' }}>
                  A secure cryptographic invitation will be dispatched to the provided email. The user must complete biometric verification within 24 hours to finalize activation.
                </p>
              </div>

              {/* Buttons */}
              <div className="flex gap-3 pt-1">
                <button onClick={() => setShowModal(false)}
                  className="flex-1 py-3.5 rounded-xl font-bold text-sm transition-all border"
                  style={{ fontFamily: 'Manrope,sans-serif', background: 'white', color: '#191c1e', borderColor: '#eceef0' }}>
                  Cancel
                </button>
                <button
                  className="flex-1 py-3.5 rounded-xl font-bold text-sm text-white transition-all"
                  style={{ fontFamily: 'Manrope,sans-serif', background: 'linear-gradient(135deg,#006e2f,#0058be)', boxShadow: '0 4px 16px rgba(0,110,47,0.22)' }}>
                  Execute Provisioning
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}