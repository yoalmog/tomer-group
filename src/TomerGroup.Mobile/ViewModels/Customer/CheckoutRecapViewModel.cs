using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class CheckoutRecapViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _tripName = "Cusco & Sacred Valley";

    [ObservableProperty]
    private string _confirmationCode = string.Empty;

    [ObservableProperty]
    private string _totalAmount = "$2,890";

    [ObservableProperty]
    private string _depositAmount = "$500";

    [ObservableProperty]
    private string _extrasLabel = "3 premium additions";

    [ObservableProperty]
    private string _statusText = "Payment confirmed and itinerary locked in.";

    [ObservableProperty]
    private string _travelerSummary = "Personalized Peru Tour";

    [ObservableProperty]
    private string _nextStep = "Your advisor will send vouchers and final confirmations.";

    public CheckoutRecapViewModel()
        : this(new LocalizationService(), new NavigationService())
    {
    }

    public CheckoutRecapViewModel(ILocalizationService localization, INavigationService navigation)
        : base(localization, navigation)
    {
        Title = Localize(LocalizationKeys.NavBookings);
        ConfirmationCode = "TG-" + DateTime.UtcNow.ToString("yyMM") + "-" + new Random().Next(1000, 9999);
    }

    [RelayCommand]
    public async Task ViewItineraryAsync()
    {
        await Navigation.NavigateToAsync("//MyTrip");
    }
}
