using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Core.Models;
using TomerGroup.Mobile.Services;
using TomerGroup.Mobile.ViewModels;
using TomerGroup.Mobile.ViewModels.Agency;
using TomerGroup.Mobile.ViewModels.Customer;
using Xunit;

namespace TomerGroup.Tests;

public class Phase16MobileNavigationTests
{
    private IServiceProvider BuildMobileServiceContainer()
    {
        var services = new ServiceCollection();

        // 1. Mobile Infrastructure Services
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());

        var mockSecureStorage = new Mock<ISecureStorageService>();
        services.AddSingleton(mockSecureStorage.Object);

        var mockApiClient = new Mock<IApiClient>();
        services.AddSingleton(mockApiClient.Object);

        var mockSyncManager = new Mock<IOfflineSyncManager>();
        services.AddSingleton(mockSyncManager.Object);
        services.AddSingleton<IDestinationImageService, DestinationImageService>();
        services.AddSingleton<IMobileMapService, MobileMapService>();
        services.AddSingleton<IMapService>(sp => sp.GetRequiredService<IMobileMapService>());

        // 2. Register all ViewModels (same as MauiProgram.cs)
        services.AddTransient<SplashViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<CustomerHomeViewModel>();
        services.AddTransient<ExploreViewModel>();
        services.AddTransient<MyTripViewModel>();
        services.AddTransient<MyDayViewModel>();
        services.AddTransient<CustomerAIAssistantViewModel>();
        services.AddTransient<PackingListViewModel>();
        services.AddTransient<TripMemoriesViewModel>();
        services.AddTransient<ActivityDetailViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<BookingsViewModel>();
        services.AddTransient<BookingDetailViewModel>();
        services.AddTransient<DocumentsViewModel>();
        services.AddTransient<NotificationsViewModel>();
        services.AddTransient<MoreViewModel>();
        services.AddTransient<AgencyDashboardViewModel>();
        services.AddTransient<AgencySettingsViewModel>();
        services.AddTransient<AgencyCustomersViewModel>();
        services.AddTransient<AgencyCustomerDetailViewModel>();
        services.AddTransient<AgencyTripsViewModel>();
        services.AddTransient<AgencyToursViewModel>();
        services.AddTransient<AgencyHotelsViewModel>();
        services.AddTransient<AgencyStaffDirectoryViewModel>();
        services.AddTransient<AgencyManifestViewModel>();
        services.AddTransient<AgencyFinanceViewModel>();
        services.AddTransient<AgencyAIAssistantViewModel>();
        services.AddTransient<AgencyReportsViewModel>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task NavigationService_TransitionsShellStates_Correctly()
    {
        var nav = new NavigationService();
        var recordedEvents = new List<string>();

        nav.ShellChanged += (shell) => recordedEvents.Add(shell);

        Assert.Equal("Splash", nav.CurrentShell);

        await nav.NavigateToLoginAsync();
        Assert.Equal("Login", nav.CurrentShell);

        await nav.NavigateToForgotPasswordAsync();
        Assert.Equal("ForgotPassword", nav.CurrentShell);

        await nav.NavigateToCustomerShellAsync();
        Assert.Equal("CustomerShell", nav.CurrentShell);

        await nav.NavigateToAgencyShellAsync();
        Assert.Equal("AgencyShell", nav.CurrentShell);

        await nav.NavigateToSplashAsync();
        Assert.Equal("Splash", nav.CurrentShell);

        Assert.Equal(5, recordedEvents.Count);
        Assert.Equal(new[] { "Login", "ForgotPassword", "CustomerShell", "AgencyShell", "Splash" }, recordedEvents);
    }

