using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager,Sales,Operations,Finance")]
public class DashboardController : ControllerBase
{
    private readonly TomerDbContext _context;

    public DashboardController(TomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AdminDashboardMetricsDto>>> GetDashboardMetrics()
    {
        var today = DateTime.UtcNow.Date;

        var todayArrivals = await _context.Trips
            .CountAsync(t => t.StartDate.Date == today && t.Status != TripStatus.Cancelled);

        var todayDepartures = await _context.Trips
            .CountAsync(t => t.EndDate.Date == today && t.Status != TripStatus.Cancelled);

        var activeTrips = await _context.Trips
            .CountAsync(t => t.Status == TripStatus.InProgress || (t.StartDate.Date <= today && t.EndDate.Date >= today && t.Status != TripStatus.Cancelled));

        var upcomingTrips = await _context.Trips
            .CountAsync(t => t.StartDate.Date > today && t.Status == TripStatus.Confirmed);

        var totalCustomers = await _context.Customers.CountAsync();
        var activeInPeru = await _context.Customers.CountAsync(c => c.IsActiveInPeru);

        var pendingBookings = await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Pending);

        var confirmedBookings = await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Confirmed);

        var pendingPayments = await _context.Payments
            .CountAsync(p => p.Status == PaymentStatus.Pending);

        var assignedTransfers = await _context.Transportations
            .CountAsync(t => t.DriverId != null && t.Status != TransportationStatus.Cancelled);

        var unassignedTransfers = await _context.Transportations
            .CountAsync(t => t.DriverId == null && t.Status != TransportationStatus.Cancelled);

        var assignedGuides = await _context.Activities
            .CountAsync(a => a.GuideId != null && a.Status != ActivityStatus.Cancelled);

        var unassignedGuides = await _context.Activities
            .CountAsync(a => a.GuideId == null && a.Status != ActivityStatus.Cancelled);

        var totalRevenue = await _context.Trips
            .Where(t => t.Status != TripStatus.Cancelled)
            .SumAsync(t => t.TotalRevenue);

        var totalExpenses = await _context.Expenses
            .SumAsync(e => e.Amount);

        var grossProfit = totalRevenue - totalExpenses;
        var marginPct = totalRevenue > 0 ? Math.Round((grossProfit / totalRevenue) * 100, 2) : 0;

        var auditLogs = await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync();

        var recentActivities = auditLogs.Select(a => new AdminActivityFeedItemDto
        {
            Id = a.Id,
            Timestamp = a.CreatedAt,
            Action = a.Action,
            Entity = a.EntityName,
            Description = $"{a.Action} on {a.EntityName} (ID: {a.EntityId})",
            UserEmail = a.UserEmail ?? a.UserId?.ToString() ?? "system"
        }).ToList();

        var metrics = new AdminDashboardMetricsDto
        {
            TodayArrivals = todayArrivals,
            TodayDepartures = todayDepartures,
            ActiveTrips = activeTrips,
            UpcomingTrips = upcomingTrips,
            TotalCustomers = totalCustomers,
            ActiveInPeruCustomers = activeInPeru,
            PendingBookings = pendingBookings,
            ConfirmedBookings = confirmedBookings,
            PendingPayments = pendingPayments,
            AssignedTransportationCount = assignedTransfers,
            UnassignedTransportationCount = unassignedTransfers,
            AssignedGuidesCount = assignedGuides,
            UnassignedGuidesCount = unassignedGuides,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            GrossProfit = grossProfit,
            MarginPercentage = marginPct,
            RecentActivities = recentActivities
        };

        return Ok(ApiResponse<AdminDashboardMetricsDto>.Ok(metrics));
    }
}

