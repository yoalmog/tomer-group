namespace TomerGroup.Core.DTOs;

public class RateLimitStatusDto
{
    public string ClientKey { get; set; } = string.Empty;
    public bool IsAllowed { get; set; } = true;
    public int RemainingRequests { get; set; }
    public int LimitPerWindow { get; set; }
    public double WindowResetSeconds { get; set; }
}

public class AuditLogSummaryDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditLogFilterDto
{
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

