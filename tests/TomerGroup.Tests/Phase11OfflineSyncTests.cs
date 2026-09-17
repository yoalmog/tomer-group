using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase11OfflineSyncTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_Phase11_{Guid.NewGuid()}")
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task PullDeltaPackageAsync_FullSync_ReturnsAllTripsBookingsAndEmergencyContacts()
    {
        using var context = CreateInMemoryContext();
        var syncService = new SyncService(context);

        var userId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "Danny",
            LastName = "Cohen",
            Phone = "+972541234567"
        };

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            TripCode = "PERU-2026-00482",
            CustomerId = customer.Id,
            Title = "Peru Classic Experience",
            Description = "Cusco & Machu Picchu",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(17),
            Status = TripStatus.Confirmed
        };

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-2026-00482",
            CustomerId = customer.Id,
            Customer = customer,
            TotalAmount = 2500,
            PaidAmount = 2500,
            Status = BookingStatus.Confirmed
        };

        var doc = new Document
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Name = "Machu Picchu Circuit 2",
            Type = DocumentType.MachuPicchuPermit,
            FileExtension = "pdf",
            MimeType = "application/pdf",
            StoragePath = "docs/mp.pdf",
            IsCustomerVisible = true
        };

        await context.Customers.AddAsync(customer);
        await context.Trips.AddAsync(trip);
        await context.Bookings.AddAsync(booking);
        await context.Documents.AddAsync(doc);
        await context.SaveChangesAsync();

        var request = new SyncPullRequestDto
        {
            CustomerId = customer.Id,
            LastSyncTimestamp = null // Full sync
        };

        var result = await syncService.PullDeltaPackageAsync(request, userId, "Customer");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsDeltaSync);
        Assert.Single(result.Data.Trips);
        Assert.Single(result.Data.Bookings);
        Assert.Single(result.Data.Documents);
        Assert.NotEmpty(result.Data.EmergencyContacts);

        // Verify emergency contacts contain Tomer Group Cusco Desk and Chabad House
        Assert.Contains(result.Data.EmergencyContacts, c => c.Name.Contains("Tomer Group") && c.HasOxygen);
        Assert.Contains(result.Data.EmergencyContacts, c => c.Name.Contains("Chabad House"));
    }

    [Fact]
    public async Task PullDeltaPackageAsync_DeltaSync_OnlyReturnsModifiedEntities()
    {
        using var context = CreateInMemoryContext();
        var syncService = new SyncService(context);

        var userId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "Danny",
            LastName = "Cohen"
        };
        await context.Customers.AddAsync(customer);

        // Trip 1: modified yesterday
        var oldTrip = new Trip
        {
            Id = Guid.NewGuid(),
            TripCode = "TG-OLD",
            CustomerId = customer.Id,
            Title = "Old Trip",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // Trip 2: modified 10 minutes ago
        var recentTrip = new Trip
        {
            Id = Guid.NewGuid(),
            TripCode = "TG-RECENT",
            CustomerId = customer.Id,
            Title = "Recent Trip",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        await context.Trips.AddRangeAsync(oldTrip, recentTrip);
        await context.SaveChangesAsync();

        var request = new SyncPullRequestDto
        {
            CustomerId = customer.Id,
            LastSyncTimestamp = DateTime.UtcNow.AddHours(-1) // Delta sync cutoff
        };

        var result = await syncService.PullDeltaPackageAsync(request, userId, "Customer");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsDeltaSync);
        Assert.Single(result.Data.Trips);
        Assert.Equal("Recent Trip", result.Data.Trips[0].Title);
    }

    [Fact]
    public async Task PullDeltaPackageAsync_CustomerIsolation_CannotSyncOthersData()
    {
        using var context = CreateInMemoryContext();
        var syncService = new SyncService(context);

        var dannyUserId = Guid.NewGuid();
        var danny = new Customer { Id = Guid.NewGuid(), UserId = dannyUserId, FirstName = "Danny", LastName = "Cohen" };

        var noamUserId = Guid.NewGuid();
        var noam = new Customer { Id = Guid.NewGuid(), UserId = noamUserId, FirstName = "Noam", LastName = "Levi" };

        await context.Customers.AddRangeAsync(danny, noam);
        await context.SaveChangesAsync();

        // Noam tries to sync Danny's package
        var request = new SyncPullRequestDto { CustomerId = danny.Id };
        var result = await syncService.PullDeltaPackageAsync(request, noamUserId, "Customer");

        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message);
    }

    [Fact]
    public async Task PushClientChangesAsync_UpdatesEmergencyDetails_AndLogsAudit()
    {
        using var context = CreateInMemoryContext();
        var syncService = new SyncService(context);

        var userId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = "Danny",
            LastName = "Cohen",
            EmergencyContactName = "Old Contact",
            EmergencyContactPhone = "+972 50 000 0000"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var pushRequest = new SyncPushRequestDto
        {
            CustomerId = customer.Id,
            DeviceId = "iPhone-15-Danny",
            UpdatedEmergencyContact = new UpdateCustomerEmergencyDto
            {
                EmergencyContactName = "Sarah Cohen (Wife)",
                EmergencyContactPhone = "+972 52 987 6543",
                MedicalNotes = "Mild asthma, carries Ventolin inhaler"
            }
        };

        var result = await syncService.PushClientChangesAsync(pushRequest, userId, "Customer");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Success);
        Assert.Equal(1, result.Data.AcknowledgedItems);

        // Verify Customer updated in DB
        var updatedCustomer = await context.Customers.FindAsync(customer.Id);
        Assert.NotNull(updatedCustomer);
        Assert.Equal("Sarah Cohen (Wife)", updatedCustomer.EmergencyContactName);
        Assert.Equal("+972 52 987 6543", updatedCustomer.EmergencyContactPhone);
        Assert.Contains("asthma", updatedCustomer.MedicalNotes);

        // Verify AuditLog
        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "OfflineSyncPush");
        Assert.NotNull(audit);
        Assert.Equal(userId, audit.UserId);
        Assert.Contains("iPhone-15-Danny", audit.MetadataJson);
    }

    [Fact]
    public async Task PushClientChangesAsync_CompletesActivityCheckIns()
    {
        using var context = CreateInMemoryContext();
        var syncService = new SyncService(context);

        var userId = Guid.NewGuid();
        var customer = new Customer { Id = Guid.NewGuid(), UserId = userId, FirstName = "Danny", LastName = "Cohen" };
        var trip = new Trip { Id = Guid.NewGuid(), CustomerId = customer.Id, Title = "Salkantay" };
        var day = new TripDay { Id = Guid.NewGuid(), TripId = trip.Id, DayNumber = 1, Title = "Day 1" };
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            TripDayId = day.Id,
            Title = "Arrival at Soraypampa Camp",
            Status = ActivityStatus.Confirmed
        };

        await context.Customers.AddAsync(customer);
        await context.Trips.AddAsync(trip);
        await context.TripDays.AddAsync(day);
        await context.Activities.AddAsync(activity);
        await context.SaveChangesAsync();

        var pushRequest = new SyncPushRequestDto
        {
            CustomerId = customer.Id,
            ActivityCheckIns = new List<ActivityCheckInDto>
            {
                new()
                {
                    ActivityId = activity.Id,
                    CheckInTimestamp = DateTime.UtcNow,
                    Latitude = -13.3892,
                    Longitude = -72.5714,
                    Note = "Arrived safely at Soraypampa, oxygen levels 92%"
                }
            }
        };

        var result = await syncService.PushClientChangesAsync(pushRequest, userId, "Customer");

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.AcknowledgedItems);

        var updatedActivity = await context.Activities.FindAsync(activity.Id);
        Assert.NotNull(updatedActivity);
        Assert.Equal(ActivityStatus.Completed, updatedActivity.Status);
    }
}
