#if !NET8_0_OR_GREATER || USE_MAUI
using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyTripsPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly AgencyTripsViewModel _viewModel;

    public AgencyTripsPage(AgencyTripsViewModel viewModel)
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

