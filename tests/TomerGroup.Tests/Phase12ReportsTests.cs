using System.Text;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase12ReportsTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_Phase12_{Guid.NewGuid()}")
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task GetExecutiveAnalyticsAsync_CalculatesKPIsCorrectly()
    {
        using var context = CreateInMemoryContext();
        var reportService = new ReportService(context);

        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Danny", LastName = "Cohen" };
        await context.Customers.AddAsync(customer);

        // 2 Bookings: $5,000 and $3,000 -> Total Revenue: $8,000
        var b1 = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-B1",
            CustomerId = customer.Id,
            TotalAmount = 5000,
            PaidAmount = 5000,
            Status = BookingStatus.Confirmed
        };
        var b2 = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-B2",
            CustomerId = customer.Id,
            TotalAmount = 3000,
            PaidAmount = 3000,
            Status = BookingStatus.Confirmed
        };
        await context.Bookings.AddRangeAsync(b1, b2);

        // 2 Expenses: $2,000 (Hotel) and $1,200 (Transport) -> Total Expenses: $3,200
        var e1 = new Expense
        {
            Id = Guid.NewGuid(),
            Category = "Hotel",
            Amount = 2000,
            Currency = Currency.USD,
            ExchangeRateToUsd = 1.0m,
            Description = "Hotel Palacio del Inka"
        };
        var e2 = new Expense
        {
            Id = Guid.NewGuid(),
            Category = "Transport",
            Amount = 1200,
            Currency = Currency.USD,
            ExchangeRateToUsd = 1.0m,
            Description = "PeruRail Vistadome"
        };
        await context.Expenses.AddRangeAsync(e1, e2);

        // 1 Active Trip and 1 Completed Trip
        var t1 = new Trip
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            TripCode = "TG-T1",
            Title = "Active Trip",
            Status = TripStatus.InProgress,
            TotalRevenue = 5000
        };
        var t2 = new Trip
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            TripCode = "TG-T2",
            Title = "Completed Trip",
            Status = TripStatus.Completed,
            TotalRevenue = 3000
        };
        await context.Trips.AddRangeAsync(t1, t2);

        await context.SaveChangesAsync();

        var result = await reportService.GetExecutiveAnalyticsAsync();

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        // Financial formulas
        Assert.Equal(8000m, result.Data.TotalRevenue);
        Assert.Equal(3200m, result.Data.TotalExpenses);
        Assert.Equal(4800m, result.Data.NetProfit);
        Assert.Equal(60.00m, result.Data.ProfitMarginPercentage);

        // Operational stats
        Assert.Equal(2, result.Data.TotalBookingsCount);
        Assert.Equal(1, result.Data.ActiveTripsCount);
        Assert.Equal(1, result.Data.CompletedTripsCount);
    }

    [Fact]
    public async Task GetExecutiveAnalyticsAsync_AggregatesDestinationVolumes()
    {
        using var context = CreateInMemoryContext();
        var reportService = new ReportService(context);

        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Danny", LastName = "Cohen" };
        await context.Customers.AddAsync(customer);

        var trip1 = new Trip { Id = Guid.NewGuid(), CustomerId = customer.Id, TripCode = "TG-MP", Title = "Machu Picchu Explorer", TotalRevenue = 4000 };
        trip1.Days.Add(new TripDay { Id = Guid.NewGuid(), DayNumber = 1, Destination = "Machu Picchu", Title = "MP Day" });

        var trip2 = new Trip { Id = Guid.NewGuid(), CustomerId = customer.Id, TripCode = "TG-SV", Title = "Sacred Valley Trek", TotalRevenue = 2000 };
        trip2.Days.Add(new TripDay { Id = Guid.NewGuid(), DayNumber = 1, Destination = "Sacred Valley", Title = "SV Day" });

        await context.Trips.AddRangeAsync(trip1, trip2);
        await context.SaveChangesAsync();

        var result = await reportService.GetExecutiveAnalyticsAsync();

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.DestinationBreakdowns.Count);

        var mp = result.Data.DestinationBreakdowns.FirstOrDefault(d => d.Destination == "Machu Picchu");
        Assert.NotNull(mp);
        Assert.Equal("מאצ'ו פיצ'ו", mp.HebrewDestination);
        Assert.Equal(1, mp.TripsCount);
        Assert.Equal(4000m, mp.RevenueUsd);
    }

    [Fact]
    public async Task GetExecutiveAnalyticsAsync_DateFiltering_ExcludesOutOfRangeRecords()
    {
        using var context = CreateInMemoryContext();
        var reportService = new ReportService(context);

        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Danny", LastName = "Cohen" };
        await context.Customers.AddAsync(customer);

        var pastBooking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-PAST",
            CustomerId = customer.Id,
            TotalAmount = 1000,
            CreatedAt = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        var currentBooking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-CURRENT",
            CustomerId = customer.Id,
            TotalAmount = 5000,
            CreatedAt = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc)
        };

        await context.Bookings.AddRangeAsync(pastBooking, currentBooking);
        await context.SaveChangesAsync();

        var filter = new DateRangeFilterDto
        {
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc)
        };

        var result = await reportService.GetExecutiveAnalyticsAsync(filter);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(5000m, result.Data.TotalRevenue);
        Assert.Equal(1, result.Data.TotalBookingsCount);
    }

    [Fact]
    public async Task ExportReportAsync_Csv_GeneratesValidCsvWithBOMAndHeaders()
    {
        using var context = CreateInMemoryContext();
        var reportService = new ReportService(context);

        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Danny", LastName = "Cohen" };
        var booking = new Booking { Id = Guid.NewGuid(), BookingCode = "TG-01", CustomerId = customer.Id, TotalAmount = 3500 };
        await context.Customers.AddAsync(customer);
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        var result = await reportService.ExportReportAsync(null, "csv");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("text/csv; charset=utf-8", result.Data.ContentType);
        Assert.EndsWith(".csv", result.Data.FileName);

        var csvText = Encoding.UTF8.GetString(result.Data.FileBytes);
        Assert.Contains("Tomer Group - Peru Travel Experience", csvText);
        Assert.Contains("Total Revenue (USD)", csvText);
        Assert.Contains("$3,500.00", csvText);
    }

    [Fact]
    public async Task ExportReportAsync_Pdf_GeneratesValidPdfBuffer()
    {
        using var context = CreateInMemoryContext();
        var reportService = new ReportService(context);

        var result = await reportService.ExportReportAsync(null, "pdf");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("application/pdf", result.Data.ContentType);
        Assert.EndsWith(".pdf", result.Data.FileName);

        var pdfHeader = Encoding.UTF8.GetString(result.Data.FileBytes, 0, Math.Min(8, result.Data.FileBytes.Length));
        Assert.StartsWith("%PDF-1.4", pdfHeader);
    }
}
