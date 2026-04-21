import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard, Pill, Package,
  Bell, Settings, ArrowLeftRight,
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

const BOTTOM_ITEMS = [
  { path: '/dashboard',    icon: <LayoutDashboard size={20} />, label: 'Home' },
  { path: '/medicines',    icon: <Pill size={20} />,            label: 'Meds' },
  { path: '/inventory',    icon: <Package size={20} />,         label: 'Stock' },
  { path: '/transactions', icon: <ArrowLeftRight size={20} />,  label: 'Txns' },
  { path: '/alerts',       icon: <Bell size={20} />,            label: 'Alerts' },
  { path: '/settings',     icon: <Settings size={20} />,        label: 'Settings' },
];

export function BottomNav({ alertCount = 0 }: { alertCount?: number }) {
  return (
    <nav
      className="lg:hidden fixed bottom-0 left-0 right-0 z-30 flex items-center justify-around px-1 py-2"
      style={{
        background:   'var(--bg-nav)',
        backdropFilter: 'blur(20px)',
        borderTop:    '1px solid var(--border)',
        boxShadow:    '0 -4px 20px rgba(0,0,0,0.06)',
      }}
    >
      {BOTTOM_ITEMS.map(item => (
        <NavLink
          key={item.path}
          to={item.path}
          className={({ isActive }) =>
            `flex flex-col items-center gap-0.5 px-2 py-1.5 rounded-xl transition-all relative ${
              isActive
                ? 'text-[#006e2f]'
                : 'text-[var(--text-muted)]'
            }`
          }
        >
          {({ isActive }) => (
            <>
              <div className={`relative p-1.5 rounded-xl transition-all ${isActive ? 'bg-[rgba(0,110,47,0.1)]' : ''}`}>
                {item.icon}
                {item.path === '/alerts' && alertCount > 0 && (
                  <span className="absolute -top-0.5 -right-0.5 w-4 h-4 rounded-full bg-[#ba1a1a] text-white text-[9px] font-black flex items-center justify-center border-2 border-[var(--bg-nav)]">
                    {alertCount > 9 ? '9+' : alertCount}
                  </span>
                )}
              </div>
              <span className={`text-[10px] font-semibold transition-all ${isActive ? 'text-[#006e2f]' : 'text-[var(--text-muted)]'}`}>
                {item.label}
              </span>
            </>
          )}
        </NavLink>
      ))}
    </nav>
  );
}