using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class AgencyDashboardViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private int _todayArrivals = 0;

    [ObservableProperty]
    private int _todayDepartures = 0;

    [ObservableProperty]
    private int _todayTours = 0;

    [ObservableProperty]
    private int _activeTravelers = 0;

    [ObservableProperty]
    private int _pendingBookings = 0;

    [ObservableProperty]
    private int _pendingPayments = 0;

    [ObservableProperty]
    private int _unassignedTransfers = 0;

    [ObservableProperty]
    private int _unassignedGuides = 0;

    [ObservableProperty]
    private decimal _revenue = 0;

    [ObservableProperty]
    private decimal _expenses = 0;

    [ObservableProperty]
    private decimal _grossProfit = 0;

    [ObservableProperty]
    private decimal _marginPercentage = 0;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

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
            var response = await _apiClient.GetAdminDashboardMetricsAsync();
            if (response.Success && response.Data != null)
            {
                var data = response.Data;
                TodayArrivals = data.TodayArrivals;
                TodayDepartures = data.TodayDepartures;
                ActiveTravelers = data.ActiveInPeruCustomers > 0 ? data.ActiveInPeruCustomers : data.TotalCustomers;
                PendingBookings = data.PendingBookings;
                PendingPayments = data.PendingPayments;
                UnassignedTransfers = data.UnassignedTransportationCount;
                UnassignedGuides = data.UnassignedGuidesCount;
                Revenue = data.TotalRevenue;
                Expenses = data.TotalExpenses;
                GrossProfit = data.GrossProfit;
                MarginPercentage = data.MarginPercentage;

                RecentActivities.Clear();
                if (data.RecentActivities != null)
                {
                    foreach (var item in data.RecentActivities)
                    {
                        RecentActivities.Add(item);
                    }
                }
            }
            else
            {
                // Fallback to reports or summary if dashboard endpoint is unavailable
                var finResponse = await _apiClient.GetFinancialSummaryAsync();
                if (finResponse.Success && finResponse.Data != null)
                {
                    Revenue = finResponse.Data.TotalRevenueUsd;
                    Expenses = finResponse.Data.TotalExpensesUsd;
                    GrossProfit = finResponse.Data.NetProfitUsd;
                    MarginPercentage = finResponse.Data.MarginPercentage;
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading metrics: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
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
    private string _whatsApp = "+51 984 231961";

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
        SelectedLanguage = lang;
        Localization.SetLanguage(lang);
        RefreshDirection();
        Title = Localize(LocalizationKeys.Settings);
    }

    [RelayCommand]
    public async Task SaveBrandingAsync()
    {
        IsBusy = true;
        try
        {
            Brand.AgencyName = AgencyName;
            Brand.Tagline = Tagline;
            Brand.PrimaryColor = PrimaryColor;
            Brand.SecondaryColor = SecondaryColor;
            Brand.AccentColor = AccentColor;
            Brand.ContactPhone = ContactPhone;
            Brand.WhatsApp = WhatsApp;

            await Task.Delay(200);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
