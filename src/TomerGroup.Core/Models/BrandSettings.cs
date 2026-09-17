namespace TomerGroup.Core.Models;

/// <summary>
/// Centralized dynamic branding configuration for Tomer Group.
/// Configurable by the agency owner; never hardcoded across UI pages.
/// </summary>
public class BrandSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AgencyName { get; set; } = "Tomer Group";
    public string AppName { get; set; } = "Tomer Group";
    public string Tagline { get; set; } = "Peru Travel Experience";
    public string LogoUrl { get; set; } = "https://tomergroup.com/assets/logo.png";
    public string LogoPlaceholder { get; set; } = "tomer_group_logo";

    // Tomer Group Official Brand Palette: Brand Magenta, Deep Luxury Black/Onyx, Clean White
    public string PrimaryColor { get; set; } = "#BC225E";
    public string SecondaryColor { get; set; } = "#0A0A0C";
    public string AccentColor { get; set; } = "#E11D68";
    public string BackgroundLight { get; set; } = "#FAF9FB";
    public string BackgroundDark { get; set; } = "#000000";
    public string SurfaceLight { get; set; } = "#FFFFFF";
    public string SurfaceDark { get; set; } = "#121218";
    public string TextLight { get; set; } = "#0A0A0C";
    public string TextDark { get; set; } = "#FAF9FB";

    // Contact Information (Cusco headquarters)
    public string ContactPhone { get; set; } = "+51 84 223 456";
    public string WhatsApp { get; set; } = "+51 984 123 456";
    public string Email { get; set; } = "info@tomergroup.com";
    public string Website { get; set; } = "https://tomergroup.com";
    public string Address { get; set; } = "Portal de Panes 123, Plaza de Armas, Cusco, Peru";
    public string EmergencyContact { get; set; } = "+51 984 999 888";

    // Social Links
    public Dictionary<string, string> SocialLinks { get; set; } = new()
    {
        { "Instagram", "https://instagram.com/tomergroup_peru" },
        { "Facebook", "https://facebook.com/tomergroup" },
        { "WhatsAppGroup", "https://chat.whatsapp.com/tomergroup_cusco" }
    };

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static BrandSettings CreateDefault() => new();
}
