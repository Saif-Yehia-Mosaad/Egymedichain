import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ShieldCheck, Eye, EyeOff, Mail, Lock, User, ArrowLeft, KeyRound, RefreshCw } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../api/endpoints';
import { Button, Input } from '../../components/ui';

// ─── Shared Auth Shell ─────────────────────────────────────────
function AuthShell({ children, title, subtitle }: {
  children: React.ReactNode; title: string; subtitle: string;
}) {
  return (
    <div className="min-h-screen flex bg-slate-50">
      
      {/* ✅ LEFT PANEL (تم التعديل هنا فقط) */}
      <div className="flex md:w-1/2 w-full bg-gradient-to-br from-emerald-500 via-emerald-600 to-blue-600 relative overflow-hidden flex-col items-center justify-center p-12">
        
        <div className="absolute inset-0 opacity-10">
          {Array.from({ length: 12 }).map((_, i) => (
            <div
              key={i}
              className="absolute rounded-full border border-white"
              style={{
                width: `${(i + 1) * 80}px`,
                height: `${(i + 1) * 80}px`,
                top: '50%',
                left: '50%',
                transform: 'translate(-50%, -50%)',
              }}
            />
          ))}
        </div>

        <div className="relative z-10 text-center text-white space-y-6">
          <div className="w-20 h-20 rounded-3xl bg-white/20 backdrop-blur flex items-center justify-center mx-auto shadow-2xl">
            <ShieldCheck size={40} className="text-white" />
          </div>

          <div>
            <h1 className="text-3xl font-black tracking-tight">EgyMediChain</h1>
            <p className="text-emerald-100 text-lg mt-1">Drug Supply Chain Governance</p>
          </div>

          <p className="text-emerald-100/80 text-sm max-w-xs leading-relaxed">
            National platform for tracking & verifying pharmaceutical supply chains across Egypt. Aligned with Vision 2030.
          </p>

          <div className="grid grid-cols-3 gap-3 pt-4">
            {[
              { label: 'Medicines Tracked', value: '8.4M+' },
              { label: 'Pharmacies', value: '60K+' },
              { label: 'Uptime', value: '99.9%' },
            ].map((s) => (
              <div key={s.label} className="bg-white/10 rounded-2xl p-3 text-center backdrop-blur">
                <p className="text-white font-bold text-lg">{s.value}</p>
                <p className="text-emerald-100/70 text-[11px] mt-0.5">{s.label}</p>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* RIGHT PANEL (زي ما هو) */}
      <div className="md:w-1/2 w-full flex items-center justify-center p-6 bg-white">
        <div className="w-full max-w-md space-y-8">

          <div className="flex items-center gap-3 lg:hidden">
            <div className="w-10 h-10 rounded-2xl bg-gradient-to-br from-emerald-500 to-blue-500 flex items-center justify-center">
              <ShieldCheck size={20} className="text-white" />
            </div>
            <div>
              <p className="font-black text-slate-800">EgyMediChain</p>
              <p className="text-xs text-slate-400">Supply Chain Governance</p>
            </div>
          </div>

          <div>
            <h2 className="text-2xl font-black text-slate-800">{title}</h2>
            <p className="text-slate-400 mt-1 text-sm">{subtitle}</p>
          </div>

          {children}
        </div>
      </div>
    </div>
  );
}

// ─── Login ─────────────────────────────────────────────────────
const loginSchema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});
type LoginForm = z.infer<typeof loginSchema>;

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [showPw, setShowPw] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginForm) => {
    setError(null);
    try {
      await login(data);
      navigate('/dashboard');
    } catch (e: unknown) {
      setError((e as { message?: string })?.message ?? 'Login failed. Check credentials.');
    }
  };

  return (
    <AuthShell title="Welcome back" subtitle="Sign in to your EgyMediChain account">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        <Input
          label="Email address"
          type="email"
          placeholder="admin@moh.gov.eg"
          leftAddon={<Mail size={14} />}
          error={errors.email?.message}
          {...register('email')}
        />
        <div className="flex flex-col gap-1.5">
          <div className="flex items-center justify-between">
            <label className="text-sm font-medium text-slate-700">Password</label>
            <Link to="/forgot-password" className="text-xs text-emerald-600 hover:text-emerald-700 font-semibold">
              Forgot password?
            </Link>
          </div>
          <Input
            type={showPw ? 'text' : 'password'}
            placeholder="••••••••"
            leftAddon={<Lock size={14} />}
            rightAddon={
              <button type="button" onClick={() => setShowPw((v) => !v)} className="hover:text-slate-600">
                {showPw ? <EyeOff size={14} /> : <Eye size={14} />}
              </button>
            }
            error={errors.password?.message}
            {...register('password')}
          />
        </div>

        {error && (
          <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">
            {error}
          </div>
        )}

        <Button type="submit" variant="gradient" size="lg" isLoading={isSubmitting} className="w-full">
          Sign in
        </Button>

        <p className="text-center text-sm text-slate-400">
          Don't have an account?{' '}
          <Link to="/register" className="text-emerald-600 font-semibold hover:text-emerald-700">
            Create one
          </Link>
        </p>
      </form>
    </AuthShell>
  );
}

