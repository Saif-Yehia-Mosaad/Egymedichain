import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { User, Lock, Bell, Globe, Shield, Eye, EyeOff, CheckCircle } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { Card, CardHeader, Button, Input, Toggle } from '../../components/ui';

const profileSchema = z.object({
  name: z.string().min(2, 'Required'),
  email: z.string().email('Valid email required'),
  phone: z.string().min(11, 'Phone required'),
});

const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'Required'),
  newPassword: z.string().min(8, 'Min 8 characters'),
  confirmPassword: z.string(),
}).refine((d) => d.newPassword === d.confirmPassword, { message: 'Passwords do not match', path: ['confirmPassword'] });

type ProfileForm = z.infer<typeof profileSchema>;
type PasswordForm = z.infer<typeof passwordSchema>;

export function SettingsPage() {
  const { user } = useAuth();
  const [showPw, setShowPw] = useState(false);
  const [profileSaved, setProfileSaved] = useState(false);
  const [passwordSaved, setPasswordSaved] = useState(false);

  const [prefs, setPrefs] = useState({
    emailAlerts: true,
    lowStockAlerts: true,
    expiryAlerts: true,
    suspiciousAlerts: true,
    realtimeNotifs: true,
    darkMode: false,
    compactMode: false,
    language: 'en',
  });

  const { register: regProfile, handleSubmit: handleProfile, formState: { errors: profileErrors, isSubmitting: profileLoading } } = useForm<ProfileForm>({
    resolver: zodResolver(profileSchema),
    values: user ? { name: user.name, email: user.email, phone: '' } : undefined,
  });

  const { register: regPw, handleSubmit: handlePw, reset: resetPw, formState: { errors: pwErrors, isSubmitting: pwLoading } } = useForm<PasswordForm>({
    resolver: zodResolver(passwordSchema),
  });

  const onProfileSave = async (_data: ProfileForm) => {
    // await usersApi.updateProfile(data) — placeholder
    await new Promise((r) => setTimeout(r, 800));
    setProfileSaved(true);
    setTimeout(() => setProfileSaved(false), 3000);
  };

  const onPasswordSave = async (_data: PasswordForm) => {
    await new Promise((r) => setTimeout(r, 800));
    setPasswordSaved(true);
    resetPw();
    setTimeout(() => setPasswordSaved(false), 3000);
  };

  const AVATAR_COLORS: Record<string, string> = {
    Ministry: 'from-violet-400 to-violet-600',
    Manufacturer: 'from-blue-400 to-blue-600',
    Warehouse: 'from-orange-400 to-orange-600',
    Pharmacy: 'from-emerald-400 to-emerald-600',
  };

  const SECTIONS = [
    { id: 'profile', label: 'Profile', icon: <User size={15} /> },
    { id: 'security', label: 'Security', icon: <Lock size={15} /> },
    { id: 'notifications', label: 'Notifications', icon: <Bell size={15} /> },
    { id: 'preferences', label: 'Preferences', icon: <Globe size={15} /> },
  ];

  const [activeSection, setActiveSection] = useState('profile');

  return (
    <div className="space-y-6 max-w-3xl">
      <div>
        <h1 className="text-2xl font-black text-slate-800">Settings</h1>
        <p className="text-slate-400 text-sm mt-0.5">Manage your account and preferences</p>
      </div>

      <div className="flex gap-6">
        {/* Sidebar nav */}
        <div className="w-44 flex-shrink-0">
          <nav className="space-y-1">
            {SECTIONS.map((s) => (
              <button key={s.id} onClick={() => setActiveSection(s.id)}
                className={`w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
                  activeSection === s.id ? 'bg-emerald-500 text-white shadow-md shadow-emerald-200' : 'text-slate-500 hover:bg-slate-100'
                }`}>
                {s.icon} {s.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Content */}
        <div className="flex-1 space-y-5">
          {/* Profile section */}
          {activeSection === 'profile' && (
            <>
              <Card>
                <CardHeader title="Profile Information" subtitle="Update your personal details" />
                {/* Avatar */}
                <div className="flex items-center gap-4 mb-6 pb-6 border-b border-slate-100">
                  <div className={`w-16 h-16 rounded-2xl bg-gradient-to-br ${AVATAR_COLORS[user?.role ?? 'Ministry']} flex items-center justify-center text-white text-2xl font-black shadow-lg`}>
                    {user?.name?.[0]?.toUpperCase() ?? 'U'}
                  </div>
                  <div>
                    <p className="font-bold text-slate-800">{user?.name}</p>
                    <p className="text-sm text-slate-400">{user?.role} · {user?.entityName}</p>
                    <div className="flex items-center gap-2 mt-2">
                      <Shield size={12} className="text-emerald-500" />
                      <span className="text-xs text-emerald-600 font-semibold">Verified Account</span>
                    </div>
                  </div>
                </div>

                <form onSubmit={handleProfile(onProfileSave)} className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="col-span-2">
                      <Input label="Full Name" error={profileErrors.name?.message} {...regProfile('name')} />
                    </div>
                    <Input label="Email" type="email" error={profileErrors.email?.message} {...regProfile('email')} />
                    <Input label="Phone" type="tel" error={profileErrors.phone?.message} {...regProfile('phone')} />
                  </div>
                  <div className="flex items-center gap-3">
                    <Button type="submit" variant="gradient" isLoading={profileLoading}>Save Profile</Button>
                    {profileSaved && (
                      <span className="flex items-center gap-1.5 text-sm text-emerald-600 font-semibold">
                        <CheckCircle size={14} /> Saved!
                      </span>
                    )}
                  </div>
                </form>
              </Card>
            </>
          )}

          {/* Security section */}
          {activeSection === 'security' && (
            <Card>
              <CardHeader title="Change Password" subtitle="Keep your account secure" />
              <form onSubmit={handlePw(onPasswordSave)} className="space-y-4">
                <div className="flex flex-col gap-1.5">
                  <label className="text-sm font-medium text-slate-700">Current Password</label>
                  <Input
                    type={showPw ? 'text' : 'password'}
                    placeholder="••••••••"
                    leftAddon={<Lock size={14} />}
                    rightAddon={<button type="button" onClick={() => setShowPw((v) => !v)}>{showPw ? <EyeOff size={14} /> : <Eye size={14} />}</button>}
                    error={pwErrors.currentPassword?.message}
                    {...regPw('currentPassword')}
                  />
                </div>
                <Input
                  label="New Password"
                  type={showPw ? 'text' : 'password'}
                  placeholder="Min 8 characters"
                  leftAddon={<Lock size={14} />}
                  error={pwErrors.newPassword?.message}
                  {...regPw('newPassword')}
                />
                <Input
                  label="Confirm New Password"
                  type={showPw ? 'text' : 'password'}
                  placeholder="Repeat new password"
                  leftAddon={<Lock size={14} />}
                  error={pwErrors.confirmPassword?.message}
                  {...regPw('confirmPassword')}
                />
                <div className="flex items-center gap-3">
                  <Button type="submit" variant="gradient" isLoading={pwLoading}>Update Password</Button>
                  {passwordSaved && <span className="flex items-center gap-1.5 text-sm text-emerald-600 font-semibold"><CheckCircle size={14} /> Password updated!</span>}
                </div>
              </form>
            </Card>
          )}

          {/* Notifications section */}
          {activeSection === 'notifications' && (
            <Card>
              <CardHeader title="Notification Preferences" subtitle="Choose what alerts you receive" />
              <div className="space-y-4">
                {[
                  { key: 'emailAlerts', label: 'Email Alerts', desc: 'Receive alerts via email' },
                  { key: 'lowStockAlerts', label: 'Low Stock Alerts', desc: 'Notify when stock falls below threshold' },
                  { key: 'expiryAlerts', label: 'Expiry Warnings', desc: 'Warn about medicines expiring in 45 days' },
                  { key: 'suspiciousAlerts', label: 'Suspicious Transfer Alerts', desc: 'Detect unusual transfer patterns' },
                  { key: 'realtimeNotifs', label: 'Real-Time Notifications', desc: 'Enable SignalR live notifications' },
                ].map((pref) => (
                  <div key={pref.key} className="flex items-center justify-between py-3 border-b border-slate-100 last:border-0">
                    <div>
                      <p className="text-sm font-semibold text-slate-700">{pref.label}</p>
                      <p className="text-xs text-slate-400 mt-0.5">{pref.desc}</p>
                    </div>
                    <Toggle
                      checked={prefs[pref.key as keyof typeof prefs] as boolean}
                      onChange={(v) => setPrefs((p) => ({ ...p, [pref.key]: v }))}
                    />
                  </div>
                ))}
              </div>
              <div className="mt-4">
                <Button variant="gradient" onClick={() => {}}>Save Preferences</Button>
              </div>
            </Card>
          )}

          {/* Preferences section */}
          {activeSection === 'preferences' && (
            <Card>
              <CardHeader title="App Preferences" subtitle="Customize your experience" />
              <div className="space-y-4">
                {[
                  { key: 'darkMode', label: 'Dark Mode', desc: 'Switch to dark theme (coming soon)' },
                  { key: 'compactMode', label: 'Compact Mode', desc: 'Reduce spacing for more data on screen' },
                ].map((pref) => (
                  <div key={pref.key} className="flex items-center justify-between py-3 border-b border-slate-100 last:border-0">
                    <div>
                      <p className="text-sm font-semibold text-slate-700">{pref.label}</p>
                      <p className="text-xs text-slate-400 mt-0.5">{pref.desc}</p>
                    </div>
                    <Toggle
                      checked={prefs[pref.key as keyof typeof prefs] as boolean}
                      onChange={(v) => setPrefs((p) => ({ ...p, [pref.key]: v }))}
                    />
                  </div>
                ))}
                <div className="py-3">
                  <p className="text-sm font-semibold text-slate-700 mb-2">Language</p>
                  <select
                    value={prefs.language}
                    onChange={(e) => setPrefs((p) => ({ ...p, language: e.target.value }))}
                    className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm text-slate-800 focus:outline-none focus:ring-2 focus:ring-emerald-400/40"
                  >
                    <option value="en">English</option>
                    <option value="ar">العربية (Arabic)</option>
                  </select>
                </div>
              </div>
              <div className="mt-4">
                <Button variant="gradient">Save Preferences</Button>
              </div>
            </Card>
          )}

          {/* API info card */}
          <Card className="bg-slate-50 border-slate-200">
            <CardHeader title="API Configuration" subtitle="Backend connection settings" />
            <div className="space-y-2 text-xs font-mono">
              <div className="flex justify-between">
                <span className="text-slate-400">API Base URL</span>
                <span className="text-slate-700">{import.meta.env.VITE_API_URL ?? 'https://localhost:7001/api'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">SignalR Hub</span>
                <span className="text-slate-700">{import.meta.env.VITE_HUB_URL ?? 'https://localhost:7001/hubs/alerts'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Your Role</span>
                <span className="text-emerald-600 font-bold">{user?.role}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Entity</span>
                <span className="text-slate-700">{user?.entityName}</span>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
