using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class AddOnsViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool _travelInsuranceSelected = true;

    [ObservableProperty]
    private bool _airportTransferSelected = true;

    [ObservableProperty]
    private bool _vipLoungeSelected = false;

    [ObservableProperty]
    private bool _privateGuideSelected = true;

    [ObservableProperty]
    private decimal _totalAddOnPrice = 420m;

    [ObservableProperty]
    private string _statusMessage = "Concierge-ready package";

    public AddOnsViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        RecalculateTotal();
    }

    partial void OnTravelInsuranceSelectedChanged(bool value)
    {
        RecalculateTotal();
    }

    partial void OnAirportTransferSelectedChanged(bool value)
    {
        RecalculateTotal();
    }

    partial void OnVipLoungeSelectedChanged(bool value)
    {
        RecalculateTotal();
    }

    partial void OnPrivateGuideSelectedChanged(bool value)
    {
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        var total = 0m;
        if (TravelInsuranceSelected) total += 180m;
        if (AirportTransferSelected) total += 120m;
        if (VipLoungeSelected) total += 210m;
        if (PrivateGuideSelected) total += 190m;

        TotalAddOnPrice = total;
        StatusMessage = $"Selected extras: ${TotalAddOnPrice:N0} USD";
    }

    [RelayCommand]
    public async Task ContinueToPaymentAsync()
    {
        StatusMessage = $"Add-ons confirmed. Total extras: ${TotalAddOnPrice:N0} USD";
        await _navigationService.NavigateToAsync("Payment");
    }
}
