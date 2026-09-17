namespace TomerGroup.Core.Models;

public class PackingItem : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? TripId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public bool IsChecked { get; set; }
    public bool IsCustom { get; set; }
}

public class TripMemory : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? TripId { get; set; }
    public Guid? ActivityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime ExperienceDate { get; set; } = DateTime.UtcNow;
    public int Rating { get; set; } = 5;
    public bool IsPrivate { get; set; } = true;
}

public class Review : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? TourId { get; set; }
    public Guid? TripId { get; set; }
    public int Rating { get; set; } = 5;
    public string ReviewText { get; set; } = string.Empty;
    public List<string> PhotoUrls { get; set; } = new();
    public bool IsPrivateFeedback { get; set; } = false;
    public bool IsApprovedForPublic { get; set; } = false;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

