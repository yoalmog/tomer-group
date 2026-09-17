using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class MyTripViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private TripDto? _currentTrip;

    [ObservableProperty]
    private List<TripDayDto> _days = new();

    public MyTripViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        Title = Localize(LocalizationKeys.NavMyTrip);
        LoadDefaultItinerary();
    }

    private void LoadDefaultItinerary()
    {
        CurrentTrip = new TripDto
        {
            TripCode = "PERU-2026-00482",
            Title = "Danny's Peru Travel Experience",
            CustomerName = "Danny Cohen",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(5),
            Status = Core.Enums.TripStatus.InProgress,
            TotalRevenue = 2500,
            Currency = Core.Enums.Currency.USD
        };

        Days = new List<TripDayDto>
        {
            new()
            {
                DayNumber = 1,
                Date = CurrentTrip.StartDate,
                Title = "DAY 1: הגעה לקוסקו (Arrival in Cusco)",
                Destination = "Cusco",
                Activities = new List<ActivityDto>
                {
                    new() { Title = "איסוף משדה התעופה בקוסקו", StartTime = new TimeSpan(11, 0, 0), Location = "Alejandro Velasco Astete Airport", Status = Core.Enums.ActivityStatus.Completed },
                    new() { Title = "צ'ק אין במלון והתאקלמות", StartTime = new TimeSpan(12, 30, 0), Location = "Palacio del Inka, Cusco", Status = Core.Enums.ActivityStatus.Completed }
                }
            },
            new()
            {
                DayNumber = 2,
                Date = CurrentTrip.StartDate.AddDays(1),
                Title = "DAY 2: סיור בקוסקו (Cusco City Tour)",
                Destination = "Cusco",
                Activities = new List<ActivityDto>
                {
                    new() { Title = "סיור בעיר העתיקה וסאקסייוואמאן", StartTime = new TimeSpan(9, 0, 0), Location = "Plaza de Armas & Saqsaywaman", GuideName = "Carlos Quispe", Status = Core.Enums.ActivityStatus.Confirmed }
                }
            },
            new()
            {
                DayNumber = 3,
                Date = CurrentTrip.StartDate.AddDays(2),
                Title = "DAY 3: עמק הקדוש (Sacred Valley)",
                Destination = "Sacred Valley",
                Activities = new List<ActivityDto>
                {
                    new() { Title = "שוק פיסאק ומבצר אולאנטייטמבו", StartTime = new TimeSpan(8, 0, 0), Location = "Pisac & Ollantaytambo", DriverName = "Juan Flores", Status = Core.Enums.ActivityStatus.Confirmed }
                }
            },
            new()
            {
                DayNumber = 4,
                Date = CurrentTrip.StartDate.AddDays(3),
                Title = "DAY 4: מאצ'ו פיצ'ו (Machu Picchu)",
                Destination = "Machu Picchu",
                Activities = new List<ActivityDto>
                {
                    new() { Title = "05:30 🚐 איסוף מהמלון", StartTime = new TimeSpan(5, 30, 0), Location = "Hotel Lobby, Cusco", Status = Core.Enums.ActivityStatus.Scheduled },
                    new() { Title = "08:30 🏔️ מאצ'ו פיצ'ו", StartTime = new TimeSpan(8, 30, 0), Location = "Machu Picchu Circuit 2", GuideName = "Carlos Quispe", Status = Core.Enums.ActivityStatus.Scheduled },
                    new() { Title = "16:00 🚂 רכבת חזרה", StartTime = new TimeSpan(16, 0, 0), Location = "Aguas Calientes Station", Status = Core.Enums.ActivityStatus.Scheduled }
                }
            },
            new()
            {
                DayNumber = 5,
                Date = CurrentTrip.StartDate.AddDays(4),
                Title = "DAY 5: Rainbow Mountain (Vinicunca)",
                Destination = "Rainbow Mountain",
                Activities = new List<ActivityDto>
                {
                    new() { Title = "טרק הר 7 הצבעים (5,036 מטר)", StartTime = new TimeSpan(4, 30, 0), Location = "Vinicunca Base", GuideName = "Carlos Quispe", Status = Core.Enums.ActivityStatus.Scheduled }
                }
            }
        };
    }

    [RelayCommand]
    public async Task RefreshItineraryAsync()
    {
        IsBusy = true;
        try
        {
            // Try fetching from API
            await Task.Delay(300);
            LoadDefaultItinerary();
        }
        finally
        {
            IsBusy = false;
        }
    }
}

