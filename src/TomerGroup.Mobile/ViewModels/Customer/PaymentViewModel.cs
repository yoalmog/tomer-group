using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class PaymentViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private decimal _totalAmount = 2890m;

    [ObservableProperty]
    private decimal _depositAmount = 1200m;

    [ObservableProperty]
    private decimal _cardAmount = 1200m;

    [ObservableProperty]
    private string _selectedMethod = "Visa ending in 2048";

    [ObservableProperty]
    private string _statusMessage = "Secure payment ready";

    public PaymentViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task PayNowAsync()
    {
        StatusMessage = $"Payment of ${CardAmount:N0} USD processed securely for your Peru booking.";
        await _navigationService.NavigateToAsync("PreTripChecklist");
    }
}
