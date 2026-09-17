using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
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

    public ForgotPasswordViewModel(ILocalizationService localization, INavigationService navigation, IApiClient apiClient)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        Title = "איפוס סיסמה / Reset Password";
    }

    [RelayCommand]
    public async Task RequestResetTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your registered email address";
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
                InfoMessage = result.Message ?? "Reset instructions and token have been issued.";
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
            ErrorMessage = "Please enter the reset token and new password";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.ResetPasswordAsync(Email, ResetToken, NewPassword);
            if (result.Success)
            {
                InfoMessage = "Password reset successfully! Please log in.";
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

