using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class MoreViewModel : BaseViewModel
{
    private readonly ISecureStorageService _secureStorage;
    private readonly IOfflineSyncManager _syncManager;

    [ObservableProperty]
    private string _agencyPhone = "+51 84 231961";

    [ObservableProperty]
    private string _agencyWhatsApp = "+51 984 231961";

    [ObservableProperty]
    private string _emergencyPhone = "+51 984 231961";

    [ObservableProperty]
    private string _chabadAddress = "Calle San Agustín 415, Cusco (2 min from Plaza de Armas)";

    [ObservableProperty]
    private string _chabadPhone = "+51 84 236842";

    [ObservableProperty]
    private string _syncStatusMessage = string.Empty;

    public MoreViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        ISecureStorageService secureStorage,
        IOfflineSyncManager syncManager)
        : base(localization, navigation)
    {
        _secureStorage = secureStorage;
        _syncManager = syncManager;
        Title = Localize(LocalizationKeys.NavMore);
    }

    protected override void OnLanguageChanged()
    {
        Title = Localize(LocalizationKeys.NavMore);
    }

    public string SelectedLanguage => CurrentLanguage;

    // Localized dynamic UI labels for MorePage
    public string OfficeLocationLabel => CurrentLanguage switch
    {
        "en" => "Cusco Office: Portal de Panes 123, Plaza de Armas",
        "es" => "Oficina Cusco: Portal de Panes 123, Plaza de Armas",
        _ => "משרד קוסקו: Portal de Panes 123, Plaza de Armas"
    };

    public string WhatsAppButtonText => CurrentLanguage switch
    {
        "en" => "Open WhatsApp with Agency 💬",
        "es" => "Abrir WhatsApp con la Agencia 💬",
        _ => "פתח שיחת WhatsApp עם הסוכנות 💬"
    };

    public string EmergencyLinePrefix => CurrentLanguage switch
    {
        "en" => "🚨 24/7 Emergency Line: ",
        "es" => "🚨 Línea de Emergencia 24/7: ",
        _ => "🚨 קו חירום 24/7: "
    };

    public string ChabadSectionTitle => CurrentLanguage switch
    {
        "en" => "✡️ Chabad House Cusco",
        "es" => "✡️ Casa Chabad Cusco",
        _ => "✡️ בית חב״ד קוסקו (Chabad House Cusco)"
    };

    public string ChabadDescription => CurrentLanguage switch
    {
        "en" => "Shabbat meals, daily prayers, Torah classes, and Glatt Kosher restaurant.",
        "es" => "Cenas de Shabat, rezos diarios, clases y restaurante Glatt Kosher.",
        _ => "סעודות שבת, תפילות, שיעורים ומסעדה כשרה למהדרין."
    };

    public string OfflineSyncTitle => CurrentLanguage switch
    {
        "en" => "🔄 Offline Data Sync",
        "es" => "🔄 Sincronización Fuera de Línea",
        _ => "🔄 סנכרון נתונים לאופליין"
    };

    public string OfflineSyncDescription => CurrentLanguage switch
    {
        "en" => "Sync all vouchers, train tickets, and guide details before departing to remote Andean areas without cell reception.",
        "es" => "Sincronice todos los vouchers, boletos de tren y datos de guías antes de salir a zonas sin cobertura en los Andes.",
        _ => "סנכרן את כל השוברים, כרטיסי הרכבת ופרטי המדריכים לפני יציאה לאזורים ללא קליטה בהרי האנדים."
    };

    public string OfflineSyncButtonText => CurrentLanguage switch
    {
        "en" => "Sync Data Now",
        "es" => "Sincronizar Datos Ahora",
        _ => "סנכרן נתונים עכשיו"
    };

    public string LanguageSectionTitle => CurrentLanguage switch
    {
        "en" => "🌐 App Language",
        "es" => "🌐 Idioma de la Aplicación",
        _ => "🌐 שפת הממשק (Language)"
    };

    public string SignOutButtonText => CurrentLanguage switch
    {
        "en" => "Sign Out 🚪",
        "es" => "Cerrar Sesión 🚪",
        _ => "התנתק מהחשבון 🚪"
    };

    [RelayCommand]
    public void ChangeLanguage(string lang)
    {
        Localization.SetLanguage(lang);
    }

    [RelayCommand]
    public async Task OpenAgencyWhatsAppAsync()
    {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
        try
        {
            var uri = new Uri("https://wa.me/51984231961");
            await Microsoft.Maui.ApplicationModel.Launcher.Default.OpenAsync(uri);
        }
        catch
        {
            // Ignore failure if device has no browser or WhatsApp handler
        }
#else
        await Task.CompletedTask;
#endif
    }

    [RelayCommand]
    public async Task TriggerOfflineSyncAsync()
    {
        IsBusy = true;
        try
        {
            var success = await _syncManager.SynchronizeAsync(Guid.Empty, true);
            SyncStatusMessage = success 
                ? (CurrentLanguage switch
                   {
                       "en" => "✓ All vouchers, tickets and itinerary are fully synced for offline use",
                       "es" => "✓ Todos los vouchers, boletos e itinerario están sincronizados fuera de línea",
                       _ => "✓ כל המסמכים, אישורי הכניסה ומסלול הטיול עודכנו לאופליין בהצלחה"
                   })
                : (CurrentLanguage switch
                   {
                       "en" => "Note: Data saved locally and will synchronize when connected",
                       "es" => "Nota: Datos guardados localmente; se sincronizarán al conectarse",
                       _ => "שים לב: הנתונים נשמרו מקומית וייסונכרנו בעת חיבור לרשת"
                   });
        }
        catch (Exception ex)
        {
            SyncStatusMessage = $"Sync error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
        await _secureStorage.RemoveAsync("auth_token");
        await _secureStorage.RemoveAsync("refresh_token");
        await Navigation.NavigateToCustomerShellAsync();
    }
}
