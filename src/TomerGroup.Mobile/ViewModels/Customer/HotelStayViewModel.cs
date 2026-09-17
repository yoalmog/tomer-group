using System.Collections.ObjectModel;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class HotelStayViewModel
{
    public string HotelName { get; } = "Casa Andina Premium Cusco";

    public string StayWindow { get; } = "18 Oct 2026 – 21 Oct 2026";

    public string RoomType { get; } = "Deluxe city view suite";

    public ObservableCollection<string> Features { get; } = new()
    {
        "Breakfast included",
        "Rooftop spa",
        "Airport pickup arranged"
    };
}
