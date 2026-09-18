using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService? _secureStorage;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPhoneLoginMode = false;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _verificationCode = string.Empty;

    [ObservableProperty]
    private bool _isCodeSent = false;

    [ObservableProperty]
    private string? _statusInfo;

    [ObservableProperty]
    private string _selectedLanguage = "he";

    public List<string> AvailableLanguages => new() { "he", "en", "es" };

    public string SignInButtonText => CurrentLanguage switch
    {
        "en" => "Sign In",
        "es" => "Iniciar Sesión",
        _ => "התחבר עכשיו / Sign In"
    };

    public string AdminQuickFillText => CurrentLanguage switch
    {
        "en" => "Admin Login (tomergroupe@gmail.com)",
        "es" => "Acceso Admin (tomergroupe@gmail.com)",
        _ => "התחברות מנהל (tomergroupe@gmail.com)"
    };

    public string BackToExploreText => CurrentLanguage switch
    {
        "en" => "✕ Back to Explore",
        "es" => "✕ Volver a Explorar",
        _ => "✕ חזרה לגלות"
    };

    public string EmailTabTitle => CurrentLanguage switch
    {
        "en" => "Email & Password",
        "es" => "Correo y Contraseña",
        _ => "אימייל וסיסמה"
    };

    public string PhoneTabTitle => CurrentLanguage switch
    {
        "en" => "SMS Code",
        "es" => "Código por SMS",
        _ => "קוד ב-SMS"
    };

    public string PersonalAreaHeading => CurrentLanguage switch
    {
        "en" => "Sign in to Your Account",
        "es" => "Acceso a su Cuenta",
        _ => "כניסה לאזור האישי"
    };

    public string EmailFieldLabel => CurrentLanguage switch
    {
        "en" => "Email Address",
        "es" => "Correo Electrónico",
        _ => "כתובת אימייל"
    };

    public string PasswordFieldLabel => CurrentLanguage switch
    {
        "en" => "Password",
        "es" => "Contraseña",
        _ => "סיסמה"
    };

    public string ForgotPasswordButtonText => CurrentLanguage switch
    {
        "en" => "Forgot Password?",
        "es" => "¿Olvidó su Contraseña?",
        _ => "שכחת סיסמה?"
    };

    public string PhoneLoginHeading => CurrentLanguage switch
    {
        "en" => "Sign in with SMS",
        "es" => "Acceso con SMS",
        _ => "כניסה באמצעות SMS"
    };

    public string PhoneFieldLabel => CurrentLanguage switch
    {
        "en" => "Mobile Phone Number",
        "es" => "Número de Teléfono Móvil",
        _ => "מספר טלפון נייד"
    };

    public string SendCodeButtonText => CurrentLanguage switch
    {
        "en" => "Send Verification Code 📲",
        "es" => "Enviar Código 📲",
        _ => "שלח קוד אימות 📲"
    };

    public string CodeFieldLabel => CurrentLanguage switch
    {
        "en" => "6-digit Verification Code",
        "es" => "Código de Verificación de 6 dígitos",
        _ => "קוד אימות בן 6 ספרות"
    };

    public string VerifyCodeButtonText => CurrentLanguage switch
    {
        "en" => "Verify Code & Sign In ✓",
        "es" => "Verificar e Iniciar Sesión ✓",
        _ => "אמת קוד והתחבר ✓"
    };

    public string ContinueAsGuestText => CurrentLanguage switch
    {
        "en" => "Continue Exploring as Guest ←",
        "es" => "Continuar como Invitado ←",
        _ => "המשך לגלות יעדים כאורח ←"
    };

    public string SupportFooterText => CurrentLanguage switch
    {
        "en" => "Tomer Group team is available 24/7 for support",
        "es" => "El equipo de Tomer Group está disponible 24/7",
        _ => "צוות Tomer Group זמין 24/7 לכל סיוע"
    };

    public LoginViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IApiClient apiClient,
        ISecureStorageService? secureStorage = null)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;
        Title = Localize(LocalizationKeys.Login);
    }

    protected override void OnLanguageChanged()
    {
        Title = Localize(LocalizationKeys.Login);
        OnPropertyChanged(nameof(SignInButtonText));
        OnPropertyChanged(nameof(AdminQuickFillText));
        OnPropertyChanged(nameof(BackToExploreText));
        OnPropertyChanged(nameof(EmailTabTitle));
        OnPropertyChanged(nameof(PhoneTabTitle));
        OnPropertyChanged(nameof(PersonalAreaHeading));
        OnPropertyChanged(nameof(EmailFieldLabel));
        OnPropertyChanged(nameof(PasswordFieldLabel));
        OnPropertyChanged(nameof(ForgotPasswordButtonText));
        OnPropertyChanged(nameof(PhoneLoginHeading));
        OnPropertyChanged(nameof(PhoneFieldLabel));
        OnPropertyChanged(nameof(SendCodeButtonText));
        OnPropertyChanged(nameof(CodeFieldLabel));
        OnPropertyChanged(nameof(VerifyCodeButtonText));
        OnPropertyChanged(nameof(ContinueAsGuestText));
        OnPropertyChanged(nameof(SupportFooterText));
    }

    [RelayCommand]
    public void FillAdminCredentials()
    {
        IsPhoneLoginMode = false;
        Email = "tomergroupe@gmail.com";
        Password = "123456";
        ErrorMessage = null;
        StatusInfo = CurrentLanguage switch
        {
            "en" => "Admin credentials populated. Tap Sign In to enter.",
            "es" => "Credenciales de administrador cargadas. Pulse Iniciar Sesión.",
            _ => "פרטי מנהל הוזנו. לחץ 'התחבר עכשיו' לכניסה למערכת."
        };
    }

    [RelayCommand]
    public void ToggleLoginMethod(string method)
    {
        IsPhoneLoginMode = method.Equals("Phone", StringComparison.OrdinalIgnoreCase);
        ErrorMessage = null;
        StatusInfo = null;
    }

    [RelayCommand]
    public void ChangeLanguage(string lang)
    {
        SelectedLanguage = lang;
        Localization.SetLanguage(lang);
        RefreshDirection();
        OnLanguageChanged();
    }

    [RelayCommand]
    public async Task SendPhoneCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            ErrorMessage = "נא להזין מספר טלפון תקין";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.SendPhoneCodeAsync(PhoneNumber);
            if (result.Success)
            {
                IsCodeSent = true;
                StatusInfo = "קוד האימות נשלח בהודעת SMS.";
            }
            else
            {
                ErrorMessage = result.Message;
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
    public async Task VerifyPhoneCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(VerificationCode))
        {
            ErrorMessage = "נא להזין את קוד האימות";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.VerifyPhoneCodeAsync(PhoneNumber, VerificationCode);
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
                await Navigation.NavigateToCustomerShellAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "קוד שגוי או שפג תוקפו";
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
    public async Task OpenForgotPasswordAsync()
    {
        await Navigation.NavigateToForgotPasswordAsync();
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        // Smoothly returns to exploring without trapping user in login loop
        await Navigation.NavigateToCustomerShellAsync();
    }

    [RelayCommand]
    public async Task ContinueAsGuestAsync()
    {
        // Smoothly returns to public travel experience
        await Navigation.NavigateToCustomerShellAsync();
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "נא להזין כתובת אימייל וסיסמה";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.LoginAsync(Email, Password);
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

                var role = result.Data.User.Role;
                if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    await Navigation.NavigateToCustomerShellAsync();
                }
                else
                {
                    await Navigation.NavigateToAgencyShellAsync();
                }
            }
            else
            {
                ErrorMessage = result.Message ?? "פרטי התחברות שגויים";
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
}
