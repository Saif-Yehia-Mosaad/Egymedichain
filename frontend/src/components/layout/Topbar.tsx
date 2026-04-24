import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search, Bell, LogOut, User, Settings, ChevronDown, X, Menu, Moon, Sun } from 'lucide-react';
import { useAuth }  from '../../context/AuthContext';
import { fmtDate }  from '../../utils';
import type { Alert } from '../../types';

const SEV_COLOR: Record<string, string> = {
  Critical: '#f87171', High: '#fbbf24', Medium: '#818cf8', Low: '#4a5e78',
};

// Simple theme toggle (persists to localStorage)
function useTheme() {
  const [dark, setDark] = useState(() => {
    const s = localStorage.getItem('egm_theme');
    return s ? s === 'dark' : true; // default dark
  });
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
    localStorage.setItem('egm_theme', dark ? 'dark' : 'light');
    // Our CSS variables are always dark — light mode just lightens bg slightly
    if (!dark) {
      document.documentElement.style.setProperty('--bg',    '#0d1827');
      document.documentElement.style.setProperty('--bg-2',  '#111f32');
      document.documentElement.style.setProperty('--bg-3',  '#152438');
      document.documentElement.style.setProperty('--tx',    '#e8f0fa');
    } else {
      document.documentElement.style.removeProperty('--bg');
      document.documentElement.style.removeProperty('--bg-2');
      document.documentElement.style.removeProperty('--bg-3');
      document.documentElement.style.removeProperty('--tx');
    }
  }, [dark]);
  return { dark, toggle: () => setDark(d => !d) };
}

