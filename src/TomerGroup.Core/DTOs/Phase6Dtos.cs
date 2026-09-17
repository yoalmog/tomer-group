using TomerGroup.Core.Enums;

namespace TomerGroup.Core.DTOs;

public class GuideDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? HebrewName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string CertificationNumber { get; set; } = string.Empty;
    public decimal DailyRate { get; set; }
    public Currency Currency { get; set; }
    public double Rating { get; set; }
    public bool FirstAidCertified { get; set; }
    public bool IsJewishHeritageExpert { get; set; }
    public List<string> Languages { get; set; } = new();
    public string Specialization { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }
    public bool IsAvailable { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }
}

public class CreateGuideDto
{
    public string FullName { get; set; } = string.Empty;
    public string? HebrewName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string CertificationNumber { get; set; } = string.Empty;
    public decimal DailyRate { get; set; } = 120;
    public Currency Currency { get; set; } = Currency.USD;
    public bool FirstAidCertified { get; set; } = true;
    public bool IsJewishHeritageExpert { get; set; } = true;
    public List<string> Languages { get; set; } = new() { "Hebrew", "English", "Spanish" };
    public string Specialization { get; set; } = "Inca History, High Altitude Trekking & Jewish Heritage";
    public string? EmergencyContact { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateGuideDto : CreateGuideDto
{
    public double Rating { get; set; } = 4.9;
}

public class DriverDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? LicenseNumber { get; set; }
    public string LicenseCategory { get; set; } = "A-IIIa Profesional";
    public DateTime? LicenseExpirationDate { get; set; }
    public double Rating { get; set; }
    public string? EmergencyContact { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
    public List<VehicleDto> Vehicles { get; set; } = new();
}

public class CreateDriverDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? LicenseNumber { get; set; }
    public string LicenseCategory { get; set; } = "A-IIIa Profesional";
    public DateTime? LicenseExpirationDate { get; set; }
    public string? EmergencyContact { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateDriverDto : CreateDriverDto
{
    public double Rating { get; set; } = 4.8;
}

public class GuideDailyManifestDto
{
    public Guid GuideId { get; set; }
    public string GuideName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<GuideManifestItemDto> Assignments { get; set; } = new();
}

public class GuideManifestItemDto
{
    public Guid ActivityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerWhatsApp { get; set; } = string.Empty;
    public string? DietaryRestrictions { get; set; }
    public string? MedicalNotes { get; set; }
    public ActivityStatus Status { get; set; }
}

public class DriverDailyManifestDto
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<DriverManifestItemDto> Transfers { get; set; } = new();
}

public class DriverManifestItemDto
{
    public Guid TransportationId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string HebrewServiceType { get; set; } = string.Empty;
    public DateTime ScheduledPickupTime { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DropoffLocation { get; set; } = string.Empty;
    public int PassengerCount { get; set; }
    public string? FlightOrTrainNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerWhatsApp { get; set; } = string.Empty;
    public string? VehiclePlate { get; set; }
    public string? VehicleModel { get; set; }
    public TransportationStatus Status { get; set; }
}

public class AssignGuideToActivityDto
{
    public Guid ActivityId { get; set; }
    public Guid GuideId { get; set; }
}

public class AssignDriverToTransferDto
{
    public Guid TransportationId { get; set; }
    public Guid DriverId { get; set; }
    public Guid? VehicleId { get; set; }
}

