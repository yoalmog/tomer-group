using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService _secureStorage;
    private Guid _customerId = Guid.Empty;

    public ProfileViewModel(
        IApiClient apiClient,
        ISecureStorageService secureStorage,
        ILocalizationService localizationService,
        INavigationService navigationService)
        : base(localizationService, navigationService)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;
        Title = Localize(LocalizationKeys.NavProfile);
        DietaryOptions = new List<string>
        {
            "כשר למהדרין (Kosher Mehudar)",
            "כשר רגיל (Kosher Standard)",
            "צמחוני (Vegetarian)",
            "טבעוני (Vegan)",
            "ללא גלוטן (Gluten-Free)",
            "ללא הגבלות (No Restrictions)"
        };
    }

    public List<string> DietaryOptions { get; }

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _hebrewName = string.Empty;

    [ObservableProperty]
    private string _passportName = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _whatsApp = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _country = "Israel";

    [ObservableProperty]
    private string _maskedPassportNumber = string.Empty;

    [ObservableProperty]
    private string _unmaskedPassportNumber = string.Empty;

    [ObservableProperty]
    private bool _isPassportRevealed = false;

    [ObservableProperty]
    private DateTime? _passportExpiration;

    [ObservableProperty]
    private bool _isPassportExpiringSoon = false;

    [ObservableProperty]
    private DateTime? _dateOfBirth;

    [ObservableProperty]
    private string _israelIdNumber = string.Empty;

    [ObservableProperty]
    private string _emergencyContactName = string.Empty;

    [ObservableProperty]
    private string _emergencyContactPhone = string.Empty;

    [ObservableProperty]
    private string _specialRequests = string.Empty;

    [ObservableProperty]
    private string _dietaryPreferences = string.Empty;

    [ObservableProperty]
    private string _selectedDietaryOption = string.Empty;

    [ObservableProperty]
    private string _medicalNotes = string.Empty;

    [ObservableProperty]
    private string _insuranceCompany = string.Empty;

    [ObservableProperty]
    private string _insurancePolicyNumber = string.Empty;

    [ObservableProperty]
    private bool _isActiveInPeru = true;

    [ObservableProperty]
    private bool _isEditing = false;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccessMessage = false;

    public string DisplayPassportNumber => IsPassportRevealed && !string.IsNullOrEmpty(UnmaskedPassportNumber)
        ? UnmaskedPassportNumber
        : MaskedPassportNumber;

    public string Initials
    {
        get
        {
            var first = !string.IsNullOrEmpty(FirstName) ? FirstName[0] : 'T';
            var last = !string.IsNullOrEmpty(LastName) ? LastName[0] : 'G';
            return $"{first}{last}".ToUpper();
        }
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsAuthenticated = _apiClient.IsAuthenticated;
        if (!IsAuthenticated)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _apiClient.GetMyProfileAsync();
            if (response.Success && response.Data != null)
            {
                var c = response.Data;
                _customerId = c.Id;
                FirstName = c.FirstName;
                LastName = c.LastName;
                HebrewName = c.HebrewName;
                PassportName = c.PassportName;
                Phone = c.Phone;
                WhatsApp = c.WhatsApp;
                Email = c.Email;
                Country = c.Country;
                MaskedPassportNumber = c.MaskedPassportNumber;
                PassportExpiration = c.PassportExpiration;
                IsPassportExpiringSoon = c.IsPassportExpiringSoon;
                DateOfBirth = c.DateOfBirth;
                MedicalNotes = c.MedicalNotes ?? string.Empty;
                InsuranceCompany = c.InsuranceCompany ?? string.Empty;
                InsurancePolicyNumber = c.InsurancePolicyNumber ?? string.Empty;
                IsActiveInPeru = c.IsActiveInPeru;
                EmergencyContactName = c.EmergencyContactName ?? string.Empty;
                EmergencyContactPhone = c.EmergencyContactPhone ?? string.Empty;
                SpecialRequests = c.SpecialRequests ?? string.Empty;
                DietaryPreferences = c.DietaryPreferences ?? string.Empty;
                SelectedDietaryOption = DietaryPreferences;
                OnPropertyChanged(nameof(Initials));
                OnPropertyChanged(nameof(DisplayPassportNumber));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"שגיאה בטעינת הפרופיל: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ToggleEditMode()
    {
        IsEditing = !IsEditing;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    public async Task ToggleRevealPassportAsync()
    {
        if (IsPassportRevealed)
        {
            IsPassportRevealed = false;
            OnPropertyChanged(nameof(DisplayPassportNumber));
            return;
        }

        IsBusy = true;
        try
        {
            if (_customerId != Guid.Empty)
            {
                var response = await _apiClient.GetSensitiveDetailsAsync(_customerId);
                if (response.Success && response.Data != null)
                {
                    UnmaskedPassportNumber = response.Data.PassportNumber;
                    IsraelIdNumber = response.Data.IsraelIdNumber;
                    IsPassportRevealed = true;
                    OnPropertyChanged(nameof(DisplayPassportNumber));
                    return;
                }
            }
        }
        catch
        {
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveProfileAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            DietaryPreferences = SelectedDietaryOption;

            var dto = new UpdateCustomerProfileDto
            {
                FirstName = FirstName,
                LastName = LastName,
                HebrewName = HebrewName,
                PassportName = PassportName,
                Phone = Phone,
                WhatsApp = WhatsApp,
                Email = Email,
                Country = Country,
                PassportNumber = IsPassportRevealed ? UnmaskedPassportNumber : null,
                PassportExpiration = PassportExpiration,
                DateOfBirth = DateOfBirth,
                IsraelIdNumber = IsraelIdNumber,
                EmergencyContactName = EmergencyContactName,
                EmergencyContactPhone = EmergencyContactPhone,
                SpecialRequests = SpecialRequests,
                DietaryPreferences = DietaryPreferences,
                MedicalNotes = MedicalNotes,
                InsuranceCompany = InsuranceCompany,
                InsurancePolicyNumber = InsurancePolicyNumber
            };

            var result = await _apiClient.UpdateMyProfileAsync(dto);
            if (result.Success)
            {
                IsSuccessMessage = true;
                StatusMessage = Localize(LocalizationKeys.ProfileUpdatedSuccess);
                IsEditing = false;
            }
            else
            {
                IsSuccessMessage = false;
                StatusMessage = result.Message ?? "Failed to save profile changes";
            }
        }
        catch (Exception ex)
        {
            IsSuccessMessage = false;
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }


    // Localized dynamic UI labels for ProfilePage
    public string GatewayTitle => CurrentLanguage switch
    {
        "en" => "Your Journey Starts Here",
        "es" => "Tu Viaje Comienza Aquí",
        _ => "המסע שלך מתחיל כאן"
    };

    public string GatewayDescription => CurrentLanguage switch
    {
        "en" => "Log in to your Tomer Group account to manage your itinerary, view vouchers, and update personal and Kosher preferences.",
        "es" => "Inicie sesión en su cuenta de Tomer Group para gestionar su itinerario, ver vouchers y actualizar preferencias personales y Kosher.",
        _ => "התחבר לחשבונך ב-Tomer Group כדי לנהל את מסלול הטיול, לצפות בשוברים, ולעדכן העדפות אישיות וכשרות."
    };

    public string BenefitItinerary => CurrentLanguage switch
    {
        "en" => "Personalized day-by-day itinerary",
        "es" => "Itinerario personalizado día por día",
        _ => "מסלול טיול אישי ומפורט לפי ימים"
    };

    public string BenefitVouchers => CurrentLanguage switch
    {
        "en" => "Vouchers, Machu Picchu permits & train tickets",
        "es" => "Vouchers, permisos a Machu Picchu y boletos de tren",
        _ => "שוברים, אישורי מאצ'ו פיצ'ו וכרטיסי רכבת"
    };

    public string BenefitWallet => CurrentLanguage switch
    {
        "en" => "Offline travel wallet accessible anytime",
        "es" => "Billetera de documentos disponible sin internet",
        _ => "ארנק מסמכים זמין ללא צורך באינטרנט"
    };

    public string BenefitPreferences => CurrentLanguage switch
    {
        "en" => "Kosher, Shabbat & Andean oxygen room preferences",
        "es" => "Preferencias de Kosher, Shabat y oxígeno en los Andes",
        _ => "העדפות כשרות, שבת וחדרי חמצן באנדים"
    };

    public string LoginButtonText => CurrentLanguage switch
    {
        "en" => "Log In to Your Account",
        "es" => "Iniciar Sesión",
        _ => "התחבר לחשבונך"
    };

    public string DietarySectionTitle => CurrentLanguage switch
    {
        "en" => "🍽️ Dietary & Kosher Preferences",
        "es" => "🍽️ Preferencias Culinarias y Kosher",
        _ => "🍽️ העדפות קולינריה וכשרות"
    };

    public string NoDietaryPreferencesText => CurrentLanguage switch
    {
        "en" => "No special dietary preferences set yet",
        "es" => "Sin preferencias especiales establecidas aún",
        _ => "לא הוגדרו עדיין העדפות כשרות מיוחדות"
    };

    public string DietaryPickerTitle => CurrentLanguage switch
    {
        "en" => "Select dietary & kosher preference",
        "es" => "Seleccionar preferencia culinaria y kosher",
        _ => "בחר העדפת כשרות ותזונה"
    };

    public string PassportSectionTitle => CurrentLanguage switch
    {
        "en" => "🛂 Passport Details for Permits",
        "es" => "🛂 Datos de Pasaporte para Permisos",
        _ => "🛂 פרטי דרכון לאישורי כניסה"
    };

    public string PassportRevealButtonText => CurrentLanguage switch
    {
        "en" => "👁️ Show",
        "es" => "👁️ Mostrar",
        _ => "👁️ הצג"
    };

    public string PassportNoticeText => CurrentLanguage switch
    {
        "en" => "Passport is used to issue Machu Picchu sanctuary permits and train tickets.",
        "es" => "El pasaporte se utiliza para emitir permisos de Machu Picchu y billetes de tren.",
        _ => "הדרכון משמש להנפקת אישורי שמורת מאצ'ו פיצ'ו וכרטיסי רכבת."
    };

    public string ContactNameLabel => CurrentLanguage switch
    {
        "en" => "Contact Name:",
        "es" => "Nombre de Contacto:",
        _ => "שם איש קשר:"
    };

    public string EmergencyPhoneLabel => CurrentLanguage switch
    {
        "en" => "Emergency Phone:",
        "es" => "Teléfono de Emergencia:",
        _ => "טלפון לשעת חירום:"
    };

    public string ContactNamePlaceholder => CurrentLanguage switch
    {
        "en" => "Full name of emergency contact",
        "es" => "Nombre completo del contacto de emergencia",
        _ => "שם מלא של איש הקשר"
    };

    public string EmergencyPhonePlaceholder => CurrentLanguage switch
    {
        "en" => "Emergency phone (including country code)",
        "es" => "Teléfono de emergencia (con código de país)",
        _ => "טלפון איש הקשר (כולל קידומת בינלאומית)"
    };

    public string SaveProfileButtonText => CurrentLanguage switch
    {
        "en" => "Save Profile Changes ✓",
        "es" => "Guardar Cambios del Perfil ✓",
        _ => "שמור שינויים בפרופיל ✓"
    };

    public string EmergencySectionTitle => CurrentLanguage switch
    {
        "en" => "🚨 Emergency Contact",
        "es" => "🚨 Contacto de Emergencia",
        _ => "🚨 איש קשר לשעת חירום"
    };

    public string LanguageSectionTitle => CurrentLanguage switch
    {
        "en" => "🌐 App Language",
        "es" => "🌐 Idioma de la Aplicación",
        _ => "🌐 שפת האפליקציה (Language)"
    };

    public string SupportTitle => CurrentLanguage switch
    {
        "en" => "Tomer Group Concierge at Your Service",
        "es" => "Equipo Tomer Group a su Servicio",
        _ => "צוות Tomer Group לשירותכם"
    };

    public string SupportSubtitle => CurrentLanguage switch
    {
        "en" => "Available on WhatsApp for questions, arrangements and custom requests",
        "es" => "Disponibles por WhatsApp para cualquier consulta o coordinación",
        _ => "זמינים בוואטסאפ לכל שאלה, תיאום או בקשה מיוחדת במסלול"
    };

    public string LogoutButtonText => CurrentLanguage switch
    {
        "en" => "Log Out",
        "es" => "Cerrar Sesión",
        _ => "התנתק מהחשבון"
    };

    public string QuickLinksTitle => CurrentLanguage switch
    {
        "en" => "🎒 My Travel Management",
        "es" => "🎒 Gestión de Mi Viaje",
        _ => "🎒 ניהול הטיול שלי"
    };

    public string MyTripsLinkText => CurrentLanguage switch
    {
        "en" => "My Trips & Itinerary",
        "es" => "Mis Viajes e Itinerario",
        _ => "הטיולים ומסלול הנסיעה שלי"
    };

    public string BookingsLinkText => CurrentLanguage switch
    {
        "en" => "Bookings & Reservations",
        "es" => "Mis Reservas y Servicios",
        _ => "ההזמנות והשרותים שלי"
    };

    public string DocumentsLinkText => CurrentLanguage switch
    {
        "en" => "Travel Documents & Permits",
        "es" => "Documentos y Permisos",
        _ => "מסמכי נסיעה ואישורי כניסה"
    };

    public string NotificationsLinkText => CurrentLanguage switch
    {
        "en" => "Travel Alerts & Updates",
        "es" => "Alertas y Notificaciones",
        _ => "התראות ועדכוני מסלול"
    };

    protected override void OnLanguageChanged()
    {
        Title = Localize(LocalizationKeys.NavProfile);
        OnPropertyChanged(nameof(GatewayTitle));
        OnPropertyChanged(nameof(GatewayDescription));
        OnPropertyChanged(nameof(BenefitItinerary));
        OnPropertyChanged(nameof(BenefitVouchers));
        OnPropertyChanged(nameof(BenefitWallet));
        OnPropertyChanged(nameof(BenefitPreferences));
        OnPropertyChanged(nameof(LoginButtonText));
        OnPropertyChanged(nameof(DietarySectionTitle));
        OnPropertyChanged(nameof(NoDietaryPreferencesText));
        OnPropertyChanged(nameof(DietaryPickerTitle));
        OnPropertyChanged(nameof(PassportSectionTitle));
        OnPropertyChanged(nameof(PassportRevealButtonText));
        OnPropertyChanged(nameof(PassportNoticeText));
        OnPropertyChanged(nameof(EmergencySectionTitle));
        OnPropertyChanged(nameof(ContactNameLabel));
        OnPropertyChanged(nameof(EmergencyPhoneLabel));
        OnPropertyChanged(nameof(ContactNamePlaceholder));
        OnPropertyChanged(nameof(EmergencyPhonePlaceholder));
        OnPropertyChanged(nameof(SaveProfileButtonText));
        OnPropertyChanged(nameof(LanguageSectionTitle));
        OnPropertyChanged(nameof(SupportTitle));
        OnPropertyChanged(nameof(SupportSubtitle));
        OnPropertyChanged(nameof(LogoutButtonText));
        OnPropertyChanged(nameof(QuickLinksTitle));
        OnPropertyChanged(nameof(MyTripsLinkText));
        OnPropertyChanged(nameof(BookingsLinkText));
        OnPropertyChanged(nameof(DocumentsLinkText));
        OnPropertyChanged(nameof(NotificationsLinkText));
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        _apiClient.SetAuthToken(null);
        await _secureStorage.RemoveAsync("auth_token");
        await _secureStorage.RemoveAsync("refresh_token");
        IsAuthenticated = false;
        // LOGOUT -> PUBLIC HOME (Never force user to login screen)
        await Navigation.NavigateToCustomerShellAsync();
    }

    [RelayCommand]
    public async Task OpenSignInAsync()
    {
        await Navigation.NavigateToLoginAsync();
    }

    [RelayCommand]
    public void ChangeLanguage(string lang)
    {
        Localization.SetLanguage(lang);
    }

    [RelayCommand]
    public async Task OpenMyTripsAsync()
    {
        await Navigation.NavigateToAsync("//MyTrip");
    }

    [RelayCommand]
    public async Task OpenBookingsAsync()
    {
        await Navigation.NavigateToAsync("//Bookings");
    }

    [RelayCommand]
    public async Task OpenDocumentsAsync()
    {
        await Navigation.NavigateToAsync("Documents");
    }

    [RelayCommand]
    public async Task OpenNotificationsAsync()
    {
        await Navigation.NavigateToAsync("Notifications");
    }

    [RelayCommand]
    public async Task OpenWhatsAppAsync()
    {
#if USE_MAUI
        try
        {
            var phone = Brand.ContactPhone.Replace(" ", "").Replace("-", "").Replace("+", "");
            var uri = new Uri($"https://wa.me/{phone}?text={Uri.EscapeDataString("Hello Tomer Group Support, I need assistance with my trip.")}");
            await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(uri);
        }
        catch
        {
            await Navigation.NavigateToAsync("More");
        }
#else
        await Task.CompletedTask;
#endif
    }
}
