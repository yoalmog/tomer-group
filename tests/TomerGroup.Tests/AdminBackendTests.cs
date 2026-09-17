using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Api.Controllers;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Security;
using Xunit;

namespace TomerGroup.Tests;

public class AdminBackendTests
{
    private TomerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task AdminDashboard_EmptyDatabase_ReturnsZeroMetricsWithoutError()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new DashboardController(context);

        // Act
        var result = await controller.GetDashboardMetrics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AdminDashboardMetricsDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(0, response.Data.TodayArrivals);
        Assert.Equal(0, response.Data.TodayDepartures);
        Assert.Equal(0, response.Data.ActiveTrips);
        Assert.Equal(0, response.Data.TotalCustomers);
        Assert.Equal(0m, response.Data.TotalRevenue);
        Assert.Equal(0m, response.Data.TotalExpenses);
        Assert.Equal(0m, response.Data.GrossProfit);
        Assert.Empty(response.Data.RecentActivities);
    }

    [Fact]
    public async Task AdminDashboard_PopulatedDatabase_AggregatesRealMetricsAccurately()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var today = DateTime.UtcNow.Date;

        var customer = new Customer
        {
            FirstName = "Ariel",
            LastName = "Shapiro",
            Email = "ariel@traveler.com",
            Country = "Israel",
            IsActiveInPeru = true
        };
        await context.Customers.AddAsync(customer);

        var trip = new Trip
        {
            TripCode = "PERU-2026-99901",
            Title = "Cusco & Machu Picchu Expedition",
            CustomerId = customer.Id,
            StartDate = today,
            EndDate = today.AddDays(4),
            Status = TripStatus.InProgress,
            TotalRevenue = 3200m
        };
        await context.Trips.AddAsync(trip);

        var expense = new Expense
        {
            TripId = trip.Id,
            Category = "Transport",
            Amount = 1400m,
            Currency = Currency.USD,
            Date = today
        };
        await context.Expenses.AddAsync(expense);

        var booking = new Booking
        {
            BookingCode = "TG-2026-99901",
            CustomerId = customer.Id,
            TripId = trip.Id,
            Status = BookingStatus.Pending,
            TotalAmount = 3200m
        };
        await context.Bookings.AddAsync(booking);

        var payment = new Payment
        {
            BookingId = booking.Id,
            CustomerId = customer.Id,
            Amount = 1600m,
            Status = PaymentStatus.Pending
        };
        await context.Payments.AddAsync(payment);

        await context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = Guid.NewGuid(),
            Action = "CREATE_BOOKING",
            EntityName = "Booking",
            EntityId = booking.Id.ToString(),
            MetadataJson = "Created pending booking"
        });

        await context.SaveChangesAsync();

        var controller = new DashboardController(context);

        // Act
        var result = await controller.GetDashboardMetrics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AdminDashboardMetricsDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.TodayArrivals);
        Assert.Equal(1, response.Data.ActiveTrips);
        Assert.Equal(1, response.Data.TotalCustomers);
        Assert.Equal(1, response.Data.ActiveInPeruCustomers);
        Assert.Equal(1, response.Data.PendingBookings);
        Assert.Equal(1, response.Data.PendingPayments);
        Assert.Equal(3200m, response.Data.TotalRevenue);
        Assert.Equal(1400m, response.Data.TotalExpenses);
        Assert.Equal(1800m, response.Data.GrossProfit);
        Assert.Equal(56.25m, response.Data.MarginPercentage);
        Assert.Single(response.Data.RecentActivities);
    }

    [Fact]
    public async Task AdminUsers_CreateUser_HashesPassword_AndLogsAudit()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var hasher = new PasswordHasher();
        var controller = new UsersController(context, hasher);

        var createDto = new CreateAdminUserDto
        {
            Email = "newops@tomergroup.com",
            Password = "SecurePassword2026!",
            FirstName = "Sofia",
            LastName = "Morales",
            Phone = "+51 984 888 999",
            Role = UserRole.Operations
        };

        // Act
        var result = await controller.CreateUser(createDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AdminUserDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("newops@tomergroup.com", response.Data.Email);
        Assert.Equal(UserRole.Operations, response.Data.Role);
        Assert.True(response.Data.IsActive);

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "newops@tomergroup.com");
        Assert.NotNull(savedUser);
        Assert.NotEmpty(savedUser.PasswordHash);
        Assert.NotEqual("SecurePassword2026!", savedUser.PasswordHash); // Password must be hashed!

        var auditLog = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "CREATE_USER");
        Assert.NotNull(auditLog);
        Assert.Equal("User", auditLog.EntityName);
    }

    [Fact]
    public async Task AdminUsers_DeactivateUser_SetsInactive_AndLogsAudit()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var hasher = new PasswordHasher();
        var (hash, salt) = hasher.HashPassword("TempPass2026!");

        var user = new User
        {
            Email = "staff@tomergroup.com",
            PasswordHash = hash,
            Salt = salt,
            FirstName = "Staff",
            LastName = "Member",
            Role = UserRole.Sales,
            IsActive = true
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var controller = new UsersController(context, hasher);

        // Act
        var result = await controller.DeactivateUser(user.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data);

        var updatedUser = await context.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.IsActive);

        var auditLog = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "DEACTIVATE_USER");
        Assert.NotNull(auditLog);
    }

    [Fact]
    public async Task AdminAudit_GetAuditLogs_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        await context.AuditLogs.AddRangeAsync(
            new AuditLog { UserId = Guid.NewGuid(), Action = "LOGIN", EntityName = "User", EntityId = "1", MetadataJson = "User logged in" },
            new AuditLog { UserId = Guid.NewGuid(), Action = "CREATE_PAYMENT", EntityName = "Payment", EntityId = "2", MetadataJson = "Recorded payment $1500" },
            new AuditLog { UserId = Guid.NewGuid(), Action = "UPDATE_TRIP", EntityName = "Trip", EntityId = "3", MetadataJson = "Updated itinerary" }
        );
        await context.SaveChangesAsync();

        var controller = new AuditController(context);

        // Act
        var result = await controller.GetAuditLogs(action: "PAYMENT");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PagedResult<AdminActivityFeedItemDto>>>(okResult.Value);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data.Items);
        Assert.Equal("CREATE_PAYMENT", response.Data.Items[0].Action);
    }
}
