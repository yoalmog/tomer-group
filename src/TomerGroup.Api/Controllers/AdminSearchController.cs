using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager,Sales,Operations,Finance,Guide,Driver")]
public class SearchController : ControllerBase
{
    private readonly TomerDbContext _context;

    public SearchController(TomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AdminSearchResultDto>>> Search([FromQuery] string? q)
    {
        var result = new AdminSearchResultDto
        {
            Query = q ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            return Ok(ApiResponse<AdminSearchResultDto>.Ok(result));
        }

        var term = q.Trim().ToLowerInvariant();

        // 1. Customers
        var customers = await _context.Customers
            .Include(c => c.User)
            .Where(c => c.FirstName.ToLower().Contains(term) ||
                        c.LastName.ToLower().Contains(term) ||
                        c.Email.ToLower().Contains(term) ||
                        c.Phone.ToLower().Contains(term) ||
                        (c.PassportNumber != null && c.PassportNumber.ToLower().Contains(term)))
            .Take(8)
            .Select(c => new AdminSearchItemDto
            {
                Id = c.Id,
                Category = "Customer",
                Title = $"{c.FirstName} {c.LastName}",
                Subtitle = c.Email,
                Status = c.IsActiveInPeru ? "In Peru" : "Registered",
                ExtraInfo = c.Phone
            })
            .ToListAsync();
        result.Customers = customers;

        // 2. Bookings
        var bookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Trip)
            .Where(b => b.BookingCode.ToLower().Contains(term) ||
                        (b.Notes != null && b.Notes.ToLower().Contains(term)) ||
                        (b.Customer != null && (b.Customer.FirstName.ToLower().Contains(term) || b.Customer.LastName.ToLower().Contains(term))))
            .Take(8)
            .Select(b => new AdminSearchItemDto
            {
                Id = b.Id,
                Category = "Booking",
                Title = $"Booking {b.BookingCode}",
                Subtitle = b.Trip != null ? b.Trip.Title : "Custom Trip",
                Status = b.Status.ToString(),
                ExtraInfo = $"${b.TotalAmount:N0} ({b.PaymentStatus})"
            })
            .ToListAsync();
        result.Bookings = bookings;

        // 3. Trips
        var trips = await _context.Trips
            .Include(t => t.Customer)
            .Where(t => t.Title.ToLower().Contains(term) ||
                        (t.Customer != null && (t.Customer.FirstName.ToLower().Contains(term) || t.Customer.LastName.ToLower().Contains(term))))
            .Take(8)
            .Select(t => new AdminSearchItemDto
            {
                Id = t.Id,
                Category = "Trip",
                Title = t.Title,
                Subtitle = t.Customer != null ? $"{t.Customer.FirstName} {t.Customer.LastName}" : "Tomer Group Trip",
                Status = t.Status.ToString(),
                ExtraInfo = $"{t.StartDate:dd/MM/yyyy} - {t.EndDate:dd/MM/yyyy}"
            })
            .ToListAsync();
        result.Trips = trips;

        // 4. Treks & Tours
        var treks = await _context.TrekRoutes
            .Where(tr => tr.Name.ToLower().Contains(term) || tr.Region.ToLower().Contains(term))
            .Take(8)
            .Select(tr => new AdminSearchItemDto
            {
                Id = tr.Id,
                Category = "Trek",
                Title = tr.Name,
                Subtitle = $"{tr.DurationDays} Days - {tr.Region}",
                Status = tr.IsPublished ? "Published" : "Draft",
                ExtraInfo = $"{tr.DistanceKm:N1} km | Max {tr.MaxElevationMeters}m"
            })
            .ToListAsync();
        result.Treks = treks;

        // 5. Guides
        var guides = await _context.Guides
            .Where(g => g.FullName.ToLower().Contains(term) ||
                        (g.Specialization != null && g.Specialization.ToLower().Contains(term)))
            .Take(8)
            .Select(g => new AdminSearchItemDto
            {
                Id = g.Id,
                Category = "Guide",
                Title = g.FullName,
                Subtitle = string.Join(", ", g.Languages),
                Status = g.IsAvailable ? "Available" : "Busy",
                ExtraInfo = g.Specialization ?? "High-altitude treks"
            })
            .ToListAsync();
        result.Guides = guides;

        // 6. Drivers
        var drivers = await _context.Drivers
            .Where(d => d.FullName.ToLower().Contains(term) ||
                        (d.LicenseNumber != null && d.LicenseNumber.ToLower().Contains(term)) ||
                        d.Phone.ToLower().Contains(term))
            .Take(8)
            .Select(d => new AdminSearchItemDto
            {
                Id = d.Id,
                Category = "Driver",
                Title = d.FullName,
                Subtitle = d.Phone,
                Status = d.IsAvailable ? "Available" : "On Trip",
                ExtraInfo = d.LicenseCategory ?? "Professional Driver"
            })
            .ToListAsync();
        result.Drivers = drivers;

        // 7. Hotels
        var hotels = await _context.Hotels
            .Where(h => h.Name.ToLower().Contains(term) ||
                        h.Destination.ToLower().Contains(term) ||
                        h.Address.ToLower().Contains(term))
            .Take(8)
            .Select(h => new AdminSearchItemDto
            {
                Id = h.Id,
                Category = "Hotel",
                Title = h.Name,
                Subtitle = $"{h.Stars} Stars - {h.Destination}",
                Status = h.IsActive ? "Partner" : "Inactive",
                ExtraInfo = h.Address
            })
            .ToListAsync();
        result.Hotels = hotels;

        // 8. Support Tickets
        var tickets = await _context.SupportTickets
            .Include(st => st.Customer)
            .Where(st => st.Subject.ToLower().Contains(term) ||
                         (st.Customer != null && (st.Customer.FirstName.ToLower().Contains(term) || st.Customer.LastName.ToLower().Contains(term))))
            .Take(8)
            .Select(st => new AdminSearchItemDto
            {
                Id = st.Id,
                Category = "Support",
                Title = st.Subject,
                Subtitle = st.Customer != null ? $"{st.Customer.FirstName} {st.Customer.LastName}" : "Customer Inquiry",
                Status = st.Status.ToString(),
                ExtraInfo = $"Priority: {st.Priority}"
            })
            .ToListAsync();
        result.SupportTickets = tickets;

        return Ok(ApiResponse<AdminSearchResultDto>.Ok(result));
    }
}
