using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using Microsoft.Maui.Controls;
#endif
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class AgencyDashboardViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    // Search Zone
    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _hasSearchResults;

    public ObservableCollection<AdminSearchItemDto> SearchResults { get; } = new();

    // Summary Metric Zone
    [ObservableProperty]
    private int _todayArrivals = 0;

    [ObservableProperty]
    private int _todayDepartures = 0;

    [ObservableProperty]
    private int _activeTrips = 0;

    [ObservableProperty]
    private int _upcomingTrips = 0;

    [ObservableProperty]
    private int _pendingBookings = 0;

    [ObservableProperty]
    private int _pendingPayments = 0;

    [ObservableProperty]
    private int _openSupportRequests = 0;

    [ObservableProperty]
    private int _openTasks = 0;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Operations Schedule Zone
    public ObservableCollection<OperationalScheduleItemDto> TodaySchedule { get; } = new();

    // Critical Alerts Zone
    public ObservableCollection<AdminAlertItemDto> CriticalAlerts { get; } = new();

    // Activity Feed Zone
    public ObservableCollection<AdminActivityFeedItemDto> RecentActivities { get; } = new();

    public AgencyDashboardViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        Title = Localize(LocalizationKeys.AgencyDashboard);
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await RefreshMetricsAsync();
    }

    [RelayCommand]
    public async Task RefreshMetricsAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiClient.GetOperationsDashboardV2Async();
            if (response.Success && response.Data != null)
            {
                var data = response.Data;
                TodayArrivals = data.TodayArrivalsCount;
                TodayDepartures = data.TodayDeparturesCount;
                ActiveTrips = data.ActiveTripsCount;
                UpcomingTrips = data.UpcomingTripsCount;
                PendingBookings = data.PendingBookingsCount;
                PendingPayments = data.PendingPaymentsCount;
                OpenSupportRequests = data.OpenSupportRequestsCount;
                OpenTasks = data.OpenStaffTasksCount;

                TodaySchedule.Clear();
                if (data.TodaySchedule != null)
                {
                    foreach (var item in data.TodaySchedule)
                    {
                        TodaySchedule.Add(item);
                    }
                }

                CriticalAlerts.Clear();
                if (data.CriticalAlerts != null)
                {
                    foreach (var alert in data.CriticalAlerts)
                    {
                        CriticalAlerts.Add(alert);
                    }
                }

                RecentActivities.Clear();
                if (data.RecentActivities != null)
                {
                    foreach (var act in data.RecentActivities)
                    {
                        RecentActivities.Add(act);
                    }
                }
            }
            else
            {
                // Fallback to basic metrics
                var basicResponse = await _apiClient.GetAdminDashboardMetricsAsync();
                if (basicResponse.Success && basicResponse.Data != null)
                {
                    TodayArrivals = basicResponse.Data.TodayArrivals;
                    TodayDepartures = basicResponse.Data.TodayDepartures;
                    ActiveTrips = basicResponse.Data.ActiveTrips;
                    UpcomingTrips = basicResponse.Data.UpcomingTrips;
                    PendingBookings = basicResponse.Data.PendingBookings;
                    PendingPayments = basicResponse.Data.PendingPayments;

                    RecentActivities.Clear();
                    if (basicResponse.Data.RecentActivities != null)
                    {
                        foreach (var act in basicResponse.Data.RecentActivities)
                        {
                            RecentActivities.Add(act);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading operations dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Trim().Length < 2)
        {
            SearchResults.Clear();
            HasSearchResults = false;
            return;
        }

        IsSearching = true;
        try
        {
            var response = await _apiClient.SearchGlobalAsync(SearchQuery.Trim());
            SearchResults.Clear();

            if (response.Success && response.Data != null)
            {
                var data = response.Data;
                foreach (var item in data.Customers) SearchResults.Add(item);
                foreach (var item in data.Bookings) SearchResults.Add(item);
                foreach (var item in data.Trips) SearchResults.Add(item);
                foreach (var item in data.Treks) SearchResults.Add(item);
                foreach (var item in data.Guides) SearchResults.Add(item);
                foreach (var item in data.Drivers) SearchResults.Add(item);
                foreach (var item in data.Hotels) SearchResults.Add(item);
                foreach (var item in data.SupportTickets) SearchResults.Add(item);

                HasSearchResults = SearchResults.Count > 0;
            }
            else
            {
                HasSearchResults = false;
            }
        }
        catch
        {
            HasSearchResults = false;
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    public void ClearSearch()
    {
        SearchQuery = string.Empty;
        SearchResults.Clear();
        HasSearchResults = false;
    }

    [RelayCommand]
    public async Task SelectSearchResultAsync(AdminSearchItemDto item)
    {
        if (item == null) return;

        ClearSearch();

        if (item.Category.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Shell.Current.GoToAsync($"AgencyCustomer360?customerId={item.Id}");
#else
            await Task.CompletedTask;
#endif
        }
        else if (item.Category.Equals("Booking", StringComparison.OrdinalIgnoreCase))
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Shell.Current.GoToAsync("AgencyBookings");
#else
            await Task.CompletedTask;
#endif
        }
        else if (item.Category.Equals("Trek", StringComparison.OrdinalIgnoreCase))
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Shell.Current.GoToAsync($"AgencyTrekEditor?trekId={item.Id}");
#else
            await Task.CompletedTask;
#endif
        }
        else if (item.Category.Equals("SupportTicket", StringComparison.OrdinalIgnoreCase))
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Shell.Current.GoToAsync("AgencySupport");
#else
            await Task.CompletedTask;
#endif
        }
        else if (item.Category.Equals("Trip", StringComparison.OrdinalIgnoreCase))
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Shell.Current.GoToAsync("AgencyTrips");
#else
            await Task.CompletedTask;
#endif
        }
    }

    [RelayCommand]
    public void DismissAlert(AdminAlertItemDto alert)
    {
        if (alert != null)
        {
            CriticalAlerts.Remove(alert);
        }
    }

    [RelayCommand]
    public async Task NavigateToAlertAsync(AdminAlertItemDto alert)
    {
        if (alert == null) return;

        if (alert.TargetEntity.Equals("Transportation", StringComparison.OrdinalIgnoreCase) ||
            alert.TargetEntity.Equals("Driver", StringComparison.OrdinalIgnoreCase))
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Shell.Current.GoToAsync("AgencyStaffDirectory");
#else
            await Task.CompletedTask;
#endif
        }
        else if (alert.TargetEntity.Equals("SupportTicket", StringComparison.OrdinalIgnoreCase))
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Shell.Current.GoToAsync("AgencySupport");
#else
            await Task.CompletedTask;
