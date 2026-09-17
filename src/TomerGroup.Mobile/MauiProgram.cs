#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Pages;
using TomerGroup.Mobile.Pages.Agency;
using TomerGroup.Mobile.Pages.Customer;
using TomerGroup.Mobile.Services;
using TomerGroup.Mobile.Shells;
using TomerGroup.Mobile.ViewModels;
using TomerGroup.Mobile.ViewModels.Agency;
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Core Services
        builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
        builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();
        builder.Services.AddSingleton<IDestinationImageService, DestinationImageService>();
        builder.Services.AddSingleton<IMobileMapService, MobileMapService>();
        builder.Services.AddSingleton<IMapService>(sp => sp.GetRequiredService<IMobileMapService>());
        var navService = new NavigationService();
        builder.Services.AddSingleton<NavigationService>(navService);
        builder.Services.AddSingleton<INavigationService>(navService);
        builder.Services.AddSingleton<IOfflineSyncManager, OfflineSyncManager>();

        // Configure HTTP client for API communication
        builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            // Connects to local ASP.NET Core API
            client.BaseAddress = new Uri("http://localhost:5000/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // Register ViewModels
        builder.Services.AddTransient<SplashViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<CustomerHomeViewModel>();
        builder.Services.AddTransient<ExploreViewModel>();
        builder.Services.AddTransient<MyTripViewModel>();
        builder.Services.AddTransient<ActivityDetailViewModel>();
        builder.Services.AddTransient<PlanTripViewModel>();
        builder.Services.AddTransient<BookingConfirmationViewModel>();
        builder.Services.AddTransient<AddOnsViewModel>();
        builder.Services.AddTransient<PaymentViewModel>();
        builder.Services.AddTransient<CheckoutRecapViewModel>();
        builder.Services.AddTransient<PreTripChecklistViewModel>();
        builder.Services.AddTransient<TripSummaryViewModel>();
        builder.Services.AddTransient<TravelConciergeViewModel>();
        builder.Services.AddTransient<HotelStayViewModel>();
        builder.Services.AddTransient<TransferDetailsViewModel>();
        builder.Services.AddTransient<BookingDashboardViewModel>();
        builder.Services.AddTransient<LuxuryBrandingViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<BookingsViewModel>();
        builder.Services.AddTransient<BookingDetailViewModel>();
        builder.Services.AddTransient<DocumentsViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<MoreViewModel>();
        builder.Services.AddTransient<MyDayViewModel>();
        builder.Services.AddTransient<CustomerAIAssistantViewModel>();
        builder.Services.AddTransient<PackingListViewModel>();
        builder.Services.AddTransient<TripMemoriesViewModel>();
        builder.Services.AddTransient<AgencyDashboardViewModel>();
        builder.Services.AddTransient<AgencySettingsViewModel>();
        builder.Services.AddTransient<AgencyCustomersViewModel>();
        builder.Services.AddTransient<AgencyCustomerDetailViewModel>();
        builder.Services.AddTransient<AgencyTripsViewModel>();
        builder.Services.AddTransient<AgencyToursViewModel>();
        builder.Services.AddTransient<AgencyHotelsViewModel>();
        builder.Services.AddTransient<AgencyStaffDirectoryViewModel>();
        builder.Services.AddTransient<AgencyManifestViewModel>();
        builder.Services.AddTransient<AgencyFinanceViewModel>();
        builder.Services.AddTransient<AgencyAIAssistantViewModel>();
        builder.Services.AddTransient<AgencyReportsViewModel>();

        // Register Shells and Pages
        builder.Services.AddTransient<CustomerShell>();
        builder.Services.AddTransient<AgencyShell>();
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<CustomerHomePage>();
        builder.Services.AddTransient<ExplorePage>();
        builder.Services.AddTransient<MyTripPage>();
        builder.Services.AddTransient<MyDayPage>();
        builder.Services.AddTransient<ActivityDetailPage>();
        builder.Services.AddTransient<CustomerAIAssistantPage>();
        builder.Services.AddTransient<PackingListPage>();
        builder.Services.AddTransient<TripMemoriesPage>();
        builder.Services.AddTransient<PlanTripPage>();
        builder.Services.AddTransient<BookingConfirmationPage>();
        builder.Services.AddTransient<AddOnsPage>();
        builder.Services.AddTransient<PaymentPage>();
        builder.Services.AddTransient<CheckoutRecapPage>();
        builder.Services.AddTransient<PreTripChecklistPage>();
        builder.Services.AddTransient<TripSummaryPage>();
        builder.Services.AddTransient<TravelConciergePage>();
        builder.Services.AddTransient<HotelStayPage>();
        builder.Services.AddTransient<TransferDetailsPage>();
        builder.Services.AddTransient<BookingDashboardPage>();
        builder.Services.AddTransient<LuxuryBrandingPage>();
        builder.Services.AddTransient<BookingsPage>();
        builder.Services.AddTransient<BookingDetailPage>();
        builder.Services.AddTransient<DocumentsPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<MorePage>();
        builder.Services.AddTransient<AgencyDashboardPage>();
        builder.Services.AddTransient<AgencySettingsPage>();
        builder.Services.AddTransient<AgencyCustomersPage>();
        builder.Services.AddTransient<AgencyCustomerDetailPage>();
        builder.Services.AddTransient<AgencyTripsPage>();
        builder.Services.AddTransient<AgencyToursPage>();
        builder.Services.AddTransient<AgencyHotelsPage>();
        builder.Services.AddTransient<AgencyStaffDirectoryPage>();
        builder.Services.AddTransient<AgencyManifestPage>();
        builder.Services.AddTransient<AgencyFinancePage>();
        builder.Services.AddTransient<AgencyAIAssistantPage>();
        builder.Services.AddTransient<AgencyReportsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
#endif
