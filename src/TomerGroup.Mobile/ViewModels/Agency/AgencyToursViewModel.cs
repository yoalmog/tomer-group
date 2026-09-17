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

            if (response.Success && response.Data != null)
            {
                _allTours = response.Data;
            }

            FilterTours();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading tours: {ex.Message}";
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
}
