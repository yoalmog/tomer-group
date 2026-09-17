using TomerGroup.Core.Enums;

namespace TomerGroup.Core.Models;

public class Tour : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HebrewDescription { get; set; } = string.Empty;
    public string? Destination { get; set; } // e.g. Cusco, Machu Picchu, Rainbow Mountain
    public TourCategory Category { get; set; } = TourCategory.DayTour;
    public List<string> ImageUrls { get; set; } = new();

    public string Duration { get; set; } = string.Empty; // e.g. "Full Day", "2 Days / 1 Night", "5 Days / 4 Nights"
    public int DurationDays { get; set; } = 1;
    public string Difficulty { get; set; } = "Moderate"; // Easy, Moderate, Challenging, Strenuous
    public int MaxCapacity { get; set; } = 15;
    public int AltitudeMaxMeters { get; set; } = 3400;
    public bool RequiresAcclimatization { get; set; } = false;

    // Pricing from database — NOT hardcoded
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

    // Israeli / Kosher features
    public bool KosherFoodAvailable { get; set; } = true;
    public string? KosherCertificationDetails { get; set; } = "Mehudar / Glatt packed meals available from Cusco Chabad";
    public int BookingCutoffHours { get; set; } = 24;

    public bool IsActive { get; set; } = true;
}

public class Booking : BaseEntity
{
    public string BookingCode { get; set; } = string.Empty; // Unique ID e.g. TG-2026-00482

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount => TotalAmount - PaidAmount;
    public Currency Currency { get; set; } = Currency.USD;

    public BookingStatus Status { get; set; } = BookingStatus.Inquiry;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string? Notes { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<HotelBooking> HotelBookings { get; set; } = new List<HotelBooking>();
    public ICollection<Transportation> Transportations { get; set; } = new List<Transportation>();
}