// ─── Register ─────────────────────────────────────────────────
const registerSchema = z.object({
  name: z.string().min(2, 'Name required'),
  email: z.string().email('Valid email required'),
  phone: z.string().min(11, 'Phone required'),
  role: z.enum(['Manufacturer', 'Warehouse', 'Pharmacy']),
  password: z.string().min(8, 'Min 8 characters'),
  confirmPassword: z.string(),
}).refine((d) => d.password === d.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});
type RegisterForm = z.infer<typeof registerSchema>;

export function RegisterPage() {
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [showPw, setShowPw] = useState(false);

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterForm) => {
    setError(null);
    try {
      await authApi.register(data);
      navigate('/login');
    } catch (e: unknown) {
      setError((e as { message?: string })?.message ?? 'Registration failed');
    }
  };

  return (
    <AuthShell title="Create account" subtitle="Register your organization on EgyMediChain">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input label="Full Name" placeholder="Dr. Ahmed Hassan" leftAddon={<User size={14} />} error={errors.name?.message} {...register('name')} />
        <Input label="Email" type="email" placeholder="email@organization.eg" leftAddon={<Mail size={14} />} error={errors.email?.message} {...register('email')} />
        <Input label="Phone" type="tel" placeholder="01XXXXXXXXX" error={errors.phone?.message} {...register('phone')} />

        <div className="flex flex-col gap-1.5">
          <label className="text-sm font-medium text-slate-700">Role</label>
          <select
            className="w-full rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-emerald-400/40 focus:border-emerald-400"
            {...register('role')}
          >
            <option value="">Select your role</option>
            <option value="Manufacturer">Manufacturer — صانع أدوية</option>
            <option value="Warehouse">Warehouse — مستودع</option>
            <option value="Pharmacy">Pharmacy — صيدلية</option>
          </select>
          {errors.role && <p className="text-xs text-red-500">{errors.role.message}</p>}
        </div>

        <Input label="Password" type={showPw ? 'text' : 'password'} placeholder="Min 8 characters" leftAddon={<Lock size={14} />}
          rightAddon={<button type="button" onClick={() => setShowPw((v) => !v)}>{showPw ? <EyeOff size={14} /> : <Eye size={14} />}</button>}
          error={errors.password?.message} {...register('password')} />
        <Input label="Confirm Password" type={showPw ? 'text' : 'password'} placeholder="Repeat password" leftAddon={<Lock size={14} />}
          error={errors.confirmPassword?.message} {...register('confirmPassword')} />

        {error && <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">{error}</div>}

        <Button type="submit" variant="gradient" size="lg" isLoading={isSubmitting} className="w-full">
          Register Organization
        </Button>

        <p className="text-center text-sm text-slate-400">
          Already registered?{' '}
          <Link to="/login" className="text-emerald-600 font-semibold hover:text-emerald-700">Sign in</Link>
        </p>
      </form>
    </AuthShell>
  );
}

