namespace TomerGroup.Mobile.Pages.Customer;

public partial class CheckoutRecapPage : Microsoft.Maui.Controls.ContentPage
{
    public CheckoutRecapPage()
    {
        InitializeComponent();
    }

    public CheckoutRecapPage(TomerGroup.Mobile.ViewModels.Customer.CheckoutRecapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
