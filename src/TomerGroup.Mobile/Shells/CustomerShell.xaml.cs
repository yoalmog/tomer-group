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
        Routing.RegisterRoute("PlanTrip", typeof(PlanTripPage));
        Routing.RegisterRoute("BookingConfirmation", typeof(BookingConfirmationPage));
        Routing.RegisterRoute("Payment", typeof(PaymentPage));
        Routing.RegisterRoute("CheckoutRecap", typeof(CheckoutRecapPage));
        Routing.RegisterRoute("AddOns", typeof(AddOnsPage));
        Routing.RegisterRoute("PreTripChecklist", typeof(PreTripChecklistPage));
        Routing.RegisterRoute("TripSummary", typeof(TripSummaryPage));
        Routing.RegisterRoute("TravelConcierge", typeof(TravelConciergePage));
        Routing.RegisterRoute("HotelStay", typeof(HotelStayPage));
        Routing.RegisterRoute("TransferDetails", typeof(TransferDetailsPage));
        Routing.RegisterRoute("BookingDashboard", typeof(BookingDashboardPage));
        Routing.RegisterRoute("LuxuryBranding", typeof(LuxuryBrandingPage));
    }
}
#endif