    [Fact]
    public void MobileDependencyInjection_AllViewModelsResolve_WithoutMissingDependencies()
    {
        var provider = BuildMobileServiceContainer();

        // Customer & Shared ViewModels
        Assert.NotNull(provider.GetRequiredService<SplashViewModel>());
        Assert.NotNull(provider.GetRequiredService<LoginViewModel>());
        Assert.NotNull(provider.GetRequiredService<ForgotPasswordViewModel>());
        Assert.NotNull(provider.GetRequiredService<CustomerHomeViewModel>());
        Assert.NotNull(provider.GetRequiredService<ExploreViewModel>());
        Assert.NotNull(provider.GetRequiredService<MyTripViewModel>());
        Assert.NotNull(provider.GetRequiredService<MyDayViewModel>());
        Assert.NotNull(provider.GetRequiredService<CustomerAIAssistantViewModel>());
        Assert.NotNull(provider.GetRequiredService<PackingListViewModel>());
        Assert.NotNull(provider.GetRequiredService<TripMemoriesViewModel>());
        Assert.NotNull(provider.GetRequiredService<ActivityDetailViewModel>());
        Assert.NotNull(provider.GetRequiredService<ProfileViewModel>());
        Assert.NotNull(provider.GetRequiredService<BookingsViewModel>());
        Assert.NotNull(provider.GetRequiredService<BookingDetailViewModel>());
        Assert.NotNull(provider.GetRequiredService<DocumentsViewModel>());
        Assert.NotNull(provider.GetRequiredService<NotificationsViewModel>());
        Assert.NotNull(provider.GetRequiredService<MoreViewModel>());

        // Agency Operations ViewModels
        Assert.NotNull(provider.GetRequiredService<AgencyDashboardViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencySettingsViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyCustomersViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyCustomerDetailViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyTripsViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyToursViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyHotelsViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyStaffDirectoryViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyManifestViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyFinanceViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyAIAssistantViewModel>());
        Assert.NotNull(provider.GetRequiredService<AgencyReportsViewModel>());
    }

    [Fact]
    public void LocalizationService_SupportsHebrewRtlAndDirectionalProperties()
    {
        var loc = new LocalizationService();

        // Default Hebrew RTL
        loc.SetLanguage("he");
        Assert.Equal("he", loc.CurrentLanguage);
        Assert.True(loc.IsRightToLeft);

        // English LTR
        loc.SetLanguage("en");
        Assert.Equal("en", loc.CurrentLanguage);
        Assert.False(loc.IsRightToLeft);

        // Spanish LTR
        loc.SetLanguage("es");
        Assert.Equal("es", loc.CurrentLanguage);
        Assert.False(loc.IsRightToLeft);
    }

    [Fact]
    public void AgencySettingsViewModel_ChangesLanguage_AndRefreshesState()
    {
        var provider = BuildMobileServiceContainer();
        var vm = provider.GetRequiredService<AgencySettingsViewModel>();
        var loc = provider.GetRequiredService<ILocalizationService>();

        vm.ChangeLanguage("en");
        Assert.Equal("en", vm.SelectedLanguage);
        Assert.Equal("en", loc.CurrentLanguage);

        vm.ChangeLanguage("he");
        Assert.Equal("he", vm.SelectedLanguage);
        Assert.Equal("he", loc.CurrentLanguage);
    }

    [Fact]
    public async Task MoreViewModel_Operations_ExecuteSuccessfully()
    {
        var provider = BuildMobileServiceContainer();
        var vm = provider.GetRequiredService<MoreViewModel>();
        var nav = provider.GetRequiredService<NavigationService>();

        // Language toggle
        vm.ChangeLanguage("es");
        Assert.Equal("es", vm.SelectedLanguage);

        // Trigger sync
        await vm.TriggerOfflineSyncAsync();
        Assert.False(string.IsNullOrWhiteSpace(vm.SyncStatusMessage));

        // Sign out returns to Public Home (CustomerShell)
        await vm.SignOutAsync();
        Assert.Equal("CustomerShell", nav.CurrentShell);
    }

    [Fact]
    public async Task SplashViewModel_AlwaysNavigatesToCustomerShell_NeverLogin()
    {
        var nav = new NavigationService();
        var mockApi = new Mock<IApiClient>();
        mockApi.Setup(a => a.GetBrandingAsync())
            .ReturnsAsync(ApiResponse<BrandSettings>.Ok(BrandSettings.CreateDefault()));

        var loc = new LocalizationService();
        var vm = new SplashViewModel(loc, nav, mockApi.Object);

        // Act
        await vm.InitializeAsync();

        // Assert: ALWAYS launches to CustomerShell, NEVER Login
        Assert.Equal("CustomerShell", nav.CurrentShell);
        Assert.NotEqual("Login", nav.CurrentShell);
    }

