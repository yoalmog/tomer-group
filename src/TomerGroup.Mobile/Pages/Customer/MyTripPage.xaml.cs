#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class MyTripPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly MyTripViewModel? _viewModel;

    public MyTripPage()
    {
        InitializeComponent();
    }

    public MyTripPage(MyTripViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.InitializeAsync();
        }
    }
}
#endif
