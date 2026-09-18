#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using Microsoft.Maui.Controls;
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Mobile.Pages.Customer;

public partial class TrekMapPage : ContentPage
{
    public TrekMapPage(TrekMapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
#endif

