using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;

namespace TomerGroup.Core.Interfaces;

public interface IAuthenticationService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ICustomerService
{
    Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerSensitiveDetailsDto>> GetSensitiveDetailsAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<CustomerDto>>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetSummariesAsync(CustomerSearchFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerDto request, Guid creatorId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto request, Guid editorId, string editorRole = "Staff", CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> UpdateOwnProfileAsync(Guid userId, UpdateCustomerProfileDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerStatsDto>> GetTravelerStatsAsync(CancellationToken cancellationToken = default);
}

public interface ITripService
{
    Task<ApiResponse<TripDto>> GetByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<TripDto>>> GetByCustomerIdAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerHomeDashboardDto>> GetCustomerHomeDashboardAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<TripDto>>> GetAllTripsAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<ApiResponse<TripDto>> CreateAsync(CreateTripDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TripDto>> UpdateAsync(Guid id, UpdateTripDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TripDayDto>> AddTripDayAsync(Guid tripId, CreateTripDayDto request, Guid creatorId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ActivityDto>> AddActivityAsync(Guid tripDayId, CreateActivityDto request, Guid creatorId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ActivityDto>> UpdateActivityStatusAsync(Guid activityId, ActivityStatus status, Guid editorId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<DestinationDto>>> GetDestinationsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<DestinationDto>> CreateDestinationAsync(CreateDestinationDto request, Guid creatorId, CancellationToken cancellationToken = default);
}

public interface IBookingService
{
    Task<ApiResponse<BookingDto>> GetByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<BookingDetailedDto>> GetDetailedByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<BookingDto>>> GetByCustomerIdAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<BookingDto>>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<ApiResponse<BookingDto>> CreateAsync(CreateBookingDto request, CancellationToken cancellationToken = default);
}

public interface ITourService
{
    Task<ApiResponse<List<TourDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<TourDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<TourDto>> CreateAsync(CreateTourDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TourDto>> UpdateAsync(Guid id, UpdateTourDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IHotelService
{
    Task<ApiResponse<List<HotelDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<HotelDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<HotelDto>> CreateAsync(CreateHotelDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<HotelDto>> UpdateAsync(Guid id, UpdateHotelDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<HotelBookingDto>> CreateHotelBookingAsync(CreateHotelBookingDto request, Guid creatorId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<HotelBookingDto>>> GetCustomerHotelBookingsAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> UpdateHotelBookingStatusAsync(Guid hotelBookingId, string status, CancellationToken cancellationToken = default);
}

public interface ITransportationService
{
    Task<ApiResponse<List<TransportationDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<TransportationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<TransportationDto>> CreateAsync(CreateTransportationDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TransportationDto>> UpdateStatusAsync(Guid id, UpdateTransportationStatusDto request, Guid editorId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<TransportationDto>>> GetCustomerTransfersAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<VehicleDto>>> GetVehiclesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<VehicleDto>> CreateVehicleAsync(CreateVehicleDto request, CancellationToken cancellationToken = default);
}

public interface IDriverService
{
    Task<ApiResponse<List<DriverDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<DriverDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<DriverDto>> CreateAsync(CreateDriverDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<DriverDto>> UpdateAsync(Guid id, UpdateDriverDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<DriverDailyManifestDto>> GetDailyManifestAsync(Guid driverId, DateTime date, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> AssignDriverToTransferAsync(AssignDriverToTransferDto request, Guid requestingUserId, CancellationToken cancellationToken = default);
}

public interface IGuideService
{
    Task<ApiResponse<List<GuideDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GuideDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GuideDto>> CreateAsync(CreateGuideDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GuideDto>> UpdateAsync(Guid id, UpdateGuideDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GuideDailyManifestDto>> GetDailyManifestAsync(Guid guideId, DateTime date, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> AssignGuideToActivityAsync(AssignGuideToActivityDto request, Guid requestingUserId, CancellationToken cancellationToken = default);
}

public interface IPaymentService
{
    Task<ApiResponse<PaymentDto>> RecordPaymentAsync(RecordPaymentDto request, Guid recordedByUserId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PaymentDto>>> GetByBookingIdAsync(Guid bookingId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PaymentDto>>> GetCustomerPaymentsAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<AgencyFinancialSummaryDto>> GetFinancialSummaryAsync(CancellationToken cancellationToken = default);
}

public interface IExpenseService
{
    Task<ApiResponse<ExpenseDto>> RecordExpenseAsync(RecordExpenseDto request, Guid recordedByUserId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ExpenseDto>>> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default);
    Task<ApiResponse<TripProfitabilityDto>> GetTripProfitabilityAsync(Guid tripId, CancellationToken cancellationToken = default);
}

public interface IDocumentService
{
    Task<ApiResponse<DocumentDto>> UploadAsync(UploadDocumentDto request, Guid uploadedByUserId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<DocumentDto>>> GetCustomerDocumentsAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<DocumentDto>> GetByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<DocumentContentDto>> GetContentAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto request, CancellationToken cancellationToken = default);
}

public interface IWhatsAppService
{
    string SanitizePhoneNumber(string phone, string defaultCountryCode = "+972");
    string GenerateWhatsAppUrl(string phone, string message);
    string BuildTemplatedMessage(string templateKey, Dictionary<string, string> parameters, string language = "he");
    Task<ApiResponse<WhatsAppDispatchResultDto>> PrepareOrSendMessageAsync(SendWhatsAppMessageDto request, Guid? staffUserId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<MessageTemplateDto>>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<MessageDto>>> GetMessageHistoryAsync(Guid? customerId = null, CancellationToken cancellationToken = default);
}

public interface IMapService
{
    string GetMapNavigationUrl(double latitude, double longitude, string label);
}

public interface IAIService
{
    Task<ApiResponse<ItineraryDraftResultDto>> GenerateItineraryDraftAsync(GenerateItineraryPromptDto prompt, Guid staffUserId, CancellationToken cancellationToken = default);
    Task<ApiResponse<WhatsAppDraftResultDto>> DraftWhatsAppMessageAsync(DraftWhatsAppPromptDto prompt, Guid staffUserId, CancellationToken cancellationToken = default);
    Task<ApiResponse<AIRequestDto>> GetRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<AIRequestDto>>> GetRequestsAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ApproveRequestAsync(Guid id, Guid approvedByUserId, ApproveAIRequestDto? approval = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> RejectRequestAsync(Guid id, Guid rejectedByUserId, string? reason = null, CancellationToken cancellationToken = default);
}

public interface ISyncService
{
    Task<ApiResponse<SyncPackageDto>> PullDeltaPackageAsync(SyncPullRequestDto request, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<SyncPushResultDto>> PushClientChangesAsync(SyncPushRequestDto request, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SyncOfflineDataAsync(Guid customerId, CancellationToken cancellationToken = default);
}

public interface IReportService
{
    Task<ApiResponse<ExecutiveAnalyticsReportDto>> GetExecutiveAnalyticsAsync(DateRangeFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<ExportReportResultDto>> ExportReportAsync(DateRangeFilterDto? filter = null, string format = "csv", CancellationToken cancellationToken = default);
}

public interface IBrandingService
{
    Task<BrandSettings> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<BrandSettings> UpdateSettingsAsync(BrandSettings newSettings, CancellationToken cancellationToken = default);
}

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string? entityId, Guid? userId, string userEmail, string? metadata = null);
}

public interface ILocalizationService
{
    string GetString(string key, string? language = null);
    string CurrentLanguage { get; }
    bool IsRightToLeft { get; }
    void SetLanguage(string language);
}

public interface IRateLimitingService
{
    RateLimitStatusDto CheckRateLimit(string clientKey, string category = "General");
    void ResetLimit(string clientKey);
}

public interface ISecuritySanitizerService
{
    string MaskPassport(string? passport);
    string MaskIsraelId(string? israelId);
    string MaskCreditCard(string? cc);
    string SanitizeInput(string? input);
    bool IsPathTraversalSafe(string path);
}

public interface ISecurityAuditService
{
    Task<ApiResponse<PagedResult<AuditLogSummaryDto>>> GetAuditLogsAsync(AuditLogFilterDto filter, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
}

