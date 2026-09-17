namespace TomerGroup.Mobile.Pages.Customer;

public partial class LuxuryBrandingPage : Microsoft.Maui.Controls.ContentPage
{
    public LuxuryBrandingPage()
    {
        InitializeComponent();
    }

    public LuxuryBrandingPage(TomerGroup.Mobile.ViewModels.Customer.LuxuryBrandingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
