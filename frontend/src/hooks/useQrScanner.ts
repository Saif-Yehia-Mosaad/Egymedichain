import { useRef, useState, useCallback } from 'react';

export type ScannerState = 'idle' | 'opening' | 'scanning' | 'error';

interface BarcodeDetectorResult { rawValue: string; }

export function useQrScanner(onResult: (serial: string) => void) {
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const streamRef = useRef<MediaStream | null>(null);
  const rafRef = useRef<number | null>(null);
  const [state, setState] = useState<ScannerState>('idle');
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const stop = useCallback(() => {
    if (rafRef.current) cancelAnimationFrame(rafRef.current);
    streamRef.current?.getTracks().forEach((t) => t.stop());
    streamRef.current = null;
    rafRef.current = null;
    setState('idle');
  }, []);

  const scan = useCallback(async (videoEl: HTMLVideoElement) => {
    const win = window as unknown as Record<string, unknown>;
    if (!('BarcodeDetector' in window)) {
      setState('error');
      setErrorMsg('QR scanning not supported in this browser. Enter serial manually.');
      return;
    }

    try {
      const BarcodeDetector = win['BarcodeDetector'] as new (opts: { formats: string[] }) => {
        detect(source: HTMLVideoElement): Promise<BarcodeDetectorResult[]>;
      };
      const detector = new BarcodeDetector({ formats: ['qr_code', 'code_128'] });

      const tick = async () => {
        if (videoEl.readyState === videoEl.HAVE_ENOUGH_DATA) {
          try {
            const barcodes = await detector.detect(videoEl);
            if (barcodes.length > 0) {
              const raw = barcodes[0].rawValue;
              const serial = raw.startsWith('EGM:SERIAL:') ? raw.split(':')[2] : raw;
              stop();
              onResult(serial);
              return;
            }
          } catch { /* frame not ready */ }
        }
        rafRef.current = requestAnimationFrame(tick);
      };
      rafRef.current = requestAnimationFrame(tick);
    } catch (e) {
      setState('error');
      setErrorMsg((e as Error).message);
    }
  }, [stop, onResult]);

  const start = useCallback(async () => {
    setState('opening');
    setErrorMsg(null);
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' },
      });
      streamRef.current = stream;
      setState('scanning');
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
        await videoRef.current.play();
        scan(videoRef.current);
      }
    } catch (e) {
      setState('error');
      setErrorMsg((e as Error).message ?? 'Camera access denied');
    }
  }, [scan]);

  return { videoRef, state, errorMsg, start, stop };
}
