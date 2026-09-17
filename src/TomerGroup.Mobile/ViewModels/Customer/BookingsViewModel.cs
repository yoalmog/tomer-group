using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class BookingsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public BookingsViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Bookings = new ObservableCollection<BookingDto>();
        HotelBookings = new ObservableCollection<HotelBookingDto>();
        Transfers = new ObservableCollection<TransportationDto>();
    }

    public ObservableCollection<BookingDto> Bookings { get; }
    public ObservableCollection<HotelBookingDto> HotelBookings { get; }
    public ObservableCollection<TransportationDto> Transfers { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private decimal _totalOutstanding;

    [ObservableProperty]
    private bool _hasBookings = false;

    [ObservableProperty]
    private string _emptyTitle = "אין עדיין הזמנות";

    [ObservableProperty]
    private string _emptyDescription = "הזמנות חדשות שייווצרו במערכת עבור הטיול שלך יופיעו כאן עם כל פרטי המלונות וההסעות.";

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadBookingsAsync();
    }

    [RelayCommand]
    public async Task LoadBookingsAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            Bookings.Clear();
            HotelBookings.Clear();
            Transfers.Clear();

            var bookingsResponse = await _apiClient.GetCustomerBookingsAsync(Guid.Empty);
            if (bookingsResponse.Success && bookingsResponse.Data != null && bookingsResponse.Data.Count > 0)
            {
                foreach (var b in bookingsResponse.Data)
                {
                    Bookings.Add(b);
                }
                TotalOutstanding = Bookings.Sum(b => b.OutstandingAmount);
                HasBookings = true;
            }
            else
            {
                TotalOutstanding = 0;
                HasBookings = false;
            }

            var hotelsResponse = await _apiClient.GetCustomerHotelBookingsAsync(Guid.Empty);
            if (hotelsResponse.Success && hotelsResponse.Data != null && hotelsResponse.Data.Count > 0)
            {
                foreach (var h in hotelsResponse.Data)
                {
                    HotelBookings.Add(h);
                }
            }

            var transfersResponse = await _apiClient.GetCustomerTransfersAsync(Guid.Empty);
            if (transfersResponse.Success && transfersResponse.Data != null && transfersResponse.Data.Count > 0)
            {
                foreach (var t in transfersResponse.Data)
                {
                    Transfers.Add(t);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"שגיאה בטעינת ההזמנות: {ex.Message}";
            HasBookings = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ContactAgencyAsync()
    {
        await _navigationService.NavigateToAsync("//More");
    }
}
