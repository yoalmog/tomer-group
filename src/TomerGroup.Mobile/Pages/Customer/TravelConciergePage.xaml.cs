namespace TomerGroup.Mobile.Pages.Customer;

public partial class TravelConciergePage : Microsoft.Maui.Controls.ContentPage
{
    public TravelConciergePage()
    {
        InitializeComponent();
    }

    public TravelConciergePage(TomerGroup.Mobile.ViewModels.Customer.TravelConciergeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
