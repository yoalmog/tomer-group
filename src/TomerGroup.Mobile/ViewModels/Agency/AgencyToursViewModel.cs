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
    private string _errorMessage = string.Empty;

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
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiClient.GetToursAsync();
            _allTours.Clear();

            if (response.Success && response.Data != null && response.Data.Count > 0)
            {
                _allTours = response.Data;
            }
            else
            {
                // Fallback default tours
                _allTours = new List<TourDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Machu Picchu Classic Citadel Tour",
                        HebrewName = "מאצ'ו פיצ'ו סיור מצודה קלאסי",
                        Destination = "Machu Picchu",
                        Category = TourCategory.DayTour,
                        Duration = "Full Day",
                        Difficulty = "Moderate",
                        AdultPrice = 380,
                        AltitudeMaxMeters = 2430,
                        KosherFoodAvailable = true,
                        KosherCertificationDetails = "Glatt Kosher lunch box from Chabad Cusco"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Salkantay Trek to Machu Picchu",
                        HebrewName = "טרק סלקנטאי למאצ'ו פיצ'ו 5 ימים",
                        Destination = "Salkantay",
                        Category = TourCategory.Trek,
                        Duration = "5 Days / 4 Nights",
                        Difficulty = "Challenging",
                        AdultPrice = 650,
                        AltitudeMaxMeters = 4630,
                        RequiresAcclimatization = true,
                        KosherFoodAvailable = true,
                        KosherCertificationDetails = "Kosher trail cooking available"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Rainbow Mountain & Red Valley Trek",
                        HebrewName = "הר שבעת הצבעים ועמק האדום",
                        Destination = "Rainbow Mountain",
                        Category = TourCategory.Trek,
                        Duration = "Full Day",
                        Difficulty = "Strenuous",
                        AdultPrice = 120,
                        AltitudeMaxMeters = 5036,
                        RequiresAcclimatization = true,
                        KosherFoodAvailable = true,
                        KosherCertificationDetails = "Kosher breakfast and packed lunch"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Sacred Valley & Pisac Market",
                        HebrewName = "העמק הקדוש ושוק פיסאק",
                        Destination = "Sacred Valley",
                        Category = TourCategory.Cultural,
                        Duration = "Full Day",
                        Difficulty = "Easy",
                        AdultPrice = 150,
                        AltitudeMaxMeters = 2972,
                        KosherFoodAvailable = true,
                        KosherCertificationDetails = "Kosher options certified by Chabad"
                    }
                };
            }

            FilterTours();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading tours: {ex.Message}";
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
    }
}

