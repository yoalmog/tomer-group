using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase5ToursHotelsTransportationTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_Phase5_{Guid.NewGuid()}")
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task TourCatalog_PersistsBilingualAttributes_AndKosherCertification()
    {
        using var context = CreateInMemoryContext();
        var tourService = new TourService(context);

        var request = new CreateTourDto
        {
            Name = "Salkantay Trek to Machu Picchu",
            HebrewName = "טרק סלקנטאי למאצ'ו פיצ'ו 5 ימים",
            Description = "Spectacular 5-day alpine trek crossing 4,630m pass.",
            HebrewDescription = "טרק אלפיני מרהיב החוצה מעבר הרים בגובה 4,630 מ'.",
            Destination = "Salkantay",
            Category = TourCategory.Trek,
            Duration = "5 Days / 4 Nights",
            DurationDays = 5,
            Difficulty = "Challenging",
            MaxCapacity = 12,
            AltitudeMaxMeters = 4630,
            RequiresAcclimatization = true,
            AdultPrice = 650,
            ChildPrice = 550,
            PrivatePrice = 1100,
            AgencyCost = 420,
            Currency = Currency.USD,
            KosherFoodAvailable = true,
            KosherCertificationDetails = "Dedicated kosher cookware and certified trail food from Chabad Cusco",
            BookingCutoffHours = 120
        };

        var response = await tourService.CreateAsync(request);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("טרק סלקנטאי למאצ'ו פיצ'ו 5 ימים", response.Data.HebrewName);
        Assert.Equal(TourCategory.Trek, response.Data.Category);
        Assert.Equal(4630, response.Data.AltitudeMaxMeters);
        Assert.True(response.Data.RequiresAcclimatization);
        Assert.True(response.Data.KosherFoodAvailable);
        Assert.Contains("Chabad Cusco", response.Data.KosherCertificationDetails);
    }

    [Fact]
    public async Task HotelManagement_VerifiesOxygenAndShabbatFriendliness()
    {
        using var context = CreateInMemoryContext();
        var hotelService = new HotelService(context);

        var request = new CreateHotelDto
        {
            Name = "Palacio del Inka, A Luxury Collection Hotel",
            HebrewName = "פלאסיו דל אינקה - קוסקו",
            Address = "Plazoleta Santo Domingo 259, Cusco",
            Destination = "Cusco",
            Stars = 5,
            Phone = "+51 84 231961",
            WhatsApp = "+51 984 231961",
            Email = "concierge@palaciodelinka.com",
            HasOxygenEnrichedRooms = true,
            HasOxygenConcentrators = true,
            HasHeating = true,
            IsKosherFriendly = true,
            ShabbatFriendly = true,
            WalkingDistanceToChabadCusco = true,
            RoomTypes = new() { "Classic Room", "Deluxe Oxygen-Enriched", "Colonial Suite" }
        };

        var response = await hotelService.CreateAsync(request);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(5, response.Data.Stars);
        Assert.True(response.Data.HasOxygenEnrichedRooms);
        Assert.True(response.Data.ShabbatFriendly);
        Assert.True(response.Data.WalkingDistanceToChabadCusco);
        Assert.Contains("Deluxe Oxygen-Enriched", response.Data.RoomTypes);
    }

    [Fact]
    public async Task HotelBooking_GeneratesConfirmationNumber_AndEnforcesCustomerIsolation()
    {
        using var context = CreateInMemoryContext();
        var hotelService = new HotelService(context);

        // Setup Customer A and Customer B
        var userA = new User { Id = Guid.NewGuid(), Email = "customerA@gmail.com" };
        var customerA = new Customer { Id = Guid.NewGuid(), UserId = userA.Id, FirstName = "Danny", LastName = "Cohen" };
        var userB = new User { Id = Guid.NewGuid(), Email = "customerB@gmail.com" };
        var customerB = new Customer { Id = Guid.NewGuid(), UserId = userB.Id, FirstName = "Yossi", LastName = "Levi" };
        var hotel = new Hotel { Id = Guid.NewGuid(), Name = "Palacio del Inka", Destination = "Cusco", Stars = 5 };

        await context.Users.AddRangeAsync(userA, userB);
        await context.Customers.AddRangeAsync(customerA, customerB);
        await context.Hotels.AddAsync(hotel);
        await context.SaveChangesAsync();

        // Customer A creates a hotel booking
        var bookingRequest = new CreateHotelBookingDto
        {
            CustomerId = customerA.Id,
            HotelId = hotel.Id,
            RoomType = "Deluxe Oxygen-Enriched",
            CheckInDate = DateTime.UtcNow.AddDays(3),
            CheckOutDate = DateTime.UtcNow.AddDays(7),
            NumberOfGuests = 2,
            RoomCount = 1,
            OxygenRoomRequested = true,
            SellingPrice = 1200,
            AgencyCost = 850,
            SpecialRequests = "Mechanical key for Shabbat observance"
        };

        var bookingResponse = await hotelService.CreateHotelBookingAsync(bookingRequest, userA.Id);

        Assert.True(bookingResponse.Success);
        Assert.NotNull(bookingResponse.Data);
        Assert.StartsWith("HTL-", bookingResponse.Data.ConfirmationNumber);
        Assert.True(bookingResponse.Data.OxygenRoomRequested);

        // Customer A can view their own hotel bookings
        var customerABookings = await hotelService.GetCustomerHotelBookingsAsync(customerA.Id, userA.Id, "Customer");
        Assert.True(customerABookings.Success);
        Assert.Single(customerABookings.Data!);

        // Customer B attempting to access Customer A's hotel bookings is strictly DENIED
        var customerBAttempt = await hotelService.GetCustomerHotelBookingsAsync(customerA.Id, userB.Id, "Customer");
        Assert.False(customerBAttempt.Success);
        Assert.Contains("Access denied", customerBAttempt.Message);
    }

    [Fact]
    public async Task TransportationTransfer_Lifecycle_AssignedToCompleted_WithDriverAndVehicle()
    {
        using var context = CreateInMemoryContext();
        var transportService = new TransportationService(context);

        var driver = new Driver { Id = Guid.NewGuid(), FullName = "Carlos Quispe Mendoza", Phone = "+51 984 555 666", WhatsApp = "+51984555666" };
        var vehicle = new Vehicle { Id = Guid.NewGuid(), Model = "Mercedes-Benz Sprinter", LicensePlate = "X4T-892", PassengerCapacity = 19, LuggageCapacity = 20 };
        var customer = new Customer { Id = Guid.NewGuid(), FirstName = "Danny", LastName = "Cohen" };

        await context.Drivers.AddAsync(driver);
        await context.Vehicles.AddAsync(vehicle);
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var createRequest = new CreateTransportationDto
        {
            ServiceType = "Airport Transfer",
            HebrewServiceType = "איסוף משדה התעופה קוסקו",
            Type = TransportationType.AirportTransfer,
            DriverId = driver.Id,
            VehicleId = vehicle.Id,
            CustomerId = customer.Id,
            PickupLocation = "Alejandro Velasco Astete Airport (CUZ)",
            DropoffLocation = "Palacio del Inka, Cusco",
            ScheduledPickupTime = DateTime.UtcNow.AddDays(1).Date.AddHours(14),
            PassengerCount = 2,
            Airline = "LATAM",
            FlightOrTrainNumber = "LA-2014"
        };

        var response = await transportService.CreateAsync(createRequest);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(TransportationStatus.Assigned, response.Data.Status);
        Assert.Equal("Carlos Quispe Mendoza", response.Data.DriverName);
        Assert.Equal("Mercedes-Benz Sprinter", response.Data.VehicleModel);

        // Update status to OnTheWay
        var statusUpdate1 = await transportService.UpdateStatusAsync(response.Data.Id, new UpdateTransportationStatusDto
        {
            Status = TransportationStatus.OnTheWay,
            Notes = "Driver dispatched to CUZ airport"
        }, Guid.NewGuid());
        Assert.True(statusUpdate1.Success);
        Assert.Equal(TransportationStatus.OnTheWay, statusUpdate1.Data!.Status);

        // Update status to Completed
        var statusUpdate2 = await transportService.UpdateStatusAsync(response.Data.Id, new UpdateTransportationStatusDto
        {
            Status = TransportationStatus.Completed,
            Notes = "Passengers safely arrived at hotel"
        }, Guid.NewGuid());
        Assert.True(statusUpdate2.Success);
        Assert.Equal(TransportationStatus.Completed, statusUpdate2.Data!.Status);

        // Verify audit logs generated
        var auditLogs = await context.AuditLogs.Where(a => a.EntityName == "Transportation").ToListAsync();
        Assert.Equal(2, auditLogs.Count);
    }

    [Fact]
    public async Task TrainAndFlightBooking_StoresOperatorAndSeatServiceDetails()
    {
        using var context = CreateInMemoryContext();
        var transportService = new TransportationService(context);

        var request = new CreateTransportationDto
        {
            ServiceType = "Panoramic Train to Machu Picchu",
            HebrewServiceType = "רכבת פנורמית ויסטדום למאצ'ו פיצ'ו",
            Type = TransportationType.Train,
            PickupLocation = "Ollantaytambo Station",
            DropoffLocation = "Aguas Calientes Station",
            ScheduledPickupTime = DateTime.UtcNow.AddDays(4).Date.AddHours(7).AddMinutes(30),
            TrainCompany = "PeruRail",
            TrainService = "Vistadome",
            TrainNumber = "Train 31",
            TrainStationDeparture = "Ollantaytambo",
            TrainStationArrival = "Aguas Calientes (Machu Picchu Pueblo)",
            PassengerCount = 2,
            Notes = "Window seats requested with panoramic glass roof"
        };

        var response = await transportService.CreateAsync(request);

        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(TransportationType.Train, response.Data.Type);
        Assert.Equal("PeruRail", response.Data.TrainCompany);
        Assert.Equal("Vistadome", response.Data.TrainService);
        Assert.Equal("Train 31", response.Data.TrainNumber);
        Assert.Equal("Ollantaytambo", response.Data.TrainStationDeparture);
    }

    [Fact]
    public async Task DetailedBooking_AggregatesHotelsAndTransfers_Accurately()
    {
        using var context = CreateInMemoryContext();
        var bookingService = new BookingService(context);

        var user = new User { Id = Guid.NewGuid(), Email = "traveler@tomergroup.com" };
        var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, FirstName = "Danny", LastName = "Cohen" };
        var hotel = new Hotel { Id = Guid.NewGuid(), Name = "Palacio del Inka", Destination = "Cusco" };
        var driver = new Driver { Id = Guid.NewGuid(), FullName = "Carlos Quispe", Phone = "+51 984 555 666" };

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            BookingCode = "TG-2026-00999",
            CustomerId = customer.Id,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(10),
            TotalAmount = 3200,
            PaidAmount = 2000,
            Status = BookingStatus.Confirmed,
            PaymentStatus = PaymentStatus.Partial
        };

        var hotelBooking = new HotelBooking
        {
            BookingId = booking.Id,
            CustomerId = customer.Id,
            HotelId = hotel.Id,
            RoomType = "Deluxe Oxygen-Enriched",
            ConfirmationNumber = "HTL-2026-00999",
            CheckInDate = DateTime.UtcNow.AddDays(2),
            CheckOutDate = DateTime.UtcNow.AddDays(6),
            SellingPrice = 1200
        };

        var transfer = new Transportation
        {
            BookingId = booking.Id,
            CustomerId = customer.Id,
            DriverId = driver.Id,
            ServiceType = "Airport Transfer",
            HebrewServiceType = "איסוף משדה התעופה",
            PickupLocation = "CUZ Airport",
            DropoffLocation = "Palacio del Inka",
            ScheduledPickupTime = DateTime.UtcNow.AddDays(2).Date.AddHours(11)
        };

        await context.Users.AddAsync(user);
        await context.Customers.AddAsync(customer);
        await context.Hotels.AddAsync(hotel);
        await context.Drivers.AddAsync(driver);
        await context.Bookings.AddAsync(booking);
        await context.HotelBookings.AddAsync(hotelBooking);
        await context.Transportations.AddAsync(transfer);
        await context.SaveChangesAsync();

        var detailedResponse = await bookingService.GetDetailedByIdAsync(booking.Id, user.Id, "Customer");

        Assert.True(detailedResponse.Success);
        Assert.NotNull(detailedResponse.Data);
        Assert.Equal("TG-2026-00999", detailedResponse.Data.BookingCode);
        Assert.Equal(3200, detailedResponse.Data.TotalAmount);
        Assert.Equal(2000, detailedResponse.Data.PaidAmount);
        Assert.Equal(1200, detailedResponse.Data.OutstandingAmount);
        Assert.Single(detailedResponse.Data.HotelBookings);
        Assert.Equal("Palacio del Inka", detailedResponse.Data.HotelBookings[0].HotelName);
        Assert.Single(detailedResponse.Data.Transportations);
        Assert.Equal("Carlos Quispe", detailedResponse.Data.Transportations[0].DriverName);
    }

    [Fact]
    public async Task CustomerIsolation_CustomerACannotAccessCustomerBTransfers()
    {
        using var context = CreateInMemoryContext();
        var transportService = new TransportationService(context);

        var userA = new User { Id = Guid.NewGuid(), Email = "userA@gmail.com" };
        var customerA = new Customer { Id = Guid.NewGuid(), UserId = userA.Id, FirstName = "CustomerA" };
        var userB = new User { Id = Guid.NewGuid(), Email = "userB@gmail.com" };
        var customerB = new Customer { Id = Guid.NewGuid(), UserId = userB.Id, FirstName = "CustomerB" };

        var transferA = new Transportation
        {
            CustomerId = customerA.Id,
            PickupLocation = "Cusco",
            DropoffLocation = "Ollantaytambo",
            ScheduledPickupTime = DateTime.UtcNow.AddDays(1)
        };

        await context.Users.AddRangeAsync(userA, userB);
        await context.Customers.AddRangeAsync(customerA, customerB);
        await context.Transportations.AddAsync(transferA);
        await context.SaveChangesAsync();

        // User B attempts to access Customer A's transfers
        var result = await transportService.GetCustomerTransfersAsync(customerA.Id, userB.Id, "Customer");

        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message);
    }
}

