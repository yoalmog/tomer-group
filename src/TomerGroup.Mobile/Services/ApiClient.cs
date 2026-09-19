using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;

namespace TomerGroup.Mobile.Services;

public interface IApiClient
{
    void SetAuthToken(string? token);
    bool IsAuthenticated { get; }
    Task<ApiResponse<LoginResponseDto>> LoginAsync(string email, string password);
    Task<ApiResponse<LoginResponseDto>> RegisterCustomerAsync(CustomerRegisterRequestDto request);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(Guid userId, string refreshToken);
    Task<ApiResponse<bool>> SendPhoneCodeAsync(string phoneNumber);
    Task<ApiResponse<LoginResponseDto>> VerifyPhoneCodeAsync(string phoneNumber, string code);
    Task<ApiResponse<string>> ForgotPasswordAsync(string email);
    Task<ApiResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword);
    Task<ApiResponse<HealthStatusDto>> GetHealthAsync();
    Task<ApiResponse<BrandSettings>> GetBrandingAsync();
    Task<ApiResponse<List<TripDto>>> GetCustomerTripsAsync(Guid customerId);
    Task<ApiResponse<List<BookingDto>>> GetCustomerBookingsAsync(Guid customerId);
    Task<ApiResponse<List<TourDto>>> GetToursAsync();

    // Customer & Profile management
    Task<ApiResponse<CustomerDto>> GetMyProfileAsync();
    Task<ApiResponse<CustomerDto>> UpdateMyProfileAsync(UpdateCustomerProfileDto request);
    Task<ApiResponse<CustomerSensitiveDetailsDto>> GetSensitiveDetailsAsync(Guid customerId);
    Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(Guid customerId);
    Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetAgencyCustomersAsync(CustomerSearchFilterDto filter);
    Task<ApiResponse<CustomerStatsDto>> GetCustomerStatsAsync();

    // Trips, Itineraries & Destinations
    Task<ApiResponse<CustomerHomeDashboardDto>> GetCustomerDashboardAsync(Guid customerId);
    Task<ApiResponse<TripDto>> GetTripDetailsAsync(Guid tripId);
    Task<ApiResponse<List<DestinationDto>>> GetDestinationsAsync();
    Task<ApiResponse<TripDayDto>> AddTripDayAsync(Guid tripId, CreateTripDayDto request);
    Task<ApiResponse<ActivityDto>> AddActivityAsync(Guid dayId, CreateActivityDto request);
    Task<ApiResponse<ActivityDto>> UpdateActivityStatusAsync(Guid activityId, ActivityStatus status);

    // Phase 5: Tours, Hotels, Transportation, Detailed Bookings
    Task<ApiResponse<TourDto>> GetTourByIdAsync(Guid id);
    Task<ApiResponse<List<HotelDto>>> GetHotelsAsync();
    Task<ApiResponse<HotelDto>> GetHotelByIdAsync(Guid id);
    Task<ApiResponse<HotelBookingDto>> CreateHotelBookingAsync(CreateHotelBookingDto request);
    Task<ApiResponse<List<HotelBookingDto>>> GetCustomerHotelBookingsAsync(Guid customerId);
    Task<ApiResponse<List<TransportationDto>>> GetCustomerTransfersAsync(Guid customerId);
    Task<ApiResponse<List<TransportationDto>>> GetTransportationListAsync();
    Task<ApiResponse<BookingDetailedDto>> GetDetailedBookingByIdAsync(Guid id);

    // Phase 6: Guides, Drivers, and Manifests
    Task<ApiResponse<List<GuideDto>>> GetGuidesAsync();
    Task<ApiResponse<GuideDto>> GetGuideByIdAsync(Guid id);
    Task<ApiResponse<GuideDailyManifestDto>> GetGuideManifestAsync(Guid guideId, DateTime date);
    Task<ApiResponse<List<DriverDto>>> GetDriversAsync();
    Task<ApiResponse<DriverDto>> GetDriverByIdAsync(Guid id);
    Task<ApiResponse<DriverDailyManifestDto>> GetDriverManifestAsync(Guid driverId, DateTime date);
    Task<ApiResponse<bool>> AssignGuideAsync(AssignGuideToActivityDto request);
    Task<ApiResponse<bool>> AssignDriverAsync(AssignDriverToTransferDto request);

    // Phase 7: Payments, Expenses, and Profitability
    Task<ApiResponse<PaymentDto>> RecordPaymentAsync(RecordPaymentDto request);
    Task<ApiResponse<List<PaymentDto>>> GetCustomerPaymentsAsync(Guid customerId);
    Task<ApiResponse<AgencyFinancialSummaryDto>> GetFinancialSummaryAsync();
    Task<ApiResponse<ExpenseDto>> RecordExpenseAsync(RecordExpenseDto request);
    Task<ApiResponse<TripProfitabilityDto>> GetTripProfitabilityAsync(Guid tripId);

    // Phase 8: Documents, Permits, and Vouchers
    Task<ApiResponse<List<DocumentDto>>> GetCustomerDocumentsAsync(Guid customerId);
    Task<ApiResponse<DocumentDto>> GetDocumentByIdAsync(Guid id);
    Task<ApiResponse<DocumentDto>> UploadDocumentAsync(UploadDocumentDto request);
    Task<ApiResponse<bool>> DeleteDocumentAsync(Guid id);

    // Phase 9: Notifications & WhatsApp System
    Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync(bool unreadOnly = false);
    Task<ApiResponse<int>> GetUnreadNotificationCountAsync();
    Task<ApiResponse<bool>> MarkNotificationAsReadAsync(Guid id);
    Task<ApiResponse<bool>> MarkAllNotificationsAsReadAsync();
    Task<ApiResponse<List<MessageTemplateDto>>> GetWhatsAppTemplatesAsync();
    Task<ApiResponse<WhatsAppDispatchResultDto>> SendWhatsAppMessageAsync(SendWhatsAppMessageDto request);
    Task<ApiResponse<string>> GenerateWhatsAppUrlAsync(string phone, string message);

    // Phase 10: AI Assistant Engine
    Task<ApiResponse<ItineraryDraftResultDto>> GenerateItineraryDraftAsync(GenerateItineraryPromptDto prompt);
    Task<ApiResponse<WhatsAppDraftResultDto>> DraftAIWhatsAppMessageAsync(DraftWhatsAppPromptDto prompt);
    Task<ApiResponse<List<AIRequestDto>>> GetAIRequestsAsync(string? status = null);
    Task<ApiResponse<bool>> ApproveAIRequestAsync(Guid id, string? notes = null);

    // Phase 11: Offline Sync System
    Task<ApiResponse<SyncPackageDto>> PullDeltaSyncPackageAsync(SyncPullRequestDto request);
    Task<ApiResponse<SyncPushResultDto>> PushOfflineChangesAsync(SyncPushRequestDto request);
    Task<bool> CheckServerConnectivityAsync();

    // Phase 12: Reports & Analytics
    Task<ApiResponse<ExecutiveAnalyticsReportDto>> GetExecutiveAnalyticsAsync(DateRangeFilterDto? filter = null);
    Task<ApiResponse<ExportReportResultDto>> ExportExecutiveReportAsync(string format = "csv", DateRangeFilterDto? filter = null);

    // Admin Real Backend API
    Task<ApiResponse<AdminDashboardMetricsDto>> GetAdminDashboardMetricsAsync();
    Task<ApiResponse<List<AdminUserDto>>> GetAdminUsersAsync(UserRole? role = null, string? search = null);
    Task<ApiResponse<AdminUserDto>> GetAdminUserByIdAsync(Guid id);
    Task<ApiResponse<AdminUserDto>> CreateAdminUserAsync(CreateAdminUserDto request);
    Task<ApiResponse<AdminUserDto>> UpdateAdminUserAsync(Guid id, UpdateAdminUserDto request);
    // Operations Platform V2
    Task<ApiResponse<AdminDashboardV2Dto>> GetOperationsDashboardV2Async();
    Task<ApiResponse<AdminSearchResultDto>> SearchGlobalAsync(string query);
    Task<ApiResponse<Customer360Dto>> GetCustomer360Async(Guid customerId);
    Task<ApiResponse<List<TrekRouteEntity>>> GetAdminTreksAsync();
    Task<ApiResponse<TrekRouteEntity>> GetAdminTrekByIdAsync(Guid id);
    Task<ApiResponse<TrekRouteEntity>> SaveTrekRouteAsync(SaveTrekRouteDto dto);
    Task<ApiResponse<TrekRouteEntity>> UpdateTrekRouteAsync(Guid id, SaveTrekRouteDto dto);
    Task<ApiResponse<TrekRouteEntity>> PublishTrekRouteAsync(Guid id);
    Task<ApiResponse<TrekRouteEntity>> UnpublishTrekRouteAsync(Guid id);
    Task<ApiResponse<GpxImportResultDto>> ImportGpxAsync(string gpxContent);
    Task<ApiResponse<List<OperationalCalendarEventDto>>> GetOperationalCalendarAsync(DateTime? start = null, DateTime? end = null, string? filter = null);
    Task<ApiResponse<List<SupportTicketSummaryDto>>> GetSupportTicketsAsync(SupportTicketStatus? status = null, SupportTicketPriority? priority = null);
    Task<ApiResponse<SupportTicket>> GetSupportTicketByIdAsync(Guid id);
    Task<ApiResponse<SupportTicket>> CreateSupportTicketAsync(CreateSupportTicketDto dto);
    Task<ApiResponse<SupportTicketMessage>> ReplyToSupportTicketAsync(Guid id, SupportTicketReplyDto dto);
    Task<ApiResponse<SupportTicket>> UpdateSupportTicketStatusAsync(Guid id, UpdateSupportTicketStatusDto dto);
    Task<ApiResponse<List<StaffTask>>> GetStaffTasksAsync(StaffTaskStatus? status = null, bool dueSoonOnly = false);
    Task<ApiResponse<StaffTask>> CreateStaffTaskAsync(CreateStaffTaskDto dto);
    Task<ApiResponse<StaffTask>> UpdateStaffTaskStatusAsync(Guid id, UpdateStaffTaskStatusDto dto);
    Task<ApiResponse<Booking>> UpdateBookingAdminAsync(Guid id, UpdateBookingAdminDto dto);
    Task<ApiResponse<Payment>> RecordBookingPaymentAsync(Guid id, PaymentItemDto dto);
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private string? _authToken;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);

    public void SetAuthToken(string? token)
    {
        _authToken = token;
        if (string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginRequestDto { Email = email, Password = password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
                if (result?.Data?.Token != null)
                {
                    SetAuthToken(result.Data.Token);
                }
                return result ?? ApiResponse<LoginResponseDto>.Fail("Empty response from server");
            }

            var error = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
            return error ?? ApiResponse<LoginResponseDto>.Fail($"Login failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponseDto>.Fail($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> RegisterCustomerAsync(CustomerRegisterRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
                if (result?.Data?.Token != null)
                {
                    SetAuthToken(result.Data.Token);
                }
                return result ?? ApiResponse<LoginResponseDto>.Fail("Empty response from server");
            }

            var error = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
            return error ?? ApiResponse<LoginResponseDto>.Fail(error?.Message ?? $"Registration failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponseDto>.Fail($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(Guid userId, string refreshToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequestDto { UserId = userId, RefreshToken = refreshToken });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
                if (result?.Data?.Token != null)
                {
                    SetAuthToken(result.Data.Token);
                }
                return result ?? ApiResponse<LoginResponseDto>.Fail("Empty refresh response");
            }

            return ApiResponse<LoginResponseDto>.Fail("Session renewal failed. Please log in again.");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponseDto>.Fail($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> SendPhoneCodeAsync(string phoneNumber)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/phone/send-code", new SendPhoneCodeRequestDto { PhoneNumber = phoneNumber });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Failed to send phone code");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LoginResponseDto>> VerifyPhoneCodeAsync(string phoneNumber, string code)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/phone/verify-code", new VerifyPhoneCodeRequestDto { PhoneNumber = phoneNumber, VerificationCode = code });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
                if (result?.Data?.Token != null)
                {
                    SetAuthToken(result.Data.Token);
                }
                return result ?? ApiResponse<LoginResponseDto>.Fail("Empty response from server");
            }

            var error = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
            return error ?? ApiResponse<LoginResponseDto>.Fail("Verification failed");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponseDto>.Fail($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(string email)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new ForgotPasswordRequestDto { Email = email });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Request failed");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", new ResetPasswordRequestDto
            {
                Email = email,
                ResetToken = token,
                NewPassword = newPassword
            });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Password reset failed");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Connection error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HealthStatusDto>> GetHealthAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<HealthStatusDto>>("api/health");
            return result ?? ApiResponse<HealthStatusDto>.Fail("Empty health response");
        }
        catch (Exception ex)
        {
            return ApiResponse<HealthStatusDto>.Fail($"Health check failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BrandSettings>> GetBrandingAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<BrandSettings>>("api/settings/branding");
            return result ?? ApiResponse<BrandSettings>.Fail("Failed to load branding");
        }
        catch
        {
            return ApiResponse<BrandSettings>.Ok(BrandSettings.CreateDefault());
        }
    }

    public async Task<ApiResponse<List<TripDto>>> GetCustomerTripsAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<TripDto>>>($"api/trips/customer/{customerId}");
            return result ?? ApiResponse<List<TripDto>>.Fail("No trips found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TripDto>>.Fail($"Could not load trips: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<BookingDto>>> GetCustomerBookingsAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<BookingDto>>>($"api/bookings/customer/{customerId}");
            return result ?? ApiResponse<List<BookingDto>>.Fail("No bookings found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<BookingDto>>.Fail($"Could not load bookings: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TourDto>>> GetToursAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<TourDto>>>("api/tours");
            return result ?? ApiResponse<List<TourDto>>.Fail("No tours found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TourDto>>.Fail($"Could not load tours: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerDto>> GetMyProfileAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerDto>>("api/customers/me");
            return result ?? ApiResponse<CustomerDto>.Fail("Customer profile not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<CustomerDto>.Fail($"Failed to load profile: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerDto>> UpdateMyProfileAsync(UpdateCustomerProfileDto request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("api/customers/me", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
            return result ?? ApiResponse<CustomerDto>.Fail("Failed to update profile");
        }
        catch (Exception ex)
        {
            return ApiResponse<CustomerDto>.Fail($"Error updating profile: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerSensitiveDetailsDto>> GetSensitiveDetailsAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerSensitiveDetailsDto>>($"api/customers/{customerId}/sensitive");
            return result ?? ApiResponse<CustomerSensitiveDetailsDto>.Fail("Could not fetch sensitive details");
        }
        catch (Exception ex)
        {
            return ApiResponse<CustomerSensitiveDetailsDto>.Fail($"Failed to retrieve sensitive details: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerDto>>($"api/customers/{customerId}");
            return result ?? ApiResponse<CustomerDto>.Fail("Customer not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<CustomerDto>.Fail($"Error loading customer: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetAgencyCustomersAsync(CustomerSearchFilterDto filter)
    {
        try
        {
            var query = $"api/customers/summaries?Page={filter.Page}&PageSize={filter.PageSize}";
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query += $"&Search={Uri.EscapeDataString(filter.Search)}";
            if (!string.IsNullOrWhiteSpace(filter.Country))
                query += $"&Country={Uri.EscapeDataString(filter.Country)}";
            if (!string.IsNullOrWhiteSpace(filter.DietaryPreference))
                query += $"&DietaryPreference={Uri.EscapeDataString(filter.DietaryPreference)}";
            if (filter.IsActiveInPeru.HasValue)
                query += $"&IsActiveInPeru={filter.IsActiveInPeru.Value}";
            if (filter.HasExpiringPassport.HasValue)
                query += $"&HasExpiringPassport={filter.HasExpiringPassport.Value}";

            var result = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<CustomerSummaryDto>>>(query);
            return result ?? ApiResponse<PagedResult<CustomerSummaryDto>>.Fail("No travelers found");
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<CustomerSummaryDto>>.Fail($"Error fetching travelers: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerStatsDto>> GetCustomerStatsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerStatsDto>>("api/customers/stats");
            return result ?? ApiResponse<CustomerStatsDto>.Fail("Could not load traveler stats");
        }
        catch (Exception ex)
        {
            return ApiResponse<CustomerStatsDto>.Fail($"Error loading stats: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CustomerHomeDashboardDto>> GetCustomerDashboardAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<CustomerHomeDashboardDto>>($"api/trips/customer/{customerId}/dashboard");
            return result ?? ApiResponse<CustomerHomeDashboardDto>.Fail("Could not load trip dashboard");
        }
        catch (Exception ex)
        {
            return ApiResponse<CustomerHomeDashboardDto>.Fail($"Error loading dashboard: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TripDto>> GetTripDetailsAsync(Guid tripId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<TripDto>>($"api/trips/{tripId}");
            return result ?? ApiResponse<TripDto>.Fail("Could not load trip details");
        }
        catch (Exception ex)
        {
            return ApiResponse<TripDto>.Fail($"Error loading trip: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<DestinationDto>>> GetDestinationsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<DestinationDto>>>("api/trips/destinations");
            return result ?? ApiResponse<List<DestinationDto>>.Fail("Could not load destinations");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<DestinationDto>>.Fail($"Error loading destinations: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TripDayDto>> AddTripDayAsync(Guid tripId, CreateTripDayDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/trips/{tripId}/days", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TripDayDto>>();
            return result ?? ApiResponse<TripDayDto>.Fail("Failed to add trip day");
        }
        catch (Exception ex)
        {
            return ApiResponse<TripDayDto>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ActivityDto>> AddActivityAsync(Guid dayId, CreateActivityDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/trips/days/{dayId}/activities", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ActivityDto>>();
            return result ?? ApiResponse<ActivityDto>.Fail("Failed to add activity");
        }
        catch (Exception ex)
        {
            return ApiResponse<ActivityDto>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ActivityDto>> UpdateActivityStatusAsync(Guid activityId, ActivityStatus status)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/trips/activities/{activityId}/status", new UpdateActivityStatusDto { Status = status });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ActivityDto>>();
            return result ?? ApiResponse<ActivityDto>.Fail("Failed to update activity status");
        }
        catch (Exception ex)
        {
            return ApiResponse<ActivityDto>.Fail($"Error: {ex.Message}");
        }
    }

    // Phase 5: Tours, Hotels, Transportation
    public async Task<ApiResponse<TourDto>> GetTourByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<TourDto>>($"api/tours/{id}");
            return result ?? ApiResponse<TourDto>.Fail("Tour not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<TourDto>.Fail($"Error loading tour: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<HotelDto>>> GetHotelsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<HotelDto>>>("api/hotels");
            return result ?? ApiResponse<List<HotelDto>>.Fail("No hotels found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<HotelDto>>.Fail($"Error loading hotels: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HotelDto>> GetHotelByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<HotelDto>>($"api/hotels/{id}");
            return result ?? ApiResponse<HotelDto>.Fail("Hotel not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<HotelDto>.Fail($"Error loading hotel: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HotelBookingDto>> CreateHotelBookingAsync(CreateHotelBookingDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/hotels/bookings", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<HotelBookingDto>>();
            return result ?? ApiResponse<HotelBookingDto>.Fail("Failed to create hotel booking");
        }
        catch (Exception ex)
        {
            return ApiResponse<HotelBookingDto>.Fail($"Error creating hotel booking: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<HotelBookingDto>>> GetCustomerHotelBookingsAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<HotelBookingDto>>>($"api/hotels/bookings/customer/{customerId}");
            return result ?? ApiResponse<List<HotelBookingDto>>.Fail("No hotel bookings found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<HotelBookingDto>>.Fail($"Error loading hotel bookings: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TransportationDto>>> GetCustomerTransfersAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<TransportationDto>>>($"api/transportation/customer/{customerId}");
            return result ?? ApiResponse<List<TransportationDto>>.Fail("No transfers found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TransportationDto>>.Fail($"Error loading transfers: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TransportationDto>>> GetTransportationListAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<TransportationDto>>>("api/transportation");
            return result ?? ApiResponse<List<TransportationDto>>.Fail("No transportation records found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TransportationDto>>.Fail($"Error loading transportation: {ex.Message}");
        }
    }

    public async Task<ApiResponse<BookingDetailedDto>> GetDetailedBookingByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<BookingDetailedDto>>($"api/bookings/{id}/detailed");
            return result ?? ApiResponse<BookingDetailedDto>.Fail("Booking not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<BookingDetailedDto>.Fail($"Error loading booking: {ex.Message}");
        }
    }

    // Phase 6: Guides, Drivers, and Manifests
    public async Task<ApiResponse<List<GuideDto>>> GetGuidesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<GuideDto>>>("api/guides");
            return result ?? ApiResponse<List<GuideDto>>.Fail("No guides found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<GuideDto>>.Fail($"Error loading guides: {ex.Message}");
        }
    }

    public async Task<ApiResponse<GuideDto>> GetGuideByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<GuideDto>>($"api/guides/{id}");
            return result ?? ApiResponse<GuideDto>.Fail("Guide not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<GuideDto>.Fail($"Error loading guide: {ex.Message}");
        }
    }

    public async Task<ApiResponse<GuideDailyManifestDto>> GetGuideManifestAsync(Guid guideId, DateTime date)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<GuideDailyManifestDto>>($"api/guides/{guideId}/manifest?date={date:yyyy-MM-dd}");
            return result ?? ApiResponse<GuideDailyManifestDto>.Fail("No manifest found");
        }
        catch (Exception ex)
        {
            return ApiResponse<GuideDailyManifestDto>.Fail($"Error loading guide manifest: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<DriverDto>>> GetDriversAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<DriverDto>>>("api/drivers");
            return result ?? ApiResponse<List<DriverDto>>.Fail("No drivers found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<DriverDto>>.Fail($"Error loading drivers: {ex.Message}");
        }
    }

    public async Task<ApiResponse<DriverDto>> GetDriverByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<DriverDto>>($"api/drivers/{id}");
            return result ?? ApiResponse<DriverDto>.Fail("Driver not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<DriverDto>.Fail($"Error loading driver: {ex.Message}");
        }
    }

    public async Task<ApiResponse<DriverDailyManifestDto>> GetDriverManifestAsync(Guid driverId, DateTime date)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<DriverDailyManifestDto>>($"api/drivers/{driverId}/manifest?date={date:yyyy-MM-dd}");
            return result ?? ApiResponse<DriverDailyManifestDto>.Fail("No driver manifest found");
        }
        catch (Exception ex)
        {
            return ApiResponse<DriverDailyManifestDto>.Fail($"Error loading driver manifest: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> AssignGuideAsync(AssignGuideToActivityDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/guides/assign", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Failed to assign guide");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Error assigning guide: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> AssignDriverAsync(AssignDriverToTransferDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/drivers/assign", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Failed to assign driver");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Error assigning driver: {ex.Message}");
        }
    }

    // Phase 7: Payments, Expenses, and Profitability
    public async Task<ApiResponse<PaymentDto>> RecordPaymentAsync(RecordPaymentDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/payments", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentDto>>();
            return result ?? ApiResponse<PaymentDto>.Fail("Failed to record payment");
        }
        catch (Exception ex)
        {
            return ApiResponse<PaymentDto>.Fail($"Error recording payment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<PaymentDto>>> GetCustomerPaymentsAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<PaymentDto>>>($"api/payments/customer/{customerId}");
            return result ?? ApiResponse<List<PaymentDto>>.Fail("No payments found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PaymentDto>>.Fail($"Error loading payments: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AgencyFinancialSummaryDto>> GetFinancialSummaryAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<AgencyFinancialSummaryDto>>("api/payments/summary");
            return result ?? ApiResponse<AgencyFinancialSummaryDto>.Fail("No financial summary found");
        }
        catch (Exception ex)
        {
            return ApiResponse<AgencyFinancialSummaryDto>.Fail($"Error loading summary: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ExpenseDto>> RecordExpenseAsync(RecordExpenseDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/expenses", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ExpenseDto>>();
            return result ?? ApiResponse<ExpenseDto>.Fail("Failed to record expense");
        }
        catch (Exception ex)
        {
            return ApiResponse<ExpenseDto>.Fail($"Error recording expense: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TripProfitabilityDto>> GetTripProfitabilityAsync(Guid tripId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<TripProfitabilityDto>>($"api/expenses/trip/{tripId}/profitability");
            return result ?? ApiResponse<TripProfitabilityDto>.Fail("Trip profitability not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<TripProfitabilityDto>.Fail($"Error loading profitability: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetCustomerDocumentsAsync(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<DocumentDto>>>($"api/documents/customer/{customerId}");
            return result ?? ApiResponse<List<DocumentDto>>.Fail("No documents found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<DocumentDto>>.Fail($"Error loading documents: {ex.Message}");
        }
    }

    public async Task<ApiResponse<DocumentDto>> GetDocumentByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<DocumentDto>>($"api/documents/{id}");
            return result ?? ApiResponse<DocumentDto>.Fail("Document not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<DocumentDto>.Fail($"Error loading document: {ex.Message}");
        }
    }

    public async Task<ApiResponse<DocumentDto>> UploadDocumentAsync(UploadDocumentDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/documents/upload", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<DocumentDto>>();
            return result ?? ApiResponse<DocumentDto>.Fail("Failed to upload document");
        }
        catch (Exception ex)
        {
            return ApiResponse<DocumentDto>.Fail($"Error uploading document: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteDocumentAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/documents/{id}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Failed to delete document");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Error deleting document: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetNotificationsAsync(bool unreadOnly = false)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<NotificationDto>>>($"api/notifications?unreadOnly={unreadOnly}");
            return result ?? ApiResponse<List<NotificationDto>>.Fail("No notifications found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<NotificationDto>>.Fail($"Error loading notifications: {ex.Message}");
        }
    }

    public async Task<ApiResponse<int>> GetUnreadNotificationCountAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<int>>("api/notifications/unread-count");
            return result ?? ApiResponse<int>.Ok(0);
        }
        catch (Exception ex)
        {
            return ApiResponse<int>.Fail($"Error getting unread count: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> MarkNotificationAsReadAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/notifications/{id}/read", null);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Failed to mark as read");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Error marking as read: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> MarkAllNotificationsAsReadAsync()
    {
        try
        {
            var response = await _httpClient.PutAsync("api/notifications/read-all", null);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Failed to mark all as read");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Error marking all as read: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<MessageTemplateDto>>> GetWhatsAppTemplatesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<MessageTemplateDto>>>("api/whatsapp/templates");
            return result ?? ApiResponse<List<MessageTemplateDto>>.Fail("No templates found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MessageTemplateDto>>.Fail($"Error loading templates: {ex.Message}");
        }
    }

    public async Task<ApiResponse<WhatsAppDispatchResultDto>> SendWhatsAppMessageAsync(SendWhatsAppMessageDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/whatsapp/send", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<WhatsAppDispatchResultDto>>();
            return result ?? ApiResponse<WhatsAppDispatchResultDto>.Fail("Failed to send WhatsApp message");
        }
        catch (Exception ex)
        {
            return ApiResponse<WhatsAppDispatchResultDto>.Fail($"Error sending WhatsApp message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> GenerateWhatsAppUrlAsync(string phone, string message)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/whatsapp/generate-url", new { Phone = phone, Message = message });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Failed to generate WhatsApp URL");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"Error generating URL: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ItineraryDraftResultDto>> GenerateItineraryDraftAsync(GenerateItineraryPromptDto prompt)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/ai/itinerary/draft", prompt);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ItineraryDraftResultDto>>();
            return result ?? ApiResponse<ItineraryDraftResultDto>.Fail("Failed to generate itinerary draft");
        }
        catch (Exception ex)
        {
            return ApiResponse<ItineraryDraftResultDto>.Fail($"Error generating itinerary: {ex.Message}");
        }
    }

    public async Task<ApiResponse<WhatsAppDraftResultDto>> DraftAIWhatsAppMessageAsync(DraftWhatsAppPromptDto prompt)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/ai/whatsapp/draft", prompt);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<WhatsAppDraftResultDto>>();
            return result ?? ApiResponse<WhatsAppDraftResultDto>.Fail("Failed to draft WhatsApp message");
        }
        catch (Exception ex)
        {
            return ApiResponse<WhatsAppDraftResultDto>.Fail($"Error drafting message: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AIRequestDto>>> GetAIRequestsAsync(string? status = null)
    {
        try
        {
            var url = string.IsNullOrEmpty(status) ? "api/ai/requests" : $"api/ai/requests?status={status}";
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<AIRequestDto>>>(url);
            return result ?? ApiResponse<List<AIRequestDto>>.Fail("No AI requests found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AIRequestDto>>.Fail($"Error loading requests: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ApproveAIRequestAsync(Guid id, string? notes = null)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/ai/requests/{id}/approve", new ApproveAIRequestDto { Notes = notes });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Failed to approve AI request");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Error approving request: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SyncPackageDto>> PullDeltaSyncPackageAsync(SyncPullRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/sync/pull", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SyncPackageDto>>();
            return result ?? ApiResponse<SyncPackageDto>.Fail("Failed to pull sync package");
        }
        catch (Exception ex)
        {
            return ApiResponse<SyncPackageDto>.Fail($"Error during sync pull: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SyncPushResultDto>> PushOfflineChangesAsync(SyncPushRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/sync/push", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SyncPushResultDto>>();
            return result ?? ApiResponse<SyncPushResultDto>.Fail("Failed to push offline changes");
        }
        catch (Exception ex)
        {
            return ApiResponse<SyncPushResultDto>.Fail($"Error during sync push: {ex.Message}");
        }
    }

    public async Task<bool> CheckServerConnectivityAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/sync/status");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ApiResponse<ExecutiveAnalyticsReportDto>> GetExecutiveAnalyticsAsync(DateRangeFilterDto? filter = null)
    {
        try
        {
            var url = "api/reports/analytics";
            if (filter?.StartDate.HasValue == true || filter?.EndDate.HasValue == true)
            {
                url += $"?startDate={filter?.StartDate:O}&endDate={filter?.EndDate:O}";
            }

            var result = await _httpClient.GetFromJsonAsync<ApiResponse<ExecutiveAnalyticsReportDto>>(url);
            return result ?? ApiResponse<ExecutiveAnalyticsReportDto>.Fail("No analytics data found");
        }
        catch (Exception ex)
        {
            return ApiResponse<ExecutiveAnalyticsReportDto>.Fail($"Error loading analytics: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ExportReportResultDto>> ExportExecutiveReportAsync(string format = "csv", DateRangeFilterDto? filter = null)
    {
        try
        {
            var url = $"api/reports/export?format={format}";
            if (filter?.StartDate.HasValue == true || filter?.EndDate.HasValue == true)
            {
                url += $"&startDate={filter?.StartDate:O}&endDate={filter?.EndDate:O}";
            }

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse<ExportReportResultDto>.Fail("Failed to export report");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "text/csv";
            var fileName = response.Content.Headers.ContentDisposition?.FileName ?? $"TomerGroup_Report.{format}";

            return ApiResponse<ExportReportResultDto>.Ok(new ExportReportResultDto
            {
                FileName = fileName,
                ContentType = contentType,
                FileBytes = bytes
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<ExportReportResultDto>.Fail($"Error exporting report: {ex.Message}");
        }
    }

    // Admin Real Backend API Implementations
    public async Task<ApiResponse<AdminDashboardMetricsDto>> GetAdminDashboardMetricsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<AdminDashboardMetricsDto>>("api/admin/dashboard");
            return result ?? ApiResponse<AdminDashboardMetricsDto>.Fail("No dashboard metrics returned");
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminDashboardMetricsDto>.Fail($"Error loading admin dashboard: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<AdminUserDto>>> GetAdminUsersAsync(UserRole? role = null, string? search = null)
    {
        try
        {
            var query = "api/admin/users";
            var queryParams = new List<string>();
            if (role.HasValue) queryParams.Add($"role={role.Value}");
            if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (queryParams.Count > 0) query += "?" + string.Join("&", queryParams);

            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<AdminUserDto>>>(query);
            return result ?? ApiResponse<List<AdminUserDto>>.Fail("No staff users found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AdminUserDto>>.Fail($"Error loading staff users: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AdminUserDto>> GetAdminUserByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<AdminUserDto>>($"api/admin/users/{id}");
            return result ?? ApiResponse<AdminUserDto>.Fail("Staff user not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminUserDto>.Fail($"Error loading staff user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AdminUserDto>> CreateAdminUserAsync(CreateAdminUserDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/users", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminUserDto>>();
            return result ?? ApiResponse<AdminUserDto>.Fail("Failed to create staff user");
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminUserDto>.Fail($"Error creating user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AdminUserDto>> UpdateAdminUserAsync(Guid id, UpdateAdminUserDto request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{id}", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminUserDto>>();
            return result ?? ApiResponse<AdminUserDto>.Fail("Failed to update staff user");
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminUserDto>.Fail($"Error updating user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeactivateAdminUserAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/users/{id}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Failed to deactivate staff user");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Error deactivating user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<AdminActivityFeedItemDto>>> GetAdminAuditLogsAsync(string? action = null, string? entity = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = $"api/admin/audit?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(action)) query += $"&action={Uri.EscapeDataString(action)}";
            if (!string.IsNullOrWhiteSpace(entity)) query += $"&entity={Uri.EscapeDataString(entity)}";

            var result = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<AdminActivityFeedItemDto>>>(query);
            return result ?? ApiResponse<PagedResult<AdminActivityFeedItemDto>>.Fail("No audit logs found");
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<AdminActivityFeedItemDto>>.Fail($"Error loading audit logs: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AdminDashboardV2Dto>> GetOperationsDashboardV2Async()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<AdminDashboardV2Dto>>("api/admin/dashboard/v2");
            return result ?? ApiResponse<AdminDashboardV2Dto>.Fail("Failed to load operations dashboard");
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminDashboardV2Dto>.Fail($"Error loading operations dashboard: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AdminSearchResultDto>> SearchGlobalAsync(string query)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<AdminSearchResultDto>>($"api/admin/search?q={Uri.EscapeDataString(query)}");
            return result ?? ApiResponse<AdminSearchResultDto>.Fail("No search results found");
        }
        catch (Exception ex)
        {
            return ApiResponse<AdminSearchResultDto>.Fail($"Search error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Customer360Dto>> GetCustomer360Async(Guid customerId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<Customer360Dto>>($"api/customers/{customerId}/360");
            return result ?? ApiResponse<Customer360Dto>.Fail("Customer 360 profile not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<Customer360Dto>.Fail($"Error loading customer profile: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TrekRouteEntity>>> GetAdminTreksAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<TrekRouteEntity>>>("api/admin/treks");
            return result ?? ApiResponse<List<TrekRouteEntity>>.Fail("No trek routes found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TrekRouteEntity>>.Fail($"Error loading trek routes: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrekRouteEntity>> GetAdminTrekByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<TrekRouteEntity>>($"api/admin/treks/{id}");
            return result ?? ApiResponse<TrekRouteEntity>.Fail("Trek route not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrekRouteEntity>.Fail($"Error loading trek route: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrekRouteEntity>> SaveTrekRouteAsync(SaveTrekRouteDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/treks", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TrekRouteEntity>>();
            return result ?? ApiResponse<TrekRouteEntity>.Fail("Failed to save trek route");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrekRouteEntity>.Fail($"Error saving trek route: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrekRouteEntity>> UpdateTrekRouteAsync(Guid id, SaveTrekRouteDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/treks/{id}", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TrekRouteEntity>>();
            return result ?? ApiResponse<TrekRouteEntity>.Fail("Failed to update trek route");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrekRouteEntity>.Fail($"Error updating trek route: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrekRouteEntity>> PublishTrekRouteAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/admin/treks/{id}/publish", null);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TrekRouteEntity>>();
            return result ?? ApiResponse<TrekRouteEntity>.Fail("Failed to publish trek route");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrekRouteEntity>.Fail($"Error publishing trek route: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TrekRouteEntity>> UnpublishTrekRouteAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/admin/treks/{id}/unpublish", null);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TrekRouteEntity>>();
            return result ?? ApiResponse<TrekRouteEntity>.Fail("Failed to unpublish trek route");
        }
        catch (Exception ex)
        {
            return ApiResponse<TrekRouteEntity>.Fail($"Error unpublishing trek route: {ex.Message}");
        }
    }

    public async Task<ApiResponse<GpxImportResultDto>> ImportGpxAsync(string gpxContent)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/treks/import-gpx", gpxContent);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<GpxImportResultDto>>();
            return result ?? ApiResponse<GpxImportResultDto>.Fail("Failed to parse GPX content");
        }
        catch (Exception ex)
        {
            return ApiResponse<GpxImportResultDto>.Fail($"Error parsing GPX: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<OperationalCalendarEventDto>>> GetOperationalCalendarAsync(DateTime? start = null, DateTime? end = null, string? filter = null)
    {
        try
        {
            var query = "api/admin/calendar?";
            if (start.HasValue) query += $"start={start.Value:yyyy-MM-dd}&";
            if (end.HasValue) query += $"end={end.Value:yyyy-MM-dd}&";
            if (!string.IsNullOrEmpty(filter)) query += $"filter={Uri.EscapeDataString(filter)}";

            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<OperationalCalendarEventDto>>>(query.TrimEnd('&', '?'));
            return result ?? ApiResponse<List<OperationalCalendarEventDto>>.Fail("No operational calendar events found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<OperationalCalendarEventDto>>.Fail($"Error loading calendar: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<SupportTicketSummaryDto>>> GetSupportTicketsAsync(SupportTicketStatus? status = null, SupportTicketPriority? priority = null)
    {
        try
        {
            var query = "api/support?";
            if (status.HasValue) query += $"status={status.Value}&";
            if (priority.HasValue) query += $"priority={priority.Value}&";

            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<SupportTicketSummaryDto>>>(query.TrimEnd('&', '?'));
            return result ?? ApiResponse<List<SupportTicketSummaryDto>>.Fail("No support tickets found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<SupportTicketSummaryDto>>.Fail($"Error loading support tickets: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SupportTicket>> GetSupportTicketByIdAsync(Guid id)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<ApiResponse<SupportTicket>>($"api/support/{id}");
            return result ?? ApiResponse<SupportTicket>.Fail("Support ticket not found");
        }
        catch (Exception ex)
        {
            return ApiResponse<SupportTicket>.Fail($"Error loading support ticket: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SupportTicket>> CreateSupportTicketAsync(CreateSupportTicketDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/support", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SupportTicket>>();
            return result ?? ApiResponse<SupportTicket>.Fail("Failed to create support ticket");
        }
        catch (Exception ex)
        {
            return ApiResponse<SupportTicket>.Fail($"Error creating support ticket: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SupportTicketMessage>> ReplyToSupportTicketAsync(Guid id, SupportTicketReplyDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/support/{id}/reply", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SupportTicketMessage>>();
            return result ?? ApiResponse<SupportTicketMessage>.Fail("Failed to reply to ticket");
        }
        catch (Exception ex)
        {
            return ApiResponse<SupportTicketMessage>.Fail($"Error replying to ticket: {ex.Message}");
        }
    }

    public async Task<ApiResponse<SupportTicket>> UpdateSupportTicketStatusAsync(Guid id, UpdateSupportTicketStatusDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/support/{id}/status", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SupportTicket>>();
            return result ?? ApiResponse<SupportTicket>.Fail("Failed to update ticket status");
        }
        catch (Exception ex)
        {
            return ApiResponse<SupportTicket>.Fail($"Error updating ticket status: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<StaffTask>>> GetStaffTasksAsync(StaffTaskStatus? status = null, bool dueSoonOnly = false)
    {
        try
        {
            var query = $"api/stafftasks?dueSoonOnly={dueSoonOnly}";
            if (status.HasValue) query += $"&status={status.Value}";

            var result = await _httpClient.GetFromJsonAsync<ApiResponse<List<StaffTask>>>(query);
            return result ?? ApiResponse<List<StaffTask>>.Fail("No staff tasks found");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<StaffTask>>.Fail($"Error loading staff tasks: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StaffTask>> CreateStaffTaskAsync(CreateStaffTaskDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/stafftasks", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StaffTask>>();
            return result ?? ApiResponse<StaffTask>.Fail("Failed to create staff task");
        }
        catch (Exception ex)
        {
            return ApiResponse<StaffTask>.Fail($"Error creating staff task: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StaffTask>> UpdateStaffTaskStatusAsync(Guid id, UpdateStaffTaskStatusDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/stafftasks/{id}/status", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StaffTask>>();
            return result ?? ApiResponse<StaffTask>.Fail("Failed to update staff task status");
        }
        catch (Exception ex)
        {
            return ApiResponse<StaffTask>.Fail($"Error updating task status: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Booking>> UpdateBookingAdminAsync(Guid id, UpdateBookingAdminDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/bookings/{id}/admin", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Booking>>();
            return result ?? ApiResponse<Booking>.Fail("Failed to update booking");
        }
        catch (Exception ex)
        {
            return ApiResponse<Booking>.Fail($"Error updating booking: {ex.Message}");
        }
    }

    public async Task<ApiResponse<Payment>> RecordBookingPaymentAsync(Guid id, PaymentItemDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/bookings/{id}/payments", dto);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Payment>>();
            return result ?? ApiResponse<Payment>.Fail("Failed to record payment");
        }
        catch (Exception ex)
        {
            return ApiResponse<Payment>.Fail($"Error recording payment: {ex.Message}");
        }
    }
}





