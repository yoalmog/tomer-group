using TomerGroup.Core.Enums;

namespace TomerGroup.Core.DTOs;

public class AdminDashboardMetricsDto
{
    public int TodayArrivals { get; set; }
    public int TodayDepartures { get; set; }
    public int ActiveTrips { get; set; }
    public int UpcomingTrips { get; set; }
    public int TotalCustomers { get; set; }
    public int ActiveInPeruCustomers { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int PendingPayments { get; set; }
    public int AssignedTransportationCount { get; set; }
    public int UnassignedTransportationCount { get; set; }
    public int AssignedGuidesCount { get; set; }
    public int UnassignedGuidesCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal MarginPercentage { get; set; }
    public List<AdminActivityFeedItemDto> RecentActivities { get; set; } = new();
}

public class AdminActivityFeedItemDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string RoleName => Role.ToString();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CreateAdminUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Operations;
}

public class UpdateAdminUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}

