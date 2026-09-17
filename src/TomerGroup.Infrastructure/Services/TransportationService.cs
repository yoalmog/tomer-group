using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class TransportationService : ITransportationService
{
    private readonly TomerDbContext _context;

    public TransportationService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<TransportationDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var transportations = await _context.Transportations
            .AsNoTracking()
            .Include(t => t.Driver)
            .Include(t => t.Vehicle)
            .Include(t => t.Customer)
            .OrderByDescending(t => t.ScheduledPickupTime)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<TransportationDto>>.Ok(transportations);
    }

    public async Task<ApiResponse<TransportationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var t = await _context.Transportations
            .AsNoTracking()
            .Include(t => t.Driver)
            .Include(t => t.Vehicle)
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (t == null)
        {
            return ApiResponse<TransportationDto>.Fail("Transportation record not found");
        }

        return ApiResponse<TransportationDto>.Ok(MapToDto(t));
    }

    public async Task<ApiResponse<TransportationDto>> CreateAsync(CreateTransportationDto request, CancellationToken cancellationToken = default)
    {
        var transportation = new Transportation
        {
            ServiceType = request.ServiceType,
            HebrewServiceType = request.HebrewServiceType,
            Type = request.Type,
            DriverId = request.DriverId,
            VehicleId = request.VehicleId,
            BookingId = request.BookingId,
            CustomerId = request.CustomerId,
            PickupLocation = request.PickupLocation,
            DropoffLocation = request.DropoffLocation,
            ScheduledPickupTime = request.ScheduledPickupTime,
            EstimatedArrivalTime = request.EstimatedArrivalTime,
            PassengerCount = request.PassengerCount,
            TrainCompany = request.TrainCompany,
            TrainService = request.TrainService,
            TrainNumber = request.TrainNumber,
            TrainStationDeparture = request.TrainStationDeparture,
            TrainStationArrival = request.TrainStationArrival,
            Airline = request.Airline,
            FlightOrTrainNumber = request.FlightOrTrainNumber,
            Status = TransportationStatus.Assigned,
            Notes = request.Notes
        };

        await _context.Transportations.AddAsync(transportation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch populated entity
        return await GetByIdAsync(transportation.Id, cancellationToken);
    }

    public async Task<ApiResponse<TransportationDto>> UpdateStatusAsync(Guid id, UpdateTransportationStatusDto request, Guid editorId, CancellationToken cancellationToken = default)
    {
        var transportation = await _context.Transportations
            .Include(t => t.Driver)
            .Include(t => t.Vehicle)
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (transportation == null)
        {
            return ApiResponse<TransportationDto>.Fail("Transportation record not found");
        }

        var oldStatus = transportation.Status;
        transportation.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            transportation.Notes = request.Notes;
        }
        transportation.UpdatedAt = DateTime.UtcNow;

        // Log audit trail
        var audit = new AuditLog
        {
            Action = "UpdateTransportationStatus",
            EntityName = "Transportation",
            EntityId = id.ToString(),
            UserId = editorId,
            UserEmail = "staff@tomergroup.com",
            MetadataJson = $"Transportation {id} status changed from {oldStatus} to {request.Status}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TransportationDto>.Ok(MapToDto(transportation), "Transportation status updated successfully");
    }

    public async Task<ApiResponse<List<TransportationDto>>> GetCustomerTransfersAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        if (requestingRole == "Customer")
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
            if (customer == null || customer.UserId != requestingUserId)
            {
                return ApiResponse<List<TransportationDto>>.Fail("Access denied: You are not authorized to view another customer's transportation");
            }
        }

        var transfers = await _context.Transportations
            .AsNoTracking()
            .Include(t => t.Driver)
            .Include(t => t.Vehicle)
            .Include(t => t.Customer)
            .Where(t => t.CustomerId == customerId || (t.Booking != null && t.Booking.CustomerId == customerId))
            .OrderBy(t => t.ScheduledPickupTime)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<TransportationDto>>.Ok(transfers);
    }

    public async Task<ApiResponse<List<VehicleDto>>> GetVehiclesAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await _context.Vehicles
            .AsNoTracking()
            .Include(v => v.AssignedDriver)
            .Where(v => v.IsActive)
            .OrderBy(v => v.Model)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                Model = v.Model,
                LicensePlate = v.LicensePlate,
                PassengerCapacity = v.PassengerCapacity,
                LuggageCapacity = v.LuggageCapacity,
                Type = v.Type,
                Year = v.Year,
                HasAirConditioning = v.HasAirConditioning,
                IsActive = v.IsActive,
                AssignedDriverId = v.AssignedDriverId,
                AssignedDriverName = v.AssignedDriver != null ? v.AssignedDriver.FullName : null
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<VehicleDto>>.Ok(vehicles);
    }

    public async Task<ApiResponse<VehicleDto>> CreateVehicleAsync(CreateVehicleDto request, CancellationToken cancellationToken = default)
    {
        var vehicle = new Vehicle
        {
            Model = request.Model,
            LicensePlate = request.LicensePlate,
            PassengerCapacity = request.PassengerCapacity,
            LuggageCapacity = request.LuggageCapacity,
            Type = request.Type,
            Year = request.Year,
            HasAirConditioning = request.HasAirConditioning,
            AssignedDriverId = request.AssignedDriverId,
            IsActive = true
        };

        await _context.Vehicles.AddAsync(vehicle, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<VehicleDto>.Ok(new VehicleDto
        {
            Id = vehicle.Id,
            Model = vehicle.Model,
            LicensePlate = vehicle.LicensePlate,
            PassengerCapacity = vehicle.PassengerCapacity,
            LuggageCapacity = vehicle.LuggageCapacity,
            Type = vehicle.Type,
            Year = vehicle.Year,
            HasAirConditioning = vehicle.HasAirConditioning,
            IsActive = vehicle.IsActive,
            AssignedDriverId = vehicle.AssignedDriverId
        }, "Vehicle created successfully");
    }

    private static TransportationDto MapToDto(Transportation t) => new()
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
        CustomerName = t.Customer != null ? $"{t.Customer.FirstName} {t.Customer.LastName}" : null,
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
    };
}
