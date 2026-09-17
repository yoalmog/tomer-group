using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class TourService : ITourService
{
    private readonly TomerDbContext _context;

    public TourService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<TourDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tours = await _context.Tours
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<TourDto>>.Ok(tours);
    }

    public async Task<ApiResponse<TourDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tour = await _context.Tours.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tour == null)
        {
            return ApiResponse<TourDto>.Fail("Tour not found");
        }

        return ApiResponse<TourDto>.Ok(MapToDto(tour));
    }

    public async Task<ApiResponse<TourDto>> CreateAsync(CreateTourDto request, CancellationToken cancellationToken = default)
    {
        var tour = new Tour
        {
            Name = request.Name,
            HebrewName = request.HebrewName,
            Description = request.Description,
            HebrewDescription = request.HebrewDescription,
            Destination = request.Destination,
            Category = request.Category,
            Duration = request.Duration,
            DurationDays = request.DurationDays,
            Difficulty = request.Difficulty,
            MaxCapacity = request.MaxCapacity,
            AltitudeMaxMeters = request.AltitudeMaxMeters,
            RequiresAcclimatization = request.RequiresAcclimatization,
            AdultPrice = request.AdultPrice,
            ChildPrice = request.ChildPrice,
            PrivatePrice = request.PrivatePrice,
            AgencyCost = request.AgencyCost,
            Currency = request.Currency,
            IncludedServices = request.IncludedServices,
            ExcludedServices = request.ExcludedServices,
            PickupInformation = request.PickupInformation,
            MeetingPoint = request.MeetingPoint,
            LocationsVisited = request.LocationsVisited,
            CancellationPolicy = request.CancellationPolicy,
            KosherFoodAvailable = request.KosherFoodAvailable,
            KosherCertificationDetails = request.KosherCertificationDetails,
            BookingCutoffHours = request.BookingCutoffHours
        };

        await _context.Tours.AddAsync(tour, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TourDto>.Ok(MapToDto(tour), "Tour created successfully");
    }

    public async Task<ApiResponse<TourDto>> UpdateAsync(Guid id, UpdateTourDto request, CancellationToken cancellationToken = default)
    {
        var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tour == null)
        {
            return ApiResponse<TourDto>.Fail("Tour not found");
        }

        tour.Name = request.Name;
        tour.HebrewName = request.HebrewName;
        tour.Description = request.Description;
        tour.HebrewDescription = request.HebrewDescription;
        tour.Destination = request.Destination;
        tour.Category = request.Category;
        tour.Duration = request.Duration;
        tour.DurationDays = request.DurationDays;
        tour.Difficulty = request.Difficulty;
        tour.MaxCapacity = request.MaxCapacity;
        tour.AltitudeMaxMeters = request.AltitudeMaxMeters;
        tour.RequiresAcclimatization = request.RequiresAcclimatization;
        tour.AdultPrice = request.AdultPrice;
        tour.ChildPrice = request.ChildPrice;
        tour.PrivatePrice = request.PrivatePrice;
        tour.AgencyCost = request.AgencyCost;
        tour.Currency = request.Currency;
        tour.IncludedServices = request.IncludedServices;
        tour.ExcludedServices = request.ExcludedServices;
        tour.PickupInformation = request.PickupInformation;
        tour.MeetingPoint = request.MeetingPoint;
        tour.LocationsVisited = request.LocationsVisited;
        tour.CancellationPolicy = request.CancellationPolicy;
        tour.KosherFoodAvailable = request.KosherFoodAvailable;
        tour.KosherCertificationDetails = request.KosherCertificationDetails;
        tour.BookingCutoffHours = request.BookingCutoffHours;
        tour.IsActive = request.IsActive;
        tour.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TourDto>.Ok(MapToDto(tour), "Tour updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tour == null)
        {
            return ApiResponse<bool>.Fail("Tour not found");
        }

        tour.IsDeleted = true;
        tour.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Tour deleted successfully");
    }

    private static TourDto MapToDto(Tour t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        HebrewName = t.HebrewName,
        Description = t.Description,
        HebrewDescription = t.HebrewDescription,
        Destination = t.Destination,
        Category = t.Category,
        Duration = t.Duration,
        DurationDays = t.DurationDays,
        Difficulty = t.Difficulty,
        MaxCapacity = t.MaxCapacity,
        AltitudeMaxMeters = t.AltitudeMaxMeters,
        RequiresAcclimatization = t.RequiresAcclimatization,
        AdultPrice = t.AdultPrice,
        ChildPrice = t.ChildPrice,
        PrivatePrice = t.PrivatePrice,
        AgencyCost = t.AgencyCost,
        Currency = t.Currency,
        IncludedServices = t.IncludedServices,
        ExcludedServices = t.ExcludedServices,
        PickupInformation = t.PickupInformation,
        MeetingPoint = t.MeetingPoint,
        LocationsVisited = t.LocationsVisited,
        CancellationPolicy = t.CancellationPolicy,
        KosherFoodAvailable = t.KosherFoodAvailable,
        KosherCertificationDetails = t.KosherCertificationDetails,
        BookingCutoffHours = t.BookingCutoffHours,
        IsActive = t.IsActive
    };
}

