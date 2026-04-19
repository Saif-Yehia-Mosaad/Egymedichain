import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuth } from '../../context/AuthContext';
import { authApi } from '../../api/endpoints';

// ─── Forgot Password ──────────────────────────────────────────────
export function ForgotPasswordPage() {
  const [step, setStep] = useState<'email' | 'success'>('email');
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email) return;
    setLoading(true);
    try { await authApi.forgotPassword({ email }); setStep('success'); }
    catch { /* show error */ }
    finally { setLoading(false); }
  };

  return (
    <body className="font-body text-[#191c1e] min-h-screen flex flex-col antialiased" style={{fontFamily:'Inter,sans-serif'}}>
      <main className="flex-grow flex items-center justify-center relative overflow-hidden p-6"
        style={{background:'radial-gradient(circle at top left, #f7f9fb 0%, #eceef0 100%)'}}>
        {/* decorative blobs */}
        <div className="absolute -top-24 -left-24 w-96 h-96 rounded-full blur-3xl" style={{background:'rgba(0,110,47,0.05)'}}/>
        <div className="absolute -bottom-24 -right-24 w-96 h-96 rounded-full blur-3xl" style={{background:'rgba(0,88,190,0.05)'}}/>

        <div className="w-full max-w-md z-10">
          {/* Branding */}
          <div className="flex flex-col items-center mb-10">
            <div className="w-16 h-16 rounded-xl flex items-center justify-center mb-4"
              style={{background:'linear-gradient(135deg,#006e2f,#0058be)',boxShadow:'0 8px 24px rgba(0,110,47,0.2)'}}>
              <span className="material-symbols-outlined text-white text-3xl" style={{fontVariationSettings:"'FILL' 1"}}>medical_services</span>
            </div>
            <h1 style={{fontFamily:'Manrope,sans-serif',fontWeight:800,fontSize:'1.5rem',letterSpacing:'-0.02em',color:'#191c1e'}}>
              BioChain Logistics
            </h1>
          </div>

          {/* Step 1: Email */}
          {step === 'email' && (
            <div className="p-8 rounded-xl border" style={{background:'rgba(255,255,255,0.7)',backdropFilter:'blur(24px)',borderColor:'rgba(255,255,255,0.4)',boxShadow:'0 4px 30px rgba(0,0,0,0.03)'}}>
              <div className="mb-8">
                <h2 style={{fontFamily:'Manrope,sans-serif',fontWeight:700,fontSize:'1.25rem',color:'#191c1e',marginBottom:'0.5rem'}}>Forgot Password?</h2>
                <p style={{color:'#3d4a3d',fontSize:'0.875rem',lineHeight:'1.6'}}>No worries, it happens. Enter the email associated with your logistics account and we'll send you a secure link.</p>
              </div>
              <form className="space-y-6" onSubmit={handleSubmit}>
                <div className="space-y-2">
                  <label className="text-xs font-bold uppercase tracking-wider ml-1" style={{color:'#3d4a3d'}}>Work Email</label>
                  <div className="relative group">
                    <div className="absolute inset-y-0 left-4 flex items-center pointer-events-none" style={{color:'#6d7b6c'}}>
                      <span className="material-symbols-outlined text-lg">mail</span>
                    </div>
                    <input
                      className="w-full pl-11 pr-4 py-3 rounded-xl outline-none transition-all"
                      style={{background:'#e0e3e5',border:'none',color:'#191c1e',fontSize:'0.875rem'}}
                      placeholder="name@pharmachain.com"
                      type="email" required value={email}
                      onChange={e => setEmail(e.target.value)}
                      onFocus={e => { e.target.style.background='#fff'; e.target.style.boxShadow='0 0 0 4px rgba(0,110,47,0.1)'; }}
                      onBlur={e => { e.target.style.background='#e0e3e5'; e.target.style.boxShadow='none'; }}
                    />
                  </div>
                </div>
                <button type="submit" disabled={loading}
                  className="w-full py-4 rounded-xl text-white flex items-center justify-center gap-2 transition-all"
                  style={{background:'linear-gradient(135deg,#006e2f,#0058be)',fontFamily:'Manrope,sans-serif',fontWeight:700,fontSize:'0.875rem',letterSpacing:'0.025em',boxShadow:'0 8px 24px rgba(0,110,47,0.2)'}}>
                  {loading ? 'Sending…' : 'Send Reset Link'}
                  <span className="material-symbols-outlined text-lg">arrow_forward</span>
                </button>
              </form>
              <div className="mt-8 pt-8 flex justify-center" style={{borderTop:'1px solid rgba(188,203,185,0.15)'}}>
                <Link to="/login" className="text-sm font-semibold flex items-center gap-2 transition-colors" style={{color:'#0058be'}}>
                  <span className="material-symbols-outlined text-base">arrow_back</span>
                  Back to Login
                </Link>
              </div>
            </div>
          )}

          {/* Step 2: Success */}
          {step === 'success' && (
            <div className="p-8 rounded-xl border text-center" style={{background:'rgba(255,255,255,0.7)',backdropFilter:'blur(24px)',borderColor:'rgba(255,255,255,0.4)',boxShadow:'0 4px 30px rgba(0,0,0,0.03)'}}>
              <div className="w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-6" style={{background:'rgba(34,197,94,0.15)'}}>
                <span className="material-symbols-outlined text-3xl" style={{color:'#006e2f',fontVariationSettings:"'FILL' 1"}}>mark_email_read</span>
              </div>
              <h2 style={{fontFamily:'Manrope,sans-serif',fontWeight:700,fontSize:'1.25rem',color:'#191c1e',marginBottom:'0.75rem'}}>Check your inbox</h2>
              <p style={{color:'#3d4a3d',fontSize:'0.875rem',lineHeight:'1.6',marginBottom:'2rem'}}>
                We've sent a password reset link to<br/>
                <span style={{color:'#191c1e',fontWeight:600}}>{email}</span>
              </p>
              <div className="space-y-4">
                <button className="w-full py-4 rounded-xl font-bold text-sm transition-all" style={{fontFamily:'Manrope,sans-serif',background:'#e0e3e5',color:'#191c1e'}}>
                  Open Email Client
                </button>
                <p style={{fontSize:'0.75rem',color:'#3d4a3d'}}>
                  Didn't receive an email?{' '}
                  <button onClick={() => setStep('email')} className="font-bold hover:underline" style={{color:'#0058be'}}>Resend link</button>
                </p>
              </div>
              <div className="mt-8 pt-8" style={{borderTop:'1px solid rgba(188,203,185,0.15)'}}>
                <Link to="/login" className="text-sm font-semibold transition-colors" style={{color:'#0058be'}}>Return to login screen</Link>
              </div>
            </div>
          )}

          {/* Security Protocol note */}
          <div className="mt-8 flex items-start gap-4 p-4 rounded-xl" style={{background:'rgba(33,112,228,0.05)',border:'1px solid rgba(33,112,228,0.1)'}}>
            <span className="material-symbols-outlined text-xl mt-0.5" style={{color:'#2170e4'}}>verified_user</span>
            <div>
              <h4 className="text-xs font-bold uppercase tracking-widest mb-1" style={{color:'#2170e4'}}>Security Protocol</h4>
              <p style={{fontSize:'0.75rem',color:'#3d4a3d',lineHeight:'1.6'}}>Multi-factor authentication may be required upon reset to maintain GxP compliance and cold-chain integrity protocols.</p>
            </div>
          </div>
        </div>

        {/* Decorative rotated card */}
        <div className="absolute -z-10 w-3/4 h-3/4 rounded-[4rem] opacity-40 pointer-events-none"
          style={{background:'#eceef0',transform:'rotate(6deg) translate(80px,80px)'}}/>
      </main>

      <footer className="p-8 text-center">
        <p style={{fontSize:'0.75rem',color:'#6d7b6c',fontWeight:500,letterSpacing:'-0.01em'}}>
          © 2024 BioChain Systems. Secure Pharmaceutical Logistics Network.
          <span className="mx-2">•</span>
          System Version 4.2.1-clinical
        </p>
      </footer>

      {/* Lab image corner */}
      <div className="fixed bottom-0 right-0 w-64 h-64 opacity-10 grayscale pointer-events-none overflow-hidden">
        <div className="w-full h-full" style={{background:'linear-gradient(135deg,#006e2f22,#0058be22)'}}/>
      </div>
    </body>
  );
}

