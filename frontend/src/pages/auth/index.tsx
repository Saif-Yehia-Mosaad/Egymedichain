import { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  Eye, EyeOff, ArrowLeft, ArrowRight, RefreshCw,
  Shield, Building2, Warehouse, Pill, User,
  CheckCircle, Wallet, AlertCircle
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../api/endpoints';
import type { UserRole } from '../../types';

// ─── Design tokens ───────────────────────────────────────────────
const C = {
  green: '#006e2f', blue: '#0058be',
  surface: '#eceef0', white: '#ffffff',
  dark: '#191c1e', muted: '#6d7b6c',
  error: '#ba1a1a',
} as const;

const inputCls = `
  w-full rounded-xl border-0 bg-[#eceef0] text-sm text-[#191c1e]
  placeholder-[#6d7b6c] outline-none transition-all duration-200
  px-4 py-3 focus:bg-white focus:ring-4 focus:ring-[rgba(0,88,190,0.14)]
`.trim();

const inputWithIcon = `${inputCls} pl-11`;

// ─── Shared split layout ─────────────────────────────────────────
function AuthShell({ panel, children }: { panel: React.ReactNode; children: React.ReactNode }) {
  return (
    <div className="min-h-screen flex bg-white">
      {/* Left panel — hidden on mobile */}
      <div className="hidden lg:flex lg:w-[48%] xl:w-[44%] relative overflow-hidden flex-shrink-0"
        style={{ background: 'linear-gradient(160deg,#031a10 0%,#0a2a1a 45%,#051528 100%)' }}>
        <div className="absolute inset-0 pointer-events-none" style={{
          background: 'radial-gradient(ellipse at 35% 55%,rgba(0,200,90,.22) 0%,transparent 58%),' +
            'radial-gradient(ellipse at 80% 15%,rgba(0,88,190,.18) 0%,transparent 52%)',
        }} />
        <div className="relative z-10 w-full flex flex-col p-10 xl:p-14">{panel}</div>
      </div>
      {/* Right form panel */}
      <div className="flex-1 flex items-center justify-center overflow-y-auto px-5 py-10 sm:px-10 lg:px-14">
        <div className="w-full max-w-md">{children}</div>
      </div>
    </div>
  );
}

// ─── Role selector chip ──────────────────────────────────────────
const ROLES: { value: UserRole; label: string; arabic: string; icon: React.ReactNode; color: string }[] = [
  { value: 'Ministry', label: 'Ministry', arabic: 'وزارة الصحة', icon: <Shield size={15} />, color: '#5b21b6' },
  { value: 'Manufacturer', label: 'Manufacturer', arabic: 'مصنع أدوية', icon: <Building2 size={15} />, color: '#1d4ed8' },
  { value: 'Warehouse', label: 'Warehouse', arabic: 'مستودع طبي', icon: <Warehouse size={15} />, color: '#b45309' },
  { value: 'Pharmacy', label: 'Pharmacy', arabic: 'صيدلية', icon: <Pill size={15} />, color: '#006e2f' },
  { value: 'Consumer', label: 'Consumer', arabic: 'مواطن / مريض', icon: <User size={15} />, color: '#0058be' },
];

// ─── LOGIN ───────────────────────────────────────────────────────
const loginSchema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(6, 'Password required'),
});

