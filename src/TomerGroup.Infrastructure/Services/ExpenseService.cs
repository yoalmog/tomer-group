using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly TomerDbContext _context;

    public ExpenseService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ExpenseDto>> RecordExpenseAsync(RecordExpenseDto request, Guid recordedByUserId, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            return ApiResponse<ExpenseDto>.Fail("Expense amount must be greater than zero");
        }

        var expense = new Expense
        {
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            Currency = request.Currency,
            ExchangeRateToUsd = request.ExchangeRateToUsd > 0 ? request.ExchangeRateToUsd : 1.0m,
            Date = request.Date,
            TripId = request.TripId,
            BookingId = request.BookingId,
            SupplierName = request.SupplierName,
            ReceiptUrl = request.ReceiptUrl,
            Notes = request.Notes
        };

        await _context.Expenses.AddAsync(expense, cancellationToken);

        // Update Trip TotalCost atomically
        if (request.TripId.HasValue)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == request.TripId.Value, cancellationToken);
            if (trip != null)
            {
                var costInUsd = expense.Amount * expense.ExchangeRateToUsd;
                trip.TotalCost += costInUsd;
                trip.UpdatedAt = DateTime.UtcNow;
            }
        }

        var audit = new AuditLog
        {
            Action = "RecordExpense",
            EntityName = "Expense",
            EntityId = expense.Id.ToString(),
            UserId = recordedByUserId,
            UserEmail = "finance@tomergroup.com",
            MetadataJson = $"Recorded expense of {request.Amount} {request.Currency} [{request.Category}] for Supplier: {request.SupplierName}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ExpenseDto>.Ok(MapToDto(expense), "Expense recorded successfully");
    }

    public async Task<ApiResponse<List<ExpenseDto>>> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        var expenses = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Trip)
            .Where(e => e.TripId == tripId)
            .OrderByDescending(e => e.Date)
            .Select(e => MapToDto(e))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<ExpenseDto>>.Ok(expenses);
    }

    public async Task<ApiResponse<TripProfitabilityDto>> GetTripProfitabilityAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tripId, cancellationToken);

        if (trip == null)
        {
            return ApiResponse<TripProfitabilityDto>.Fail("Trip not found");
        }

        var expenses = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.TripId == tripId)
            .ToListAsync(cancellationToken);

        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Booking)
            .Include(p => p.Customer)
            .Where(p => p.Booking != null && p.Booking.TripId == tripId)
            .ToListAsync(cancellationToken);

        var expensesByCategory = expenses
            .GroupBy(e => e.Category)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(e => e.Amount * (e.ExchangeRateToUsd > 0 ? e.ExchangeRateToUsd : 1.0m)));

        var totalExpensesUsd = expenses.Sum(e => e.Amount * (e.ExchangeRateToUsd > 0 ? e.ExchangeRateToUsd : 1.0m));

        var profitability = new TripProfitabilityDto
        {
            TripId = trip.Id,
            TripCode = trip.TripCode,
            TripTitle = trip.Title,
            TotalRevenue = trip.TotalRevenue,
            TotalCost = totalExpensesUsd,
            Currency = trip.Currency,
            ExpensesByCategory = expensesByCategory,
            Expenses = expenses.Select(e => MapToDto(e)).ToList(),
            Payments = payments.Select(p => new PaymentDto
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
                CustomerName = p.Customer != null ? $"{p.Customer.FirstName} {p.Customer.LastName}" : string.Empty
            }).ToList()
        };

        return ApiResponse<TripProfitabilityDto>.Ok(profitability);
    }

    private static ExpenseDto MapToDto(Expense e) => new()
    {
        Id = e.Id,
        Category = e.Category,
        Description = e.Description,
        Amount = e.Amount,
        Currency = e.Currency,
        ExchangeRateToUsd = e.ExchangeRateToUsd,
        Date = e.Date,
        TripId = e.TripId,
        TripTitle = e.Trip?.Title,
        BookingId = e.BookingId,
        SupplierName = e.SupplierName,
        ReceiptUrl = e.ReceiptUrl,
        Notes = e.Notes
    };
}

