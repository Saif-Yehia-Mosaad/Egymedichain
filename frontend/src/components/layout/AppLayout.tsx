import { useState, useCallback } from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';
import { useRealtimeAlerts } from '../../hooks/useRealtimeAlerts';
import type { Alert } from '../../types';

export function AppLayout() {
  const [alerts, setAlerts] = useState<Alert[]>([]);

  const handleNewAlert = useCallback((alert: Alert) => {
    setAlerts((prev) => {
      if (prev.some((a) => a.id === alert.id)) return prev;
      return [alert, ...prev].slice(0, 50); // cap at 50
    });
  }, []);

  const handleClearAlert = useCallback((id: number) => {
    setAlerts((prev) => prev.filter((a) => a.id !== id));
  }, []);

  useRealtimeAlerts(handleNewAlert, true);

  const unreadCount = alerts.filter((a) => !a.isResolved).length;

  return (
    <div className="flex h-screen bg-slate-50 overflow-hidden">
      <Sidebar alertCount={unreadCount} />
      <div className="flex flex-col flex-1 min-w-0 overflow-hidden">
        <Topbar alerts={alerts} onClearAlert={handleClearAlert} />
        <main className="flex-1 overflow-y-auto">
          <div className="p-6 max-w-screen-xl mx-auto">
            <Outlet context={{ alerts }} />
          </div>
        </main>
      </div>
    </div>
  );
}
