namespace TomerGroup.Core.DTOs;

public class SyncPullRequestDto
{
    public Guid CustomerId { get; set; }
    public DateTime? LastSyncTimestamp { get; set; }
    public string DeviceId { get; set; } = "MobileClient";
}

public class EmergencyContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool HasOxygen { get; set; }
}

public class SyncPackageDto
{
    public DateTime SyncTimestamp { get; set; } = DateTime.UtcNow;
    public Guid CustomerId { get; set; }
    public CustomerDto? Customer { get; set; }
    public List<TripDto> Trips { get; set; } = new();
    public List<BookingDto> Bookings { get; set; } = new();
    public List<DocumentDto> Documents { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = new();
    public List<EmergencyContactDto> EmergencyContacts { get; set; } = new();
    public string ServerVersion { get; set; } = "1.0.0";
    public bool IsDeltaSync { get; set; }
}

public class ActivityCheckInDto
{
    public Guid ActivityId { get; set; }
    public DateTime CheckInTimestamp { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Note { get; set; }
}

public class UpdateCustomerEmergencyDto
{
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string? MedicalNotes { get; set; }
}

public class SyncPushRequestDto
{
    public Guid CustomerId { get; set; }
    public string DeviceId { get; set; } = "MobileClient";
    public DateTime ClientSyncTimestamp { get; set; } = DateTime.UtcNow;
    public UpdateCustomerEmergencyDto? UpdatedEmergencyContact { get; set; }
    public List<ActivityCheckInDto> ActivityCheckIns { get; set; } = new();
}

public class SyncPushResultDto
{
    public bool Success { get; set; }
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    public int AcknowledgedItems { get; set; }
    public int ConflictsResolved { get; set; }
    public string Message { get; set; } = string.Empty;
}

