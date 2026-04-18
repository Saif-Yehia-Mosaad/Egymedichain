# EgyMediChain — Frontend

React 19 + TypeScript + Vite + Tailwind CSS v4
Pharmaceutical Supply Chain Governance | Egypt Vision 2030 | DEPI

---

## Quick Start

```bash
npm install
cp .env.example .env.local   # set your backend URL
npm run dev                  # http://localhost:5173
npm run build                # production build → dist/
```

---

## Project Structure

```
src/
  types/index.ts                 ← All TS types
  api/
    client.ts                    ← Axios + JWT + refresh interceptors
    endpoints.ts                 ← All API calls
  context/AuthContext.tsx        ← Login/logout/role state
  hooks/
    useRealtimeAlerts.ts         ← SignalR → SSE → Polling fallback
    useQrScanner.ts              ← Camera BarcodeDetector scanner
  utils/index.ts                 ← Helpers, permissions, formatting
  components/
    ui/index.tsx                 ← Button, Card, Input, Badge, Skeleton…
    layout/Sidebar.tsx           ← Collapsible role-aware sidebar
    layout/Topbar.tsx            ← Search + notifications + profile
    layout/AppLayout.tsx         ← App shell
    shared/QrScannerModal.tsx    ← Real camera QR modal
  pages/
    auth/          Login, Register, ForgotPassword, ResetPassword
    dashboard/     KPI cards + charts + activity feed
    medicines/     List + form + detail
    inventory/     Stock cards + low-stock glow + expiry warning
    warehouses/    Radial capacity charts + add modal
    pharmacy/      Stock request + QR verifier + history
    transactions/  Immutable timeline + new transfer modal
    alerts/        Severity management + resolve
    users/         Role cards + toggle status
    audit/         Filter chips + audit table
    settings/      Profile, security, notifications, preferences
```

---

## Environment Variables

Create `.env.local` in project root:

```env
VITE_API_URL=https://localhost:7001/api
VITE_HUB_URL=https://localhost:7001/hubs/alerts
```

---

## Backend Contract (ASP.NET Core)

```
POST  /api/auth/login               → { accessToken, refreshToken, user }
POST  /api/auth/refresh
POST  /api/auth/forgot-password
POST  /api/auth/verify-reset-code
POST  /api/auth/reset-password

GET   /api/analytics/dashboard      → DashboardStats
GET   /api/medicines                → PaginatedResponse<Medicine>
POST  /api/medicines
PUT   /api/medicines/:id
GET   /api/medicines/:id/batches

GET   /api/inventory                → PaginatedResponse<InventoryItem>
GET   /api/inventory/:nodeId

GET   /api/transactions             → PaginatedResponse<Transaction>
POST  /api/transactions

GET   /api/alerts                   → PaginatedResponse<Alert>
POST  /api/alerts/:id/resolve
GET   /api/alerts/stream            ← SSE stream

GET   /api/warehouses
POST  /api/warehouses
GET   /api/pharmacies
POST  /api/stock-requests
GET   /api/users
POST  /api/users/:id/toggle-status
GET   /api/audit
GET   /api/verify/:serial           → QrVerifyResult
```

SignalR Hub: `/hubs/alerts`
Events: `AlertCreated(alert)`, `ReceiveAlert(alert)`

---

## CORS (Backend Program.cs)

```csharp
builder.Services.AddCors(options => {
    options.AddPolicy("Frontend", p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});
app.UseCors("Frontend");
```

---

## To enable real SignalR

```bash
npm install @microsoft/signalr
```

The hook (`useRealtimeAlerts`) auto-detects it at runtime.
Without it, SSE then polling are used as fallback.