export function LoginPage() {
  const { login, roleHome } = useAuth();
  const navigate = useNavigate();
  const [showPw, setShowPw] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: any) => {
    setApiError(null);
    try { await login(data); navigate(roleHome); }
    catch { setApiError('Invalid credentials. Please check your email and password.'); }
  };

  return (
    <AuthShell panel={
      <>
        <div className="flex items-center gap-3 mb-auto">
          <div className="w-9 h-9 rounded-xl flex items-center justify-center"
            style={{ background: 'linear-gradient(135deg,#22c55e,#16a34a)' }}>
            <Shield size={18} className="text-white" />
          </div>
          <span className="font-black text-white text-[15px]" style={{ fontFamily: 'Manrope,sans-serif' }}>EgyMediChain</span>
        </div>
        <div className="mt-auto">
          <h1 className="font-black text-5xl text-white leading-[1.08] mb-5" style={{ fontFamily: 'Manrope,sans-serif' }}>
            Integrity in<br />every <span style={{ color: '#4ae176' }}>molecular</span><br />link.
          </h1>
          <p className="text-[14px] leading-relaxed max-w-[340px]" style={{ color: 'rgba(255,255,255,.5)' }}>
            Egypt's national pharmaceutical supply chain management system — securing every step from manufacturer to patient.
          </p>
          <div className="flex gap-10 mt-10 pt-8 border-t border-white/10">
            {[['99.9%', 'Uptime'], ['2.4M', 'Batches Verified'], ['61K+', 'Active Pharmacies']].map(([v, l]) => (
              <div key={l}>
                <p className="font-black text-2xl text-white" style={{ fontFamily: 'Manrope,sans-serif' }}>{v}</p>
                <p className="text-[11px] uppercase tracking-widest mt-1" style={{ color: 'rgba(255,255,255,.3)', fontWeight: 600 }}>{l}</p>
              </div>
            ))}
          </div>
        </div>
      </>
    }>
      {/* Mobile logo */}
      <div className="flex items-center gap-2 mb-8 lg:hidden">
        <div className="w-8 h-8 rounded-xl flex items-center justify-center" style={{ background: 'linear-gradient(135deg,#006e2f,#0058be)' }}>
          <Shield size={16} className="text-white" />
        </div>
        <span className="font-black text-[#191c1e] text-[15px]" style={{ fontFamily: 'Manrope,sans-serif' }}>EgyMediChain</span>
      </div>

      <h2 className="font-black text-3xl text-[#191c1e] mb-1.5" style={{ fontFamily: 'Manrope,sans-serif' }}>Welcome Back</h2>
      <p className="text-[14px] text-[#6d7b6c] mb-8">Access your pharmaceutical logistics dashboard.</p>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {/* Email */}
        <div className="space-y-1.5">
          <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">Work Email</label>
          <div className="relative">
            <svg className="absolute left-4 top-1/2 -translate-y-1/2 text-[#6d7b6c]" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" /><polyline points="22,6 12,13 2,6" /></svg>
            <input {...register('email')} type="email" placeholder="name@organization.eg" className={inputWithIcon} />
          </div>
          {errors.email && <p className="text-[11px] text-[#ba1a1a] font-medium">{errors.email.message as string}</p>}
        </div>

        {/* Password */}
        <div className="space-y-1.5">
          <div className="flex items-center justify-between">
            <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">Password</label>
            <Link to="/forgot-password" className="text-[12px] font-semibold hover:underline" style={{ color: C.blue }}>Forgot?</Link>
          </div>
          <div className="relative">
            <svg className="absolute left-4 top-1/2 -translate-y-1/2 text-[#6d7b6c]" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><rect x="3" y="11" width="18" height="11" rx="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" /></svg>
            <input {...register('password')} type={showPw ? 'text' : 'password'} placeholder="••••••••••••"
              className={`${inputWithIcon} pr-12`} />
            <button type="button" onClick={() => setShowPw(!showPw)}
              className="absolute right-4 top-1/2 -translate-y-1/2 text-[#6d7b6c] hover:text-[#191c1e] transition-colors">
              {showPw ? <EyeOff size={16} /> : <Eye size={16} />}
            </button>
          </div>
          {errors.password && <p className="text-[11px] text-[#ba1a1a] font-medium">{errors.password.message as string}</p>}
        </div>

        {apiError && (
          <div className="flex items-center gap-2.5 px-4 py-3 rounded-xl" style={{ background: 'rgba(255,218,214,.5)' }}>
            <AlertCircle size={15} style={{ color: C.error, flexShrink: 0 }} />
            <p className="text-[12px] font-medium" style={{ color: '#93000a' }}>{apiError}</p>
          </div>
        )}

        <button type="submit" disabled={isSubmitting}
          className="w-full py-3.5 rounded-xl text-white font-bold text-[14px] flex items-center justify-center gap-2 transition-all hover:-translate-y-px disabled:opacity-50"
          style={{ fontFamily: 'Manrope,sans-serif', background: `linear-gradient(135deg,${C.green},${C.blue})`, boxShadow: '0 4px 14px rgba(0,110,47,.25)' }}>
          {isSubmitting ? <RefreshCw size={16} className="animate-spin" /> : <>Secure Sign In <ArrowRight size={16} /></>}
        </button>
      </form>

      <p className="text-center text-[13px] text-[#6d7b6c] mt-6">
        Don't have an account?{' '}
        <Link to="/register" className="font-bold hover:underline" style={{ color: C.blue }}>Create account</Link>
      </p>

      <div className="mt-8 pt-6 border-t border-[rgba(188,203,185,.18)] flex justify-between text-[11px]" style={{ color: 'rgba(109,123,108,.5)' }}>
        <span>© 2025 EgyMediChain</span>
        <div className="flex gap-3">
          <button className="hover:opacity-80">Privacy</button>
          <button className="hover:opacity-80">System Status</button>
        </div>
      </div>
    </AuthShell>
  );
}

// ─── REGISTER — Dynamic fields per role ──────────────────────────
// Base schema
const baseSchema = {
  name: z.string().min(2, 'Full name required'),
  email: z.string().email('Valid email required'),
  phone: z.string().min(11, 'Valid phone required'),
  password: z.string().min(8, 'Min 8 characters'),
  confirmPassword: z.string(),
  walletAddress: z.string().optional(),
};

// Role-specific extra fields
const roleFields: Record<UserRole, Record<string, z.ZodTypeAny>> = {
  Ministry: { nationalId: z.string().min(14, '14-digit national ID required') },
  Manufacturer: { commercialRegistry: z.string().min(4, 'Commercial registry required'), factoryLicense: z.string().min(4, 'Factory license required') },
  Warehouse: { warehouseLicense: z.string().min(4, 'Warehouse license required'), governorate: z.string().min(2, 'Governorate required') },
  Pharmacy: { pharmacyLicense: z.string().min(4, 'Pharmacy license required'), syndicateNumber: z.string().min(4, 'Syndicate number required') },
  Consumer: { nationalId: z.string().min(14, '14-digit national ID required') },
};

