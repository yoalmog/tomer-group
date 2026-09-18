using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class PreTripChecklistViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _passportReady = true;

    [ObservableProperty]
    private bool _insuranceReady = true;

    [ObservableProperty]
    private bool _flightConfirmationReady = true;

    [ObservableProperty]
    private bool _hotelVouchersReady = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public string PageTitle => CurrentLanguage switch
    {
        "en" => "Pre-trip Essentials",
        "es" => "Preparativos Esenciales",
        _ => "הכנות חיוניות לפני יציאה"
    };

    public string EyebrowText => CurrentLanguage switch
    {
        "en" => "Before Departure",
        "es" => "Antes de Partir",
        _ => "לפני ההמראה"
    };

    public string HeroTitle => CurrentLanguage switch
    {
        "en" => "Your Travel Checklist",
        "es" => "Tu Lista de Viaje",
        _ => "צ'ק-ליסט הטיול שלך"
    };

    public string HeroSubtitle => CurrentLanguage switch
    {
        "en" => "Keep everything ready for a smooth arrival in Peru.",
        "es" => "Mantén todo listo para una llegada perfecta a Perú.",
        _ => "כל מה שצריך מסודר ומוכן לנחיתה חלקה בפרו."
    };

    public string SectionTitle => CurrentLanguage switch
    {
        "en" => "Checklist",
        "es" => "Lista de Verificación",
        _ => "רשימת בדיקה"
    };

    public string PassportTitle => CurrentLanguage switch
    {
        "en" => "Passport and Visa",
        "es" => "Pasaporte y Visas",
        _ => "דרכון ואשרות כניסה"
    };

    public string PassportSubtitle => CurrentLanguage switch
    {
        "en" => "Valid and ready for travel",
        "es" => "Válido y listo para viajar",
        _ => "בתוקף ומוכן לטיול"
    };

    public string InsuranceTitle => CurrentLanguage switch
    {
        "en" => "Travel & Rescue Insurance",
        "es" => "Seguro de Viaje y Rescate",
        _ => "ביטוח נסיעות וחילוץ"
    };

    public string InsuranceSubtitle => CurrentLanguage switch
    {
        "en" => "Policy uploaded and active",
        "es" => "Póliza subida y activa",
        _ => "פוליסה פעילה ומעודכנת באפליקציה"
    };

    public string FlightTitle => CurrentLanguage switch
    {
        "en" => "Flight Confirmation",
        "es" => "Confirmación de Vuelos",
        _ => "אישורי טיסות"
    };

    public string FlightSubtitle => CurrentLanguage switch
    {
        "en" => "Arrival and return flight details",
        "es" => "Detalles de vuelos de llegada y regreso",
        _ => "פרטי נחיתה והמראות בינלאומיות ופנימיות"
    };

    public string HotelTitle => CurrentLanguage switch
    {
        "en" => "Hotel & Service Vouchers",
        "es" => "Vouchers de Hoteles y Servicios",
        _ => "שוברי מלונות ושירותים"
    };

    public string HotelSubtitle => CurrentLanguage switch
    {
        "en" => "Ready for check-in",
        "es" => "Listos para el check-in",
        _ => "מוכנים לצ'ק-אין ולספקים"
    };

    public string SummaryButtonText => CurrentLanguage switch
    {
        "en" => "View Trip Summary →",
        "es" => "Ver Resumen del Viaje →",
        _ => "צפה בסיכום הטיול ←"
    };

    public PreTripChecklistViewModel(INavigationService navigationService)
        : this(new LocalizationService(), navigationService)
    {
    }

    public PreTripChecklistViewModel(ILocalizationService localizationService, INavigationService navigationService)
        : base(localizationService, navigationService)
    {
        Title = PageTitle;
        UpdateStatus();
    }

    partial void OnPassportReadyChanged(bool value) => UpdateStatus();
    partial void OnInsuranceReadyChanged(bool value) => UpdateStatus();
    partial void OnFlightConfirmationReadyChanged(bool value) => UpdateStatus();
    partial void OnHotelVouchersReadyChanged(bool value) => UpdateStatus();

    protected override void OnLanguageChanged()
    {
        Title = PageTitle;
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(EyebrowText));
        OnPropertyChanged(nameof(HeroTitle));
        OnPropertyChanged(nameof(HeroSubtitle));
        OnPropertyChanged(nameof(SectionTitle));
        OnPropertyChanged(nameof(PassportTitle));
        OnPropertyChanged(nameof(PassportSubtitle));
        OnPropertyChanged(nameof(InsuranceTitle));
        OnPropertyChanged(nameof(InsuranceSubtitle));
        OnPropertyChanged(nameof(FlightTitle));
        OnPropertyChanged(nameof(FlightSubtitle));
        OnPropertyChanged(nameof(HotelTitle));
        OnPropertyChanged(nameof(HotelSubtitle));
        OnPropertyChanged(nameof(SummaryButtonText));
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var completed = new[] { PassportReady, InsuranceReady, FlightConfirmationReady, HotelVouchersReady }.Count(x => x);
        StatusMessage = completed switch
        {
            4 => CurrentLanguage switch
            {
                "en" => "Everything is ready for departure.",
                "es" => "Todo está listo para la partida.",
                _ => "הכל מוכן לקראת היציאה לדרך."
            },
            3 => CurrentLanguage switch
            {
                "en" => "One item still needs attention.",
                "es" => "Falta un elemento por revisar.",
                _ => "פריט אחד עדיין דורש תשומת לב."
            },
            2 => CurrentLanguage switch
            {
                "en" => "Two items still need attention.",
                "es" => "Faltan dos elementos por revisar.",
                _ => "שני פריטים עדיין דורשים תשומת לב."
            },
            _ => CurrentLanguage switch
            {
                "en" => "Please complete the required checklist items.",
                "es" => "Por favor complete los elementos requeridos.",
                _ => "נא להשלים את הפריטים הנדרשים ברשימה."
            }
        };
    }

    [RelayCommand]
    public async Task OpenTripSummaryAsync()
    {
        await Navigation.NavigateToAsync("TripSummary");
    }
}
