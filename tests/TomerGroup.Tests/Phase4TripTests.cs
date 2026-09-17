using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase4TripTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_TripTests_{Guid.NewGuid()}")
            .Options;

        return new TomerDbContext(options);
    }

    [Fact]
    public async Task TripCreation_GeneratesCanonicalPeruTripCode()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var tripService = new TripService(context);

        var customer = new Customer
        {
            FirstName = "Danny",
            LastName = "Cohen",
            Email = "danny@israel.com"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var createDto = new CreateTripDto
        {
            CustomerId = customer.Id,
            Title = "Danny's Peru Travel Experience",
            Description = "VIP Expedition across Cusco, Sacred Valley and Machu Picchu",
            StartDate = new DateTime(2026, 9, 15),
            EndDate = new DateTime(2026, 9, 22),
            TotalRevenue = 2500,
            Currency = Currency.USD
        };

        // Act
        var result = await tripService.CreateAsync(createDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.StartsWith("PERU-2026-", result.Data.TripCode);
        Assert.Equal(TripStatus.Scheduled, result.Data.Status);
        Assert.Equal(2500, result.Data.TotalRevenue);
    }

    [Fact]
    public async Task CustomerIsolation_CustomerACannotAccessCustomerBTrip()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var tripService = new TripService(context);

        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();

        var customerA = new Customer { UserId = userAId, FirstName = "Danny", LastName = "Cohen" };
        var customerB = new Customer { UserId = userBId, FirstName = "Maya", LastName = "Levi" };

        await context.Customers.AddRangeAsync(customerA, customerB);
        await context.SaveChangesAsync();

        var tripB = new Trip
        {
            CustomerId = customerB.Id,
            Customer = customerB,
            TripCode = "PERU-2026-00088",
            Title = "Maya's Sacred Valley Trip",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5)
        };
        await context.Trips.AddAsync(tripB);
        await context.SaveChangesAsync();

        // Act: Customer A attempts to access Customer B's trip
        var result = await tripService.GetByIdAsync(tripB.Id, userAId, "Customer");

        // Assert (Strict customer isolation enforced: Section 34)
        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message);
    }

    [Fact]
    public async Task CustomerIsolation_CustomerACannotAccessCustomerBDashboard()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var tripService = new TripService(context);

        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();

        var customerA = new Customer { UserId = userAId, FirstName = "Danny" };
        var customerB = new Customer { UserId = userBId, FirstName = "Maya" };

        await context.Customers.AddRangeAsync(customerA, customerB);
        await context.SaveChangesAsync();

        // Act: Customer A attempts to view Customer B's home dashboard
        var result = await tripService.GetCustomerHomeDashboardAsync(customerB.Id, userAId, "Customer");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message);
    }

    [Fact]
    public async Task TripBuilder_AddsDaysAndActivities_MaintainsChronologicalTimeline()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var tripService = new TripService(context);
        var staffId = Guid.NewGuid();

        var customer = new Customer { FirstName = "Danny", LastName = "Cohen", Email = "danny@israel.com" };
        var guide = new Guide { FullName = "Carlos Quispe", Phone = "+51 984 777 666" };
        var driver = new Driver { FullName = "Juan Flores", Phone = "+51 984 888 555" };

        await context.Customers.AddAsync(customer);
        await context.Guides.AddAsync(guide);
        await context.Drivers.AddAsync(driver);
        await context.SaveChangesAsync();

        var trip = new Trip
        {
            CustomerId = customer.Id,
            Customer = customer,
            TripCode = "PERU-2026-00482",
            Title = "Peru Itinerary",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(5)
        };
        await context.Trips.AddAsync(trip);
        await context.SaveChangesAsync();

        // Act 1: Add Day 4 (Machu Picchu)
        var dayResult = await tripService.AddTripDayAsync(trip.Id, new CreateTripDayDto
        {
            TripId = trip.Id,
            DayNumber = 4,
            Date = trip.StartDate.AddDays(3),
            Title = "DAY 4: Machu Picchu VIP Expedition",
            Destination = "Machu Picchu",
            Description = "Full day exploration of Incan sanctuary"
        }, staffId);

        Assert.True(dayResult.Success);
        var dayId = dayResult.Data!.Id;

        // Act 2: Add 3 Chronological Activities matching Section 8 & 9
        var act1 = await tripService.AddActivityAsync(dayId, new CreateActivityDto
        {
            TripDayId = dayId,
            Title = "05:30 🚐 איסוף מהמלון",
            StartTime = new TimeSpan(5, 30, 0),
            EndTime = new TimeSpan(6, 0, 0),
            Location = "Hotel Lobby, Cusco",
            DriverId = driver.Id,
            Instructions = "Please wait in lobby with daypack and passport",
            Status = ActivityStatus.Confirmed
        }, staffId);

        var act2 = await tripService.AddActivityAsync(dayId, new CreateActivityDto
        {
            TripDayId = dayId,
            Title = "08:30 🏔️ מאצ'ו פיצ'ו",
            StartTime = new TimeSpan(8, 30, 0),
            EndTime = new TimeSpan(12, 30, 0),
            Location = "Machu Picchu Circuit 2",
            GuideId = guide.Id,
            Instructions = "Original physical passport required for entry",
            Status = ActivityStatus.Confirmed
        }, staffId);

        var act3 = await tripService.AddActivityAsync(dayId, new CreateActivityDto
        {
            TripDayId = dayId,
            Title = "16:00 🚂 רכבת חזרה",
            StartTime = new TimeSpan(16, 0, 0),
            EndTime = new TimeSpan(18, 30, 0),
            Location = "Aguas Calientes PeruRail Station",
            Status = ActivityStatus.Scheduled
        }, staffId);

        // Assert
        Assert.True(act1.Success);
        Assert.True(act2.Success);
        Assert.True(act3.Success);

        // Retrieve full trip and verify activities order
        var fullTrip = await tripService.GetByIdAsync(trip.Id, staffId, "Staff");
        Assert.True(fullTrip.Success);
        Assert.Single(fullTrip.Data!.Days);

        var day = fullTrip.Data.Days[0];
        Assert.Equal(3, day.Activities.Count);
        Assert.Equal(new TimeSpan(5, 30, 0), day.Activities[0].StartTime);
        Assert.Equal(new TimeSpan(8, 30, 0), day.Activities[1].StartTime);
        Assert.Equal(new TimeSpan(16, 0, 0), day.Activities[2].StartTime);
        Assert.Equal("Juan Flores", day.Activities[0].DriverName);
        Assert.Equal("Carlos Quispe", day.Activities[1].GuideName);
    }

    [Fact]
    public async Task ActivityStatus_TransitionsThroughLifecycle_AndLogsAudit()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var tripService = new TripService(context);
        var guideUserId = Guid.NewGuid();

        var tripDay = new TripDay
        {
            DayNumber = 1,
            Date = DateTime.UtcNow,
            Title = "Arrival Day"
        };
        await context.TripDays.AddAsync(tripDay);

        var activity = new Activity
        {
            TripDayId = tripDay.Id,
            Title = "Airport Pickup",
            StartTime = new TimeSpan(10, 0, 0),
            Status = ActivityStatus.Scheduled
        };
        await context.Activities.AddAsync(activity);
        await context.SaveChangesAsync();

        // Act 1: Transition to InProgress
        var startResult = await tripService.UpdateActivityStatusAsync(activity.Id, ActivityStatus.InProgress, guideUserId);
        Assert.True(startResult.Success);
        Assert.Equal(ActivityStatus.InProgress, startResult.Data!.Status);

        // Act 2: Transition to Completed
        var finishResult = await tripService.UpdateActivityStatusAsync(activity.Id, ActivityStatus.Completed, guideUserId);
        Assert.True(finishResult.Success);
        Assert.Equal(ActivityStatus.Completed, finishResult.Data!.Status);

        // Assert: Audit log records status change
        var auditLogs = await context.AuditLogs.Where(a => a.Action == "ActivityStatusUpdate").ToListAsync();
        Assert.Equal(2, auditLogs.Count);
    }

    [Fact]
    public async Task PeruDestinations_DynamicSupportWithoutHardcoding()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var tripService = new TripService(context);
        var staffId = Guid.NewGuid();

        // Initial query: empty
        var initial = await tripService.GetDestinationsAsync();
        Assert.True(initial.Success);
        Assert.Empty(initial.Data!);

        // Act: Agency adds new dynamic destination (Section 10: Dynamic Destinations)
        var createResult = await tripService.CreateDestinationAsync(new CreateDestinationDto
        {
            Name = "Choquequirao",
            HebrewName = "צ'וקקיראו",
            SpanishName = "Choquequirao",
            Region = "Cusco",
            AltitudeMeters = 3050,
            Description = "Sacred sister city of Machu Picchu reached via challenging 4-day trek",
            AltitudeWarning = "High altitude mountain passes",
            IsPopular = true
        }, staffId);

        // Assert
        Assert.True(createResult.Success);
        Assert.NotNull(createResult.Data);
        Assert.Equal("Choquequirao", createResult.Data.Name);
        Assert.Equal(3050, createResult.Data.AltitudeMeters);

        // Verify destination is now returned in directory
        var list = await tripService.GetDestinationsAsync();
        Assert.True(list.Success);
        Assert.Single(list.Data!);
        Assert.Equal("צ'וקקיראו", list.Data![0].HebrewName);
    }

    [Fact]
    public async Task CustomerHomeDashboard_ReturnsTodayActivitiesAndEmergencyInfo()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var tripService = new TripService(context);
        var customerUserId = Guid.NewGuid();

        var customer = new Customer
        {
            UserId = customerUserId,
            FirstName = "Danny",
            LastName = "Cohen",
            HebrewName = "דני כהן"
        };
        await context.Customers.AddAsync(customer);

        var trip = new Trip
        {
            CustomerId = customer.Id,
            Customer = customer,
            TripCode = "PERU-2026-00482",
            Title = "חוויית פרו — 5 ימים",
            Status = TripStatus.InProgress,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(2)
        };
        await context.Trips.AddAsync(trip);

        var day4 = new TripDay
        {
            TripId = trip.Id,
            DayNumber = 4,
            Date = DateTime.UtcNow.Date,
            Title = "יום 4: סיור VIP במאצ'ו פיצ'ו",
            Destination = "Machu Picchu"
        };
        await context.TripDays.AddAsync(day4);

        var act1 = new Activity
        {
            TripDayId = day4.Id,
            Title = "05:30 🚐 איסוף מהמלון",
            StartTime = new TimeSpan(5, 30, 0),
            Status = ActivityStatus.Completed
        };
        var act2 = new Activity
        {
            TripDayId = day4.Id,
            Title = "08:30 🏔️ מאצ'ו פיצ'ו",
            StartTime = new TimeSpan(8, 30, 0),
            Status = ActivityStatus.Scheduled
        };
        await context.Activities.AddRangeAsync(act1, act2);
        await context.SaveChangesAsync();

        // Act
        var dashResult = await tripService.GetCustomerHomeDashboardAsync(customer.Id, customerUserId, "Customer");

        // Assert
        Assert.True(dashResult.Success);
        var dash = dashResult.Data!;
        Assert.Equal("PERU-2026-00482", dash.TripCode);
        Assert.Equal("דני כהן", dash.CustomerName);
        Assert.Equal("Machu Picchu", dash.CurrentDestination);
        Assert.Equal(2, dash.TodayActivities.Count);
        // Next activity should be the uncompleted one (08:30 Machu Picchu)
        Assert.NotNull(dash.NextActivity);
        Assert.Equal("08:30 🏔️ מאצ'ו פיצ'ו", dash.NextActivity.Title);
        Assert.Equal("+51 984 999 888", dash.AgencyEmergencyPhone);
    }
}

