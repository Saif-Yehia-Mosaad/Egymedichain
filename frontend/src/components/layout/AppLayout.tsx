import { useState, useCallback } from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar }           from './Sidebar';
import { Topbar }            from './Topbar';
import { BottomNav }         from './BottomNav';
import { useRealtimeAlerts } from '../../hooks/useRealtimeAlerts';
import type { Alert }        from '../../types';

export function AppLayout() {
  const [alerts, setAlerts] = useState<Alert[]>([]);

  const handleNewAlert = useCallback((alert: Alert) => {
    setAlerts(prev => {
      if (prev.some(a => a.id === alert.id)) return prev;
      return [alert, ...prev].slice(0, 50);
    });
  }, []);

  const handleClearAlert = useCallback((id: number) => {
    setAlerts(prev => prev.filter(a => a.id !== id));
  }, []);

  useRealtimeAlerts(handleNewAlert, true);
  const unreadCount = alerts.filter(a => !a.isResolved).length;

  return (
    <div className="flex h-screen overflow-hidden" style={{ background: 'var(--bg)' }}>
      {/* Desktop sidebar — hidden on mobile */}
      <Sidebar alertCount={unreadCount} />

      {/* Main column */}
      <div className="flex flex-col flex-1 min-w-0 overflow-hidden">
        <Topbar alerts={alerts} onClearAlert={handleClearAlert} />

        {/* Independent scroll zone */}
        <main className="flex-1 overflow-y-auto overflow-x-hidden pb-16 lg:pb-0">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 py-5">
            <Outlet context={{ alerts }} />
          </div>
        </main>
      </div>

      {/* Mobile bottom nav — hidden on desktop */}
      <BottomNav alertCount={unreadCount} />
    </div>
  );
}