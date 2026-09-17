using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class TripService : ITripService
{
    private readonly TomerDbContext _context;

    public TripService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TripDto>> GetByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .AsNoTracking()
            .Include(t => t.Customer)
            .Include(t => t.Days.OrderBy(d => d.DayNumber))
                .ThenInclude(d => d.Activities.OrderBy(a => a.StartTime))
                    .ThenInclude(a => a.Driver)
            .Include(t => t.Days)
                .ThenInclude(d => d.Activities)
                    .ThenInclude(a => a.Guide)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (trip == null)
        {
            return ApiResponse<TripDto>.Fail("Trip not found");
        }

        // Strict Customer Isolation (Section 34)
        if (requestingRole == "Customer" && trip.Customer?.UserId != requestingUserId)
        {
            return ApiResponse<TripDto>.Fail("Access denied: You are not authorized to view another traveler's itinerary");
        }

        return ApiResponse<TripDto>.Ok(MapToDto(trip));
    }

    public async Task<ApiResponse<List<TripDto>>> GetByCustomerIdAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        // Strict Customer Isolation check
        if (requestingRole == "Customer")
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
            if (customer == null || customer.UserId != requestingUserId)
            {
                return ApiResponse<List<TripDto>>.Fail("Access denied: You are not authorized to view these trips");
            }
        }

        var trips = await _context.Trips
            .AsNoTracking()
            .Include(t => t.Customer)
            .Include(t => t.Days.OrderBy(d => d.DayNumber))
                .ThenInclude(d => d.Activities.OrderBy(a => a.StartTime))
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.StartDate)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<TripDto>>.Ok(trips);
    }

    public async Task<ApiResponse<CustomerHomeDashboardDto>> GetCustomerHomeDashboardAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        // Strict Isolation Guard
        if (requestingRole == "Customer")
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
            if (customer == null || customer.UserId != requestingUserId)
            {
                return ApiResponse<CustomerHomeDashboardDto>.Fail("Access denied: You are not authorized to view another traveler's dashboard");
            }
        }

        var activeTrip = await _context.Trips
            .AsNoTracking()
            .Include(t => t.Customer)
            .Include(t => t.Days.OrderBy(d => d.DayNumber))
                .ThenInclude(d => d.Activities.OrderBy(a => a.StartTime))
                    .ThenInclude(a => a.Driver)
            .Include(t => t.Days)
                .ThenInclude(d => d.Activities)
                    .ThenInclude(a => a.Guide)
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.Status == TripStatus.InProgress)
            .ThenByDescending(t => t.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        var dashboard = new CustomerHomeDashboardDto();

        if (activeTrip != null)
        {
            dashboard.TripId = activeTrip.Id;
            dashboard.TripCode = activeTrip.TripCode;
            dashboard.TripTitle = activeTrip.Title;
            dashboard.CustomerName = activeTrip.Customer != null ? activeTrip.Customer.HebrewName : "מטייל";
            dashboard.StartDate = activeTrip.StartDate;
            dashboard.EndDate = activeTrip.EndDate;
            dashboard.TotalDays = activeTrip.Days.Count > 0 ? activeTrip.Days.Count : 5;

            var currentDay = activeTrip.Days.FirstOrDefault(d => d.DayNumber == 4) ?? activeTrip.Days.FirstOrDefault();
            if (currentDay != null)
            {
                dashboard.CurrentDayNumber = currentDay.DayNumber;
                dashboard.CurrentDayTitle = currentDay.Title;
                dashboard.CurrentDestination = currentDay.Destination;
                dashboard.TodayActivities = currentDay.Activities.Select(MapActivityToDto).ToList();
                dashboard.NextActivity = dashboard.TodayActivities.FirstOrDefault(a => a.Status != ActivityStatus.Completed) 
                    ?? dashboard.TodayActivities.FirstOrDefault();
            }
        }

        return ApiResponse<CustomerHomeDashboardDto>.Ok(dashboard);
    }

    public async Task<ApiResponse<PagedResult<TripDto>>> GetAllTripsAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Trips
            .AsNoTracking()
            .Include(t => t.Customer)
            .Include(t => t.Days)
                .ThenInclude(d => d.Activities)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(t =>
                t.TripCode.ToLower().Contains(search) ||
                t.Title.ToLower().Contains(search) ||
                (t.Customer != null && (t.Customer.FirstName.ToLower().Contains(search) || t.Customer.LastName.ToLower().Contains(search) || t.Customer.HebrewName.ToLower().Contains(search))));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(t => t.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<TripDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<TripDto>>.Ok(result);
    }

    public async Task<ApiResponse<TripDto>> CreateAsync(CreateTripDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<TripDto>.Fail("Customer not found");
        }

        var tripCount = await _context.Trips.CountAsync(cancellationToken) + 1;
        var tripCode = $"PERU-{DateTime.UtcNow.Year}-{tripCount:D5}";

        var trip = new Trip
        {
            TripCode = tripCode,
            Title = request.Title,
            Description = request.Description,
            CustomerId = request.CustomerId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalRevenue = request.TotalRevenue,
            Currency = request.Currency,
            Status = TripStatus.Scheduled
        };

        await _context.Trips.AddAsync(trip, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TripDto>.Ok(MapToDto(trip), "Trip created successfully");
    }

    public async Task<ApiResponse<TripDto>> UpdateAsync(Guid id, UpdateTripDto request, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips.Include(t => t.Customer).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (trip == null)
        {
            return ApiResponse<TripDto>.Fail("Trip not found");
        }

        trip.Title = request.Title;
        trip.Description = request.Description;
        trip.StartDate = request.StartDate;
        trip.EndDate = request.EndDate;
        trip.Status = request.Status;
        trip.TotalRevenue = request.TotalRevenue;
        trip.Currency = request.Currency;
        trip.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<TripDto>.Ok(MapToDto(trip), "Trip updated successfully");
    }

    public async Task<ApiResponse<TripDayDto>> AddTripDayAsync(Guid tripId, CreateTripDayDto request, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips.FindAsync(new object[] { tripId }, cancellationToken);
        if (trip == null)
        {
            return ApiResponse<TripDayDto>.Fail("Trip not found");
        }

        var day = new TripDay
        {
            TripId = tripId,
            DayNumber = request.DayNumber,
            Date = request.Date,
            Title = request.Title,
            Destination = request.Destination,
            Description = request.Description
        };

        await _context.TripDays.AddAsync(day, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TripDayDto>.Ok(new TripDayDto
        {
            Id = day.Id,
            DayNumber = day.DayNumber,
            Date = day.Date,
            Title = day.Title,
            Destination = day.Destination
        }, "Trip day added successfully");
    }

    public async Task<ApiResponse<ActivityDto>> AddActivityAsync(Guid tripDayId, CreateActivityDto request, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var day = await _context.TripDays.FindAsync(new object[] { tripDayId }, cancellationToken);
        if (day == null)
        {
            return ApiResponse<ActivityDto>.Fail("Trip day not found");
        }

        var activity = new Activity
        {
            TripDayId = tripDayId,
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Location = request.Location,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = request.Address,
            Instructions = request.Instructions,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            DriverId = request.DriverId,
            GuideId = request.GuideId,
            TransportationId = request.TransportationId,
            Status = request.Status,
            Notes = request.Notes
        };

        await _context.Activities.AddAsync(activity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ActivityDto>.Ok(MapActivityToDto(activity), "Activity added successfully");
    }

    public async Task<ApiResponse<ActivityDto>> UpdateActivityStatusAsync(Guid activityId, ActivityStatus status, Guid editorId, CancellationToken cancellationToken = default)
    {
        var activity = await _context.Activities
            .Include(a => a.Driver)
            .Include(a => a.Guide)
            .FirstOrDefaultAsync(a => a.Id == activityId, cancellationToken);

        if (activity == null)
        {
            return ApiResponse<ActivityDto>.Fail("Activity not found");
        }

        activity.Status = status;
        activity.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            UserId = editorId,
            Action = "ActivityStatusUpdate",
            EntityName = "Activity",
            EntityId = activity.Id.ToString(),
            MetadataJson = $"{{\"activity\":\"{activity.Title}\",\"newStatus\":\"{status}\"}}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<ActivityDto>.Ok(MapActivityToDto(activity), "Activity status updated successfully");
    }

    public async Task<ApiResponse<List<DestinationDto>>> GetDestinationsAsync(CancellationToken cancellationToken = default)
    {
        var destinations = await _context.Destinations
            .AsNoTracking()
            .OrderByDescending(d => d.IsPopular)
            .ThenBy(d => d.Name)
            .Select(d => new DestinationDto
            {
                Id = d.Id,
                Name = d.Name,
                HebrewName = d.HebrewName,
                SpanishName = d.SpanishName,
                Region = d.Region,
                AltitudeMeters = d.AltitudeMeters,
                Description = d.Description,
                AltitudeWarning = d.AltitudeWarning,
                IsPopular = d.IsPopular
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<DestinationDto>>.Ok(destinations);
    }

    public async Task<ApiResponse<DestinationDto>> CreateDestinationAsync(CreateDestinationDto request, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var destination = new Destination
        {
            Name = request.Name,
            HebrewName = request.HebrewName,
            SpanishName = request.SpanishName,
            Region = request.Region,
            AltitudeMeters = request.AltitudeMeters,
            Description = request.Description,
            AltitudeWarning = request.AltitudeWarning,
            IsPopular = request.IsPopular
        };

        await _context.Destinations.AddAsync(destination, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<DestinationDto>.Ok(new DestinationDto
        {
            Id = destination.Id,
            Name = destination.Name,
            HebrewName = destination.HebrewName,
            SpanishName = destination.SpanishName,
            Region = destination.Region,
            AltitudeMeters = destination.AltitudeMeters,
            Description = destination.Description,
            AltitudeWarning = destination.AltitudeWarning,
            IsPopular = destination.IsPopular
        }, "Destination added successfully");
    }

    private static TripDto MapToDto(Trip t) => new()
    {
        Id = t.Id,
        TripCode = t.TripCode,
        Title = t.Title,
        Description = t.Description,
        CustomerId = t.CustomerId,
        CustomerName = t.Customer != null ? $"{t.Customer.FirstName} {t.Customer.LastName}" : string.Empty,
        StartDate = t.StartDate,
        EndDate = t.EndDate,
        Status = t.Status,
        TotalRevenue = t.TotalRevenue,
        TotalCost = t.TotalCost,
        GrossProfit = t.GrossProfit,
        Currency = t.Currency,
        Days = t.Days.Select(d => new TripDayDto
        {
            Id = d.Id,
            DayNumber = d.DayNumber,
            Date = d.Date,
            Title = d.Title,
            Destination = d.Destination,
            Activities = d.Activities.Select(MapActivityToDto).ToList()
        }).ToList()
    };

    private static ActivityDto MapActivityToDto(Activity a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        StartTime = a.StartTime,
        EndTime = a.EndTime,
        Location = a.Location,
        Instructions = a.Instructions,
        ContactName = a.ContactName,
        ContactPhone = a.ContactPhone,
        DriverName = a.Driver != null ? a.Driver.FullName : null,
        DriverPhone = a.Driver?.Phone,
        GuideName = a.Guide != null ? a.Guide.FullName : null,
        GuidePhone = a.Guide?.Phone,
        Status = a.Status
    };
}
