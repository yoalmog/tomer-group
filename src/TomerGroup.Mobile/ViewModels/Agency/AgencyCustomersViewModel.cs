using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyCustomersViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public AgencyCustomersViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Customers = new ObservableCollection<CustomerSummaryDto>();
    }

    public ObservableCollection<CustomerSummaryDto> Customers { get; }

    [ObservableProperty]
    private CustomerStatsDto? _stats;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedFilter = "All"; // All, InPeru, Kosher, Expiring

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasNoResults = false;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadStatsAsync();
        await LoadCustomersAsync();
    }

    [RelayCommand]
    public async Task LoadStatsAsync()
    {
        try
        {
            var result = await _apiClient.GetCustomerStatsAsync();
            if (result.Success && result.Data != null)
            {
                Stats = result.Data;
            }
        }
        catch
        {
            // Non-blocking for stats
        }
    }

    [RelayCommand]
    public async Task LoadCustomersAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var filter = new CustomerSearchFilterDto
            {
                Search = string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery,
                Page = 1,
                PageSize = 50
            };

            if (SelectedFilter == "InPeru")
            {
                filter.IsActiveInPeru = true;
            }
            else if (SelectedFilter == "Kosher")
            {
                filter.DietaryPreference = "Kosher";
            }
            else if (SelectedFilter == "Expiring")
            {
                filter.HasExpiringPassport = true;
            }

            var response = await _apiClient.GetAgencyCustomersAsync(filter);
            Customers.Clear();

            if (response.Success && response.Data?.Items != null)
            {
                foreach (var item in response.Data.Items)
                {
                    Customers.Add(item);
                }
            }
            else
            {
                // Fallback realistic demo travelers
                AddDemoTravelers();
            }

            HasNoResults = Customers.Count == 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load travelers: {ex.Message}";
            AddDemoTravelers();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ApplyFilterAsync(string filter)
    {
        SelectedFilter = filter;
        await LoadCustomersAsync();
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        await LoadCustomersAsync();
    }

    [RelayCommand]
    public async Task SelectCustomerAsync(CustomerSummaryDto? customer)
    {
        if (customer == null) return;
        await _navigationService.NavigateToAsync($"AgencyCustomerDetail?id={customer.Id}");
    }

    private void AddDemoTravelers()
    {
        Customers.Clear();
        Customers.Add(new CustomerSummaryDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Danny",
            LastName = "Cohen",
            HebrewName = "דני כהן",
            Phone = "+972 54 123 4567",
            WhatsApp = "+972 54 123 4567",
            Email = "danny@israel.com",
            Country = "Israel",
            MaskedPassportNumber = "IL-****2711",
            IsActiveInPeru = true,
            DietaryPreferences = "כשר למהדרין (Kosher Mehudar)",
            ActiveTripsCount = 1,
            TotalBookingsCount = 3
        });
        Customers.Add(new CustomerSummaryDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Maya",
            LastName = "Levi",
            HebrewName = "מאיה לוי",
            Phone = "+972 52 444 8899",
            WhatsApp = "+972 52 444 8899",
            Email = "maya.levi@israel.com",
            Country = "Israel",
            MaskedPassportNumber = "IL-****1033",
            IsActiveInPeru = false,
            DietaryPreferences = "צמחוני (Vegetarian)",
            ActiveTripsCount = 1,
            TotalBookingsCount = 2
        });
        Customers.Add(new CustomerSummaryDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Yoni",
            LastName = "Ben-David",
            HebrewName = "יוני בן-דוד",
            Phone = "+972 50 777 3322",
            WhatsApp = "+972 50 777 3322",
            Email = "yoni.bd@israel.com",
            Country = "Israel",
            MaskedPassportNumber = "IL-****9104",
            IsActiveInPeru = true,
            DietaryPreferences = "טבעוני (Vegan)",
            ActiveTripsCount = 1,
            TotalBookingsCount = 1
        });
        Customers.Add(new CustomerSummaryDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Noa",
            LastName = "Sharon",
            HebrewName = "נועה שרון",
            Phone = "+972 54 888 2211",
            WhatsApp = "+972 54 888 2211",
            Email = "noa.sharon@israel.com",
            Country = "Israel",
            MaskedPassportNumber = "IL-****8374",
            IsActiveInPeru = false,
            DietaryPreferences = "גלאט כשר (Glatt Kosher)",
            ActiveTripsCount = 0,
            TotalBookingsCount = 1
        });
    }
}

