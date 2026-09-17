using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class RateLimitingService : IRateLimitingService
{
    private static readonly ConcurrentDictionary<string, List<DateTime>> _requestWindows = new();
    private static readonly object _lock = new();

    private readonly Dictionary<string, (int Limit, TimeSpan Window)> _policies = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Auth", (Limit: 5, Window: TimeSpan.FromMinutes(1)) },
        { "Documents", (Limit: 20, Window: TimeSpan.FromMinutes(1)) },
        { "General", (Limit: 120, Window: TimeSpan.FromMinutes(1)) }
    };

    public RateLimitStatusDto CheckRateLimit(string clientKey, string category = "General")
    {
        var key = $"{category}:{clientKey}";
        var policy = _policies.TryGetValue(category, out var p) ? p : _policies["General"];
        var now = DateTime.UtcNow;
        var windowStart = now - policy.Window;

        lock (_lock)
        {
            var log = _requestWindows.GetOrAdd(key, _ => new List<DateTime>());
            // Remove timestamps outside active sliding window
            log.RemoveAll(t => t < windowStart);

            var isAllowed = log.Count < policy.Limit;
            if (isAllowed)
            {
                log.Add(now);
            }

            var remaining = Math.Max(0, policy.Limit - log.Count);
            var oldest = log.FirstOrDefault();
            var resetSeconds = oldest == default ? 0 : Math.Max(0, (policy.Window - (now - oldest)).TotalSeconds);

            return new RateLimitStatusDto
            {
                ClientKey = clientKey,
                IsAllowed = isAllowed,
                RemainingRequests = remaining,
                LimitPerWindow = policy.Limit,
                WindowResetSeconds = Math.Round(resetSeconds, 1)
            };
        }
    }

    public void ResetLimit(string clientKey)
    {
        foreach (var category in _policies.Keys)
        {
            _requestWindows.TryRemove($"{category}:{clientKey}", out _);
        }
    }
}

public class SecuritySanitizerService : ISecuritySanitizerService
{
    public string MaskPassport(string? passport)
    {
        if (string.IsNullOrWhiteSpace(passport)) return string.Empty;
        var clean = passport.Trim();
        if (clean.Length <= 4) return "****";
        return new string('*', clean.Length - 4) + clean.Substring(clean.Length - 4);
    }

    public string MaskIsraelId(string? israelId)
    {
        if (string.IsNullOrWhiteSpace(israelId)) return string.Empty;
        var clean = israelId.Trim();
        if (clean.Length <= 4) return "****";
        return new string('*', clean.Length - 4) + clean.Substring(clean.Length - 4);
    }

    public string MaskCreditCard(string? cc)
    {
        if (string.IsNullOrWhiteSpace(cc)) return string.Empty;
        var digitsOnly = Regex.Replace(cc, @"[^\d]", "");
        if (digitsOnly.Length <= 4) return "****";
        return "**** **** **** " + digitsOnly.Substring(digitsOnly.Length - 4);
    }

    public string SanitizeInput(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Strip script tags, javascript: references, and dangerous characters
        var sanitized = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"javascript:", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"[<>""'\0]", "");
        return sanitized.Trim();
    }

    public bool IsPathTraversalSafe(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        // Detect path traversal vectors
        if (path.Contains("..") || path.Contains("/") || path.Contains("\\") || path.Contains("\0"))
        {
            return false;
        }

        // Check for invalid file name characters
        var invalidChars = Path.GetInvalidFileNameChars();
        return !path.Any(c => invalidChars.Contains(c));
    }
}

public class SecurityAuditService : ISecurityAuditService
{
    private readonly TomerDbContext _context;

    public SecurityAuditService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<AuditLogSummaryDto>>> GetAuditLogsAsync(AuditLogFilterDto filter, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(a => a.Action.Contains(filter.Action));
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            query = query.Where(a => a.EntityName == filter.EntityName);
        }

        if (filter.UserId.HasValue && filter.UserId.Value != Guid.Empty)
        {
            query = query.Where(a => a.UserId == filter.UserId.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogSummaryDto
            {
                Id = a.Id,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                IpAddress = a.IpAddress,
                MetadataJson = a.MetadataJson,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<PagedResult<AuditLogSummaryDto>>.Ok(new PagedResult<AuditLogSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        });
    }
}
