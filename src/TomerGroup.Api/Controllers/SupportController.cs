using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager,Operations,Sales")]
public class SupportController : ControllerBase
{
    private readonly TomerDbContext _context;

    public SupportController(TomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SupportTicketSummaryDto>>>> GetTickets(
        [FromQuery] SupportTicketStatus? status = null,
        [FromQuery] SupportTicketPriority? priority = null,
        [FromQuery] string? search = null)
    {
        var query = _context.SupportTickets
            .Include(t => t.Customer)
            .Include(t => t.Messages)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(t => t.Subject.ToLower().Contains(term) ||
                                     (t.Customer != null && (t.Customer.FirstName.ToLower().Contains(term) || t.Customer.LastName.ToLower().Contains(term))));
        }

        var tickets = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.UpdatedAt)
            .Select(t => new SupportTicketSummaryDto
            {
                Id = t.Id,
                Subject = t.Subject,
                Category = t.Category.ToString(),
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
                MessagesCount = t.Messages.Count
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SupportTicketSummaryDto>>.Ok(tickets));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SupportTicket>>> GetTicketById(Guid id)
    {
        var ticket = await _context.SupportTickets
            .Include(t => t.Customer)
            .Include(t => t.Trip)
            .Include(t => t.AssignedStaffUser)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return NotFound(ApiResponse<SupportTicket>.Fail("Support ticket not found"));
        }

        return Ok(ApiResponse<SupportTicket>.Ok(ticket));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SupportTicket>>> CreateTicket([FromBody] CreateSupportTicketDto dto)
    {
        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
        {
            return BadRequest(ApiResponse<SupportTicket>.Fail("Customer does not exist"));
        }

        var userEmail = User.Identity?.Name ?? "staff@tomergroup.com";
        var staffUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        var ticket = new SupportTicket
        {
            CustomerId = dto.CustomerId,
            TripId = dto.TripId,
            Subject = dto.Subject,
            Category = dto.Category,
            Priority = dto.Priority,
            Status = SupportTicketStatus.New,
            AssignedStaffUserId = staffUser?.Id,
            Messages = new List<SupportTicketMessage>()
        };

        if (!string.IsNullOrWhiteSpace(dto.InitialMessage))
        {
            ticket.Messages.Add(new SupportTicketMessage
            {
                SenderUserId = staffUser?.Id,
                SenderName = staffUser != null ? $"{staffUser.FirstName} {staffUser.LastName}" : "Staff",
                SenderRole = "Staff",
                MessageText = dto.InitialMessage,
                IsInternalNote = false
            });
        }

        await _context.SupportTickets.AddAsync(ticket);

        // Audit Log
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Action = "CREATE_SUPPORT_TICKET",
            EntityName = "SupportTicket",
            EntityId = ticket.Id.ToString(),
            UserId = staffUser?.Id,
            UserEmail = userEmail,
            MetadataJson = $"{{\"subject\":\"{ticket.Subject}\",\"priority\":\"{ticket.Priority}\"}}"
        });

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ApiResponse<SupportTicket>.Ok(ticket));
    }

    [HttpPost("{id}/reply")]
    public async Task<ActionResult<ApiResponse<SupportTicketMessage>>> ReplyToTicket(Guid id, [FromBody] SupportTicketReplyDto dto)
    {
        var ticket = await _context.SupportTickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound(ApiResponse<SupportTicketMessage>.Fail("Support ticket not found"));
        }

        var userEmail = User.Identity?.Name ?? "staff@tomergroup.com";
        var staffUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        var message = new SupportTicketMessage
        {
            SupportTicketId = id,
            SenderUserId = staffUser?.Id,
            SenderName = staffUser != null ? $"{staffUser.FirstName} {staffUser.LastName}" : "Staff Support",
            SenderRole = "Staff",
            MessageText = dto.MessageText,
            IsInternalNote = dto.IsInternalNote
        };

        await _context.SupportTicketMessages.AddAsync(message);

        // Advance status from New/WaitingStaff to WaitingCustomer if public response
        if (!dto.IsInternalNote && (ticket.Status == SupportTicketStatus.New || ticket.Status == SupportTicketStatus.WaitingStaff))
        {
            ticket.Status = SupportTicketStatus.WaitingCustomer;
        }

        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<SupportTicketMessage>.Ok(message));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<SupportTicket>>> UpdateStatus(Guid id, [FromBody] UpdateSupportTicketStatusDto dto)
    {
        var ticket = await _context.SupportTickets.FindAsync(id);
        if (ticket == null)
        {
            return NotFound(ApiResponse<SupportTicket>.Fail("Support ticket not found"));
        }

        var beforeStatus = ticket.Status;
        ticket.Status = dto.Status;

        if (dto.Priority.HasValue)
        {
            ticket.Priority = dto.Priority.Value;
        }

        if (dto.AssignedStaffUserId.HasValue)
        {
            ticket.AssignedStaffUserId = dto.AssignedStaffUserId.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes))
        {
            ticket.ResolutionNotes = dto.ResolutionNotes;
        }

        if (dto.Status == SupportTicketStatus.Resolved || dto.Status == SupportTicketStatus.Closed)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }

        ticket.UpdatedAt = DateTime.UtcNow;

        // Append-only audit
        var userEmail = User.Identity?.Name ?? "staff@tomergroup.com";
        var staffUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Action = "UPDATE_SUPPORT_STATUS",
            EntityName = "SupportTicket",
            EntityId = ticket.Id.ToString(),
            UserId = staffUser?.Id,
            UserEmail = userEmail,
            MetadataJson = $"{{\"before\":\"{beforeStatus}\",\"after\":\"{ticket.Status}\"}}"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<SupportTicket>.Ok(ticket));
    }
}

