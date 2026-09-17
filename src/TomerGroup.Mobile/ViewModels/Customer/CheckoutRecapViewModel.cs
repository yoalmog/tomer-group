namespace TomerGroup.Mobile.ViewModels.Customer;

public class CheckoutRecapViewModel
{
    public string TripName { get; } = "Cusco & Sacred Valley";
    public string ConfirmationCode { get; } = "TG-PRU-2048";
    public string TotalAmount { get; } = "$2,890";
    public string DepositAmount { get; } = "$1,200";
    public string ExtrasLabel { get; } = "3 premium additions";
    public string StatusText { get; } = "Payment secured and itinerary locked in.";
    public string TravelerSummary { get; } = "2 adults • 1 suite • Flexible dates";
    public string NextStep { get; } = "Your advisor will send vouchers and final confirmations within 30 minutes.";
}
