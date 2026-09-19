using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly TomerDbContext _context;

    public CustomersController(ICustomerService customerService, TomerDbContext context)
    {
        _customerService = customerService;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Sales,Operations")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _customerService.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("summaries")]
    [Authorize(Roles = "Admin,Manager,Sales,Operations")]
    public async Task<IActionResult> GetSummaries([FromQuery] CustomerSearchFilterDto filter)
    {
        var result = await _customerService.GetSummariesAsync(filter);
        return Ok(result);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager,Sales,Operations,Finance")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _customerService.GetTravelerStatsAsync();
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _customerService.GetByUserIdAsync(userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCustomerProfileDto request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _customerService.UpdateOwnProfileAsync(userId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _customerService.GetByIdAsync(id, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/sensitive")]
    public async Task<IActionResult> GetSensitiveDetails(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _customerService.GetSensitiveDetailsAsync(id, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/360")]
    [Authorize(Roles = "Admin,Manager,Sales,Operations,Finance")]
    public async Task<IActionResult> GetCustomer360(Guid id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound(ApiResponse<Customer360Dto>.Fail("Customer not found"));
        }

        var trips = await _context.Trips
            .AsNoTracking()
            .Include(t => t.Days)
            .Where(t => t.CustomerId == id)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Trip)
            .Where(b => b.CustomerId == id)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();

        var documents = await _context.Documents
            .AsNoTracking()
            .Where(d => d.CustomerId == id)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();

        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.CustomerId == id)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var supportTickets = await _context.SupportTickets
            .AsNoTracking()
            .Include(st => st.Messages)
            .Where(st => st.CustomerId == id)
            .OrderByDescending(st => st.CreatedAt)
            .ToListAsync();

        var auditLogs = await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.TargetEntityId == id || (a.TargetEntity == "Customer" && a.TargetEntityId == id))
            .OrderByDescending(a => a.Timestamp)
            .Take(20)
            .ToListAsync();

        decimal totalSpent = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
        decimal pendingBalance = bookings.Sum(b => b.OutstandingAmount);

        var dto = new Customer360Dto
        {
            CustomerId = customer.Id,
            FullName = $"{customer.FirstName} {customer.LastName}".Trim(),
            Email = customer.Email,
            Phone = customer.Phone ?? string.Empty,
            Nationality = customer.Country ?? "Israel",
            PassportNumber = customer.PassportNumber ?? string.Empty,
            PreferredLanguage = "he",
            IsActiveInPeru = customer.IsActiveInPeru,
            Notes = customer.Notes ?? customer.MedicalNotes,
            TotalTrips = trips.Count,
            TotalBookings = bookings.Count,
            TotalSpent = totalSpent,
            PendingBalance = pendingBalance,
            DocumentsCount = documents.Count,
            VerifiedDocumentsCount = documents.Count,
            OpenSupportRequests = supportTickets.Count(s => s.Status != SupportTicketStatus.Closed && s.Status != SupportTicketStatus.Resolved),
            Trips = trips.Select(t => new TripSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Status = t.Status.ToString(),
                Destinations = string.Join(", ", t.Days.Select(d => d.Destination).Where(d => !string.IsNullOrEmpty(d)).Distinct()),
                TravelersCount = 1
            }).ToList(),
            Bookings = bookings.Select(b => new BookingSummaryDto
            {
                Id = b.Id,
                BookingCode = b.BookingCode,
                TourOrTripName = b.Trip?.Title ?? "Tour Package",
                BookingDate = b.StartDate,
                TotalAmount = b.TotalAmount,
                PaidAmount = b.PaidAmount,
                Status = b.Status.ToString(),
                PaymentStatus = b.PaymentStatus.ToString()
            }).ToList(),
            Documents = documents.Select(d => new DocumentItemDto
            {
                Id = d.Id,
                FileName = d.Name,
                DocumentType = d.Type.ToString(),
                Status = "Verified",
                UploadedAt = d.UploadDate,
                RejectionReason = null
            }).ToList(),
            Payments = payments.Select(p => new PaymentItemDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency.ToString(),
                Status = p.Status.ToString(),
                PaymentMethod = p.Method.ToString(),
                TransactionReference = p.ReferenceNumber,
                PaymentDate = p.PaymentDate
            }).ToList(),
            SupportTickets = supportTickets.Select(st => new SupportTicketSummaryDto
            {
                Id = st.Id,
                Subject = st.Subject,
                Category = st.Category.ToString(),
                Priority = st.Priority.ToString(),
                Status = st.Status.ToString(),
                CreatedAt = st.CreatedAt,
                MessagesCount = st.Messages.Count
            }).ToList(),
            ActivityLog = auditLogs.Select(a => new AdminActivityFeedItemDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                Action = a.Action,
                Entity = a.TargetEntity,
                Description = a.Description ?? string.Empty,
                UserEmail = a.UserEmail
            }).ToList()
        };

        return Ok(ApiResponse<Customer360Dto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto request)
    {
        var userId = GetUserId();
        var result = await _customerService.CreateAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto request)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var result = await _customerService.UpdateAsync(id, request, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var result = await _customerService.DeleteAsync(id, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}