    [Fact]
    public async Task SplashViewModel_WhenVideoDisabledInBrand_FallsBackToStaticMode()
    {
        var nav = new NavigationService();
        var mockApi = new Mock<IApiClient>();
        var brand = BrandSettings.CreateDefault();
        brand.SplashVideoEnabled = false;

        mockApi.Setup(a => a.GetBrandingAsync())
            .ReturnsAsync(ApiResponse<BrandSettings>.Ok(brand));

        var loc = new LocalizationService();
        var vm = new SplashViewModel(loc, nav, mockApi.Object);

        // Act
        await vm.InitializeAsync();

        // Assert
        Assert.False(vm.IsVideoAvailable);
        Assert.False(vm.IsVideoPlaying);
        Assert.Equal("CustomerShell", nav.CurrentShell);
    }

    [Fact]
    public async Task SplashViewModel_LogoRevealSequence_IsTriggeredDuringVideo()
    {
        var nav = new NavigationService();
        var mockApi = new Mock<IApiClient>();
        mockApi.Setup(a => a.GetBrandingAsync())
            .ReturnsAsync(ApiResponse<BrandSettings>.Ok(BrandSettings.CreateDefault()));

        var loc = new LocalizationService();
        var vm = new SplashViewModel(loc, nav, mockApi.Object);

        bool logoRevealed = false;
        bool transitionRan = false;

        vm.RequestShowLogo = () =>
        {
            logoRevealed = true;
            return Task.CompletedTask;
        };

        vm.RequestTransitionAnimation = () =>
        {
            transitionRan = true;
            return Task.CompletedTask;
        };

        // Act
        await vm.InitializeAsync();

        // Assert: Logo reveal is called during splash, transition runs, and shell is CustomerShell
        Assert.True(logoRevealed);
        Assert.True(transitionRan);
        Assert.Equal("CustomerShell", nav.CurrentShell);
    }

    [Fact]
    public async Task CustomerHomeViewModel_GuestMode_InitializesComplete9Categories_WithoutLogin()
    {
        var nav = new NavigationService();
        var mockApi = new Mock<IApiClient>();
        mockApi.Setup(a => a.IsAuthenticated).Returns(false);

        var loc = new LocalizationService();
        var img = new DestinationImageService();
        var vm = new CustomerHomeViewModel(loc, nav, mockApi.Object, img);

        // Act
        await vm.InitializeAsync();

        // Assert: Guest mode is fully operational without authentication
        Assert.False(vm.IsAuthenticated);
        Assert.Equal("Tomer Group", vm.CustomerGreeting);

        // 9 Categories verified
        Assert.False(string.IsNullOrWhiteSpace(vm.CategoryTreksLabel));
        Assert.False(string.IsNullOrWhiteSpace(vm.CategoryDestinationsLabel));
        Assert.False(string.IsNullOrWhiteSpace(vm.CategoryToursLabel));
        Assert.False(string.IsNullOrWhiteSpace(vm.CategoryServicesLabel));
        Assert.False(string.IsNullOrWhiteSpace(vm.CategoryHotelsLabel));
        Assert.False(string.IsNullOrWhiteSpace(vm.CategoryActivitiesLabel));
        Assert.False(string.IsNullOrWhiteSpace(vm.CategoryAboutLabel));
        Assert.False(string.IsNullOrWhiteSpace(vm.CategorySupportLabel));
        Assert.False(string.IsNullOrWhiteSpace(vm.CategoryMyTripLabel));

        // Rich collections verified
        Assert.NotEmpty(vm.FeaturedTreks);
        Assert.NotEmpty(vm.FeaturedTours);
        Assert.NotEmpty(vm.CuratedHotels);
        Assert.NotEmpty(vm.AdventureActivities);

        Assert.Contains(vm.FeaturedTreks, t => t.Title.Contains("Salkantay"));
        Assert.Contains(vm.FeaturedTours, t => t.Title.Contains("Sacred Valley"));
        Assert.Contains(vm.CuratedHotels, h => h.Name.Contains("Belmond"));
        Assert.Contains(vm.AdventureActivities, a => a.Title.Contains("Zipline"));
    }
}

