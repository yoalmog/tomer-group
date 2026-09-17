#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Agency;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyCustomerDetailPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly AgencyCustomerDetailViewModel _viewModel;

    public AgencyCustomerDetailPage(AgencyCustomerDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
#endif

