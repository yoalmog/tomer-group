using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class AuditController : ControllerBase
{
    private readonly TomerDbContext _context;

    public AuditController(TomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminActivityFeedItemDto>>>> GetAuditLogs(
        [FromQuery] string? action = null,
        [FromQuery] string? entity = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
        {
            var a = action.Trim().ToUpperInvariant();
            query = query.Where(l => l.Action.ToUpper().Contains(a));
        }

        if (!string.IsNullOrWhiteSpace(entity))
        {
            var e = entity.Trim().ToLowerInvariant();
            query = query.Where(l => l.EntityName.ToLower().Contains(e));
        }

        var totalCount = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AdminActivityFeedItemDto
            {
                Id = l.Id,
                Timestamp = l.CreatedAt,
                Action = l.Action,
                Entity = l.EntityName,
                Description = $"{l.Action} on {l.EntityName} (ID: {l.EntityId}): {l.MetadataJson}",
                UserEmail = l.UserEmail ?? l.UserId.ToString() ?? "system"
            })
            .ToListAsync();

        var result = new PagedResult<AdminActivityFeedItemDto>
        {
            Items = logs,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PagedResult<AdminActivityFeedItemDto>>.Ok(result));
    }
}
