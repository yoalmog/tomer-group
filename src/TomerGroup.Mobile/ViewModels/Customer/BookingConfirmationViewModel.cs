using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class BookingConfirmationViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _confirmationNumber = "TG-PRU-2048";

    [ObservableProperty]
    private string _destination = "Cusco & Sacred Valley";

    [ObservableProperty]
    private string _tripDates = "18 Oct 2026 – 24 Oct 2026";

    [ObservableProperty]
    private int _travelers = 2;

    [ObservableProperty]
    private decimal _totalAmount = 2890m;

    [ObservableProperty]
    private decimal _depositAmount = 1200m;

    [ObservableProperty]
    private string _statusMessage = "Your booking request has been prepared.";

    public BookingConfirmationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task ConfirmBookingAsync()
    {
        StatusMessage = $"Booking {ConfirmationNumber} confirmed for {Travelers} traveler(s) to {Destination}.";
        await _navigationService.NavigateToAsync("//AddOns");
    }
}
