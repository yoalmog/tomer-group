# CHECKPOINT CP-P3: PHASE 3 — CUSTOMERS & PROFILES

**Platform**: Tomer Group — Peru Travel Experience  
**Date**: 2026-09-16  
**Status**: VERIFIED & PASSING  
**Lead Architect**: Senior .NET MAUI / C# / ASP.NET Core Engineer  

---

## 1. Phase Overview
- **Phase**: Phase 3 — Customers & Profiles
- **Goal**: Deliver a comprehensive customer & traveler management subsystem for both Israeli travelers (Customer App) and agency operations staff (Agency Portal):
  - Bilingual profiles (Hebrew & English) with passport details, contact numbers (with WhatsApp format), country, and emergency contacts.
  - Section 14 Sensitive Data Protection & Passport Masking: Masked by default (`IL-****2711`), with dedicated authorized unmasking endpoint generating an automated `AuditLog` entry.
  - 6-Month Peru Immigration Passport Expiration Warning calculated dynamically.
  - Israeli traveler specific care: Kosher (Mehudar, Standard), Vegetarian, Vegan, Gluten-Free options, Medical & Altitude sickness (Soroche) advisory in Cusco (3,400m), Travel/Rescue Insurance details.
  - Section 34 Strict Customer Isolation: Customer A cannot access Customer B's profile. Customer self-update cannot modify agency internal notes.
  - Section 54 Safe Soft Deletion: Deleting a customer with active or confirmed trips/bookings is rejected to prevent data loss.
  - Section 24 & 41 Agency Travelers Directory: Paged search across Hebrew/English names, phone, passport, filters (In Peru Now, Kosher, Expiring passports), and real-time operational stats.
  - Mobile UI: Full Andean-palette `ProfilePage.xaml`, `ProfileViewModel`, `AgencyCustomersPage.xaml`, `AgencyCustomersViewModel`, `AgencyCustomerDetailPage.xaml`, `AgencyCustomerDetailViewModel`.

---

## 2. Features Completed

### A. Core Domain & Extended DTOs
- `Customer.cs` enhanced with:
  - `IsraelIdNumber` (Teudat Zehut)
  - `MedicalNotes` (Altitude sickness/Soroche history, medications)
  - `InsuranceCompany` & `InsurancePolicyNumber` (Emergency rescue in Andes)
  - `IsActiveInPeru`
  - Computed properties: `MaskedPassportNumber`, `MaskedIsraelId`, `IsPassportExpiringSoon`
- `CustomerExtendedDtos.cs`:
  - `CustomerSensitiveDetailsDto` (Unmasked identity data for authorized reveal)
  - `CustomerSummaryDto` (Card representation with initials avatar and active trip badges)
  - `CustomerSearchFilterDto` (Search term, country, dietary, active status, expiring passport)
  - `CustomerStatsDto` (Total travelers, Active in Peru, Israeli %, Kosher count, Expiring count)
  - `UpdateCustomerProfileDto` (Traveler self-service model)
- `LocalizationKeys.cs` & `LocalizationService.cs`:
  - Added bilingual Hebrew, English, and Spanish translations for all customer profile and dietary terms.

### B. Backend Services & Business Logic (`CustomerService.cs`)
- `GetByIdAsync`: Enforces strict customer isolation. Masks passport and ID. Protects agency internal `Notes` from customer view.
- `GetSensitiveDetailsAsync`: Restricted to self or authorized agency staff (`Admin,Manager,Sales,Operations`). Automatically emits an `AuditLog` entry (`ViewSensitiveCustomerData`).
- `GetByUserIdAsync`: Shortcut for customer profile lookups.
- `GetAllAsync` & `GetSummariesAsync`: Search across English/Hebrew names, phone, email, passport; filters by country, dietary, active status, expiring passports.
- `UpdateOwnProfileAsync`: Customer self-updates contact, dietary, and emergency details without tampering with role or agency notes.
- `DeleteAsync`: Enforces safety guards against deleting customers with active trips or unpaid bookings, followed by soft-delete (`IsDeleted = true`) and audit logging.
- `GetTravelerStatsAsync`: Calculates agency metrics across travelers in Peru and dietary restrictions.

### C. API Controller Endpoints (`CustomersController.cs`)
- `GET /api/customers`: Paged search for agency staff.
- `GET /api/customers/summaries`: Filtered summaries for agency directory.
- `GET /api/customers/stats`: Agency traveler analytics.
- `GET /api/customers/me`: Current traveler profile.
- `PUT /api/customers/me`: Traveler self-update profile.
- `GET /api/customers/{id}`: Customer details (Isolation guarded).
- `GET /api/customers/{id}/sensitive`: Unmasked sensitive passport info (Authorized & Audit logged).
- `POST /api/customers`: Staff customer creation.
- `PUT /api/customers/{id}`: Staff customer update.
- `DELETE /api/customers/{id}`: Staff soft-delete with safety checks.

### D. Mobile Client Pages & MVVM
- `ApiClient.cs`: Added methods for `GetMyProfileAsync`, `UpdateMyProfileAsync`, `GetSensitiveDetailsAsync`, `GetAgencyCustomersAsync`, `GetCustomerStatsAsync`.
- `ProfileViewModel`: MVVM logic with edit mode toggle, secure passport reveal toggle, dietary pickers, emergency contacts, validation, and Hebrew RTL strings.
- `ProfilePage.xaml`: High-end Andean aesthetic featuring deep navy hero card, initials avatar, Peru status badge, sensitive passport card with reveal button, dietary card, Cusco altitude advisory, emergency contact card, and save buttons.
- `AgencyCustomersViewModel` & `AgencyCustomersPage.xaml`: Operational directory for agency staff with live search, filter chips, avatar cards, and WhatsApp quick actions.
- `AgencyCustomerDetailViewModel` & `AgencyCustomerDetailPage.xaml`: Comprehensive traveler file and staff passport unmasking.
- `AgencyShell.xaml`: Added `Travelers / Customers` flyout item.
- `MauiProgram.cs`: Registered all ViewModels and Pages into DI.

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
- **Result**: `Passed: 35, Failed: 0, Total: 35 (100% Success)`
- **Suites Executed**:
  1. `Phase3CustomerTests`:
     - `CustomerCreation_PersistsAllHebrewAndTravelFields` (Passed)
     - `CustomerIsolation_CustomerACannotAccessCustomerBProfile` (Passed)
     - `SensitiveDataMasking_DefaultGetById_MasksPassportAndId` (Passed)
     - `SensitiveDataEndpoint_AuthorizedReveal_UnmasksPassportAndLogsAudit` (Passed)
     - `SensitiveDataEndpoint_UnauthorizedUser_Rejected` (Passed)
     - `CustomerSelfUpdate_UpdatesProfile_WithoutModifyingAgencyInternalNotes` (Passed)
     - `AgencySearchAndFilter_SupportsHebrewAndDietaryFilters` (Passed)
     - `SafeDeletion_PreventsDeletingCustomerWithActiveTrips` (Passed)
     - `SafeDeletion_CleanCustomer_SoftDeletesSuccessfully` (Passed)
     - `TravelerStats_CalculatesAccurateMetrics` (Passed)
  2. `Phase2AuthenticationTests` (6 passed)
  3. `BrandSettingsTests` (2 passed)
  4. `LocalizationTests` (5 passed)
  5. `CustomerIsolationTests` (1 passed)
  6. `AuthenticationTests` (3 passed)
  7. `DomainModelTests` (6 passed)
  8. `ApiHealthTests` (1 passed)
  9. `NavigationTests` (1 passed)

---

## 5. Next Phase
- **Phase 4: Trips and Itinerary**

