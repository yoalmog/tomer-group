using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly TomerDbContext _context;

    public BookingService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<BookingDto>> GetByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (booking == null)
        {
            return ApiResponse<BookingDto>.Fail("Booking not found");
        }

        // Strict Customer Isolation check
        if (requestingRole == "Customer" && booking.Customer?.UserId != requestingUserId)
        {
            return ApiResponse<BookingDto>.Fail("Access denied: You are not authorized to view another customer's booking");
        }

        return ApiResponse<BookingDto>.Ok(MapToDto(booking));
    }

    public async Task<ApiResponse<BookingDetailedDto>> GetDetailedByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Include(b => b.HotelBookings)
                .ThenInclude(hb => hb.Hotel)
            .Include(b => b.Transportations)
                .ThenInclude(t => t.Driver)
            .Include(b => b.Transportations)
                .ThenInclude(t => t.Vehicle)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (booking == null)
        {
            return ApiResponse<BookingDetailedDto>.Fail("Booking not found");
        }

        // Strict Customer Isolation check
        if (requestingRole == "Customer" && booking.Customer?.UserId != requestingUserId)
        {
            return ApiResponse<BookingDetailedDto>.Fail("Access denied: You are not authorized to view another customer's booking");
        }

        var detailed = new BookingDetailedDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            CustomerId = booking.CustomerId,
            CustomerName = booking.Customer != null ? $"{booking.Customer.FirstName} {booking.Customer.LastName}" : string.Empty,
            TripId = booking.TripId,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            TotalAmount = booking.TotalAmount,
            PaidAmount = booking.PaidAmount,
            Currency = booking.Currency,
            Status = booking.Status,
            PaymentStatus = booking.PaymentStatus,
            HotelBookings = booking.HotelBookings.Select(hb => new HotelBookingDto
            {
                Id = hb.Id,
                BookingId = hb.BookingId,
                CustomerId = hb.CustomerId,
                CustomerName = booking.Customer != null ? $"{booking.Customer.FirstName} {booking.Customer.LastName}" : string.Empty,
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
            }).ToList(),
            Transportations = booking.Transportations.Select(t => new TransportationDto
            {
                Id = t.Id,
                ServiceType = t.ServiceType,
                HebrewServiceType = t.HebrewServiceType,
                Type = t.Type,
                DriverId = t.DriverId,
                DriverName = t.Driver?.FullName,
                DriverPhone = t.Driver?.Phone,
                DriverWhatsApp = t.Driver?.WhatsApp,
                VehicleId = t.VehicleId,
                VehicleModel = t.Vehicle?.Model,
                VehiclePlate = t.Vehicle?.LicensePlate,
                BookingId = t.BookingId,
                CustomerId = t.CustomerId,
                CustomerName = booking.Customer != null ? $"{booking.Customer.FirstName} {booking.Customer.LastName}" : null,
                PickupLocation = t.PickupLocation,
                DropoffLocation = t.DropoffLocation,
                ScheduledPickupTime = t.ScheduledPickupTime,
                EstimatedArrivalTime = t.EstimatedArrivalTime,
                PassengerCount = t.PassengerCount,
                TrainCompany = t.TrainCompany,
                TrainService = t.TrainService,
                TrainNumber = t.TrainNumber,
                TrainStationDeparture = t.TrainStationDeparture,
                TrainStationArrival = t.TrainStationArrival,
                Airline = t.Airline,
                FlightOrTrainNumber = t.FlightOrTrainNumber,
                Status = t.Status,
                Notes = t.Notes
            }).ToList()
        };

        return ApiResponse<BookingDetailedDto>.Ok(detailed);
    }

    public async Task<ApiResponse<List<BookingDto>>> GetByCustomerIdAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        if (requestingRole == "Customer")
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
            if (customer == null || customer.UserId != requestingUserId)
            {
                return ApiResponse<List<BookingDto>>.Fail("Access denied: You are not authorized to view these bookings");
            }
        }

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => MapToDto(b))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<BookingDto>>.Ok(bookings);
    }

    public async Task<ApiResponse<PagedResult<BookingDto>>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.BookingCode.ToLower().Contains(term) ||
                (b.Customer != null && (
                    b.Customer.FirstName.ToLower().Contains(term) ||
                    b.Customer.LastName.ToLower().Contains(term) ||
                    b.Customer.HebrewName.ToLower().Contains(term))));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => MapToDto(b))
            .ToListAsync(cancellationToken);

        return ApiResponse<PagedResult<BookingDto>>.Ok(new PagedResult<BookingDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<BookingDto>> CreateAsync(CreateBookingDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<BookingDto>.Fail("Customer not found");
        }

        var bookingCount = await _context.Bookings.CountAsync(cancellationToken) + 1;
        var bookingCode = $"TG-{DateTime.UtcNow.Year}-{bookingCount:D5}";

        var booking = new Booking
        {
            BookingCode = bookingCode,
            CustomerId = request.CustomerId,
            TripId = request.TripId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalAmount = request.TotalAmount,
            Currency = request.Currency,
            Status = Core.Enums.BookingStatus.Confirmed,
            PaymentStatus = Core.Enums.PaymentStatus.Pending,
            Notes = request.Notes
        };

        await _context.Bookings.AddAsync(booking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        booking.Customer = customer;
        return ApiResponse<BookingDto>.Ok(MapToDto(booking), "Booking created successfully");
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        BookingCode = b.BookingCode,
        CustomerId = b.CustomerId,
        CustomerName = b.Customer != null ? $"{b.Customer.FirstName} {b.Customer.LastName}" : string.Empty,
        TripId = b.TripId,
        StartDate = b.StartDate,
        EndDate = b.EndDate,
        TotalAmount = b.TotalAmount,
        PaidAmount = b.PaidAmount,
        Currency = b.Currency,
        Status = b.Status,
        PaymentStatus = b.PaymentStatus
    };
}



