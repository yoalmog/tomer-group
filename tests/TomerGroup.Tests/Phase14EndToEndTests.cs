using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase14EndToEndTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task Scenario1_CompleteCommercialTravelerLifecycle_PassesAllStages()
    {
        var context = CreateInMemoryContext();

        // 1. Service Orchestration
        var customerService = new CustomerService(context);
        var tripService = new TripService(context);
        var bookingService = new BookingService(context);
        var paymentService = new PaymentService(context);
        var expenseService = new ExpenseService(context);
        var documentService = new DocumentService(context);
        var syncService = new SyncService(context);
        var reportService = new ReportService(context);
        var sanitizerService = new SecuritySanitizerService();

        var agencyStaffUserId = Guid.NewGuid();
        var travelerUserId = Guid.NewGuid();

        // 2. Traveler Onboarding (Israeli traveler)
        var createCustomer = new CreateCustomerDto
        {
            FirstName = "Yossi",
            LastName = "Cohen",
            HebrewName = "יוסי כהן",
            PassportName = "YOSSI COHEN",
            Phone = "054-9876543",
            WhatsApp = "+972549876543",
            Email = "yossi.cohen@gmail.com",
            Country = "Israel",
            PassportNumber = "21984729",
            PassportExpiration = DateTime.UtcNow.AddYears(4),
            DateOfBirth = new DateTime(1995, 7, 12, 0, 0, 0, DateTimeKind.Utc),
            IsraelIdNumber = "319284721",
            MedicalNotes = "Mild asthma, carrying rescue inhaler",
            EmergencyContactName = "Rivka Cohen (Mother)",
            EmergencyContactPhone = "+972501234567",
            DietaryPreferences = "Strictly Kosher (Chabad)",
            IsActiveInPeru = true
        };

        var customerResult = await customerService.CreateAsync(createCustomer, agencyStaffUserId);
        Assert.True(customerResult.Success, customerResult.Message);
        var customerId = customerResult.Data!.Id;

        // Associate user ID to customer for isolation testing
        var customerEntity = await context.Customers.FindAsync(customerId);
        customerEntity!.UserId = travelerUserId;
        await context.SaveChangesAsync();

        // Verify PII masking works on customer identity
        var maskedPassport = sanitizerService.MaskPassport(createCustomer.PassportNumber);
        Assert.Equal("****4729", maskedPassport);

        // 3. Trip & Itinerary Construction (3 Days: Cusco -> Sacred Valley -> Machu Picchu)
        var createTrip = new CreateTripDto
        {
            Title = "Cusco, Sacred Valley & Machu Picchu Express",
            Description = "VIP 3-day private package for Israeli travelers",
            CustomerId = customerId,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(9),
            TotalRevenue = 1200.00m,
            Currency = Currency.USD
        };

        var tripResult = await tripService.CreateAsync(createTrip);
        Assert.True(tripResult.Success, tripResult.Message);
        var tripId = tripResult.Data!.Id;

        // Add Trip Days
        var day1Result = await tripService.AddTripDayAsync(tripId, new CreateTripDayDto
        {
            TripId = tripId,
            DayNumber = 1,
            Date = createTrip.StartDate,
            Title = "Sacred Valley Acclimatization & Pisac",
            Destination = "Sacred Valley"
        }, agencyStaffUserId);
        Assert.True(day1Result.Success);

        var day2Result = await tripService.AddTripDayAsync(tripId, new CreateTripDayDto
        {
            TripId = tripId,
            DayNumber = 2,
            Date = createTrip.StartDate.AddDays(1),
            Title = "Ollantaytambo & Panoramic Train to Aguas Calientes",
            Destination = "Aguas Calientes"
        }, agencyStaffUserId);
        Assert.True(day2Result.Success);

        var day3Result = await tripService.AddTripDayAsync(tripId, new CreateTripDayDto
        {
            TripId = tripId,
            DayNumber = 3,
            Date = createTrip.StartDate.AddDays(2),
            Title = "Machu Picchu Circuit 2 Sunrise Tour & Return",
            Destination = "Machu Picchu"
        }, agencyStaffUserId);
        Assert.True(day3Result.Success);

        // Add Activity on Day 3
        var activityResult = await tripService.AddActivityAsync(day3Result.Data!.Id, new CreateActivityDto
        {
            TripDayId = day3Result.Data!.Id,
            Title = "Guided Tour Machu Picchu Citadel",
            Description = "Hebrew-speaking licensed guide entrance 08:00 AM",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(11, 30, 0),
            Location = "Machu Picchu Sanctuary Gate",
            Latitude = -13.1631,
            Longitude = -72.5450,
            Status = ActivityStatus.Scheduled
        }, agencyStaffUserId);
        Assert.True(activityResult.Success);
        var activityId = activityResult.Data!.Id;

        // 4. Booking Creation
        var createBooking = new CreateBookingDto
        {
            CustomerId = customerId,
            TripId = tripId,
            StartDate = createTrip.StartDate,
            EndDate = createTrip.EndDate,
            TotalAmount = 1200.00m,
            Currency = Currency.USD,
            Notes = "VIP package with Kosher lunch packs"
        };
        var bookingResult = await bookingService.CreateAsync(createBooking);
        Assert.True(bookingResult.Success, bookingResult.Message);
        var bookingId = bookingResult.Data!.Id;

        // 5. Payments & Multi-Currency Settlement
        // Payment 1: Deposit $400 USD
        var p1Result = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            BookingId = bookingId,
            Amount = 400.00m,
            Currency = Currency.USD,
            ExchangeRateToUsd = 1.0m,
            Method = PaymentMethod.BankTransfer,
            ReferenceNumber = "TX-IL-98124",
            Notes = "Deposit 33%"
        }, agencyStaffUserId);
        Assert.True(p1Result.Success);

        // Payment 2: Balance 3,040 PEN (exchange rate ~0.263158 -> $800 USD)
        var p2Result = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            BookingId = bookingId,
            Amount = 3040.00m,
            Currency = Currency.PEN,
            ExchangeRateToUsd = 0.26315789m,
            Method = PaymentMethod.CreditCard,
            ReferenceNumber = "VISA-AUTH-7731",
            Notes = "Remaining balance in Soles"
        }, agencyStaffUserId);
        Assert.True(p2Result.Success);

        // Verify booking is fully paid
        var bookingCheck = await bookingService.GetByIdAsync(bookingId, agencyStaffUserId, "Admin");
        Assert.True(bookingCheck.Success);
        Assert.Equal(PaymentStatus.Paid, bookingCheck.Data!.PaymentStatus);
        Assert.True(bookingCheck.Data.PaidAmount >= 1199.99m);

        // 6. Direct Operating Expenses
        // Expense A: Hotel Aguas Calientes ($250 USD)
        await expenseService.RecordExpenseAsync(new RecordExpenseDto
        {
            TripId = tripId,
            BookingId = bookingId,
            Category = "Hotel",
            Description = "Tierra Viva Machu Picchu Hotel (1 night)",
            Amount = 250.00m,
            Currency = Currency.USD,
            ExchangeRateToUsd = 1.0m,
            SupplierName = "Tierra Viva Hotels",
            Date = createTrip.StartDate.AddDays(1)
        }, agencyStaffUserId);

        // Expense B: Panoramic Train ($180 USD)
        await expenseService.RecordExpenseAsync(new RecordExpenseDto
        {
            TripId = tripId,
            BookingId = bookingId,
            Category = "Transport",
            Description = "Inca Rail 360 RT Ollanta-Aguas Calientes",
            Amount = 180.00m,
            Currency = Currency.USD,
            ExchangeRateToUsd = 1.0m,
            SupplierName = "Inca Rail",
            Date = createTrip.StartDate.AddDays(1)
        }, agencyStaffUserId);

        // Expense C: Machu Picchu Permit (304 PEN = $80 USD at 0.263158)
        await expenseService.RecordExpenseAsync(new RecordExpenseDto
        {
            TripId = tripId,
            BookingId = bookingId,
            Category = "Tickets",
            Description = "Ministerio de Cultura Machu Picchu Circuit 2 Permit",
            Amount = 304.00m,
            Currency = Currency.PEN,
            ExchangeRateToUsd = 0.26315789m,
            SupplierName = "Ministerio de Cultura del Peru",
            Date = createTrip.StartDate.AddDays(2)
        }, agencyStaffUserId);

        // Verify Trip Profitability
        var profitability = await expenseService.GetTripProfitabilityAsync(tripId);
        Assert.True(profitability.Success);
        Assert.Equal(1200.00m, profitability.Data!.TotalRevenue);
        Assert.True(profitability.Data.TotalCost >= 509.9m && profitability.Data.TotalCost <= 510.1m);
        Assert.True(profitability.Data.GrossProfit >= 689.9m && profitability.Data.GrossProfit <= 690.1m);

        // 7. Voucher & Document Generation
        var voucherResult = await documentService.UploadAsync(new UploadDocumentDto
        {
            CustomerId = customerId,
            TripId = tripId,
            BookingId = bookingId,
            Name = "Machu Picchu Circuit 2 Official Permit",
            HebrewName = "אישור כניסה רשמי למאצ'ו פיצ'ו",
            Type = DocumentType.MachuPicchuPermit,
            Circuit = "Circuit 2 Classic",
            PermitPassportNumber = "21984729",
            FileExtension = "pdf",
            FileBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x31 } // %PDF1
        }, agencyStaffUserId);
        Assert.True(voucherResult.Success);

        // 8. Offline Delta Sync (Customer Pull)
        var pullResult = await syncService.PullDeltaPackageAsync(new SyncPullRequestDto
        {
            CustomerId = customerId,
            LastSyncTimestamp = null,
            DeviceId = "Galaxy-S24-Traveler"
        }, travelerUserId, "Customer");

        Assert.True(pullResult.Success);
        Assert.NotNull(pullResult.Data);
        Assert.NotNull(pullResult.Data.Customer);
        Assert.Equal("יוסי כהן", pullResult.Data.Customer!.HebrewName);
        Assert.Single(pullResult.Data.Trips);
        Assert.Equal(3, pullResult.Data.Trips[0].Days.Count);
        Assert.Single(pullResult.Data.Documents);
        Assert.NotEmpty(pullResult.Data.EmergencyContacts);

        // 9. Offline Client Activity Check-In & Emergency Push
        var pushResult = await syncService.PushClientChangesAsync(new SyncPushRequestDto
        {
            CustomerId = customerId,
            DeviceId = "Galaxy-S24-Traveler",
            ActivityCheckIns = new List<ActivityCheckInDto>
            {
                new()
                {
                    ActivityId = activityId,
                    CheckInTimestamp = DateTime.UtcNow,
                    Latitude = -13.1631,
                    Longitude = -72.5450,
                    Note = "Entered citadel on time with Hebrew guide"
                }
            },
            UpdatedEmergencyContact = new UpdateCustomerEmergencyDto
            {
                EmergencyContactName = "Rivka Cohen (Mother Updated)",
                EmergencyContactPhone = "+972509999999",
                MedicalNotes = "Altitude acclimated well, oxygen not needed"
            }
        }, travelerUserId, "Customer");

        Assert.True(pushResult.Success);
        Assert.Equal(2, pushResult.Data!.AcknowledgedItems);

        // Verify activity status was updated to Completed
        var updatedActivity = await context.Activities.FindAsync(activityId);
        Assert.NotNull(updatedActivity);
        Assert.Equal(ActivityStatus.Completed, updatedActivity!.Status);

        // 10. Executive Analytics Integration
        var analyticsResult = await reportService.GetExecutiveAnalyticsAsync();
        Assert.True(analyticsResult.Success);
        Assert.NotNull(analyticsResult.Data);
        Assert.True(analyticsResult.Data.TotalRevenue >= 1200.00m);
        Assert.True(analyticsResult.Data.TotalExpenses >= 509.90m);
        Assert.True(analyticsResult.Data.NetProfit >= 689.90m);
        Assert.True(analyticsResult.Data.ProfitMarginPercentage > 50.0m);
    }

    [Fact]
    public async Task Scenario2_AltitudeAcclimatizationAndKosherRules_EnforcedCorrectly()
    {
        var context = CreateInMemoryContext();
        var aiService = new AIService(context);
        var staffUserId = Guid.NewGuid();

        // Request 5-day package for kosher traveler arriving in Cusco
        var prompt = new GenerateItineraryPromptDto
        {
            Destination = "Cusco",
            Days = 5,
            TravelerProfile = "Adults",
            IsKosherRequired = true,
            IsShabbatObservant = true,
            AdditionalNotes = "First time in Peru, needs proper acclimatization"
        };

        var draftResult = await aiService.GenerateItineraryDraftAsync(prompt, staffUserId);
        Assert.True(draftResult.Success);
        var draft = draftResult.Data!;

        // 1. Acclimatization rule check
        Assert.True(draft.FollowsAcclimatizationRule, "Itinerary must obey altitude acclimatization");
        Assert.True(draft.HasKosherArrangements, "Must include kosher food arrangements");
        Assert.True(draft.HasShabbatArrangements, "Must include Shabbat arrangements");

        // 2. Day 1 must NOT be high-altitude trekking (Rainbow Mountain >5000m or Humantay Lake >4200m)
        var day1 = draft.Days.First(d => d.DayNumber == 1);
        Assert.True(day1.AltitudeMeters <= 3500, "Day 1 altitude must not exceed 3,500m for proper acclimatization");
        Assert.DoesNotContain("Rainbow", day1.Title, StringComparison.OrdinalIgnoreCase);

        // 3. Human-in-the-loop review workflow
        var requests = await aiService.GetRequestsAsync("Draft");
        Assert.True(requests.Success);
        Assert.NotEmpty(requests.Data!);

        var aiRequestId = requests.Data!.First().Id;
        var approvalResult = await aiService.ApproveRequestAsync(aiRequestId, staffUserId, new ApproveAIRequestDto
        {
            Notes = "Approved with Chabad Cusco Shabbat dinner reservation"
        });
        Assert.True(approvalResult.Success);

        // Verify request transitioned to Approved
        var approvedCheck = await aiService.GetRequestByIdAsync(aiRequestId);
        Assert.True(approvedCheck.Success);
        Assert.Equal("Approved", approvedCheck.Data!.Status);
        Assert.True(approvedCheck.Data.IsApproved);
    }

    [Fact]
    public async Task Scenario3_WhatsAppCustomerDispatch_GeneratesValidHebrewMessageAndDeepLink()
    {
        var context = CreateInMemoryContext();
        var whatsAppService = new WhatsAppService(context);
        var staffUserId = Guid.NewGuid();

        var israeliMobile = "052-8812345";
        var sanitizedPhone = whatsAppService.SanitizePhoneNumber(israeliMobile);
        Assert.Equal("972528812345", sanitizedPhone);

        // Prepare pickup dispatch message
        var dispatchRequest = new SendWhatsAppMessageDto
        {
            Phone = israeliMobile,
            TemplateKey = "DriverPickupReminder",
            Language = "he",
            TemplateParameters = new Dictionary<string, string>
            {
                { "CustomerName", "יוסי" },
                { "DriverName", "Carlos Quispe" },
                { "PickupTime", "09:00" },
                { "PickupLocation", "שער יציאה ראשי, שדה התעופה קוסקו" },
                { "VehicleModel", "Toyota HiAce" },
                { "PlateNumber", "X2B-492" },
                { "DriverPhone", "+51 984 332 110" }
            }
        };

        var dispatchResult = await whatsAppService.PrepareOrSendMessageAsync(dispatchRequest, staffUserId);
        Assert.True(dispatchResult.Success);
        Assert.NotNull(dispatchResult.Data);
        Assert.Contains("972528812345", dispatchResult.Data.WhatsAppUrl);
        Assert.StartsWith("https://wa.me/972528812345?text=", dispatchResult.Data.WhatsAppUrl);

        // Verify message was logged to history
        var history = await whatsAppService.GetMessageHistoryAsync();
        Assert.True(history.Success);
        Assert.NotEmpty(history.Data!);
        var lastMsg = history.Data!.First();
        Assert.Equal("972528812345", lastMsg.RecipientPhone);
        Assert.Contains("Carlos Quispe", lastMsg.Content);
    }

    [Fact]
    public async Task Scenario4_TravelerDataIsolation_BlocksCrossCustomerAccess()
    {
        var context = CreateInMemoryContext();
        var customerService = new CustomerService(context);
        var bookingService = new BookingService(context);
        var paymentService = new PaymentService(context);
        var documentService = new DocumentService(context);

        var travelerA_UserId = Guid.NewGuid();
        var travelerB_UserId = Guid.NewGuid();

        // Customer A
        var customerA = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Avi",
            LastName = "Levi",
            HebrewName = "אבי לוי",
            Email = "avi@levi.co.il",
            UserId = travelerA_UserId,
            Phone = "+972541111111"
        };
        // Customer B
        var customerB = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Dana",
            LastName = "Shahar",
            HebrewName = "דנה שחר",
            Email = "dana@shahar.co.il",
            UserId = travelerB_UserId,
            Phone = "+972542222222"
        };
        context.Customers.AddRange(customerA, customerB);

        // Booking for Customer A
        var bookingA = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "BKG-2026-0001",
            CustomerId = customerA.Id,
            Customer = customerA,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(10),
            TotalAmount = 2500m,
            Currency = Currency.USD,
            Status = BookingStatus.Confirmed,
            PaymentStatus = PaymentStatus.Partial
        };
        context.Bookings.Add(bookingA);

        // Document for Customer A
        var docA = new Document
        {
            Id = Guid.NewGuid(),
            CustomerId = customerA.Id,
            BookingId = bookingA.Id,
            Name = "Private Inca Trail Permit",
            HebrewName = "אישור שביל האינקה",
            Type = DocumentType.IncaTrailPermit,
            FileExtension = "pdf",
            MimeType = "application/pdf",
            FileSizeBytes = 1024,
            IsCustomerVisible = true
        };
        context.Documents.Add(docA);
        await context.SaveChangesAsync();

        // Traveler B attempts to read Customer A's booking
        var crossBookingAccess = await bookingService.GetByIdAsync(bookingA.Id, travelerB_UserId, "Customer");
        Assert.False(crossBookingAccess.Success, "Cross-customer booking access must be blocked");

        // Traveler B attempts to read Customer A's documents
        var crossDocAccess = await documentService.GetCustomerDocumentsAsync(customerA.Id, travelerB_UserId, "Customer");
        Assert.False(crossDocAccess.Success, "Cross-customer document access must be blocked");

        // Traveler B attempts to read Customer A's payment receipts
        var crossPaymentAccess = await paymentService.GetCustomerPaymentsAsync(customerA.Id, travelerB_UserId, "Customer");
        Assert.False(crossPaymentAccess.Success, "Cross-customer payment access must be blocked");

        // Admin has full authorized access
        var adminBookingAccess = await bookingService.GetByIdAsync(bookingA.Id, Guid.NewGuid(), "Admin");
        Assert.True(adminBookingAccess.Success, "Admin must have full access");
    }
}
