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
[Authorize]
[Route("api/[controller]")]
[Route("api/customers/[controller]")]
public class MeController : ControllerBase
{
    private readonly TomerDbContext _context;
    private readonly ITripService _tripService;
    private readonly IBookingService _bookingService;
    private readonly IDocumentService _documentService;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MeController> _logger;

    public MeController(
        TomerDbContext context,
        ITripService tripService,
        IBookingService bookingService,
        IDocumentService documentService,
        IPaymentService paymentService,
        INotificationService notificationService,
        ILogger<MeController> logger)
    {
        _context = context;
        _tripService = tripService;
        _bookingService = bookingService;
        _documentService = documentService;
        _paymentService = paymentService;
        _notificationService = notificationService;
        _logger = logger;
    }

    private string GetAuthUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("user_id")?.Value
            ?? string.Empty;
    }

    private string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("email")?.Value
            ?? string.Empty;
    }

    private async Task<Customer?> GetCurrentCustomerAsync(CancellationToken cancellationToken = default)
    {
        var authUserId = GetAuthUserId();
        var email = GetUserEmail().ToLower().Trim();

        if (string.IsNullOrWhiteSpace(authUserId) && string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        Customer? customer = null;
        if (!string.IsNullOrWhiteSpace(authUserId))
        {
            customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AuthUserId == authUserId, cancellationToken);

            if (customer == null && Guid.TryParse(authUserId, out var parsedGuid))
            {
                customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == parsedGuid || c.Id == parsedGuid, cancellationToken);
            }
        }

        if (customer == null && !string.IsNullOrWhiteSpace(email))
        {
            customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email, cancellationToken);

            if (customer != null && string.IsNullOrWhiteSpace(customer.AuthUserId) && !string.IsNullOrWhiteSpace(authUserId))
            {
                customer.AuthUserId = authUserId;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return customer;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var customer = await GetCurrentCustomerAsync(cancellationToken);
        if (customer == null)
        {
            return NotFound(ApiResponse<CustomerProfileDto>.Fail("Customer profile not found for authenticated user."));
        }

        var dto = new CustomerProfileDto
        {
            Id = customer.Id,
            AuthUserId = customer.AuthUserId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            HebrewName = customer.HebrewName,
            PassportName = customer.PassportName,
            Email = customer.Email,
            Phone = customer.Phone,
            WhatsApp = customer.WhatsApp,
            Country = customer.Country,
            Language = customer.Language,
            ProfileImageUrl = customer.ProfileImageUrl,
            Status = customer.Status,
            MaskedPassportNumber = customer.MaskedPassportNumber,
            DietaryPreferences = customer.DietaryPreferences,
            EmergencyContactName = customer.EmergencyContactName,
            EmergencyContactPhone = customer.EmergencyContactPhone,
            SpecialRequests = customer.SpecialRequests,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };

        return Ok(ApiResponse<CustomerProfileDto>.Ok(dto));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileDto request, CancellationToken cancellationToken)
    {
        var customer = await GetCurrentCustomerAsync(cancellationToken);
        if (customer == null)
        {
            return NotFound(ApiResponse<CustomerProfileDto>.Fail("Customer profile not found for authenticated user."));
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName)) customer.FirstName = request.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(request.LastName)) customer.LastName = request.LastName.Trim();
        if (!string.IsNullOrWhiteSpace(request.HebrewName)) customer.HebrewName = request.HebrewName.Trim();
        if (!string.IsNullOrWhiteSpace(request.PassportName)) customer.PassportName = request.PassportName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Phone)) customer.Phone = request.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(request.WhatsApp)) customer.WhatsApp = request.WhatsApp.Trim();
        if (!string.IsNullOrWhiteSpace(request.Country)) customer.Country = request.Country.Trim();
        if (!string.IsNullOrWhiteSpace(request.Language)) customer.Language = request.Language.Trim();
        if (request.DietaryPreferences != null) customer.DietaryPreferences = request.DietaryPreferences.Trim();
        if (request.EmergencyContactName != null) customer.EmergencyContactName = request.EmergencyContactName.Trim();
        if (request.EmergencyContactPhone != null) customer.EmergencyContactPhone = request.EmergencyContactPhone.Trim();
        if (request.SpecialRequests != null) customer.SpecialRequests = request.SpecialRequests.Trim();
        if (request.ProfileImageUrl != null) customer.ProfileImageUrl = request.ProfileImageUrl.Trim();

        customer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetProfile(cancellationToken);
    }

    [HttpGet("trips")]
    public async Task<IActionResult> GetMyTrips()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null)
        {
            return Ok(ApiResponse<List<TripDto>>.Ok(new List<TripDto>()));
        }

        var result = await _tripService.GetByCustomerIdAsync(customer.Id, customer.UserId, "Customer");
        return Ok(result);
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null)
        {
            return Ok(ApiResponse<List<BookingDto>>.Ok(new List<BookingDto>()));
        }

        var result = await _bookingService.GetByCustomerIdAsync(customer.Id, customer.UserId, "Customer");
        return Ok(result);
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetMyDocuments()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null)
        {
            return Ok(ApiResponse<List<DocumentDto>>.Ok(new List<DocumentDto>()));
        }

        var result = await _documentService.GetCustomerDocumentsAsync(customer.Id, customer.UserId, "Customer");
        return Ok(result);
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetMyPayments(CancellationToken cancellationToken)
    {
        var customer = await GetCurrentCustomerAsync(cancellationToken);
        if (customer == null)
        {
            return Ok(ApiResponse<List<PaymentDto>>.Ok(new List<PaymentDto>()));
        }

        var payments = await _context.Payments
            .Include(p => p.Booking)
            .Where(p => p.Booking != null && p.Booking.CustomerId == customer.Id)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                BookingCode = p.Booking != null ? p.Booking.BookingCode : string.Empty,
                CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
                Amount = p.Amount,
                Currency = p.Currency,
                PaymentDate = p.PaymentDate,
                Method = p.Method,
                Status = p.Status,
                ReferenceNumber = p.ReferenceNumber
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<PaymentDto>>.Ok(payments));
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var customer = await GetCurrentCustomerAsync();
        if (customer == null)
        {
            return Ok(ApiResponse<List<NotificationDto>>.Ok(new List<NotificationDto>()));
        }

        var result = await _notificationService.GetUserNotificationsAsync(customer.UserId);
        return Ok(result);
    }

    [HttpGet("support")]
    public async Task<IActionResult> GetMySupportTickets(CancellationToken cancellationToken)
    {
        var customer = await GetCurrentCustomerAsync(cancellationToken);
        if (customer == null)
        {
            return Ok(ApiResponse<List<SupportTicketSummaryDto>>.Ok(new List<SupportTicketSummaryDto>()));
        }

        var tickets = await _context.SupportTickets
            .Include(t => t.Messages)
            .Where(s => s.CustomerId == customer.Id)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SupportTicketSummaryDto
            {
                Id = s.Id,
                Subject = s.Subject,
                Category = s.Category.ToString(),
                Priority = s.Priority.ToString(),
                Status = s.Status.ToString(),
                CreatedAt = s.CreatedAt,
                MessagesCount = s.Messages.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<SupportTicketSummaryDto>>.Ok(tickets));
    }
}
