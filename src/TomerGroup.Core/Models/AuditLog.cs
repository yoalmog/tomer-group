namespace TomerGroup.Core.Models;

public class AIConversation : BaseEntity
{
    public Guid StaffUserId { get; set; }
    public User? StaffUser { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Context { get; set; } = "ItineraryBuilder"; // ItineraryBuilder, WhatsAppDrafting, CustomerSummary
    public ICollection<AIRequest> Requests { get; set; } = new List<AIRequest>();
}

public class AIRequest : BaseEntity
{
    public Guid ConversationId { get; set; }
    public AIConversation? Conversation { get; set; }

    public string Prompt { get; set; } = string.Empty;
    public string ResponsePayload { get; set; } = string.Empty; // Structured JSON matching typed schema
    public string SchemaType { get; set; } = "ItineraryDraft";

    public bool IsApproved { get; set; } = false;
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, UnderReview, Approved, Rejected
}

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = "system";
    public string Action { get; set; } = string.Empty; // e.g. Login, CustomerCreate, PaymentReceived, AIApproval
    public string EntityName { get; set; } = string.Empty; // Customer, Trip, Booking, Payment, Document
    public string? EntityId { get; set; }
    public string? MetadataJson { get; set; }
    public string? IpAddress { get; set; }

    public string TargetEntity { get => EntityName; set => EntityName = value; }
    public Guid? TargetEntityId { get => Guid.TryParse(EntityId, out var g) ? g : null; set => EntityId = value?.ToString(); }
    public string? Description { get => MetadataJson; set => MetadataJson = value; }
    public DateTime Timestamp { get => CreatedAt; set => CreatedAt = value; }
}

