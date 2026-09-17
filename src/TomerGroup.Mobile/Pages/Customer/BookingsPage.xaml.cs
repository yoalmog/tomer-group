#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class BookingsPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly BookingsViewModel? _viewModel;

    public BookingsPage()
    {
        InitializeComponent();
    }

    public BookingsPage(BookingsViewModel viewModel)
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
