using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
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

        // 2. Register all ViewModels (same as MauiProgram.cs)
        services.AddTransient<SplashViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<CustomerHomeViewModel>();
        services.AddTransient<MyTripViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<BookingsViewModel>();
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
        Assert.NotNull(provider.GetRequiredService<MyTripViewModel>());
        Assert.NotNull(provider.GetRequiredService<ProfileViewModel>());
        Assert.NotNull(provider.GetRequiredService<BookingsViewModel>());
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

        // Sign out
        await vm.SignOutAsync();
        Assert.Equal("Login", nav.CurrentShell);
    }
}

