using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isAgencyMode = false;

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

    public LoginViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
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
            ErrorMessage = "Please enter a valid phone number";
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
                StatusInfo = "Verification code sent! (Dev Test OTP: 123456)";
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
            ErrorMessage = "Please enter the 6-digit code";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.VerifyPhoneCodeAsync(PhoneNumber, VerificationCode);
            if (result.Success)
            {
                await Navigation.NavigateToCustomerShellAsync();
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
    public async Task OpenForgotPasswordAsync()
    {
        await Navigation.NavigateToAsync("ForgotPassword");
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your email and password";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.LoginAsync(Email, Password);
            if (result.Success && result.Data != null)
            {
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
                ErrorMessage = result.Message ?? "Invalid credentials";
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
