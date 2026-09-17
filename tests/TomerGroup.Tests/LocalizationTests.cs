using TomerGroup.Core.Enums;
using TomerGroup.Core.Localization;
using Xunit;

namespace TomerGroup.Tests;

public class LocalizationTests
{
    [Fact]
    public void DefaultLanguage_ShouldBeHebrewWithTrueRtl()
    {
        // Arrange
        var service = new LocalizationService();

        // Assert
        Assert.Equal("he", service.CurrentLanguage);
        Assert.True(service.IsRightToLeft, "Hebrew layout direction MUST be Right-To-Left");
        Assert.Equal("שלום דני 👋", service.GetString(LocalizationKeys.Greeting));
        Assert.Equal("פרו 🇵🇪", service.GetString(LocalizationKeys.CountryPeru));
        Assert.Equal("הטיול שלי", service.GetString(LocalizationKeys.NavMyTrip));
    }

    [Fact]
    public void EnglishAndSpanish_ShouldHaveLtrLayoutDirection()
    {
        // Arrange
        var service = new LocalizationService();

        // Act & Assert English
        service.SetLanguage("en");
        Assert.Equal("en", service.CurrentLanguage);
        Assert.False(service.IsRightToLeft, "English layout direction MUST be Left-To-Right");
        Assert.Equal("Hello Danny 👋", service.GetString(LocalizationKeys.Greeting));
        Assert.Equal("Peru Travel Experience", service.GetString(LocalizationKeys.Tagline));

        // Act & Assert Spanish
        service.SetLanguage("es");
        Assert.Equal("es", service.CurrentLanguage);
        Assert.False(service.IsRightToLeft, "Spanish layout direction MUST be Left-To-Right");
        Assert.Equal("Hola Danny 👋", service.GetString(LocalizationKeys.Greeting));
        Assert.Equal("Mi Viaje", service.GetString(LocalizationKeys.NavMyTrip));
    }

    [Fact]
    public void PhoneNumbers_MustRetainLtrFormattingInHebrew()
    {
        // Arrange
        var phone = "+51 84 223 456";

        // Act
        var formatted = LocalizationService.FormatPhoneNumber(phone);

        // Assert: Must be wrapped in Left-to-Right embedding \u202A ... \u202C
        Assert.StartsWith("\u202A", formatted);
        Assert.EndsWith("\u202C", formatted);
        Assert.Contains("+51 84 223 456", formatted);
    }

    [Fact]
    public void TechnicalIds_MustRetainLtrFormattingInHebrew()
    {
        // Arrange
        var bookingCode = "TG-2026-00482";
        var passportNumber = "IL-99882211";

        // Act
        var formattedCode = LocalizationService.FormatTechnicalId(bookingCode);
        var formattedPassport = LocalizationService.FormatTechnicalId(passportNumber);

        // Assert
        Assert.StartsWith("\u202A", formattedCode);
        Assert.EndsWith("\u202C", formattedCode);
        Assert.Contains("TG-2026-00482", formattedCode);

        Assert.StartsWith("\u202A", formattedPassport);
        Assert.EndsWith("\u202C", formattedPassport);
    }

    [Fact]
    public void CurrencyFormatting_ShouldDisplayCorrectSymbols()
    {
        // Assert USD, PEN, ILS
        Assert.Equal("$2,500 USD", LocalizationService.FormatCurrency(2500, Currency.USD));
        Assert.Equal("S/ 350 PEN", LocalizationService.FormatCurrency(350, Currency.PEN));
        Assert.Equal("₪850 ILS", LocalizationService.FormatCurrency(850, Currency.ILS));
    }
}