#endif
        }
        else if (alert.TargetEntity.Equals("Payment", StringComparison.OrdinalIgnoreCase) ||
                 alert.TargetEntity.Equals("Document", StringComparison.OrdinalIgnoreCase))
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Shell.Current.GoToAsync("AgencyBookings");
#else
            await Task.CompletedTask;
#endif
        }
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        _apiClient.SetAuthToken(null);
        await Navigation.NavigateToLoginAsync();
    }
}

public partial class AgencySettingsViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private string _agencyName = "Tomer Group";

    [ObservableProperty]
    private string _tagline = "Peru Travel Experience";

    [ObservableProperty]
    private string _primaryColor = "#BC225E";

    [ObservableProperty]
    private string _secondaryColor = "#0A0A0C";

    [ObservableProperty]
    private string _accentColor = "#E11D68";

    [ObservableProperty]
    private string _contactPhone = "+51 984 231961";

    [ObservableProperty]
    private string _selectedLanguage = "he";

    public AgencySettingsViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        Title = Localize(LocalizationKeys.Settings);
        SelectedLanguage = localization.CurrentLanguage;
    }

    [RelayCommand]
    public void ChangeLanguage(string lang)
    {
        Localization.SetLanguage(lang);
        SelectedLanguage = lang;
    }
}
