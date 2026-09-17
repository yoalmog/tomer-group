using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase6GuidesDriversTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_Phase6_{Guid.NewGuid()}")
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task GuideCreation_StoresDirceturLicense_AndFirstAidCertification()
    {
        using var context = CreateInMemoryContext();
        var guideService = new GuideService(context);

        var request = new CreateGuideDto
        {
            FullName = "Carlos Quispe Mendoza",
            HebrewName = "קרלוס קיספה (דובר עברית)",
            Phone = "+51 984 555 666",
            WhatsApp = "+51984555666",
            Email = "carlos.guide@tomergroup.com",
            CertificationNumber = "DIRCETUR-CUS-1049",
            DailyRate = 130,
            Currency = Currency.USD,
            FirstAidCertified = true,
            IsJewishHeritageExpert = true,
            Languages = new() { "Hebrew", "English", "Spanish", "Quechua" },
            Specialization = "Inca History, High-Altitude Trekking, Kosher Awareness"
        };

        var response = await guideService.CreateAsync(request);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("DIRCETUR-CUS-1049", response.Data.CertificationNumber);
        Assert.True(response.Data.FirstAidCertified);
        Assert.True(response.Data.IsJewishHeritageExpert);
        Assert.Contains("Hebrew", response.Data.Languages);
        Assert.Equal(130, response.Data.DailyRate);
    }

    [Fact]
    public async Task DriverCreation_StoresMtcLicense_AndProfessionalCategory()
    {
        using var context = CreateInMemoryContext();
        var driverService = new DriverService(context);

        var request = new CreateDriverDto
        {
            FullName = "Juan Huaman Ortiz",
            Phone = "+51 984 112 233",
            WhatsApp = "+51984112233",
            Email = "juan.driver@tomergroup.com",
            LicenseNumber = "Q482910",
            LicenseCategory = "A-IIIa Profesional",
            LicenseExpirationDate = DateTime.UtcNow.AddYears(3),
            EmergencyContact = "Maria Huaman (+51 984 998 877)"
        };

        var response = await driverService.CreateAsync(request);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Q482910", response.Data.LicenseNumber);
        Assert.Equal("A-IIIa Profesional", response.Data.LicenseCategory);
        Assert.True(response.Data.IsAvailable);
    }

    [Fact]
    public async Task GuideAssignment_ScheduleConflict_PreventsDoubleBooking()
    {
        using var context = CreateInMemoryContext();
        var guideService = new GuideService(context);

        var guide = new Guide
        {
            Id = Guid.NewGuid(),
            FullName = "Carlos Quispe",
            CertificationNumber = "DIRCETUR-CUS-1049",
            IsAvailable = true
        };

        var tripDay = new TripDay
        {
            Id = Guid.NewGuid(),
            DayNumber = 1,
            Date = new DateTime(2026, 10, 15)
        };

        var existingActivity = new Activity
        {
            Id = Guid.NewGuid(),
            TripDayId = tripDay.Id,
            TripDay = tripDay,
            GuideId = guide.Id,
            Title = "Machu Picchu Citadel Morning Tour",
            StartTime = new TimeSpan(8, 30, 0),
            EndTime = new TimeSpan(12, 30, 0),
            Status = ActivityStatus.Confirmed
        };

        var overlappingActivity = new Activity
        {
            Id = Guid.NewGuid(),
            TripDayId = tripDay.Id,
            TripDay = tripDay,
            Title = "Huayna Picchu Hike",
            StartTime = new TimeSpan(11, 0, 0),
            EndTime = new TimeSpan(14, 0, 0),
            Status = ActivityStatus.Scheduled
        };

        await context.Guides.AddAsync(guide);
        await context.TripDays.AddAsync(tripDay);
        await context.Activities.AddRangeAsync(existingActivity, overlappingActivity);
        await context.SaveChangesAsync();

        // Attempting to assign guide to overlapping activity
        var assignResponse = await guideService.AssignGuideToActivityAsync(new AssignGuideToActivityDto
        {
            ActivityId = overlappingActivity.Id,
            GuideId = guide.Id
        }, Guid.NewGuid());

        Assert.False(assignResponse.Success);
        Assert.Contains("Schedule conflict", assignResponse.Message);
        Assert.Contains("already assigned", assignResponse.Message);
    }

    [Fact]
    public async Task GuideAssignment_NonOverlapping_SucceedsAndLogsAudit()
    {
        using var context = CreateInMemoryContext();
        var guideService = new GuideService(context);

        var guide = new Guide
        {
            Id = Guid.NewGuid(),
            FullName = "Carlos Quispe",
            CertificationNumber = "DIRCETUR-CUS-1049",
            IsAvailable = true
        };

        var tripDay = new TripDay
        {
            Id = Guid.NewGuid(),
            DayNumber = 1,
            Date = new DateTime(2026, 10, 15)
        };

        var morningActivity = new Activity
        {
            Id = Guid.NewGuid(),
            TripDayId = tripDay.Id,
            TripDay = tripDay,
            GuideId = guide.Id,
            Title = "Machu Picchu Morning Tour",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(11, 0, 0),
            Status = ActivityStatus.Confirmed
        };

        var afternoonActivity = new Activity
        {
            Id = Guid.NewGuid(),
            TripDayId = tripDay.Id,
            TripDay = tripDay,
            Title = "Inca Bridge Afternoon Hike",
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(16, 30, 0),
            Status = ActivityStatus.Scheduled
        };

        await context.Guides.AddAsync(guide);
        await context.TripDays.AddAsync(tripDay);
        await context.Activities.AddRangeAsync(morningActivity, afternoonActivity);
        await context.SaveChangesAsync();

        var assignResponse = await guideService.AssignGuideToActivityAsync(new AssignGuideToActivityDto
        {
            ActivityId = afternoonActivity.Id,
            GuideId = guide.Id
        }, Guid.NewGuid());

        Assert.True(assignResponse.Success);

        // Verify guide assigned and audit log recorded
        var updated = await context.Activities.FindAsync(afternoonActivity.Id);
        Assert.Equal(guide.Id, updated!.GuideId);

        var auditLog = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "AssignGuideToActivity");
        Assert.NotNull(auditLog);
        Assert.Contains("Carlos Quispe", auditLog.MetadataJson);
    }

    [Fact]
    public async Task DriverAssignment_ScheduleBufferConflict_EnforcesTransitBuffer()
    {
        using var context = CreateInMemoryContext();
        var driverService = new DriverService(context);

        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            FullName = "Juan Huaman",
            LicenseNumber = "Q482910",
            IsAvailable = true
        };

        var transfer1 = new Transportation
        {
            Id = Guid.NewGuid(),
            DriverId = driver.Id,
            ServiceType = "Airport Transfer",
            ScheduledPickupTime = new DateTime(2026, 10, 15, 10, 0, 0),
            Status = TransportationStatus.Assigned
        };

        var transfer2 = new Transportation
        {
            Id = Guid.NewGuid(),
            ServiceType = "Hotel to Train Station Transfer",
            ScheduledPickupTime = new DateTime(2026, 10, 15, 10, 35, 0), // Only 35 minutes later - conflicts with 60m buffer!
            Status = TransportationStatus.Assigned
        };

        await context.Drivers.AddAsync(driver);
        await context.Transportations.AddRangeAsync(transfer1, transfer2);
        await context.SaveChangesAsync();

        var assignResponse = await driverService.AssignDriverToTransferAsync(new AssignDriverToTransferDto
        {
            TransportationId = transfer2.Id,
            DriverId = driver.Id
        }, Guid.NewGuid());

        Assert.False(assignResponse.Success);
        Assert.Contains("Schedule conflict", assignResponse.Message);
        Assert.Contains("60 minute buffer", assignResponse.Message);
    }

    [Fact]
    public async Task GuideDailyManifest_AggregatesTravelerDetails_AndSpecialNeeds()
    {
        using var context = CreateInMemoryContext();
        var guideService = new GuideService(context);

        var guide = new Guide { Id = Guid.NewGuid(), FullName = "Carlos Quispe" };
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Danny",
            LastName = "Cohen",
            Phone = "+972 54 123 4567",
            WhatsApp = "+972541234567",
            DietaryPreferences = "Mehudar Glatt Kosher (Chabad)",
            MedicalNotes = "Mild Soroche on Day 1"
        };
        var trip = new Trip { Id = Guid.NewGuid(), CustomerId = customer.Id, Customer = customer };
        var tripDay = new TripDay { Id = Guid.NewGuid(), TripId = trip.Id, Trip = trip, Date = new DateTime(2026, 10, 20) };
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            TripDayId = tripDay.Id,
            TripDay = tripDay,
            GuideId = guide.Id,
            Title = "Machu Picchu Citadel Tour",
            StartTime = new TimeSpan(8, 30, 0),
            EndTime = new TimeSpan(12, 30, 0),
            Location = "Sanctuary Gate",
            Instructions = "Passports required"
        };

        await context.Guides.AddAsync(guide);
        await context.Customers.AddAsync(customer);
        await context.Trips.AddAsync(trip);
        await context.TripDays.AddAsync(tripDay);
        await context.Activities.AddAsync(activity);
        await context.SaveChangesAsync();

        var manifestResponse = await guideService.GetDailyManifestAsync(guide.Id, new DateTime(2026, 10, 20));

        Assert.True(manifestResponse.Success);
        Assert.NotNull(manifestResponse.Data);
        Assert.Single(manifestResponse.Data.Assignments);
        var item = manifestResponse.Data.Assignments[0];
        Assert.Equal("Machu Picchu Citadel Tour", item.Title);
        Assert.Equal("Danny Cohen", item.CustomerName);
        Assert.Equal("+972541234567", item.CustomerWhatsApp);
        Assert.Contains("Mehudar Glatt Kosher", item.DietaryRestrictions);
        Assert.Contains("Soroche", item.MedicalNotes);
    }

    [Fact]
    public async Task DriverDailyManifest_AggregatesPickupSchedule_AndFlightNumbers()
    {
        using var context = CreateInMemoryContext();
        var driverService = new DriverService(context);

        var driver = new Driver { Id = Guid.NewGuid(), FullName = "Juan Huaman" };
        var vehicle = new Vehicle { Id = Guid.NewGuid(), Model = "Mercedes-Benz Sprinter", LicensePlate = "X4T-892" };
        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Danny", LastName = "Cohen", Phone = "+972 54 123 4567" };

        var transfer = new Transportation
        {
            Id = Guid.NewGuid(),
            DriverId = driver.Id,
            VehicleId = vehicle.Id,
            CustomerId = customer.Id,
            Customer = customer,
            Vehicle = vehicle,
            ServiceType = "Airport Transfer",
            HebrewServiceType = "איסוף מנמל התעופה",
            ScheduledPickupTime = new DateTime(2026, 10, 21, 14, 15, 0),
            PickupLocation = "CUZ Airport",
            DropoffLocation = "Hotel Palacio del Inka",
            PassengerCount = 2,
            FlightOrTrainNumber = "LATAM LA-2014"
        };

        await context.Drivers.AddAsync(driver);
        await context.Vehicles.AddAsync(vehicle);
        await context.Customers.AddAsync(customer);
        await context.Transportations.AddAsync(transfer);
        await context.SaveChangesAsync();

        var manifestResponse = await driverService.GetDailyManifestAsync(driver.Id, new DateTime(2026, 10, 21));

        Assert.True(manifestResponse.Success);
        Assert.NotNull(manifestResponse.Data);
        Assert.Single(manifestResponse.Data.Transfers);
        var item = manifestResponse.Data.Transfers[0];
        Assert.Equal("CUZ Airport", item.PickupLocation);
        Assert.Equal("LATAM LA-2014", item.FlightOrTrainNumber);
        Assert.Equal("Danny Cohen", item.CustomerName);
        Assert.Equal("Mercedes-Benz Sprinter", item.VehicleModel);
        Assert.Equal("X4T-892", item.VehiclePlate);
    }
}

