namespace TomerGroup.Mobile.Pages.Customer;

public partial class TripSummaryPage : Microsoft.Maui.Controls.ContentPage
{
    public TripSummaryPage()
    {
        InitializeComponent();
    }

    public TripSummaryPage(TomerGroup.Mobile.ViewModels.Customer.TripSummaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
