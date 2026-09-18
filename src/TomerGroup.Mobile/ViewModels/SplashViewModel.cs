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
        Localization.LanguageChanged += OnLocalizationLanguageChanged;
    }

    private void OnLocalizationLanguageChanged()
    {
        RefreshDirection();
        OnLanguageChanged();
        OnPropertyChanged(string.Empty);
    }

    protected virtual void OnLanguageChanged()
    {
    }

    public string Localize(string key) => Localization.GetString(key);

    public string CurrentLanguage => Localization.CurrentLanguage;
    public bool IsHebrewSelected => Localization.CurrentLanguage.Equals("he", StringComparison.OrdinalIgnoreCase);
    public bool IsEnglishSelected => Localization.CurrentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);
    public bool IsSpanishSelected => Localization.CurrentLanguage.Equals("es", StringComparison.OrdinalIgnoreCase);

    public string HebrewButtonBg => IsHebrewSelected ? "#BC225E" : "#F1F5F9";
    public string HebrewButtonText => IsHebrewSelected ? "#FFFFFF" : "#0F172A";
    public string HebrewFontWeight => IsHebrewSelected ? "Bold" : "None";

    public string EnglishButtonBg => IsEnglishSelected ? "#BC225E" : "#F1F5F9";
    public string EnglishButtonText => IsEnglishSelected ? "#FFFFFF" : "#0F172A";
    public string EnglishFontWeight => IsEnglishSelected ? "Bold" : "None";

    public string SpanishButtonBg => IsSpanishSelected ? "#BC225E" : "#F1F5F9";
    public string SpanishButtonText => IsSpanishSelected ? "#FFFFFF" : "#0F172A";
    public string SpanishFontWeight => IsSpanishSelected ? "Bold" : "None";

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

    [ObservableProperty]
    private bool _isVideoAvailable = true;

    [ObservableProperty]
    private bool _isVideoPlaying = true;

    [ObservableProperty]
    private string _videoHtmlContent = string.Empty;

    public Func<Task>? RequestTransitionAnimation { get; set; }

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
        var startTime = DateTime.UtcNow;
        IsBusy = true;
        StatusMessage = Localize(LocalizationKeys.Loading);

        try
        {
            // Initialize cinematic video content
            await PrepareVideoAssetAsync();

            // Load dynamic branding in parallel
            var brandTask = _apiClient.GetBrandingAsync();

            string? token = null;
            if (_secureStorage != null)
            {
                token = await _secureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _apiClient.SetAuthToken(token);
                }
            }

            var brandResult = await brandTask;
            if (brandResult.Success && brandResult.Data != null)
            {
                Brand = brandResult.Data;
                Tagline = Brand.Tagline;
                if (!Brand.SplashVideoEnabled)
                {
                    IsVideoAvailable = false;
                    IsVideoPlaying = false;
                }
            }

            // Target cinematic splash duration: approximately 2.5 - 3.0 seconds
            var elapsedMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            var targetDurationMs = IsVideoAvailable ? 2800 : 800;
            var remainingMs = targetDurationMs - elapsedMs;

            if (remainingMs > 50)
            {
                await Task.Delay(remainingMs);
            }

            // Smooth fade-out animation if supported by UI
            if (RequestTransitionAnimation != null)
            {
                await RequestTransitionAnimation();
            }

            // ALWAYS navigate to the Public Customer Home screen (never Login screen on startup)
            await Navigation.NavigateToCustomerShellAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsVideoAvailable = false;
            // Graceful fallback to Public Home
            await Navigation.NavigateToCustomerShellAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task PrepareVideoAssetAsync()
    {
        try
        {
            byte[]? videoBytes = null;

#if USE_MAUI
            try
            {
                using var stream = await Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("splash_trek_video.mp4");
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                videoBytes = ms.ToArray();
            }
            catch
            {
                // Fallback: check Raw folder directly on disk if local debugging
                var localRaw = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Raw", "splash_trek_video.mp4");
                if (File.Exists(localRaw))
                {
                    videoBytes = await File.ReadAllBytesAsync(localRaw);
                }
            }
#else
            var localRaw = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Raw", "splash_trek_video.mp4");
            if (File.Exists(localRaw))
            {
                videoBytes = await File.ReadAllBytesAsync(localRaw);
            }
#endif

            if (videoBytes != null && videoBytes.Length > 0)
            {
                var base64 = Convert.ToBase64String(videoBytes);
                VideoHtmlContent = GenerateVideoHtml(base64);
                IsVideoAvailable = true;
                IsVideoPlaying = true;
            }
            else
            {
                // Video asset could not be loaded -> graceful static fallback
                IsVideoAvailable = false;
                IsVideoPlaying = false;
            }
        }
        catch
        {
            // Never crash on video preparation failure
            IsVideoAvailable = false;
            IsVideoPlaying = false;
        }
    }

    private static string GenerateVideoHtml(string base64Video)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
<meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
<style>
  * {{ margin: 0; padding: 0; box-sizing: border-box; }}
  html, body {{
    width: 100vw;
    height: 100vh;
    overflow: hidden;
    background-color: #000000;
  }}
  video {{
    position: absolute;
    top: 50%;
    left: 50%;
    width: 100vw;
    height: 100vh;
    transform: translate(-50%, -50%);
    object-fit: cover;
    pointer-events: none;
  }}
</style>
</head>
<body>
  <video id='splashVideo' autoplay muted playsinline loop preload='auto'>
    <source src='data:video/mp4;base64,{base64Video}' type='video/mp4'>
  </video>
</body>
</html>";
    }
}

