using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyFinanceViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public AgencyFinanceViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        RecentPayments = new ObservableCollection<PaymentDto>();
    }

    public ObservableCollection<PaymentDto> RecentPayments { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private decimal _totalRevenueUsd = 0;

    [ObservableProperty]
    private decimal _totalCollectedUsd = 0;

    [ObservableProperty]
    private decimal _totalOutstandingUsd = 0;

    [ObservableProperty]
    private decimal _totalExpensesUsd = 0;

    [ObservableProperty]
    private decimal _netProfitUsd = 0;

    [ObservableProperty]
    private decimal _marginPercentage = 0;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadFinancialsAsync();
    }

    [RelayCommand]
    public async Task LoadFinancialsAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiClient.GetFinancialSummaryAsync();
            RecentPayments.Clear();

            if (response.Success && response.Data != null)
            {
                TotalRevenueUsd = response.Data.TotalRevenueUsd;
                TotalCollectedUsd = response.Data.TotalCollectedUsd;
                TotalOutstandingUsd = response.Data.TotalOutstandingUsd;
                TotalExpensesUsd = response.Data.TotalExpensesUsd;
                NetProfitUsd = response.Data.NetProfitUsd;
                MarginPercentage = response.Data.MarginPercentage;

                if (response.Data.RecentPayments != null)
                {
                    foreach (var p in response.Data.RecentPayments)
                    {
                        RecentPayments.Add(p);
                    }
                }
            }

            IsEmpty = RecentPayments.Count == 0;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading financials: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
