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

    [ObservableProperty]
    private TripDto? _currentTrip;

    [ObservableProperty]
    private ObservableCollection<TripDayDto> _days = new();

    [ObservableProperty]
    private bool _hasTrip = false;

    [ObservableProperty]
    private string _emptyTitle = "אין עדיין טיול פעיל";

    [ObservableProperty]
    private string _emptyDescription = "מסלול הטיול האישי שלך ב-Tomer Group יופיע כאן ברגע שצוות הסוכנות יקים עבורך את תוכנית המסע.";

    public MyTripViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        Title = Localize(LocalizationKeys.NavMyTrip);
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await RefreshItineraryAsync();
    }

    [RelayCommand]
    public async Task RefreshItineraryAsync()
    {
        IsBusy = true;
        try
        {
            Days.Clear();
            var response = await _apiClient.GetCustomerTripsAsync(Guid.Empty);
            if (response.Success && response.Data != null && response.Data.Count > 0)
            {
                CurrentTrip = response.Data.FirstOrDefault();
                if (CurrentTrip != null)
                {
                    HasTrip = true;
                    foreach (var day in CurrentTrip.Days)
                    {
                        Days.Add(day);
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
    public async Task OpenContactAsync()
    {
        await Navigation.NavigateToAsync("//More");
    }
}
