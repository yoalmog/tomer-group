using TomerGroup.Mobile.Services;
using Xunit;

namespace TomerGroup.Tests;

public class NavigationTests
{
    [Fact]
    public async Task NavigationService_ShouldSwitchShellsAndRaiseEvent()
    {
        // Arrange
        var navService = new NavigationService();
        string? notifiedShell = null;
        navService.ShellChanged += shell => notifiedShell = shell;

        // Act & Assert 1: Start on Splash
        Assert.Equal("Splash", navService.CurrentShell);

        // Act & Assert 2: Navigate to Login
        await navService.NavigateToLoginAsync();
        Assert.Equal("Login", navService.CurrentShell);
        Assert.Equal("Login", notifiedShell);

        // Act & Assert 3: Customer Login -> CustomerShell
        await navService.NavigateToCustomerShellAsync();
        Assert.Equal("CustomerShell", navService.CurrentShell);
        Assert.Equal("CustomerShell", notifiedShell);

        // Act & Assert 4: Agency Staff Login -> AgencyShell
        await navService.NavigateToAgencyShellAsync();
        Assert.Equal("AgencyShell", navService.CurrentShell);
        Assert.Equal("AgencyShell", notifiedShell);
    }
}

