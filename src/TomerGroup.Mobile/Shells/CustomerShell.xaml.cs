#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
namespace TomerGroup.Mobile.Shells;

public partial class CustomerShell : Microsoft.Maui.Controls.Shell
{
    public CustomerShell()
    {
        InitializeComponent();
    }
}
#endif

