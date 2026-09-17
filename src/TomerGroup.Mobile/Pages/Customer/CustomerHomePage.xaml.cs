#if !NET8_0_OR_GREATER || USE_MAUI
using TomerGroup.Mobile.ViewModels;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class CustomerHomePage : Microsoft.Maui.Controls.ContentPage
{
    public CustomerHomePage(CustomerHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif

