using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;

namespace TomerGroup.Core.DTOs;

public class AdminActivityFeedItemDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
}

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
    public double MarginPercentage { get; set; }
    public List<AdminActivityFeedItemDto> RecentActivities { get; set; } = new();
}

#region Global Search DTOs

public class AdminSearchResultDto
{
    public string Query { get; set; } = string.Empty;
    public int TotalMatches => Customers.Count + Bookings.Count + Trips.Count + Treks.Count + Guides.Count + Drivers.Count + Hotels.Count + SupportTickets.Count;

    public List<AdminSearchItemDto> Customers { get; set; } = new();
    public List<AdminSearchItemDto> Bookings { get; set; } = new();
    public List<AdminSearchItemDto> Trips { get; set; } = new();
    public List<AdminSearchItemDto> Treks { get; set; } = new();
    public List<AdminSearchItemDto> Guides { get; set; } = new();
    public List<AdminSearchItemDto> Drivers { get; set; } = new();
    public List<AdminSearchItemDto> Hotels { get; set; } = new();
    public List<AdminSearchItemDto> SupportTickets { get; set; } = new();
}

public class AdminSearchItemDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ExtraInfo { get; set; } = string.Empty;
}

#endregion

#region 360 Customer Profile DTOs

public class Customer360Dto
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "he";
    public bool IsActiveInPeru { get; set; }
    public string? Notes { get; set; }

    // Tab 1: Overview Summary
    public int TotalTrips { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal PendingBalance { get; set; }
    public int DocumentsCount { get; set; }
    public int VerifiedDocumentsCount { get; set; }
    public int OpenSupportRequests { get; set; }

    // Tab 2: Trips
    public List<TripSummaryDto> Trips { get; set; } = new();

    // Tab 3: Bookings
    public List<BookingSummaryDto> Bookings { get; set; } = new();

    // Tab 4: Documents
    public List<DocumentItemDto> Documents { get; set; } = new();

    // Tab 5: Payments
    public List<PaymentItemDto> Payments { get; set; } = new();

    // Tab 6: Support & Messages
    public List<SupportTicketSummaryDto> SupportTickets { get; set; } = new();

    // Tab 7: Recent Activity / Staff Audit Log
    public List<AdminActivityFeedItemDto> ActivityLog { get; set; } = new();
}

public class TripSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Destinations { get; set; } = string.Empty;
    public int TravelersCount { get; set; }
}

public class BookingSummaryDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string TourOrTripName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}

public class DocumentItemDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Verified, Rejected
    public DateTime UploadedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class PaymentItemDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class SupportTicketSummaryDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MessagesCount { get; set; }
}

#endregion

#region Operations Dashboard V2 DTOs

public class AdminDashboardV2Dto
{
    // TOP
    public string CurrentAdminEmail { get; set; } = string.Empty;
    public string CurrentAdminRole { get; set; } = string.Empty;
    public int UnreadNotificationsCount { get; set; }

    // SUMMARY
    public int ActiveTripsCount { get; set; }
    public int UpcomingTripsCount { get; set; }
    public int PendingBookingsCount { get; set; }
    public int PendingPaymentsCount { get; set; }
    public int OpenSupportRequestsCount { get; set; }
    public int OpenStaffTasksCount { get; set; }
    public int TodayArrivalsCount { get; set; }
    public int TodayDeparturesCount { get; set; }

    // OPERATIONS (Today's Schedule)
    public List<OperationalScheduleItemDto> TodaySchedule { get; set; } = new();

    // ALERTS
    public List<AdminAlertItemDto> CriticalAlerts { get; set; } = new();

    // RECENT ACTIVITY FEED
    public List<AdminActivityFeedItemDto> RecentActivities { get; set; } = new();
}

public class OperationalScheduleItemDto
{
    public Guid Id { get; set; }
    public string Time { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // Pickup, Tour, Trek, CheckIn, Departure
    public string Title { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? AssignedPerson { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AdminAlertItemDto
{
    public string AlertType { get; set; } = string.Empty; // MissingDocument, PendingPayment, UnassignedService, UrgentSupport
    public string Severity { get; set; } = "High"; // Low, Medium, High, Critical
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
}

#endregion

#region Calendar DTOs

public class OperationalCalendarEventDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string EventType { get; set; } = string.Empty; // "Pickup", "Dropoff", "Trek", "HotelCheckIn", "HotelCheckOut", "Activity"
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? GuideName { get; set; }
    public string? DriverName { get; set; }
    public string Status { get; set; } = "Scheduled";
}

#endregion

#region Support & Tasks DTOs

public class CreateSupportTicketDto
{
    public Guid CustomerId { get; set; }
    public Guid? TripId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public SupportCategory Category { get; set; } = SupportCategory.General;
    public SupportTicketPriority Priority { get; set; } = SupportTicketPriority.Medium;
    public string InitialMessage { get; set; } = string.Empty;
}

public class SupportTicketReplyDto
{
    public string MessageText { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; } = false;
}

public class UpdateSupportTicketStatusDto
{
    public SupportTicketStatus Status { get; set; }
    public SupportTicketPriority? Priority { get; set; }
    public Guid? AssignedStaffUserId { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class CreateStaffTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid? TripId { get; set; }
    public DateTime DueDate { get; set; }
    public StaffTaskPriority Priority { get; set; } = StaffTaskPriority.Medium;
    public Guid? AssignedStaffUserId { get; set; }
}

public class UpdateStaffTaskStatusDto
{
    public StaffTaskStatus Status { get; set; }
    public string? CompletionNotes { get; set; }
}

#endregion

#region Trek Map & Route DTOs

public class SaveTrekRouteDto
{
    public Guid? Id { get; set; }
    public Guid? TourId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = "Cusco, Peru";
    public string Difficulty { get; set; } = "Challenging";
    public int DurationDays { get; set; } = 4;
    public double DistanceKm { get; set; }
    public int MaxElevationMeters { get; set; }
    public string CoordinatesJson { get; set; } = "[]";
    public string WaypointsJson { get; set; } = "[]";
    public string ElevationProfileJson { get; set; } = "[]";
    public string? OriginalGpxContent { get; set; }
    public bool IsPublished { get; set; } = false;
}

public class GpxImportResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string TrackName { get; set; } = string.Empty;
    public double TotalDistanceKm { get; set; }
    public int MaxElevationMeters { get; set; }
    public int MinElevationMeters { get; set; }
    public int TotalAscentMeters { get; set; }
    public int CoordinatePointsCount { get; set; }
    public int WaypointsCount { get; set; }
    public string CoordinatesJson { get; set; } = "[]";
    public string WaypointsJson { get; set; } = "[]";
    public string ElevationProfileJson { get; set; } = "[]";
}

#endregion

#region Booking Management DTOs

public class CreateBookingAdminDto
{
    public Guid CustomerId { get; set; }
    public Guid? TourId { get; set; }
    public Guid? TripId { get; set; }
    public DateTime BookingDate { get; set; }
    public int TravelersCount { get; set; } = 1;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Confirmed";
    public string? InternalNotes { get; set; }
}

public class UpdateBookingAdminDto
{
    public string Status { get; set; } = "Confirmed"; // Pending, Confirmed, Cancelled, Completed
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime BookingDate { get; set; }
    public int TravelersCount { get; set; }
    public string? InternalNotes { get; set; }
}

#endregion

#region Admin Staff User DTOs

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CreateAdminUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.Operations;
}

public class UpdateAdminUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}

#endregion

