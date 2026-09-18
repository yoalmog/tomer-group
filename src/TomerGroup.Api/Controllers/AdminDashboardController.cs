using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
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
        var marginPct = totalRevenue > 0 ? (double)Math.Round((grossProfit / totalRevenue) * 100, 2) : 0;

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

    [HttpGet("v2")]
    public async Task<ActionResult<ApiResponse<AdminDashboardV2Dto>>> GetOperationsDashboardV2()
    {
        var today = DateTime.UtcNow.Date;
        var now = DateTime.UtcNow;

        // TOP Information
        var userEmail = User.Identity?.Name ?? "staff@tomergroup.com";
        var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "Operations";
        var unreadNotifications = await _context.Notifications.CountAsync(n => !n.IsRead);

        // SUMMARY Counts
        var todayArrivals = await _context.Trips
            .CountAsync(t => t.StartDate.Date == today && t.Status != TripStatus.Cancelled);

        var todayDepartures = await _context.Trips
            .CountAsync(t => t.EndDate.Date == today && t.Status != TripStatus.Cancelled);

        var activeTrips = await _context.Trips
            .CountAsync(t => t.Status == TripStatus.InProgress || (t.StartDate.Date <= today && t.EndDate.Date >= today && t.Status != TripStatus.Cancelled));

        var upcomingTrips = await _context.Trips
            .CountAsync(t => t.StartDate.Date > today && t.Status == TripStatus.Confirmed);

        var pendingBookings = await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Pending);

        var pendingPayments = await _context.Payments
            .CountAsync(p => p.Status == PaymentStatus.Pending);

        var openSupportRequests = await _context.SupportTickets
            .CountAsync(st => st.Status == SupportTicketStatus.New || st.Status == SupportTicketStatus.Open || st.Status == SupportTicketStatus.WaitingStaff);

        var openTasks = await _context.StaffTasks
            .CountAsync(st => st.Status == StaffTaskStatus.Open || st.Status == StaffTaskStatus.InProgress);

        // OPERATIONS (Today's Schedule)
        var todaySchedule = new List<OperationalScheduleItemDto>();

        // 1. Pickups & Transfers today
        var todayTransfers = await _context.Transportations
            .Include(t => t.Customer)
            .Include(t => t.Driver)
            .Where(t => t.ScheduledPickupTime.Date == today && t.Status != TransportationStatus.Cancelled)
            .OrderBy(t => t.ScheduledPickupTime)
            .Take(10)
            .ToListAsync();

        foreach (var transfer in todayTransfers)
        {
            todaySchedule.Add(new OperationalScheduleItemDto
            {
                Id = transfer.Id,
                Time = transfer.ScheduledPickupTime.ToString("HH:mm"),
                EventType = transfer.Type.ToString(),
                Title = $"{transfer.ServiceType}: {transfer.PickupLocation} -> {transfer.DropoffLocation}",
                CustomerName = transfer.Customer != null ? $"{transfer.Customer.FirstName} {transfer.Customer.LastName}" : "Traveler",
                Location = transfer.PickupLocation,
                AssignedPerson = transfer.Driver?.FullName ?? "Unassigned Driver",
                Status = transfer.Status.ToString()
            });
        }

        // 2. Activities today
        var todayActivities = await _context.Activities
            .Include(a => a.Guide)
            .Include(a => a.TripDay)
            .Where(a => a.TripDay != null && a.TripDay.Date == today)
            .OrderBy(a => a.StartTime)
            .Take(10)
            .ToListAsync();

        foreach (var act in todayActivities)
        {
            todaySchedule.Add(new OperationalScheduleItemDto
            {
                Id = act.Id,
                Time = act.StartTime.ToString(@"hh\:mm"),
                EventType = "Activity",
                Title = act.Title,
                CustomerName = "Group",
                Location = act.Location,
                AssignedPerson = act.Guide?.FullName ?? "Unassigned Guide",
                Status = "Scheduled"
            });
        }

        // ALERTS
        var alerts = new List<AdminAlertItemDto>();

        // Alert: Unassigned Transportations
        var unassignedTransfersCount = await _context.Transportations
            .CountAsync(t => t.DriverId == null && t.ScheduledPickupTime >= now && t.ScheduledPickupTime <= now.AddDays(2) && t.Status != TransportationStatus.Cancelled);
        if (unassignedTransfersCount > 0)
        {
            alerts.Add(new AdminAlertItemDto
            {
                AlertType = "UnassignedService",
                Severity = "High",
                Title = $"{unassignedTransfersCount} Unassigned Transfer(s)",
                Description = "Upcoming airport/hotel pickups within 48h require driver assignments.",
                TargetEntity = "Transportation",
                TargetId = Guid.Empty
            });
        }

        // Alert: Unassigned Guides
        var unassignedGuidesCount = await _context.Activities
            .CountAsync(a => a.GuideId == null && a.TripDay != null && a.TripDay.Date >= today && a.TripDay.Date <= today.AddDays(2));
        if (unassignedGuidesCount > 0)
        {
            alerts.Add(new AdminAlertItemDto
            {
                AlertType = "UnassignedService",
                Severity = "High",
                Title = $"{unassignedGuidesCount} Unassigned Tour Guide(s)",
                Description = "Tours and activities starting in the next 48h have no certified guide assigned.",
                TargetEntity = "Activity",
                TargetId = Guid.Empty
            });
        }

        // Alert: Missing Documents
        var unverifiedDocsCount = await _context.Documents
            .CountAsync(d => string.IsNullOrEmpty(d.PermitPassportNumber));
        if (unverifiedDocsCount > 0)
        {
            alerts.Add(new AdminAlertItemDto
            {
                AlertType = "MissingDocument",
                Severity = "Medium",
                Title = $"{unverifiedDocsCount} Pending Document(s)",
                Description = "Customer passports and permits awaiting government ticket assignment.",
                TargetEntity = "Document",
                TargetId = Guid.Empty
            });
        }

        // Alert: Pending Payments
        if (pendingPayments > 0)
        {
            alerts.Add(new AdminAlertItemDto
            {
                AlertType = "PendingPayment",
                Severity = "Medium",
                Title = $"{pendingPayments} Outstanding Payment(s)",
                Description = "Bookings awaiting deposit confirmation or full settlement.",
                TargetEntity = "Payment",
                TargetId = Guid.Empty
            });
        }

        // Alert: Urgent Support Requests
        var urgentTickets = await _context.SupportTickets
            .Where(st => (st.Priority == SupportTicketPriority.Urgent || st.Priority == SupportTicketPriority.High) &&
                         (st.Status == SupportTicketStatus.New || st.Status == SupportTicketStatus.Open))
            .Take(3)
            .ToListAsync();
        foreach (var ut in urgentTickets)
        {
            alerts.Add(new AdminAlertItemDto
            {
                AlertType = "UrgentSupport",
                Severity = "Critical",
                Title = $"Urgent: {ut.Subject}",
                Description = $"Open support request in category {ut.Category}.",
                TargetEntity = "SupportTicket",
                TargetId = ut.Id
            });
        }

        // RECENT ACTIVITY FEED
        var auditLogs = await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(12)
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

        var dashboard = new AdminDashboardV2Dto
        {
            CurrentAdminEmail = userEmail,
            CurrentAdminRole = userRole,
            UnreadNotificationsCount = unreadNotifications,
            ActiveTripsCount = activeTrips,
            UpcomingTripsCount = upcomingTrips,
            PendingBookingsCount = pendingBookings,
            PendingPaymentsCount = pendingPayments,
            OpenSupportRequestsCount = openSupportRequests,
            OpenStaffTasksCount = openTasks,
            TodayArrivalsCount = todayArrivals,
            TodayDeparturesCount = todayDepartures,
            TodaySchedule = todaySchedule,
            CriticalAlerts = alerts,
            RecentActivities = recentActivities
        };

        return Ok(ApiResponse<AdminDashboardV2Dto>.Ok(dashboard));
    }
}

