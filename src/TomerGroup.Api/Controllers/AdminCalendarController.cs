using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager,Sales,Operations,Finance,Guide,Driver")]
public class CalendarController : ControllerBase
{
    private readonly TomerDbContext _context;

    public CalendarController(TomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OperationalCalendarEventDto>>>> GetCalendarEvents(
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end,
        [FromQuery] string? filter)
    {
        var startDate = start?.ToUniversalTime().Date ?? DateTime.UtcNow.Date.AddDays(-1);
        var endDate = end?.ToUniversalTime().Date ?? DateTime.UtcNow.Date.AddDays(14);

        if (endDate < startDate)
        {
            endDate = startDate.AddDays(7);
        }

        var events = new List<OperationalCalendarEventDto>();

        // 1. Transportations / Pickups / Transfers
        if (string.IsNullOrEmpty(filter) || filter.Equals("all", StringComparison.OrdinalIgnoreCase) || filter.Equals("transfers", StringComparison.OrdinalIgnoreCase))
        {
            var transportations = await _context.Transportations
                .AsNoTracking()
                .Include(t => t.Customer)
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .Where(t => t.ScheduledPickupTime >= startDate && t.ScheduledPickupTime <= endDate.AddDays(1))
                .OrderBy(t => t.ScheduledPickupTime)
                .ToListAsync();

            foreach (var t in transportations)
            {
                var eventType = t.ServiceType.Contains("Airport", StringComparison.OrdinalIgnoreCase) ? "Pickup" :
                                t.ServiceType.Contains("Transfer", StringComparison.OrdinalIgnoreCase) ? "Transfer" : "Transportation";

                events.Add(new OperationalCalendarEventDto
                {
                    Id = t.Id,
                    StartTime = t.ScheduledPickupTime,
                    EndTime = t.EstimatedArrivalTime ?? t.ScheduledPickupTime.AddHours(1.5),
                    EventType = eventType,
                    Title = $"{t.ServiceType}: {t.PickupLocation} \u2192 {t.DropoffLocation}",
                    Details = $"Vehicle: {t.Vehicle?.Model ?? "Unassigned"} ({t.Vehicle?.LicensePlate ?? "TBD"}) | Passengers: {t.PassengerCount}",
                    CustomerName = t.Customer != null ? $"{t.Customer.FirstName} {t.Customer.LastName}".Trim() : "Traveler",
                    Location = t.PickupLocation,
                    DriverName = t.Driver?.FullName,
                    Status = t.Status.ToString()
                });
            }
        }

        // 2. Activities & Tours
        if (string.IsNullOrEmpty(filter) || filter.Equals("all", StringComparison.OrdinalIgnoreCase) || filter.Equals("activities", StringComparison.OrdinalIgnoreCase))
        {
            var activities = await _context.Activities
                .AsNoTracking()
                .Include(a => a.TripDay)
                    .ThenInclude(td => td!.Trip)
                        .ThenInclude(tr => tr!.Customer)
                .Include(a => a.Guide)
                .Where(a => a.TripDay != null && a.TripDay.Date >= startDate && a.TripDay.Date <= endDate.AddDays(1))
                .ToListAsync();

            foreach (var a in activities)
            {
                var dayDate = a.TripDay!.Date.Date;
                var actStart = dayDate.Add(a.StartTime);
                var actEnd = a.EndTime > a.StartTime ? dayDate.Add(a.EndTime) : actStart.AddHours(2);

                events.Add(new OperationalCalendarEventDto
                {
                    Id = a.Id,
                    StartTime = actStart,
                    EndTime = actEnd,
                    EventType = "Activity",
                    Title = a.Title,
                    Details = a.Description ?? $"Destination: {a.TripDay.Destination}",
                    CustomerName = a.TripDay.Trip?.Customer != null ? $"{a.TripDay.Trip.Customer.FirstName} {a.TripDay.Trip.Customer.LastName}".Trim() : "Customer Group",
                    Location = string.IsNullOrWhiteSpace(a.Location) ? a.TripDay.Destination : a.Location,
                    GuideName = a.Guide?.FullName,
                    Status = "Confirmed"
                });
            }
        }

        // 3. Hotel Check-ins and Check-outs
        if (string.IsNullOrEmpty(filter) || filter.Equals("all", StringComparison.OrdinalIgnoreCase) || filter.Equals("hotels", StringComparison.OrdinalIgnoreCase))
        {
            var hotelBookings = await _context.HotelBookings
                .AsNoTracking()
                .Include(hb => hb.Hotel)
                .Include(hb => hb.Customer)
                .Where(hb => (hb.CheckInDate >= startDate && hb.CheckInDate <= endDate.AddDays(1)) ||
                             (hb.CheckOutDate >= startDate && hb.CheckOutDate <= endDate.AddDays(1)))
                .ToListAsync();

            foreach (var hb in hotelBookings)
            {
                var custName = hb.Customer != null ? $"{hb.Customer.FirstName} {hb.Customer.LastName}".Trim() : "Guest";

                if (hb.CheckInDate >= startDate && hb.CheckInDate <= endDate.AddDays(1))
                {
                    events.Add(new OperationalCalendarEventDto
                    {
                        Id = Guid.NewGuid(),
                        StartTime = hb.CheckInDate.Date.AddHours(14),
                        EndTime = hb.CheckInDate.Date.AddHours(15),
                        EventType = "HotelCheckIn",
                        Title = $"Check-In: {hb.Hotel?.Name ?? "Hotel"}",
                        Details = $"Room: {hb.RoomType} ({hb.RoomCount} room/s) | Guests: {hb.NumberOfGuests}",
                        CustomerName = custName,
                        Location = hb.Hotel?.Address ?? hb.Hotel?.Destination ?? "Cusco",
                        Status = hb.Status
                    });
                }

                if (hb.CheckOutDate >= startDate && hb.CheckOutDate <= endDate.AddDays(1))
                {
                    events.Add(new OperationalCalendarEventDto
                    {
                        Id = Guid.NewGuid(),
                        StartTime = hb.CheckOutDate.Date.AddHours(10),
                        EndTime = hb.CheckOutDate.Date.AddHours(11),
                        EventType = "HotelCheckOut",
                        Title = $"Check-Out: {hb.Hotel?.Name ?? "Hotel"}",
                        Details = $"Room: {hb.RoomType} | Room Key Return & Luggage Storage",
                        CustomerName = custName,
                        Location = hb.Hotel?.Address ?? hb.Hotel?.Destination ?? "Cusco",
                        Status = hb.Status
                    });
                }
            }
        }

        // Sort chronologically
        events = events.OrderBy(e => e.StartTime).ToList();

        return Ok(ApiResponse<List<OperationalCalendarEventDto>>.Ok(events));
    }
}

