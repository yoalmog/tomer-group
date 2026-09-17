#if !NET8_0_OR_GREATER || USE_MAUI
using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyManifestPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly AgencyManifestViewModel _viewModel;

    public AgencyManifestPage(AgencyManifestViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
#endif

