namespace TomerGroup.Core.Models;

/// <summary>
/// Authoritative Role-Based Access Control (RBAC) permission constants for Tomer Group internal operations.
/// Backend endpoints strictly enforce these permissions.
/// </summary>
public static class Permissions
{
    // Customers
    public const string CustomersView = "CUSTOMERS_VIEW";
    public const string CustomersEdit = "CUSTOMERS_EDIT";

    // Trips & Itineraries
    public const string TripsView = "TRIPS_VIEW";
    public const string TripsEdit = "TRIPS_EDIT";

    // Bookings
    public const string BookingsView = "BOOKINGS_VIEW";
    public const string BookingsEdit = "BOOKINGS_EDIT";

    // Payments & Finance
    public const string PaymentsView = "PAYMENTS_VIEW";
    public const string PaymentsEdit = "PAYMENTS_EDIT";

    // Treks & Routes
    public const string TreksView = "TREKS_VIEW";
    public const string TreksEdit = "TREKS_EDIT";
    public const string TreksPublish = "TREKS_PUBLISH";

    // Documents
    public const string DocumentsView = "DOCUMENTS_VIEW";
    public const string DocumentsManage = "DOCUMENTS_MANAGE";

    // Support Center
    public const string SupportView = "SUPPORT_VIEW";
    public const string SupportManage = "SUPPORT_MANAGE";

    // Operational Tasks
    public const string TasksView = "TASKS_VIEW";
    public const string TasksManage = "TASKS_MANAGE";

    // Reports & Analytics
    public const string ReportsView = "REPORTS_VIEW";

    // Settings & Branding
    public const string SettingsManage = "SETTINGS_MANAGE";

    // Audit Log
    public const string AuditView = "AUDIT_VIEW";

    public static readonly IReadOnlyList<string> All = new[]
    {
        CustomersView, CustomersEdit,
        TripsView, TripsEdit,
        BookingsView, BookingsEdit,
        PaymentsView, PaymentsEdit,
        TreksView, TreksEdit, TreksPublish,
        DocumentsView, DocumentsManage,
        SupportView, SupportManage,
        TasksView, TasksManage,
        ReportsView, SettingsManage, AuditView
    };
}