// ─── Register ────────────────────────────────────────────────────
const registerSchema = z.object({
  name: z.string().min(2),
  email: z.string().email(),
  role: z.string().min(1),
  password: z.string().min(12, 'Minimum 12 characters with at least one symbol.'),
  terms: z.boolean().refine(v => v, 'You must accept the terms'),
});
type RegisterForm = z.infer<typeof registerSchema>;

export function RegisterPage() {
  const navigate = useNavigate();
  const [showPw, setShowPw] = useState(false);
  const [error, setError] = useState<string|null>(null);
  const { register, handleSubmit, formState:{errors,isSubmitting} } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
  });
  const onSubmit = async (data: RegisterForm) => {
    try { await authApi.register(data); navigate('/login'); }
    catch { setError('Registration failed.'); }
  };

  const inputStyle = {
    background:'#e0e3e5', border:'none', color:'#191c1e', fontSize:'0.875rem',
    width:'100%', borderRadius:'0.75rem', padding:'1rem 1rem 1rem 2.75rem',
    outline:'none', transition:'all 0.3s',
  };

  return (
    <main className="flex min-h-screen" style={{fontFamily:'Inter,sans-serif'}}>
      {/* Left: Image + Glass card */}
      <section className="hidden lg:flex lg:w-1/2 relative overflow-hidden items-center justify-center p-12"
        style={{background:'#eceef0'}}>
        {/* Background gradient overlay (lab atmosphere) */}
        <div className="absolute inset-0 z-0">
          <div className="w-full h-full" style={{background:'linear-gradient(135deg,rgba(0,110,47,0.15) 0%,rgba(0,88,190,0.15) 100%)'}}/>
        </div>
        {/* Glassmorphic branding card */}
        <div className="relative z-10 p-10 rounded-[2rem] max-w-lg"
          style={{background:'rgba(255,255,255,0.7)',backdropFilter:'blur(24px)',border:'1px solid rgba(255,255,255,0.2)',boxShadow:'0 20px 50px rgba(0,0,0,0.1)'}}>
          <div className="mb-8">
            <span className="inline-flex items-center justify-center p-3 rounded-xl mb-6"
              style={{background:'linear-gradient(135deg,#006e2f,#0058be)'}}>
              <span className="material-symbols-outlined text-white text-3xl">biotech</span>
            </span>
            <h1 style={{fontFamily:'Manrope,sans-serif',fontWeight:800,fontSize:'2.25rem',letterSpacing:'-0.02em',color:'#191c1e',lineHeight:1.2,marginBottom:'1rem'}}>
              Securing the <span style={{color:'#006e2f'}}>Life Science</span> Supply Chain.
            </h1>
            <p style={{color:'#3d4a3d',fontSize:'1.125rem',lineHeight:1.6}}>
              Join BioChain Logistics to access real-time visibility, automated cold-chain compliance, and blockchain-backed pharmaceutical tracking.
            </p>
          </div>
          <div className="space-y-4">
            {[
              { icon:'verified', text:'GDP & GMP Compliant Architecture' },
              { icon:'ac_unit',  text:'Real-time Temperature Monitoring' },
            ].map(f => (
              <div key={f.icon} className="flex items-center gap-4 p-4 rounded-xl" style={{background:'rgba(255,255,255,0.5)'}}>
                <span className="material-symbols-outlined" style={{color:'#006e2f'}}>{f.icon}</span>
                <span style={{fontSize:'0.875rem',fontWeight:600,color:'#191c1e'}}>{f.text}</span>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Right: Form */}
      <section className="w-full lg:w-1/2 flex items-center justify-center p-6 md:p-12 lg:p-24"
        style={{background:'#f7f9fb'}}>
        <div className="w-full max-w-md">
          <div className="mb-10">
            <h2 style={{fontFamily:'Manrope,sans-serif',fontWeight:700,fontSize:'1.875rem',color:'#191c1e',marginBottom:'0.5rem'}}>Create Account</h2>
            <p style={{color:'#3d4a3d'}}>Enter your credentials to access the logistics portal.</p>
          </div>

          <form className="space-y-6" onSubmit={handleSubmit(onSubmit)}>
            {/* Full Name */}
            <div className="space-y-2">
              <label className="block text-sm font-semibold ml-1" style={{color:'#3d4a3d'}}>Full Name</label>
              <div className="relative group">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none"><span className="material-symbols-outlined text-[20px]" style={{color:'#6d7b6c'}}>person</span></div>
                <input {...register('name')} style={inputStyle} placeholder="Dr. Sarah Jenkins" type="text"/>
              </div>
              {errors.name && <p className="text-xs ml-1" style={{color:'#ba1a1a'}}>{errors.name.message}</p>}
            </div>

            {/* Email */}
            <div className="space-y-2">
              <label className="block text-sm font-semibold ml-1" style={{color:'#3d4a3d'}}>Work Email</label>
              <div className="relative group">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none"><span className="material-symbols-outlined text-[20px]" style={{color:'#6d7b6c'}}>mail</span></div>
                <input {...register('email')} style={inputStyle} placeholder="s.jenkins@biochain.com" type="email"/>
              </div>
              {errors.email && <p className="text-xs ml-1" style={{color:'#ba1a1a'}}>{errors.email.message}</p>}
            </div>

            {/* Role */}
            <div className="space-y-2">
              <label className="block text-sm font-semibold ml-1" style={{color:'#3d4a3d'}}>System Role</label>
              <div className="relative group">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none"><span className="material-symbols-outlined text-[20px]" style={{color:'#6d7b6c'}}>badge</span></div>
                <select {...register('role')} style={{...inputStyle, paddingRight:'2.5rem', appearance:'none', cursor:'pointer'}}>
                  <option value="">Select your role</option>
                  <option value="Manufacturer">Manufacturer</option>
                  <option value="Warehouse">Warehouse Manager</option>
                  <option value="Pharmacy">Pharmacy Admin</option>
                </select>
                <div className="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none"><span className="material-symbols-outlined" style={{color:'#6d7b6c'}}>expand_more</span></div>
              </div>
              {errors.role && <p className="text-xs ml-1" style={{color:'#ba1a1a'}}>{errors.role.message}</p>}
            </div>

            {/* Password */}
            <div className="space-y-2">
              <label className="block text-sm font-semibold ml-1" style={{color:'#3d4a3d'}}>Password</label>
              <div className="relative group">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none"><span className="material-symbols-outlined text-[20px]" style={{color:'#6d7b6c'}}>lock</span></div>
                <input {...register('password')} style={{...inputStyle, paddingRight:'3rem'}} placeholder="••••••••••••" type={showPw?'text':'password'}/>
                <button type="button" onClick={()=>setShowPw(!showPw)} className="absolute inset-y-0 right-0 pr-4 flex items-center" style={{color:'#6d7b6c'}}>
                  <span className="material-symbols-outlined text-[20px]">{showPw?'visibility_off':'visibility'}</span>
                </button>
              </div>
              <p className="text-[11px] ml-1 mt-1" style={{color:'#3d4a3d'}}>Minimum 12 characters with at least one symbol.</p>
              {errors.password && <p className="text-xs ml-1" style={{color:'#ba1a1a'}}>{errors.password.message}</p>}
            </div>

            {/* Terms */}
            <div className="flex items-start gap-3 py-2">
              <input {...register('terms')} id="terms" type="checkbox" className="w-5 h-5 rounded-md mt-0.5" style={{accentColor:'#006e2f'}}/>
              <label htmlFor="terms" className="text-xs leading-relaxed" style={{color:'#3d4a3d'}}>
                I agree to the{' '}
                <a href="#" className="font-bold hover:underline" style={{color:'#006e2f'}}>Terms of Service</a>{' '}
                and{' '}
                <a href="#" className="font-bold hover:underline" style={{color:'#006e2f'}}>Data Protection Policy</a>{' '}
                for pharmaceutical handling.
              </label>
            </div>

            {error && <p className="text-sm" style={{color:'#ba1a1a'}}>{error}</p>}

            <button type="submit" disabled={isSubmitting}
              className="w-full py-4 rounded-xl text-white font-bold transition-all"
              style={{fontFamily:'Manrope,sans-serif',background:'linear-gradient(135deg,#006e2f,#0058be)',boxShadow:'0 10px 30px rgba(0,110,47,0.2)'}}>
              {isSubmitting ? 'Creating account…' : 'Complete Registration'}
            </button>
          </form>

          <div className="mt-8 pt-8 flex flex-col items-center gap-4" style={{borderTop:'1px solid rgba(188,203,185,0.2)'}}>
            <p className="text-sm" style={{color:'#3d4a3d'}}>
              Already have an account?{' '}
              <Link to="/login" className="font-bold hover:underline" style={{color:'#0058be'}}>Sign In</Link>
            </p>
            <div className="flex gap-4">
              <button className="p-3 rounded-full transition-colors" style={{background:'#f2f4f6'}}>
                <span className="material-symbols-outlined text-[20px]" style={{color:'#6d7b6c'}}>fingerprint</span>
              </button>
            </div>
          </div>
        </div>
      </section>

      {/* Support FAB */}
      <div className="fixed bottom-8 right-8">
        <button className="p-4 rounded-full flex items-center justify-center group transition-all"
          style={{background:'#fff',boxShadow:'0 8px 32px rgba(0,0,0,0.12)',border:'1px solid rgba(188,203,185,0.1)',color:'#006e2f'}}>
          <span className="material-symbols-outlined">help_center</span>
        </button>
      </div>
    </main>
  );
}

// ─── Login ───────────────────────────────────────────────────────
const loginSchema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(6, 'Password required'),
});
type LoginForm = z.infer<typeof loginSchema>;

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [showPw, setShowPw] = useState(false);
  const [error, setError] = useState<string|null>(null);
  const { register, handleSubmit, formState:{errors,isSubmitting} } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
  });
  const onSubmit = async (data: LoginForm) => {
    setError(null);
    try { await login(data); navigate('/dashboard'); }
    catch { setError('Invalid credentials. Please try again.'); }
  };

  const inputStyle = {
    background:'#e0e3e5', border:'none', color:'#191c1e', fontSize:'0.875rem',
    width:'100%', borderRadius:'0.75rem', padding:'1rem 1rem 1rem 2.75rem',
    outline:'none', transition:'all 0.3s',
  };

  return (
    <main className="flex min-h-screen" style={{fontFamily:'Inter,sans-serif'}}>
      {/* Left panel */}
      <section className="hidden lg:flex lg:w-1/2 relative overflow-hidden items-center justify-center p-12"
        style={{background:'linear-gradient(160deg,#031a10 0%,#0a2a1a 40%,#051528 100%)'}}>
        <div className="absolute inset-0" style={{background:'radial-gradient(ellipse at 40% 50%,rgba(0,200,90,0.2),transparent 60%),radial-gradient(ellipse at 80% 20%,rgba(0,88,190,0.15),transparent 55%)'}}/>
        <div className="relative z-10 max-w-md">
          <div className="flex items-center gap-3 mb-12">
            <div className="w-9 h-9 rounded-xl flex items-center justify-center" style={{background:'linear-gradient(135deg,#22c55e,#16a34a)'}}>
              <span className="material-symbols-outlined text-white" style={{fontSize:'18px',fontVariationSettings:"'FILL' 1"}}>biotech</span>
            </div>
            <span style={{fontFamily:'Manrope,sans-serif',fontWeight:800,color:'white',fontSize:'16px'}}>BioChain</span>
          </div>
          <h1 style={{fontFamily:'Manrope,sans-serif',fontWeight:900,fontSize:'3rem',color:'white',lineHeight:1.1,marginBottom:'1.25rem'}}>
            Integrity in every<br/><span style={{color:'#4ae176'}}>molecular</span> link.
          </h1>
          <p style={{color:'rgba(255,255,255,0.5)',fontSize:'0.875rem',lineHeight:1.7,maxWidth:'360px'}}>
            The world's most advanced pharmaceutical supply chain management system, secured by biometric and blockchain verification.
          </p>
          <div className="flex gap-10 mt-10">
            <div>
              <p style={{fontFamily:'Manrope,sans-serif',fontWeight:900,fontSize:'1.875rem',color:'white'}}>99.9%</p>
              <p style={{fontSize:'11px',textTransform:'uppercase',letterSpacing:'0.1em',color:'rgba(255,255,255,0.3)',fontWeight:600,marginTop:'4px'}}>Uptime Reliability</p>
            </div>
            <div style={{width:'1px',background:'rgba(255,255,255,0.1)'}}/>
            <div>
              <p style={{fontFamily:'Manrope,sans-serif',fontWeight:900,fontSize:'1.875rem',color:'white'}}>2.4M</p>
              <p style={{fontSize:'11px',textTransform:'uppercase',letterSpacing:'0.1em',color:'rgba(255,255,255,0.3)',fontWeight:600,marginTop:'4px'}}>Batches Verified</p>
            </div>
          </div>
        </div>
      </section>

      {/* Right: Form */}
      <section className="w-full lg:w-1/2 flex items-center justify-center p-6 md:p-12 lg:p-20 overflow-y-auto"
        style={{background:'white'}}>
        <div className="w-full max-w-[400px]">
          <div className="mb-8">
            <h2 style={{fontFamily:'Manrope,sans-serif',fontWeight:900,fontSize:'1.875rem',color:'#191c1e',marginBottom:'0.25rem'}}>Welcome Back</h2>
            <p style={{color:'#3d4a3d',fontSize:'0.875rem'}}>Access your pharmaceutical logistics dashboard.</p>
          </div>

          <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
            <div className="space-y-2">
              <label className="text-[11px] font-bold uppercase tracking-widest" style={{color:'#3d4a3d'}}>Work Email</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-4 flex items-center pointer-events-none"><span className="material-symbols-outlined text-[18px]" style={{color:'#6d7b6c'}}>alternate_email</span></div>
                <input {...register('email')} style={inputStyle} placeholder="name@organization.com" type="email"/>
              </div>
              {errors.email && <p className="text-[11px]" style={{color:'#ba1a1a'}}>{errors.email.message}</p>}
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <label className="text-[11px] font-bold uppercase tracking-widest" style={{color:'#3d4a3d'}}>Security Key</label>
                <Link to="/forgot-password" className="text-[12px] font-semibold hover:underline" style={{color:'#0058be'}}>Forgot Password?</Link>
              </div>
              <div className="relative">
                <div className="absolute inset-y-0 left-4 flex items-center pointer-events-none"><span className="material-symbols-outlined text-[18px]" style={{color:'#6d7b6c'}}>lock</span></div>
                <input {...register('password')} style={{...inputStyle, paddingRight:'2.75rem'}} placeholder="••••••••••••" type={showPw?'text':'password'}/>
                <button type="button" onClick={()=>setShowPw(!showPw)} className="absolute inset-y-0 right-0 pr-4 flex items-center" style={{color:'#6d7b6c'}}>
                  <span className="material-symbols-outlined text-[18px]">{showPw?'visibility_off':'visibility'}</span>
                </button>
              </div>
              {errors.password && <p className="text-[11px]" style={{color:'#ba1a1a'}}>{errors.password.message}</p>}
            </div>

            <label className="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" className="w-4 h-4 rounded" style={{accentColor:'#006e2f'}}/>
              <span className="text-[13px]" style={{color:'#3d4a3d'}}>Keep this workstation authenticated</span>
            </label>

            {error && <p className="text-[12px] px-4 py-3 rounded-xl" style={{background:'rgba(255,218,214,0.5)',color:'#93000a'}}>{error}</p>}

            <button type="submit" disabled={isSubmitting}
              className="w-full py-4 rounded-xl text-white font-bold flex items-center justify-center gap-2 transition-all"
              style={{fontFamily:'Manrope,sans-serif',background:'linear-gradient(135deg,#006e2f,#0058be)',boxShadow:'0 8px 24px rgba(0,110,47,0.22)'}}>
              {isSubmitting ? 'Signing in…' : 'Secure Sign In'}
              <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
            </button>
          </form>

          <div className="mt-8 pt-6 flex justify-between text-[11px]" style={{borderTop:'1px solid rgba(188,203,185,0.2)',color:'rgba(109,123,108,0.6)'}}>
            <span>© 2024 EgyMediChain</span>
            <div className="flex gap-4">
              <button className="hover:opacity-100 transition-opacity">Privacy</button>
              <button className="hover:opacity-100 transition-opacity">System Status</button>
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}

