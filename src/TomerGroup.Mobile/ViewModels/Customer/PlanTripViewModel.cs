using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class PlanTripViewModel : ObservableObject
{
    private readonly INavigationService? _navigationService;

    public ObservableCollection<string> DestinationOptions { get; } = new()
    {
        "Cusco & Sacred Valley",
        "Machu Picchu",
        "Arequipa & Colca",
        "Lima & Paracas",
        "Amazon Rainforest"
    };

    [ObservableProperty]
    private string _selectedDestination = "Cusco & Sacred Valley";

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(14);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(18);

    [ObservableProperty]
    private int _travelers = 2;

    [ObservableProperty]
    private decimal _estimatedTotal = 2400m;

    [ObservableProperty]
    private string _statusMessage = "Ready to plan your journey";

    public PlanTripViewModel() : this(null)
    {
    }

    public PlanTripViewModel(INavigationService? navigationService)
    {
        _navigationService = navigationService;
        RecalculateEstimate();
    }

    partial void OnSelectedDestinationChanged(string value)
    {
        RecalculateEstimate();
    }

    partial void OnTravelersChanged(int value)
    {
        RecalculateEstimate();
    }

    partial void OnStartDateChanged(DateTime value)
    {
        RecalculateEstimate();
    }

    partial void OnEndDateChanged(DateTime value)
    {
        RecalculateEstimate();
    }

    private void RecalculateEstimate()
    {
        var basePrice = SelectedDestination switch
        {
            "Cusco & Sacred Valley" => 1150m,
            "Machu Picchu" => 1450m,
            "Arequipa & Colca" => 1280m,
            "Lima & Paracas" => 980m,
            "Amazon Rainforest" => 1600m,
            _ => 1200m
        };

        var durationDays = Math.Max(1, (EndDate - StartDate).Days + 1);
        EstimatedTotal = (basePrice * Travelers) + (durationDays * 110m);
        StatusMessage = $"Estimated total: ${EstimatedTotal:N0} USD";
    }

    [RelayCommand]
    public async Task CreateBookingAsync()
    {
        var tripLength = Math.Max(1, (EndDate - StartDate).Days + 1);
        StatusMessage = $"Trip plan created for {Travelers} traveler(s) to {SelectedDestination}. Estimated total: ${EstimatedTotal:N0} USD for {tripLength} days.";

        if (_navigationService is not null)
        {
            await _navigationService.NavigateToAsync("//Bookings");
        }
    }

    public void CreateBooking()
    {
        CreateBookingAsync().GetAwaiter().GetResult();
    }
}
