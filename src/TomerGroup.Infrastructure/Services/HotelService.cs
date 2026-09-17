using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class HotelService : IHotelService
{
    private readonly TomerDbContext _context;

    public HotelService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<HotelDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var hotels = await _context.Hotels
            .AsNoTracking()
            .Where(h => h.IsActive)
            .OrderBy(h => h.Destination)
            .ThenByDescending(h => h.Stars)
            .Select(h => MapToDto(h))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<HotelDto>>.Ok(hotels);
    }

    public async Task<ApiResponse<HotelDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hotel = await _context.Hotels.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        if (hotel == null)
        {
            return ApiResponse<HotelDto>.Fail("Hotel not found");
        }

        return ApiResponse<HotelDto>.Ok(MapToDto(hotel));
    }

    public async Task<ApiResponse<HotelDto>> CreateAsync(CreateHotelDto request, CancellationToken cancellationToken = default)
    {
        var hotel = new Hotel
        {
            Name = request.Name,
            HebrewName = request.HebrewName,
            Address = request.Address,
            Destination = request.Destination,
            Stars = request.Stars,
            Phone = request.Phone,
            WhatsApp = request.WhatsApp,
            Email = request.Email,
            Website = request.Website,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            HasOxygenEnrichedRooms = request.HasOxygenEnrichedRooms,
            HasOxygenConcentrators = request.HasOxygenConcentrators,
            HasHeating = request.HasHeating,
            IsKosherFriendly = request.IsKosherFriendly,
            ShabbatFriendly = request.ShabbatFriendly,
            WalkingDistanceToChabadCusco = request.WalkingDistanceToChabadCusco,
            RoomTypes = request.RoomTypes,
            PhotoUrls = request.PhotoUrls,
            Notes = request.Notes
        };

        await _context.Hotels.AddAsync(hotel, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<HotelDto>.Ok(MapToDto(hotel), "Hotel created successfully");
    }

    public async Task<ApiResponse<HotelDto>> UpdateAsync(Guid id, UpdateHotelDto request, CancellationToken cancellationToken = default)
    {
        var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        if (hotel == null)
        {
            return ApiResponse<HotelDto>.Fail("Hotel not found");
        }

        hotel.Name = request.Name;
        hotel.HebrewName = request.HebrewName;
        hotel.Address = request.Address;
        hotel.Destination = request.Destination;
        hotel.Stars = request.Stars;
        hotel.Phone = request.Phone;
        hotel.WhatsApp = request.WhatsApp;
        hotel.Email = request.Email;
        hotel.Website = request.Website;
        hotel.Latitude = request.Latitude;
        hotel.Longitude = request.Longitude;
        hotel.HasOxygenEnrichedRooms = request.HasOxygenEnrichedRooms;
        hotel.HasOxygenConcentrators = request.HasOxygenConcentrators;
        hotel.HasHeating = request.HasHeating;
        hotel.IsKosherFriendly = request.IsKosherFriendly;
        hotel.ShabbatFriendly = request.ShabbatFriendly;
        hotel.WalkingDistanceToChabadCusco = request.WalkingDistanceToChabadCusco;
        hotel.RoomTypes = request.RoomTypes;
        hotel.PhotoUrls = request.PhotoUrls;
        hotel.Notes = request.Notes;
        hotel.IsActive = request.IsActive;
        hotel.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<HotelDto>.Ok(MapToDto(hotel), "Hotel updated successfully");
    }

    public async Task<ApiResponse<HotelBookingDto>> CreateHotelBookingAsync(CreateHotelBookingDto request, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<HotelBookingDto>.Fail("Customer not found");
        }

        var hotel = await _context.Hotels.FindAsync(new object[] { request.HotelId }, cancellationToken);
        if (hotel == null)
        {
            return ApiResponse<HotelBookingDto>.Fail("Hotel not found");
        }

        var count = await _context.HotelBookings.CountAsync(cancellationToken) + 1;
        var confirmationNumber = $"HTL-{DateTime.UtcNow.Year}-{count:D5}";

        var hotelBooking = new HotelBooking
        {
            BookingId = request.BookingId,
            CustomerId = request.CustomerId,
            HotelId = request.HotelId,
            RoomType = request.RoomType,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            NumberOfGuests = request.NumberOfGuests,
            RoomCount = request.RoomCount,
            OxygenRoomRequested = request.OxygenRoomRequested,
            AgencyCost = request.AgencyCost,
            SellingPrice = request.SellingPrice,
            Currency = request.Currency,
            Status = "Confirmed",
            ConfirmationNumber = confirmationNumber,
            SpecialRequests = request.SpecialRequests,
            Notes = request.Notes
        };

        await _context.HotelBookings.AddAsync(hotelBooking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        hotelBooking.Customer = customer;
        hotelBooking.Hotel = hotel;

        return ApiResponse<HotelBookingDto>.Ok(MapToBookingDto(hotelBooking), "Hotel booking confirmed successfully");
    }

    public async Task<ApiResponse<List<HotelBookingDto>>> GetCustomerHotelBookingsAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        if (requestingRole == "Customer")
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
            if (customer == null || customer.UserId != requestingUserId)
            {
                return ApiResponse<List<HotelBookingDto>>.Fail("Access denied: You are not authorized to view another customer's hotel bookings");
            }
        }

        var bookings = await _context.HotelBookings
            .AsNoTracking()
            .Include(hb => hb.Customer)
            .Include(hb => hb.Hotel)
            .Where(hb => hb.CustomerId == customerId)
            .OrderBy(hb => hb.CheckInDate)
            .Select(hb => MapToBookingDto(hb))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<HotelBookingDto>>.Ok(bookings);
    }

    public async Task<ApiResponse<bool>> UpdateHotelBookingStatusAsync(Guid hotelBookingId, string status, CancellationToken cancellationToken = default)
    {
        var booking = await _context.HotelBookings.FirstOrDefaultAsync(hb => hb.Id == hotelBookingId, cancellationToken);
        if (booking == null)
        {
            return ApiResponse<bool>.Fail("Hotel booking not found");
        }

        booking.Status = status;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Hotel booking status updated successfully");
    }

    private static HotelDto MapToDto(Hotel h) => new()
    {
        Id = h.Id,
        Name = h.Name,
        HebrewName = h.HebrewName,
        Address = h.Address,
        Destination = h.Destination,
        Stars = h.Stars,
        Phone = h.Phone,
        WhatsApp = h.WhatsApp,
        Email = h.Email,
        Website = h.Website,
        Latitude = h.Latitude,
        Longitude = h.Longitude,
        HasOxygenEnrichedRooms = h.HasOxygenEnrichedRooms,
        HasOxygenConcentrators = h.HasOxygenConcentrators,
        HasHeating = h.HasHeating,
        IsKosherFriendly = h.IsKosherFriendly,
        ShabbatFriendly = h.ShabbatFriendly,
        WalkingDistanceToChabadCusco = h.WalkingDistanceToChabadCusco,
        RoomTypes = h.RoomTypes,
        PhotoUrls = h.PhotoUrls,
        Notes = h.Notes,
        IsActive = h.IsActive
    };

    private static HotelBookingDto MapToBookingDto(HotelBooking hb) => new()
    {
        Id = hb.Id,
        BookingId = hb.BookingId,
        CustomerId = hb.CustomerId,
        CustomerName = hb.Customer != null ? $"{hb.Customer.FirstName} {hb.Customer.LastName}" : string.Empty,
        HotelId = hb.HotelId,
        HotelName = hb.Hotel?.Name ?? string.Empty,
        Destination = hb.Hotel?.Destination ?? string.Empty,
        RoomType = hb.RoomType,
        CheckInDate = hb.CheckInDate,
        CheckOutDate = hb.CheckOutDate,
        NumberOfGuests = hb.NumberOfGuests,
        RoomCount = hb.RoomCount,
        OxygenRoomRequested = hb.OxygenRoomRequested,
        AgencyCost = hb.AgencyCost,
        SellingPrice = hb.SellingPrice,
        Currency = hb.Currency,
        Status = hb.Status,
        ConfirmationNumber = hb.ConfirmationNumber,
        SpecialRequests = hb.SpecialRequests,
        Notes = hb.Notes
    };
}

