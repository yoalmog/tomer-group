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

    protected override void OnLanguageChanged()
    {
        Title = Localize(LocalizationKeys.NavProfile);
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
}
