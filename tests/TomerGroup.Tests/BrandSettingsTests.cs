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
        Assert.Equal("#1B365D", brand.PrimaryColor); // Deep Andean Navy
        Assert.Equal("#C28251", brand.SecondaryColor); // Warm Incan Terracotta
        Assert.Equal("#2D9CDB", brand.AccentColor); // High Mountain Sky Blue
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
        brand.PrimaryColor = "#002040";

        // Assert
        Assert.Equal("Tomer Group Expeditions", brand.AgencyName);
        Assert.Equal("#002040", brand.PrimaryColor);
    }
}

