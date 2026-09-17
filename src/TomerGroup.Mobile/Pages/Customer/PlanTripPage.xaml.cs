#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class PlanTripPage : Microsoft.Maui.Controls.ContentPage
{
    public PlanTripPage()
    {
        InitializeComponent();
    }

    public PlanTripPage(PlanTripViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif
