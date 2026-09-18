using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Api.Controllers;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class AdminOperationsPlatformTests
{
    private TomerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TomerDbContext(options);
    }

    private static void SetAdminUserContext(ControllerBase controller, Guid? userId = null, string email = "admin@tomergroup.com")
    {
        var id = userId ?? Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task OperationsDashboard_GetV2_ReturnsFiveOperationalZones()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new DashboardController(context);
        SetAdminUserContext(controller);

        var today = DateTime.UtcNow.Date;
        var cust = new Customer { FirstName = "David", LastName = "Cohen", Email = "david@cohen.il", Country = "Israel" };
        await context.Customers.AddAsync(cust);

        var transport = new Transportation
        {
            CustomerId = cust.Id,
            ServiceType = "Airport Transfer",
            PickupLocation = "CUZ Airport",
            DropoffLocation = "Hotel Monasterio Cusco",
            ScheduledPickupTime = today.AddHours(14),
            Status = TransportationStatus.Assigned
        };
        await context.Transportations.AddAsync(transport);

        var ticket = new SupportTicket
        {
            CustomerId = cust.Id,
            Subject = "Acclimatization Oxygen Request",
            Category = SupportCategory.HealthAndAltitude,
            Priority = SupportTicketPriority.Urgent,
            Status = SupportTicketStatus.New
        };
        await context.SupportTickets.AddAsync(ticket);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetOperationsDashboardV2();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var res = Assert.IsType<ApiResponse<AdminDashboardV2Dto>>(okResult.Value);

        Assert.True(res.Success);
        Assert.NotNull(res.Data);
        Assert.Single(res.Data.TodaySchedule);
        Assert.Equal("CUZ Airport", res.Data.TodaySchedule[0].Location);
        Assert.Contains(res.Data.CriticalAlerts, a => a.AlertType == "UrgentSupport");
    }

    [Fact]
    public async Task AdminSearch_MultipleEntities_ReturnsStructuredResults()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new SearchController(context);

        var cust = new Customer { FirstName = "Noam", LastName = "Levy", Email = "noam@levy.il", Country = "Israel" };
        await context.Customers.AddAsync(cust);

        var trek = new TrekRouteEntity
        {
            Name = "Salkantay Trek Flagship",
            Region = "Cusco, Mollepata",
            DurationDays = 5,
            DistanceKm = 74.0,
            IsPublished = true
        };
        await context.TrekRoutes.AddAsync(trek);
        await context.SaveChangesAsync();

        // Act - Search for "Salkantay"
        var resultTrek = await controller.Search("Salkantay");
        var okTrek = Assert.IsType<OkObjectResult>(resultTrek.Result);
        var resTrek = Assert.IsType<ApiResponse<AdminSearchResultDto>>(okTrek.Value);

        Assert.True(resTrek.Success);
        Assert.Single(resTrek.Data!.Treks);
        Assert.Equal("Salkantay Trek Flagship", resTrek.Data.Treks[0].Title);

        // Act - Search for "Levy"
        var resultCust = await controller.Search("Levy");
        var okCust = Assert.IsType<OkObjectResult>(resultCust.Result);
        var resCust = Assert.IsType<ApiResponse<AdminSearchResultDto>>(okCust.Value);

        Assert.True(resCust.Success);
        Assert.Single(resCust.Data!.Customers);
        Assert.Contains("Noam Levy", resCust.Data.Customers[0].Title);
    }

    [Fact]
    public async Task Customer360_ValidCustomerId_ReturnsCompleteProfile()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var customerService = new CustomerService(context);
        var controller = new CustomersController(customerService, context);
        SetAdminUserContext(controller);

        var cust = new Customer
        {
            FirstName = "Yael",
            LastName = "Barak",
            Email = "yael@barak.il",
            Phone = "+972 50 1234567",
            Country = "Israel",
            PassportNumber = "IL-98765432"
        };
        await context.Customers.AddAsync(cust);

        var trip = new Trip
        {
            CustomerId = cust.Id,
            Title = "Inca Trail & Sacred Valley",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(7),
            Status = TripStatus.Confirmed
        };
        await context.Trips.AddAsync(trip);

        var booking = new Booking
        {
            CustomerId = cust.Id,
            TripId = trip.Id,
            BookingCode = "TG-2026-TEST",
            StartDate = DateTime.UtcNow,
            TotalAmount = 2500,
            PaidAmount = 1000,
            Status = BookingStatus.Confirmed
        };
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetCustomer360(cust.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var res = Assert.IsType<ApiResponse<Customer360Dto>>(okResult.Value);

        Assert.True(res.Success);
        Assert.NotNull(res.Data);
        Assert.Equal("Yael Barak", res.Data.FullName);
        Assert.Equal(1, res.Data.TotalTrips);
        Assert.Equal(1, res.Data.TotalBookings);
        Assert.Equal(1500, res.Data.PendingBalance);
        Assert.Single(res.Data.Trips);
        Assert.Single(res.Data.Bookings);
    }

    [Fact]
    public void TreksController_ImportGpx_ParsesTrackAndCalculatesElevation()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new TreksController(context);
        SetAdminUserContext(controller);

        var gpxXml = @"<?xml version='1.0' encoding='UTF-8'?>
