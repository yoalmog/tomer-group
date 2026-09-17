using System.Collections.ObjectModel;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class BookingDashboardViewModel
{
    public string TripName { get; } = "Cusco & Sacred Valley";
    public string HotelName { get; } = "Casa Andina Premium Cusco";
    public string TransferTitle { get; } = "Private airport transfer";
    public string ConciergeName { get; } = "Sofia Alvarez";

    public ObservableCollection<string> ConciergeServices { get; } = new()
    {
        "Private airport transfer",
        "Restaurant reservations",
        "VIP support line"
    };
}
