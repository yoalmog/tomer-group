namespace TomerGroup.Core.Models;

public enum StaffTaskStatus
{
    Open,
    InProgress,
    Completed,
    Cancelled
}

public enum StaffTaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public class StaffTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public DateTime DueDate { get; set; }
    public StaffTaskPriority Priority { get; set; } = StaffTaskPriority.Medium;
    public StaffTaskStatus Status { get; set; } = StaffTaskStatus.Open;

    public Guid? AssignedStaffUserId { get; set; }
    public User? AssignedStaffUser { get; set; }

    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime? CompletedAt { get; set; }
    public string? CompletionNotes { get; set; }
}

