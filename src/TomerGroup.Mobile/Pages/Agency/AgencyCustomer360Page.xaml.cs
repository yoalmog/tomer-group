using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyCustomer360Page : ContentPage
{
    private readonly AgencyCustomer360ViewModel _viewModel;

    public AgencyCustomer360Page(AgencyCustomer360ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCustomer360Async();
    }
}

