#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using Microsoft.Maui.Controls;
using TomerGroup.Mobile.Pages.Agency;

namespace TomerGroup.Mobile.Shells;

public partial class AgencyShell : Microsoft.Maui.Controls.Shell
{
    public AgencyShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("AgencyCustomerDetail", typeof(AgencyCustomerDetailPage));
        Routing.RegisterRoute("TrekMap", typeof(TomerGroup.Mobile.Pages.Customer.TrekMapPage));
    }
}
#endif

