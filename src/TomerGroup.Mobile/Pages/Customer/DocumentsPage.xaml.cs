#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class DocumentsPage : Microsoft.Maui.Controls.ContentPage
{
    private readonly DocumentsViewModel _viewModel;

    public DocumentsPage(DocumentsViewModel viewModel)
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
