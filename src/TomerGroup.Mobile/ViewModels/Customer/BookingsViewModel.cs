using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class RichBookingCard
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string BookingType { get; set; } = "חבילה";
    public string DateFormatted { get; set; } = string.Empty;
    public string Status { get; set; } = "מאושר";
    public string StatusColor { get; set; } = "#10B981";
    public string ImageUrl { get; set; } = string.Empty;
    public string ConfirmationCode { get; set; } = string.Empty;
    public int TravelersCount { get; set; } = 2;
    public string PriceFormatted { get; set; } = string.Empty;
    public object? RawData { get; set; }
}

public partial class BookingsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;
    private readonly IDestinationImageService? _imageService;

    public BookingsViewModel(
        IApiClient apiClient,
        INavigationService navigationService,
        IDestinationImageService? imageService = null)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        _imageService = imageService;
        Bookings = new ObservableCollection<BookingDto>();
        HotelBookings = new ObservableCollection<HotelBookingDto>();
        Transfers = new ObservableCollection<TransportationDto>();
        DisplayCards = new ObservableCollection<RichBookingCard>();
    }

    public ObservableCollection<BookingDto> Bookings { get; }
    public ObservableCollection<HotelBookingDto> HotelBookings { get; }
    public ObservableCollection<TransportationDto> Transfers { get; }
    public ObservableCollection<RichBookingCard> DisplayCards { get; }

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private decimal _totalOutstanding;

    [ObservableProperty]
    private bool _hasBookings = false;

    [ObservableProperty]
    private string _selectedFilter = "הכל";

    [ObservableProperty]
    private string _emptyTitle = "אין עדיין הזמנות";

    [ObservableProperty]
    private string _emptyDescription = "הזמנות ושוברי השירות של הטיול שלך יופיעו כאן ברגע שהסוכנות תאשר את ההזמנה.";

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsAuthenticated = _apiClient.IsAuthenticated;
        if (!IsAuthenticated)
        {
            HasBookings = false;
            Bookings.Clear();
            HotelBookings.Clear();
            Transfers.Clear();
            DisplayCards.Clear();
            return;
        }

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
            DisplayCards.Clear();

            var bookingsResponse = await _apiClient.GetCustomerBookingsAsync(Guid.Empty);
            if (bookingsResponse.Success && bookingsResponse.Data != null && bookingsResponse.Data.Count > 0)
            {
                foreach (var b in bookingsResponse.Data)
                {
                    Bookings.Add(b);
                    DisplayCards.Add(new RichBookingCard
                    {
                        Id = b.Id,
                        Title = "חבילת מסע לפרו • Tomer Group",
                        Subtitle = "כולל שירותי קרקע, הדרכה ותיאומים",
                        BookingType = "חבילת מסע",
                        DateFormatted = $"{b.StartDate:dd/MM/yyyy}",
                        Status = b.Status.ToString(),
                        StatusColor = "#10B981",
                        ImageUrl = _imageService?.GetMachuPicchuImage() ?? "https://images.unsplash.com/photo-1526392060635-9d6019884377?q=80&w=800",
                        ConfirmationCode = b.BookingCode,
                        TravelersCount = 2,
                        PriceFormatted = $"${b.TotalAmount:N0} USD",
                        RawData = b
                    });
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
                    DisplayCards.Add(new RichBookingCard
                    {
                        Id = h.Id,
                        Title = h.HotelName,
                        Subtitle = $"{h.Destination} • {h.RoomType}",
                        BookingType = "מלון",
                        DateFormatted = $"{h.CheckInDate:dd/MM} — {h.CheckOutDate:dd/MM/yyyy}",
                        Status = "מאושר ומסודר",
                        StatusColor = "#0284C7",
                        ImageUrl = _imageService?.GetActivityImage("hotel", "hotel") ?? "https://images.unsplash.com/photo-1566073771259-6a8506099945?q=80&w=800",
                        ConfirmationCode = h.ConfirmationNumber ?? "אישור מאושר",
                        TravelersCount = h.NumberOfGuests,
                        PriceFormatted = "כלול בחבילה",
                        RawData = h
                    });
                }
                HasBookings = true;
            }

            var transfersResponse = await _apiClient.GetCustomerTransfersAsync(Guid.Empty);
            if (transfersResponse.Success && transfersResponse.Data != null && transfersResponse.Data.Count > 0)
            {
                foreach (var t in transfersResponse.Data)
                {
                    Transfers.Add(t);
                    DisplayCards.Add(new RichBookingCard
                    {
                        Id = t.Id,
                        Title = t.HebrewServiceType,
                        Subtitle = $"{t.PickupLocation} ← {t.DropoffLocation}",
                        BookingType = "העברה פרטית",
                        DateFormatted = $"{t.ScheduledPickupTime:dd/MM HH:mm}",
                        Status = "נהג ממתין",
                        StatusColor = "#166534",
                        ImageUrl = _imageService?.GetActivityImage("transfer", "transfer") ?? "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?q=80&w=800",
                        ConfirmationCode = t.VehicleModel ?? "העברה פרטית",
                        TravelersCount = 2,
                        PriceFormatted = "כלול בחבילה",
                        RawData = t
                    });
                }
                HasBookings = true;
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
    public async Task OpenBookingDetailAsync(RichBookingCard card)
    {
        if (card == null) return;
        await _navigationService.NavigateToAsync($"BookingDetail?id={card.Id}&title={Uri.EscapeDataString(card.Title)}&type={Uri.EscapeDataString(card.BookingType)}&code={Uri.EscapeDataString(card.ConfirmationCode)}");
    }

    [RelayCommand]
    public async Task OpenSignInAsync()
    {
        await _navigationService.NavigateToLoginAsync();
    }

    [RelayCommand]
    public async Task ContinueExploringAsync()
    {
        await _navigationService.NavigateToAsync("//Home");
    }

    [RelayCommand]
    public async Task ContactAgencyAsync()
    {
        await _navigationService.NavigateToAsync("More");
    }
}
