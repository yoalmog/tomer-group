namespace TomerGroup.Mobile.Pages.Customer;

public partial class TransferDetailsPage : Microsoft.Maui.Controls.ContentPage
{
    public TransferDetailsPage()
    {
        InitializeComponent();
    }

    public TransferDetailsPage(TomerGroup.Mobile.ViewModels.Customer.TransferDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
