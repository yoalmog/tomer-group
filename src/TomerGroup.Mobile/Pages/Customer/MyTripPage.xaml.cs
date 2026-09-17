#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class MyTripPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly MyTripViewModel _viewModel;

    public MyTripPage(MyTripViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
#endif
