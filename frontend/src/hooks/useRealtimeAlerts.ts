import { useEffect, useRef, useCallback } from 'react';
import type { Alert } from '../types';
import { tokenStore } from '../api/client';

const HUB_URL = (import.meta.env.VITE_HUB_URL as string | undefined) ?? 'https://localhost:7001/hubs/alerts';
const API_URL = (import.meta.env.VITE_API_URL as string | undefined) ?? 'https://localhost:7001/api';
const SSE_URL = `${API_URL}/alerts/stream`;

type AlertHandler = (alert: Alert) => void;

export function useRealtimeAlerts(onAlert: AlertHandler, enabled = true) {
  const handlerRef = useRef(onAlert);
  handlerRef.current = onAlert;
  const cleanupRef = useRef<(() => void) | null>(null);

  const startSignalR = useCallback(async (): Promise<boolean> => {
    try {
      // Dynamic import — won't crash if package isn't installed
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const mod = await import(/* @vite-ignore */ '@microsoft/signalr').catch(() => null) as any;
      if (!mod) return false;

      const connection = new mod.HubConnectionBuilder()
        .withUrl(HUB_URL, { accessTokenFactory: () => tokenStore.getAccess() ?? '' })
        .withAutomaticReconnect()
        .configureLogging(mod.LogLevel?.Warning ?? 2)
        .build();

      // Use generic handler and cast inside
      connection.on('AlertCreated', (...args: unknown[]) => handlerRef.current(args[0] as Alert));
      connection.on('ReceiveAlert', (...args: unknown[]) => handlerRef.current(args[0] as Alert));

      await connection.start();
      console.info('[Realtime] SignalR connected');
      cleanupRef.current = () => connection.stop();
      return true;
    } catch {
      return false;
    }
  }, []);

  const startSSE = useCallback((): boolean => {
    try {
      const token = tokenStore.getAccess();
      const url = token ? `${SSE_URL}?access_token=${token}` : SSE_URL;
      const es = new EventSource(url);
      es.addEventListener('alert', (e: MessageEvent) => {
        try { handlerRef.current(JSON.parse(e.data as string) as Alert); } catch { /* skip */ }
      });
      es.onerror = () => es.close();
      cleanupRef.current = () => es.close();
      console.info('[Realtime] SSE connected');
      return true;
    } catch {
      return false;
    }
  }, []);

  const startPolling = useCallback(() => {
    let lastId = 0;
    const timer = setInterval(async () => {
      try {
        const { apiClient } = await import('../api/client');
        const { data } = await apiClient.get<{ data: Alert[] }>('/alerts', {
          params: { isResolved: false, afterId: lastId, pageSize: 10 },
        });
        (data?.data ?? []).forEach((a) => {
          if (a.id > lastId) { lastId = a.id; handlerRef.current(a); }
        });
      } catch { /* backend not ready */ }
    }, 30_000);
    cleanupRef.current = () => clearInterval(timer);
  }, []);

  useEffect(() => {
    if (!enabled) return;
    (async () => {
      if (await startSignalR()) return;
      if (startSSE()) return;
      startPolling();
    })();
    return () => { cleanupRef.current?.(); cleanupRef.current = null; };
  }, [enabled, startSignalR, startSSE, startPolling]);
}
