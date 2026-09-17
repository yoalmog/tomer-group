#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
namespace TomerGroup.Mobile.Shells;

public partial class AgencyShell : Microsoft.Maui.Controls.Shell
{
    public AgencyShell()
    {
        InitializeComponent();
    }
}
#endif

