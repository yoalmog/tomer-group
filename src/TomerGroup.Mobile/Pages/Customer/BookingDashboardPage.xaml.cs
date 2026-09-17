namespace TomerGroup.Mobile.Pages.Customer;

public partial class BookingDashboardPage : Microsoft.Maui.Controls.ContentPage
{
    public BookingDashboardPage()
    {
        InitializeComponent();
    }

    public BookingDashboardPage(TomerGroup.Mobile.ViewModels.Customer.BookingDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
