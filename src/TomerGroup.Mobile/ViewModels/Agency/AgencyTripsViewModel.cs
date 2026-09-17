using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyTripsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public AgencyTripsViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Trips = new ObservableCollection<TripDto>();
        Destinations = new ObservableCollection<DestinationDto>();
    }

    public ObservableCollection<TripDto> Trips { get; }
    public ObservableCollection<DestinationDto> Destinations { get; }

    [ObservableProperty]
    private TripDto? _selectedTrip;

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private bool _hasError = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty = false;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadDestinationsAsync();
        await LoadTripsAsync();
    }

    [RelayCommand]
    public async Task LoadTripsAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            Trips.Clear();
            var response = await _apiClient.GetCustomerTripsAsync(Guid.Empty);
            if (response.Success && response.Data != null)
            {
                foreach (var trip in response.Data)
                {
                    Trips.Add(trip);
                }
            }

            IsEmpty = Trips.Count == 0;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
            IsEmpty = Trips.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LoadDestinationsAsync()
    {
        try
        {
            var result = await _apiClient.GetDestinationsAsync();
            Destinations.Clear();
            if (result.Success && result.Data != null)
            {
                foreach (var d in result.Data)
                {
                    Destinations.Add(d);
                }
            }
        }
        catch
        {
            // Non-blocking
        }
    }
}
