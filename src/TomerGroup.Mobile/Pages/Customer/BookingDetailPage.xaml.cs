#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class BookingDetailPage : Microsoft.Maui.Controls.ContentPage
{
    public BookingDetailPage(BookingDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif
