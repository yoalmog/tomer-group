# CHECKPOINT CP-P2: PHASE 2 — AUTHENTICATION & SECURITY

**Platform**: Tomer Group — Peru Travel Experience  
**Date**: 2026-09-16  
**Status**: VERIFIED & PASSING  
**Lead Architect**: Senior .NET MAUI / C# / ASP.NET Core Engineer  

---

## 1. Phase Overview
- **Phase**: Phase 2 — Authentication & Security
- **Goal**: Implement a complete, production-ready, enterprise-grade authentication and authorization subsystem across backend and mobile client:
  - Phone OTP Authentication (SMS/WhatsApp flow with secure hashing, 5-minute TTL, attempt rate limiting, test bypass `123456`).
  - Brute-force account protection (lockout after 5 failed attempts for 15 minutes).
  - Forgot & Reset Password workflow with token expiration and automatic session revocation (revoking active refresh tokens).
  - Refresh token rotation (exchange old refresh token for new access token + new refresh token).
  - 8-Role RBAC Authorization Policies (Admin, Manager, Sales, Operations, Finance, Guide, Driver, Customer) enforced on endpoints.
  - Mobile authentication architecture: `TokenRefreshHandler` (automatic 401 interceptor with mutex refresh), `SecureStorageService`, `ApiClient`, and dual-mode `LoginPage` (Email/Password + Phone OTP + 8-Role Demo Quick Select) and `ForgotPasswordPage`.
  - Comprehensive automated tests proving security rules, lockout, OTP flows, and token rotation.

---

## 2. Features Completed

### A. Extended Domain Models & Security Entities
- `PasswordResetToken`: Entity with token hash, expiration (15 minutes), `IsUsed`, `UsedAt`, and user linkage.
- `PhoneVerificationCode`: Entity with phone number, code salt, code hash, expiration (5 minutes), attempt tracking (`AttemptsCount`, max 3), `IsVerified`, and IP address tracking.
- `User` entity updates:
  - `FailedLoginAttempts`: Counter incremented on invalid credentials; reset to 0 on successful authentication.
  - `LockoutEnd`: UTC timestamp until which login attempts are rejected.
  - `IsLockedOut`: Calculated property checking if `LockoutEnd > DateTime.UtcNow`.
  - `PhoneNumberConfirmed`: Flag indicating phone verification status.

### B. Role-Based Access Control (RBAC) & Policies
- Defined `RolePolicies` constants and registered authorization policies in ASP.NET Core:
  - `AdminOnly`: Requires `Admin` role.
  - `ManagerOrAdmin`: Requires `Admin` or `Manager` role.
  - `OperationsStaff`: Requires `Admin`, `Manager`, or `Operations` role.
  - `SalesStaff`: Requires `Admin`, `Manager`, or `Sales` role.
  - `FinanceStaff`: Requires `Admin`, `Manager`, or `Finance` role.
  - `GuideOnly`: Requires `Guide` role.
  - `DriverOnly`: Requires `Driver` role.
  - `CustomerOnly`: Requires `Customer` role.
  - `StaffOnly`: Requires any staff role (`Admin`, `Manager`, `Sales`, `Operations`, `Finance`, `Guide`, `Driver`).
- Database seeded with active accounts for all 8 distinct roles.

### C. Phone OTP Authentication Service (`IPhoneAuthService` / `PhoneAuthService`)
- Phone number normalization (E.164 format parsing for Israel `+972`, Peru `+51`, and international numbers).
- Cryptographic 6-digit OTP generation using secure random generator.
- Code hashing with HMAC-SHA512 and individual per-code salt.
- Test mode verification: `123456` accepted for rapid QA and demo environments.
- Throttling & Security controls:
  - Maximum 3 verification attempts per code; invalidates code upon 3rd failure.
  - 5-minute code expiration.
  - Automatic user provisioning or retrieval: creates a new `Customer` profile and `User` account if phone does not exist, or logs in existing user.
  - Audit log entries created for both code issuance and verification.

### D. Brute-Force Protection & Account Lockout
- `AuthenticationService.LoginAsync`:
  - Checks if user is currently locked out (`LockoutEnd > UtcNow`); returns lockout error with remaining minutes.
  - On password mismatch, increments `FailedLoginAttempts`.
  - Upon reaching 5 failed attempts, sets `LockoutEnd = UtcNow.AddMinutes(15)`.
  - On successful login, clears `FailedLoginAttempts` and `LockoutEnd`.

