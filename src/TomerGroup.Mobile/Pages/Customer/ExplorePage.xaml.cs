#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class ExplorePage : Microsoft.Maui.Controls.ContentPage
{
    private readonly ExploreViewModel _viewModel;

    public ExplorePage(ExploreViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
}
#endif
