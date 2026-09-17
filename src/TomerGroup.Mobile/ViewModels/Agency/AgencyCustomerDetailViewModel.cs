using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

#if USE_MAUI
using Microsoft.Maui.Controls;
#endif

namespace TomerGroup.Mobile.ViewModels.Agency;

#if USE_MAUI
public partial class AgencyCustomerDetailViewModel : ObservableObject, IQueryAttributable
#else
public partial class AgencyCustomerDetailViewModel : ObservableObject
#endif
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public AgencyCustomerDetailViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj))
        {
            if (idObj is Guid idGuid)
            {
                _ = LoadCustomerAsync(idGuid);
            }
            else if (idObj is string idStr && Guid.TryParse(idStr, out var parsedGuid))
            {
                _ = LoadCustomerAsync(parsedGuid);
            }
        }
    }

    [ObservableProperty]
    private CustomerDto? _customer;

    [ObservableProperty]
    private CustomerSensitiveDetailsDto? _sensitiveDetails;

    [ObservableProperty]
    private bool _isPassportRevealed = false;

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public string DisplayPassportNumber => IsPassportRevealed && SensitiveDetails != null
        ? SensitiveDetails.PassportNumber
        : Customer?.MaskedPassportNumber ?? string.Empty;

    [RelayCommand]
    public async Task LoadCustomerAsync(Guid customerId)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiClient.GetCustomerByIdAsync(customerId);
            if (response.Success && response.Data != null)
            {
                Customer = response.Data;
                OnPropertyChanged(nameof(DisplayPassportNumber));
            }
            else
            {
                ErrorMessage = response.Message ?? "Failed to load customer details";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ToggleRevealPassportAsync()
    {
        if (IsPassportRevealed)
        {
            IsPassportRevealed = false;
            OnPropertyChanged(nameof(DisplayPassportNumber));
            return;
        }

        if (Customer == null) return;

        IsBusy = true;
        try
        {
            var response = await _apiClient.GetSensitiveDetailsAsync(Customer.Id);
            if (response.Success && response.Data != null)
            {
                SensitiveDetails = response.Data;
                IsPassportRevealed = true;
                OnPropertyChanged(nameof(DisplayPassportNumber));
            }
            else
            {
                ErrorMessage = response.Message ?? "Could not reveal sensitive details";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }
}