const ROLE_FIELD_LABELS: Record<string, { label: string; placeholder: string; icon: React.ReactNode }> = {
  nationalId: { label: 'National ID', placeholder: '29912345678901', icon: <User size={15} /> },
  commercialRegistry: { label: 'Commercial Registry', placeholder: 'CR-2024-XXXXXXX', icon: <Building2 size={15} /> },
  factoryLicense: { label: 'Factory License No.', placeholder: 'FL-XXXXXXX', icon: <Shield size={15} /> },
  warehouseLicense: { label: 'Warehouse License No.', placeholder: 'WH-XXXXXXX', icon: <Warehouse size={15} /> },
  governorate: { label: 'Governorate', placeholder: 'e.g. Cairo', icon: <Building2 size={15} /> },
  pharmacyLicense: { label: 'Pharmacy License No.', placeholder: 'PH-XXXXXXX', icon: <Pill size={15} /> },
  syndicateNumber: { label: 'Syndicate Number', placeholder: 'SYN-XXXXXXX', icon: <Shield size={15} /> },
};

// OTP input component
function OtpInput({ onComplete, loading, onResend, countdown }: {
  onComplete: (otp: string) => void;
  loading: boolean;
  onResend: () => void;
  countdown: number;
}) {
  const [digits, setDigits] = useState(['', '', '', '', '', '']);
  const refs = useRef<(HTMLInputElement | null)[]>([]);

  const handleChange = (i: number, val: string) => {
    const d = val.replace(/\D/g, '').slice(-1);
    const next = [...digits];
    next[i] = d;
    setDigits(next);
    if (d && i < 5) refs.current[i + 1]?.focus();
    if (next.every(v => v) && next.join('').length === 6) onComplete(next.join(''));
  };

  const handleKeyDown = (i: number, e: React.KeyboardEvent) => {
    if (e.key === 'Backspace' && !digits[i] && i > 0) refs.current[i - 1]?.focus();
  };

  const handlePaste = (e: React.ClipboardEvent) => {
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6);
    if (pasted.length === 6) {
      setDigits(pasted.split(''));
      onComplete(pasted);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex gap-2 justify-center" onPaste={handlePaste}>
        {digits.map((d, i) => (
          <input key={i} ref={el => { refs.current[i] = el; }}
            type="text" inputMode="numeric" maxLength={1} value={d}
            onChange={e => handleChange(i, e.target.value)}
            onKeyDown={e => handleKeyDown(i, e)}
            className="w-12 h-14 text-center text-2xl font-black rounded-xl border-0 outline-none transition-all"
            style={{
              fontFamily: 'Manrope,sans-serif',
              background: d ? C.white : C.surface,
              color: C.dark,
              boxShadow: d ? `0 0 0 3px ${C.blue}30` : 'none',
            }} />
        ))}
      </div>
      <div className="flex items-center justify-center gap-2 text-[13px]">
        {countdown > 0 ? (
          <span className="text-[#6d7b6c]">Resend in <b className="text-[#191c1e]">{countdown}s</b></span>
        ) : (
          <button onClick={onResend} disabled={loading}
            className="flex items-center gap-1.5 font-semibold hover:underline disabled:opacity-50 transition-colors"
            style={{ color: C.blue }}>
            <RefreshCw size={13} /> Resend OTP
          </button>
        )}
      </div>
    </div>
  );
}

// MetaMask connect button
function MetaMaskButton({ onConnect, address }: { onConnect: (addr: string) => void; address?: string }) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const connect = async () => {
    setLoading(true); setError(null);
    try {
      const win = window as any;
      if (!win.ethereum) throw new Error('MetaMask not installed');
      const [addr] = await win.ethereum.request({ method: 'eth_requestAccounts' });
      onConnect(addr);
    } catch (e: any) {
      setError(e.message?.includes('not installed')
        ? 'MetaMask not installed. Install it from metamask.io'
        : 'Connection rejected.');
    } finally { setLoading(false); }
  };

  if (address) return (
    <div className="flex items-center gap-3 px-4 py-3 rounded-xl" style={{ background: 'rgba(0,110,47,.07)' }}>
      <CheckCircle size={16} style={{ color: C.green, flexShrink: 0 }} />
      <div className="min-w-0">
        <p className="text-[11px] font-bold text-[#006e2f] uppercase tracking-widest">Wallet Connected</p>
        <p className="text-[12px] text-[#191c1e] font-mono truncate">{address}</p>
      </div>
    </div>
  );

  return (
    <div className="space-y-2">
      <button type="button" onClick={connect} disabled={loading}
        className="w-full py-3 rounded-xl font-bold text-[13px] flex items-center justify-center gap-2 transition-all border-2 hover:-translate-y-px disabled:opacity-50"
        style={{ borderColor: '#f97316', color: '#f97316', background: 'rgba(249,115,22,.06)' }}>
        <Wallet size={16} />
        {loading ? 'Connecting…' : 'Connect MetaMask Wallet'}
      </button>
      {error && <p className="text-[11px] text-[#ba1a1a] font-medium">{error}</p>}
      <p className="text-[11px] text-[#6d7b6c] text-center">Optional but required for blockchain-verified roles</p>
    </div>
  );
}

