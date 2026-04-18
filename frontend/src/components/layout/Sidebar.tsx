import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard, Pill, Package, Warehouse, Store,
  ArrowLeftRight, Bell, Users, ClipboardList, Settings,
  ChevronLeft, ChevronRight, Activity, ShieldCheck,
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { cn } from '../../utils';

interface NavItem {
  path: string;
  label: string;
  icon: React.ReactNode;
  roles?: string[];
}

const NAV_ITEMS: NavItem[] = [
  { path: '/dashboard', label: 'Dashboard', icon: <LayoutDashboard size={18} /> },
  { path: '/medicines', label: 'Medicines', icon: <Pill size={18} />, roles: ['Ministry', 'Manufacturer', 'Warehouse'] },
  { path: '/inventory', label: 'Inventory', icon: <Package size={18} /> },
  { path: '/warehouses', label: 'Warehouses', icon: <Warehouse size={18} />, roles: ['Ministry', 'Warehouse'] },
  { path: '/pharmacy', label: 'Pharmacy', icon: <Store size={18} />, roles: ['Ministry', 'Pharmacy'] },
  { path: '/transactions', label: 'Transactions', icon: <ArrowLeftRight size={18} /> },
  { path: '/alerts', label: 'Alerts', icon: <Bell size={18} /> },
  { path: '/users', label: 'Users', icon: <Users size={18} />, roles: ['Ministry'] },
  { path: '/audit', label: 'Audit Logs', icon: <ClipboardList size={18} />, roles: ['Ministry'] },
  { path: '/settings', label: 'Settings', icon: <Settings size={18} /> },
];

interface SidebarProps {
  alertCount?: number;
}

export function Sidebar({ alertCount = 0 }: SidebarProps) {
  const [collapsed, setCollapsed] = useState(false);
  const { user } = useAuth();

  const visibleItems = NAV_ITEMS.filter(
    (item) => !item.roles || (user && item.roles.includes(user.role))
  );

  return (
    <aside className={cn(
      'relative flex flex-col h-full bg-white border-r border-slate-200/80 transition-all duration-300',
      collapsed ? 'w-16' : 'w-60'
    )}>
      {/* Logo */}
      <div className={cn('flex items-center gap-3 px-4 py-5 border-b border-slate-100', collapsed && 'justify-center px-0')}>
        <div className="flex-shrink-0 w-8 h-8 rounded-xl bg-gradient-to-br from-emerald-500 to-blue-500 flex items-center justify-center shadow-md shadow-emerald-200">
          <ShieldCheck size={16} className="text-white" />
        </div>
        {!collapsed && (
          <div className="overflow-hidden">
            <p className="text-sm font-extrabold text-slate-800 leading-tight whitespace-nowrap">EgyMediChain</p>
            <p className="text-[10px] text-slate-400 whitespace-nowrap">Supply Chain Gov.</p>
          </div>
        )}
      </div>

      {/* Backend health dot */}
      {!collapsed && (
        <div className="mx-4 mt-3 mb-1 flex items-center gap-2 px-3 py-2 rounded-xl bg-slate-50 border border-slate-100">
          <Activity size={12} className="text-slate-400" />
          <span className="text-[11px] text-slate-400">Connecting to API…</span>
        </div>
      )}

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto py-3 space-y-0.5 px-2">
        {visibleItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-150 relative group',
                isActive
                  ? 'bg-gradient-to-r from-emerald-500 to-emerald-600 text-white shadow-md shadow-emerald-200'
                  : 'text-slate-500 hover:bg-slate-100 hover:text-slate-800',
                collapsed && 'justify-center px-0'
              )
            }
          >
            <span className="flex-shrink-0">{item.icon}</span>
            {!collapsed && <span className="truncate">{item.label}</span>}
            {!collapsed && item.path === '/alerts' && alertCount > 0 && (
              <span className="ml-auto bg-red-500 text-white text-[10px] font-bold rounded-full px-1.5 py-0.5 min-w-[18px] text-center">
                {alertCount > 99 ? '99+' : alertCount}
              </span>
            )}
            {collapsed && (
              <span className="absolute left-14 bg-slate-800 text-white text-xs px-2 py-1 rounded-lg opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap z-50 pointer-events-none">
                {item.label}
              </span>
            )}
          </NavLink>
        ))}
      </nav>

      {/* User info */}
      {!collapsed && user && (
        <div className="m-2 p-3 rounded-xl bg-slate-50 border border-slate-100">
          <p className="text-xs font-semibold text-slate-700 truncate">{user.name}</p>
          <p className="text-[11px] text-slate-400 truncate">{user.email}</p>
          <span className="inline-block mt-1 text-[10px] font-bold px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-700">
            {user.role}
          </span>
        </div>
      )}

      {/* Collapse toggle */}
      <button
        onClick={() => setCollapsed((c) => !c)}
        className="absolute -right-3 top-20 w-6 h-6 rounded-full border border-slate-200 bg-white shadow-sm flex items-center justify-center text-slate-400 hover:text-slate-600 transition-colors z-10"
      >
        {collapsed ? <ChevronRight size={12} /> : <ChevronLeft size={12} />}
      </button>
    </aside>
  );
}
