using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _resetToken = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isTokenSent = false;

    [ObservableProperty]
    private string? _infoMessage;

    public string PageHeading => CurrentLanguage switch
    {
        "en" => "Reset Password",
        "es" => "Restablecer Contraseña",
        _ => "איפוס סיסמה"
    };

    public string PageSubheading => CurrentLanguage switch
    {
        "en" => "Tomer Group Traveler Recovery",
        "es" => "Recuperación de Acceso Tomer Group",
        _ => "שחזור גישה למטיילי Tomer Group"
    };

    public string EmailStepInstruction => CurrentLanguage switch
    {
        "en" => "Enter your registered email address:",
        "es" => "Ingrese su correo electrónico registrado:",
        _ => "הזן את כתובת הדואר האלקטרוני שלך:"
    };

    public string EmailPlaceholder => "user@example.com";

    public string SendResetCodeButtonText => CurrentLanguage switch
    {
        "en" => "Send Reset Code",
        "es" => "Enviar Código de Restablecimiento",
        _ => "שלח קוד איפוס"
    };

    public string ResetTokenLabel => CurrentLanguage switch
    {
        "en" => "Reset Token / Code:",
        "es" => "Código de Restablecimiento:",
        _ => "קוד איפוס:"
    };

    public string ResetTokenPlaceholder => CurrentLanguage switch
    {
        "en" => "Enter reset token",
        "es" => "Ingrese código recibido",
        _ => "הזן קוד איפוס"
    };

    public string NewPasswordLabel => CurrentLanguage switch
    {
        "en" => "New Password:",
        "es" => "Nueva Contraseña:",
        _ => "סיסמה חדשה:"
    };

    public string NewPasswordPlaceholder => CurrentLanguage switch
    {
        "en" => "New password (min. 6 characters)",
        "es" => "Nueva contraseña (mínimo 6 caracteres)",
        _ => "סיסמה חדשה (לפחות 6 תווים)"
    };

    public string ConfirmPasswordLabel => CurrentLanguage switch
    {
        "en" => "Confirm New Password:",
        "es" => "Confirmar Nueva Contraseña:",
        _ => "אימות סיסמה חדשה:"
    };

    public string ConfirmPasswordPlaceholder => CurrentLanguage switch
    {
        "en" => "Confirm password",
        "es" => "Confirmar contraseña",
        _ => "אימות סיסמה"
    };

    public string SubmitNewPasswordButtonText => CurrentLanguage switch
    {
        "en" => "Update Password & Log In",
        "es" => "Actualizar Contraseña e Iniciar Sesión",
        _ => "החלף סיסמה והתחבר"
    };

    public string BackToLoginButtonText => CurrentLanguage switch
    {
        "en" => "← Back to Login",
        "es" => "← Volver al Inicio de Sesión",
        _ => "← חזרה להתחברות"
    };

    public ForgotPasswordViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        Title = PageHeading;
    }

    protected override void OnLanguageChanged()
    {
        Title = PageHeading;
        OnPropertyChanged(nameof(PageHeading));
        OnPropertyChanged(nameof(PageSubheading));
        OnPropertyChanged(nameof(EmailStepInstruction));
        OnPropertyChanged(nameof(EmailPlaceholder));
        OnPropertyChanged(nameof(SendResetCodeButtonText));
        OnPropertyChanged(nameof(ResetTokenLabel));
        OnPropertyChanged(nameof(ResetTokenPlaceholder));
        OnPropertyChanged(nameof(NewPasswordLabel));
        OnPropertyChanged(nameof(NewPasswordPlaceholder));
        OnPropertyChanged(nameof(ConfirmPasswordLabel));
        OnPropertyChanged(nameof(ConfirmPasswordPlaceholder));
        OnPropertyChanged(nameof(SubmitNewPasswordButtonText));
        OnPropertyChanged(nameof(BackToLoginButtonText));
    }

    [RelayCommand]
    public async Task RequestResetTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = CurrentLanguage switch
            {
                "en" => "Please enter your registered email address",
                "es" => "Por favor ingrese su correo electrónico registrado",
                _ => "נא להזין כתובת דוא\"ל רשומה"
            };
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        InfoMessage = null;

        try
        {
            var result = await _apiClient.ForgotPasswordAsync(Email);
            if (result.Success)
            {
                IsTokenSent = true;
                InfoMessage = result.Message ?? (CurrentLanguage switch
                {
                    "en" => "Reset instructions and token have been issued.",
                    "es" => "Las instrucciones y el código han sido enviados.",
                    _ => "הוראות וקוד איפוס נשלחו בהצלחה."
                });

                if (!string.IsNullOrEmpty(result.Data))
                {
                    ResetToken = result.Data; // Pre-fill token in test/dev
                }
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
    public async Task SubmitNewPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(ResetToken) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = CurrentLanguage switch
            {
                "en" => "Please enter the reset token and new password",
                "es" => "Por favor ingrese el código y la nueva contraseña",
                _ => "נא להזין קוד איפוס וסיסמה חדשה"
            };
            return;
        }

        if (NewPassword != ConfirmPassword)
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
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.ResetPasswordAsync(Email, ResetToken, NewPassword);
            if (result.Success)
            {
                InfoMessage = CurrentLanguage switch
                {
                    "en" => "Password reset successfully! Please log in.",
                    "es" => "¡Contraseña actualizada con éxito! Por favor inicie sesión.",
                    _ => "הסיסמה שונתה בהצלחה! מתחבר כעת..."
                };
                await Task.Delay(1000);
                await Navigation.NavigateToLoginAsync();
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
    public async Task BackToLoginAsync()
    {
        await Navigation.NavigateToLoginAsync();
    }
}
