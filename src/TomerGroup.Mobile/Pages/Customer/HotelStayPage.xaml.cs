namespace TomerGroup.Mobile.Pages.Customer;

public partial class HotelStayPage : Microsoft.Maui.Controls.ContentPage
{
    public HotelStayPage()
    {
        InitializeComponent();
    }

    public HotelStayPage(TomerGroup.Mobile.ViewModels.Customer.HotelStayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
