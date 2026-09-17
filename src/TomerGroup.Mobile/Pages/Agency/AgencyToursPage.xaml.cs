#if !NET8_0_OR_GREATER || USE_MAUI
using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyToursPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly AgencyToursViewModel _viewModel;

    public AgencyToursPage(AgencyToursViewModel viewModel)
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

