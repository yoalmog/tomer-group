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
            }
            else
            {
                // Default booking for Danny Cohen
                var defaultBooking = new BookingDto
                {
                    Id = Guid.NewGuid(),
                    BookingCode = "TG-2026-00482",
                    CustomerName = "Danny Cohen",
                    StartDate = DateTime.UtcNow.AddDays(2),
                    EndDate = DateTime.UtcNow.AddDays(12),
                    TotalAmount = 2500,
                    PaidAmount = 2500,
                    Currency = Core.Enums.Currency.USD,
                    Status = Core.Enums.BookingStatus.Confirmed,
                    PaymentStatus = Core.Enums.PaymentStatus.Paid
                };
                Bookings.Add(defaultBooking);
                TotalOutstanding = 0;
            }

            var hotelsResponse = await _apiClient.GetCustomerHotelBookingsAsync(Guid.Empty);
            if (hotelsResponse.Success && hotelsResponse.Data != null && hotelsResponse.Data.Count > 0)
            {
                foreach (var h in hotelsResponse.Data)
                {
                    HotelBookings.Add(h);
                }
            }
            else
            {
                // Default hotel booking
                HotelBookings.Add(new HotelBookingDto
                {
                    Id = Guid.NewGuid(),
                    HotelName = "Palacio del Inka, Luxury Collection",
                    Destination = "Cusco",
                    RoomType = "Deluxe Oxygen-Enriched",
                    ConfirmationNumber = "HTL-2026-00102",
                    CheckInDate = DateTime.UtcNow.AddDays(2),
                    CheckOutDate = DateTime.UtcNow.AddDays(5),
                    NumberOfGuests = 2,
                    OxygenRoomRequested = true,
                    Status = "Confirmed",
                    SpecialRequests = "Shabbat mechanical key, upper floor oxygen room"
                });
            }

            var transfersResponse = await _apiClient.GetCustomerTransfersAsync(Guid.Empty);
            if (transfersResponse.Success && transfersResponse.Data != null && transfersResponse.Data.Count > 0)
            {
                foreach (var t in transfersResponse.Data)
                {
                    Transfers.Add(t);
                }
            }
            else
            {
                // Default transfer
                Transfers.Add(new TransportationDto
                {
                    Id = Guid.NewGuid(),
                    ServiceType = "Airport Transfer",
                    HebrewServiceType = "איסוף משדה התעופה קוסקו",
                    PickupLocation = "Alejandro Velasco Astete Airport (CUZ)",
                    DropoffLocation = "Palacio del Inka Hotel, Cusco",
                    ScheduledPickupTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10).AddMinutes(30),
                    DriverName = "Carlos Quispe Mendoza",
                    DriverPhone = "+51 984 555 666",
                    DriverWhatsApp = "+51984555666",
                    VehicleModel = "Mercedes-Benz Sprinter 2024",
                    VehiclePlate = "X4T-892",
                    Status = Core.Enums.TransportationStatus.Assigned
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading bookings: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

