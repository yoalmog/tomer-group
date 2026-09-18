using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

#if USE_MAUI
using Microsoft.Maui.Controls;
#endif

namespace TomerGroup.Mobile.ViewModels.Customer;

#if USE_MAUI
public partial class BookingDetailViewModel : BaseViewModel, IQueryAttributable
#else
public partial class BookingDetailViewModel : BaseViewModel
#endif
{
    private readonly IDestinationImageService _imageService;

    [ObservableProperty]
    private string _bookingTitle = string.Empty;

    [ObservableProperty]
    private string _bookingType = string.Empty;

    [ObservableProperty]
    private string _confirmationCode = "TG-2026-CONFIRMED";

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _headerImage = string.Empty;

    [ObservableProperty]
    private string _dates = string.Empty;

    [ObservableProperty]
    private string _provider = "Tomer Group Peru Travel Experience";

    [ObservableProperty]
    private string _location = "Cusco, Peru";

    [ObservableProperty]
    private string _travelers = string.Empty;

    [ObservableProperty]
    private string _specialRequests = string.Empty;

    [ObservableProperty]
    private string _voucherNotes = string.Empty;

    public string BackButtonText => CurrentLanguage switch
    {
        "en" => "← Back to Bookings",
        "es" => "← Volver a Reservas",
        _ => "← חזרה להזמנות"
    };

    public string VoucherCodeHeader => CurrentLanguage switch
    {
        "en" => "OFFICIAL CONFIRMATION NUMBER / VOUCHER CODE",
        "es" => "NÚMERO OFICIAL DE CONFIRMACIÓN / CÓDIGO DE VOUCHER",
        _ => "מספר אישור רשמי / VOUCHER CODE"
    };

    public string DatesLabel => CurrentLanguage switch
    {
        "en" => "📅 Service Dates",
        "es" => "📅 Fechas del Servicio",
        _ => "📅 תאריכי השירות"
    };

    public string TravelersLabel => CurrentLanguage switch
    {
        "en" => "👥 Travelers",
        "es" => "👥 Pasajeros",
        _ => "👥 נוסעים"
    };

    public string ProviderAndLocationTitle => CurrentLanguage switch
    {
        "en" => "Service Provider & Location",
        "es" => "Proveedor del Servicio y Ubicación",
        _ => "ספק שירות ומיקום"
    };

    public string ApprovedRequestsTitle => CurrentLanguage switch
    {
        "en" => "✨ Approved Preferences & Notes",
        "es" => "✨ Preferencias y Solicitudes Confirmadas",
        _ => "✨ בקשות ודגשים שאושרו"
    };

    public string VoucherNotesTitle => CurrentLanguage switch
    {
        "en" => "📋 Voucher Instructions",
        "es" => "📋 Instrucciones del Voucher",
        _ => "📋 הנחיות שימוש בשובר"
    };

    public string ContactSupportButtonText => CurrentLanguage switch
    {
        "en" => "Contact Reservations Team 💬",
        "es" => "Contactar Equipo de Reservas 💬",
        _ => "צור קשר עם מוקד ההזמנות 💬"
    };

    public BookingDetailViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _imageService = imageService;
        Title = CurrentLanguage switch
        {
            "en" => "Voucher Details",
            "es" => "Detalles del Voucher",
            _ => "פרטי שובר"
        };
        HeaderImage = _imageService.GetMachuPicchuImage();
        ApplyLocalizedDefaults();
    }

    private void ApplyLocalizedDefaults()
    {
        if (string.IsNullOrWhiteSpace(BookingTitle) || BookingTitle == "פרטי הזמנה ושובר" || BookingTitle == "Booking & Voucher Details" || BookingTitle == "Detalles de Reserva y Voucher")
        {
            BookingTitle = CurrentLanguage switch
            {
                "en" => "Booking & Voucher Details",
                "es" => "Detalles de Reserva y Voucher",
                _ => "פרטי הזמנה ושובר"
            };
        }

        if (string.IsNullOrWhiteSpace(BookingType) || BookingType == "חבילת שירות" || BookingType == "Service Package" || BookingType == "Paquete de Servicio")
        {
            BookingType = CurrentLanguage switch
            {
                "en" => "Service Package",
                "es" => "Paquete de Servicio",
                _ => "חבילת שירות"
            };
        }

        if (string.IsNullOrWhiteSpace(Status) || Status == "מאושר ומסודר" || Status == "Confirmed & Ready" || Status == "Confirmado y Listo")
        {
            Status = CurrentLanguage switch
            {
                "en" => "Confirmed & Ready",
                "es" => "Confirmado y Listo",
                _ => "מאושר ומסודר"
            };
        }

        if (string.IsNullOrWhiteSpace(Dates) || Dates == "ספטמבר 2026" || Dates == "September 2026" || Dates == "Septiembre 2026")
        {
            Dates = CurrentLanguage switch
            {
                "en" => "September 2026",
                "es" => "Septiembre 2026",
                _ => "ספטמבר 2026"
            };
        }

        if (string.IsNullOrWhiteSpace(Travelers) || Travelers.Contains("נוסעים") || Travelers.Contains("Travelers") || Travelers.Contains("Pasajeros"))
        {
            Travelers = CurrentLanguage switch
            {
                "en" => "2 Travelers",
                "es" => "2 Pasajeros",
                _ => "2 נוסעים"
            };
        }

        if (string.IsNullOrWhiteSpace(Location) || Location == "קוסקו, פרו" || Location == "Cusco, Peru")
        {
            Location = CurrentLanguage switch
            {
                "en" => "Cusco, Peru",
                "es" => "Cusco, Perú",
                _ => "קוסקו, פרו"
            };
        }

        if (string.IsNullOrWhiteSpace(SpecialRequests) || SpecialRequests.Contains("חדר מועשר") || SpecialRequests.Contains("Oxygen-enriched") || SpecialRequests.Contains("Habitación con oxígeno"))
        {
            SpecialRequests = CurrentLanguage switch
            {
                "en" => "Oxygen-enriched room, kosher Shabbat meals coordinated with Chabad, private transfers only.",
                "es" => "Habitación enriquecida con oxígeno, comidas kosher de Shabat coordinadas con Jabad, solo traslados privados.",
                _ => "חדר מועשר בחמצן, ארוחות שבת כשרות בתיאום עם חב\"ד, העברות פרטיות בלבד"
            };
        }

        if (string.IsNullOrWhiteSpace(VoucherNotes) || VoucherNotes.Contains("שובר זה מהווה") || VoucherNotes.Contains("This voucher serves as an official") || VoucherNotes.Contains("Este voucher constituye confirmación oficial"))
        {
            VoucherNotes = CurrentLanguage switch
            {
                "en" => "This voucher serves as an official confirmation from Tomer Group. Please present this voucher and original passports upon service check-in.",
                "es" => "Este voucher constituye confirmación oficial de Tomer Group. Por favor presente este voucher y pasaportes originales al recibir el servicio.",
                _ => "שובר זה מהווה אישור רשמי של Tomer Group. יש להציג שובר זה ודרכונים מקוריים בעת קבלת השירות."
            };
        }
    }

    protected override void OnLanguageChanged()
    {
        Title = CurrentLanguage switch
        {
            "en" => "Voucher Details",
            "es" => "Detalles del Voucher",
            _ => "פרטי שובר"
        };
        ApplyLocalizedDefaults();
        OnPropertyChanged(nameof(BackButtonText));
        OnPropertyChanged(nameof(VoucherCodeHeader));
        OnPropertyChanged(nameof(DatesLabel));
        OnPropertyChanged(nameof(TravelersLabel));
        OnPropertyChanged(nameof(ProviderAndLocationTitle));
        OnPropertyChanged(nameof(ApprovedRequestsTitle));
        OnPropertyChanged(nameof(VoucherNotesTitle));
        OnPropertyChanged(nameof(ContactSupportButtonText));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("title", out var titleObj) && titleObj is string titleStr)
        {
            BookingTitle = Uri.UnescapeDataString(titleStr);
            HeaderImage = _imageService.GetActivityImage(BookingTitle, BookingType);
        }

        if (query.TryGetValue("type", out var typeObj) && typeObj is string typeStr)
        {
            BookingType = Uri.UnescapeDataString(typeStr);
        }

        if (query.TryGetValue("code", out var codeObj) && codeObj is string codeStr)
        {
            ConfirmationCode = Uri.UnescapeDataString(codeStr);
        }
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Navigation.GoBackAsync();
    }

    [RelayCommand]
    public async Task ContactSupportAsync()
    {
        await Navigation.NavigateToAsync("More");
    }
}
