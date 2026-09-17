#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
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

