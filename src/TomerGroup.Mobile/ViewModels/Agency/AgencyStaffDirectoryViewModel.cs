using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyStaffDirectoryViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    private List<GuideDto> _allGuides = new();
    private List<DriverDto> _allDrivers = new();

    public AgencyStaffDirectoryViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Guides = new ObservableCollection<GuideDto>();
        Drivers = new ObservableCollection<DriverDto>();
    }

    public ObservableCollection<GuideDto> Guides { get; }
    public ObservableCollection<DriverDto> Drivers { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedRoleTab = "מדריכים"; // מדריכים / נהגים

    public List<string> RoleTabs { get; } = new() { "מדריכים", "נהגים" };

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadStaffAsync();
    }

    [RelayCommand]
    public async Task LoadStaffAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var guidesResponse = await _apiClient.GetGuidesAsync();
            _allGuides.Clear();
            if (guidesResponse.Success && guidesResponse.Data != null && guidesResponse.Data.Count > 0)
            {
                _allGuides = guidesResponse.Data;
            }
            else
            {
                // Default fallback guides
                _allGuides = new List<GuideDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Carlos Quispe Mendoza",
                        HebrewName = "קרלוס קיספה (דובר עברית)",
                        Phone = "+51 984 555 666",
                        WhatsApp = "+51984555666",
                        CertificationNumber = "DIRCETUR-CUS-1049",
                        DailyRate = 120,
                        Rating = 4.9,
                        FirstAidCertified = true,
                        IsJewishHeritageExpert = true,
                        Languages = new() { "Hebrew", "English", "Spanish", "Quechua" },
                        Specialization = "Inca History, High Altitude Trekking & Jewish Heritage",
                        IsAvailable = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Yossi Ben-David",
                        HebrewName = "יוסי בן-דוד",
                        Phone = "+51 984 777 888",
                        WhatsApp = "+51984777888",
                        CertificationNumber = "DIRCETUR-CUS-2084",
                        DailyRate = 150,
                        Rating = 5.0,
                        FirstAidCertified = true,
                        IsJewishHeritageExpert = true,
                        Languages = new() { "Hebrew", "English", "Spanish" },
                        Specialization = "Salkantay Trek & High-Altitude Acclimatization",
                        IsAvailable = true
                    }
                };
            }

            var driversResponse = await _apiClient.GetDriversAsync();
            _allDrivers.Clear();
            if (driversResponse.Success && driversResponse.Data != null && driversResponse.Data.Count > 0)
            {
                _allDrivers = driversResponse.Data;
            }
            else
            {
                _allDrivers = new List<DriverDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Juan Huaman Ortiz",
                        Phone = "+51 984 112 233",
                        WhatsApp = "+51984112233",
                        LicenseNumber = "Q482910",
                        LicenseCategory = "A-IIIa Profesional",
                        Rating = 4.9,
                        IsAvailable = true,
                        Vehicles = new()
                        {
                            new() { Model = "Mercedes-Benz Sprinter 2024", LicensePlate = "X4T-892", PassengerCapacity = 19 }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Marco Choque Ttito",
                        Phone = "+51 984 334 455",
                        WhatsApp = "+51984334455",
                        LicenseNumber = "Q102938",
                        LicenseCategory = "A-IIb Profesional",
                        Rating = 4.8,
                        IsAvailable = true,
                        Vehicles = new()
                        {
                            new() { Model = "Hyundai H1 Grand Starex", LicensePlate = "B9Z-415", PassengerCapacity = 8 }
                        }
                    }
                };
            }

            FilterStaff();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading staff directory: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void SelectTab(string tab)
    {
        SelectedRoleTab = tab;
        FilterStaff();
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilterStaff();
    }

    private void FilterStaff()
    {
        Guides.Clear();
        Drivers.Clear();

        var term = SearchQuery.Trim().ToLower();

        if (SelectedRoleTab == "מדריכים")
        {
            var filteredGuides = _allGuides.Where(g =>
                string.IsNullOrWhiteSpace(term) ||
                g.FullName.ToLower().Contains(term) ||
                (g.HebrewName != null && g.HebrewName.ToLower().Contains(term)) ||
                g.CertificationNumber.ToLower().Contains(term) ||
                g.Specialization.ToLower().Contains(term));

            foreach (var g in filteredGuides)
            {
                Guides.Add(g);
            }
        }
        else
        {
            var filteredDrivers = _allDrivers.Where(d =>
                string.IsNullOrWhiteSpace(term) ||
                d.FullName.ToLower().Contains(term) ||
                (d.LicenseNumber != null && d.LicenseNumber.ToLower().Contains(term)));

            foreach (var d in filteredDrivers)
            {
                Drivers.Add(d);
            }
        }
    }
}

