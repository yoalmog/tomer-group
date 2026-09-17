namespace TomerGroup.Mobile.Services;

public interface IDestinationImageService
{
    string GetDestinationHeroImage(string? destination);
    string GetActivityImage(string? activityTitle, string? category = null);
    string GetTourImage(string? tourTitle);
    string GetDefaultPeruHeroImage();
    string GetMachuPicchuImage();
    string GetCuscoImage();
    string GetSacredValleyImage();
    string GetLakeTiticacaImage();
    string GetArequipaImage();
    string GetAmazonImage();
    string GetRainbowMountainImage();
}

public class DestinationImageService : IDestinationImageService
{
    // Curated high-resolution, verified travel photography of Peru
    private const string HeroMachuPicchu = "https://images.unsplash.com/photo-1526392060635-9d6019884377?q=80&w=1200&auto=format&fit=crop";
    private const string HeroCusco = "https://images.unsplash.com/photo-1589308078059-be1415eab4c3?q=80&w=1200&auto=format&fit=crop";
    private const string HeroSacredValley = "https://images.unsplash.com/photo-1580619305218-8423a7ef79b4?q=80&w=1200&auto=format&fit=crop";
    private const string HeroLakeTiticaca = "https://images.unsplash.com/photo-1587595431973-160d0d94add1?q=80&w=1200&auto=format&fit=crop";
    private const string HeroArequipa = "https://images.unsplash.com/photo-1531968455001-5c5272a41129?q=80&w=1200&auto=format&fit=crop";
    private const string HeroAmazon = "https://images.unsplash.com/photo-1516026672322-bc52d61a55d5?q=80&w=1200&auto=format&fit=crop";
    private const string HeroRainbowMountain = "https://images.unsplash.com/photo-1578328819058-b69f3a3b0f6b?q=80&w=1200&auto=format&fit=crop";
    private const string HeroLima = "https://images.unsplash.com/photo-1531968455001-5c5272a41129?q=80&w=1200&auto=format&fit=crop";

    // Activity & Travel Experience Photography
    private const string ImgTrain = "https://images.unsplash.com/photo-1544620347-c4fd4a3d5957?q=80&w=800&auto=format&fit=crop";
    private const string ImgHotel = "https://images.unsplash.com/photo-1566073771259-6a8506099945?q=80&w=800&auto=format&fit=crop";
    private const string ImgTransfer = "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?q=80&w=800&auto=format&fit=crop";
    private const string ImgTrek = "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?q=80&w=800&auto=format&fit=crop";
    private const string ImgFood = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?q=80&w=800&auto=format&fit=crop";

    public string GetDefaultPeruHeroImage() => HeroMachuPicchu;
    public string GetMachuPicchuImage() => HeroMachuPicchu;
    public string GetCuscoImage() => HeroCusco;
    public string GetSacredValleyImage() => HeroSacredValley;
    public string GetLakeTiticacaImage() => HeroLakeTiticaca;
    public string GetArequipaImage() => HeroArequipa;
    public string GetAmazonImage() => HeroAmazon;
    public string GetRainbowMountainImage() => HeroRainbowMountain;

    public string GetDestinationHeroImage(string? destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
            return HeroMachuPicchu;

        var d = destination.ToLowerInvariant();
        if (d.Contains("machu") || d.Contains("aguas"))
            return HeroMachuPicchu;
        if (d.Contains("cusco") || d.Contains("cuzco"))
            return HeroCusco;
        if (d.Contains("sacred") || d.Contains("urubamba") || d.Contains("ollantaytambo") || d.Contains("pisac"))
            return HeroSacredValley;
        if (d.Contains("titicaca") || d.Contains("puno") || d.Contains("uros"))
            return HeroLakeTiticaca;
        if (d.Contains("arequipa") || d.Contains("colca"))
            return HeroArequipa;
        if (d.Contains("amazon") || d.Contains("tambopata") || d.Contains("maldonado") || d.Contains("selva"))
            return HeroAmazon;
        if (d.Contains("rainbow") || d.Contains("vinicunca"))
            return HeroRainbowMountain;
        if (d.Contains("lima") || d.Contains("miraflores"))
            return HeroLima;

        return HeroMachuPicchu;
    }

    public string GetActivityImage(string? activityTitle, string? category = null)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.ToLowerInvariant();
            if (cat.Contains("hotel") || cat.Contains("lodging") || cat.Contains("check")) return ImgHotel;
            if (cat.Contains("transfer") || cat.Contains("pickup") || cat.Contains("car") || cat.Contains("flight")) return ImgTransfer;
            if (cat.Contains("train") || cat.Contains("rail")) return ImgTrain;
            if (cat.Contains("trek") || cat.Contains("hike") || cat.Contains("trail")) return ImgTrek;
            if (cat.Contains("meal") || cat.Contains("dinner") || cat.Contains("lunch") || cat.Contains("kosher")) return ImgFood;
        }

        if (string.IsNullOrWhiteSpace(activityTitle))
            return HeroCusco;

        var t = activityTitle.ToLowerInvariant();
        if (t.Contains("train") || t.Contains("perurail") || t.Contains("incarail")) return ImgTrain;
        if (t.Contains("hotel") || t.Contains("check-in") || t.Contains("resort")) return ImgHotel;
        if (t.Contains("transfer") || t.Contains("pickup") || t.Contains("airport") || t.Contains("drive")) return ImgTransfer;
        if (t.Contains("trek") || t.Contains("hike") || t.Contains("climb") || t.Contains("huayna")) return ImgTrek;
        if (t.Contains("dinner") || t.Contains("lunch") || t.Contains("kosher") || t.Contains("shabbat") || t.Contains("restaurant")) return ImgFood;
        if (t.Contains("machu")) return HeroMachuPicchu;
        if (t.Contains("sacred") || t.Contains("ollantaytambo")) return HeroSacredValley;
        if (t.Contains("rainbow") || t.Contains("vinicunca")) return HeroRainbowMountain;

        return HeroCusco;
    }

    public string GetTourImage(string? tourTitle)
    {
        return GetDestinationHeroImage(tourTitle);
    }
}

