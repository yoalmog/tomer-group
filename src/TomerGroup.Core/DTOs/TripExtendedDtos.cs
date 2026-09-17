using TomerGroup.Core.Enums;

namespace TomerGroup.Core.DTOs;

public class CreateTripDayDto
{
    public Guid TripId { get; set; }
    public int DayNumber { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateActivityDto
{
    public Guid TripDayId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? Instructions { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? GuideId { get; set; }
    public Guid? TransportationId { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Scheduled;
    public string? Notes { get; set; }
}

public class UpdateActivityStatusDto
{
    public ActivityStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class DestinationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string Region { get; set; } = "Cusco";
    public int AltitudeMeters { get; set; }
    public string? Description { get; set; }
    public string? AltitudeWarning { get; set; }
    public bool IsPopular { get; set; }
}

public class CreateDestinationDto
{
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string Region { get; set; } = "Cusco";
    public int AltitudeMeters { get; set; }
    public string? Description { get; set; }
    public string? AltitudeWarning { get; set; }
    public bool IsPopular { get; set; } = true;
}

public class CustomerHomeDashboardDto
{
    public Guid? TripId { get; set; }
    public string TripCode { get; set; } = "PERU-2026-00482";
    public string TripTitle { get; set; } = "חוויית פרו — 5 ימים קוסקו ומאצ'ו פיצ'ו";
    public string CustomerName { get; set; } = "דני כהן";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CurrentDestination { get; set; } = "Cusco / Machu Picchu";
    public string CurrentDayTitle { get; set; } = "יום 4: סיור VIP במאצ'ו פיצ'ו";
    public int CurrentDayNumber { get; set; } = 4;
    public int TotalDays { get; set; } = 5;

    // Next upcoming activity
    public ActivityDto? NextActivity { get; set; }

    // Today's scheduled activities
    public List<ActivityDto> TodayActivities { get; set; } = new();

    // Emergency Contacts
    public string AgencyEmergencyPhone { get; set; } = "+51 984 999 888";
    public string AgencyWhatsApp { get; set; } = "+51 984 123 456";
    public string CuscoOfficeAddress { get; set; } = "Portal de Panes 123, Plaza de Armas, Cusco";
}

