using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Core.Models;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private bool _isRtl = true; // Default Hebrew RTL

    [ObservableProperty]
    private BrandSettings _brand = BrandSettings.CreateDefault();

    protected readonly ILocalizationService Localization;
    protected readonly INavigationService Navigation;

    protected BaseViewModel(ILocalizationService localization, INavigationService navigation)
    {
        Localization = localization;
        Navigation = navigation;
        _isRtl = Localization.IsRightToLeft;
    }

    public string Localize(string key) => Localization.GetString(key);

    public void RefreshDirection()
    {
        IsRtl = Localization.IsRightToLeft;
    }
}

public partial class SplashViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService? _secureStorage;

    [ObservableProperty]
    private string _tagline = "Peru Travel Experience";

    [ObservableProperty]
    private string _statusMessage = "Loading travel experience...";

    public SplashViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IApiClient apiClient,
        ISecureStorageService? secureStorage = null)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;
        Title = "Tomer Group";
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        StatusMessage = Localize(LocalizationKeys.Loading);

        try
        {
            // Load dynamic branding
            var brandResult = await _apiClient.GetBrandingAsync();
            if (brandResult.Success && brandResult.Data != null)
            {
                Brand = brandResult.Data;
                Tagline = Brand.Tagline;
            }

            if (_secureStorage != null)
            {
                var token = await _secureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _apiClient.SetAuthToken(token);
                }
            }

            // Brief delay for splash presentation
            await Task.Delay(300);

            // SPLASH -> HOME (Always launch into the Tomer Group travel experience)
            await Navigation.NavigateToCustomerShellAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await Navigation.NavigateToCustomerShellAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}

