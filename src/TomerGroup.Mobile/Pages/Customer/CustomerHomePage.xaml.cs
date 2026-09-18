#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class CustomerHomePage : Microsoft.Maui.Controls.ContentPage
{
    private readonly CustomerHomeViewModel _viewModel;

    public CustomerHomePage(CustomerHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomerHomePage.OnAppearing] Error: {ex.Message}");
        }
    }
}
#endif
