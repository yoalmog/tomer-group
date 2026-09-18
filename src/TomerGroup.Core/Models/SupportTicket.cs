namespace TomerGroup.Core.Models;

public enum SupportTicketStatus
{
    New,
    Open,
    WaitingCustomer,
    WaitingStaff,
    Resolved,
    Closed
}

public enum SupportTicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum SupportCategory
{
    General,
    TripInquiry,
    ItineraryChange,
    Emergency,
    HealthAndAltitude,
    PaymentBilling,
    Documentation,
    TransportPickup
}

public class SupportTicket : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public string Subject { get; set; } = string.Empty;
    public SupportCategory Category { get; set; } = SupportCategory.General;
    public SupportTicketPriority Priority { get; set; } = SupportTicketPriority.Medium;
    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.New;

    public Guid? AssignedStaffUserId { get; set; }
    public User? AssignedStaffUser { get; set; }

    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public List<SupportTicketMessage> Messages { get; set; } = new();
}

public class SupportTicketMessage : BaseEntity
{
    public Guid SupportTicketId { get; set; }
    public SupportTicket? SupportTicket { get; set; }

    public Guid? SenderUserId { get; set; }
    public User? SenderUser { get; set; }

    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = "Staff"; // "Customer", "Staff", "Admin"
    public string MessageText { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; } = false;
}

