using TomerGroup.Core.Models;
using Xunit;

namespace TomerGroup.Tests;

public class BrandSettingsTests
{
    [Fact]
    public void DefaultBrandSettings_ShouldHaveTomerGroupIdentity()
    {
        // Act
        var brand = BrandSettings.CreateDefault();

        // Assert
        Assert.Equal("Tomer Group", brand.AgencyName);
        Assert.Equal("Tomer Group", brand.AppName);
        Assert.Equal("Peru Travel Experience", brand.Tagline);
        Assert.Equal("#BC225E", brand.PrimaryColor); // Official Tomer Magenta
        Assert.Equal("#0A0A0C", brand.SecondaryColor); // Deep Luxury Black/Onyx
        Assert.Equal("#E11D68", brand.AccentColor); // Soft Rose Accent
        Assert.Contains("Cusco", brand.Address);
        Assert.NotEmpty(brand.WhatsApp);
        Assert.NotEmpty(brand.EmergencyContact);
    }

    [Fact]
    public void BrandSettings_CanBeCustomizedByAgency()
    {
        // Arrange
        var brand = BrandSettings.CreateDefault();

        // Act
        brand.AgencyName = "Tomer Group Expeditions";
        brand.PrimaryColor = "#BE123C";

        // Assert
        Assert.Equal("Tomer Group Expeditions", brand.AgencyName);
        Assert.Equal("#BE123C", brand.PrimaryColor);
    }
}
