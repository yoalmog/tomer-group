using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase3CustomerTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_CustomerTests_{Guid.NewGuid()}")
            .Options;

        return new TomerDbContext(options);
    }

    [Fact]
    public async Task CustomerCreation_PersistsAllHebrewAndTravelFields()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);
        var staffUserId = Guid.NewGuid();

        var createDto = new CreateCustomerDto
        {
            FirstName = "Yoni",
            LastName = "Ben-David",
            HebrewName = "יוני בן-דוד",
            PassportName = "YONATAN BEN DAVID",
            Phone = "+972 50 777 3322",
            WhatsApp = "+972 50 777 3322",
            Email = "yoni.bd@israel.com",
            Country = "Israel",
            PassportNumber = "IL-55829104",
            PassportExpiration = DateTime.UtcNow.AddYears(4),
            DateOfBirth = new DateTime(1990, 11, 3, 0, 0, 0, DateTimeKind.Utc),
            IsraelIdNumber = "028193847",
            MedicalNotes = "Asthma inhaler carried during high altitude treks",
            InsuranceCompany = "Phoenix Insurance (Extreme Sports & Rescue)",
            InsurancePolicyNumber = "PH-2026-7788",
            IsActiveInPeru = true,
            EmergencyContactName = "Tamar Ben-David",
            EmergencyContactPhone = "+972 50 777 3322",
            DietaryPreferences = "Vegan / טבעוני",
            SpecialRequests = "Private trekking guide for Salkantay",
            Notes = "Experienced high-altitude trekker."
        };

        // Act
        var result = await customerService.CreateAsync(createDto, staffUserId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("יוני בן-דוד", result.Data.HebrewName);
        Assert.Equal("YONATAN BEN DAVID", result.Data.PassportName);
        Assert.Equal("+972 50 777 3322", result.Data.WhatsApp);
        Assert.Equal("Vegan / טבעוני", result.Data.DietaryPreferences);
        Assert.True(result.Data.IsActiveInPeru);

        // Verify audit log was written
        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "CustomerCreate");
        Assert.NotNull(audit);
        Assert.Equal(staffUserId, audit.UserId);
    }

    [Fact]
    public async Task CustomerIsolation_CustomerACannotAccessCustomerBProfile()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);

        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();

        var customerA = new Customer
        {
            UserId = userAId,
            FirstName = "Danny",
            LastName = "Cohen",
            Email = "danny@israel.com",
            PassportNumber = "IL-39482711"
        };
        var customerB = new Customer
        {
            UserId = userBId,
            FirstName = "Maya",
            LastName = "Levi",
            Email = "maya@israel.com",
            PassportNumber = "IL-28941033"
        };

        await context.Customers.AddRangeAsync(customerA, customerB);
        await context.SaveChangesAsync();

        // Act: Customer A attempts to access Customer B's profile
        var result = await customerService.GetByIdAsync(customerB.Id, userAId, "Customer");

        // Assert (Strict isolation enforced: Section 34)
        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message);
    }

    [Fact]
    public async Task SensitiveDataMasking_DefaultGetById_MasksPassportAndId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);
        var userId = Guid.NewGuid();

        var customer = new Customer
        {
            UserId = userId,
            FirstName = "Danny",
            LastName = "Cohen",
            Email = "danny@israel.com",
            PassportNumber = "IL-39482711",
            IsraelIdNumber = "038291048",
            PassportExpiration = DateTime.UtcNow.AddYears(3)
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        // Act: Default fetch
        var result = await customerService.GetByIdAsync(customer.Id, userId, "Customer");

        // Assert (Section 14: Protect sensitive information)
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("IL-****2711", result.Data.MaskedPassportNumber);
        Assert.Equal("******048", result.Data.MaskedIsraelId);
    }

    [Fact]
    public async Task SensitiveDataEndpoint_AuthorizedReveal_UnmasksPassportAndLogsAudit()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);
        var userId = Guid.NewGuid();
        var opsStaffId = Guid.NewGuid();

        var customer = new Customer
        {
            UserId = userId,
            FirstName = "Danny",
            LastName = "Cohen",
            Email = "danny@israel.com",
            PassportNumber = "IL-39482711",
            IsraelIdNumber = "038291048",
            PassportExpiration = DateTime.UtcNow.AddYears(3)
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        // Act 1: Customer themselves views their own unmasked details
        var selfResult = await customerService.GetSensitiveDetailsAsync(customer.Id, userId, "Customer");
        Assert.True(selfResult.Success);
        Assert.Equal("IL-39482711", selfResult.Data!.PassportNumber);
        Assert.Equal("038291048", selfResult.Data.IsraelIdNumber);

        // Act 2: Operations staff views unmasked details
        var opsResult = await customerService.GetSensitiveDetailsAsync(customer.Id, opsStaffId, "Operations");
        Assert.True(opsResult.Success);
        Assert.Equal("IL-39482711", opsResult.Data!.PassportNumber);

        // Assert: Audit log records both views
        var auditLogs = await context.AuditLogs
            .Where(a => a.Action == "ViewSensitiveCustomerData")
            .ToListAsync();
        Assert.Equal(2, auditLogs.Count);
        Assert.Contains(auditLogs, a => a.UserId == userId);
        Assert.Contains(auditLogs, a => a.UserId == opsStaffId);
    }

    [Fact]
    public async Task SensitiveDataEndpoint_UnauthorizedUser_Rejected()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);
        var customerUserId = Guid.NewGuid();
        var unauthorizedTravelerId = Guid.NewGuid();

        var customer = new Customer
        {
            UserId = customerUserId,
            FirstName = "Danny",
            LastName = "Cohen",
            PassportNumber = "IL-39482711"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        // Act: Another traveler attempts to access unmasked passport
        var result = await customerService.GetSensitiveDetailsAsync(customer.Id, unauthorizedTravelerId, "Customer");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message);
    }

    [Fact]
    public async Task CustomerSelfUpdate_UpdatesProfile_WithoutModifyingAgencyInternalNotes()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);
        var userId = Guid.NewGuid();

        var customer = new Customer
        {
            UserId = userId,
            FirstName = "Danny",
            LastName = "Cohen",
            Email = "danny@israel.com",
            DietaryPreferences = "Vegetarian",
            Notes = "CONFIDENTIAL AGENCY NOTE: VIP traveler with special security attention."
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        // Act: Customer self-updates dietary and special requests
        var updateDto = new UpdateCustomerProfileDto
        {
            FirstName = "Danny",
            LastName = "Cohen",
            HebrewName = "דני כהן",
            PassportName = "DANNY COHEN",
            Phone = "+972 54 123 4567",
            WhatsApp = "+972 54 123 4567",
            Email = "danny@israel.com",
            Country = "Israel",
            DietaryPreferences = "כשר למהדרין (Kosher Mehudar)",
            SpecialRequests = "Needs early breakfast box for Machu Picchu day",
            EmergencyContactName = "Sarah Cohen",
            EmergencyContactPhone = "+972 54 999 1122"
        };

        var result = await customerService.UpdateOwnProfileAsync(userId, updateDto);

        // Assert
        Assert.True(result.Success);
        var updated = await context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        Assert.NotNull(updated);
        Assert.Equal("כשר למהדרין (Kosher Mehudar)", updated.DietaryPreferences);
        Assert.Equal("דני כהן", updated.HebrewName);
        // Agency internal note must remain intact and unmodified
        Assert.Equal("CONFIDENTIAL AGENCY NOTE: VIP traveler with special security attention.", updated.Notes);
    }

    [Fact]
    public async Task AgencySearchAndFilter_SupportsHebrewAndDietaryFilters()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);

        var c1 = new Customer
        {
            FirstName = "Danny",
            LastName = "Cohen",
            HebrewName = "דני כהן",
            Phone = "+972 54 123 4567",
            WhatsApp = "+972 54 123 4567",
            Email = "danny@israel.com",
            Country = "Israel",
            DietaryPreferences = "Kosher Mehudar",
            IsActiveInPeru = true
        };
        var c2 = new Customer
        {
            FirstName = "Maya",
            LastName = "Levi",
            HebrewName = "מאיה לוי",
            Phone = "+972 52 444 8899",
            Email = "maya@israel.com",
            Country = "Israel",
            DietaryPreferences = "Vegetarian",
            IsActiveInPeru = false
        };
        var c3 = new Customer
        {
            FirstName = "Carlos",
            LastName = "Gomez",
            HebrewName = "",
            Phone = "+51 984 555 111",
            Email = "carlos@peru.com",
            Country = "Peru",
            DietaryPreferences = "None",
            IsActiveInPeru = true
        };

        await context.Customers.AddRangeAsync(c1, c2, c3);
        await context.SaveChangesAsync();

        // Act 1: Search by Hebrew name "דני"
        var hebrewSearch = await customerService.GetSummariesAsync(new CustomerSearchFilterDto { Search = "דני" });
        Assert.True(hebrewSearch.Success);
        Assert.Single(hebrewSearch.Data!.Items);
        Assert.Equal("Danny", hebrewSearch.Data.Items[0].FirstName);

        // Act 2: Filter by Kosher dietary preference
        var kosherFilter = await customerService.GetSummariesAsync(new CustomerSearchFilterDto { DietaryPreference = "Kosher" });
        Assert.True(kosherFilter.Success);
        Assert.Single(kosherFilter.Data!.Items);
        Assert.Equal("Danny", kosherFilter.Data.Items[0].FirstName);

        // Act 3: Filter by Active In Peru
        var activeInPeru = await customerService.GetSummariesAsync(new CustomerSearchFilterDto { IsActiveInPeru = true });
        Assert.True(activeInPeru.Success);
        Assert.Equal(2, activeInPeru.Data!.Items.Count);
    }

    [Fact]
    public async Task SafeDeletion_PreventsDeletingCustomerWithActiveTrips()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);
        var staffId = Guid.NewGuid();

        var customer = new Customer
        {
            FirstName = "Danny",
            LastName = "Cohen",
            Email = "danny@israel.com"
        };
        await context.Customers.AddAsync(customer);

        var activeTrip = new Trip
        {
            CustomerId = customer.Id,
            TripCode = "PERU-2026-00482",
            Title = "Peru Adventure",
            Status = TripStatus.InProgress,
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(5)
        };
        await context.Trips.AddAsync(activeTrip);
        await context.SaveChangesAsync();

        // Act: Attempt to delete customer with active trip
        var result = await customerService.DeleteAsync(customer.Id, staffId);

        // Assert (Section 54: Prevent dangerous data loss)
        Assert.False(result.Success);
        Assert.Contains("Cannot delete traveler", result.Message);

        var reloaded = await context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        Assert.NotNull(reloaded);
        Assert.False(reloaded.IsDeleted);
    }

    [Fact]
    public async Task SafeDeletion_CleanCustomer_SoftDeletesSuccessfully()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);
        var staffId = Guid.NewGuid();

        var customer = new Customer
        {
            FirstName = "Inquiry",
            LastName = "Only",
            Email = "inquiry@israel.com"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        // Act: Delete customer without active trips or bookings
        var result = await customerService.DeleteAsync(customer.Id, staffId);

        // Assert
        Assert.True(result.Success);

        // Verify soft deletion
        var inDb = await context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == customer.Id);
        Assert.NotNull(inDb);
        Assert.True(inDb.IsDeleted);

        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "CustomerDelete");
        Assert.NotNull(audit);
    }

    [Fact]
    public async Task TravelerStats_CalculatesAccurateMetrics()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);

        var c1 = new Customer
        {
            FirstName = "Danny",
            LastName = "Cohen",
            Country = "Israel",
            Phone = "+972 54 123 4567",
            WhatsApp = "+972 54 123 4567",
            IsActiveInPeru = true,
            DietaryPreferences = "Kosher Mehudar",
            PassportExpiration = DateTime.UtcNow.AddYears(3)
        };
        var c2 = new Customer
        {
            FirstName = "Maya",
            LastName = "Levi",
            Country = "Israel",
            Phone = "+972 52 444 8899",
            WhatsApp = "+972 52 444 8899",
            IsActiveInPeru = false,
            DietaryPreferences = "Vegetarian",
            PassportExpiration = DateTime.UtcNow.AddMonths(2) // Expiring soon!
        };
        var c3 = new Customer
        {
            FirstName = "Yoni",
            LastName = "Ben-David",
            Country = "Israel",
            Phone = "+972 50 777 3322",
            WhatsApp = "+972 50 777 3322",
            IsActiveInPeru = true,
            DietaryPreferences = "Vegan",
            PassportExpiration = DateTime.UtcNow.AddYears(2)
        };

        await context.Customers.AddRangeAsync(c1, c2, c3);
        await context.SaveChangesAsync();

        // Act
        var statsResult = await customerService.GetTravelerStatsAsync();

        // Assert
        Assert.True(statsResult.Success);
        var stats = statsResult.Data!;
        Assert.Equal(3, stats.TotalTravelers);
        Assert.Equal(2, stats.ActiveInPeru);
        Assert.Equal(3, stats.IsraeliTravelersCount);
        Assert.Equal(100.0, stats.IsraeliTravelersPercentage);
        Assert.Equal(1, stats.KosherTravelersCount);
        Assert.Equal(2, stats.VegetarianVeganCount);
        Assert.Equal(1, stats.ExpiringPassportCount);
    }
}

