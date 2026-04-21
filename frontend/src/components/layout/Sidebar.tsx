import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard, Pill, Package, Warehouse, Store,
  ArrowLeftRight, Bell, Users, ClipboardList, Settings,
  ChevronLeft, ChevronRight, ShieldCheck, Menu, X,
} from 'lucide-react';
import { useAuth }  from '../../context/AuthContext';
import { cn }       from '../../utils';

const NAV_ITEMS = [
  { path:'/dashboard',    label:'Dashboard',    icon:<LayoutDashboard size={16} /> },
  { path:'/medicines',    label:'Medicines',    icon:<Pill size={16} />,           roles:['Ministry','Manufacturer','Warehouse'] },
  { path:'/inventory',    label:'Inventory',    icon:<Package size={16} /> },
  { path:'/warehouses',   label:'Warehouses',   icon:<Warehouse size={16} />,      roles:['Ministry','Warehouse'] },
  { path:'/pharmacy',     label:'Pharmacy',     icon:<Store size={16} />,          roles:['Ministry','Pharmacy'] },
  { path:'/transactions', label:'Transactions', icon:<ArrowLeftRight size={16} /> },
  { path:'/alerts',       label:'Alerts',       icon:<Bell size={16} /> },
  { path:'/users',        label:'Users',        icon:<Users size={16} />,          roles:['Ministry'] },
  { path:'/audit',        label:'Audit Logs',   icon:<ClipboardList size={16} />, roles:['Ministry'] },
  { path:'/settings',     label:'Settings',     icon:<Settings size={16} /> },
];

const ROLE_GRAD: Record<string, string> = {
  Ministry:'from-violet-600 to-blue-700', Manufacturer:'from-blue-600 to-cyan-700',
  Warehouse:'from-amber-600 to-orange-700', Pharmacy:'from-emerald-600 to-teal-700',
};