### E. Password Recovery & Session Revocation
- `ForgotPasswordAsync`: Generates cryptographically secure base64 reset token, stores SHA-256 hash in `PasswordResetTokens` table with 15-minute expiration.
- `ResetPasswordAsync`: Validates token hash and expiration, updates user password hash, clears lockout status, and **revokes active refresh tokens** (`RefreshToken = null`).
- `ChangePasswordAsync`: Verifies current password, applies new password, and revokes active refresh tokens to terminate stale sessions across other devices.

### F. API Controller Endpoints (`AuthController.cs`)
- `POST /api/auth/phone/send-code`: Issue OTP to phone number.
- `POST /api/auth/phone/verify-code`: Verify OTP and issue JWT + refresh token.
- `POST /api/auth/forgot-password`: Request password reset token.
- `POST /api/auth/reset-password`: Reset password with token.
- `POST /api/auth/change-password`: Change password while authenticated.
- `GET /api/auth/session`: Validates current token and returns user details and roles.

### G. Mobile Security & Client Architecture
- `ISecureStorageService` / `SecureStorageService`: Securely stores `access_token`, `refresh_token`, and `current_user` with in-memory fallback.
- `TokenRefreshHandler`: HTTP DelegatingHandler intercepting `401 Unauthorized`:
  - Mutex-protected semaphore to prevent concurrent refresh race conditions.
  - Automatically calls `/api/auth/refresh` with stored refresh token.
  - Updates stored tokens and retries the original request seamlessly.
  - Automatically redirects to `LoginPage` if refresh token is invalid or expired.
- `ApiClient`: Updated with methods for phone authentication, password recovery, session validation, and token refresh.
- `LoginPage` & `LoginViewModel`:
  - Toggle between **Email / Password** and **Phone / OTP** tabs.
  - Phone mode: Step 1 Enter phone number -> Step 2 Enter 6-digit OTP code with countdown timer.
  - 8-Role Quick-Select Demo Bar (`Admin`, `Manager`, `Sales`, `Operations`, `Finance`, `Guide`, `Driver`, `Customer`) for instant testing of all personas.
  - Forgot Password navigation button.
- `ForgotPasswordPage` & `ForgotPasswordViewModel`: Clean UI for requesting reset token and completing password reset.

---

## 3. Build Status
- **Command**: `dotnet build TomerGroup.sln`
- **Result**: `Build succeeded. 0 Warning(s), 0 Error(s)`
- **Projects Built**:
  - `TomerGroup.Core` (net8.0)
  - `TomerGroup.Infrastructure` (net8.0)
  - `TomerGroup.Api` (net8.0)
  - `TomerGroup.Mobile` (net8.0 / MAUI dual-target)
  - `TomerGroup.Tests` (net8.0)

---

## 4. Test Status
- **Command**: `dotnet test TomerGroup.sln --logger "console;verbosity=detailed"`
- **Result**: `Passed: 25, Failed: 0, Total: 25 (100% Success)`
- **Suites Executed**:
  1. `Phase2AuthenticationTests`:
     - `BruteForceLockout_After5FailedAttempts_LocksAccount` (Passed)
     - `PhoneAuth_FullFlow_SendCodeAndVerify` (Passed)
     - `PhoneAuth_RejectsInvalidCode_AndTracksAttempts` (Passed)
     - `PasswordRecovery_FullFlow_ResetAndRevokeSessions` (Passed)
     - `RefreshTokenRotation_GeneratesNewPairAndRevokesOld` (Passed)
     - `All8Roles_HaveDistinctRolePoliciesDefined` (Passed)
  2. `BrandSettingsTests` (2 passed)
  3. `LocalizationTests` (5 passed)
  4. `CustomerIsolationTests` (1 passed)
  5. `AuthenticationTests` (3 passed)
  6. `DomainModelTests` (6 passed)
  7. `ApiHealthTests` (1 passed)
  8. `NavigationTests` (1 passed)

---

## 5. Database & Seeding Updates
- Added `PasswordResetTokens` and `PhoneVerificationCodes` DbSets in `TomerDbContext`.
- Configured indexes for quick phone lookup, expiration checks, and token hash matching.
- Updated `DatabaseSeeder` with active accounts for all 8 roles:
  - Admin: `admin@tomergroup.com`
  - Manager: `manager@tomergroup.com`
  - Sales: `sales@tomergroup.com`
  - Operations: `ops@tomergroup.com`
  - Finance: `finance@tomergroup.com`
  - Guide: `guide.carlos@tomergroup.com`
  - Driver: `driver.juan@tomergroup.com`
  - Customer: `danny@israel.com`

---

## 6. Next Phase
- **Phase 3: Customers & Profiles**
  - Customer registration, profile management, passport/document uploads, dietary preferences, emergency contacts, medical notices, and full customer management in agency back-office.

