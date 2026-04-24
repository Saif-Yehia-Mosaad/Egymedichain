import { useState, useCallback } from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar }           from './Sidebar';
import { Topbar }            from './Topbar';
import { useRealtimeAlerts } from '../../hooks/useRealtimeAlerts';
import type { Alert }        from '../../types';

export function AppLayout() {
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const handleNewAlert = useCallback((a: Alert) => {
    setAlerts(prev => prev.some(x => x.id === a.id) ? prev : [a, ...prev].slice(0, 50));
  }, []);

  const handleClearAlert = useCallback((id: number) => {
    setAlerts(prev => prev.filter(a => a.id !== id));
  }, []);

  useRealtimeAlerts(handleNewAlert, true);
  const unread = alerts.filter(a => !a.isResolved).length;

  return (
    <div className="app-shell">
      {/* Mobile overlay */}
      <div
        className={`sidebar-overlay${sidebarOpen ? ' visible' : ''}`}
        onClick={() => setSidebarOpen(false)}
      />

      <Sidebar
        alertCount={unread}
        mobileOpen={sidebarOpen}
        onMobileClose={() => setSidebarOpen(false)}
      />

      <div className="main-area">
        <Topbar
          alerts={alerts}
          onClearAlert={handleClearAlert}
          onMenuClick={() => setSidebarOpen(o => !o)}
        />

        {/* Only this scrolls */}
        <main className="page-scroll">
          <div className="page-inner">
            <Outlet context={{ alerts }} />
          </div>
        </main>
      </div>
    </div>
  );
}
