using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class MyTripViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IDestinationImageService _imageService;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private TripDto? _currentTrip;

    [ObservableProperty]
    private ObservableCollection<TripDayDto> _days = new();

    [ObservableProperty]
    private TripDayDto? _selectedDay;

    [ObservableProperty]
    private int _selectedDayNumber = 1;

    [ObservableProperty]
    private string _selectedDayTitle = string.Empty;

    [ObservableProperty]
    private string _selectedDayDate = string.Empty;

    [ObservableProperty]
    private string _selectedDayDestination = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ActivityDto> _selectedDayActivities = new();

    [ObservableProperty]
    private bool _hasTrip = false;

    [ObservableProperty]
    private string _tripCoverImage = string.Empty;

    [ObservableProperty]
    private string _tripDestinations = string.Empty;

    [ObservableProperty]
    private string _tripDates = string.Empty;

    [ObservableProperty]
    private string _emptyTitle = "אין עדיין טיול פעיל";

    [ObservableProperty]
    private string _emptyDescription = "מסלול הטיול האישי שלך ב-Tomer Group יופיע כאן ברגע שצוות הסוכנות יקים עבורך את תוכנית המסע.";

    public MyTripViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IApiClient apiClient,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        _imageService = imageService;
        Title = Localize(LocalizationKeys.NavMyTrip);
        TripCoverImage = _imageService.GetMachuPicchuImage();
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsAuthenticated = _apiClient.IsAuthenticated;
        if (!IsAuthenticated)
        {
            HasTrip = false;
            CurrentTrip = null;
            Days.Clear();
            SelectedDayActivities.Clear();
            return;
        }

        await RefreshItineraryAsync();
    }

    [RelayCommand]
    public async Task RefreshItineraryAsync()
    {
        IsBusy = true;
        try
        {
            Days.Clear();
            SelectedDayActivities.Clear();

            var response = await _apiClient.GetCustomerTripsAsync(Guid.Empty);
            if (response.Success && response.Data != null && response.Data.Count > 0)
            {
                CurrentTrip = response.Data.FirstOrDefault();
                if (CurrentTrip != null)
                {
                    HasTrip = true;
                    TripDates = $"{CurrentTrip.StartDate:dd.MM.yyyy} — {CurrentTrip.EndDate:dd.MM.yyyy}";
                    TripCoverImage = _imageService.GetDestinationHeroImage(CurrentTrip.Title);

                    foreach (var day in CurrentTrip.Days.OrderBy(d => d.DayNumber))
                    {
                        Days.Add(day);
                    }

                    TripDestinations = string.Join(" · ", CurrentTrip.Days.Select(d => d.Destination).Distinct());

                    if (Days.Count > 0)
                    {
                        SelectDay(Days[0]);
                    }
                }
                else
                {
                    HasTrip = false;
                    CurrentTrip = null;
                }
            }
            else
            {
                HasTrip = false;
                CurrentTrip = null;
            }
        }
        catch
        {
            HasTrip = false;
            CurrentTrip = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void SelectDay(TripDayDto day)
    {
        if (day == null) return;

        SelectedDay = day;
        SelectedDayNumber = day.DayNumber;
        SelectedDayTitle = day.Title;
        SelectedDayDate = $"{day.Date:dddd, dd MMMM yyyy}";
        SelectedDayDestination = day.Destination;

        SelectedDayActivities.Clear();
        foreach (var act in day.Activities.OrderBy(a => a.StartTime))
        {
            SelectedDayActivities.Add(act);
        }
    }

    [RelayCommand]
    public void SelectDayByNumber(int dayNumber)
    {
        var target = Days.FirstOrDefault(d => d.DayNumber == dayNumber);
        if (target != null)
        {
            SelectDay(target);
        }
    }

    [RelayCommand]
    public async Task OpenActivityDetailAsync(ActivityDto activity)
    {
        if (activity == null) return;
        // Navigate to ActivityDetail passing activity title or ID
        await Navigation.NavigateToAsync($"ActivityDetail?id={activity.Id}&title={Uri.EscapeDataString(activity.Title)}");
    }

    [RelayCommand]
    public async Task OpenSignInAsync()
    {
        await Navigation.NavigateToLoginAsync();
    }

    [RelayCommand]
    public async Task ContinueExploringAsync()
    {
        await Navigation.NavigateToAsync("//Home");
    }

    [RelayCommand]
    public async Task OpenContactAsync()
    {
        await Navigation.NavigateToAsync("More");
    }
}
