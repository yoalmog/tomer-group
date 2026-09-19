using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;
using TomerGroup.Mobile.ViewModels;
using TomerGroup.Mobile.ViewModels.Customer;
using Xunit;

namespace TomerGroup.Tests;

public class ButtonNavigationSafetyTests
{
    private class TrackingNavigationService : INavigationService
    {
        public string CurrentShell { get; set; } = "Splash";
        public string? LastNavigatedRoute { get; private set; }
        public int GoBackCallCount { get; private set; }

        public Task NavigateToSplashAsync()
        {
            CurrentShell = "Splash";
            return Task.CompletedTask;
        }

        public Task NavigateToLoginAsync()
        {
            CurrentShell = "Login";
            return Task.CompletedTask;
        }

        public Task NavigateToRegisterAsync()
        {
            CurrentShell = "Register";
            return Task.CompletedTask;
        }

        public Task NavigateToForgotPasswordAsync()
        {
            CurrentShell = "ForgotPassword";
            return Task.CompletedTask;
        }

        public Task NavigateToCustomerShellAsync()
        {
            CurrentShell = "CustomerShell";
            return Task.CompletedTask;
        }

        public Task NavigateToAgencyShellAsync()
        {
            CurrentShell = "AgencyShell";
            return Task.CompletedTask;
        }

        public Task NavigateToAsync(string route)
        {
            LastNavigatedRoute = route;
            return Task.CompletedTask;
        }

        public Task GoBackAsync()
        {
            GoBackCallCount++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task NavigationService_NormalizesRoutesAndSpecialDestinations()
    {
        var nav = new NavigationService();

        // 1. Navigate to Login via string route
        await nav.NavigateToAsync("Login");
        Assert.Equal("Login", nav.CurrentShell);

        // 2. Navigate to ForgotPassword via string route
        await nav.NavigateToAsync("ForgotPassword");
        Assert.Equal("ForgotPassword", nav.CurrentShell);

        // 3. Navigate to Splash via string route
        await nav.NavigateToAsync("Splash");
        Assert.Equal("Splash", nav.CurrentShell);

        // 4. Navigate to arbitrary route does not throw unhandled exceptions
        await nav.NavigateToAsync("//More");
        Assert.Equal("CustomerShell", nav.CurrentShell);

        await nav.NavigateToAsync("UnknownNonExistentPage");
        // Must complete safely without unhandled exceptions
        await nav.GoBackAsync();
    }

    [Fact]
    public async Task LoginViewModel_OpenForgotPassword_SwitchesToForgotPassword()
    {
        var nav = new NavigationService();
        var mockApiClient = new Mock<IApiClient>();
        var mockSecureStorage = new Mock<ISecureStorageService>();
        var loc = new LocalizationService();

        var vm = new LoginViewModel(loc, nav, mockApiClient.Object, mockSecureStorage.Object);

        await vm.OpenForgotPasswordCommand.ExecuteAsync(null);

        Assert.Equal("ForgotPassword", nav.CurrentShell);
    }

    [Fact]
    public async Task CustomerHomeViewModel_OpenSupport_NavigatesToMore()
    {
        var nav = new TrackingNavigationService();
        var mockApiClient = new Mock<IApiClient>();
        var loc = new LocalizationService();
        var mockImg = new Mock<IDestinationImageService>();

        var vm = new CustomerHomeViewModel(loc, nav, mockApiClient.Object, mockImg.Object);

        await vm.OpenSupportCommand.ExecuteAsync(null);

        Assert.Equal("More", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task MyTripViewModel_OpenContact_NavigatesToMore()
    {
        var nav = new TrackingNavigationService();
        var mockApiClient = new Mock<IApiClient>();
        var loc = new LocalizationService();
        var mockImg = new Mock<IDestinationImageService>();

        var vm = new MyTripViewModel(loc, nav, mockApiClient.Object, mockImg.Object);

        await vm.OpenContactCommand.ExecuteAsync(null);

        Assert.Equal("More", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task BookingsViewModel_ContactAgency_NavigatesToMore()
    {
        var nav = new TrackingNavigationService();
        var mockApiClient = new Mock<IApiClient>();
        var mockImg = new Mock<IDestinationImageService>();

        var vm = new BookingsViewModel(mockApiClient.Object, nav, mockImg.Object);

        await vm.ContactAgencyCommand.ExecuteAsync(null);

        Assert.Equal("More", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task DocumentsViewModel_ContactAgency_NavigatesToMore()
    {
        var nav = new TrackingNavigationService();
        var mockApiClient = new Mock<IApiClient>();

        var vm = new DocumentsViewModel(mockApiClient.Object, nav);

        await vm.ContactAgencyCommand.ExecuteAsync(null);

        Assert.Equal("More", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task ExploreViewModel_ContactTeam_NavigatesToMore()
    {
        var nav = new TrackingNavigationService();
        var loc = new LocalizationService();
        var mockImg = new Mock<IDestinationImageService>();

        var vm = new ExploreViewModel(loc, nav, mockImg.Object);

        await vm.ContactTeamCommand.ExecuteAsync(null);

        Assert.Equal("More", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task ActivityDetailViewModel_ContactSupport_NavigatesToMore()
    {
        var nav = new TrackingNavigationService();
        var loc = new LocalizationService();
        var mockImg = new Mock<IDestinationImageService>();

        var vm = new ActivityDetailViewModel(loc, nav, mockImg.Object);

        await vm.ContactSupportCommand.ExecuteAsync(null);

        Assert.Equal("More", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task BookingDetailViewModel_ContactSupport_NavigatesToMore()
    {
        var nav = new TrackingNavigationService();
        var loc = new LocalizationService();
        var mockImg = new Mock<IDestinationImageService>();

        var vm = new BookingDetailViewModel(loc, nav, mockImg.Object);

        await vm.ContactSupportCommand.ExecuteAsync(null);

        Assert.Equal("More", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task AddOnsViewModel_ContinueToPayment_NavigatesToPayment()
    {
        var nav = new TrackingNavigationService();
        var vm = new AddOnsViewModel(nav);

        await vm.ContinueToPaymentCommand.ExecuteAsync(null);

        Assert.Equal("Payment", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task PaymentViewModel_PayNow_NavigatesToPreTripChecklist()
    {
        var nav = new TrackingNavigationService();
        var vm = new PaymentViewModel(nav);

        await vm.PayNowCommand.ExecuteAsync(null);

        Assert.Equal("PreTripChecklist", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task PreTripChecklistViewModel_OpenTripSummary_NavigatesToTripSummary()
    {
        var nav = new TrackingNavigationService();
        var vm = new PreTripChecklistViewModel(nav);

        await vm.OpenTripSummaryCommand.ExecuteAsync(null);

        Assert.Equal("TripSummary", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task BookingConfirmationViewModel_ConfirmBooking_NavigatesToAddOns()
    {
        var nav = new TrackingNavigationService();
        var vm = new BookingConfirmationViewModel(nav);

        await vm.ConfirmBookingCommand.ExecuteAsync(null);

        Assert.Equal("AddOns", nav.LastNavigatedRoute);
    }

    [Fact]
    public async Task MoreViewModel_OpenAgencyWhatsApp_ExecutesWithoutError()
    {
        var nav = new TrackingNavigationService();
        var loc = new LocalizationService();
        var mockSec = new Mock<ISecureStorageService>();
        var mockSync = new Mock<IOfflineSyncManager>();

        var vm = new MoreViewModel(loc, nav, mockSec.Object, mockSync.Object);

        await vm.OpenAgencyWhatsAppCommand.ExecuteAsync(null);
        // Completed without exception
    }
}
