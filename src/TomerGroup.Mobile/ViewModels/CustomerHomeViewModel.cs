using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class CustomerHomeViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private string _customerGreeting = "שלום דני 👋";

    [ObservableProperty]
    private string _tripTitle = "הטיול שלך עם Tomer Group";

    [ObservableProperty]
    private string _countryBadge = "פרו 🇵🇪";

    [ObservableProperty]
    private string _tripDates = "15.09.2026 – 22.09.2026";

    [ObservableProperty]
    private string _currentDestination = "Cusco & Machu Picchu";

    // Activity Schedule matching Section 8 exactly
    [ObservableProperty]
    private string _nextActivityTime = "05:30";

    [ObservableProperty]
    private string _nextActivityTitle = "🚐 איסוף מהמלון";

    [ObservableProperty]
    private string _secondActivityTime = "08:30";

    [ObservableProperty]
    private string _secondActivityTitle = "🏔️ מאצ'ו פיצ'ו";

    [ObservableProperty]
    private string _thirdActivityTime = "16:00";

    [ObservableProperty]
    private string _thirdActivityTitle = "🚂 רכבת חזרה";

    [ObservableProperty]
    private string _agencyContactPhone = "+51 84 223 456";

    [ObservableProperty]
    private string _emergencyPhone = "+51 984 999 888";

    [ObservableProperty]
    private int _unreadNotificationsCount = 1;

    public CustomerHomeViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        Title = Localize(LocalizationKeys.NavHome);
        UpdateLocalizedContent();
    }

    private void UpdateLocalizedContent()
    {
        CustomerGreeting = Localize(LocalizationKeys.Greeting);
        TripTitle = Localize(LocalizationKeys.YourTripWith);
        CountryBadge = Localize(LocalizationKeys.CountryPeru);
        NextActivityTitle = Localize(LocalizationKeys.HotelPickup);
        SecondActivityTitle = Localize(LocalizationKeys.MachuPicchuTour);
        ThirdActivityTitle = Localize(LocalizationKeys.ReturnTrain);
        AgencyContactPhone = LocalizationService.FormatPhoneNumber(Brand.ContactPhone);
        EmergencyPhone = LocalizationService.FormatPhoneNumber(Brand.EmergencyContact);
    }

    [RelayCommand]
    public async Task OpenMyTripAsync()
    {
        await Navigation.NavigateToAsync("//MyTrip");
    }

    [RelayCommand]
    public async Task OpenBookingsAsync()
    {
        await Navigation.NavigateToAsync("//Bookings");
    }

    [RelayCommand]
    public async Task OpenDocumentsAsync()
    {
        await Navigation.NavigateToAsync("//Documents");
    }

    [RelayCommand]
    public async Task ContactAgencyAsync()
    {
        // Triggers WhatsApp or phone call
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        _apiClient.SetAuthToken(null);
        await Navigation.NavigateToLoginAsync();
    }
}

