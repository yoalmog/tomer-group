#if !NET8_0_OR_GREATER || USE_MAUI
using TomerGroup.Mobile.ViewModels;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class MyTripPage : Microsoft.Maui.Controls.ContentPage
{
    public MyTripPage(MyTripViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif

