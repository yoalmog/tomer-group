using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using Xunit;

namespace TomerGroup.Tests;

public class DomainModelTests
{
    [Fact]
    public void Trip_ShouldCalculateGrossProfitAccurately()
    {
        // Arrange (Section 22: Revenue $2500, Costs $1700, Gross Profit $800)
        var trip = new Trip
        {
            TripCode = "PERU-2026-00482",
            Title = "Danny's Peru Travel Experience",
            TotalRevenue = 2500m,
            TotalCost = 1700m,
            Currency = Currency.USD
        };

        // Act & Assert
        Assert.Equal(800m, trip.GrossProfit);
    }

    [Fact]
    public void Booking_ShouldCalculateOutstandingAmountCorrectly()
    {
        // Arrange
        var booking = new Booking
        {
            BookingCode = "TG-2026-00482",
            TotalAmount = 2500m,
            PaidAmount = 1500m,
            Currency = Currency.USD,
            Status = BookingStatus.PartiallyPaid
        };

        // Act & Assert
        Assert.Equal(1000m, booking.OutstandingAmount);
    }

    [Theory]
    [InlineData("TG-2026-00482")]
    [InlineData("TG-2026-00001")]
    public void BookingCode_ShouldFollowTomerGroupStandard(string code)
    {
        Assert.StartsWith("TG-", code);
        Assert.Equal(13, code.Length);
    }

    [Theory]
    [InlineData("PERU-2026-00482")]
    [InlineData("PERU-2026-00001")]
    public void TripCode_ShouldFollowPeruFormat(string code)
    {
        Assert.StartsWith("PERU-", code);
        Assert.Equal(15, code.Length);
    }
}

