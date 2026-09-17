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
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

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
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var guidesResponse = await _apiClient.GetGuidesAsync();
            _allGuides.Clear();
            if (guidesResponse.Success && guidesResponse.Data != null)
            {
                _allGuides = guidesResponse.Data;
            }

            var driversResponse = await _apiClient.GetDriversAsync();
            _allDrivers.Clear();
            if (driversResponse.Success && driversResponse.Data != null)
            {
                _allDrivers = driversResponse.Data;
            }

            FilterStaff();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading staff directory: {ex.Message}";
            FilterStaff();
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
                (g.CertificationNumber != null && g.CertificationNumber.ToLower().Contains(term)) ||
                (g.Specialization != null && g.Specialization.ToLower().Contains(term)));

            foreach (var g in filteredGuides)
            {
                Guides.Add(g);
            }

            IsEmpty = Guides.Count == 0;
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

            IsEmpty = Drivers.Count == 0;
        }
    }
}
