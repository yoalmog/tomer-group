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
    private string _errorMessage = string.Empty;

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
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiClient.GetHotelsAsync();
            _allHotels.Clear();

            if (response.Success && response.Data != null && response.Data.Count > 0)
            {
                _allHotels = response.Data;
            }
            else
            {
                // Fallback default hotels
                _allHotels = new List<HotelDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Palacio del Inka, A Luxury Collection Hotel",
                        HebrewName = "פלאסיו דל אינקה - קוסקו",
                        Destination = "Cusco",
                        Stars = 5,
                        HasOxygenEnrichedRooms = true,
                        HasOxygenConcentrators = true,
                        HasHeating = true,
                        IsKosherFriendly = true,
                        ShabbatFriendly = true,
                        WalkingDistanceToChabadCusco = true,
                        Phone = "+51 84 231961"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Monasterio, A Belmond Hotel",
                        HebrewName = "מונסטריו בלמונד - קוסקו",
                        Destination = "Cusco",
                        Stars = 5,
                        HasOxygenEnrichedRooms = true,
                        HasOxygenConcentrators = true,
                        HasHeating = true,
                        IsKosherFriendly = true,
                        ShabbatFriendly = true,
                        WalkingDistanceToChabadCusco = true,
                        Phone = "+51 84 604000"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Casa Andina Premium Cusco",
                        HebrewName = "קאסה אנדינה פרימיום קוסקו",
                        Destination = "Cusco",
                        Stars = 4,
                        HasOxygenEnrichedRooms = false,
                        HasOxygenConcentrators = true,
                        HasHeating = true,
                        IsKosherFriendly = true,
                        ShabbatFriendly = true,
                        WalkingDistanceToChabadCusco = true,
                        Phone = "+51 84 232610"
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Tambo del Inka Resort & Spa",
                        HebrewName = "טמבו דל אינקה - העמק הקדוש",
                        Destination = "Sacred Valley",
                        Stars = 5,
                        HasOxygenEnrichedRooms = false,
                        HasOxygenConcentrators = true,
                        HasHeating = true,
                        IsKosherFriendly = true,
                        ShabbatFriendly = false,
                        WalkingDistanceToChabadCusco = false,
                        Phone = "+51 84 581777"
                    }
                };
            }

            FilterHotels();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading hotels: {ex.Message}";
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
                h.HebrewName.ToLower().Contains(term) ||
                h.Destination.ToLower().Contains(term));
        }

        foreach (var h in query)
        {
            Hotels.Add(h);
        }
    }
}

