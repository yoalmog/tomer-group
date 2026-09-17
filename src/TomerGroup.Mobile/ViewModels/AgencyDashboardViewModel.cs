using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class AgencyDashboardViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private int _todayArrivals = 3;

    [ObservableProperty]
    private int _todayDepartures = 2;

    [ObservableProperty]
    private int _todayTours = 5;

    [ObservableProperty]
    private int _activeTravelers = 18;

    [ObservableProperty]
    private decimal _revenue = 2500;

    [ObservableProperty]
    private decimal _expenses = 1700;

    [ObservableProperty]
    private decimal _grossProfit = 800;

    [ObservableProperty]
    private decimal _outstandingPayments = 0;

    public AgencyDashboardViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        Title = Localize(LocalizationKeys.AgencyDashboard);
    }

    [RelayCommand]
    public async Task RefreshMetricsAsync()
    {
        IsBusy = true;
        try
        {
            await Task.Delay(300);
            Revenue = 2500;
            Expenses = 1700;
            GrossProfit = 800;
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
    private string _primaryColor = "#1B365D";

    [ObservableProperty]
    private string _secondaryColor = "#C28251";

    [ObservableProperty]
    private string _accentColor = "#2D9CDB";

    [ObservableProperty]
    private string _contactPhone = "+51 84 223 456";

    [ObservableProperty]
    private string _whatsApp = "+51 984 123 456";

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

