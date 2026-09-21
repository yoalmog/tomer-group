# TOMER GROUP V2 — CLOUD DEPLOYMENT GUIDE
## Supabase (PostgreSQL + Auth + Storage) & Render Free (ASP.NET Core REST API)

---

## 1. Architecture Overview

```text
┌────────────────────────────────────────────────────────┐
│                   CLIENT APPLICATIONS                  │
│                                                        │
│   Tomer Group Mobile App (MAUI)     Admin Web Portal   │
└──────────────────────────┬─────────────────────────────┘
                           │ HTTPS / Bearer JWT
                           ▼
┌────────────────────────────────────────────────────────┐
│               ASP.NET CORE 8 REST API                  │
│               (Hosted on Render Free)                  │
│                                                        │
│   • Docker Container (net8.0 runtime)                  │
│   • Dynamic Port Binding ($PORT)                       │
│   • Multi-Issuer JWT Validation (Supabase + Internal)  │
│   • Customer Data Isolation (/api/me/*)                │
│   • RBAC Authorization (Customer vs Admin)             │
│   • Health Checks (/api/health, live, ready)           │
└──────────────────────────┬─────────────────────────────┘
                           │ SSL / TLS
                           ▼
┌────────────────────────────────────────────────────────┐
│                    SUPABASE CLOUD                      │
│                                                        │
│   ├── PostgreSQL Database (Data persistence, EF Core)  │
│   ├── Supabase Auth (Customer identity & GoTrue JWT)   │
│   └── Supabase Storage (Documents, Trek GPX, Photos)   │
└────────────────────────────────────────────────────────┘
```

---

## 2. Supabase Free Setup

### A. Create Project
1. Log in to [Supabase](https://supabase.com/) and click **New project**.
2. Name: `tomer-group-v2`
3. Database Password: Generate a strong password (save securely).
4. Region: Choose `East US (North Virginia)` or closest to your users.
5. Pricing Plan: **Free Tier**.

### B. Retrieve API Credentials
In **Project Settings -> API**:
- **Project URL**: `https://<PROJECT-REF>.supabase.co`
- **Project API Keys**:
  - `anon` (public): Used by Mobile client for GoTrue auth.
  - `service_role` (secret): **API Backend ONLY**. Never bundle in mobile client or web frontend.
- **JWT Secret**: Found under **Project Settings -> API -> JWT Settings**. Used by ASP.NET Core `JwtBearer` middleware to validate authentication tokens.

### C. Retrieve Database Connection String
In **Project Settings -> Database -> Connection string**:
- Select **URI** mode with Connection Pooling (Transaction mode port 6543 or Session mode port 5432):
  ```text
  postgresql://postgres.<PROJECT-REF>:[YOUR-PASSWORD]@aws-0-[REGION].pooler.supabase.com:6543/postgres?sslmode=Require
  ```

### D. Setup Storage Buckets
In **Storage -> Create Bucket**:
1. `customer-documents`: Set to **Private**. Access is mediated through backend signed URLs (`/api/documents/{id}/download`).
2. `trek-media`: Set to **Public** for trail maps, waypoint photos, and route assets.
3. `system-assets`: Set to **Public** for company branding and splash footage.

---

## 3. Render Free Web Service Deployment

### A. Deploy via Blueprint (`render.yaml`)
1. Push this repository to GitHub or GitLab.
2. In [Render Dashboard](https://dashboard.render.com/):
   - Click **Blueprints** -> **New Blueprint Instance**.
   - Select your repository.
   - Render will detect `render.yaml` and configure `tomergroup-api`.

### B. Or Deploy Manually as a Docker Web Service
1. Click **New +** -> **Web Service**.
2. Select your repository.
3. Configuration:
   - **Name**: `tomergroup-api`
   - **Language**: `Docker`
   - **Region**: `Oregon, USA` (or matching Supabase region)
   - **Branch**: `main`
   - **Instance Type**: `Free`
   - **Health Check Path**: `/api/health`

### C. Configure Environment Variables
Add the following in Render **Environment**:

| Key | Example Value | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Runs API in hardened production mode |
| `SUPABASE_URL` | `https://xyzcompany.supabase.co` | Supabase project URL |
| `SUPABASE_SERVICE_ROLE_KEY` | `eyJhbGciOi...` | Supabase service_role secret key |
| `SUPABASE_JWT_SECRET` | `super-secret-jwt-key...` | Supabase project JWT secret |
| `SUPABASE_DB_CONNECTION_STRING` | `Host=aws-0...;Port=5432;...` | Npgsql/PostgreSQL connection string |
| `ALLOWED_ORIGINS` | `https://tomergroup.com` | Allowed CORS origins for browser access |

---

## 4. Mobile Client Configuration (`TomerGroup.Mobile`)

The mobile application is pre-configured to use production cloud endpoints in Release mode:

1. In `src/TomerGroup.Mobile/Services/ApiConfiguration.cs`:
   - Production API: `https://tomergroup-api.onrender.com/`
   - Production Domain: `https://api.tomergroup.com/` (reserved for custom domain mapping)
   - Zero-localhost enforcement active for all Release builds.

2. To build the Release APK:
   ```powershell
   dotnet publish src/TomerGroup.Mobile/TomerGroup.Mobile.csproj -f net8.0-android -c Release -p:EnableMaui=true
   ```

---

## 5. Cold Starts & Health Checks

Render Free web services spin down after 15 minutes of inactivity:
- The initial request after spin-down may take ~30-45 seconds (cold start).
- `ApiClient.cs` on mobile handles extended initial timeouts gracefully with informative messaging ("Connecting to Tomer Group cloud services...").
- Keep-alive uptime monitoring can be configured using services like UptimeRobot pinging `https://tomergroup-api.onrender.com/api/health` every 10 minutes.

