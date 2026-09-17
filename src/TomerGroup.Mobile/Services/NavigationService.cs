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

    public Task NavigateToAsync(string route)
    {
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        return Task.CompletedTask;
    }
}

