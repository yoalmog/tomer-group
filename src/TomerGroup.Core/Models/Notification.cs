using TomerGroup.Core.Enums;

namespace TomerGroup.Core.Models;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // Booking, Payment, Driver, Tour, Document, Emergency
    public string HebrewMessage { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
}

public class Message : BaseEntity
{
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? StaffUserId { get; set; }
    public User? StaffUser { get; set; }

    public string Channel { get; set; } = "WhatsApp"; // WhatsApp, Push, InApp
    public string RecipientPhone { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Language { get; set; } = "he"; // he, en, es
    public bool IsSent { get; set; } = false;
    public DateTime? SentAt { get; set; }
    public string? Status { get; set; } // PendingReview, Approved, Sent, Failed
}

public class MessageTemplate : BaseEntity
{
    public string TemplateKey { get; set; } = string.Empty; // e.g. AirportPickup, TourReminder
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = "he"; // he, en, es
    public string ContentPattern { get; set; } = string.Empty;
    public string Category { get; set; } = "Operational";
}

