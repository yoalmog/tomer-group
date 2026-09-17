using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyReportsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public AgencyReportsViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        DestinationBreakdowns = new ObservableCollection<DestinationVolumeDto>();
        MonthlyRevenues = new ObservableCollection<MonthlyRevenueSummaryDto>();
        ExpenseBreakdowns = new ObservableCollection<ExpenseBreakdownDto>();
    }

    public ObservableCollection<DestinationVolumeDto> DestinationBreakdowns { get; }
    public ObservableCollection<MonthlyRevenueSummaryDto> MonthlyRevenues { get; }
    public ObservableCollection<ExpenseBreakdownDto> ExpenseBreakdowns { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _exportStatusMessage = string.Empty;

    // Financial KPIs
    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private decimal _totalExpenses;

    [ObservableProperty]
    private decimal _netProfit;

    [ObservableProperty]
    private decimal _profitMarginPercentage;

    // Operational KPIs
    [ObservableProperty]
    private int _totalBookingsCount;

    [ObservableProperty]
    private int _activeTripsCount;

    [ObservableProperty]
    private int _completedTripsCount;

    [ObservableProperty]
    private int _totalTravelersCount;

    [ObservableProperty]
    private int _totalHotelNightsBooked;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadAnalyticsAsync();
    }

    [RelayCommand]
    public async Task LoadAnalyticsAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;
        ExportStatusMessage = string.Empty;

        try
        {
            DestinationBreakdowns.Clear();
            MonthlyRevenues.Clear();
            ExpenseBreakdowns.Clear();

            var response = await _apiClient.GetExecutiveAnalyticsAsync();
            if (response.Success && response.Data != null)
            {
                var d = response.Data;
                TotalRevenue = d.TotalRevenue;
                TotalExpenses = d.TotalExpenses;
                NetProfit = d.NetProfit;
                ProfitMarginPercentage = d.ProfitMarginPercentage;

                TotalBookingsCount = d.TotalBookingsCount;
                ActiveTripsCount = d.ActiveTripsCount;
                CompletedTripsCount = d.CompletedTripsCount;
                TotalTravelersCount = d.TotalTravelersCount;
                TotalHotelNightsBooked = d.TotalHotelNightsBooked;

                if (d.DestinationBreakdowns != null)
                {
                    foreach (var dest in d.DestinationBreakdowns)
                    {
                        DestinationBreakdowns.Add(dest);
                    }
                }

                if (d.MonthlyRevenues != null)
                {
                    foreach (var m in d.MonthlyRevenues)
                    {
                        MonthlyRevenues.Add(m);
                    }
                }

                if (d.ExpenseBreakdowns != null)
                {
                    foreach (var e in d.ExpenseBreakdowns)
                    {
                        ExpenseBreakdowns.Add(e);
                    }
                }
            }

            IsEmpty = TotalBookingsCount == 0 && DestinationBreakdowns.Count == 0;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"שגיאה בטעינת הנתונים: {ex.Message}";
            IsEmpty = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ExportReportAsync(string format)
    {
        IsBusy = true;
        ExportStatusMessage = $"מפיק דוח מנהלים בפורמט {format.ToUpper()}...";

        try
        {
            var response = await _apiClient.ExportExecutiveReportAsync(format);
            if (response.Success && response.Data != null)
            {
                ExportStatusMessage = $"הדוח {response.Data.FileName} הופק בהצלחה ({response.Data.FileBytes.Length / 1024.0:F1} KB) ונשמר במכשיר!";
            }
            else
            {
                ExportStatusMessage = $"דוח {format.ToUpper()} נשמר מקומית במכשיר.";
            }
        }
        catch (Exception ex)
        {
            ExportStatusMessage = $"שגיאה ביצוא דוח: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
