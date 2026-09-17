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
    private string _errorMessage = string.Empty;

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
        ErrorMessage = string.Empty;

        try
        {
            Trips.Clear();
            var response = await _apiClient.GetCustomerTripsAsync(Guid.Empty);
            // Default trips
            AddDefaultTrips();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            AddDefaultTrips();
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

    private void AddDefaultTrips()
    {
        Trips.Clear();
        Trips.Add(new TripDto
        {
            Id = Guid.NewGuid(),
            TripCode = "PERU-2026-00482",
            Title = "חוויית פרו VIP — דני כהן (Danny Cohen)",
            CustomerName = "Danny Cohen (דני כהן)",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(5),
            Status = TripStatus.InProgress,
            TotalRevenue = 2500,
            TotalCost = 1450,
            GrossProfit = 1050,
            Currency = Currency.USD,
            Days = new List<TripDayDto>
            {
                new() { DayNumber = 1, Date = DateTime.UtcNow.Date, Title = "Day 1: הגעה לקוסקו", Destination = "Cusco" },
                new() { DayNumber = 2, Date = DateTime.UtcNow.Date.AddDays(1), Title = "Day 2: סיור עיר וסאקסייוואמאן", Destination = "Cusco" },
                new() { DayNumber = 3, Date = DateTime.UtcNow.Date.AddDays(2), Title = "Day 3: שוק פיסאק ומבצר אולאנטייטמבו", Destination = "Sacred Valley" },
                new() { DayNumber = 4, Date = DateTime.UtcNow.Date.AddDays(3), Title = "Day 4: סיור VIP במאצ'ו פיצ'ו", Destination = "Machu Picchu" },
                new() { DayNumber = 5, Date = DateTime.UtcNow.Date.AddDays(4), Title = "Day 5: הר שבעת הצבעים (ויניקונקה)", Destination = "Rainbow Mountain" }
            }
        });

        Trips.Add(new TripDto
        {
            Id = Guid.NewGuid(),
            TripCode = "PERU-2026-00483",
            Title = "עמק הקדוש ומאצ'ו פיצ'ו — מאיה לוי",
            CustomerName = "Maya Levi (מאיה לוי)",
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(12),
            Status = TripStatus.Confirmed,
            TotalRevenue = 1800,
            TotalCost = 1020,
            GrossProfit = 780,
            Currency = Currency.USD
        });
    }
}

