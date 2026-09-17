namespace TomerGroup.Mobile.Pages.Customer;

public partial class AddOnsPage : Microsoft.Maui.Controls.ContentPage
{
    public AddOnsPage()
    {
        InitializeComponent();
    }

    public AddOnsPage(TomerGroup.Mobile.ViewModels.Customer.AddOnsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
