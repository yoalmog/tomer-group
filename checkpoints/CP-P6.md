# CHECKPOINT CP-P6: PHASE 6 — GUIDES & DRIVERS MANAGEMENT

**Platform**: Tomer Group — Peru Travel Experience  
**Date**: 2026-09-16  
**Status**: VERIFIED & PASSING  
**Lead Architect**: Senior .NET MAUI / C# / ASP.NET Core Engineer  

---

## 1. Phase Overview
- **Phase**: Phase 6 — Guides & Drivers Management (Sections 17, 18, 19)
- **Goal**: Implement complete operational staffing, vehicle fleet management, assignment scheduling, conflict detection, and daily mission manifests:
  - **Guide Management (Section 17)**:
    - Official DIRCETUR Cusco license certification (`DIRCETUR-CUS-XXXX`).
    - Bilingual Hebrew, English, Spanish, and Quechua language proficiencies.
    - High-altitude first-aid certification (`FirstAidCertified = true`) and oxygen kit handling.
    - Jewish heritage expertise & Kosher awareness (`IsJewishHeritageExpert = true`).
    - Daily rates in USD, rating metrics (1.0 - 5.0), and availability tracking.
  - **Driver & Vehicle Fleet Management (Section 18)**:
    - MTC Peru professional license categorization (`A-IIIa Profesional`, `A-IIb`).
    - Peruvian mandatory insurance (SOAT) and technical inspection tracking.
    - Safety equipment: Oxygen tanks on board (`HasOxygenOnBoard = true`) for Andean mountain passes.
    - Fleet inventory (Mercedes-Benz Sprinter 19pax, Hyundai H1 8pax, Toyota Fortuner 4x4).
  - **Conflict Detection & Assignment Guardrails (Section 19)**:
    - Guide double-booking prevention: Evaluates activity date and `[StartTime, EndTime]` overlaps to eliminate double bookings.
    - Driver transit buffer enforcement: Enforces a minimum 60-minute travel and traffic buffer between consecutive transfers in Cusco and Sacred Valley.
  - **Daily Mission Sheets / Manifests**:
    - `GuideDailyManifest`: Lists activities for today, traveler names, WhatsApp contacts, hotel pickups, kosher meal requirements, and medical notes (Soroche/altitude warnings).
    - `DriverDailyManifest`: Lists scheduled pickups, airline flight numbers (e.g. LATAM), train arrival times (e.g. PeruRail), pickup/dropoff points, and passenger counts.
  - **Mobile Client (MAUI)**:
    - `AgencyStaffDirectoryViewModel` & `AgencyStaffDirectoryPage.xaml`: Interactive directory of certified guides and licensed drivers with star ratings, certification tags, and WhatsApp direct links.
    - `AgencyManifestViewModel` & `AgencyManifestPage.xaml`: Operational day sheet with date selector and tabbed views for guides and driver transfers.
    - Registered in `AgencyShell.xaml` flyout and `MauiProgram.cs` DI.

---

## 2. Features Completed

### A. Core Models & DTOs
- `Guide.cs` (`Transportation.cs`):
  - Added `HebrewName`, `Email`, `CertificationNumber`, `DailyRate`, `Currency`, `Rating`, `FirstAidCertified`, `IsJewishHeritageExpert`, `EmergencyContact`.
- `Driver.cs`:
  - Added `Email`, `LicenseCategory`, `LicenseExpirationDate`, `Rating`, `EmergencyContact`.
- `Vehicle.cs`:
  - Added `HasOxygenOnBoard`, `SoatPolicyNumber`, `SoatExpirationDate`, `TechnicalInspectionExpirationDate`, `GpsDeviceId`.
- `Phase6Dtos.cs`:
  - DTOs for `GuideDto`, `CreateGuideDto`, `UpdateGuideDto`, `DriverDto`, `CreateDriverDto`, `UpdateDriverDto`, `GuideDailyManifestDto`, `GuideManifestItemDto`, `DriverDailyManifestDto`, `DriverManifestItemDto`, `AssignGuideToActivityDto`, `AssignDriverToTransferDto`.

### B. Service Implementations
- `GuideService.cs`:
  - Implements `IGuideService`: CRUD, daily mission sheet retrieval, and activity assignment with time-window conflict detection.
- `DriverService.cs`:
  - Implements `IDriverService`: CRUD, daily manifest retrieval, and transfer assignment with 60-minute transit buffer conflict detection.
- `Program.cs`:
  - Registered `IGuideService` and `IDriverService` into ASP.NET Core DI.

### C. API Controllers
- `GuidesController.cs`:
  - `GET /api/guides`: List guides.
  - `GET /api/guides/{id}`: Guide profile.
  - `POST /api/guides`: Create guide.
  - `PUT /api/guides/{id}`: Update guide.
  - `GET /api/guides/{id}/manifest`: Guide mission sheet for date.
  - `POST /api/guides/assign`: Assign guide to activity with conflict check.
- `DriversController.cs`:
  - `GET /api/drivers`: List drivers.
  - `GET /api/drivers/{id}`: Driver profile.
  - `POST /api/drivers`: Create driver.
  - `PUT /api/drivers/{id}`: Update driver.
  - `GET /api/drivers/{id}/manifest`: Driver transfer sheet for date.
  - `POST /api/drivers/assign`: Assign driver to transfer with buffer check.

### D. Mobile Client & UI
- `ApiClient.cs`: Added methods for guides, drivers, manifests, and assignments.
- `AgencyStaffDirectoryViewModel` & `AgencyStaffDirectoryPage.xaml`: Complete staff directory with certification badges, Hebrew tags, ratings, and WhatsApp quick-launch.
- `AgencyManifestViewModel` & `AgencyManifestPage.xaml`: Operational daily mission sheet showing traveler names, dietary restrictions, and hotel details.
- `AgencyShell.xaml` & `MauiProgram.cs`: Fully registered and integrated.

---

## 3. Verification & Test Status
- **Build Status**: `dotnet build TomerGroup.sln` -> `0 Warning(s), 0 Error(s)`
- **Test Status**: `dotnet test TomerGroup.sln` -> `Passed: 56, Failed: 0, Total: 56 (100% Success)`
- **Suites Executed**:
  1. `Phase6GuidesDriversTests`:
     - `GuideCreation_StoresDirceturLicense_AndFirstAidCertification` (Passed)
     - `DriverCreation_StoresMtcLicense_AndProfessionalCategory` (Passed)
     - `GuideAssignment_ScheduleConflict_PreventsDoubleBooking` (Passed)
     - `GuideAssignment_NonOverlapping_SucceedsAndLogsAudit` (Passed)
     - `DriverAssignment_ScheduleBufferConflict_EnforcesTransitBuffer` (Passed)
     - `GuideDailyManifest_AggregatesTravelerDetails_AndSpecialNeeds` (Passed)
     - `DriverDailyManifest_AggregatesPickupSchedule_AndFlightNumbers` (Passed)
  2. All Phase 1 through Phase 5 regression tests passing.