// ─── Forgot Password — Step 1: Enter email ────────────────────
export function ForgotPasswordPage() {
  const [step, setStep] = useState<'email' | 'code' | 'done'>('email');
  const [email, setEmail] = useState('');
  const [code, setCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [resendTimer, setResendTimer] = useState(0);

  const startResendTimer = () => {
    setResendTimer(60);
    const interval = setInterval(() => {
      setResendTimer((t) => {
        if (t <= 1) { clearInterval(interval); return 0; }
        return t - 1;
      });
    }, 1000);
  };

  const handleEmailSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email) return;
    setLoading(true); setError(null);
    try {
      await authApi.forgotPassword({ email });
      setStep('code');
      startResendTimer();
    } catch (err: unknown) {
      setError((err as { message?: string })?.message ?? 'Failed to send code');
    } finally { setLoading(false); }
  };

  const handleCodeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (code.length !== 6) { setError('Enter the 6-digit code'); return; }
    setLoading(true); setError(null);
    try {
      await authApi.verifyCode({ email, code });
      // Navigate to reset password with token
      window.location.href = `/#/reset-password?email=${encodeURIComponent(email)}&code=${code}`;
    } catch (err: unknown) {
      setError((err as { message?: string })?.message ?? 'Invalid or expired code');
    } finally { setLoading(false); }
  };

  const handleResend = async () => {
    if (resendTimer > 0) return;
    setLoading(true); setError(null);
    try {
      await authApi.forgotPassword({ email });
      startResendTimer();
    } catch (err: unknown) {
      setError((err as { message?: string })?.message ?? 'Failed to resend code');
    } finally { setLoading(false); }
  };

  return (
    <AuthShell title="Forgot password?" subtitle="We'll send a verification code to your email">
      {step === 'email' && (
        <form onSubmit={handleEmailSubmit} className="space-y-5">
          <div className="w-14 h-14 rounded-2xl bg-emerald-50 border border-emerald-100 flex items-center justify-center">
            <Mail size={24} className="text-emerald-500" />
          </div>
          <Input
            label="Email address"
            type="email"
            placeholder="your-email@organization.eg"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            leftAddon={<Mail size={14} />}
            required
          />
          {error && <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">{error}</div>}
          <Button type="submit" variant="gradient" size="lg" className="w-full" isLoading={loading}>
            Send verification code
          </Button>
          <Link to="/login" className="flex items-center justify-center gap-2 text-sm text-slate-400 hover:text-slate-600 transition-colors">
            <ArrowLeft size={14} /> Back to sign in
          </Link>
        </form>
      )}

      {step === 'code' && (
        <form onSubmit={handleCodeSubmit} className="space-y-5">
          <div className="w-14 h-14 rounded-2xl bg-emerald-50 border border-emerald-100 flex items-center justify-center">
            <KeyRound size={24} className="text-emerald-500" />
          </div>
          <div className="rounded-xl bg-emerald-50 border border-emerald-100 px-4 py-3">
            <p className="text-sm text-emerald-700">
              We sent a 6-digit code to <strong>{email}</strong>
            </p>
          </div>

          {/* OTP input */}
          <div className="flex flex-col gap-1.5">
            <label className="text-sm font-medium text-slate-700">Verification code</label>
            <input
              type="text"
              inputMode="numeric"
              maxLength={6}
              placeholder="000000"
              value={code}
              onChange={(e) => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
              className="w-full text-center text-2xl font-bold tracking-[0.5em] rounded-xl border border-slate-200 bg-white px-4 py-3 focus:outline-none focus:ring-2 focus:ring-emerald-400/40 focus:border-emerald-400 transition-all"
            />
          </div>

          {error && <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">{error}</div>}

          <Button type="submit" variant="gradient" size="lg" className="w-full" isLoading={loading} disabled={code.length !== 6}>
            Verify code
          </Button>

          <div className="flex items-center justify-between text-sm">
            <button
              type="button"
              onClick={() => { setStep('email'); setCode(''); setError(null); }}
              className="flex items-center gap-1 text-slate-400 hover:text-slate-600 transition-colors"
            >
              <ArrowLeft size={13} /> Change email
            </button>
            <button
              type="button"
              onClick={handleResend}
              disabled={resendTimer > 0 || loading}
              className="flex items-center gap-1 text-emerald-600 hover:text-emerald-700 disabled:text-slate-400 disabled:cursor-not-allowed font-semibold transition-colors"
            >
              <RefreshCw size={13} />
              {resendTimer > 0 ? `Resend in ${resendTimer}s` : 'Resend code'}
            </button>
          </div>
        </form>
      )}
    </AuthShell>
  );
}

// ─── Reset Password ────────────────────────────────────────────
const resetSchema = z.object({
  newPassword: z.string().min(8, 'Min 8 characters'),
  confirmPassword: z.string(),
}).refine((d) => d.newPassword === d.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});
type ResetForm = z.infer<typeof resetSchema>;

export function ResetPasswordPage() {
  const navigate = useNavigate();
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showPw, setShowPw] = useState(false);

  // Read email + code from hash query params
  const params = new URLSearchParams(window.location.hash.split('?')[1] ?? '');
  const email = params.get('email') ?? '';
  const token = params.get('code') ?? '';

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<ResetForm>({
    resolver: zodResolver(resetSchema),
  });

  const onSubmit = async (data: ResetForm) => {
    setError(null);
    try {
      await authApi.resetPassword({ email, token, newPassword: data.newPassword, confirmPassword: data.confirmPassword });
      setSuccess(true);
    } catch (e: unknown) {
      setError((e as { message?: string })?.message ?? 'Failed to reset password');
    }
  };

  return (
    <AuthShell title="Reset password" subtitle="Create a new secure password for your account">
      {success ? (
        <div className="space-y-5 text-center">
          <div className="w-16 h-16 rounded-full bg-emerald-100 flex items-center justify-center mx-auto">
            <ShieldCheck size={28} className="text-emerald-500" />
          </div>
          <div>
            <p className="font-bold text-slate-800 text-lg">Password changed!</p>
            <p className="text-slate-400 text-sm mt-1">Your password has been reset successfully.</p>
          </div>
          <Button variant="gradient" size="lg" className="w-full" onClick={() => navigate('/login')}>
            Sign in now
          </Button>
        </div>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
          <div className="w-14 h-14 rounded-2xl bg-blue-50 border border-blue-100 flex items-center justify-center">
            <Lock size={24} className="text-blue-500" />
          </div>
          <Input
            label="New Password"
            type={showPw ? 'text' : 'password'}
            placeholder="Min 8 characters"
            leftAddon={<Lock size={14} />}
            rightAddon={<button type="button" onClick={() => setShowPw((v) => !v)}>{showPw ? <EyeOff size={14} /> : <Eye size={14} />}</button>}
            error={errors.newPassword?.message}
            {...register('newPassword')}
          />
          <Input
            label="Confirm New Password"
            type={showPw ? 'text' : 'password'}
            placeholder="Repeat password"
            leftAddon={<Lock size={14} />}
            error={errors.confirmPassword?.message}
            {...register('confirmPassword')}
          />

          {/* Password strength hints */}
          <ul className="text-xs text-slate-400 space-y-1 pl-1">
            <li>• At least 8 characters</li>
            <li>• Mix of uppercase, lowercase, numbers</li>
            <li>• Avoid common words</li>
          </ul>

          {error && <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">{error}</div>}

          <Button type="submit" variant="gradient" size="lg" className="w-full" isLoading={isSubmitting}>
            Set new password
          </Button>
          <Link to="/login" className="flex items-center justify-center gap-2 text-sm text-slate-400 hover:text-slate-600 transition-colors">
            <ArrowLeft size={14} /> Back to sign in
          </Link>
        </form>
      )}
    </AuthShell>
  );
}
