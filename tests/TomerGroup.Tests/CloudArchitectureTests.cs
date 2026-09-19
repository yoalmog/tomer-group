using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TomerGroup.Api.Controllers;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Security;
using TomerGroup.Infrastructure.Services;
using TomerGroup.Mobile.Services;
using Xunit;

namespace TomerGroup.Tests;

public class CloudArchitectureTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TomerDbContext(options);
    }

    private static void SetCustomerClaims(ControllerBase controller, string authUserId, string email)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, authUserId),
            new Claim("sub", authUserId),
            new Claim(ClaimTypes.Email, email),
            new Claim("email", email),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims, "SupabaseAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task MeController_GetProfile_ReturnsAuthenticatedCustomerData()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authUserId = Guid.NewGuid().ToString();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AuthUserId = authUserId,
            Email = "alon.traveler@example.com",
            FirstName = "Alon",
            LastName = "Mizrahi",
            Phone = "+972501234567",
            Language = "he",
            Status = "Active"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var controller = new MeController(
            context,
            new TripService(context),
            new BookingService(context),
            new DocumentService(context),
            new PaymentService(context),
            new NotificationService(context),
            NullLogger<MeController>.Instance);

        SetCustomerClaims(controller, authUserId, customer.Email);

        // Act
        var result = await controller.GetProfile(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CustomerProfileDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(customer.Id, response.Data.Id);
        Assert.Equal("Alon", response.Data.FirstName);
        Assert.Equal("Mizrahi", response.Data.LastName);
        Assert.Equal("alon.traveler@example.com", response.Data.Email);
        Assert.Equal(authUserId, response.Data.AuthUserId);
    }

    [Fact]
    public async Task MeController_UpdateProfile_UpdatesOnlyAuthenticatedCustomer()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var authUserId = Guid.NewGuid().ToString();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AuthUserId = authUserId,
            Email = "noa.cohen@example.com",
            FirstName = "Noa",
            LastName = "Cohen",
            Phone = "+972549998877",
            Language = "en"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var controller = new MeController(
            context,
            new TripService(context),
            new BookingService(context),
            new DocumentService(context),
            new PaymentService(context),
            new NotificationService(context),
            NullLogger<MeController>.Instance);

        SetCustomerClaims(controller, authUserId, customer.Email);

        var updateDto = new UpdateCustomerProfileDto
        {
            FirstName = "Noa Renamed",
            LastName = "Cohen Levi",
            Phone = "+972541112233",
            Language = "he",
            ProfileImageUrl = "https://supabase.co/storage/v1/object/public/avatars/noa.jpg"
        };

        // Act
        var result = await controller.UpdateProfile(updateDto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CustomerProfileDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Noa Renamed", response.Data.FirstName);
        Assert.Equal("Cohen Levi", response.Data.LastName);
        Assert.Equal("+972541112233", response.Data.Phone);
        Assert.Equal("he", response.Data.Language);

        // Verify database persistence
        var dbCustomer = await context.Customers.FindAsync(customer.Id);
        Assert.NotNull(dbCustomer);
        Assert.Equal("Noa Renamed", dbCustomer.FirstName);
        Assert.Equal("he", dbCustomer.Language);
        Assert.Equal("https://supabase.co/storage/v1/object/public/avatars/noa.jpg", dbCustomer.ProfileImageUrl);
    }

    [Fact]
    public async Task MeController_CustomerDataIsolation_StrictlyIsolatesTripsBookingsDocumentsPayments()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Customer A
        var authUserA = Guid.NewGuid().ToString();
        var customerA = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AuthUserId = authUserA,
            Email = "userA@israel.com",
            FirstName = "Yoni",
            LastName = "A",
            Phone = "+972500000001"
        };

        // Customer B
        var authUserB = Guid.NewGuid().ToString();
        var customerB = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AuthUserId = authUserB,
            Email = "userB@israel.com",
            FirstName = "Tamar",
            LastName = "B",
            Phone = "+972500000002"
        };

        await context.Customers.AddRangeAsync(customerA, customerB);

        // Trips
        var tripA = new Trip { Id = Guid.NewGuid(), TripCode = "TRIP-A", Title = "Yoni Inca Trail", CustomerId = customerA.Id };
        var tripB = new Trip { Id = Guid.NewGuid(), TripCode = "TRIP-B", Title = "Tamar Salkantay", CustomerId = customerB.Id };
        await context.Trips.AddRangeAsync(tripA, tripB);

        // Bookings
        var bookingA = new Booking { Id = Guid.NewGuid(), BookingCode = "BK-A", CustomerId = customerA.Id, TripId = tripA.Id, Status = BookingStatus.Confirmed, TotalAmount = 1500 };
        var bookingB = new Booking { Id = Guid.NewGuid(), BookingCode = "BK-B", CustomerId = customerB.Id, TripId = tripB.Id, Status = BookingStatus.Confirmed, TotalAmount = 2200 };
        await context.Bookings.AddRangeAsync(bookingA, bookingB);

        // Documents
        var docA = new Document { Id = Guid.NewGuid(), CustomerId = customerA.Id, Type = DocumentType.Passport, Name = "Yoni Passport", StoragePath = "passports/a.pdf" };
        var docB = new Document { Id = Guid.NewGuid(), CustomerId = customerB.Id, Type = DocumentType.Passport, Name = "Tamar Passport", StoragePath = "passports/b.pdf" };
        await context.Documents.AddRangeAsync(docA, docB);

        // Payments
        var payA = new Payment { Id = Guid.NewGuid(), BookingId = bookingA.Id, Booking = bookingA, CustomerId = customerA.Id, Amount = 500, Status = PaymentStatus.Paid, Method = PaymentMethod.CreditCard };
        var payB = new Payment { Id = Guid.NewGuid(), BookingId = bookingB.Id, Booking = bookingB, CustomerId = customerB.Id, Amount = 900, Status = PaymentStatus.Paid, Method = PaymentMethod.BankTransfer };
        await context.Payments.AddRangeAsync(payA, payB);

        // Support tickets
        var ticketA = new SupportTicket
        {
            Id = Guid.NewGuid(),
            CustomerId = customerA.Id,
            Subject = "Question A",
            Priority = SupportTicketPriority.Medium,
            Status = SupportTicketStatus.Open,
            Messages = new List<SupportTicketMessage>
            {
                new SupportTicketMessage { Id = Guid.NewGuid(), MessageText = "Need altitude advice", SenderName = "Yoni", SenderRole = "Customer" }
            }
        };
        var ticketB = new SupportTicket
        {
            Id = Guid.NewGuid(),
            CustomerId = customerB.Id,
            Subject = "Question B",
            Priority = SupportTicketPriority.Medium,
            Status = SupportTicketStatus.Open,
            Messages = new List<SupportTicketMessage>
            {
                new SupportTicketMessage { Id = Guid.NewGuid(), MessageText = "Dietary request", SenderName = "Tamar", SenderRole = "Customer" }
            }
        };
        await context.SupportTickets.AddRangeAsync(ticketA, ticketB);

        await context.SaveChangesAsync();

        var controller = new MeController(
            context,
            new TripService(context),
            new BookingService(context),
            new DocumentService(context),
            new PaymentService(context),
            new NotificationService(context),
            NullLogger<MeController>.Instance);

        // Act as Customer A
        SetCustomerClaims(controller, authUserA, customerA.Email);

        var tripsResult = await controller.GetMyTrips();
        var bookingsResult = await controller.GetMyBookings();
        var documentsResult = await controller.GetMyDocuments();
        var paymentsResult = await controller.GetMyPayments(CancellationToken.None);
        var supportResult = await controller.GetMySupportTickets(CancellationToken.None);

        // Assert: Customer A sees ONLY Customer A's records
        var tripsOk = Assert.IsType<OkObjectResult>(tripsResult);
        var tripsResponse = Assert.IsType<ApiResponse<List<TripDto>>>(tripsOk.Value);
        Assert.NotNull(tripsResponse.Data);
        Assert.Single(tripsResponse.Data);
        Assert.Equal("TRIP-A", tripsResponse.Data[0].TripCode);

        var bookingsOk = Assert.IsType<OkObjectResult>(bookingsResult);
        var bookingsResponse = Assert.IsType<ApiResponse<List<BookingDto>>>(bookingsOk.Value);
        Assert.NotNull(bookingsResponse.Data);
        Assert.Single(bookingsResponse.Data);
        Assert.Equal("BK-A", bookingsResponse.Data[0].BookingCode);

        var docsOk = Assert.IsType<OkObjectResult>(documentsResult);
        var docsResponse = Assert.IsType<ApiResponse<List<DocumentDto>>>(docsOk.Value);
        Assert.NotNull(docsResponse.Data);
        Assert.Single(docsResponse.Data);
        Assert.Equal("Yoni Passport", docsResponse.Data[0].Name);

        var paysOk = Assert.IsType<OkObjectResult>(paymentsResult);
        var paysResponse = Assert.IsType<ApiResponse<List<PaymentDto>>>(paysOk.Value);
        Assert.NotNull(paysResponse.Data);
        Assert.Single(paysResponse.Data);
        Assert.Equal(500, paysResponse.Data[0].Amount);

        var supportOk = Assert.IsType<OkObjectResult>(supportResult);
        var supportResponse = Assert.IsType<ApiResponse<List<SupportTicketSummaryDto>>>(supportOk.Value);
        Assert.NotNull(supportResponse.Data);
        Assert.Single(supportResponse.Data);
        Assert.Equal("Question A", supportResponse.Data[0].Subject);
    }

    [Fact]
    public async Task MeController_UnknownIdentity_ReturnsNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var controller = new MeController(
            context,
            new TripService(context),
            new BookingService(context),
            new DocumentService(context),
            new PaymentService(context),
            new NotificationService(context),
            NullLogger<MeController>.Instance);

        SetCustomerClaims(controller, "non-existent-auth-id", "ghost@example.com");

        // Act
        var result = await controller.GetProfile(CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CustomerProfileDto>>(notFound.Value);
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains("not found", response.Message);
    }

    [Fact]
    public async Task AuthController_SyncProfile_CreatesNewCustomerWhenNotExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "TomerGroupTestSecretKey2026!MustBeAtLeast32Chars" },
                { "Jwt:Issuer", "TomerGroupApi" },
                { "Jwt:Audience", "TomerGroupMobile" }
            })
            .Build();

        var hasher = new PasswordHasher();
        var jwt = new JwtTokenService(config);
        var audit = new AuditService(context);
        var authService = new AuthenticationService(context, hasher, jwt, audit);
        var controller = new AuthController(authService);

        var authUserId = Guid.NewGuid().ToString();
        var request = new CreateCustomerProfileRequestDto
        {
            AuthUserId = authUserId,
            Email = "newtraveler@example.com",
            FirstName = "Gal",
            LastName = "Gadot",
            Phone = "+972520001122",
            Language = "he"
        };

        SetCustomerClaims(controller, authUserId, request.Email);

        // Act
        var result = await controller.SyncProfile(request, context, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CustomerProfileDto>>(okResult.Value);
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Gal", response.Data.FirstName);
        Assert.Equal("Gadot", response.Data.LastName);
        Assert.Equal(authUserId, response.Data.AuthUserId);

        // Verify entity persisted in DB
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.AuthUserId == authUserId);
        Assert.NotNull(customer);
        Assert.Equal("newtraveler@example.com", customer.Email);
    }

    [Fact]
    public async Task HealthController_LiveAndReadyProbes_ReturnHealthy()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var brandingService = new BrandingService(context);
        var config = new ConfigurationBuilder().Build();
        var controller = new HealthController(context, brandingService, config);

        // Act
        var liveResult = controller.GetLiveness();
        var readyResult = await controller.GetReadiness();

        // Assert
        var liveOk = Assert.IsType<OkObjectResult>(liveResult);
        var liveResponse = Assert.IsType<ApiResponse<LivenessStatusDto>>(liveOk.Value);
        Assert.NotNull(liveResponse);
        Assert.True(liveResponse.Success);
        Assert.NotNull(liveResponse.Data);
        Assert.Equal("Live", liveResponse.Data.Status);

        var readyOk = Assert.IsType<OkObjectResult>(readyResult);
        var readyResponse = Assert.IsType<ApiResponse<ReadinessStatusDto>>(readyOk.Value);
        Assert.NotNull(readyResponse);
        Assert.True(readyResponse.Success);
        Assert.NotNull(readyResponse.Data);
        Assert.Equal("Ready", readyResponse.Data.Status);
    }

    [Fact]
    public async Task SupabaseStorageService_GeneratesCorrectPublicAndSignedUrls()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "SUPABASE_URL", "https://xyztestproject.supabase.co" },
                { "SUPABASE_KEY", "anon-key-test" },
                { "SUPABASE_SERVICE_ROLE_KEY", "service-role-key-test" }
            })
            .Build();

        using var httpClient = new HttpClient();
        var storageService = new SupabaseStorageService(httpClient, config, NullLogger<SupabaseStorageService>.Instance);

        // Act
        var publicUrl = storageService.GetPublicUrl("trek-media", "machu_picchu.jpg");
        var signedUrlResult = await storageService.GetSignedUrlAsync("customer-documents", "passport_123.pdf", 3600);

        // Assert
        Assert.Equal("https://xyztestproject.supabase.co/storage/v1/object/public/trek-media/machu_picchu.jpg", publicUrl);
        Assert.NotNull(signedUrlResult);
    }

    [Fact]
    public void ApiConfiguration_ZeroLocalhost_EnforcedInProduction()
    {
        // Act
        var prodUrl = ApiConfiguration.DefaultRenderProductionUrl;
        var config = new ApiConfiguration();
        var baseAddress = config.BaseAddress.ToString();

        // Assert
        Assert.StartsWith("https://", prodUrl);
        Assert.DoesNotContain("localhost", prodUrl);
        Assert.DoesNotContain("127.0.0.1", prodUrl);
        Assert.DoesNotContain("10.0.2.2", prodUrl);

        Assert.StartsWith("https://", baseAddress);
        Assert.DoesNotContain("localhost", baseAddress);
        Assert.DoesNotContain("127.0.0.1", baseAddress);
        Assert.DoesNotContain("10.0.2.2", baseAddress);
    }
}
