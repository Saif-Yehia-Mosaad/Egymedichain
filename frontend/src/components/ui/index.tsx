import { forwardRef, type ButtonHTMLAttributes, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '../../utils';

// ─── Button ───────────────────────────────────────────────────
type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger' | 'gradient';
type ButtonSize = 'sm' | 'md' | 'lg';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  isLoading?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'primary', size = 'md', isLoading, leftIcon, rightIcon, children, className, disabled, ...rest }, ref) => {
    const base = 'inline-flex items-center gap-2 font-semibold rounded-xl transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed select-none';
    const variants: Record<ButtonVariant, string> = {
      primary: 'bg-emerald-500 text-white hover:bg-emerald-600 focus:ring-emerald-400 active:scale-95',
      secondary: 'bg-blue-500 text-white hover:bg-blue-600 focus:ring-blue-400 active:scale-95',
      ghost: 'bg-transparent text-slate-600 hover:bg-slate-100 focus:ring-slate-300 border border-slate-200',
      danger: 'bg-red-500 text-white hover:bg-red-600 focus:ring-red-400 active:scale-95',
      gradient: 'bg-gradient-to-r from-emerald-500 to-blue-500 text-white hover:from-emerald-600 hover:to-blue-600 focus:ring-emerald-400 active:scale-95 shadow-md shadow-emerald-200',
    };
    const sizes: Record<ButtonSize, string> = {
      sm: 'px-3 py-1.5 text-sm',
      md: 'px-4 py-2 text-sm',
      lg: 'px-6 py-3 text-base',
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

// ─── Badge ────────────────────────────────────────────────────
type BadgeColor = 'green' | 'blue' | 'red' | 'orange' | 'gray' | 'purple';

export function Badge({ color = 'gray', children, className }: { color?: BadgeColor; children: ReactNode; className?: string }) {
  const colors: Record<BadgeColor, string> = {
    green: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    blue: 'bg-blue-50 text-blue-700 border-blue-200',
    red: 'bg-red-50 text-red-700 border-red-200',
    orange: 'bg-orange-50 text-orange-700 border-orange-200',
    gray: 'bg-slate-100 text-slate-600 border-slate-200',
    purple: 'bg-violet-50 text-violet-700 border-violet-200',
  };
  return (
    <span className={cn('inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border', colors[color], className)}>
      {children}
    </span>
  );
}

// ─── Card ─────────────────────────────────────────────────────
export function Card({ children, className, glass, onClick }: { children: ReactNode; className?: string; glass?: boolean; onClick?: () => void }) {
  return (
    <div onClick={onClick} className={cn(
      'rounded-2xl border border-slate-200/80 p-6',
      glass ? 'bg-white/70 backdrop-blur-sm' : 'bg-white',
      'shadow-sm',
      onClick && 'cursor-pointer',
      className
    )}>
      {children}
    </div>
  );
}

export function CardHeader({ title, subtitle, action }: { title: string; subtitle?: string; action?: ReactNode }) {
  return (
    <div className="flex items-start justify-between mb-5">
      <div>
        <h3 className="text-base font-bold text-slate-800">{title}</h3>
        {subtitle && <p className="text-sm text-slate-400 mt-0.5">{subtitle}</p>}
      </div>
      {action && <div className="flex-shrink-0">{action}</div>}
    </div>
  );
}

// ─── Input ────────────────────────────────────────────────────
interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  leftAddon?: ReactNode;
  rightAddon?: ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, leftAddon, rightAddon, className, ...rest }, ref) => (
    <div className="flex flex-col gap-1.5">
      {label && <label className="text-sm font-medium text-slate-700">{label}</label>}
      <div className="relative flex items-center">
        {leftAddon && <span className="absolute left-3 text-slate-400">{leftAddon}</span>}
        <input
          ref={ref}
          className={cn(
            'w-full rounded-xl border bg-white px-4 py-2.5 text-sm text-slate-800 placeholder-slate-400',
            'transition-all duration-150 outline-none',
            'focus:ring-2 focus:ring-emerald-400/40 focus:border-emerald-400',
            error ? 'border-red-400 focus:ring-red-300/40' : 'border-slate-200 hover:border-slate-300',
            leftAddon && 'pl-9',
            rightAddon && 'pr-9',
            className
          )}
          {...rest}
        />
        {rightAddon && <span className="absolute right-3 text-slate-400">{rightAddon}</span>}
      </div>
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  )
);
Input.displayName = 'Input';

