using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyHotelsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;
    private List<HotelDto> _allHotels = new();

    public AgencyHotelsViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Hotels = new ObservableCollection<HotelDto>();
    }

    public ObservableCollection<HotelDto> Hotels { get; }

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

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadHotelsAsync();
    }

    [RelayCommand]
    public async Task LoadHotelsAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiClient.GetHotelsAsync();
            _allHotels.Clear();

            if (response.Success && response.Data != null)
            {
                _allHotels = response.Data;
            }

            FilterHotels();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading hotels: {ex.Message}";
            FilterHotels();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilterHotels();
    }

    private void FilterHotels()
    {
        Hotels.Clear();
        var query = _allHotels.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var term = SearchQuery.Trim().ToLower();
            query = query.Where(h =>
                h.Name.ToLower().Contains(term) ||
                (h.HebrewName != null && h.HebrewName.ToLower().Contains(term)) ||
                (h.Destination != null && h.Destination.ToLower().Contains(term)));
        }

        foreach (var h in query)
        {
            Hotels.Add(h);
        }

        IsEmpty = Hotels.Count == 0;
    }
}
