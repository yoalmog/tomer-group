using System.Collections.ObjectModel;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class TripSummaryViewModel
{
    public string Destination { get; } = "Cusco & Sacred Valley";

    public ObservableCollection<TripSummaryDay> ItineraryDays { get; } = new()
    {
        new TripSummaryDay
        {
            DayNumber = 1,
            Title = "Arrival to Cusco",
            Highlights = new List<string> { "Private airport transfer", "Welcome dinner", "Hotel check-in" }
        },
        new TripSummaryDay
        {
            DayNumber = 2,
            Title = "Machu Picchu day",
            Highlights = new List<string> { "Machu Picchu sunrise train", "Guided archeology circuit", "Luxury lunch" }
        },
        new TripSummaryDay
        {
            DayNumber = 3,
            Title = "Sacred Valley escape",
            Highlights = new List<string> { "Market visit", "Andean village experience", "Sunset return" }
        }
    };
}

public class TripSummaryDay
{
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new();
}
