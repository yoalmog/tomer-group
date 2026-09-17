using System.Collections.ObjectModel;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class TravelConciergeViewModel
{
    public string AssistantName { get; } = "Sofia Alvarez";

    public string StatusMessage { get; } = "Your dedicated travel designer is ready to help.";

    public ObservableCollection<ConciergeService> Services { get; } = new()
    {
        new ConciergeService { Title = "Private airport transfer", Description = "Meet-and-greet on arrival with luxury vehicle." },
        new ConciergeService { Title = "Restaurant reservations", Description = "Curated dining experiences and rooftop bookings." },
        new ConciergeService { Title = "VIP support line", Description = "Priority access to your travel advisor anytime." }
    };
}

public class ConciergeService
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
