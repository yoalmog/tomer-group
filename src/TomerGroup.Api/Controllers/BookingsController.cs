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
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly TomerDbContext _context;

    public BookingsController(IBookingService bookingService, TomerDbContext context)
    {
        _bookingService = bookingService;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Sales,Operations,Finance")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _bookingService.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _bookingService.GetByIdAsync(id, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/detailed")]
    public async Task<IActionResult> GetDetailedById(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _bookingService.GetDetailedByIdAsync(id, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(Guid customerId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _bookingService.GetByCustomerIdAsync(customerId, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto request)
    {
        var result = await _bookingService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}/admin")]
    [Authorize(Roles = "Admin,Manager,Sales,Operations,Finance")]
    public async Task<IActionResult> UpdateAdmin(Guid id, [FromBody] UpdateBookingAdminDto request)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound(ApiResponse<Booking>.Fail("Booking not found"));
        }

        if (Enum.TryParse<BookingStatus>(request.Status, true, out var status))
        {
            booking.Status = status;
        }

        booking.TotalAmount = request.TotalAmount;
        booking.PaidAmount = request.PaidAmount;
        if (booking.PaidAmount >= booking.TotalAmount && booking.TotalAmount > 0)
        {
            booking.PaymentStatus = PaymentStatus.Paid;
        }
        else if (booking.PaidAmount > 0)
        {
            booking.PaymentStatus = PaymentStatus.Partial;
        }
        else
        {
            booking.PaymentStatus = PaymentStatus.Pending;
        }

        booking.Notes = request.InternalNotes;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = GetUserId(),
            UserEmail = GetUserEmail(),
            Action = "UpdateBookingAdmin",
            TargetEntity = "Booking",
            TargetEntityId = id,
            Description = $"Updated booking '{booking.BookingCode}': Status={booking.Status}, Paid={booking.PaidAmount:C}/{booking.TotalAmount:C}"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<Booking>.Ok(booking));
    }

    [HttpPost("{id}/payments")]
    [Authorize(Roles = "Admin,Manager,Sales,Finance")]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] PaymentItemDto request)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound(ApiResponse<Payment>.Fail("Booking not found"));
        }

        if (request.Amount <= 0)
        {
            return BadRequest(ApiResponse<Payment>.Fail("Payment amount must be greater than zero"));
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = id,
            CustomerId = booking.CustomerId,
            Amount = request.Amount,
            Currency = Enum.TryParse<Currency>(request.Currency, true, out var c) ? c : Currency.USD,
            PaymentDate = request.PaymentDate != default ? request.PaymentDate : DateTime.UtcNow,
            Method = Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var m) ? m : PaymentMethod.CreditCard,
            Status = PaymentStatus.Paid,
            ReferenceNumber = request.TransactionReference ?? $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ReceiptNumber = $"REC-{DateTime.UtcNow:yyyyMMddHHmmss}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        booking.PaidAmount += request.Amount;
        if (booking.PaidAmount >= booking.TotalAmount)
        {
            booking.PaymentStatus = PaymentStatus.Paid;
        }
        else
        {
            booking.PaymentStatus = PaymentStatus.Partial;
        }
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = GetUserId(),
            UserEmail = GetUserEmail(),
            Action = "RecordPayment",
            TargetEntity = "Booking",
            TargetEntityId = id,
            Description = $"Recorded payment of {request.Amount:C} for booking '{booking.BookingCode}' (Ref: {payment.ReferenceNumber})"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<Payment>.Ok(payment));
    }

    private string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? "staff@tomergroup.com";
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
