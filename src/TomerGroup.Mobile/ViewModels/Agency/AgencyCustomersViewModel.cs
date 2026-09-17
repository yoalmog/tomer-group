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
    private bool _hasError = false;

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
        HasError = false;
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

            HasNoResults = Customers.Count == 0;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Could not load travelers: {ex.Message}";
            HasNoResults = true;
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
}