<gpx version='1.1' creator='TomerGroup' xmlns='http://www.topografix.com/GPX/1/1'>
  <trk>
    <name>Humantay Lake Alpine Route</name>
    <trkseg>
      <trkpt lat='-13.3855' lon='-72.5694'><ele>3900</ele></trkpt>
      <trkpt lat='-13.3890' lon='-72.5710'><ele>4050</ele></trkpt>
      <trkpt lat='-13.3930' lon='-72.5730'><ele>4200</ele></trkpt>
    </trkseg>
  </trk>
  <wpt lat='-13.3930' lon='-72.5730'>
    <name>Laguna Humantay Viewpoint</name>
    <ele>4200</ele>
  </wpt>
</gpx>";

        // Act
        var result = controller.ImportGpx(gpxXml);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var res = Assert.IsType<ApiResponse<GpxImportResultDto>>(okResult.Value);

        Assert.True(res.Success);
        Assert.NotNull(res.Data);
        Assert.Equal("Humantay Lake Alpine Route", res.Data.TrackName);
        Assert.Equal(3, res.Data.CoordinatePointsCount);
        Assert.Equal(1, res.Data.WaypointsCount);
        Assert.Equal(4200, res.Data.MaxElevationMeters);
        Assert.Equal(3900, res.Data.MinElevationMeters);
        Assert.Equal(300, res.Data.TotalAscentMeters);
        Assert.True(res.Data.TotalDistanceKm > 0);
    }

    [Fact]
    public async Task TreksController_PublishAndUnpublish_TogglesFlagAndAudits()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new TreksController(context);
        var adminId = Guid.NewGuid();
        SetAdminUserContext(controller, adminId);

        var trek = new TrekRouteEntity
        {
            Name = "Choquequirao Traverse",
            Region = "Apurímac Canyon",
            IsPublished = false
        };
        await context.TrekRoutes.AddAsync(trek);
        await context.SaveChangesAsync();

        // Act 1: Publish
        var pubResult = await controller.PublishTrek(trek.Id);
        var pubOk = Assert.IsType<OkObjectResult>(pubResult.Result);
        var pubRes = Assert.IsType<ApiResponse<TrekRouteEntity>>(pubOk.Value);

        Assert.True(pubRes.Data!.IsPublished);
        Assert.NotNull(pubRes.Data.PublishedAt);
        Assert.Equal(adminId, pubRes.Data.PublishedByUserId);

        // Act 2: Unpublish
        var unpubResult = await controller.UnpublishTrek(trek.Id);
        var unpubOk = Assert.IsType<OkObjectResult>(unpubResult.Result);
        var unpubRes = Assert.IsType<ApiResponse<TrekRouteEntity>>(unpubOk.Value);

        Assert.False(unpubRes.Data!.IsPublished);

        // Verify audit logs
        var auditLogs = await context.AuditLogs.Where(a => a.TargetEntityId == trek.Id).ToListAsync();
        Assert.Equal(2, auditLogs.Count);
    }

    [Fact]
    public async Task CalendarController_GetEvents_AggregatesTransfersAndActivities()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new CalendarController(context);

        var today = DateTime.UtcNow.Date;
        var cust = new Customer { FirstName = "Gal", LastName = "Gadot", Email = "gal@gadot.il" };
        await context.Customers.AddAsync(cust);

        var driver = new Driver { FullName = "Carlos Mendoza", Phone = "+51 984 112233" };
        await context.Drivers.AddAsync(driver);

        var transfer = new Transportation
        {
            CustomerId = cust.Id,
            DriverId = driver.Id,
            ServiceType = "Private Airport Transfer",
            PickupLocation = "Alejandro Velasco Astete Airport",
            DropoffLocation = "Belmond Hotel Monasterio",
            ScheduledPickupTime = today.AddHours(10),
            Status = TransportationStatus.Assigned
        };
        await context.Transportations.AddAsync(transfer);

        var trip = new Trip { CustomerId = cust.Id, Title = "Peru Luxury Experience", StartDate = today, EndDate = today.AddDays(7) };
        await context.Trips.AddAsync(trip);

        var day = new TripDay { TripId = trip.Id, DayNumber = 1, Date = today, Destination = "Cusco Historic Center" };
        await context.TripDays.AddAsync(day);

        var activity = new Activity
        {
            TripDayId = day.Id,
            Title = "Private City & Qorikancha Guided Tour",
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Location = "Qorikancha Temple"
        };
        await context.Activities.AddAsync(activity);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetCalendarEvents(today.AddDays(-1), today.AddDays(2), "all");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var res = Assert.IsType<ApiResponse<List<OperationalCalendarEventDto>>>(okResult.Value);

        Assert.True(res.Success);
        Assert.NotNull(res.Data);
        Assert.Equal(2, res.Data.Count);
        Assert.Contains(res.Data, e => e.EventType == "Pickup");
        Assert.Contains(res.Data, e => e.EventType == "Activity");
    }

    [Fact]
    public async Task SupportController_CreateReplyAndStatusUpdate_WorksProperly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var controller = new SupportController(context);
        SetAdminUserContext(controller, email: "ops@tomergroup.com");

        var cust = new Customer { FirstName = "Eitan", LastName = "Rabin", Email = "eitan@rabin.il" };
        await context.Customers.AddAsync(cust);

        var createResult = await controller.CreateTicket(new CreateSupportTicketDto
        {
            CustomerId = cust.Id,
            Subject = "Dietary request for Inca Trail",
            Category = SupportCategory.TripInquiry,
            Priority = SupportTicketPriority.Medium,
            InitialMessage = "Is Glatt Kosher catering available for Salkantay Day 2?"
        });

        var createOk = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        var createdTicketRes = Assert.IsType<ApiResponse<SupportTicket>>(createOk.Value);
        var ticketId = createdTicketRes.Data!.Id;

        // Act 1: Reply
        var replyResult = await controller.ReplyToTicket(ticketId, new SupportTicketReplyDto
        {
            MessageText = "Yes, we coordinate directly with Chabad Cusco for vacuum-sealed double-wrapped Glatt meals.",
            IsInternalNote = false
        });

        var replyOk = Assert.IsType<OkObjectResult>(replyResult.Result);
        var replyRes = Assert.IsType<ApiResponse<SupportTicketMessage>>(replyOk.Value);
        Assert.True(replyRes.Success);
        Assert.Equal("Staff", replyRes.Data!.SenderRole);

        // Act 2: Resolve
        var statusResult = await controller.UpdateStatus(ticketId, new UpdateSupportTicketStatusDto
        {
            Status = SupportTicketStatus.Resolved,
            ResolutionNotes = "Meal options confirmed with Chabad"
        });

        var statusOk = Assert.IsType<OkObjectResult>(statusResult.Result);
        var statusRes = Assert.IsType<ApiResponse<SupportTicket>>(statusOk.Value);
        Assert.Equal(SupportTicketStatus.Resolved, statusRes.Data!.Status);
        Assert.NotNull(statusRes.Data.ResolvedAt);
    }

    [Fact]
    public async Task BookingsController_AdminUpdateAndRecordPayment_UpdatesBalanceAndAudits()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var bookingService = new BookingService(context);
        var controller = new BookingsController(bookingService, context);
        SetAdminUserContext(controller);

        var cust = new Customer { FirstName = "Shani", LastName = "Bitton", Email = "shani@bitton.il" };
        await context.Customers.AddAsync(cust);

        var booking = new Booking
        {
            BookingCode = "TG-2026-PAY1",
            CustomerId = cust.Id,
            StartDate = DateTime.UtcNow,
            TotalAmount = 2000,
            PaidAmount = 0,
            Status = BookingStatus.Pending,
            PaymentStatus = PaymentStatus.Pending
        };
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // Act: Record payment of $1,000
        var payResult = await controller.RecordPayment(booking.Id, new PaymentItemDto
        {
            Amount = 1000,
            PaymentMethod = "CreditCard",
            TransactionReference = "TX-VISA-8899"
        });

        var payOk = Assert.IsType<OkObjectResult>(payResult);
        var payRes = Assert.IsType<ApiResponse<Payment>>(payOk.Value);

        Assert.True(payRes.Success);
        Assert.Equal(1000, payRes.Data!.Amount);

        // Reload booking
        var updatedBooking = await context.Bookings.FindAsync(booking.Id);
        Assert.NotNull(updatedBooking);
        Assert.Equal(1000, updatedBooking.PaidAmount);
        Assert.Equal(PaymentStatus.Partial, updatedBooking.PaymentStatus);

        // Verify audit log
        var log = await context.AuditLogs.FirstOrDefaultAsync(a => a.TargetEntityId == booking.Id && a.Action == "RecordPayment");
        Assert.NotNull(log);
        Assert.Contains("TX-VISA-8899", log.Description);
    }
}
