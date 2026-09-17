using TomerGroup.Mobile.Services;
using TomerGroup.Mobile.ViewModels.Customer;

namespace TomerGroup.Tests;

public class PlanTripFlowTests
{
    [Fact]
    public void PlanTripViewModel_ShouldCalculatePriceAndCreateBookingRequest()
    {
        var navigation = new TestNavigationService();
        var viewModel = new PlanTripViewModel(navigation);

        Assert.Equal("Cusco & Sacred Valley", viewModel.SelectedDestination);
        Assert.True(viewModel.EstimatedTotal > 0m);

        viewModel.CreateBooking();

        Assert.Contains("Trip plan created", viewModel.StatusMessage);
        Assert.Contains("Cusco & Sacred Valley", viewModel.StatusMessage);
    }

    [Fact]
    public void TripSummaryViewModel_ShouldShowPremiumItineraryOverview()
    {
        var viewModel = new TripSummaryViewModel();

        Assert.Equal("Cusco & Sacred Valley", viewModel.Destination);
        Assert.Equal(3, viewModel.ItineraryDays.Count);
        Assert.Contains("Machu Picchu", viewModel.ItineraryDays[1].Highlights.First());
    }

    [Fact]
    public void TravelConciergeViewModel_ShouldShowLuxurySupportServices()
    {
        var viewModel = new TravelConciergeViewModel();

        Assert.Equal("Tomer Group Travel Concierge", viewModel.AssistantName);
        Assert.Equal(3, viewModel.Services.Count);
        Assert.Contains("Private airport transfer", viewModel.Services[0].Title);
    }

    [Fact]
    public void HotelStayViewModel_ShouldShowPremiumAccommodationDetails()
    {
        var viewModel = new HotelStayViewModel();

        Assert.Equal("Casa Andina Premium Cusco", viewModel.HotelName);
        Assert.Equal(3, viewModel.Features.Count);
        Assert.Contains("Rooftop spa", viewModel.Features[1]);
    }

    [Fact]
    public void TransferDetailsViewModel_ShouldShowAirportPickupPlan()
    {
        var viewModel = new TransferDetailsViewModel();

        Assert.Equal("Private airport transfer", viewModel.TransferTitle);
        Assert.Equal("Assigned Driver", viewModel.DriverName);
        Assert.Contains("Vehicle", viewModel.VehicleInfo);
    }

    [Fact]
    public void BookingDashboardViewModel_ShouldAggregatePremiumTripOverview()
    {
        var viewModel = new BookingDashboardViewModel();

        Assert.Equal("Cusco & Sacred Valley", viewModel.TripName);
        Assert.Equal("Casa Andina Premium Cusco", viewModel.HotelName);
        Assert.Equal("Private airport transfer", viewModel.TransferTitle);
        Assert.Equal(3, viewModel.ConciergeServices.Count);
    }

    [Fact]
    public void LuxuryBrandingViewModel_ShouldDefinePremiumTravelIdentity()
    {
        var viewModel = new LuxuryBrandingViewModel();

        Assert.Equal("Tomer Group", viewModel.BrandName);
        Assert.Equal("Peru Travel Experience", viewModel.Tagline);
        Assert.Contains("#BC225E", viewModel.AccentColor);
    }

    [Fact]
    public void CheckoutRecapViewModel_ShouldSummarizeFinalLuxuryBooking()
    {
        var viewModel = new CheckoutRecapViewModel();

        Assert.Equal("Cusco & Sacred Valley", viewModel.TripName);
        Assert.Equal("$2,890", viewModel.TotalAmount);
        Assert.Equal("3 premium additions", viewModel.ExtrasLabel);
    }

    private sealed class TestNavigationService : INavigationService
    {
        public string CurrentShell => "CustomerShell";

        public Task NavigateToSplashAsync() => Task.CompletedTask;
        public Task NavigateToLoginAsync() => Task.CompletedTask;
        public Task NavigateToForgotPasswordAsync() => Task.CompletedTask;
        public Task NavigateToCustomerShellAsync() => Task.CompletedTask;
        public Task NavigateToAgencyShellAsync() => Task.CompletedTask;
        public Task NavigateToAsync(string route) => Task.CompletedTask;
        public Task GoBackAsync() => Task.CompletedTask;
    }
}
