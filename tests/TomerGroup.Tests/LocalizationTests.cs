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
        Assert.Equal("שלום 👋", service.GetString(LocalizationKeys.Greeting));
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
        Assert.Equal("Hello 👋", service.GetString(LocalizationKeys.Greeting));
        Assert.Equal("Peru Travel Experience", service.GetString(LocalizationKeys.Tagline));

        // Act & Assert Spanish
        service.SetLanguage("es");
        Assert.Equal("es", service.CurrentLanguage);
        Assert.False(service.IsRightToLeft, "Spanish layout direction MUST be Left-To-Right");
        Assert.Equal("Hola 👋", service.GetString(LocalizationKeys.Greeting));
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

    [Fact]
    public void LanguagePersistence_ShouldLoadAndSaveViaHooks()
    {
        // Arrange simulated Preferences storage
        string storedLanguage = "es";
        LocalizationService.LanguageGetter = d => storedLanguage;
        LocalizationService.LanguageSetter = l => storedLanguage = l;

        try
        {
            // Act 1: Initializing service should load "es" from getter
            var service = new LocalizationService();
            Assert.Equal("es", service.CurrentLanguage);

            // Act 2: Setting language should persist to setter
            bool eventFired = false;
            service.LanguageChanged += () => eventFired = true;
            service.SetLanguage("en");

            // Assert
            Assert.Equal("en", service.CurrentLanguage);
            Assert.Equal("en", storedLanguage);
            Assert.True(eventFired);
        }
        finally
        {
            // Reset hooks
            LocalizationService.LanguageGetter = null;
            LocalizationService.LanguageSetter = null;
        }
    }

    [Fact]
    public void MapService_Coordinates_MustUseInvariantCultureDotSeparators()
    {
        // Arrange
        var mobileMap = new TomerGroup.Mobile.Services.MobileMapService();
        var infraMap = new TomerGroup.Infrastructure.Services.MapService();

        // Act
        var mobileUrl = mobileMap.GetMapNavigationUrl(-13.5168, -71.9789, "Cusco");
        var infraUrl = infraMap.GetMapNavigationUrl(-13.5168, -71.9789, "Cusco");

        // Assert: URLs must contain dot decimals, NEVER comma decimals
        Assert.Contains("-13.516800,-71.978900", mobileUrl);
        Assert.DoesNotContain("-13,5168", mobileUrl);

        Assert.Contains("-13.516800,-71.978900", infraUrl);
        Assert.DoesNotContain("-13,5168", infraUrl);
    }

    [Fact]
    public void MapService_HebrewAliases_ShouldResolveCoordinates()
    {
        // Arrange
        var mobileMap = new TomerGroup.Mobile.Services.MobileMapService();

        // Act
        var cusco = mobileMap.GetCoordinates("קוסקו");
        var machu = mobileMap.GetCoordinates("מאצ'ו פיצ'ו");
        var rainbow = mobileMap.GetCoordinates("הר הצבעים");

        // Assert
        Assert.Equal(-13.5168, cusco.Lat, 4);
        Assert.Equal(-71.9789, cusco.Lng, 4);

        Assert.Equal(-13.1631, machu.Lat, 4);
        Assert.Equal(-72.5450, machu.Lng, 4);

        Assert.Equal(-13.8694, rainbow.Lat, 4);
        Assert.Equal(-71.3031, rainbow.Lng, 4);
    }
}