export function RegisterPage() {
  const navigate = useNavigate();
  const [step, setStep] = useState<'role' | 'info' | 'otp' | 'done'>('role');
  const [selectedRole, setRole] = useState<UserRole | null>(null);
  const [walletAddress, setWallet] = useState<string | undefined>();
  const [pendingEmail, setPendingEmail] = useState('');
  const [otpLoading, setOtpLoading] = useState(false);
  const [otpError, setOtpError] = useState<string | null>(null);
  const [countdown, setCountdown] = useState(0);
  const [apiError, setApiError] = useState<string | null>(null);

  // Build schema dynamically when role changes
  const schema = selectedRole
    ? z.object({ ...baseSchema, ...roleFields[selectedRole] })
      .refine(d => d.password === d.confirmPassword, { message: 'Passwords do not match', path: ['confirmPassword'] })
    : z.object(baseSchema);

  const { register, handleSubmit, reset, formState: { errors, isSubmitting }, getValues } = useForm({
    resolver: zodResolver(schema),
  });

  // Countdown timer
  useEffect(() => {
    if (countdown <= 0) return;
    const t = setTimeout(() => setCountdown(c => c - 1), 1000);
    return () => clearTimeout(t);
  }, [countdown]);

  const startCountdown = () => setCountdown(60);

  const onSelectRole = (r: UserRole) => {
    setRole(r);
    reset();
    setStep('info');
  };

  const onSubmitInfo = async (data: any) => {
    setApiError(null);
    try {
      // MOCK: simulate sending OTP
      // await authApi.register({ ...data, role: selectedRole!, walletAddress });
      setPendingEmail(data.email);
      setStep('otp');
      startCountdown();
    } catch (e: any) {
      setApiError(e.message ?? 'Registration failed.');
    }
  };

  const onOtpComplete = async (otp: string) => {
    setOtpLoading(true); setOtpError(null);
    try {
      // MOCK: simulate OTP verify
      // await authApi.verifyCode({ email: pendingEmail, code: otp });
      await new Promise(r => setTimeout(r, 800));
      setStep('done');
    } catch {
      setOtpError('Invalid OTP. Please try again.');
    } finally { setOtpLoading(false); }
  };

  const onResend = async () => {
    try {
      // await authApi.forgotPassword({ email: pendingEmail });
      startCountdown();
    } catch { /**/ }
  };

  const extraFields = selectedRole ? Object.keys(roleFields[selectedRole]) : [];
  const roleInfo = ROLES.find(r => r.value === selectedRole);

  return (
    <AuthShell panel={
      <>
        <div className="flex items-center gap-3 mb-auto">
          <div className="w-9 h-9 rounded-xl flex items-center justify-center"
            style={{ background: 'linear-gradient(135deg,#22c55e,#16a34a)' }}>
            <Shield size={18} className="text-white" />
          </div>
          <span className="font-black text-white text-[15px]" style={{ fontFamily: 'Manrope,sans-serif' }}>EgyMediChain</span>
        </div>
        <div className="mt-auto space-y-4">
          <h2 className="font-black text-3xl text-white leading-tight" style={{ fontFamily: 'Manrope,sans-serif' }}>
            Securing the <span style={{ color: '#4ae176' }}>Life Science</span> Supply Chain.
          </h2>
          <p className="text-[14px] leading-relaxed" style={{ color: 'rgba(255,255,255,.5)' }}>
            Join Egypt's national pharmaceutical governance platform — blockchain-backed, ministry-verified.
          </p>
          {['GDP & GMP Compliant', 'Real-time Temperature Monitoring', 'Blockchain Audit Trail'].map(f => (
            <div key={f} className="flex items-center gap-3 rounded-xl px-4 py-3" style={{ background: 'rgba(255,255,255,.06)' }}>
              <CheckCircle size={15} style={{ color: '#4ae176', flexShrink: 0 }} />
              <span className="text-[13px] font-medium text-white/80">{f}</span>
            </div>
          ))}
        </div>
      </>
    }>
      {/* ── Step 1: Role picker ── */}
      {step === 'role' && (
        <>
          <h2 className="font-black text-3xl text-[#191c1e] mb-1" style={{ fontFamily: 'Manrope,sans-serif' }}>Create Account</h2>
          <p className="text-[14px] text-[#6d7b6c] mb-7">Select your role in the supply chain.</p>
          <div className="space-y-3">
            {ROLES.map(r => (
              <button key={r.value} onClick={() => onSelectRole(r.value)}
                className="w-full flex items-center gap-4 px-4 py-4 rounded-2xl border-0 text-left transition-all hover:-translate-y-0.5 hover:shadow-[0_4px_20px_rgba(25,28,30,0.1)]"
                style={{ background: C.surface }}>
                <div className="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                  style={{ background: `${r.color}18`, color: r.color }}>
                  {r.icon}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="font-bold text-[14px] text-[#191c1e]">{r.label}</p>
                  <p className="text-[12px] text-[#6d7b6c]">{r.arabic}</p>
                </div>
                <ArrowRight size={16} className="text-[#bccbb9] flex-shrink-0" />
              </button>
            ))}
          </div>
          <p className="text-center text-[13px] text-[#6d7b6c] mt-8">
            Already have an account?{' '}
            <Link to="/login" className="font-bold hover:underline" style={{ color: C.blue }}>Sign In</Link>
          </p>
        </>
      )}

      {/* ── Step 2: Info form ── */}
      {step === 'info' && selectedRole && (
        <>
          <button onClick={() => setStep('role')} className="flex items-center gap-1.5 text-[13px] text-[#6d7b6c] hover:text-[#191c1e] transition-colors mb-6">
            <ArrowLeft size={14} /> Back
          </button>
          {/* Role badge */}
          <div className="flex items-center gap-3 mb-6 px-4 py-3 rounded-xl" style={{ background: `${roleInfo?.color}12` }}>
            <div className="w-8 h-8 rounded-lg flex items-center justify-center" style={{ background: `${roleInfo?.color}20`, color: roleInfo?.color }}>{roleInfo?.icon}</div>
            <div>
              <p className="font-bold text-[13px] text-[#191c1e]">Registering as: {roleInfo?.label}</p>
              <p className="text-[11px] text-[#6d7b6c]">{roleInfo?.arabic}</p>
            </div>
          </div>

          <h2 className="font-black text-2xl text-[#191c1e] mb-6" style={{ fontFamily: 'Manrope,sans-serif' }}>Your Information</h2>

          <form onSubmit={handleSubmit(onSubmitInfo)} className="space-y-4">
            {/* Base fields */}
            {[
              { name: 'name' as const, label: 'Full Name', type: 'text', placeholder: 'Dr. Ahmed Hassan' },
              { name: 'email' as const, label: 'Work Email', type: 'email', placeholder: 'name@org.eg' },
              { name: 'phone' as const, label: 'Phone Number', type: 'tel', placeholder: '010XXXXXXXX' },
            ].map(f => (
              <div key={f.name} className="space-y-1.5">
                <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">{f.label}</label>
                <input {...register(f.name)} type={f.type} placeholder={f.placeholder} className={inputCls} />
                {errors[f.name] && <p className="text-[11px] text-[#ba1a1a] font-medium">{errors[f.name]?.message}</p>}
              </div>
            ))}

            {/* Password */}
            <div className="space-y-1.5">
              <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">Password</label>
              <input {...register('password')} type="password" placeholder="Min 8 characters" className={inputCls} />
              {errors.password && <p className="text-[11px] text-[#ba1a1a] font-medium">{(errors.password as any)?.message}</p>}
            </div>
            <div className="space-y-1.5">
              <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">Confirm Password</label>
              <input {...register('confirmPassword')} type="password" placeholder="Repeat password" className={inputCls} />
              {errors.confirmPassword && <p className="text-[11px] text-[#ba1a1a] font-medium">{(errors.confirmPassword as any)?.message}</p>}
            </div>

            {/* Role-specific fields */}
            {extraFields.length > 0 && (
              <div className="pt-2 border-t border-[rgba(188,203,185,.2)] space-y-4">
                <p className="text-[11px] font-bold uppercase tracking-widest text-[#6d7b6c]">Role-specific documents</p>
                // بدل ما تعمل register(f) على string مجهول النوع
                // استخدم type assertion

                {extraFields.map(f => {
                  const meta = ROLE_FIELD_LABELS[f];
                  const fieldKey = f as 'name' | 'email' | 'phone' | 'password' | 'confirmPassword' | 'walletAddress';
                  return (
                    <div key={f} className="space-y-1.5">
                      <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">{meta.label}</label>
                      <div className="relative">
                        <span className="absolute left-4 top-1/2 -translate-y-1/2 text-[#6d7b6c]">{meta.icon}</span>
                        {/* Fix: use Controller or direct ref with any */}
                        <input
                          placeholder={meta.placeholder}
                          className={inputWithIcon}
                          {...(register as any)(f)}
                        />
                      </div>
                      {(errors as any)[f] && (
                        <p className="text-[11px] text-[#ba1a1a] font-medium">
                          {((errors as any)[f] as { message?: string })?.message}
                        </p>
                      )}
                    </div>
                  );
                })}
              </div>
            )}

            {/* MetaMask */}
            <div className="pt-2 border-t border-[rgba(188,203,185,.2)]">
              <p className="text-[11px] font-bold uppercase tracking-widest text-[#6d7b6c] mb-3">Blockchain Linkage</p>
              <MetaMaskButton onConnect={addr => { setWallet(addr); }} address={walletAddress} />
            </div>

            {apiError && (
              <div className="flex items-center gap-2.5 px-4 py-3 rounded-xl" style={{ background: 'rgba(255,218,214,.5)' }}>
                <AlertCircle size={14} style={{ color: C.error }} />
                <p className="text-[12px] font-medium" style={{ color: '#93000a' }}>{apiError}</p>
              </div>
            )}

            <button type="submit" disabled={isSubmitting}
              className="w-full py-3.5 rounded-xl text-white font-bold text-[14px] flex items-center justify-center gap-2 transition-all hover:-translate-y-px disabled:opacity-50"
              style={{ fontFamily: 'Manrope,sans-serif', background: `linear-gradient(135deg,${C.green},${C.blue})`, boxShadow: '0 4px 14px rgba(0,110,47,.25)' }}>
              {isSubmitting ? <RefreshCw size={16} className="animate-spin" /> : <>Continue <ArrowRight size={16} /></>}
            </button>
          </form>
        </>
      )}

      {/* ── Step 3: OTP ── */}
      {step === 'otp' && (
        <div className="space-y-6">
          <button onClick={() => setStep('info')} className="flex items-center gap-1.5 text-[13px] text-[#6d7b6c] hover:text-[#191c1e] transition-colors">
            <ArrowLeft size={14} /> Back
          </button>
          <div className="text-center space-y-2">
            <div className="w-16 h-16 rounded-2xl mx-auto flex items-center justify-center" style={{ background: 'rgba(0,88,190,.1)' }}>
              <Shield size={28} style={{ color: C.blue }} />
            </div>
            <h2 className="font-black text-2xl text-[#191c1e]" style={{ fontFamily: 'Manrope,sans-serif' }}>Verify Your Email</h2>
            <p className="text-[13px] text-[#6d7b6c]">
              We sent a 6-digit code to<br />
              <span className="font-bold text-[#191c1e]">{pendingEmail}</span>
            </p>
          </div>

          <OtpInput onComplete={onOtpComplete} loading={otpLoading} onResend={onResend} countdown={countdown} />

          {otpLoading && (
            <div className="flex items-center justify-center gap-2 text-[13px] text-[#6d7b6c]">
              <RefreshCw size={14} className="animate-spin" /> Verifying…
            </div>
          )}
          {otpError && <p className="text-center text-[12px] font-medium" style={{ color: C.error }}>{otpError}</p>}

          <div className="px-4 py-3 rounded-xl" style={{ background: 'rgba(0,88,190,.06)' }}>
            <p className="text-[12px] text-[#0058be] font-medium text-center">
              🔒 Code expires in 10 minutes. Do not share it with anyone.
            </p>
          </div>
        </div>
      )}

      {/* ── Step 4: Done ── */}
      {step === 'done' && (
        <div className="space-y-6 text-center">
          <div className="w-20 h-20 rounded-full mx-auto flex items-center justify-center"
            style={{ background: `linear-gradient(135deg,${C.green},${C.blue})`, boxShadow: '0 8px 24px rgba(0,110,47,.3)' }}>
            <CheckCircle size={38} className="text-white" />
          </div>
          <div>
            <h2 className="font-black text-3xl text-[#191c1e] mb-2" style={{ fontFamily: 'Manrope,sans-serif' }}>Account Created!</h2>
            <p className="text-[14px] text-[#6d7b6c] leading-relaxed">
              Your account has been created and is pending verification by the Ministry of Health.
              You'll receive an email once approved.
            </p>
          </div>
          {walletAddress && (
            <div className="px-4 py-3 rounded-xl text-left" style={{ background: 'rgba(0,110,47,.07)' }}>
              <p className="text-[11px] font-bold text-[#006e2f] uppercase tracking-widest mb-1">Wallet Linked</p>
              <p className="text-[12px] font-mono text-[#191c1e] break-all">{walletAddress}</p>
            </div>
          )}
          <button onClick={() => navigate('/login')}
            className="w-full py-3.5 rounded-xl text-white font-bold text-[14px] transition-all hover:-translate-y-px"
            style={{ fontFamily: 'Manrope,sans-serif', background: `linear-gradient(135deg,${C.green},${C.blue})`, boxShadow: '0 4px 14px rgba(0,110,47,.25)' }}>
            Go to Login
          </button>
        </div>
      )}
    </AuthShell>
  );
}