// ─── Select ───────────────────────────────────────────────────
interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  options: { value: string | number; label: string }[];
}
export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ label, error, options, className, ...rest }, ref) => (
    <div className="flex flex-col gap-1.5">
      {label && <label className="text-sm font-medium text-slate-700">{label}</label>}
      <select
        ref={ref}
        className={cn(
          'w-full rounded-xl border bg-white px-4 py-2.5 text-sm text-slate-800',
          'transition-all duration-150 outline-none',
          'focus:ring-2 focus:ring-emerald-400/40 focus:border-emerald-400',
          error ? 'border-red-400' : 'border-slate-200 hover:border-slate-300',
          className
        )}
        {...rest}
      >
        {options.map((o) => (
          <option key={o.value} value={o.value}>{o.label}</option>
        ))}
      </select>
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  )
);
Select.displayName = 'Select';

// ─── Spinner ──────────────────────────────────────────────────
export function Spinner({ size = 'md', className }: { size?: 'sm' | 'md' | 'lg'; className?: string }) {
  const sizes = { sm: 'h-4 w-4 border-2', md: 'h-6 w-6 border-2', lg: 'h-10 w-10 border-3' };
  return (
    <span className={cn('inline-block rounded-full border-transparent border-t-current animate-spin', sizes[size], className)} />
  );
}

// ─── Skeleton ─────────────────────────────────────────────────
export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('animate-pulse rounded-xl bg-slate-200', className)} />;
}

export function SkeletonCard() {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-6 space-y-3">
      <Skeleton className="h-4 w-1/3" />
      <Skeleton className="h-8 w-1/2" />
      <Skeleton className="h-3 w-2/3" />
    </div>
  );
}

// ─── Empty State ─────────────────────────────────────────────
export function EmptyState({ icon, title, description, action }: {
  icon?: ReactNode; title: string; description?: string; action?: ReactNode;
}) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center gap-3">
      {icon && <div className="text-slate-300 text-5xl mb-2">{icon}</div>}
      <p className="text-slate-700 font-semibold text-lg">{title}</p>
      {description && <p className="text-slate-400 text-sm max-w-xs">{description}</p>}
      {action && <div className="mt-3">{action}</div>}
    </div>
  );
}

// ─── Progress Bar ─────────────────────────────────────────────
export function ProgressBar({ value, max = 100, color = 'green', className }: {
  value: number; max?: number; color?: 'green' | 'blue' | 'red' | 'orange'; className?: string;
}) {
  const pct = Math.min(100, (value / max) * 100);
  const colors = {
    green: 'from-emerald-400 to-emerald-500',
    blue: 'from-blue-400 to-blue-500',
    red: 'from-red-400 to-red-500',
    orange: 'from-orange-400 to-orange-500',
  };
  return (
    <div className={cn('h-2 w-full rounded-full bg-slate-100 overflow-hidden', className)}>
      <div
        className={cn('h-full rounded-full bg-gradient-to-r transition-all duration-500', colors[color])}
        style={{ width: `${pct}%` }}
      />
    </div>
  );
}

// ─── Toggle ──────────────────────────────────────────────────
export function Toggle({ checked, onChange, label }: { checked: boolean; onChange: (v: boolean) => void; label?: string }) {
  return (
    <label className="inline-flex items-center gap-3 cursor-pointer">
      <button
        role="switch"
        aria-checked={checked}
        onClick={() => onChange(!checked)}
        className={cn(
          'relative w-11 h-6 rounded-full transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-emerald-400/40',
          checked ? 'bg-emerald-500' : 'bg-slate-200'
        )}
      >
        <span className={cn(
          'absolute top-1 left-1 w-4 h-4 rounded-full bg-white shadow transition-transform duration-200',
          checked && 'translate-x-5'
        )} />
      </button>
      {label && <span className="text-sm text-slate-600">{label}</span>}
    </label>
  );
}
