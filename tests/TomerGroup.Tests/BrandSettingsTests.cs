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
        Assert.Equal("#E11D48", brand.PrimaryColor); // Official Tomer Magenta
        Assert.Equal("#0F172A", brand.SecondaryColor); // Deep Luxury Black/Slate
        Assert.Equal("#FB7185", brand.AccentColor); // Soft Rose Accent
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
