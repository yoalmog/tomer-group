#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class DocumentsPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly DocumentsViewModel? _viewModel;

    public DocumentsPage()
    {
        InitializeComponent();
    }

    public DocumentsPage(DocumentsViewModel viewModel)
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
