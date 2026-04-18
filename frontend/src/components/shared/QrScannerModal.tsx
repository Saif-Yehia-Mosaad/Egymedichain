import { useEffect, useRef } from 'react';
import { X, Camera, AlertCircle } from 'lucide-react';
import { useQrScanner } from '../../hooks/useQrScanner';
import { Button } from '../ui';

interface QrScannerModalProps {
  open: boolean;
  onClose: () => void;
  onResult: (serial: string) => void;
}

export function QrScannerModal({ open, onClose, onResult }: QrScannerModalProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const { state, errorMsg, start, stop } = useQrScanner((serial) => {
    onResult(serial);
    onClose();
  });

  // Attach videoRef after component mounts
  const scanner = useQrScanner(onResult);

  useEffect(() => {
    if (open && videoRef.current) {
      scanner.videoRef.current = videoRef.current;
      scanner.start();
    }
    return () => {
      scanner.stop();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  if (!open) return null;

  void state; void errorMsg; void start; void stop; // suppress unused

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm"
        onClick={() => { scanner.stop(); onClose(); }}
      />

      {/* Modal */}
      <div className="relative bg-white rounded-3xl shadow-2xl w-full max-w-sm overflow-hidden z-10">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-slate-100">
          <div className="flex items-center gap-2">
            <Camera size={18} className="text-emerald-500" />
            <p className="font-bold text-slate-800">Scan QR Code</p>
          </div>
          <button
            onClick={() => { scanner.stop(); onClose(); }}
            className="w-7 h-7 flex items-center justify-center rounded-lg bg-slate-100 text-slate-500 hover:bg-slate-200 transition-colors"
          >
            <X size={14} />
          </button>
        </div>

        {/* Video / Error */}
        <div className="aspect-square bg-slate-900 relative overflow-hidden">
          <video
            ref={videoRef}
            className="w-full h-full object-cover"
            autoPlay
            muted
            playsInline
          />
          {/* Scan guide overlay */}
          {scanner.state === 'scanning' && (
            <div className="absolute inset-0 flex items-center justify-center">
              <div className="w-48 h-48 border-2 border-emerald-400 rounded-2xl relative">
                <span className="absolute -top-px -left-px w-6 h-6 border-t-2 border-l-2 border-emerald-400 rounded-tl-xl" />
                <span className="absolute -top-px -right-px w-6 h-6 border-t-2 border-r-2 border-emerald-400 rounded-tr-xl" />
                <span className="absolute -bottom-px -left-px w-6 h-6 border-b-2 border-l-2 border-emerald-400 rounded-bl-xl" />
                <span className="absolute -bottom-px -right-px w-6 h-6 border-b-2 border-r-2 border-emerald-400 rounded-br-xl" />
                {/* Scan line animation */}
                <span className="absolute inset-x-2 h-0.5 bg-emerald-400/60 top-1/2 animate-scan-line" />
              </div>
            </div>
          )}
          {(scanner.state === 'error' || scanner.errorMsg) && (
            <div className="absolute inset-0 flex flex-col items-center justify-center gap-3 text-white bg-slate-900/80 p-6">
              <AlertCircle size={32} className="text-red-400" />
              <p className="text-sm text-center text-slate-300">{scanner.errorMsg ?? 'Scanner error'}</p>
              <Button size="sm" variant="ghost" onClick={scanner.start} className="text-white border-slate-600">
                Retry
              </Button>
            </div>
          )}
          {scanner.state === 'opening' && (
            <div className="absolute inset-0 flex items-center justify-center bg-slate-900">
              <div className="flex flex-col items-center gap-3">
                <div className="w-8 h-8 rounded-full border-2 border-emerald-500 border-t-transparent animate-spin" />
                <p className="text-slate-400 text-sm">Opening camera…</p>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-5 py-4 bg-slate-50 border-t border-slate-100">
          <p className="text-xs text-slate-400 text-center">
            Point camera at the QR code on the medicine package
          </p>
        </div>
      </div>
    </div>
  );
}
