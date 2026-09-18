using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class MyDayViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService _secureStorage;
    private readonly IMobileMapService _mapService;
    private readonly IDestinationImageService _imageService;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private string _customerName = "Traveler";

    [ObservableProperty]
    private string _currentDateText = string.Empty;

    [ObservableProperty]
    private string _todayDestination = "Cusco";

    [ObservableProperty]
    private string _todayCoverImage = string.Empty;

    [ObservableProperty]
    private string _nextUpTitle = "No more activities scheduled today";

    [ObservableProperty]
    private string _nextUpTime = "--:--";

    [ObservableProperty]
    private string _nextUpCountdown = "Relax and enjoy your evening";

    [ObservableProperty]
    private string _nextUpLocation = "Hotel";

    [ObservableProperty]
    private bool _hasNextUpActivity;

    [ObservableProperty]
    private string _weatherInfo = "Andean altitude weather: brisk mornings (5°C) warming to 19°C under high sun. Rain protection recommended.";

    public ObservableCollection<ActivityDto> TodayActivities { get; } = new();

    public MyDayViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IApiClient apiClient,
        ISecureStorageService secureStorage,
        IMobileMapService mapService,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;
        _mapService = mapService;
        _imageService = imageService;

        Title = Localize(LocalizationKeys.NavMyTrip);
        CurrentDateText = DateTime.UtcNow.ToString("dddd, MMMM dd, yyyy");
        TodayCoverImage = _imageService.GetDestinationHeroImage("Cusco");
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var token = await _secureStorage.GetAsync("auth_token");
            IsAuthenticated = !string.IsNullOrWhiteSpace(token);

            if (!IsAuthenticated)
            {
                HasNextUpActivity = false;
                TodayActivities.Clear();
                return;
            }

            var userProfile = await _apiClient.GetMyProfileAsync();
            if (userProfile.Success && userProfile.Data != null)
            {
                CustomerName = userProfile.Data.FirstName;
                var tripsResponse = await _apiClient.GetCustomerTripsAsync(userProfile.Data.Id);
                if (tripsResponse.Success && tripsResponse.Data != null && tripsResponse.Data.Any())
                {
                    var activeTrip = tripsResponse.Data.FirstOrDefault(t => t.Status == TomerGroup.Core.Enums.TripStatus.InProgress || t.Status == TomerGroup.Core.Enums.TripStatus.Confirmed) 
                                     ?? tripsResponse.Data.First();

                    var tripDetail = await _apiClient.GetTripDetailsAsync(activeTrip.Id);
                    if (tripDetail.Success && tripDetail.Data?.Days != null)
                    {
                        // Find today's day in trip or day 1
                        var todayDay = tripDetail.Data.Days.FirstOrDefault() ?? new TripDayDto();
                        TodayDestination = !string.IsNullOrWhiteSpace(todayDay.Destination) ? todayDay.Destination : "Cusco";
                        TodayCoverImage = _imageService.GetDestinationHeroImage(TodayDestination);
                        TodayActivities.Clear();

                    if (todayDay.Activities != null)
                    {
                        var now = DateTime.UtcNow.TimeOfDay;
                        ActivityDto? nextActivity = null;

                        foreach (var act in todayDay.Activities)
                        {
                            TodayActivities.Add(act);
                            if (nextActivity == null && act.StartTime > now)
                            {
                                nextActivity = act;
                            }
                        }

                        if (nextActivity != null)
                        {
                            HasNextUpActivity = true;
                            NextUpTitle = nextActivity.Title;
                            NextUpTime = nextActivity.StartTime.ToString(@"hh\:mm");
                            NextUpLocation = nextActivity.Location;
                            var diff = nextActivity.StartTime - now;
                            NextUpCountdown = diff.TotalMinutes > 0 
                                ? $"In {(int)diff.TotalMinutes} minutes" 
                                : "Starting now";
                        }
                        else if (TodayActivities.Any())
                        {
                            HasNextUpActivity = true;
                            var last = TodayActivities.Last();
                            NextUpTitle = last.Title;
                            NextUpTime = last.StartTime.ToString(@"hh\:mm");
                            NextUpLocation = last.Location;
                            NextUpCountdown = "Completed for today";
                        }
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Offline or network error safe handling
        System.Diagnostics.Debug.WriteLine($"MyDay init error: {ex.Message}");
    }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OpenMapAsync(string? specificLocation = null)
    {
        var target = !string.IsNullOrWhiteSpace(specificLocation)
            ? specificLocation
            : (!string.IsNullOrWhiteSpace(NextUpLocation) ? NextUpLocation : TodayDestination);

        if (string.IsNullOrWhiteSpace(target))
        {
            target = "Cusco";
        }

        await Navigation.NavigateToAsync($"TrekMap?trekName={Uri.EscapeDataString(target)}");
    }

    [RelayCommand]
    public async Task OpenSupportAsync()
    {
        await Navigation.NavigateToAsync("More");
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Navigation.GoBackAsync();
    }
}
