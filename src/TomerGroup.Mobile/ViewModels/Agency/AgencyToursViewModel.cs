using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyToursViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;
    private List<TourDto> _allTours = new();

    public AgencyToursViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Tours = new ObservableCollection<TourDto>();
    }

    public ObservableCollection<TourDto> Tours { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "הכל";

    public List<string> Categories { get; } = new() { "הכל", "טרקים", "סיורי יום", "תרבות", "הרפתקה" };

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadToursAsync();
    }

    [RelayCommand]
    public async Task LoadToursAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiClient.GetToursAsync();
            _allTours.Clear();

            if (response.Success && response.Data != null && response.Data.Any())
            {
                _allTours = response.Data;
            }
            else
            {
                _allTours = GetDefaultPeruTours();
            }

            FilterTours();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading tours: {ex.Message}";
            if (_allTours.Count == 0)
            {
                _allTours = GetDefaultPeruTours();
            }
            FilterTours();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void SelectCategory(string category)
    {
        SelectedCategory = category;
        FilterTours();
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilterTours();
    }

    private void FilterTours()
    {
        Tours.Clear();
        var query = _allTours.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var term = SearchQuery.Trim().ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                t.HebrewName.ToLower().Contains(term) ||
                (t.Destination != null && t.Destination.ToLower().Contains(term)));
        }

        if (SelectedCategory == "טרקים")
        {
            query = query.Where(t => t.Category == TourCategory.Trek);
        }
        else if (SelectedCategory == "סיורי יום")
        {
            query = query.Where(t => t.Category == TourCategory.DayTour);
        }
        else if (SelectedCategory == "תרבות")
        {
            query = query.Where(t => t.Category == TourCategory.Cultural);
        }
        else if (SelectedCategory == "הרפתקה")
        {
            query = query.Where(t => t.Category == TourCategory.Adventure);
        }

        foreach (var t in query)
        {
            Tours.Add(t);
        }

        IsEmpty = Tours.Count == 0;
    }

    private List<TourDto> GetDefaultPeruTours()
    {
        return new List<TourDto>
        {
            new TourDto
            {
                Id = Guid.NewGuid(),
                Name = "Salkantay Trek 5D/4N to Machu Picchu",
                HebrewName = "טרק סלקנטאי 5 ימים למאצ'ו פיצ'ו",
                Destination = "Cusco & Salkantay",
                Category = TourCategory.Trek,
                DurationDays = 5,
                MaxCapacity = 12,
                AdultPrice = 450,
                Description = "טרק מרהיב בין קרחונים, לגונות טורקיז ויער גשם."
            },
            new TourDto
            {
                Id = Guid.NewGuid(),
                Name = "Rainbow Mountain Full Day Trek",
                HebrewName = "טרק יום להר הצבעים (ויניקונקה)",
                Destination = "Cusco",
                Category = TourCategory.Trek,
                DurationDays = 1,
                MaxCapacity = 15,
                AdultPrice = 65,
                Description = "העפלה לפסגת 5,200 מטר עם תצפית אל רכס הצבעים המפורסם."
            },
            new TourDto
            {
                Id = Guid.NewGuid(),
                Name = "Classic Inca Trail 4D/3N",
                HebrewName = "שביל האינקה הקלאסי 4 ימים",
                Destination = "Cusco & Machu Picchu",
                Category = TourCategory.Trek,
                DurationDays = 4,
                MaxCapacity = 10,
                AdultPrice = 750,
                Description = "המסלול ההיסטורי של שביל האינקה אל שער השמש."
            },
            new TourDto
            {
                Id = Guid.NewGuid(),
                Name = "Sacred Valley & Pisac Market",
                HebrewName = "העמק הקדוש, שוק פיסאק ומבצר אויאנטייטמבו",
                Destination = "Sacred Valley",
                Category = TourCategory.DayTour,
                DurationDays = 1,
                MaxCapacity = 16,
                AdultPrice = 85,
                Description = "סיור יום מושלם להתאקלמות לגובה וגילוי אתרי אינקה."
            },
            new TourDto
            {
                Id = Guid.NewGuid(),
                Name = "Cusco Imperial City & 4 Ruins",
                HebrewName = "סיור אתרים אימפריאלי בקוסקו וסקסייוומאן",
                Destination = "Cusco",
                Category = TourCategory.Cultural,
                DurationDays = 1,
                MaxCapacity = 20,
                AdultPrice = 55,
                Description = "מקדש השמש קוריקנצ'ה, סקסייוומאן וכיכר הנשק."
            },
            new TourDto
            {
                Id = Guid.NewGuid(),
                Name = "Amazon Tambopata Wildlife Adventure",
                HebrewName = "משלחת הרפתקה בג'ונגל האמזונס (טמבופטה)",
                Destination = "Amazon",
                Category = TourCategory.Adventure,
                DurationDays = 3,
                MaxCapacity = 8,
                AdultPrice = 420,
                Description = "שייט נהרות, צפייה בתוכים, קיימנים ולינה בלודג' אקולוגי."
            }
        };
    }

    [RelayCommand]
    public async Task OpenTrekMapAsync(TourDto? tour)
    {
        var target = tour != null ? tour.Name : "Salkantay";
        await _navigationService.NavigateToAsync($"TrekMap?trekName={Uri.EscapeDataString(target)}");
    }
}