export function Sidebar({ alertCount = 0 }: { alertCount?: number }) {
  const [collapsed, setCollapsed]   = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const { user }    = useAuth();
  const items       = NAV_ITEMS.filter(i => !i.roles || (user && i.roles.includes(user.role)));
  const initials    = user?.name?.split(' ').map((w:string) => w[0]).slice(0,2).join('').toUpperCase() ?? 'U';

  /* ── shared nav content ────────────────────────────────────── */
  const NavContent = ({ onNavClick }: { onNavClick?: () => void }) => (
    <>
      {/* Logo */}
      <div className={cn(
        'flex items-center gap-3 px-4 py-4',
        'border-b border-[var(--border)]',
        collapsed && !onNavClick && 'justify-center px-2'
      )}>
        <div className="flex-shrink-0 w-8 h-8 rounded-xl bg-gradient-to-br from-[#006e2f] to-[#0058be] flex items-center justify-center shadow-[0_4px_10px_rgba(0,110,47,.3)]">
          <ShieldCheck size={16} className="text-white" />
        </div>
        {(!collapsed || onNavClick) && (
          <div className="overflow-hidden">
            <p className="font-black text-[14px] leading-tight whitespace-nowrap" style={{ fontFamily:'Manrope,sans-serif', color:'var(--text)' }}>
              EgyMediChain
            </p>
            <p className="text-[10px] font-semibold whitespace-nowrap uppercase tracking-widest" style={{ color:'var(--text-muted)' }}>
              Supply Chain Gov.
            </p>
          </div>
        )}
      </div>

      {/* Nav items */}
      <nav className="flex-1 overflow-y-auto py-3 px-2 space-y-0.5">
        {items.map(item => (
          <NavLink key={item.path} to={item.path}
            onClick={onNavClick}
            title={collapsed && !onNavClick ? item.label : undefined}
            className={({ isActive }) => cn(
              'flex items-center gap-3 px-3 py-2.5 rounded-xl text-[13px] font-medium transition-all duration-150 relative group',
              isActive
                ? 'bg-[var(--bg-card)] font-bold shadow-[var(--shadow-card)]'
                : 'hover:bg-[var(--bg-surface-2)]',
              (collapsed && !onNavClick) && 'justify-center px-0'
            )}
            style={({ isActive }) => ({ color: isActive ? '#006e2f' : 'var(--text-muted)' })}
          >
            <span className="flex-shrink-0">{item.icon}</span>
            {(!collapsed || onNavClick) && <span className="truncate flex-1">{item.label}</span>}
            {(!collapsed || onNavClick) && item.path === '/alerts' && alertCount > 0 && (
              <span className="ml-auto bg-[#ba1a1a] text-white text-[10px] font-black rounded-full px-1.5 py-0.5 min-w-[18px] text-center">
                {alertCount > 99 ? '99+' : alertCount}
              </span>
            )}
            {/* Tooltip when collapsed */}
            {collapsed && !onNavClick && (
              <div className="absolute left-12 px-2.5 py-1.5 rounded-lg opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap z-50 pointer-events-none text-xs font-medium"
                style={{ background:'var(--bg-card)', color:'var(--text)', boxShadow:'var(--shadow-card)', border:'1px solid var(--border)' }}>
                {item.label}
              </div>
            )}
          </NavLink>
        ))}
      </nav>

      {/* User chip */}
      {(!collapsed || onNavClick) && user && (
        <div className="m-3 p-3 rounded-xl flex items-center gap-2.5" style={{ background:'var(--bg-surface)' }}>
          <div className={cn('w-8 h-8 rounded-lg bg-gradient-to-br flex items-center justify-center text-white font-black text-[11px] flex-shrink-0', ROLE_GRAD[user.role] ?? 'from-slate-500 to-slate-700')}
            style={{ fontFamily:'Manrope,sans-serif' }}>
            {initials}
          </div>
          <div className="overflow-hidden min-w-0">
            <p className="text-[12px] font-bold truncate" style={{ color:'var(--text)' }}>{user.name}</p>
            <p className="text-[10px] truncate" style={{ color:'var(--text-muted)' }}>{user.role}</p>
          </div>
        </div>
      )}
    </>
  );

  return (
    <>
      {/* ── Mobile hamburger (top-left, only shows when drawer closed) ── */}
      {!drawerOpen && (
        <button
          className="lg:hidden fixed top-3 left-3 z-50 w-9 h-9 flex items-center justify-center rounded-xl transition-colors"
          style={{ background:'var(--bg-card)', boxShadow:'var(--shadow-card)', color:'var(--text)' }}
          onClick={() => setDrawerOpen(true)}
        >
          <Menu size={18} />
        </button>
      )}

      {/* ── Mobile overlay ── */}
      {drawerOpen && (
        <div className="lg:hidden fixed inset-0 z-40 bg-black/40 backdrop-blur-sm"
          onClick={() => setDrawerOpen(false)} />
      )}

      {/* ── Mobile drawer ── */}
      <aside
        className={cn(
          'lg:hidden fixed left-0 top-0 bottom-0 z-40 flex flex-col w-[220px] transition-transform duration-300',
        )}
        style={{
          background: 'var(--sidebar-bg)',
          borderRight: '1px solid var(--border)',
          transform: drawerOpen ? 'translateX(0)' : 'translateX(-100%)',
        }}
      >
        {/* Close btn */}
        <button
          onClick={() => setDrawerOpen(false)}
          className="absolute top-3 right-3 w-7 h-7 flex items-center justify-center rounded-lg transition-colors"
          style={{ background:'var(--bg-surface)', color:'var(--text-muted)' }}
        >
          <X size={14} />
        </button>
        <NavContent onNavClick={() => setDrawerOpen(false)} />
      </aside>

      {/* ── Desktop sidebar ── */}
      <aside
        className="hidden lg:flex flex-col h-full flex-shrink-0 relative transition-all duration-300"
        style={{
          width: collapsed ? '64px' : '210px',
          background: 'var(--sidebar-bg)',
          borderRight: '1px solid var(--border)',
        }}
      >
        <NavContent />

        {/* Collapse toggle */}
        <button
          onClick={() => setCollapsed(c => !c)}
          className="absolute -right-3 top-16 w-6 h-6 rounded-full flex items-center justify-center z-20 transition-colors"
          style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border)',
            color: 'var(--text-muted)',
            boxShadow: 'var(--shadow-card)',
          }}
        >
          {collapsed ? <ChevronRight size={12} /> : <ChevronLeft size={12} />}
        </button>
      </aside>
    </>
  );
}