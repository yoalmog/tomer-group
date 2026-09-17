#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels;

namespace TomerGroup.Mobile.Pages.Agency;

public partial class AgencySettingsPage : Microsoft.Maui.Controls.ContentPage
{
    public AgencySettingsPage(AgencySettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif

