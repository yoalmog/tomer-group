using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly TomerDbContext _context;

    public PaymentService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaymentDto>> RecordPaymentAsync(RecordPaymentDto request, Guid recordedByUserId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking == null)
        {
            return ApiResponse<PaymentDto>.Fail("Booking not found");
        }

        if (request.Amount <= 0)
        {
            return ApiResponse<PaymentDto>.Fail("Payment amount must be greater than zero");
        }

        var count = await _context.Payments.CountAsync(cancellationToken) + 1;
        var receiptNumber = $"REC-{DateTime.UtcNow.Year}-{count:D5}";

        var payment = new Payment
        {
            BookingId = request.BookingId,
            CustomerId = booking.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency,
            ExchangeRateToUsd = request.ExchangeRateToUsd > 0 ? request.ExchangeRateToUsd : 1.0m,
            Method = request.Method,
            Status = PaymentStatus.Paid,
            ReferenceNumber = request.ReferenceNumber,
            ReceiptNumber = receiptNumber,
            Notes = request.Notes,
            PaymentDate = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment, cancellationToken);

        // Convert payment amount to booking currency (USD)
        var amountInUsd = Math.Round(payment.Amount * payment.ExchangeRateToUsd, 2);
        booking.PaidAmount += amountInUsd;

        if (Math.Round(booking.PaidAmount, 2) >= Math.Round(booking.TotalAmount, 2))
        {
            booking.PaymentStatus = PaymentStatus.Paid;
            booking.Status = BookingStatus.Confirmed;
        }
        else if (booking.PaidAmount > 0)
        {
            booking.PaymentStatus = PaymentStatus.Partial;
        }

        booking.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            Action = "RecordPayment",
            EntityName = "Payment",
            EntityId = payment.Id.ToString(),
            UserId = recordedByUserId,
            UserEmail = "finance@tomergroup.com",
            MetadataJson = $"Recorded payment {receiptNumber} of {request.Amount} {request.Currency} for Booking {booking.BookingCode}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        payment.Booking = booking;
        payment.Customer = booking.Customer;

        return ApiResponse<PaymentDto>.Ok(MapToDto(payment), "Payment recorded and receipt generated successfully");
    }

    public async Task<ApiResponse<List<PaymentDto>>> GetByBookingIdAsync(Guid bookingId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking == null)
        {
            return ApiResponse<List<PaymentDto>>.Fail("Booking not found");
        }

        // Strict Customer Isolation check
        if (requestingRole == "Customer" && booking.Customer?.UserId != requestingUserId)
        {
            return ApiResponse<List<PaymentDto>>.Fail("Access denied: You are not authorized to view payments for another customer's booking");
        }

        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .Include(p => p.Customer)
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<PaymentDto>>.Ok(payments);
    }

    public async Task<ApiResponse<List<PaymentDto>>> GetCustomerPaymentsAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        if (requestingRole == "Customer")
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
            if (customer == null || customer.UserId != requestingUserId)
            {
                return ApiResponse<List<PaymentDto>>.Fail("Access denied: You are not authorized to view another customer's payment receipts");
            }
        }

        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .Include(p => p.Customer)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<PaymentDto>>.Ok(payments);
    }

    public async Task<ApiResponse<AgencyFinancialSummaryDto>> GetFinancialSummaryAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _context.Bookings.AsNoTracking().ToListAsync(cancellationToken);
        var totalRevenue = bookings.Sum(b => b.TotalAmount);
        var totalCollected = bookings.Sum(b => b.PaidAmount);
        var totalOutstanding = bookings.Sum(b => b.OutstandingAmount);

        var expenses = await _context.Expenses.AsNoTracking().ToListAsync(cancellationToken);
        var totalExpenses = expenses.Sum(e => e.Amount * (e.ExchangeRateToUsd > 0 ? e.ExchangeRateToUsd : 1.0m));

        var recentPayments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .Include(p => p.Customer)
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        var summary = new AgencyFinancialSummaryDto
        {
            TotalRevenueUsd = totalRevenue,
            TotalCollectedUsd = totalCollected,
            TotalOutstandingUsd = totalOutstanding,
            TotalExpensesUsd = totalExpenses,
            RecentPayments = recentPayments
        };

        return ApiResponse<AgencyFinancialSummaryDto>.Ok(summary);
    }

    private static PaymentDto MapToDto(Payment p) => new()
    {
        Id = p.Id,
        Amount = p.Amount,
        Currency = p.Currency,
        ExchangeRateToUsd = p.ExchangeRateToUsd,
        PaymentDate = p.PaymentDate,
        Method = p.Method,
        Status = p.Status,
        ReferenceNumber = p.ReferenceNumber,
        ReceiptNumber = p.ReceiptNumber,
        BookingId = p.BookingId,
        BookingCode = p.Booking?.BookingCode ?? string.Empty,
        CustomerId = p.CustomerId,
        CustomerName = p.Customer != null ? $"{p.Customer.FirstName} {p.Customer.LastName}" : string.Empty,
        Notes = p.Notes
    };
}
