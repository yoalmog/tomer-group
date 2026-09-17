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
    private string _customerGreeting = "שלום";

    [ObservableProperty]
    private string _tripTitle = string.Empty;

    [ObservableProperty]
    private string _countryBadge = "פרו 🇵🇪";

    [ObservableProperty]
    private string _tripDates = string.Empty;

    [ObservableProperty]
    private string _currentDestination = string.Empty;

    [ObservableProperty]
    private bool _hasActiveTrip = false;

    [ObservableProperty]
    private bool _hasNextActivity = false;

    [ObservableProperty]
    private string _nextActivityTime = string.Empty;

    [ObservableProperty]
    private string _nextActivityTitle = string.Empty;

    [ObservableProperty]
    private string _nextActivityLocation = string.Empty;

    [ObservableProperty]
    private string _secondActivityTime = string.Empty;

    [ObservableProperty]
    private string _secondActivityTitle = string.Empty;

    [ObservableProperty]
    private string _thirdActivityTime = string.Empty;

    [ObservableProperty]
    private string _thirdActivityTitle = string.Empty;

    [ObservableProperty]
    private string _agencyContactPhone = "+51 84 223 456";

    [ObservableProperty]
    private string _emergencyPhone = "+51 984 999 888";

    [ObservableProperty]
    private int _unreadNotificationsCount = 0;

    [ObservableProperty]
    private string _emptyTripMessage = "אין עדיין טיול פעיל";

    [ObservableProperty]
    private string _emptyTripSubtext = "הטיול שלך ב-Tomer Group יופיע כאן ברגע שצוות הסוכנות יקים את ההזמנה שלך.";

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
        CountryBadge = Localize(LocalizationKeys.CountryPeru);
        AgencyContactPhone = LocalizationService.FormatPhoneNumber(Brand.ContactPhone);
        EmergencyPhone = LocalizationService.FormatPhoneNumber(Brand.EmergencyContact);
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var profileRes = await _apiClient.GetMyProfileAsync();
            if (profileRes.Success && profileRes.Data != null)
            {
                var name = !string.IsNullOrWhiteSpace(profileRes.Data.HebrewName)
                    ? profileRes.Data.HebrewName
                    : $"{profileRes.Data.FirstName} {profileRes.Data.LastName}".Trim();

                CustomerGreeting = !string.IsNullOrWhiteSpace(name)
                    ? $"{Localize(LocalizationKeys.Greeting)} {name} 👋"
                    : Localize(LocalizationKeys.Greeting);
            }

            var tripsRes = await _apiClient.GetCustomerTripsAsync(Guid.Empty);
            if (tripsRes.Success && tripsRes.Data != null && tripsRes.Data.Count > 0)
            {
                var activeTrip = tripsRes.Data.FirstOrDefault();
                if (activeTrip != null)
                {
                    HasActiveTrip = true;
                    TripTitle = activeTrip.Title;
                    TripDates = $"{activeTrip.StartDate:dd.MM.yyyy} – {activeTrip.EndDate:dd.MM.yyyy}";
                    CurrentDestination = activeTrip.Days.FirstOrDefault()?.Destination ?? "Peru";

                    var firstDay = activeTrip.Days.FirstOrDefault();
                    if (firstDay != null && firstDay.Activities.Count > 0)
                    {
                        var firstAct = firstDay.Activities[0];
                        HasNextActivity = true;
                        NextActivityTime = $"{firstAct.StartTime:hh\\:mm}";
                        NextActivityTitle = firstAct.Title;
                        NextActivityLocation = firstAct.Location ?? string.Empty;

                        if (firstDay.Activities.Count > 1)
                        {
                            SecondActivityTime = $"{firstDay.Activities[1].StartTime:hh\\:mm}";
                            SecondActivityTitle = firstDay.Activities[1].Title;
                        }

                        if (firstDay.Activities.Count > 2)
                        {
                            ThirdActivityTime = $"{firstDay.Activities[2].StartTime:hh\\:mm}";
                            ThirdActivityTitle = firstDay.Activities[2].Title;
                        }
                    }
                }
            }
            else
            {
                HasActiveTrip = false;
                HasNextActivity = false;
            }
        }
        catch
        {
            HasActiveTrip = false;
            HasNextActivity = false;
        }
        finally
        {
            IsBusy = false;
        }
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
    public async Task OpenMoreAsync()
    {
        await Navigation.NavigateToAsync("//More");
    }

    [RelayCommand]
    public async Task ContactAgencyAsync()
    {
        await Navigation.NavigateToAsync("//More");
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        _apiClient.SetAuthToken(null);
        await Navigation.NavigateToLoginAsync();
    }
}
