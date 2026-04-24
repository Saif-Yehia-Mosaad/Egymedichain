import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard, Pill, Package, Warehouse, Store,
  ArrowLeftRight, Bell, Users, ClipboardList, Settings,
  ChevronLeft, ChevronRight, ShieldCheck,
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

const NAV_ITEMS = [
  { path: '/dashboard',    label: 'Dashboard',    icon: LayoutDashboard },
  { path: '/medicines',    label: 'Medicines',    icon: Pill,           roles: ['Ministry','Manufacturer','Warehouse'] },
  { path: '/inventory',    label: 'Inventory',    icon: Package },
  { path: '/warehouses',   label: 'Warehouses',   icon: Warehouse,      roles: ['Ministry','Warehouse'] },
  { path: '/pharmacy',     label: 'Pharmacy',     icon: Store,          roles: ['Ministry','Pharmacy'] },
  { path: '/transactions', label: 'Transactions', icon: ArrowLeftRight },
  { path: '/alerts',       label: 'Alerts',       icon: Bell },
  { path: '/users',        label: 'Users',        icon: Users,          roles: ['Ministry'] },
  { path: '/audit',        label: 'Audit Logs',   icon: ClipboardList, roles: ['Ministry'] },
  { path: '/settings',     label: 'Settings',     icon: Settings },
];

export function Sidebar({
  alertCount = 0,
  mobileOpen = false,
  onMobileClose,
}: {
  alertCount?: number;
  mobileOpen?: boolean;
  onMobileClose?: () => void;
}) {
  const [collapsed, setCollapsed] = useState(false);
  const { user } = useAuth();

  const items = NAV_ITEMS.filter(i =>
    !i.roles || (user && i.roles.includes(user.role))
  );

  const initials = user?.name
    ?.split(' ').map((w: string) => w[0]).slice(0, 2).join('').toUpperCase() ?? 'U';

  const roleColors: Record<string, string> = {
    Ministry:     '#818cf8',
    Manufacturer: '#38bdf8',
    Warehouse:    '#fbbf24',
    Pharmacy:     '#10d9a0',
  };
  const roleColor = roleColors[user?.role ?? ''] ?? '#10d9a0';

  return (
    <aside
      className={`sidebar${collapsed ? ' collapsed' : ''}${mobileOpen ? ' mobile-open' : ''}`}
    >
      {/* ── Logo ── */}
      <div style={{
        display: 'flex',
        alignItems: 'center',
        gap: collapsed ? 0 : 10,
        padding: collapsed ? '18px 0' : '18px 16px',
        justifyContent: collapsed ? 'center' : 'flex-start',
        borderBottom: '1px solid var(--bd)',
        flexShrink: 0,
      }}>
        <div style={{
          width: 34, height: 34,
          borderRadius: 10,
          background: 'linear-gradient(135deg,#10d9a0,#0aaf82)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          flexShrink: 0,
          boxShadow: '0 0 16px rgba(16,217,160,.3)',
        }}>
          <ShieldCheck size={17} color="#07120e" />
        </div>
        {!collapsed && (
          <div style={{ overflow: 'hidden' }}>
            <div style={{
              fontFamily: 'Syne, sans-serif',
              fontWeight: 800,
              fontSize: 15,
              color: 'var(--tx)',
              whiteSpace: 'nowrap',
              letterSpacing: '-.02em',
            }}>EgyMediChain</div>
            <div style={{
              fontSize: 9.5,
              color: 'var(--tx-3)',
              textTransform: 'uppercase',
              letterSpacing: '.08em',
              fontWeight: 600,
              whiteSpace: 'nowrap',
              marginTop: 1,
            }}>Supply Chain Gov.</div>
          </div>
        )}
      </div>

      {/* ── Nav ── */}
      <nav style={{
        flex: 1,
        overflowY: 'auto',
        overflowX: 'hidden',
        padding: '12px 8px',
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
      }}>
        {items.map(item => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.path}
              to={item.path}
              onClick={onMobileClose}
              title={collapsed ? item.label : undefined}
              className={({ isActive }) =>
                `nav-item${isActive ? ' active' : ''}`
              }
              style={{ justifyContent: collapsed ? 'center' : 'flex-start', position: 'relative' }}
            >
              <Icon size={16} className="nav-icon" style={{ flexShrink: 0 }} />
              {!collapsed && (
                <span style={{ flex: 1, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                  {item.label}
                </span>
              )}
              {!collapsed && item.path === '/alerts' && alertCount > 0 && (
                <span style={{
                  background: '#f87171',
                  color: '#fff',
                  fontSize: 10,
                  fontWeight: 700,
                  borderRadius: 99,
                  padding: '1px 6px',
                  minWidth: 18,
                  textAlign: 'center',
                }}>
                  {alertCount > 99 ? '99+' : alertCount}
                </span>
              )}
              {/* Tooltip when collapsed */}
              {collapsed && (
                <span style={{
                  position: 'absolute',
                  left: 54,
                  background: 'var(--bg-5)',
                  color: 'var(--tx)',
                  fontSize: 12,
                  fontWeight: 500,
                  padding: '5px 10px',
                  borderRadius: 8,
                  whiteSpace: 'nowrap',
                  pointerEvents: 'none',
                  opacity: 0,
                  border: '1px solid var(--bd-2)',
                  zIndex: 100,
                  transition: 'opacity .15s',
                }}>
                  {item.label}
                </span>
              )}
            </NavLink>
          );
        })}
      </nav>

      {/* ── User chip ── */}
      {!collapsed && user && (
        <div style={{
          margin: '0 8px 12px',
          padding: '10px 12px',
          background: 'var(--bg-4)',
          borderRadius: 10,
          border: '1px solid var(--bd)',
          display: 'flex',
          alignItems: 'center',
          gap: 10,
          flexShrink: 0,
        }}>
          <div style={{
            width: 32, height: 32,
            borderRadius: 8,
            background: `${roleColor}22`,
            border: `1px solid ${roleColor}44`,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontFamily: 'Syne, sans-serif',
            fontWeight: 700,
            fontSize: 12,
            color: roleColor,
            flexShrink: 0,
          }}>
            {initials}
          </div>
          <div style={{ overflow: 'hidden', flex: 1 }}>
            <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--tx)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              {user.name}
            </div>
            <div style={{ fontSize: 10.5, color: roleColor, fontWeight: 500, whiteSpace: 'nowrap' }}>
              {user.role}
            </div>
          </div>
        </div>
      )}

      {/* ── Collapse toggle (desktop only) ── */}
      <button
        onClick={() => setCollapsed(c => !c)}
        style={{
          position: 'absolute',
          top: 70,
          right: -12,
          width: 24,
          height: 24,
          borderRadius: '50%',
          background: 'var(--bg-4)',
          border: '1px solid var(--bd-2)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          cursor: 'pointer',
          zIndex: 40,
          color: 'var(--tx-2)',
          transition: 'background .15s',
        }}
        onMouseEnter={e => (e.currentTarget.style.background = 'var(--bg-5)')}
        onMouseLeave={e => (e.currentTarget.style.background = 'var(--bg-4)')}
        className="[display:none] lg:[display:flex]"
      >
        {collapsed ? <ChevronRight size={12} /> : <ChevronLeft size={12} />}
      </button>
    </aside>
  );
}
