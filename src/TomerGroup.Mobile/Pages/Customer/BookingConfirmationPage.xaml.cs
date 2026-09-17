namespace TomerGroup.Mobile.Pages.Customer;

public partial class BookingConfirmationPage : Microsoft.Maui.Controls.ContentPage
{
    public BookingConfirmationPage()
    {
        InitializeComponent();
    }

    public BookingConfirmationPage(TomerGroup.Mobile.ViewModels.Customer.BookingConfirmationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
