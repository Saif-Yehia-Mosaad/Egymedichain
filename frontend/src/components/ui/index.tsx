import { forwardRef, type ButtonHTMLAttributes, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '../../utils';

// ─── Button ─────────────────────────────────────────────────────
type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger' | 'gradient' | 'surface';
type ButtonSize = 'sm' | 'md' | 'lg';
interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant; size?: ButtonSize; isLoading?: boolean;
  leftIcon?: ReactNode; rightIcon?: ReactNode;
}
export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', isLoading, leftIcon, rightIcon, children, className, disabled, ...rest }, ref) => {
    const base = 'inline-flex items-center gap-2 font-semibold rounded-xl transition-all duration-200 focus:outline-none focus:ring-4 disabled:opacity-50 disabled:cursor-not-allowed select-none whitespace-nowrap';
    const variants: Record<ButtonVariant, string> = {
      primary:   'bg-gradient-to-br from-[#006e2f] to-[#0058be] text-white hover:-translate-y-px active:scale-[.98] shadow-[0_4px_14px_rgba(0,110,47,0.25)] focus:ring-[rgba(0,110,47,0.2)]',
      gradient:  'bg-gradient-to-br from-[#006e2f] to-[#0058be] text-white hover:-translate-y-px active:scale-[.98] shadow-[0_4px_14px_rgba(0,110,47,0.25)] focus:ring-[rgba(0,110,47,0.2)]',
      secondary: 'bg-[#0058be] text-white hover:bg-[#004a9e] hover:-translate-y-px active:scale-[.98] focus:ring-[rgba(0,88,190,0.2)]',
      ghost:     'bg-[#f2f4f6] text-[#191c1e] hover:bg-[#eceef0] focus:ring-[rgba(0,110,47,0.15)] border border-[rgba(188,203,185,0.3)]',
      surface:   'bg-[#f2f4f6] text-[#191c1e] hover:bg-[#eceef0] focus:ring-[rgba(0,110,47,0.15)]',
      danger:    'bg-[#ba1a1a] text-white hover:bg-[#9e1515] hover:-translate-y-px active:scale-[.98] focus:ring-[rgba(186,26,26,0.2)]',
    };
    const sizes: Record<ButtonSize, string> = {
      sm: 'px-3 py-1.5 text-xs', md: 'px-4 py-2.5 text-sm', lg: 'px-6 py-3 text-sm',
    };
    return (
      <button ref={ref} disabled={disabled || isLoading} className={cn(base, variants[variant], sizes[size], className)} {...rest}>
        {isLoading ? <Spinner size="sm" /> : leftIcon}
        {children}
        {!isLoading && rightIcon}
      </button>
    );
  }
);
Button.displayName = 'Button';

// ─── Badge ───────────────────────────────────────────────────────
type BadgeColor = 'green' | 'blue' | 'red' | 'orange' | 'gray' | 'purple' | 'amber';
export function Badge({ color = 'gray', children, className, pulse }: {
  color?: BadgeColor; children: ReactNode; className?: string; pulse?: boolean;
}) {
  const colors: Record<BadgeColor, string> = {
    green:  'bg-[rgba(0,110,47,0.1)] text-[#004b1e]',
    blue:   'bg-[rgba(0,88,190,0.1)] text-[#0058be]',
    red:    'bg-[#ffdad6] text-[#93000a]',
    orange: 'bg-[#fff0e0] text-[#8b4700]',
    amber:  'bg-[#fff0e0] text-[#8b4700]',
    gray:   'bg-[#eceef0] text-[#3d4a3d]',
    purple: 'bg-[rgba(124,58,237,0.1)] text-[#5b21b6]',
  };
  return (
    <span className={cn('badge-clinical', colors[color], className)}>
      {pulse && <span className={cn('w-1.5 h-1.5 rounded-full animate-pulse inline-block',
        color==='green'?'bg-[#006e2f]':color==='blue'?'bg-[#0058be]':color==='red'?'bg-[#ba1a1a]':'bg-current')} />}
      {children}
    </span>
  );
}

// ─── Card ────────────────────────────────────────────────────────
export function Card({ children, className, onClick, danger, glass }: {
  children: ReactNode; className?: string; onClick?: () => void; danger?: boolean; glass?: boolean;
}) {
  return (
    <div onClick={onClick} className={cn(
      'card-clinical transition-all duration-200',
      onClick && 'cursor-pointer hover:-translate-y-0.5 hover:shadow-[0_4px_20px_rgba(25,28,30,0.08)]',
      danger && 'ring-1 ring-[rgba(186,26,26,0.15)]',
      glass && 'bg-white/70 backdrop-blur-xl',
      className
    )}>
      {children}
    </div>
  );
}

