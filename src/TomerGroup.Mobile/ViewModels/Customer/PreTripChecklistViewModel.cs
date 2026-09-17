using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class PreTripChecklistViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _passportReady = true;

    [ObservableProperty]
    private bool _insuranceReady = true;

    [ObservableProperty]
    private bool _flightConfirmationReady = true;

    [ObservableProperty]
    private bool _hotelVouchersReady = true;

    [ObservableProperty]
    private string _statusMessage = "Checklist almost complete";

    public PreTripChecklistViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        UpdateStatus();
    }

    partial void OnPassportReadyChanged(bool value)
    {
        UpdateStatus();
    }

    partial void OnInsuranceReadyChanged(bool value)
    {
        UpdateStatus();
    }

    partial void OnFlightConfirmationReadyChanged(bool value)
    {
        UpdateStatus();
    }

    partial void OnHotelVouchersReadyChanged(bool value)
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var completed = new[] { PassportReady, InsuranceReady, FlightConfirmationReady, HotelVouchersReady }.Count(x => x);
        StatusMessage = completed switch
        {
            4 => "Everything is ready for departure.",
            3 => "One item still needs attention.",
            2 => "Two items still need attention.",
            _ => "Please complete the required checklist items."
        };
    }

    [RelayCommand]
    public async Task OpenTripSummaryAsync()
    {
        await _navigationService.NavigateToAsync("TripSummary");
    }
}
