import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import { tokenStore } from '../api/client';
import { authApi } from '../api/endpoints';
import type { AuthUser, LoginPayload, UserRole } from '../types';

const ROLE_HOME: Record<UserRole, string> = {
  Ministry:     '/dashboard',
  Manufacturer: '/dashboard',
  Warehouse:    '/inventory',
  Pharmacy:     '/pharmacy',
  Consumer:     '/dashboard',
};

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (payload: LoginPayload) => Promise<void>;
  logout: () => void;
  hasRole: (...roles: UserRole[]) => boolean;
  roleHome: string;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const loadStoredUser = (): AuthUser | null => {
  try {
    const raw = localStorage.getItem('egm_user');
    return raw ? (JSON.parse(raw) as AuthUser) : null;
  } catch { return null; }
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser]      = useState<AuthUser | null>(loadStoredUser);
  const [isLoading, setLoad] = useState(false);

  const login = useCallback(async (payload: LoginPayload) => {
    setLoad(true);
    try {
      // ── MOCK — remove when backend ready ──
      const mockUser: AuthUser = {
        id: 1, email: payload.email,
        name: 'Dr. Amira Hassan',
        role: 'Ministry', entityId: 0,
        entityName: 'Ministry of Health',
      };
      tokenStore.setTokens('mock_access', 'mock_refresh');
      localStorage.setItem('egm_user', JSON.stringify(mockUser));
      setUser(mockUser);
      return;
      // ── END MOCK ──
      // eslint-disable-next-line no-unreachable
      const result = await authApi.login(payload);
      tokenStore.setTokens(result.accessToken, result.refreshToken);
      localStorage.setItem('egm_user', JSON.stringify(result.user));
      setUser(result.user);
    } finally { setLoad(false); }
  }, []);

  const logout = useCallback(() => {
    tokenStore.clear();
    localStorage.removeItem('egm_user');
    setUser(null);
  }, []);

  const hasRole = useCallback(
    (...roles: UserRole[]) => !!user && roles.includes(user.role),
    [user]
  );

  // Fix: cast role to UserRole explicitly before indexing
  const roleHome = user ? ROLE_HOME[user.role as UserRole] : '/dashboard';

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout, hasRole, roleHome }}>
      {children}
    </AuthContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be inside AuthProvider');
  return ctx;
}