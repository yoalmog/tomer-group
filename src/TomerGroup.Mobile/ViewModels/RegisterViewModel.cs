using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService? _secureStorage;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string? _statusInfo;

    public RegisterViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IApiClient apiClient,
        ISecureStorageService? secureStorage = null)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;
        Title = PageHeading;
    }

    public string PageHeading => CurrentLanguage switch
    {
        "en" => "Create Account",
        "es" => "Crear Cuenta",
        _ => "צור חשבון חדש"
    };

    public string PageSubtitle => CurrentLanguage switch
    {
        "en" => "Join Tomer Group to access your personalized itineraries, trek vouchers, and Andean travel documents.",
        "es" => "Únase a Tomer Group para acceder a sus itinerarios personalizados, vouchers de trek y documentos de viaje.",
        _ => "הצטרף ל-Tomer Group כדי לצפות במסלול האישי שלך, שוברי השירות, ואישורי הטרקים בהרי האנדים."
    };

    public string FirstNameLabel => CurrentLanguage switch
    {
        "en" => "First Name",
        "es" => "Nombre",
        _ => "שם פרטי"
    };

    public string LastNameLabel => CurrentLanguage switch
    {
        "en" => "Last Name",
        "es" => "Apellido",
        _ => "שם משפחה"
    };

    public string EmailLabel => CurrentLanguage switch
    {
        "en" => "Email Address",
        "es" => "Correo Electrónico",
        _ => "כתובת אימייל"
    };

    public string PhoneLabel => CurrentLanguage switch
    {
        "en" => "Phone / WhatsApp",
        "es" => "Teléfono / WhatsApp",
        _ => "טלפון / וואטסאפ"
    };

    public string PasswordLabel => CurrentLanguage switch
    {
        "en" => "Password (min. 6 characters)",
        "es" => "Contraseña (mín. 6 caracteres)",
        _ => "סיסמה (לפחות 6 תווים)"
    };

    public string ConfirmPasswordLabel => CurrentLanguage switch
    {
        "en" => "Confirm Password",
        "es" => "Confirmar Contraseña",
        _ => "אימות סיסמה"
    };

    public string CreateAccountButtonText => CurrentLanguage switch
    {
        "en" => "Create Account",
        "es" => "Crear Cuenta",
        _ => "צור חשבון והמשך"
    };

    public string AlreadyHaveAccountText => CurrentLanguage switch
    {
        "en" => "Already have an account? Sign In",
        "es" => "¿Ya tiene cuenta? Iniciar Sesión",
        _ => "כבר רשום? התחבר כאן"
    };

    public string BackToExploreText => CurrentLanguage switch
    {
        "en" => "✕ Back to Explore",
        "es" => "✕ Volver a Explorar",
        _ => "✕ חזרה לגלות"
    };

    public string SupportFooterText => CurrentLanguage switch
    {
        "en" => "Tomer Group • Cusco Operations Desk",
        "es" => "Tomer Group • Operaciones en Cusco",
        _ => "Tomer Group • מוקד שירות קוסקו"
    };

    protected override void OnLanguageChanged()
    {
        Title = PageHeading;
        OnPropertyChanged(nameof(PageHeading));
        OnPropertyChanged(nameof(PageSubtitle));
        OnPropertyChanged(nameof(FirstNameLabel));
        OnPropertyChanged(nameof(LastNameLabel));
        OnPropertyChanged(nameof(EmailLabel));
        OnPropertyChanged(nameof(PhoneLabel));
        OnPropertyChanged(nameof(PasswordLabel));
        OnPropertyChanged(nameof(ConfirmPasswordLabel));
        OnPropertyChanged(nameof(CreateAccountButtonText));
        OnPropertyChanged(nameof(AlreadyHaveAccountText));
        OnPropertyChanged(nameof(BackToExploreText));
        OnPropertyChanged(nameof(SupportFooterText));
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        ErrorMessage = null;
        StatusInfo = null;

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = CurrentLanguage switch
            {
                "en" => "Please enter your first and last name",
                "es" => "Por favor ingrese su nombre y apellido",
                _ => "נא להזין שם פרטי ושם משפחה"
            };
            return;
        }

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@') || !Email.Contains('.'))
        {
            ErrorMessage = CurrentLanguage switch
            {
                "en" => "Please enter a valid email address",
                "es" => "Por favor ingrese un correo válido",
                _ => "נא להזין כתובת אימייל תקינה"
            };
            return;
        }

        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
        {
            ErrorMessage = CurrentLanguage switch
            {
                "en" => "Password must be at least 6 characters long",
                "es" => "La contraseña debe tener al menos 6 caracteres",
                _ => "הסיסמה חייבת להכיל לפחות 6 תווים"
            };
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = CurrentLanguage switch
            {
                "en" => "Passwords do not match",
                "es" => "Las contraseñas no coinciden",
                _ => "הסיסמאות אינן תואמות"
            };
            return;
        }

        IsBusy = true;

        try
        {
            var request = new CustomerRegisterRequestDto
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Email = Email.Trim(),
                Phone = Phone.Trim(),
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                PreferredLanguage = CurrentLanguage
            };

            var result = await _apiClient.RegisterCustomerAsync(request);
            if (result.Success && result.Data != null)
            {
                _apiClient.SetAuthToken(result.Data.Token);
                if (_secureStorage != null)
                {
                    await _secureStorage.SetAsync("auth_token", result.Data.Token);
                    if (!string.IsNullOrEmpty(result.Data.RefreshToken))
                    {
                        await _secureStorage.SetAsync("refresh_token", result.Data.RefreshToken);
                    }
                }

                StatusInfo = CurrentLanguage switch
                {
                    "en" => "Account created successfully! Welcome to Tomer Group.",
                    "es" => "¡Cuenta creada exitosamente! Bienvenido a Tomer Group.",
                    _ => "החשבון נוצר בהצלחה! ברוכים הבאים ל-Tomer Group."
                };

                // Navigate directly to Customer shell
                await Navigation.NavigateToCustomerShellAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? (CurrentLanguage switch
                {
                    "en" => "Failed to create account. Please try again.",
                    "es" => "Error al crear la cuenta. Intente nuevamente.",
                    _ => "יצירת החשבון נכשלה. נא לנסות שנית."
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task BackToLoginAsync()
    {
        await Navigation.NavigateToLoginAsync();
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Navigation.NavigateToCustomerShellAsync();
    }

    [RelayCommand]
    public void ChangeLanguage(string lang)
    {
        Localization.SetLanguage(lang);
        RefreshDirection();
    }
}

