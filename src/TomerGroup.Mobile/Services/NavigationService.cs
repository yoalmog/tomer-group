#if USE_MAUI
using Microsoft.Maui.Controls;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
    private static readonly HashSet<string> TopLevelShellTabs = new(StringComparer.OrdinalIgnoreCase)
    {
        "Home",
        "Explore",
        "MyTrip",
        "Bookings",
        "Profile",
        "AgencyDashboard",
        "AgencyCustomers",
        "AgencyTrips",
        "AgencyTours",
        "AgencyHotels",
        "AgencyStaffDirectory",
        "AgencyManifest",
        "AgencyFinance",
        "AgencyAIAssistant",
        "AgencyReports",
        "AgencySettings"
    };

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

        var normalizedRoute = NormalizeRoute(route);

        if (normalizedRoute.Equals("Login", StringComparison.OrdinalIgnoreCase) ||
            normalizedRoute.Equals("//Login", StringComparison.OrdinalIgnoreCase))
        {
            await NavigateToLoginAsync();
            return;
        }

        if (normalizedRoute.Equals("ForgotPassword", StringComparison.OrdinalIgnoreCase) ||
            normalizedRoute.Equals("//ForgotPassword", StringComparison.OrdinalIgnoreCase))
        {
            await NavigateToForgotPasswordAsync();
            return;
        }

        if (normalizedRoute.Equals("Splash", StringComparison.OrdinalIgnoreCase) ||
            normalizedRoute.Equals("//Splash", StringComparison.OrdinalIgnoreCase))
        {
            await NavigateToSplashAsync();
            return;
        }

#if USE_MAUI
        try
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(normalizedRoute);
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Initial Shell navigation failed for '{normalizedRoute}': {ex.Message}");
        }

        CurrentShell = "CustomerShell";
        ShellChanged?.Invoke(CurrentShell);

        try
        {
            if (Shell.Current is null)
            {
                await Task.Delay(100);
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(normalizedRoute);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fallback Shell navigation failed for '{normalizedRoute}': {ex.Message}");
        }
#else
        CurrentShell = "CustomerShell";
        ShellChanged?.Invoke(CurrentShell);
        await Task.CompletedTask;
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
        catch (Exception ex)
        {
            Debug.WriteLine($"GoBackAsync navigation failed: {ex.Message}");
        }
#else
        await Task.CompletedTask;
#endif
    }

    private static string NormalizeRoute(string route)
    {
        var trimmed = route.Trim();

        if (trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            var withoutPrefix = trimmed.Substring(2);
            var pathPart = withoutPrefix;
            var queryIdx = pathPart.IndexOf('?');
            if (queryIdx >= 0)
            {
                pathPart = pathPart.Substring(0, queryIdx);
            }

            // If the route target is NOT a top-level shell tab, strip the "//" prefix
            if (!TopLevelShellTabs.Contains(pathPart))
            {
                return withoutPrefix;
            }
        }

        return trimmed;
    }
}

