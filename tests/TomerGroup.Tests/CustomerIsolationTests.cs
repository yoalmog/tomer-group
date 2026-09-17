using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class CustomerIsolationTests
{
    private TomerDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new TomerDbContext(options);
    }

    [Fact]
    public async Task CustomerA_CannotAccess_CustomerB_Data()
    {
        // Arrange
        using var context = CreateInMemoryContext("CustomerIsolation_Db");

        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();
        var staffId = Guid.NewGuid();

        var customerA = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = userAId,
            FirstName = "Danny",
            LastName = "Cohen",
            Email = "danny@israel.com"
        };

        var customerB = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = userBId,
            FirstName = "Sarah",
            LastName = "Levi",
            Email = "sarah@israel.com"
        };

        var tripB = new Trip
        {
            Id = Guid.NewGuid(),
            TripCode = "PERU-2026-00999",
            Title = "Sarah's Inca Trail Private Trip",
            CustomerId = customerB.Id,
            Customer = customerB,
            TotalRevenue = 3200
        };

        var bookingB = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-2026-00999",
            CustomerId = customerB.Id,
            Customer = customerB,
            TotalAmount = 3200
        };

        await context.Customers.AddRangeAsync(customerA, customerB);
        await context.Trips.AddAsync(tripB);
        await context.Bookings.AddAsync(bookingB);
        await context.SaveChangesAsync();

        var customerService = new CustomerService(context);
        var tripService = new TripService(context);
        var bookingService = new BookingService(context);

        // Act 1: Customer A requests their own profile -> Should Succeed
        var selfResult = await customerService.GetByIdAsync(customerA.Id, userAId, "Customer");
        Assert.True(selfResult.Success);
        Assert.Equal("Danny", selfResult.Data?.FirstName);

        // Act 2: Customer A requests Customer B's profile -> MUST FAIL
        var crossCustomerResult = await customerService.GetByIdAsync(customerB.Id, userAId, "Customer");
        Assert.False(crossCustomerResult.Success);
        Assert.Contains("Access denied", crossCustomerResult.Message);

        // Act 3: Customer A requests Customer B's trip -> MUST FAIL
        var crossTripResult = await tripService.GetByIdAsync(tripB.Id, userAId, "Customer");
        Assert.False(crossTripResult.Success);
        Assert.Contains("Access denied", crossTripResult.Message);

        // Act 4: Customer A requests Customer B's trips by CustomerId -> MUST FAIL
        var crossCustomerTripsResult = await tripService.GetByCustomerIdAsync(customerB.Id, userAId, "Customer");
        Assert.False(crossCustomerTripsResult.Success);
        Assert.Contains("Access denied", crossCustomerTripsResult.Message);

        // Act 5: Customer A requests Customer B's booking -> MUST FAIL
        var crossBookingResult = await bookingService.GetByIdAsync(bookingB.Id, userAId, "Customer");
        Assert.False(crossBookingResult.Success);
        Assert.Contains("Access denied", crossBookingResult.Message);

        // Act 6: Agency Staff requests Customer B's data -> Should Succeed
        var staffResult = await customerService.GetByIdAsync(customerB.Id, staffId, "Admin");
        Assert.True(staffResult.Success);
        Assert.Equal("Sarah", staffResult.Data?.FirstName);
    }
}

