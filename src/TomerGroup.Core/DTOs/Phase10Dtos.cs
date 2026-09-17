namespace TomerGroup.Core.DTOs;

public class GenerateItineraryPromptDto
{
    public string Destination { get; set; } = "Cusco & Machu Picchu";
    public int Days { get; set; } = 5;
    public string TravelerProfile { get; set; } = "Backpacker"; // Backpacker, Luxury, Family, Trekker
    public bool IsKosherRequired { get; set; } = true;
    public bool IsShabbatObservant { get; set; } = true;
    public string? AdditionalNotes { get; set; }
}

public class ItineraryDayDraftDto
{
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public int AltitudeMeters { get; set; }
    public string Description { get; set; } = string.Empty;
    public string HebrewDescription { get; set; } = string.Empty;
    public string AcclimatizationNote { get; set; } = string.Empty;
    public string KosherFoodNote { get; set; } = string.Empty;
    public string ShabbatNote { get; set; } = string.Empty;
}

public class ItineraryDraftResultDto
{
    public Guid RequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string HebrewSummary { get; set; } = string.Empty;
    public List<ItineraryDayDraftDto> Days { get; set; } = new();
    public bool FollowsAcclimatizationRule { get; set; } = true;
    public bool HasKosherArrangements { get; set; }
    public bool HasShabbatArrangements { get; set; }
}

public class DraftWhatsAppPromptDto
{
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string InquiryTopic { get; set; } = string.Empty; // AltitudeSickness, TrainDelay, ShabbatMeals, SalkantayGear
    public string Language { get; set; } = "he"; // he, en
    public string? AdditionalContext { get; set; }
}

public class WhatsAppDraftResultDto
{
    public Guid RequestId { get; set; }
    public string DraftedText { get; set; } = string.Empty;
    public string Language { get; set; } = "he";
    public string SuggestedAction { get; set; } = string.Empty;
}

public class AIRequestDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string ResponsePayload { get; set; } = string.Empty;
    public string SchemaType { get; set; } = "ItineraryDraft";
    public bool IsApproved { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, UnderReview, Approved, Rejected
    public DateTime CreatedAt { get; set; }
}

public class ApproveAIRequestDto
{
    public string? Notes { get; set; }
}

