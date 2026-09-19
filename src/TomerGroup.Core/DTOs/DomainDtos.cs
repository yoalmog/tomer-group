using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;

namespace TomerGroup.Core.DTOs;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CustomerRegisterRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "en";
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfoDto User { get; set; } = new();
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string PreferredLanguage { get; set; } = "he";
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string PassportName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = "Israel";

    // Masked sensitive fields (safe for general display)
    public string MaskedPassportNumber { get; set; } = string.Empty;
    public string MaskedIsraelId { get; set; } = string.Empty;
    public DateTime? PassportExpiration { get; set; }
    public bool IsPassportExpiringSoon { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public string? MedicalNotes { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public bool IsActiveInPeru { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? SpecialRequests { get; set; }
    public string? DietaryPreferences { get; set; }
    public string? Notes { get; set; } // Only populated for authorized agency staff
}

public class CreateCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string PassportName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = "Israel";
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiration { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? IsraelIdNumber { get; set; }
    public string? MedicalNotes { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public bool IsActiveInPeru { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? SpecialRequests { get; set; }
    public string? DietaryPreferences { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCustomerDto : CreateCustomerDto
{
}

public class TripDto
{
    public Guid Id { get; set; }
    public string TripCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TripStatus Status { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit { get; set; }
    public Currency Currency { get; set; }
    public List<TripDayDto> Days { get; set; } = new();
}

public class TripDayDto
{
    public Guid Id { get; set; }
    public int DayNumber { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public List<ActivityDto> Activities { get; set; } = new();

    public string FormattedDayNumber => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() switch
    {
        "en" => $"Day {DayNumber}",
        "es" => $"Día {DayNumber}",
        _ => $"יום {DayNumber}"
    };
}

public class ActivityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? GuideName { get; set; }
    public string? GuidePhone { get; set; }
    public ActivityStatus Status { get; set; }

    public string FormattedGuideName => string.IsNullOrWhiteSpace(GuideName)
        ? string.Empty
        : (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() switch
        {
            "en" => $"👤 Guide: {GuideName}",
            "es" => $"👤 Guía: {GuideName}",
            _ => $"👤 מדריך: {GuideName}"
        };

    public string FormattedDriverName => string.IsNullOrWhiteSpace(DriverName)
        ? string.Empty
        : (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() switch
        {
            "en" => $"🚗 Driver: {DriverName}",
            "es" => $"🚗 Conductor: {DriverName}",
            _ => $"🚗 נהג: {DriverName}"
        };
}

public class CreateTripDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
}

public class UpdateTripDto : CreateTripDto
{
    public TripStatus Status { get; set; }
}

public class BookingDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? TripId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount => TotalAmount - PaidAmount;
    public Currency Currency { get; set; }
    public BookingStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}

public class CreateBookingDto
{
    public Guid CustomerId { get; set; }
    public Guid? TripId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public string? Notes { get; set; }
}

public class TourDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HebrewDescription { get; set; } = string.Empty;
    public string? Destination { get; set; }
    public TourCategory Category { get; set; }
    public string Duration { get; set; } = string.Empty;
    public int DurationDays { get; set; } = 1;
    public string Difficulty { get; set; } = string.Empty;
    public int MaxCapacity { get; set; } = 15;
    public int AltitudeMaxMeters { get; set; }
    public bool RequiresAcclimatization { get; set; }
    public decimal AdultPrice { get; set; }
    public decimal ChildPrice { get; set; }
    public decimal PrivatePrice { get; set; }
    public decimal AgencyCost { get; set; }
    public Currency Currency { get; set; }
    public List<string> IncludedServices { get; set; } = new();
    public List<string> ExcludedServices { get; set; } = new();
    public string? PickupInformation { get; set; }
    public string? MeetingPoint { get; set; }
    public List<string> LocationsVisited { get; set; } = new();
    public string? CancellationPolicy { get; set; }
    public bool KosherFoodAvailable { get; set; } = true;
    public string? KosherCertificationDetails { get; set; }
    public int BookingCutoffHours { get; set; } = 24;
    public bool IsActive { get; set; } = true;
}

public class CreateTourDto
{
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HebrewDescription { get; set; } = string.Empty;
    public string? Destination { get; set; }
    public TourCategory Category { get; set; } = TourCategory.DayTour;
    public string Duration { get; set; } = string.Empty;
    public int DurationDays { get; set; } = 1;
    public string Difficulty { get; set; } = "Moderate";
    public int MaxCapacity { get; set; } = 15;
    public int AltitudeMaxMeters { get; set; } = 3400;
    public bool RequiresAcclimatization { get; set; }
    public decimal AdultPrice { get; set; }
    public decimal ChildPrice { get; set; }
    public decimal PrivatePrice { get; set; }
    public decimal AgencyCost { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public List<string> IncludedServices { get; set; } = new();
    public List<string> ExcludedServices { get; set; } = new();
    public string? PickupInformation { get; set; }
    public string? MeetingPoint { get; set; }
    public List<string> LocationsVisited { get; set; } = new();
    public string? CancellationPolicy { get; set; }
    public bool KosherFoodAvailable { get; set; } = true;
    public string? KosherCertificationDetails { get; set; } = "Mehudar / Glatt packed meals available from Cusco Chabad";
    public int BookingCutoffHours { get; set; } = 24;
}

public class HealthStatusDto
{
    public string Status { get; set; } = "Healthy";
    public string Brand { get; set; } = "Tomer Group";
    public string Tagline { get; set; } = "Peru Travel Experience";
    public string Version { get; set; } = "1.0.0";
    public string Database { get; set; } = "Connected";
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
    public TimeSpan Uptime { get; set; }
    public Dictionary<string, string> Subsystems { get; set; } = new();
}

public class LivenessStatusDto
{
    public string Status { get; set; } = "Live";
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
    public TimeSpan Uptime { get; set; }
    public double MemoryUsageMb { get; set; }
    public int ProcessId { get; set; }
}

public class ReadinessStatusDto
{
    public string Status { get; set; } = "Ready";
    public string Database { get; set; } = "Connected";
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
    public TimeSpan Uptime { get; set; }
    public Dictionary<string, string> Subsystems { get; set; } = new();
}

