namespace TomerGroup.Core.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string HebrewMessage { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // Booking, Payment, Driver, Tour, Document, Emergency
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ActionUrl { get; set; }
}

public class CreateNotificationDto
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string HebrewMessage { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? ActionUrl { get; set; }
}

public class SendWhatsAppMessageDto
{
    public Guid? CustomerId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = string.Empty;
    public Dictionary<string, string> TemplateParameters { get; set; } = new();
    public string? CustomContent { get; set; }
    public string Language { get; set; } = "he"; // he, en, es
}

public class WhatsAppDispatchResultDto
{
    public string RecipientPhone { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string WhatsAppUrl { get; set; } = string.Empty;
    public Guid? MessageId { get; set; }
    public string Status { get; set; } = "Prepared";
}

public class MessageTemplateDto
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = "he";
    public string Category { get; set; } = "Operational";
    public string ContentPattern { get; set; } = string.Empty;
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? StaffUserId { get; set; }
    public string Channel { get; set; } = "WhatsApp";
    public string RecipientPhone { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Language { get; set; } = "he";
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public string? Status { get; set; }
}

