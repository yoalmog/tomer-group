using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
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

public partial class BookingsViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;
    private readonly IDestinationImageService? _imageService;

    public BookingsViewModel(
        IApiClient apiClient,
        INavigationService navigationService,
        IDestinationImageService? imageService = null)
        : this(new LocalizationService(), navigationService, apiClient, imageService)
    {
    }

    public BookingsViewModel(
        ILocalizationService localization,
        INavigationService navigationService,
        IApiClient apiClient,
        IDestinationImageService? imageService = null)
        : base(localization, navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        _imageService = imageService;
        Bookings = new ObservableCollection<BookingDto>();
        HotelBookings = new ObservableCollection<HotelBookingDto>();
        Transfers = new ObservableCollection<TransportationDto>();
        DisplayCards = new ObservableCollection<RichBookingCard>();

        UpdateEmptyTexts();
    }

    public ObservableCollection<BookingDto> Bookings { get; }
    public ObservableCollection<HotelBookingDto> HotelBookings { get; }
    public ObservableCollection<TransportationDto> Transfers { get; }
    public ObservableCollection<RichBookingCard> DisplayCards { get; }

    [ObservableProperty]
    private bool _isAuthenticated;

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

    public string PageTitle => CurrentLanguage switch
    {
        "en" => "My Bookings & Vouchers",
        "es" => "Mis Reservas y Vouchers",
        _ => "ההזמנות והשוברים שלי"
    };

    public string GatewayTitle => CurrentLanguage switch
    {
        "en" => "Your Bookings & Vouchers",
        "es" => "Tus Reservas y Vouchers",
        _ => "ההזמנות והשוברים שלך"
    };

    public string GatewayDescription => CurrentLanguage switch
    {
        "en" => "Log in to view all your hotel vouchers, train permits, and private transfers confirmed for your journey in Peru.",
        "es" => "Inicia sesión para ver todos tus vouchers de hoteles, boletos de tren y traslados privados confirmados en Perú.",
        _ => "התחבר כדי לצפות בכל שוברי המלונות, אישורי הרכבות וההעברות הפרטיות שאושרו עבורך בפרו."
    };

    public string SignInButtonText => CurrentLanguage switch
    {
        "en" => "Sign In to View Bookings",
        "es" => "Iniciar Sesión para Ver Reservas",
        _ => "התחבר לצפייה בהזמנות"
    };

    public string ContinueExploringText => CurrentLanguage switch
    {
        "en" => "Explore Destinations ←",
        "es" => "Explorar Destinos ←",
        _ => "המשך לגלות יעדים ←"
    };

    public string ContactAgencyText => CurrentLanguage switch
    {
        "en" => "Contact Tomer Group 💬",
        "es" => "Contactar a la Agencia 💬",
        _ => "צור קשר עם הסוכנות 💬"
    };

    public string SummaryBannerStatus => CurrentLanguage switch
    {
        "en" => "Services & Bookings Status",
        "es" => "Estado de Servicios y Reservas",
        _ => "סטטוס שירותים והזמנות"
    };

    public string SummaryBannerTitle => CurrentLanguage switch
    {
        "en" => "All your bookings and hotels are organized",
        "es" => "Todas tus reservas y hoteles están organizados",
        _ => "כל ההזמנות והמלונות שלך מסודרים"
    };

    public string SummaryBannerSubtitle => CurrentLanguage switch
    {
        "en" => "Original service vouchers available offline without internet",
        "es" => "Vouchers oficiales disponibles sin conexión a internet",
        _ => "שוברי שירות מקוריים זמינים גם ללא חיבור אינטרנט"
    };

    public string ConfirmedBadgeText => CurrentLanguage switch
    {
        "en" => "✓ Confirmed",
        "es" => "✓ Confirmado",
        _ => "✓ מאושר"
    };

    public string ServicesSectionTitle => CurrentLanguage switch
    {
        "en" => "Services & Travel Vouchers",
        "es" => "Servicios y Vouchers de Viaje",
        _ => "שירותים ושוברי נסיעה"
    };

    public string DetailsAndVoucherButtonText => CurrentLanguage switch
    {
        "en" => "Details & Voucher ←",
        "es" => "Detalles y Voucher ←",
        _ => "פרטים ושובר ←"
    };

    private void UpdateEmptyTexts()
    {
        EmptyTitle = CurrentLanguage switch
        {
            "en" => "No Bookings Yet",
            "es" => "Sin Reservas Aún",
            _ => "אין עדיין הזמנות"
        };
        EmptyDescription = CurrentLanguage switch
        {
            "en" => "Your bookings and service vouchers will appear here once confirmed by the agency team.",
            "es" => "Tus reservas y vouchers de servicio aparecerán aquí una vez confirmados por el equipo.",
            _ => "הזמנות ושוברי השירות של הטיול שלך יופיעו כאן ברגע שהסוכנות תאשר את ההזמנה."
        };
    }

    protected override void OnLanguageChanged()
    {
        UpdateEmptyTexts();
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(GatewayTitle));
        OnPropertyChanged(nameof(GatewayDescription));
        OnPropertyChanged(nameof(SignInButtonText));
        OnPropertyChanged(nameof(ContinueExploringText));
        OnPropertyChanged(nameof(ContactAgencyText));
        OnPropertyChanged(nameof(SummaryBannerStatus));
        OnPropertyChanged(nameof(SummaryBannerTitle));
        OnPropertyChanged(nameof(SummaryBannerSubtitle));
        OnPropertyChanged(nameof(ConfirmedBadgeText));
        OnPropertyChanged(nameof(ServicesSectionTitle));
        OnPropertyChanged(nameof(DetailsAndVoucherButtonText));
    }

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
