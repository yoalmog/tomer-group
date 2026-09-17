using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase7PaymentsExpensesTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_Phase7_{Guid.NewGuid()}")
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task PaymentRecording_UpdatesBookingPaidAmount_AndPaymentStatusTransitions()
    {
        using var context = CreateInMemoryContext();
        var paymentService = new PaymentService(context);

        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Danny", LastName = "Cohen" };
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-2026-00482",
            CustomerId = customer.Id,
            Customer = customer,
            TotalAmount = 3000,
            PaidAmount = 0,
            Currency = Currency.USD,
            Status = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.Pending
        };

        await context.Customers.AddAsync(customer);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // 1. Record deposit of 1000 USD
        var payment1Response = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            BookingId = booking.Id,
            Amount = 1000,
            Currency = Currency.USD,
            ExchangeRateToUsd = 1.0m,
            Method = PaymentMethod.BankTransfer,
            ReferenceNumber = "HAPOALIM-DEP-01",
            Notes = "Deposit 33%"
        }, Guid.NewGuid());

        Assert.True(payment1Response.Success);
        Assert.NotNull(payment1Response.Data);
        Assert.StartsWith("REC-", payment1Response.Data.ReceiptNumber);

        var updatedBooking1 = await context.Bookings.FindAsync(booking.Id);
        Assert.Equal(1000, updatedBooking1!.PaidAmount);
        Assert.Equal(2000, updatedBooking1.OutstandingAmount);
        Assert.Equal(PaymentStatus.Partial, updatedBooking1.PaymentStatus);

        // 2. Record remaining 2000 USD
        var payment2Response = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            BookingId = booking.Id,
            Amount = 2000,
            Currency = Currency.USD,
            ExchangeRateToUsd = 1.0m,
            Method = PaymentMethod.CreditCard,
            ReferenceNumber = "CC-AUTH-9921",
            Notes = "Final balance payment"
        }, Guid.NewGuid());

        Assert.True(payment2Response.Success);
        var updatedBooking2 = await context.Bookings.FindAsync(booking.Id);
        Assert.Equal(3000, updatedBooking2!.PaidAmount);
        Assert.Equal(0, updatedBooking2.OutstandingAmount);
        Assert.Equal(PaymentStatus.Paid, updatedBooking2.PaymentStatus);
        Assert.Equal(BookingStatus.Confirmed, updatedBooking2.Status);
    }

    [Fact]
    public async Task PaymentRecording_WithCurrencyConversion_ConvertsSolToUsdCorrectly()
    {
        using var context = CreateInMemoryContext();
        var paymentService = new PaymentService(context);

        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Yossi", LastName = "Levi" };
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-2026-00500",
            CustomerId = customer.Id,
            TotalAmount = 1000, // USD
            PaidAmount = 0,
            Currency = Currency.USD
        };

        await context.Customers.AddAsync(customer);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // 1,900 PEN at 0.263 USD/PEN = ~500 USD
        var paymentResponse = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            BookingId = booking.Id,
            Amount = 1900,
            Currency = Currency.PEN,
            ExchangeRateToUsd = 0.263m,
            Method = PaymentMethod.Cash,
            Notes = "Cash payment in Peruvian Soles at Cusco office"
        }, Guid.NewGuid());

        Assert.True(paymentResponse.Success);
        Assert.Equal(1900, paymentResponse.Data!.Amount);
        Assert.Equal(Currency.PEN, paymentResponse.Data.Currency);

        var updatedBooking = await context.Bookings.FindAsync(booking.Id);
        Assert.Equal(499.7m, updatedBooking!.PaidAmount);
        Assert.Equal(PaymentStatus.Partial, updatedBooking.PaymentStatus);
    }

    [Fact]
    public async Task CustomerIsolation_CustomerACannotViewCustomerBPaymentReceipts()
    {
        using var context = CreateInMemoryContext();
        var paymentService = new PaymentService(context);

        var userA = new User { Id = Guid.NewGuid(), Email = "userA@gmail.com" };
        var customerA = new Customer { Id = Guid.NewGuid(), UserId = userA.Id, FirstName = "Danny" };
        var userB = new User { Id = Guid.NewGuid(), Email = "userB@gmail.com" };
        var customerB = new Customer { Id = Guid.NewGuid(), UserId = userB.Id, FirstName = "Yossi" };

        var bookingA = new Booking { Id = Guid.NewGuid(), CustomerId = customerA.Id, BookingCode = "TG-2026-00001" };

        var paymentA = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingA.Id,
            CustomerId = customerA.Id,
            Amount = 1500,
            ReceiptNumber = "REC-2026-00001",
            Status = PaymentStatus.Paid
        };

        await context.Users.AddRangeAsync(userA, userB);
        await context.Customers.AddRangeAsync(customerA, customerB);
        await context.Bookings.AddAsync(bookingA);
        await context.Payments.AddAsync(paymentA);
        await context.SaveChangesAsync();

        // Customer A can view their receipts
        var responseA = await paymentService.GetCustomerPaymentsAsync(customerA.Id, userA.Id, "Customer");
        Assert.True(responseA.Success);
        Assert.Single(responseA.Data!);

        // Customer B attempting to access Customer A's receipts is strictly denied
        var responseB = await paymentService.GetCustomerPaymentsAsync(customerA.Id, userB.Id, "Customer");
        Assert.False(responseB.Success);
        Assert.Contains("Access denied", responseB.Message);
    }

    [Fact]
    public async Task ExpenseRecording_AtomicallyUpdatesTripTotalCost_AndLogsAudit()
    {
        using var context = CreateInMemoryContext();
        var expenseService = new ExpenseService(context);

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            TripCode = "PERU-2026-00100",
            Title = "Machu Picchu & Sacred Valley",
            TotalRevenue = 4000,
            TotalCost = 0
        };

        await context.Trips.AddAsync(trip);
        await context.SaveChangesAsync();

        var expense1 = await expenseService.RecordExpenseAsync(new RecordExpenseDto
        {
            TripId = trip.Id,
            Category = "Hotel",
            Description = "Palacio del Inka 3 Nights Booking",
            Amount = 850,
            Currency = Currency.USD,
            SupplierName = "Marriott / Palacio del Inka"
        }, Guid.NewGuid());

        var expense2 = await expenseService.RecordExpenseAsync(new RecordExpenseDto
        {
            TripId = trip.Id,
            Category = "Transport",
            Description = "Vistadome Train Tickets (RT)",
            Amount = 380,
            Currency = Currency.USD,
            SupplierName = "PeruRail"
        }, Guid.NewGuid());

        Assert.True(expense1.Success);
        Assert.True(expense2.Success);

        var updatedTrip = await context.Trips.FindAsync(trip.Id);
        Assert.Equal(1230, updatedTrip!.TotalCost);

        var auditLogs = await context.AuditLogs.Where(a => a.EntityName == "Expense").ToListAsync();
        Assert.Equal(2, auditLogs.Count);
    }

    [Fact]
    public async Task TripProfitability_CalculatesGrossProfit_AndMarginPercentageAccurately()
    {
        using var context = CreateInMemoryContext();
        var expenseService = new ExpenseService(context);

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            TripCode = "PERU-2026-00200",
            Title = "Complete Peru Explorer",
            TotalRevenue = 5000,
            TotalCost = 0
        };

        var expenses = new List<Expense>
        {
            new() { TripId = trip.Id, Category = "Hotel", Amount = 1200, Currency = Currency.USD },
            new() { TripId = trip.Id, Category = "Guide", Amount = 500, Currency = Currency.USD },
            new() { TripId = trip.Id, Category = "Transport", Amount = 700, Currency = Currency.USD }
        };

        await context.Trips.AddAsync(trip);
        await context.Expenses.AddRangeAsync(expenses);
        await context.SaveChangesAsync();

        var result = await expenseService.GetTripProfitabilityAsync(trip.Id);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(5000, result.Data.TotalRevenue);
        Assert.Equal(2400, result.Data.TotalCost);
        Assert.Equal(2600, result.Data.GrossProfit);
        Assert.Equal(52.0m, result.Data.MarginPercentage);
        Assert.Equal(3, result.Data.ExpensesByCategory.Count);
        Assert.Equal(1200, result.Data.ExpensesByCategory["Hotel"]);
        Assert.Equal(500, result.Data.ExpensesByCategory["Guide"]);
        Assert.Equal(700, result.Data.ExpensesByCategory["Transport"]);
    }

    [Fact]
    public async Task PaymentValidation_RejectsZeroOrNegativeAmounts()
    {
        using var context = CreateInMemoryContext();
        var paymentService = new PaymentService(context);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-2026-00300",
            TotalAmount = 2000
        };

        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        var zeroResult = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            BookingId = booking.Id,
            Amount = 0
        }, Guid.NewGuid());

        var negativeResult = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            BookingId = booking.Id,
            Amount = -100
        }, Guid.NewGuid());

        Assert.False(zeroResult.Success);
        Assert.False(negativeResult.Success);
    }
}
