using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyTasksPage : ContentPage
{
    private readonly AgencyTasksViewModel _viewModel;

    public AgencyTasksPage(AgencyTasksViewModel viewModel)
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

