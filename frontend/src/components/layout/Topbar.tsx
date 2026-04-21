import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, Bell, LogOut, User, Settings, ChevronDown, X, Sun, Moon } from 'lucide-react';
import { useAuth }  from '../../context/AuthContext';
import { useTheme } from '../../context/ThemeContext';
import { fmtDate }  from '../../utils';
import type { Alert } from '../../types';

const SEV_DOT: Record<string, string> = {
  Critical:'bg-[#ba1a1a]', High:'bg-[#b45309]', Medium:'bg-[#0058be]', Low:'bg-[#6d7b6c]',
};
const ROLE_GRAD: Record<string, string> = {
  Ministry:'from-violet-600 to-blue-700', Manufacturer:'from-blue-600 to-cyan-700',
  Warehouse:'from-amber-600 to-orange-700', Pharmacy:'from-emerald-600 to-teal-700',
};

export function Topbar({ alerts, onClearAlert }: { alerts: Alert[]; onClearAlert: (id: number) => void }) {
  const { user, logout } = useAuth();
  const { toggle, isDark } = useTheme();
  const navigate = useNavigate();
  const [profileOpen, setProfileOpen] = useState(false);
  const [notifOpen,   setNotifOpen]   = useState(false);
  const [search, setSearch] = useState('');
  const profileRef = useRef<HTMLDivElement>(null);
  const notifRef   = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const h = (e: MouseEvent) => {
      if (profileRef.current && !profileRef.current.contains(e.target as Node)) setProfileOpen(false);
      if (notifRef.current   && !notifRef.current.contains(e.target as Node))   setNotifOpen(false);
    };
    document.addEventListener('mousedown', h);
    return () => document.removeEventListener('mousedown', h);
  }, []);

  const unread   = alerts.filter(a => !a.isResolved).length;
  const initials = user?.name?.split(' ').map((w:string) => w[0]).slice(0,2).join('').toUpperCase() ?? 'U';

  const dropdownStyle = {
    background:  'var(--bg-card)',
    border:      '1px solid var(--border)',
    boxShadow:   '0 8px 40px rgba(0,0,0,0.18)',
    borderRadius:'16px',
  };

  return (
    <header className="h-14 flex items-center px-4 sm:px-6 gap-3 glass-nav flex-shrink-0 z-20 pl-14 lg:pl-6">

      {/* Search */}
      <div className="relative flex-1 max-w-sm">
        <Search size={15} className="absolute left-3.5 top-1/2 -translate-y-1/2" style={{ color:'var(--text-muted)' }} />
        <input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search medicines, batches, transactions…"
          className="w-full pl-10 pr-4 py-2 text-sm rounded-full outline-none transition-all"
          style={{
            background:  'var(--bg-surface)',
            color:       'var(--text)',
            border:      'none',
          }}
          onFocus={e => (e.target.style.boxShadow = '0 0 0 4px rgba(0,88,190,.12)')}
          onBlur={e  => (e.target.style.boxShadow = 'none')}
        />
      </div>

      <div className="flex items-center gap-1.5 ml-auto">

        {/* Dark mode toggle */}
        <button
          onClick={toggle}
          className="w-9 h-9 flex items-center justify-center rounded-full transition-colors"
          style={{ background:'var(--bg-surface)', color:'var(--text-muted)' }}
          title={isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode'}
        >
          {isDark ? <Sun size={16} /> : <Moon size={16} />}
        </button>

        {/* Notifications */}
        <div className="relative" ref={notifRef}>
          <button
            onClick={() => setNotifOpen(o => !o)}
            className="relative w-9 h-9 flex items-center justify-center rounded-full transition-colors"
            style={{ background:'var(--bg-surface)', color:'var(--text-muted)' }}
          >
            <Bell size={16} />
            {unread > 0 && (
              <span className="absolute -top-0.5 -right-0.5 w-4 h-4 rounded-full bg-[#ba1a1a] text-white text-[9px] font-black flex items-center justify-center border-2 border-[var(--bg-nav)] animate-pulse">
                {unread > 9 ? '9+' : unread}
              </span>
            )}
          </button>

          {notifOpen && (
            <div className="absolute right-0 top-11 w-80 z-50 overflow-hidden" style={dropdownStyle}>
              <div className="px-4 py-3 flex items-center justify-between" style={{ borderBottom:'1px solid var(--border)' }}>
                <p className="font-bold text-[14px]" style={{ fontFamily:'Manrope,sans-serif', color:'var(--text)' }}>Notifications</p>
                {unread > 0 && <span className="text-[11px] font-bold" style={{ color:'#006e2f' }}>{unread} new</span>}
              </div>
              <div className="max-h-72 overflow-y-auto">
                {alerts.length === 0
                  ? <p className="text-center text-sm py-8" style={{ color:'var(--text-muted)' }}>No notifications</p>
                  : alerts.slice(0,10).map(a => (
                    <div key={a.id} className="flex items-start gap-3 px-4 py-3 transition-colors" style={{ borderBottom:'1px solid var(--border)' }}
                      onMouseEnter={e => (e.currentTarget.style.background = 'var(--bg-surface)')}
                      onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}>
                      <span className={`flex-shrink-0 mt-1.5 w-2 h-2 rounded-full ${SEV_DOT[a.severity] ?? 'bg-[#6d7b6c]'}`} />
                      <div className="flex-1 min-w-0">
                        <p className="text-xs leading-relaxed" style={{ color:'var(--text)' }}>{a.message}</p>
                        <p className="text-[11px] mt-0.5" style={{ color:'var(--text-muted)' }}>{fmtDate(a.createdAt, 'relative')}</p>
                      </div>
                      <button onClick={() => onClearAlert(a.id)} style={{ color:'var(--text-muted)' }} className="flex-shrink-0 hover:opacity-70 transition-opacity">
                        <X size={12} />
                      </button>
                    </div>
                  ))
                }
              </div>
              <div className="px-4 py-2.5" style={{ borderTop:'1px solid var(--border)' }}>
                <button onClick={() => { navigate('/alerts'); setNotifOpen(false); }}
                  className="text-xs font-semibold hover:underline" style={{ color:'#0058be' }}>
                  View all alerts →
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Profile */}
        <div className="relative" ref={profileRef}>
          <button
            onClick={() => setProfileOpen(o => !o)}
            className="flex items-center gap-2 px-2.5 py-1.5 rounded-xl transition-colors"
            style={{ background:'var(--bg-surface)' }}
          >
            <div className={`w-7 h-7 rounded-lg bg-gradient-to-br ${ROLE_GRAD[user?.role ?? ''] ?? 'from-slate-500 to-slate-700'} flex items-center justify-center text-white text-[11px] font-black`}
              style={{ fontFamily:'Manrope,sans-serif' }}>
              {initials}
            </div>
            <span className="text-[13px] font-semibold hidden sm:block max-w-[100px] truncate" style={{ color:'var(--text)' }}>{user?.name}</span>
            <ChevronDown size={12} className="hidden sm:block" style={{ color:'var(--text-muted)' }} />
          </button>

          {profileOpen && (
            <div className="absolute right-0 top-11 w-52 z-50 overflow-hidden" style={dropdownStyle}>
              <div className="px-4 py-3" style={{ borderBottom:'1px solid var(--border)' }}>
                <p className="text-[13px] font-bold truncate" style={{ color:'var(--text)' }}>{user?.name}</p>
                <p className="text-[11px] truncate" style={{ color:'var(--text-muted)' }}>{user?.email}</p>
                <span className="inline-block mt-1.5 text-[10px] font-bold px-2 py-0.5 rounded-full"
                  style={{ background:'rgba(0,110,47,0.12)', color:'#006e2f' }}>
                  {user?.role}
                </span>
              </div>
              <div className="py-1">
                {[
                  { label:'Profile',  Icon: User,     action: () => { navigate('/settings'); setProfileOpen(false); } },
                  { label:'Settings', Icon: Settings, action: () => { navigate('/settings'); setProfileOpen(false); } },
                ].map(item => (
                  <button key={item.label} onClick={item.action}
                    className="w-full flex items-center gap-3 px-4 py-2.5 text-[13px] transition-colors"
                    style={{ color:'var(--text-sub)' }}
                    onMouseEnter={e => (e.currentTarget.style.background = 'var(--bg-surface)')}
                    onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}
                  >
                    <item.Icon size={13} /> {item.label}
                  </button>
                ))}
                <div style={{ margin:'4px 0', borderTop:'1px solid var(--border)' }} />
                <button onClick={logout}
                  className="w-full flex items-center gap-3 px-4 py-2.5 text-[13px] transition-colors text-[#ba1a1a]"
                  onMouseEnter={e => (e.currentTarget.style.background = 'rgba(186,26,26,.07)')}
                  onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}
                >
                  <LogOut size={13} /> Sign out
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}