// ─── FORGOT PASSWORD ─────────────────────────────────────────────
export function ForgotPasswordPage() {
  const [step, setStep] = useState<'email' | 'sent'>('email');
  const [email, setEmail] = useState('');
  const [loading, setLoad] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email) return;
    setLoad(true); setError(null);
    try {
      await authApi.forgotPassword({ email });
      setStep('sent');
    } catch { setError('Failed to send reset link. Check your email.'); }
    finally { setLoad(false); }
  };

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-[#f7f9fb] px-4 py-12">
      <div className="w-full max-w-md space-y-5">
        {/* Logo */}
        <div className="text-center space-y-3">
          <div className="w-14 h-14 rounded-2xl mx-auto flex items-center justify-center shadow-[0_4px_16px_rgba(0,110,47,.25)]"
            style={{ background: `linear-gradient(135deg,${C.green},${C.blue})` }}>
            <Shield size={26} className="text-white" />
          </div>
          <p className="font-black text-[18px] text-[#191c1e]" style={{ fontFamily: 'Manrope,sans-serif' }}>EgyMediChain</p>
        </div>

        {/* Card */}
        <div className="card-clinical">
          {step === 'email' ? (
            <form onSubmit={submit} className="space-y-5">
              <div>
                <h2 className="font-black text-2xl text-[#191c1e] mb-1" style={{ fontFamily: 'Manrope,sans-serif' }}>Forgot Password?</h2>
                <p className="text-[13px] text-[#6d7b6c]">Enter your work email and we'll send you a reset link.</p>
              </div>
              <div className="space-y-1.5">
                <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">Work Email</label>
                <div className="relative">
                  <svg className="absolute left-4 top-1/2 -translate-y-1/2 text-[#6d7b6c]" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" /><polyline points="22,6 12,13 2,6" /></svg>
                  <input type="email" placeholder="name@pharmachain.com" value={email} onChange={e => setEmail(e.target.value)} required className={inputWithIcon} />
                </div>
              </div>
              {error && <p className="text-[12px] font-medium" style={{ color: C.error }}>{error}</p>}
              <button type="submit" disabled={loading}
                className="w-full py-3.5 rounded-xl text-white font-bold text-[14px] flex items-center justify-center gap-2 transition-all hover:-translate-y-px disabled:opacity-50"
                style={{ fontFamily: 'Manrope,sans-serif', background: `linear-gradient(135deg,${C.green},${C.blue})`, boxShadow: '0 4px 14px rgba(0,110,47,.25)' }}>
                {loading ? <RefreshCw size={15} className="animate-spin" /> : <>Send Reset Link <ArrowRight size={15} /></>}
              </button>
              <Link to="/login" className="flex items-center justify-center gap-1.5 text-[13px] text-[#6d7b6c] hover:text-[#191c1e] transition-colors">
                <ArrowLeft size={13} /> Back to Login
              </Link>
            </form>
          ) : (
            <div className="text-center space-y-4 py-2">
              <div className="w-14 h-14 rounded-full mx-auto flex items-center justify-center" style={{ background: 'rgba(0,110,47,.1)' }}>
                <CheckCircle size={28} style={{ color: C.green }} />
              </div>
              <h2 className="font-black text-xl text-[#191c1e]" style={{ fontFamily: 'Manrope,sans-serif' }}>Check your inbox</h2>
              <p className="text-[13px] text-[#6d7b6c]">We sent a reset link to <strong className="text-[#191c1e]">{email}</strong></p>
              <button onClick={() => setStep('email')} className="text-[13px] font-semibold hover:underline" style={{ color: C.blue }}>
                Try a different email
              </button>
            </div>
          )}
        </div>

        {/* Security note */}
        <div className="card-clinical">
          <div className="flex items-start gap-3">
            <Shield size={17} style={{ color: C.blue, flexShrink: 0, marginTop: 1 }} />
            <div>
              <p className="text-[10px] font-bold uppercase tracking-widest mb-1" style={{ color: C.blue }}>Security Protocol</p>
              <p className="text-[12px] text-[#3d4a3d] leading-relaxed">Multi-factor authentication may be required upon reset to maintain GxP compliance and cold-chain integrity protocols.</p>
            </div>
          </div>
        </div>

        <p className="text-center text-[11px]" style={{ color: 'rgba(109,123,108,.5)' }}>
          © 2025 EgyMediChain Systems • System Version 4.2.1-clinical
        </p>
      </div>
    </div>
  );
}

