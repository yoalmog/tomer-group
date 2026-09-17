#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencyDashboardPage : Microsoft.Maui.Controls.ContentPage
{
    public AgencyDashboardPage(AgencyDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif

