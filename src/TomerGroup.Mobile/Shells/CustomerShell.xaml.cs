#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using Microsoft.Maui.Controls;
using TomerGroup.Mobile.Pages;
using TomerGroup.Mobile.Pages.Customer;

namespace TomerGroup.Mobile.Shells;

public partial class CustomerShell : Microsoft.Maui.Controls.Shell
{
    public CustomerShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("ActivityDetail", typeof(ActivityDetailPage));
        Routing.RegisterRoute("BookingDetail", typeof(BookingDetailPage));
        Routing.RegisterRoute("Documents", typeof(DocumentsPage));
        Routing.RegisterRoute("Notifications", typeof(NotificationsPage));
        Routing.RegisterRoute("Login", typeof(LoginPage));
        Routing.RegisterRoute("ForgotPassword", typeof(ForgotPasswordPage));
        Routing.RegisterRoute("More", typeof(MorePage));
        Routing.RegisterRoute("MyDay", typeof(MyDayPage));
        Routing.RegisterRoute("CustomerAIAssistant", typeof(CustomerAIAssistantPage));
        Routing.RegisterRoute("PackingList", typeof(PackingListPage));
        Routing.RegisterRoute("TripMemories", typeof(TripMemoriesPage));
    }
}
#endif
