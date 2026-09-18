using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager,Operations,Sales,Finance")]
public class StaffTasksController : ControllerBase
{
    private readonly TomerDbContext _context;

    public StaffTasksController(TomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StaffTask>>>> GetTasks(
        [FromQuery] StaffTaskStatus? status = null,
        [FromQuery] StaffTaskPriority? priority = null,
        [FromQuery] bool? dueSoon = null)
    {
        var query = _context.StaffTasks
            .Include(t => t.Customer)
            .Include(t => t.Trip)
            .Include(t => t.AssignedStaffUser)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (dueSoon == true)
        {
            var horizon = DateTime.UtcNow.AddDays(2);
            query = query.Where(t => t.DueDate <= horizon && t.Status != StaffTaskStatus.Completed && t.Status != StaffTaskStatus.Cancelled);
        }

        var tasks = await query
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .ToListAsync();

        return Ok(ApiResponse<List<StaffTask>>.Ok(tasks));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StaffTask>>> CreateTask([FromBody] CreateStaffTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(ApiResponse<StaffTask>.Fail("Task title is required"));
        }

        var userEmail = User.Identity?.Name ?? "staff@tomergroup.com";
        var staffUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        var task = new StaffTask
        {
            Title = dto.Title,
            Description = dto.Description,
            CustomerId = dto.CustomerId,
            TripId = dto.TripId,
            DueDate = dto.DueDate,
            Priority = dto.Priority,
            Status = StaffTaskStatus.Open,
            AssignedStaffUserId = dto.AssignedStaffUserId ?? staffUser?.Id,
            CreatedByUserId = staffUser?.Id
        };

        await _context.StaffTasks.AddAsync(task);

        // Audit Log
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Action = "CREATE_STAFF_TASK",
            EntityName = "StaffTask",
            EntityId = task.Id.ToString(),
            UserId = staffUser?.Id,
            UserEmail = userEmail,
            MetadataJson = $"{{\"title\":\"{task.Title}\",\"due\":\"{task.DueDate:yyyy-MM-dd}\"}}"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<StaffTask>.Ok(task));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<StaffTask>>> UpdateTaskStatus(Guid id, [FromBody] UpdateStaffTaskStatusDto dto)
    {
        var task = await _context.StaffTasks.FindAsync(id);
        if (task == null)
        {
            return NotFound(ApiResponse<StaffTask>.Fail("Task not found"));
        }

        var before = task.Status;
        task.Status = dto.Status;

        if (dto.Status == StaffTaskStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.CompletionNotes))
            {
                task.CompletionNotes = dto.CompletionNotes;
            }
        }

        task.UpdatedAt = DateTime.UtcNow;

        var userEmail = User.Identity?.Name ?? "staff@tomergroup.com";
        var staffUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Action = "UPDATE_STAFF_TASK",
            EntityName = "StaffTask",
            EntityId = task.Id.ToString(),
            UserId = staffUser?.Id,
            UserEmail = userEmail,
            MetadataJson = $"{{\"before\":\"{before}\",\"after\":\"{task.Status}\"}}"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<StaffTask>.Ok(task));
    }
}