export function Topbar({
  alerts,
  onClearAlert,
  onMenuClick,
}: {
  alerts: Alert[];
  onClearAlert: (id: number) => void;
  onMenuClick: () => void;
}) {
  const { user, logout }  = useAuth();
  const navigate          = useNavigate();
  const { dark, toggle }  = useTheme();
  const [notifOpen, setNotifOpen]   = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const [search, setSearch]          = useState('');
  const notifRef   = useRef<HTMLDivElement>(null);
  const profileRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const h = (e: MouseEvent) => {
      if (notifRef.current   && !notifRef.current.contains(e.target as Node))   setNotifOpen(false);
      if (profileRef.current && !profileRef.current.contains(e.target as Node)) setProfileOpen(false);
    };
    document.addEventListener('mousedown', h);
    return () => document.removeEventListener('mousedown', h);
  }, []);

  const unread   = alerts.filter(a => !a.isResolved).length;
  const initials = user?.name?.split(' ').map((w: string) => w[0]).slice(0, 2).join('').toUpperCase() ?? 'U';

  const roleColors: Record<string, string> = {
    Ministry: '#818cf8', Manufacturer: '#38bdf8',
    Warehouse: '#fbbf24', Pharmacy: '#10d9a0',
  };
  const roleColor = roleColors[user?.role ?? ''] ?? '#10d9a0';

  const iconBtn = (onClick: () => void, children: React.ReactNode, badge?: number) => (
    <button onClick={onClick} style={{
      width: 36, height: 36,
      borderRadius: 9,
      background: 'var(--bg-4)',
      border: '1px solid var(--bd)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      cursor: 'pointer',
      color: 'var(--tx-2)',
      position: 'relative',
      transition: 'background .15s, color .15s',
      flexShrink: 0,
    }}
    onMouseEnter={e => { e.currentTarget.style.background = 'var(--bg-5)'; e.currentTarget.style.color = 'var(--tx)'; }}
    onMouseLeave={e => { e.currentTarget.style.background = 'var(--bg-4)'; e.currentTarget.style.color = 'var(--tx-2)'; }}>
      {children}
      {badge != null && badge > 0 && (
        <span style={{
          position: 'absolute', top: -4, right: -4,
          background: '#f87171', color: '#fff',
          fontSize: 9, fontWeight: 700,
          borderRadius: 99, padding: '1px 4px',
          border: '2px solid var(--bg-2)',
          minWidth: 16, textAlign: 'center',
        }}>
          {badge > 9 ? '9+' : badge}
        </span>
      )}
    </button>
  );

  return (
    <header className="topbar">
      {/* Mobile menu button */}
      <button
        onClick={onMenuClick}
        className="lg:hidden"
        style={{
          width: 34, height: 34,
          background: 'var(--bg-4)',
          border: '1px solid var(--bd)',
          borderRadius: 9,
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: 'var(--tx-2)', cursor: 'pointer', flexShrink: 0,
        }}
      >
        <Menu size={16} />
      </button>

      {/* Search */}
      <div style={{ flex: 1, maxWidth: 400, position: 'relative' }}>
        <Search size={14} style={{
          position: 'absolute', left: 12, top: '50%', transform: 'translateY(-50%)',
          color: 'var(--tx-3)', pointerEvents: 'none',
        }} />
        <input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search medicines, batches, transactions…"
          className="input"
          style={{ paddingLeft: 36, paddingTop: 8, paddingBottom: 8, borderRadius: 9, fontSize: 13 }}
        />
      </div>

      {/* Right actions */}
      <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: 8 }}>

        {/* Theme toggle */}
        {iconBtn(toggle, dark ? <Sun size={15} /> : <Moon size={15} />)}

        {/* Notifications */}
        <div ref={notifRef} style={{ position: 'relative' }}>
          {iconBtn(() => setNotifOpen(o => !o), <Bell size={15} />, unread)}

          {notifOpen && (
            <div style={{
              position: 'absolute', right: 0, top: 44,
              width: 320,
              background: 'var(--bg-3)',
              border: '1px solid var(--bd-2)',
              borderRadius: 14,
              boxShadow: 'var(--sh-float)',
              zIndex: 50,
              overflow: 'hidden',
              animation: 'fadeUp .2s var(--ease) both',
            }}>
              <div style={{
                padding: '12px 16px',
                borderBottom: '1px solid var(--bd)',
                display: 'flex', alignItems: 'center', justifyContent: 'space-between',
              }}>
                <span style={{ fontFamily: 'Syne,sans-serif', fontWeight: 700, fontSize: 14, color: 'var(--tx)' }}>
                  Notifications
                </span>
                {unread > 0 && (
                  <span className="badge badge-em">{unread} new</span>
                )}
              </div>
              <div style={{ maxHeight: 280, overflowY: 'auto' }}>
                {alerts.length === 0
                  ? <p style={{ textAlign: 'center', padding: '24px 0', fontSize: 13, color: 'var(--tx-3)' }}>
                      All clear
                    </p>
                  : alerts.slice(0, 10).map(a => (
                    <div key={a.id} style={{
                      display: 'flex', alignItems: 'flex-start', gap: 10,
                      padding: '10px 16px',
                      borderBottom: '1px solid var(--bd)',
                      transition: 'background .15s',
                    }}
                    onMouseEnter={e => (e.currentTarget.style.background = 'var(--bg-4)')}
                    onMouseLeave={e => (e.currentTarget.style.background = 'transparent')}>
                      <div style={{
                        width: 7, height: 7, borderRadius: '50%',
                        background: SEV_COLOR[a.severity] ?? 'var(--tx-3)',
                        flexShrink: 0, marginTop: 5,
                        boxShadow: `0 0 6px ${SEV_COLOR[a.severity] ?? 'var(--tx-3)'}`,
                      }} />
                      <div style={{ flex: 1, minWidth: 0 }}>
                        <p style={{ fontSize: 12, color: 'var(--tx)', lineHeight: 1.5 }}>{a.message}</p>
                        <p style={{ fontSize: 11, color: 'var(--tx-3)', marginTop: 2 }}>
                          {fmtDate(a.createdAt, 'relative')}
                        </p>
                      </div>
                      <button onClick={() => onClearAlert(a.id)}
                        style={{ color: 'var(--tx-3)', background: 'none', border: 'none', cursor: 'pointer', flexShrink: 0, padding: 2 }}>
                        <X size={11} />
                      </button>
                    </div>
                  ))
                }
              </div>
              <div style={{ padding: '8px 16px', borderTop: '1px solid var(--bd)' }}>
                <button onClick={() => { navigate('/alerts'); setNotifOpen(false); }}
                  style={{ fontSize: 12, color: 'var(--em)', fontWeight: 600, background: 'none', border: 'none', cursor: 'pointer' }}>
                  View all alerts →
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Profile */}
        <div ref={profileRef} style={{ position: 'relative' }}>
          <button onClick={() => setProfileOpen(o => !o)} style={{
            display: 'flex', alignItems: 'center', gap: 8,
            padding: '5px 10px',
            background: 'var(--bg-4)',
            border: '1px solid var(--bd)',
            borderRadius: 10,
            cursor: 'pointer',
            transition: 'background .15s, border-color .15s',
          }}
          onMouseEnter={e => { e.currentTarget.style.background = 'var(--bg-5)'; e.currentTarget.style.borderColor = 'var(--em)'; }}
          onMouseLeave={e => { e.currentTarget.style.background = 'var(--bg-4)'; e.currentTarget.style.borderColor = 'var(--bd)'; }}>
            <div style={{
              width: 26, height: 26, borderRadius: 7,
              background: `${roleColor}22`,
              border: `1px solid ${roleColor}55`,
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontFamily: 'Syne,sans-serif', fontWeight: 700, fontSize: 11,
              color: roleColor, flexShrink: 0,
            }}>
              {initials}
            </div>
            <span style={{ fontSize: 13, fontWeight: 500, color: 'var(--tx)', maxWidth: 110, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}
              className="hidden sm:block">
              {user?.name}
            </span>
            <ChevronDown size={12} style={{ color: 'var(--tx-3)', flexShrink: 0 }} className="hidden sm:block" />
          </button>

          {profileOpen && (
            <div style={{
              position: 'absolute', right: 0, top: 44,
              width: 210,
              background: 'var(--bg-3)',
              border: '1px solid var(--bd-2)',
              borderRadius: 14,
              boxShadow: 'var(--sh-float)',
              zIndex: 50,
              overflow: 'hidden',
              animation: 'fadeUp .2s var(--ease) both',
            }}>
              <div style={{ padding: '12px 14px', borderBottom: '1px solid var(--bd)' }}>
                <p style={{ fontSize: 13, fontWeight: 600, color: 'var(--tx)' }}>{user?.name}</p>
                <p style={{ fontSize: 11, color: 'var(--tx-3)', marginTop: 2 }}>{user?.email}</p>
                <span className="badge badge-em" style={{ marginTop: 6, display: 'inline-flex' }}>{user?.role}</span>
              </div>
              <div style={{ padding: '6px 0' }}>
                {[
                  { label: 'Profile',  Icon: User,     path: '/settings' },
                  { label: 'Settings', Icon: Settings, path: '/settings' },
                ].map(item => (
                  <button key={item.label}
                    onClick={() => { navigate(item.path); setProfileOpen(false); }}
                    style={{
                      width: '100%', display: 'flex', alignItems: 'center', gap: 10,
                      padding: '9px 14px', fontSize: 13, color: 'var(--tx-2)',
                      background: 'none', border: 'none', cursor: 'pointer',
                      transition: 'background .15s, color .15s', textAlign: 'left',
                    }}
                    onMouseEnter={e => { e.currentTarget.style.background = 'var(--bg-4)'; e.currentTarget.style.color = 'var(--tx)'; }}
                    onMouseLeave={e => { e.currentTarget.style.background = 'none'; e.currentTarget.style.color = 'var(--tx-2)'; }}>
                    <item.Icon size={14} /> {item.label}
                  </button>
                ))}
                <div style={{ margin: '4px 0', borderTop: '1px solid var(--bd)' }} />
                <button onClick={logout} style={{
                  width: '100%', display: 'flex', alignItems: 'center', gap: 10,
                  padding: '9px 14px', fontSize: 13, color: '#f87171',
                  background: 'none', border: 'none', cursor: 'pointer',
                  transition: 'background .15s', textAlign: 'left',
                }}
                onMouseEnter={e => (e.currentTarget.style.background = 'rgba(248,113,113,.07)')}
                onMouseLeave={e => (e.currentTarget.style.background = 'none')}>
                  <LogOut size={14} /> Sign out
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
