using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyBookingsPage : ContentPage
{
    private readonly AgencyBookingsViewModel _viewModel;

    public AgencyBookingsPage(AgencyBookingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}

