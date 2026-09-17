#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class ActivityDetailPage : Microsoft.Maui.Controls.ContentPage
{
    public ActivityDetailPage(ActivityDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif
