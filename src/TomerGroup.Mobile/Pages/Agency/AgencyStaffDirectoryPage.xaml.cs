#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyStaffDirectoryPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly AgencyStaffDirectoryViewModel _viewModel;

    public AgencyStaffDirectoryPage(AgencyStaffDirectoryViewModel viewModel)
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

