#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyReportsPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly AgencyReportsViewModel? _viewModel;

    public AgencyReportsPage()
    {
        InitializeComponent();
    }

    public AgencyReportsPage(AgencyReportsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.InitializeAsync();
        }
    }
}
#endif