export function CardHeader({ title, subtitle, action }: { title: string; subtitle?: string; action?: ReactNode }) {
  return (
    <div className="flex items-start justify-between mb-4">
      <div>
        <h3 className="font-semibold text-[15px] text-[#191c1e] leading-tight" style={{fontFamily:'Manrope,sans-serif'}}>{title}</h3>
        {subtitle && <p className="text-xs text-[#6d7b6c] mt-0.5">{subtitle}</p>}
      </div>
      {action && <div className="flex-shrink-0">{action}</div>}
    </div>
  );
}

// ─── KPI Card ────────────────────────────────────────────────────
export function KpiCard({ label, value, sub, gradient, className }: {
  label: string; value: string | number; sub?: string; gradient: string; className?: string;
}) {
  return (
    <div className={cn('relative overflow-hidden rounded-2xl p-5 text-white hover:-translate-y-0.5 transition-transform', gradient, className)}>
      <div className="absolute -top-6 -right-6 w-20 h-20 rounded-full bg-white/8 pointer-events-none" />
      <p className="text-[11px] font-bold uppercase tracking-widest text-white/60 mb-1.5">{label}</p>
      <p className="font-black text-3xl leading-none" style={{fontFamily:'Manrope,sans-serif'}}>{value}</p>
      {sub && <p className="text-[11px] mt-2.5 bg-white/15 inline-flex px-2.5 py-1 rounded-full font-medium">{sub}</p>}
    </div>
  );
}

// ─── Page Header ─────────────────────────────────────────────────
export function PageHeader({ title, subtitle, action }: { title: string; subtitle?: string; action?: ReactNode }) {
  return (
    <div className="flex items-start justify-between gap-4 mb-6 flex-wrap">
      <div>
        <h1 className="font-black text-2xl sm:text-3xl text-[#191c1e] leading-tight tracking-tight" style={{fontFamily:'Manrope,sans-serif'}}>{title}</h1>
        {subtitle && <p className="text-[#6d7b6c] mt-1 text-sm">{subtitle}</p>}
      </div>
      {action && <div className="flex items-center gap-2 flex-shrink-0 flex-wrap">{action}</div>}
    </div>
  );
}

// ─── Input ───────────────────────────────────────────────────────
interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string; error?: string; leftAddon?: ReactNode; rightAddon?: ReactNode; hint?: string;
}
export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, leftAddon, rightAddon, hint, className, ...rest }, ref) => (
    <div className="flex flex-col gap-1.5">
      {label && <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">{label}</label>}
      <div className="relative flex items-center">
        {leftAddon && <span className="absolute left-3.5 text-[#6d7b6c] pointer-events-none">{leftAddon}</span>}
        <input ref={ref} className={cn(
          'w-full rounded-xl border-0 bg-[#eceef0] text-sm text-[#191c1e] placeholder-[#6d7b6c]',
          'transition-all duration-200 outline-none',
          'focus:bg-white focus:ring-4 focus:ring-[rgba(0,88,190,0.12)]',
          error && 'ring-2 ring-[rgba(186,26,26,0.25)] bg-white',
          leftAddon ? 'pl-10 pr-4 py-3' : 'px-4 py-3',
          rightAddon ? 'pr-10' : '',
          className
        )} {...rest} />
        {rightAddon && <span className="absolute right-3.5 text-[#6d7b6c]">{rightAddon}</span>}
      </div>
      {hint && !error && <p className="text-[11px] text-[#6d7b6c]">{hint}</p>}
      {error && <p className="text-[11px] text-[#ba1a1a] font-medium">{error}</p>}
    </div>
  )
);
Input.displayName = 'Input';

