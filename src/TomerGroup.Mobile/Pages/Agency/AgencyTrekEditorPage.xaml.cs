using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyTrekEditorPage : ContentPage
{
    private readonly AgencyTrekEditorViewModel _viewModel;

    public AgencyTrekEditorPage(AgencyTrekEditorViewModel viewModel)
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

