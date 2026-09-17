using TomerGroup.Core.Enums;

namespace TomerGroup.Core.Models;

public class Trip : BaseEntity
{
    public string TripCode { get; set; } = string.Empty; // e.g. PERU-2026-00482
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Scheduled;

    public decimal TotalRevenue { get; set; } = 0;
    public decimal TotalCost { get; set; } = 0;
    public decimal GrossProfit => TotalRevenue - TotalCost;
    public Currency Currency { get; set; } = Currency.USD;

    public string? Notes { get; set; }

    public ICollection<TripDay> Days { get; set; } = new List<TripDay>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class TripDay : BaseEntity
{
    public Guid TripId { get; set; }
    public Trip? Trip { get; set; }

    public int DayNumber { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty; // e.g. "Day 4: Machu Picchu Exploration"
    public string Destination { get; set; } = string.Empty; // e.g. "Machu Picchu"
    public string? Description { get; set; }

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public class Activity : BaseEntity
{
    public Guid TripDayId { get; set; }
    public TripDay? TripDay { get; set; }

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

    // Service assignments
    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public Guid? GuideId { get; set; }
    public Guide? Guide { get; set; }

    public Guid? TransportationId { get; set; }
    public Transportation? Transportation { get; set; }

    public ActivityStatus Status { get; set; } = ActivityStatus.Scheduled;
    public string? Notes { get; set; }
}