// ─── Reset Password ───────────────────────────────────────────────
const resetSchema = z.object({
  newPassword: z.string().min(8),
  confirmPassword: z.string(),
}).refine(d => d.newPassword === d.confirmPassword, { message:'Passwords do not match', path:['confirmPassword'] });
type ResetForm = z.infer<typeof resetSchema>;

export function ResetPasswordPage() {
  const navigate = useNavigate();
  const [success, setSuccess] = useState(false);
  const [showPw, setShowPw] = useState(false);
  const params = new URLSearchParams(window.location.hash.split('?')[1] ?? '');
  const email = params.get('email') ?? '';
  const token = params.get('code') ?? '';
  const { register, handleSubmit, watch, formState:{errors,isSubmitting} } = useForm<ResetForm>({
    resolver: zodResolver(resetSchema),
  });
  const pw = watch('newPassword','');
  const checks = [
    {label:'12 Characters min.', ok: pw.length>=12},
    {label:'One uppercase',       ok: /[A-Z]/.test(pw)},
    {label:'Special symbol',      ok: /[^A-Za-z0-9]/.test(pw)},
    {label:'Numerical digit',     ok: /[0-9]/.test(pw)},
  ];
  const onSubmit = async (data: ResetForm) => {
    try {
      await authApi.resetPassword({ email, token, newPassword: data.newPassword, confirmPassword: data.confirmPassword });
      setSuccess(true);
    } catch { /* error */ }
  };

  const inputStyle = {
    background:'#e0e3e5', border:'none', color:'#191c1e', fontSize:'0.875rem',
    width:'100%', borderRadius:'0.75rem', padding:'1rem 1rem 1rem 1rem',
    outline:'none', transition:'all 0.3s',
  };

  return (
    <div className="flex min-h-screen" style={{fontFamily:'Inter,sans-serif'}}>
      {/* Left */}
      <section className="hidden lg:flex w-[55%] relative overflow-hidden" style={{background:'#1a1a2e'}}>
        <div className="absolute inset-0" style={{background:'linear-gradient(135deg,rgba(0,110,47,0.15),transparent 50%,rgba(0,88,190,0.1))'}}/>
        <div className="relative z-10 flex flex-col justify-end p-20 max-w-2xl">
          <div className="p-8 rounded-xl" style={{background:'rgba(255,255,255,0.07)',backdropFilter:'blur(24px)'}}>
            <h2 style={{fontFamily:'Manrope,sans-serif',fontWeight:800,fontSize:'1.875rem',color:'white',letterSpacing:'-0.02em',marginBottom:'1rem'}}>Integrity in every chain.</h2>
            <p style={{color:'rgba(255,255,255,0.6)',fontWeight:500,lineHeight:1.6}}>Our advanced security layer ensures that your pharmaceutical data remains untampered and accessible only to authorized personnel.</p>
          </div>
        </div>
      </section>

      {/* Right */}
      <section className="w-full lg:w-[45%] flex flex-col" style={{background:'#f7f9fb'}}>
        {/* Nav */}
        <nav className="flex justify-between items-center px-8 py-4" style={{borderBottom:'1px solid rgba(188,203,185,0.15)'}}>
          <div style={{fontFamily:'Manrope,sans-serif',fontWeight:700,fontSize:'1rem',background:'linear-gradient(90deg,#006e2f,#0058be)',WebkitBackgroundClip:'text',WebkitTextFillColor:'transparent'}}>Clinical Ethereal</div>
          <div className="hidden md:flex items-center gap-8">
            <a href="#" className="text-sm font-medium hover:opacity-80" style={{color:'#6d7b6c'}}>Support</a>
            <a href="#" className="text-sm font-medium hover:opacity-80" style={{color:'#6d7b6c'}}>Security Protocols</a>
            <Link to="/login" className="font-bold hover:opacity-80" style={{color:'#006e2f',fontSize:'0.875rem'}}>Back to Login</Link>
          </div>
        </nav>

        {success ? (
          /* Success state */
          <div className="flex-1 flex flex-col items-center justify-center p-8 lg:p-24 text-center">
            <div className="relative inline-block mb-10">
              <div className="w-24 h-24 rounded-full flex items-center justify-center" style={{background:'linear-gradient(135deg,#006e2f,#0058be)',boxShadow:'0 8px 32px rgba(0,110,47,0.3)'}}>
                <span className="material-symbols-outlined text-white text-5xl font-bold" style={{fontVariationSettings:"'FILL' 1"}}>check_circle</span>
              </div>
              <div className="absolute -inset-4 rounded-full" style={{border:'1px solid rgba(188,203,185,0.2)'}}/>
            </div>
            <h1 style={{fontFamily:'Manrope,sans-serif',fontWeight:800,fontSize:'2.5rem',color:'#191c1e',letterSpacing:'-0.02em',marginBottom:'1rem'}}>Password Updated</h1>
            <p style={{color:'#3d4a3d',fontSize:'1.125rem',fontWeight:500,lineHeight:1.6,marginBottom:'3rem'}}>
              Your security credentials have been successfully updated. You can now sign in with your new password.
            </p>
            <div className="space-y-4 w-full max-w-sm">
              <button onClick={() => navigate('/login')}
                className="w-full py-4 px-8 rounded-xl text-white font-bold transition-all"
                style={{fontFamily:'Manrope,sans-serif',fontSize:'1.125rem',background:'linear-gradient(90deg,#006e2f,#0058be)',boxShadow:'0 10px 30px rgba(0,110,47,0.2)'}}>
                Back to Login
              </button>
              <p className="text-sm font-medium flex items-center justify-center gap-2 pt-4" style={{color:'#6d7b6c'}}>
                <span className="material-symbols-outlined text-base">shield</span>
                Security Log: ID-88293-SUCCESS
              </p>
            </div>
          </div>
        ) : (
          /* Reset form */
          <div className="flex-1 flex items-center justify-center p-8 lg:p-16">
            <div className="w-full max-w-md">
              <h2 style={{fontFamily:'Manrope,sans-serif',fontWeight:800,fontSize:'2rem',color:'#191c1e',marginBottom:'0.5rem'}}>Reset Password</h2>
              <p style={{color:'#3d4a3d',fontSize:'0.875rem',marginBottom:'2rem'}}>Create a new secure password for your account.</p>

              <form className="space-y-5" onSubmit={handleSubmit(onSubmit)}>
                <div className="space-y-2">
                  <label className="text-[11px] font-bold uppercase tracking-widest" style={{color:'#3d4a3d'}}>New Password</label>
                  <div className="relative">
                    <input {...register('newPassword')} style={{...inputStyle, paddingRight:'2.75rem'}} placeholder="••••••••••••" type={showPw?'text':'password'}/>
                    <button type="button" onClick={()=>setShowPw(!showPw)} className="absolute inset-y-0 right-0 pr-4 flex items-center" style={{color:'#6d7b6c'}}>
                      <span className="material-symbols-outlined text-[18px]">{showPw?'visibility_off':'visibility'}</span>
                    </button>
                  </div>
                  {errors.newPassword && <p className="text-[11px]" style={{color:'#ba1a1a'}}>{errors.newPassword.message}</p>}
                </div>

                <div className="space-y-2">
                  <label className="text-[11px] font-bold uppercase tracking-widest" style={{color:'#3d4a3d'}}>Confirm Password</label>
                  <input {...register('confirmPassword')} style={{...inputStyle, paddingRight:'1rem'}} placeholder="••••••••••••" type={showPw?'text':'password'}/>
                  {errors.confirmPassword && <p className="text-[11px]" style={{color:'#ba1a1a'}}>{errors.confirmPassword.message}</p>}
                </div>

                {/* Security requirements */}
                <div className="p-4 rounded-xl" style={{background:'#eceef0'}}>
                  <div className="flex items-center gap-2 mb-3">
                    <span className="material-symbols-outlined text-[16px]" style={{color:'#0058be'}}>security</span>
                    <p className="text-[10px] font-bold uppercase tracking-widest" style={{color:'#3d4a3d'}}>Security Requirements</p>
                  </div>
                  <div className="grid grid-cols-2 gap-y-2">
                    {checks.map(c => (
                      <div key={c.label} className="flex items-center gap-1.5 text-[12px]">
                        <span className="material-symbols-outlined text-[14px]" style={{color:c.ok?'#006e2f':'#bccbb9'}}>
                          {c.ok?'check_circle':'radio_button_unchecked'}
                        </span>
                        <span style={{color:c.ok?'#191c1e':'#6d7b6c'}}>{c.label}</span>
                      </div>
                    ))}
                  </div>
                </div>

                <button type="submit" disabled={isSubmitting}
                  className="w-full py-4 rounded-xl text-white font-bold transition-all"
                  style={{fontFamily:'Manrope,sans-serif',background:'linear-gradient(135deg,#006e2f,#0058be)',boxShadow:'0 8px 24px rgba(0,110,47,0.22)'}}>
                  {isSubmitting ? 'Updating…' : 'Update Password'}
                </button>
              </form>

              <p className="text-center text-[12px] mt-4" style={{color:'rgba(109,123,108,0.5)'}}>
                Having trouble? <button className="hover:underline" style={{color:'#6d7b6c'}}>Contact bio-support</button>
              </p>
            </div>
          </div>
        )}

        {/* Footer */}
        <footer className="flex justify-between items-center px-12 py-6">
          <p className="text-[10px] font-medium uppercase tracking-widest" style={{color:'#bccbb9'}}>© 2024 BioChain Systems. All rights reserved.</p>
          <div className="flex gap-6">
            {['Privacy Policy','Terms of Service','GDPR Compliance'].map(l=>(
              <a key={l} href="#" className="text-[10px] font-medium uppercase tracking-widest hover:opacity-80 transition-colors" style={{color:'#bccbb9'}}>{l}</a>
            ))}
          </div>
        </footer>
      </section>
    </div>
  );
}