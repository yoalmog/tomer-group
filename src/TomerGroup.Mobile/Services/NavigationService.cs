#if USE_MAUI
using Microsoft.Maui.Controls;
#endif

namespace TomerGroup.Mobile.Services;

public interface INavigationService
{
    string CurrentShell { get; }
    Task NavigateToSplashAsync();
    Task NavigateToLoginAsync();
    Task NavigateToForgotPasswordAsync();
    Task NavigateToCustomerShellAsync();
    Task NavigateToAgencyShellAsync();
    Task NavigateToAsync(string route);
    Task GoBackAsync();
}

public class NavigationService : INavigationService
{
    public string CurrentShell { get; private set; } = "Splash";
    public event Action<string>? ShellChanged;

    public Task NavigateToSplashAsync()
    {
        CurrentShell = "Splash";
        ShellChanged?.Invoke(CurrentShell);
        return Task.CompletedTask;
    }

    public Task NavigateToLoginAsync()
    {
        CurrentShell = "Login";
        ShellChanged?.Invoke(CurrentShell);
        return Task.CompletedTask;
    }

    public Task NavigateToForgotPasswordAsync()
    {
        CurrentShell = "ForgotPassword";
        ShellChanged?.Invoke(CurrentShell);
        return Task.CompletedTask;
    }

    public Task NavigateToCustomerShellAsync()
    {
        CurrentShell = "CustomerShell";
        ShellChanged?.Invoke(CurrentShell);
        return Task.CompletedTask;
    }

    public Task NavigateToAgencyShellAsync()
    {
        CurrentShell = "AgencyShell";
        ShellChanged?.Invoke(CurrentShell);
        return Task.CompletedTask;
    }

    public async Task NavigateToAsync(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return;
        }

#if USE_MAUI
        try
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(route);
                return;
            }
        }
        catch
        {
            // Safe fallback for shell route navigation if the shell is not yet active.
        }

        CurrentShell = "CustomerShell";
        ShellChanged?.Invoke(CurrentShell);

        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync(route);
        }
#else
        CurrentShell = "CustomerShell";
        ShellChanged?.Invoke(CurrentShell);
#endif
    }

    public async Task GoBackAsync()
    {
#if USE_MAUI
        try
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }
        }
        catch
        {
            // Ignore unsupported back navigation while shell is not active.
        }
#endif
    }
}

