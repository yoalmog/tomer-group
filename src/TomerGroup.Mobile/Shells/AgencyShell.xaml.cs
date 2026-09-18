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
        Routing.RegisterRoute("AgencyCustomer360", typeof(AgencyCustomer360Page));
        Routing.RegisterRoute("AgencyBookings", typeof(AgencyBookingsPage));
        Routing.RegisterRoute("AgencyTrekEditor", typeof(AgencyTrekEditorPage));
        Routing.RegisterRoute("AgencySupport", typeof(AgencySupportPage));
        Routing.RegisterRoute("AgencyTasks", typeof(AgencyTasksPage));
        Routing.RegisterRoute("TrekMap", typeof(TomerGroup.Mobile.Pages.Customer.TrekMapPage));
    }
}
#endif

