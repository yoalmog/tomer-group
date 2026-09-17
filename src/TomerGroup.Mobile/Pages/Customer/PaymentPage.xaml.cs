namespace TomerGroup.Mobile.Pages.Customer;

public partial class PaymentPage : Microsoft.Maui.Controls.ContentPage
{
    public PaymentPage()
    {
        InitializeComponent();
    }

    public PaymentPage(TomerGroup.Mobile.ViewModels.Customer.PaymentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
