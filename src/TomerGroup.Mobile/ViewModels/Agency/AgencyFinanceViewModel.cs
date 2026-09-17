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
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private decimal _totalRevenueUsd = 45000;

    [ObservableProperty]
    private decimal _totalCollectedUsd = 38500;

    [ObservableProperty]
    private decimal _totalOutstandingUsd = 6500;

    [ObservableProperty]
    private decimal _totalExpensesUsd = 26200;

    [ObservableProperty]
    private decimal _netProfitUsd = 18800;

    [ObservableProperty]
    private decimal _marginPercentage = 41.78m;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadFinancialsAsync();
    }

    [RelayCommand]
    public async Task LoadFinancialsAsync()
    {
        IsBusy = true;
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

            if (RecentPayments.Count == 0)
            {
                // Fallback default sample transactions
                RecentPayments.Add(new PaymentDto
                {
                    Id = Guid.NewGuid(),
                    ReceiptNumber = "REC-2026-00102",
                    BookingCode = "TG-2026-00482",
                    CustomerName = "Danny Cohen",
                    Amount = 2500,
                    Currency = Core.Enums.Currency.USD,
                    Method = Core.Enums.PaymentMethod.BankTransfer,
                    Status = Core.Enums.PaymentStatus.Paid,
                    PaymentDate = DateTime.UtcNow.AddHours(-3),
                    ReferenceNumber = "HAPOALIM-88129",
                    Notes = "Full balance payment - confirmed"
                });

                RecentPayments.Add(new PaymentDto
                {
                    Id = Guid.NewGuid(),
                    ReceiptNumber = "REC-2026-00101",
                    BookingCode = "TG-2026-00481",
                    CustomerName = "Yossi Levi",
                    Amount = 1500,
                    Currency = Core.Enums.Currency.USD,
                    Method = Core.Enums.PaymentMethod.CreditCard,
                    Status = Core.Enums.PaymentStatus.Paid,
                    PaymentDate = DateTime.UtcNow.AddDays(-1),
                    ReferenceNumber = "VISA-4421-Auth",
                    Notes = "Deposit 50% for Sacred Valley + Machu Picchu"
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading financials: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