// ─── RESET PASSWORD ───────────────────────────────────────────────
const resetSchema = z.object({
  newPassword: z.string().min(8, 'Min 8 characters'),
  confirmPassword: z.string(),
}).refine(d => d.newPassword === d.confirmPassword, { message: 'Passwords do not match', path: ['confirmPassword'] });

export function ResetPasswordPage() {
  const navigate = useNavigate();
  const [success, setSuccess] = useState(false);
  const [showPw, setShowPw] = useState(false);
  const params = new URLSearchParams(window.location.hash.split('?')[1] ?? '');
  const email = params.get('email') ?? '';
  const token = params.get('code') ?? '';

  const { register, handleSubmit, watch, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(resetSchema),
  });
  const pw = watch('newPassword', '');
  const checks = [
    { label: '8 Characters min.', ok: pw.length >= 8 },
    { label: 'One uppercase', ok: /[A-Z]/.test(pw) },
    { label: 'Special symbol', ok: /[^A-Za-z0-9]/.test(pw) },
    { label: 'Numerical digit', ok: /[0-9]/.test(pw) },
  ];

  const onSubmit = async (data: any) => {
    try {
      await authApi.resetPassword({ email, token, newPassword: data.newPassword, confirmPassword: data.confirmPassword });
      setSuccess(true);
    } catch { /**/ }
  };

  if (success) return (
    <div className="min-h-screen flex items-center justify-center bg-[#f7f9fb] px-4">
      <div className="w-full max-w-md card-clinical text-center space-y-5">
        <div className="w-20 h-20 rounded-full mx-auto flex items-center justify-center"
          style={{ background: `linear-gradient(135deg,${C.green},${C.blue})`, boxShadow: '0 8px 24px rgba(0,110,47,.3)' }}>
          <CheckCircle size={36} className="text-white" />
        </div>
        <h2 className="font-black text-3xl text-[#191c1e]" style={{ fontFamily: 'Manrope,sans-serif' }}>Password Updated</h2>
        <p className="text-[14px] text-[#6d7b6c]">Your credentials have been updated. You can now sign in.</p>
        <button onClick={() => navigate('/login')}
          className="w-full py-3.5 rounded-xl text-white font-bold transition-all hover:-translate-y-px"
          style={{ fontFamily: 'Manrope,sans-serif', background: `linear-gradient(135deg,${C.green},${C.blue})` }}>
          Back to Login
        </button>
        <p className="text-[11px]" style={{ color: 'rgba(109,123,108,.5)' }}>
          🔒 Security Log: ID-{Math.random().toString(36).slice(2, 10).toUpperCase()}-SUCCESS
        </p>
      </div>
    </div>
  );

  return (
    <div className="min-h-screen flex items-center justify-center bg-[#f7f9fb] px-4 py-12">
      <div className="w-full max-w-md space-y-4">
        <div className="text-center">
          <div className="w-12 h-12 rounded-2xl mx-auto flex items-center justify-center mb-3" style={{ background: `linear-gradient(135deg,${C.green},${C.blue})` }}>
            <Shield size={22} className="text-white" />
          </div>
          <h2 className="font-black text-2xl text-[#191c1e]" style={{ fontFamily: 'Manrope,sans-serif' }}>Reset Password</h2>
          <p className="text-[13px] text-[#6d7b6c] mt-1">Create a new secure password for your account.</p>
        </div>
        <div className="card-clinical">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1.5">
              <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">New Password</label>
              <div className="relative">
                <input {...register('newPassword')} type={showPw ? 'text' : 'password'} placeholder="••••••••" className={`${inputCls} pr-12`} />
                <button type="button" onClick={() => setShowPw(!showPw)} className="absolute right-4 top-1/2 -translate-y-1/2 text-[#6d7b6c]">
                  {showPw ? <EyeOff size={15} /> : <Eye size={15} />}
                </button>
              </div>
              {errors.newPassword && <p className="text-[11px]" style={{ color: C.error }}>{(errors.newPassword as any)?.message}</p>}
            </div>
            <div className="space-y-1.5">
              <label className="text-[11px] font-bold uppercase tracking-widest text-[#3d4a3d]">Confirm Password</label>
              <input {...register('confirmPassword')} type={showPw ? 'text' : 'password'} placeholder="••••••••" className={inputCls} />
              {errors.confirmPassword && <p className="text-[11px]" style={{ color: C.error }}>{(errors.confirmPassword as any)?.message}</p>}
            </div>

            {/* Requirements checklist */}
            <div className="rounded-xl p-4" style={{ background: C.surface }}>
              <p className="text-[10px] font-bold uppercase tracking-widest text-[#6d7b6c] mb-3">Requirements</p>
              <div className="grid grid-cols-2 gap-2">
                {checks.map(c => (
                  <div key={c.label} className="flex items-center gap-2 text-[12px]">
                    <div className="w-4 h-4 rounded-full flex items-center justify-center flex-shrink-0"
                      style={{ background: c.ok ? 'rgba(0,110,47,.1)' : 'transparent', border: c.ok ? 'none' : '1.5px solid #bccbb9' }}>
                      {c.ok && <CheckCircle size={12} style={{ color: C.green }} />}
                    </div>
                    <span style={{ color: c.ok ? C.dark : '#6d7b6c' }}>{c.label}</span>
                  </div>
                ))}
              </div>
            </div>

            <button type="submit" disabled={isSubmitting}
              className="w-full py-3.5 rounded-xl text-white font-bold text-[14px] flex items-center justify-center gap-2 transition-all hover:-translate-y-px disabled:opacity-50"
              style={{ fontFamily: 'Manrope,sans-serif', background: `linear-gradient(135deg,${C.green},${C.blue})` }}>
              {isSubmitting ? <RefreshCw size={15} className="animate-spin" /> : 'Update Password'}
            </button>
            <Link to="/login" className="flex items-center justify-center gap-1.5 text-[13px] text-[#6d7b6c] hover:text-[#191c1e] transition-colors">
              <ArrowLeft size={13} /> Back to Login
            </Link>
          </form>
        </div>
      </div>
    </div>
  );
}