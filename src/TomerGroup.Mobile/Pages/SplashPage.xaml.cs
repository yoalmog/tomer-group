#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels;

namespace TomerGroup.Mobile.Pages;

public partial class SplashPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly SplashViewModel _viewModel;

    public SplashPage(SplashViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Wire smooth logo reveal during the final phase of video
        _viewModel.RequestShowLogo = async () =>
        {
            if (OverlayContainer != null)
            {
                await Task.WhenAll(
                    OverlayContainer.FadeTo(1.0, 700, Easing.CubicOut),
                    OverlayContainer.ScaleTo(1.0, 700, Easing.CubicOut)
                );
            }
        };

        // Wire smooth transition animation directly to Public Home
        _viewModel.RequestTransitionAnimation = async () =>
        {
            await this.FadeTo(0.0, 300, Easing.CubicOut);
        };

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.VideoHtmlContent))
            {
                MainThread.BeginInvokeOnMainThread(UpdateVideoSource);
            }
        };
    }

    private void UpdateVideoSource()
    {
        if (VideoWebView != null && !string.IsNullOrEmpty(_viewModel.VideoHtmlContent))
        {
            VideoWebView.Source = new HtmlWebViewSource
            {
                Html = _viewModel.VideoHtmlContent
            };
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateVideoSource();
        await _viewModel.InitializeAsync();
    }
}
#endif

