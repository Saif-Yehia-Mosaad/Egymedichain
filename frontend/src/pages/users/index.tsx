import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Search, UserCheck, UserX } from 'lucide-react';
import { usersApi } from '../../api/endpoints';
import { Card, CardHeader, Badge, Button, Skeleton, EmptyState } from '../../components/ui';
import { fmtDate, cn } from '../../utils';
import type { SystemUser, UserRole } from '../../types';

const MOCK_USERS: SystemUser[] = [
  { id: 1, email: 'admin@moh.gov.eg', name: 'Dr. Amira Hassan', phone: '01001234567', role: 'Ministry', entityId: 0, entityName: 'Ministry of Health', isActive: true, createdAt: '2023-01-10' },
  { id: 2, email: 'mfr1@egypharma.eg', name: 'Eng. Khaled Mansour', phone: '01112345678', role: 'Manufacturer', entityId: 1, entityName: 'EgyPharma Co.', isActive: true, createdAt: '2023-03-15' },
  { id: 3, email: 'wh1@cairohub.eg', name: 'Ibrahim Saleh', phone: '01023456789', role: 'Warehouse', entityId: 2, entityName: 'Cairo Central Warehouse', isActive: true, createdAt: '2023-05-20' },
  { id: 4, email: 'ph1@nasrcity.eg', name: 'Dr. Sara Nabil', phone: '01234567890', role: 'Pharmacy', entityId: 3, entityName: 'Nasr City Pharmacy #1', isActive: true, createdAt: '2023-07-01' },
  { id: 5, email: 'ph2@giza.eg', name: 'Ahmed Fouad', phone: '01098765432', role: 'Pharmacy', entityId: 4, entityName: 'Giza Pharmacy #7', isActive: false, createdAt: '2023-09-12' },
  { id: 6, email: 'mfr2@deltapharma.eg', name: 'Layla Mostafa', phone: '01187654321', role: 'Manufacturer', entityId: 5, entityName: 'Delta Pharma', isActive: true, createdAt: '2024-01-05' },
];

const ROLE_COLOR: Record<UserRole, 'green' | 'blue' | 'purple' | 'orange' | 'gray'> = {
  Ministry: 'purple',
  Manufacturer: 'blue',
  Warehouse: 'orange',
  Pharmacy: 'green',
  Consumer: 'gray',
};

const ROLE_INITIALS: Record<UserRole, string> = {
  Ministry: 'MN',
  Manufacturer: 'MF',
  Warehouse: 'WH',
  Pharmacy: 'PH',
  Consumer: 'CS',
};

const AVATAR_BG: Record<UserRole, string> = {
  Ministry: 'from-violet-400 to-violet-600',
  Manufacturer: 'from-blue-400 to-blue-600',
  Warehouse: 'from-orange-400 to-orange-600',
  Pharmacy: 'from-emerald-400 to-emerald-600',
  Consumer: 'from-slate-400 to-slate-600',
};

export function UsersPage() {
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState<UserRole | 'all'>('all');
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => usersApi.list(),
    retry: false,
  });

  const users: SystemUser[] = data?.data ?? MOCK_USERS;

  const toggleMutation = useMutation({
    mutationFn: usersApi.toggleStatus,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['users'] }),
  });

  const filtered = users.filter((u) => {
    const matchSearch = !search || u.name.toLowerCase().includes(search.toLowerCase()) || u.email.includes(search);
    const matchRole = roleFilter === 'all' || u.role === roleFilter;
    return matchSearch && matchRole;
  });

  const roleCounts = Object.entries(
    users.reduce((acc, u) => { acc[u.role] = (acc[u.role] ?? 0) + 1; return acc; }, {} as Record<string, number>)
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-slate-800">Users</h1>
          <p className="text-slate-400 text-sm mt-0.5">System accounts and role management</p>
        </div>
      </div>

      {/* Role summary */}
      <div className="flex flex-wrap gap-3">
        {roleCounts.map(([role, count]) => (
          <button
            key={role}
            onClick={() => setRoleFilter(roleFilter === role ? 'all' : role as UserRole)}
            className={cn(
              'flex items-center gap-2 px-3 py-2 rounded-xl text-sm font-semibold border transition-all',
              roleFilter === role ? 'bg-slate-800 text-white border-slate-800' : 'bg-white border-slate-200 text-slate-600 hover:border-slate-300'
            )}
          >
            <span>{role}</span>
            <span className={cn('text-xs px-1.5 py-0.5 rounded-full font-bold', roleFilter === role ? 'bg-white/20' : 'bg-slate-100')}>
              {count}
            </span>
          </button>
        ))}
      </div>

      {/* Search */}
      <div className="relative max-w-72">
        <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search by name or email…"
          className="w-full pl-9 pr-4 py-2.5 text-sm bg-white border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-400/30"
        />
      </div>

      {/* User cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        {isLoading
          ? Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="rounded-2xl border border-slate-200 bg-white p-5 flex gap-4">
              <Skeleton className="w-12 h-12 rounded-2xl flex-shrink-0" />
              <div className="flex-1 space-y-2"><Skeleton className="h-4 w-3/4" /><Skeleton className="h-3 w-1/2" /></div>
            </div>
          ))
          : filtered.length === 0 ? (
            <div className="col-span-full">
              <EmptyState title="No users found" description="Try adjusting the search or filter" />
            </div>
          ) : (
            filtered.map((user) => (
              <div key={user.id} className={cn('rounded-2xl border bg-white p-5 hover:shadow-md transition-all', !user.isActive && 'opacity-60')}>
                <div className="flex items-start gap-3">
                  <div className={cn('w-12 h-12 rounded-2xl bg-gradient-to-br flex items-center justify-center text-white text-sm font-black flex-shrink-0 shadow-md', AVATAR_BG[user.role])}>
                    {ROLE_INITIALS[user.role]}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-bold text-slate-800 truncate">{user.name}</p>
                    <p className="text-xs text-slate-400 truncate">{user.email}</p>
                    <div className="flex items-center gap-1.5 mt-1.5 flex-wrap">
                      <Badge color={ROLE_COLOR[user.role]}>{user.role}</Badge>
                      <Badge color={user.isActive ? 'green' : 'gray'}>{user.isActive ? 'Active' : 'Inactive'}</Badge>
                    </div>
                  </div>
                </div>

                <div className="mt-4 pt-4 border-t border-slate-100 space-y-1.5">
                  <p className="text-xs text-slate-400">Entity: <span className="font-semibold text-slate-600">{user.entityName}</span></p>
                  <p className="text-xs text-slate-400">Phone: <span className="font-semibold text-slate-600">{user.phone}</span></p>
                  <p className="text-xs text-slate-400">Joined: <span className="font-semibold text-slate-600">{fmtDate(user.createdAt)}</span></p>
                </div>

                <div className="mt-4 flex gap-2">
                  <Button
                    size="sm"
                    variant={user.isActive ? 'danger' : 'ghost'}
                    leftIcon={user.isActive ? <UserX size={12} /> : <UserCheck size={12} />}
                    isLoading={toggleMutation.isPending}
                    onClick={() => toggleMutation.mutate(user.id)}
                    className="flex-1 justify-center"
                  >
                    {user.isActive ? 'Deactivate' : 'Activate'}
                  </Button>
                </div>
              </div>
            ))
          )}
      </div>
    </div>
  );
}
