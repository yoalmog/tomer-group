#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.Pages;
using TomerGroup.Mobile.Services;
using TomerGroup.Mobile.Shells;

namespace TomerGroup.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly NavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public App(NavigationService navigationService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;

        _navigationService.ShellChanged += OnShellChanged;

        // Start with the branded travel homepage so the first screen feels like the agency website.
        MainPage = new Microsoft.Maui.Controls.NavigationPage(_serviceProvider.GetRequiredService<LoginPage>());
    }

    private void OnShellChanged(string shellName)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (shellName)
            {
                case "CustomerShell":
                    MainPage = _serviceProvider.GetRequiredService<CustomerShell>();
                    break;
                case "AgencyShell":
                    MainPage = _serviceProvider.GetRequiredService<AgencyShell>();
                    break;
                case "Login":
                    MainPage = new Microsoft.Maui.Controls.NavigationPage(_serviceProvider.GetRequiredService<LoginPage>());
                    break;
                case "ForgotPassword":
                    MainPage = new Microsoft.Maui.Controls.NavigationPage(_serviceProvider.GetRequiredService<ForgotPasswordPage>());
                    break;
                default:
                    MainPage = _serviceProvider.GetRequiredService<SplashPage>();
                    break;
            }
        });
    }
}
#endif

