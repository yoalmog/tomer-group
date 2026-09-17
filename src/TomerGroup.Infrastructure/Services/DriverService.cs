using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class DriverService : IDriverService
{
    private readonly TomerDbContext _context;

    public DriverService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<DriverDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var drivers = await _context.Drivers
            .AsNoTracking()
            .Include(d => d.Vehicles)
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.FullName)
            .Select(d => MapToDto(d))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<DriverDto>>.Ok(drivers);
    }

    public async Task<ApiResponse<DriverDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers
            .AsNoTracking()
            .Include(d => d.Vehicles)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (driver == null)
        {
            return ApiResponse<DriverDto>.Fail("Driver not found");
        }

        return ApiResponse<DriverDto>.Ok(MapToDto(driver));
    }

    public async Task<ApiResponse<DriverDto>> CreateAsync(CreateDriverDto request, CancellationToken cancellationToken = default)
    {
        var driver = new Driver
        {
            FullName = request.FullName,
            Phone = request.Phone,
            WhatsApp = request.WhatsApp,
            Email = request.Email,
            LicenseNumber = request.LicenseNumber,
            LicenseCategory = request.LicenseCategory,
            LicenseExpirationDate = request.LicenseExpirationDate,
            EmergencyContact = request.EmergencyContact,
            IsAvailable = request.IsAvailable,
            Notes = request.Notes
        };

        await _context.Drivers.AddAsync(driver, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<DriverDto>.Ok(MapToDto(driver), "Driver created successfully");
    }

    public async Task<ApiResponse<DriverDto>> UpdateAsync(Guid id, UpdateDriverDto request, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (driver == null)
        {
            return ApiResponse<DriverDto>.Fail("Driver not found");
        }

        driver.FullName = request.FullName;
        driver.Phone = request.Phone;
        driver.WhatsApp = request.WhatsApp;
        driver.Email = request.Email;
        driver.LicenseNumber = request.LicenseNumber;
        driver.LicenseCategory = request.LicenseCategory;
        driver.LicenseExpirationDate = request.LicenseExpirationDate;
        driver.Rating = request.Rating;
        driver.EmergencyContact = request.EmergencyContact;
        driver.IsAvailable = request.IsAvailable;
        driver.Notes = request.Notes;
        driver.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<DriverDto>.Ok(MapToDto(driver), "Driver updated successfully");
    }

    public async Task<ApiResponse<DriverDailyManifestDto>> GetDailyManifestAsync(Guid driverId, DateTime date, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.Id == driverId, cancellationToken);
        if (driver == null)
        {
            return ApiResponse<DriverDailyManifestDto>.Fail("Driver not found");
        }

        var targetDate = date.Date;

        var transfers = await _context.Transportations
            .AsNoTracking()
            .Include(t => t.Vehicle)
            .Include(t => t.Customer)
            .Where(t => t.DriverId == driverId && t.ScheduledPickupTime.Date == targetDate)
            .OrderBy(t => t.ScheduledPickupTime)
            .ToListAsync(cancellationToken);

        var manifest = new DriverDailyManifestDto
        {
            DriverId = driver.Id,
            DriverName = driver.FullName,
            Date = targetDate,
            Transfers = transfers.Select(t => new DriverManifestItemDto
            {
                TransportationId = t.Id,
                ServiceType = t.ServiceType,
                HebrewServiceType = t.HebrewServiceType,
                ScheduledPickupTime = t.ScheduledPickupTime,
                PickupLocation = t.PickupLocation,
                DropoffLocation = t.DropoffLocation,
                PassengerCount = t.PassengerCount,
                FlightOrTrainNumber = t.FlightOrTrainNumber,
                CustomerName = t.Customer != null ? $"{t.Customer.FirstName} {t.Customer.LastName}" : string.Empty,
                CustomerPhone = t.Customer?.Phone ?? string.Empty,
                CustomerWhatsApp = t.Customer?.WhatsApp ?? string.Empty,
                VehiclePlate = t.Vehicle?.LicensePlate,
                VehicleModel = t.Vehicle?.Model,
                Status = t.Status
            }).ToList()
        };

        return ApiResponse<DriverDailyManifestDto>.Ok(manifest);
    }

    public async Task<ApiResponse<bool>> AssignDriverToTransferAsync(AssignDriverToTransferDto request, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var transfer = await _context.Transportations
            .FirstOrDefaultAsync(t => t.Id == request.TransportationId, cancellationToken);

        if (transfer == null)
        {
            return ApiResponse<bool>.Fail("Transportation transfer not found");
        }

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken);
        if (driver == null)
        {
            return ApiResponse<bool>.Fail("Driver not found");
        }

        if (!driver.IsAvailable)
        {
            return ApiResponse<bool>.Fail("Driver is currently marked as unavailable");
        }

        // Conflict check: ensure driver doesn't have an overlapping transfer within 60 minutes
        var transferDate = transfer.ScheduledPickupTime.Date;
        var existingTransfers = await _context.Transportations
            .AsNoTracking()
            .Where(t => t.DriverId == request.DriverId &&
                        t.Id != request.TransportationId &&
                        t.ScheduledPickupTime.Date == transferDate &&
                        t.Status != Core.Enums.TransportationStatus.Cancelled)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingTransfers)
        {
            var diff = Math.Abs((transfer.ScheduledPickupTime - existing.ScheduledPickupTime).TotalMinutes);
            if (diff < 60)
            {
                return ApiResponse<bool>.Fail($"Schedule conflict: Driver {driver.FullName} has another transfer scheduled at {existing.ScheduledPickupTime:HH:mm} (minimum 60 minute buffer required).");
            }
        }

        transfer.DriverId = request.DriverId;
        transfer.Driver = driver;
        if (request.VehicleId.HasValue)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId.Value, cancellationToken);
            if (vehicle != null)
            {
                transfer.VehicleId = vehicle.Id;
                transfer.Vehicle = vehicle;
            }
        }
        transfer.Status = Core.Enums.TransportationStatus.Assigned;
        transfer.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            Action = "AssignDriverToTransfer",
            EntityName = "Transportation",
            EntityId = transfer.Id.ToString(),
            UserId = requestingUserId,
            UserEmail = "staff@tomergroup.com",
            MetadataJson = $"Assigned driver {driver.FullName} ({driver.LicenseNumber}) to transfer '{transfer.ServiceType}'"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, $"Driver {driver.FullName} assigned to transfer successfully");
    }

    private static DriverDto MapToDto(Driver d) => new()
    {
        Id = d.Id,
        FullName = d.FullName,
        Phone = d.Phone,
        WhatsApp = d.WhatsApp,
        Email = d.Email,
        LicenseNumber = d.LicenseNumber,
        LicenseCategory = d.LicenseCategory,
        LicenseExpirationDate = d.LicenseExpirationDate,
        Rating = d.Rating,
        EmergencyContact = d.EmergencyContact,
        IsAvailable = d.IsAvailable,
        Notes = d.Notes,
        Vehicles = d.Vehicles.Select(v => new VehicleDto
        {
            Id = v.Id,
            Model = v.Model,
            LicensePlate = v.LicensePlate,
            PassengerCapacity = v.PassengerCapacity,
            LuggageCapacity = v.LuggageCapacity,
            Type = v.Type,
            Year = v.Year,
            HasAirConditioning = v.HasAirConditioning,
            IsActive = v.IsActive
        }).ToList()
    };
}

