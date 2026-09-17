namespace TomerGroup.Mobile.Pages.Customer;

public partial class PreTripChecklistPage : Microsoft.Maui.Controls.ContentPage
{
    public PreTripChecklistPage()
    {
        InitializeComponent();
    }

    public PreTripChecklistPage(TomerGroup.Mobile.ViewModels.Customer.PreTripChecklistViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
