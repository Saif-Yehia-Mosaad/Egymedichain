import { HashRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import { AppLayout } from './components/layout/AppLayout';
import { LoginPage, RegisterPage, ForgotPasswordPage, ResetPasswordPage } from './pages/auth';
import { DashboardPage }  from './pages/dashboard';
import { MedicinesPage, MedicineFormPage, MedicineDetailPage } from './pages/medicines';
import { InventoryPage }  from './pages/inventory';
import { WarehousesPage } from './pages/warehouses';
import { PharmacyPage }   from './pages/pharmacy';
import { TransactionsPage } from './pages/transactions';
import { AlertsPage }     from './pages/alerts';
import { UsersPage }      from './pages/users';
import { AuditPage }      from './pages/audit';
import { SettingsPage }   from './pages/settings';

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, gcTime: 5*60_000, refetchOnWindowFocus: false } },
});

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

function GuestRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth();
  if (isAuthenticated) return <Navigate to="/dashboard" replace />;
  return <>{children}</>;
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login"          element={<GuestRoute><LoginPage /></GuestRoute>} />
      <Route path="/register"       element={<GuestRoute><RegisterPage /></GuestRoute>} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password"  element={<ResetPasswordPage />} />
      <Route path="/" element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard"       element={<DashboardPage />} />
        <Route path="medicines"       element={<MedicinesPage />} />
        <Route path="medicines/new"   element={<MedicineFormPage />} />
        <Route path="medicines/:id"   element={<MedicineDetailPage />} />
        <Route path="medicines/:id/edit" element={<MedicineFormPage />} />
        <Route path="inventory"       element={<InventoryPage />} />
        <Route path="warehouses"      element={<WarehousesPage />} />
        <Route path="pharmacy"        element={<PharmacyPage />} />
        <Route path="transactions"    element={<TransactionsPage />} />
        <Route path="alerts"          element={<AlertsPage />} />
        <Route path="users"           element={<UsersPage />} />
        <Route path="audit"           element={<AuditPage />} />
        <Route path="settings"        element={<SettingsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <HashRouter>
            <AppRoutes />
          </HashRouter>
        </AuthProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}