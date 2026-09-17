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
        Title = Localize(LocalizationKeys.Login);
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
        await Navigation.NavigateToAsync("ForgotPassword");
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
