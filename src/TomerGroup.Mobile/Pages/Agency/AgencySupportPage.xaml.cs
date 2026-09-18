using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencySupportPage : ContentPage
{
    private readonly AgencySupportViewModel _viewModel;

    public AgencySupportPage(AgencySupportViewModel viewModel)
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