// ─── Select ──────────────────────────────────────────────────────
interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string; error?: string; options: { value: string | number; label: string }[];
}
export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ label, error, options, className, ...rest }, ref) => (
    <div className="flex flex-col gap-1.5">
      {label && <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">{label}</label>}
      <select ref={ref} className={cn(
        'w-full rounded-xl border-0 bg-[#eceef0] px-4 py-3 text-sm text-[#191c1e]',
        'transition-all duration-200 outline-none appearance-none cursor-pointer',
        'focus:bg-white focus:ring-4 focus:ring-[rgba(0,88,190,0.12)]',
        error && 'ring-2 ring-[rgba(186,26,26,0.25)]', className
      )} {...rest}>
        {options.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
      </select>
      {error && <p className="text-[11px] text-[#ba1a1a] font-medium">{error}</p>}
    </div>
  )
);
Select.displayName = 'Select';

// ─── Spinner ─────────────────────────────────────────────────────
export function Spinner({ size = 'md', className }: { size?: 'sm' | 'md' | 'lg'; className?: string }) {
  const sizes = { sm: 'h-4 w-4 border-[2px]', md: 'h-5 w-5 border-2', lg: 'h-8 w-8 border-2' };
  return <span className={cn('inline-block rounded-full border-transparent border-t-current animate-spin', sizes[size], className)} />;
}

// ─── Skeleton ────────────────────────────────────────────────────
export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('animate-pulse rounded-xl bg-[#eceef0]', className)} />;
}
export function SkeletonCard() {
  return (
    <div className="card-clinical space-y-3">
      <Skeleton className="h-4 w-1/3" /><Skeleton className="h-8 w-1/2" /><Skeleton className="h-3 w-2/3" />
    </div>
  );
}

// ─── Empty State ─────────────────────────────────────────────────
export function EmptyState({ icon, title, description, action }: {
  icon?: ReactNode; title: string; description?: string; action?: ReactNode;
}) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center gap-3">
      {icon && <div className="text-[#bccbb9] text-5xl mb-2">{icon}</div>}
      <p className="font-bold text-lg text-[#191c1e]" style={{fontFamily:'Manrope,sans-serif'}}>{title}</p>
      {description && <p className="text-sm text-[#6d7b6c] max-w-xs">{description}</p>}
      {action && <div className="mt-3">{action}</div>}
    </div>
  );
}

// ─── Progress Bar ────────────────────────────────────────────────
export function ProgressBar({ value, max = 100, color = 'green', className }: {
  value: number; max?: number; color?: 'green' | 'blue' | 'red' | 'orange'; className?: string;
}) {
  const pct = Math.min(100, (value / max) * 100);
  const fills = {
    green: 'from-[#006e2f] to-[#22c55e]', blue: 'from-[#0058be] to-[#60a5fa]',
    red: 'from-[#ba1a1a] to-[#f87171]', orange: 'from-[#b45309] to-[#fbbf24]',
  };
  return (
    <div className={cn('progress-track', className)}>
      <div className={cn('progress-fill bg-gradient-to-r', fills[color])} style={{ width: `${pct}%` }} />
    </div>
  );
}

// ─── Toggle ──────────────────────────────────────────────────────
export function Toggle({ checked, onChange, label }: { checked: boolean; onChange: (v: boolean) => void; label?: string }) {
  return (
    <label className="inline-flex items-center gap-3 cursor-pointer">
      <button role="switch" aria-checked={checked} onClick={() => onChange(!checked)}
        className={cn('relative w-11 h-6 rounded-full transition-colors duration-200 focus:outline-none focus:ring-4 focus:ring-[rgba(0,110,47,0.2)]',
          checked ? 'bg-[#006e2f]' : 'bg-[#e0e3e5]')}>
        <span className={cn('absolute top-[3px] left-[3px] w-[18px] h-[18px] rounded-full bg-white shadow-sm transition-transform duration-200', checked && 'translate-x-5')} />
      </button>
      {label && <span className="text-sm text-[#3d4a3d]">{label}</span>}
    </label>
  );
}

// ─── Modal ───────────────────────────────────────────────────────
export function Modal({ open, onClose, title, subtitle, children }: {
  open: boolean; onClose: () => void; title: string; subtitle?: string; children: ReactNode;
}) {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-[rgba(25,28,30,0.45)] backdrop-blur-[6px]" onClick={onClose} />
      <div className="relative bg-white rounded-3xl p-6 sm:p-8 w-full max-w-lg shadow-[0_24px_80px_rgba(0,0,0,0.18)] animate-fade-up max-h-[90vh] overflow-y-auto">
        <div className="flex items-start justify-between mb-5">
          <div>
            <h2 className="font-black text-xl text-[#191c1e]" style={{fontFamily:'Manrope,sans-serif'}}>{title}</h2>
            {subtitle && <p className="text-[13px] text-[#6d7b6c] mt-1">{subtitle}</p>}
          </div>
          <button onClick={onClose} className="w-8 h-8 flex items-center justify-center rounded-full bg-[#eceef0] text-[#6d7b6c] hover:bg-[#e0e3e5] transition-colors ml-4 flex-shrink-0">✕</button>
        </div>
        {children}
      </div>
    </div>
  );
}