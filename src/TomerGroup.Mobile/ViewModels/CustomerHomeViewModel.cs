using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public class FeaturedTrekItem
{
    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string Elevation { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string HebrewSummary { get; set; } = string.Empty;
    public string RouteName { get; set; } = string.Empty;

    public string DisplayTitle => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? HebrewTitle
        : Title;

    public string DisplaySummary => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? HebrewSummary
        : Summary;
}

public class FeaturedTourItem
{
    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HebrewDescription { get; set; } = string.Empty;

    public string DisplayTitle => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? HebrewTitle
        : Title;

    public string DisplayDescription => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? HebrewDescription
        : Description;
}

public class CuratedHotelItem
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string FeatureBadge { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HebrewDescription { get; set; } = string.Empty;

    public string DisplayDescription => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? HebrewDescription
        : Description;
}

public class AdventureActivityItem
{
    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HebrewDescription { get; set; } = string.Empty;

    public string DisplayTitle => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? HebrewTitle
        : Title;

    public string DisplayDescription => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? HebrewDescription
        : Description;
}

public partial class CustomerHomeViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly IDestinationImageService _imageService;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private string _customerGreeting = "ברוכים הבאים";

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _heroImageUrl = string.Empty;

    [ObservableProperty]
    private string _heroTitle = "המסע שלך בפרו מתחיל כאן";

    [ObservableProperty]
    private string _heroSubtitle = "חוויות טיול בוטיק מותאמות אישית, ליווי ישראלי צמוד ומסלולים מרהיבים";

    [ObservableProperty]
    private bool _hasActiveTrip;

    [ObservableProperty]
    private string _tripTitle = string.Empty;

    [ObservableProperty]
    private string _tripDates = string.Empty;

    [ObservableProperty]
    private string _tripDestinationsSummary = string.Empty;

    [ObservableProperty]
    private string _tripProgressText = string.Empty;

    [ObservableProperty]
    private double _tripProgressValue = 0.0;

    [ObservableProperty]
    private bool _hasNextActivity;

    [ObservableProperty]
    private string _nextActivityTime = string.Empty;

    [ObservableProperty]
    private string _nextActivityTitle = string.Empty;

    [ObservableProperty]
    private string _nextActivityLocation = string.Empty;

    [ObservableProperty]
    private string _nextActivityDetail = string.Empty;

    [ObservableProperty]
    private string _machuPicchuImage = string.Empty;

    [ObservableProperty]
    private string _cuscoImage = string.Empty;

    [ObservableProperty]
    private string _sacredValleyImage = string.Empty;

    [ObservableProperty]
    private string _lakeTiticacaImage = string.Empty;

    [ObservableProperty]
    private string _agencyContactPhone = "+51 984 231961";

    [ObservableProperty]
    private string _emergencyPhone = "+51 984 231961";

    public ObservableCollection<FeaturedTrekItem> FeaturedTreks { get; } = new();
    public ObservableCollection<FeaturedTourItem> FeaturedTours { get; } = new();
    public ObservableCollection<CuratedHotelItem> CuratedHotels { get; } = new();
    public ObservableCollection<AdventureActivityItem> AdventureActivities { get; } = new();

    public CustomerHomeViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IApiClient apiClient,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        _imageService = imageService;
        Title = Localize(LocalizationKeys.NavHome);

        HeroImageUrl = _imageService.GetMachuPicchuImage();
        MachuPicchuImage = _imageService.GetMachuPicchuImage();
        CuscoImage = _imageService.GetCuscoImage();
        SacredValleyImage = _imageService.GetSacredValleyImage();
        LakeTiticacaImage = _imageService.GetLakeTiticacaImage();

        InitializeCatalogCollections();
        UpdateLocalizedTexts();
    }

    private void InitializeCatalogCollections()
    {
        FeaturedTreks.Clear();
        FeaturedTreks.Add(new FeaturedTrekItem
        {
            Title = "Salkantay Trek 5D",
            HebrewTitle = "טרק סלקנטאי 5 ימים",
            Elevation = "4,630m",
            Duration = "5 Days / 4 Nights",
            Difficulty = "Challenging",
            ImageUrl = _imageService.GetMachuPicchuImage(),
            Summary = "National Geographic Top 25: Turquoise Humantay Lake, snowcapped peaks, cloud forest and Machu Picchu.",
            HebrewSummary = "אחד מ-25 הטרקים הטובים בעולם: אגם הומנטאי, מעבר קרחונים 4,630 מ', יער עננים ומאצ'ו פיצ'ו.",
            RouteName = "Salkantay Trek"
        });
        FeaturedTreks.Add(new FeaturedTrekItem
        {
            Title = "Classic Inca Trail 4D",
            HebrewTitle = "שביל האינקה הקלאסי 4 ימים",
            Elevation = "4,215m",
            Duration = "4 Days / 3 Nights",
            Difficulty = "Moderate",
            ImageUrl = _imageService.GetMachuPicchuImage(),
            Summary = "The legendary ancient highway: stone ruins, Dead Woman's Pass, cloud forest and walking through the Sun Gate.",
            HebrewSummary = "הנתיב המלכותי העתיק: עתיקות אינקה שמורות, מעבר אישה מתה, וכניסה רגלית משער השמש.",
            RouteName = "Inca Trail"
        });
        FeaturedTreks.Add(new FeaturedTrekItem
        {
            Title = "Ausangate & Rainbow Mountain",
            HebrewTitle = "טרק אוסנגטה והר הצבעים",
            Elevation = "5,200m",
            Duration = "4 Days / 3 Nights",
            Difficulty = "Challenging",
            ImageUrl = _imageService.GetRainbowMountainImage(),
            Summary = "Untamed Andes wilderness: 7 glacial turquoise lagoons, wild alpacas and Vinicunca sunrise.",
            HebrewSummary = "מסע פראי סביב הר אוסנגטה המקודש, לגונות טורקיז קרחוניות, עדרי אלפקות וזריחה בהר הצבעים.",
            RouteName = "Ausangate Trek"
        });
        FeaturedTreks.Add(new FeaturedTrekItem
        {
            Title = "Choquequirao Trek",
            HebrewTitle = "טרק צ'וקקיראו (עיר האחות האבודה)",
            Elevation = "3,050m",
            Duration = "4 Days / 3 Nights",
            Difficulty = "Demanding",
            ImageUrl = _imageService.GetCuscoImage(),
            Summary = "The sacred sister citadel of Machu Picchu perched dramatically over the deep Apurímac Canyon.",
            HebrewSummary = "המצודה הנסתרת והבודדה התלויה מעל קניון אפורימאק העמוק, ללא המוני תיירים.",
            RouteName = "Choquequirao"
        });

        FeaturedTours.Clear();
        FeaturedTours.Add(new FeaturedTourItem
        {
            Title = "Sacred Valley VIP Day Tour",
            HebrewTitle = "סיור VIP העמק הקדוש",
            Duration = "Full Day",
            Location = "Sacred Valley",
            ImageUrl = _imageService.GetSacredValleyImage(),
            Description = "Pisac artisan market & ruins, Ollantaytambo living Inca fortress, Moray terraces and Maras salt mines.",
            HebrewDescription = "שוק ועתיקות פיסאק, מבצר אויאנטייטמבו, טרסות הניסוי החקלאי במוראי ומכרות המלח המרהיבים במאראס."
        });
        FeaturedTours.Add(new FeaturedTourItem
        {
            Title = "Humantay Glacial Lake",
            HebrewTitle = "יום טיול לאגם הומנטאי",
            Duration = "Full Day",
            Location = "Mollepata / Soraypampa",
            ImageUrl = _imageService.GetMachuPicchuImage(),
            Description = "Hike to the breathtaking turquoise jewel nestled beneath the soaring Mount Humantay peak at 4,200m.",
            HebrewDescription = "עלייה רגלית לאגם הטורקיז הקרחוני המפורסם ביותר בפרו בגובה 4,200 מטר למרגלות ההר המושלג."
        });
        FeaturedTours.Add(new FeaturedTourItem
        {
            Title = "Cusco Historic City & Ruins",
            HebrewTitle = "סיור עיר היסטורית ואתרי קוסקו",
            Duration = "Half Day",
            Location = "Cusco",
            ImageUrl = _imageService.GetCuscoImage(),
            Description = "Plaza de Armas, Sun Temple (Qorikancha), and the megalithic stone fortress of Saqsaywaman.",
            HebrewDescription = "כיכר פלאזה דה ארמס, מקדש השמש קוריקנצ'ה, ומבצר האבן המגליתי המדהים סקסיואמן."
        });

        CuratedHotels.Clear();
        CuratedHotels.Add(new CuratedHotelItem
        {
            Name = "Belmond Palacio Nazarenas & Monasterio",
            Location = "Cusco Historic Center",
            FeatureBadge = "Oxygenated Suites",
            ImageUrl = _imageService.GetCuscoImage(),
            Description = "Former 16th-century convent and palace featuring enriched oxygen systems and world-class luxury.",
            HebrewDescription = "מנזר וארמון היסטורי מהמאה ה-16 עם מערכות העשרת חמצן בחדרים ושירות בוטיק ברמה עולמית."
        });
        CuratedHotels.Add(new CuratedHotelItem
        {
            Name = "Tambo del Inka Luxury Collection",
            Location = "Sacred Valley (Urubamba)",
            FeatureBadge = "Private Train Station",
            ImageUrl = _imageService.GetSacredValleyImage(),
            Description = "Riverside luxury resort in the lower Sacred Valley with private train departure directly to Machu Picchu.",
            HebrewDescription = "אתר נופש יוקרתי על גדות הנהר בעמק הקדוש הנמוך, עם רציף רכבת פרטי ישיר למאצ'ו פיצ'ו."
        });
        CuratedHotels.Add(new CuratedHotelItem
        {
            Name = "Inkaterra Machu Picchu Pueblo",
            Location = "Aguas Calientes",
            FeatureBadge = "Cloud Forest Luxury",
            ImageUrl = _imageService.GetMachuPicchuImage(),
            Description = "Exclusive Andean casitas hidden within lush cloud forest gardens, home to native orchids and bears.",
            HebrewDescription = "קסיטות בוטיק יוקרתיות השוכנות בלב יער העננים הטרופי למרגלות המאצ'ו פיצ'ו."
        });

        AdventureActivities.Clear();
        AdventureActivities.Add(new AdventureActivityItem
        {
            Title = "Sacred Valley Zipline & Via Ferrata",
            HebrewTitle = "אומגות ו-Via Ferrata בעמק הקדוש",
            Location = "Pachar, Sacred Valley",
            Duration = "Half Day",
            ImageUrl = _imageService.GetSacredValleyImage(),
            Description = "Climb the 400m rock face via safety cables and soar back down across 7 exhilarating canopy zip lines.",
            HebrewDescription = "טיפוס מצוקים מאובטח בכבלים וטיסה בין הרכסים על גבי 7 כבלי אומגה מהירים מעל נופי העמק."
        });
        AdventureActivities.Add(new AdventureActivityItem
        {
            Title = "ATV Quad Biking in Maras & Moray",
            HebrewTitle = "טרקטורוני ATV במאראס ומוראי",
            Location = "Maras Plateau",
            Duration = "Half Day",
            ImageUrl = _imageService.GetSacredValleyImage(),
            Description = "High-energy off-road quad biking expedition across the Andean plains with views of snow-capped peaks.",
            HebrewDescription = "רכיבת שטח אדרנלינית בטרקטורונים בנופי הרמות הגבוהות של האנדים אל עבר בריכות המלח ומוראי."
        });
        AdventureActivities.Add(new AdventureActivityItem
        {
            Title = "Urubamba River Rafting",
            HebrewTitle = "ראפטינג בנהר האורובמבה",
            Location = "Sacred Valley",
            Duration = "Full Day",
            ImageUrl = _imageService.GetSacredValleyImage(),
            Description = "Navigate Class III-IV mountain rapids through breathtaking gorges with professional safety kayakers.",
            HebrewDescription = "שיט אקסטרים במימי הנהר הקדוש בין קניונים מרשימים בליווי מדריכים מוסמכים וקיאקי הצלה."
        });
        AdventureActivities.Add(new AdventureActivityItem
        {
            Title = "Peruvian Culinary & Pisco Masterclass",
            HebrewTitle = "סדנת בישול פרואני ופיסקו סאוור",
            Location = "Cusco",
            Duration = "3 Hours",
            ImageUrl = _imageService.GetCuscoImage(),
            Description = "Market walk through San Pedro, Ceviche & Lomo Saltado preparation, and classic Pisco Sour mixing.",
            HebrewDescription = "סיור קולינרי בשוק סן פדרו, הכנת סביצ'ה טרי, לומו סלטאדו וערבוב הפיסקו סאוור המפורסם."
        });
    }

    // 9 Core Browse Category Labels
    public string CategoryTreksLabel => CurrentLanguage switch
    {
        "en" => "Treks",
        "es" => "Treks",
        _ => "טרקים"
    };

    public string CategoryDestinationsLabel => CurrentLanguage switch
    {
        "en" => "Destinations",
        "es" => "Destinos",
        _ => "יעדים"
    };

    public string CategoryToursLabel => CurrentLanguage switch
    {
        "en" => "Tours",
        "es" => "Tours",
        _ => "טיולי יום"
    };

    public string CategoryServicesLabel => CurrentLanguage switch
    {
        "en" => "Services",
        "es" => "Servicios",
        _ => "שירותים"
    };

    public string CategoryHotelsLabel => CurrentLanguage switch
    {
        "en" => "Hotels",
        "es" => "Hoteles",
        _ => "מלונות"
    };

    public string CategoryActivitiesLabel => CurrentLanguage switch
    {
        "en" => "Activities",
        "es" => "Actividades",
        _ => "פעילויות"
    };

    public string CategoryAboutLabel => CurrentLanguage switch
    {
        "en" => "About Us",
        "es" => "Nosotros",
        _ => "אודות"
    };

    public string CategorySupportLabel => CurrentLanguage switch
    {
        "en" => "Support",
        "es" => "Soporte",
        _ => "מוקד שירות"
    };

    public string CategoryMyTripLabel => CurrentLanguage switch
    {
        "en" => "My Trip",
        "es" => "Mi Viaje",
        _ => "הטיול שלי"
    };

    // Section Titles
    public string FeaturedTreksTitle => CurrentLanguage switch
    {
        "en" => "🏔️ Iconic Andean Treks",
        "es" => "🏔️ Treks Emblemáticos en los Andes",
        _ => "🏔️ טרקים נבחרים בהרי האנדים"
    };

    public string FeaturedToursTitle => CurrentLanguage switch
    {
        "en" => "🚌 Day Tours & Excursions",
        "es" => "🚌 Tours y Excursiones de un Día",
        _ => "🚌 טיולי יום וסיורים מודרכים"
    };

    public string CuratedHotelsTitle => CurrentLanguage switch
    {
        "en" => "🏨 Curated Andean Lodges & Hotels",
        "es" => "🏨 Hoteles y Lodges Exclusivos",
        _ => "🏨 מלונות ולודג'ים נבחרים"
    };

    public string AdventureActivitiesTitle => CurrentLanguage switch
    {
        "en" => "🧗 Adventure & Cultural Activities",
        "es" => "🧗 Actividades de Aventura y Cultura",
        _ => "🧗 פעילויות אקסטרים ותרבות"
    };

    public string ViewInteractiveMapText => CurrentLanguage switch
    {
        "en" => "Interactive Trek Map 🗺️",
        "es" => "Mapa de Trek Interactivo 🗺️",
        _ => "מפת מסלול אינטראקטיבית 🗺️"
    };

    public string DiscoverTreksButtonText => CurrentLanguage switch
    {
        "en" => "Explore Treks & Tours →",
        "es" => "Explorar Treks y Tours →",
        _ => "גלה טרקים וסיורים ←"
    };

    public string ContactWhatsAppButtonText => CurrentLanguage switch
    {
        "en" => "WhatsApp 24/7 💬",
        "es" => "WhatsApp 24/7 💬",
        _ => "וואטסאפ 24/7 💬"
    };

    public string ViewFullItineraryButtonText => CurrentLanguage switch
    {
        "en" => "View Full Itinerary",
        "es" => "Ver Itinerario Completo",
        _ => "לצפייה במסלול הטיול המלא"
    };

    public string NextActivityLabel => CurrentLanguage switch
    {
        "en" => "Your Next Activity",
        "es" => "Tu Próxima Actividad",
        _ => "הפעילות הבאה שלך"
    };

    public string TimeLabel => CurrentLanguage switch
    {
        "en" => "TIME",
        "es" => "HORA",
        _ => "שעה"
    };

    public string ActivityDetailsButtonText => CurrentLanguage switch
    {
        "en" => "Activity Details & Instructions",
        "es" => "Detalles e Instrucciones",
        _ => "פרטי פעילות והנחיות"
    };

    public string JourneyProgressLabel => CurrentLanguage switch
    {
        "en" => "Journey Progress",
        "es" => "Progreso del Viaje",
        _ => "התקדמות במסע"
    };

    public string FlagshipDestinationsTitle => CurrentLanguage switch
    {
        "en" => "Iconic Peru Destinations",
        "es" => "Destinos Emblemáticos de Perú",
        _ => "יעדי הדגל בפרו"
    };

    public string ViewAllButtonText => CurrentLanguage switch
    {
        "en" => "View All →",
        "es" => "Ver Todos →",
        _ => "צפה בהכל ←"
    };

    public string CuscoName => CurrentLanguage switch
    {
        "en" => "Cusco",
        "es" => "Cusco",
        _ => "קוסקו (Cusco)"
    };

    public string CuscoDescription => CurrentLanguage switch
    {
        "en" => "Capital of the Inca Empire, Plaza de Armas, traditional markets and Chabad House",
        "es" => "Capital del Imperio Inca, Plaza de Armas, mercados y Casa Chabad",
        _ => "בירת אימפריית האינקה, כיכר פלאזה דה ארמס, שווקים ובית חב\"ד"
    };

    public string WorldWonderBadge => CurrentLanguage switch
    {
        "en" => "WONDER OF THE WORLD",
        "es" => "MARAVILLA DEL MUNDO",
        _ => "פלא עולם"
    };

    public string MachuPicchuName => CurrentLanguage switch
    {
        "en" => "Machu Picchu",
        "es" => "Machu Picchu",
        _ => "מאצ'ו פיצ'ו (Machu Picchu)"
    };

    public string MachuPicchuDescription => CurrentLanguage switch
    {
        "en" => "The sacred citadel hidden in the cloud forest, panoramic Vistadome train & Huayna Picchu",
        "es" => "La ciudadela sagrada oculta en la niebla, tren panorámico y Huayna Picchu",
        _ => "מצודת האינקה הנסתרת בעננים, רכבות פנורמיות ווואינה פיצ'ו"
    };

    public string SacredValleyName => CurrentLanguage switch
    {
        "en" => "Sacred Valley",
        "es" => "Valle Sagrado",
        _ => "העמק הקדוש (Sacred Valley)"
    };

    public string SacredValleyDescription => CurrentLanguage switch
    {
        "en" => "Ollantaytambo fortress, Moray agricultural terraces, Maras salt mines & boutique haciendas",
        "es" => "Fortaleza de Ollantaytambo, andenes de Moray, salineras de Maras y haciendas",
        _ => "מבצר אויאנטייטמבו, טרסות מוראי, מכרות המלח במאראס ומלונות בוטיק"
    };

    public string LakeTiticacaName => CurrentLanguage switch
    {
        "en" => "Lake Titicaca",
        "es" => "Lago Titicaca",
        _ => "אגם טיטיקקה (Lake Titicaca)"
    };

    public string LakeTiticacaDescription => CurrentLanguage switch
    {
        "en" => "Floating Uros reed islands, ancient indigenous culture and breathtaking high-altitude sunsets",
        "es" => "Islas flotantes de los Uros, cultura ancestral y atardeceres en el lago más alto",
        _ => "האיים הצפים של האורוס, תרבות ילידית ושקיעות מרהיבות"
    };

    public string ServiceExperienceTitle => CurrentLanguage switch
    {
        "en" => "The Tomer Group Service Advantage",
        "es" => "La Ventaja de Servicio Tomer Group",
        _ => "חוויית השירות של Tomer Group"
    };

    public string AcclimatizationTitle => CurrentLanguage switch
    {
        "en" => "🫁 Altitude Acclimatization & Oxygen",
        "es" => "🫁 Aclimatación y Oxígeno Medicinal",
        _ => "🫁 התאקלמות לגובה וחמצן רפואי"
    };

    public string AcclimatizationDescription => CurrentLanguage switch
    {
        "en" => "Oxygen-enriched rooms in Cusco luxury hotels and gradual elevation-gain itinerary planning.",
        "es" => "Habitaciones con oxígeno en Cusco y diseño progresivo de itinerarios para evitar el soroche.",
        _ => "חדרים מועשרים בחמצן במלונות קוסקו ותכנון מסלול הדרגתי מותאם אישית."
    };

    public string KosherShabbatTitle => CurrentLanguage switch
    {
        "en" => "✡️ Kosher & Shabbat Hospitality",
        "es" => "✡️ Hospitalidad Kosher y Shabat",
        _ => "✡️ כשרות ושבת בפרו"
    };

    public string KosherShabbatDescription => CurrentLanguage switch
    {
        "en" => "Glatt kosher Shabbat meals coordinated with local Chabad centers in Cusco and Lima.",
        "es" => "Comidas de Shabat coordinadas con las sedes locales de Jabad en Cusco y Lima.",
        _ => "סיוע בתיאום ארוחות שבת כשרות בקוסקו ולימה עם בתי חב\"ד המקומיים."
    };

    public string PrivateTransportTitle => CurrentLanguage switch
    {
        "en" => "🚐 Private VIP Andean Transport",
        "es" => "🚐 Transporte VIP Privado",
        _ => "🚐 הסעות פרטיות VIP"
    };

    public string PrivateTransportDescription => CurrentLanguage switch
    {
        "en" => "Chauffeured Mercedes Sprinters with panoramic windows, oxygen on board, and mountain-certified drivers.",
        "es" => "Vehículos ejecutivos con ventanas panorámicas, oxígeno a bordo y conductores certificados en los Andes.",
        _ => "רכבי הסעה חדישים, נהגים מנוסים בהרי האנדים, חמצן ברכב ועצירות נוף גמישות."
    };

    public string MultilingualGuidesTitle => CurrentLanguage switch
    {
        "en" => "🗣️ Multilingual Expert Guides",
        "es" => "🗣️ Guías Expertos Multilingües",
        _ => "🗣️ הדרכה בעברית ובאנגלית"
    };

    public string MultilingualGuidesDescription => CurrentLanguage switch
    {
        "en" => "Official licensed guides fluent in English, Hebrew, and Spanish, steeped in Inca history and trail botany.",
        "es" => "Guías oficiales con dominio de inglés, hebreo y español, expertos en historia inca y senderismo.",
        _ => "מדריכי שטח מוסמכים דוברי עברית ואנגלית, מומחים בהיסטוריה של האינקה ובמסלולי הטרקים."
    };

    // About Us Section
    public string AboutUsTitle => CurrentLanguage switch
    {
        "en" => "About Tomer Group",
        "es" => "Acerca de Tomer Group",
        _ => "אודות Tomer Group"
    };

    public string AboutUsStory => CurrentLanguage switch
    {
        "en" => "Tomer Group is a premier boutique travel operator based directly in Cusco, Peru. For over a decade, we have created tailor-made Andean adventures, private luxury expeditions, and trekking journeys with 24/7 field control, medical safety protocols, and personalized care.",
        "es" => "Tomer Group es un operador boutique líder con base en Cusco, Perú. Durante más de una década hemos diseñado expediciones privadas, aventuras de lujo y trekkings con control logístico 24/7 y la máxima seguridad.",
        _ => "Tomer Group היא סוכנות בוטיק מובילה שמשרדיה פועלים ישירות בקוסקו, פרו. במשך למעלה מעשור אנו מתמחים בהפקת מסעות פרטיים, טיולי יוקרה וטרקים באנדים עם שליטה לוגיסטית 24/7, פרוטוקולי בטיחות מחמירים ויחס אישי לכל מטייל."
    };

    public string AboutBadgeCusco => CurrentLanguage switch
    {
        "en" => "📍 Cusco Headquarters",
        "es" => "📍 Sede Central en Cusco",
        _ => "📍 בסיס שטח בקוסקו"
    };

    public string AboutBadgeDispatch => CurrentLanguage switch
    {
        "en" => "🛡️ 24/7 Field Dispatch",
        "es" => "🛡️ Control de Campo 24/7",
        _ => "🛡️ מוקד שליטה 24/7"
    };

    public string AboutBadgeExperience => CurrentLanguage switch
    {
        "en" => "⭐ 10+ Years Experience",
        "es" => "⭐ +10 Años de Experiencia",
        _ => "⭐ מעל עשור ניסיון"
    };

    public string AboutBadgeSafety => CurrentLanguage switch
    {
        "en" => "🫁 Altitude Safety Protocol",
        "es" => "🫁 Seguridad en Altura",
        _ => "🫁 פרוטוקול בטיחות גובה"
    };

    // Contact & Support Section
    public string SupportHotlineTitle => CurrentLanguage switch
    {
        "en" => "24/7 Field Support in Peru",
        "es" => "Soporte en Perú 24/7",
        _ => "מוקד שירות וסיוע 24/7 בפרו"
    };

    public string SupportHotlineSubtitle => CurrentLanguage switch
    {
        "en" => "Direct line to our local Cusco operations team via WhatsApp or phone",
        "es" => "Línea directa con nuestro equipo de operaciones en Cusco por WhatsApp o teléfono",
        _ => "קשר ישיר לצוות התפעול שלנו בקוסקו בוואטסאפ ובטלפון"
    };

    public string WhatsAppActionLabel => CurrentLanguage switch
    {
        "en" => "Chat on WhatsApp",
        "es" => "Escribir al WhatsApp",
        _ => "שוחח בוואטסאפ"
    };

    public string CallOfficeActionLabel => CurrentLanguage switch
    {
        "en" => "Call Field Team",
        "es" => "Llamar al Equipo",
        _ => "חייג לצוות השטח"
    };

    private void UpdateLocalizedTexts()
    {
        AgencyContactPhone = LocalizationService.FormatPhoneNumber(Brand.ContactPhone);
        EmergencyPhone = LocalizationService.FormatPhoneNumber(Brand.EmergencyContact);

        OnPropertyChanged(nameof(CategoryTreksLabel));
        OnPropertyChanged(nameof(CategoryDestinationsLabel));
        OnPropertyChanged(nameof(CategoryToursLabel));
        OnPropertyChanged(nameof(CategoryServicesLabel));
        OnPropertyChanged(nameof(CategoryHotelsLabel));
        OnPropertyChanged(nameof(CategoryActivitiesLabel));
        OnPropertyChanged(nameof(CategoryAboutLabel));
        OnPropertyChanged(nameof(CategorySupportLabel));
        OnPropertyChanged(nameof(CategoryMyTripLabel));
        OnPropertyChanged(nameof(FeaturedTreksTitle));
        OnPropertyChanged(nameof(FeaturedToursTitle));
        OnPropertyChanged(nameof(CuratedHotelsTitle));
        OnPropertyChanged(nameof(AdventureActivitiesTitle));
        OnPropertyChanged(nameof(ViewInteractiveMapText));
        OnPropertyChanged(nameof(DiscoverTreksButtonText));
        OnPropertyChanged(nameof(ContactWhatsAppButtonText));
        OnPropertyChanged(nameof(ViewFullItineraryButtonText));
        OnPropertyChanged(nameof(NextActivityLabel));
        OnPropertyChanged(nameof(TimeLabel));
        OnPropertyChanged(nameof(ActivityDetailsButtonText));
        OnPropertyChanged(nameof(JourneyProgressLabel));
        OnPropertyChanged(nameof(FlagshipDestinationsTitle));
        OnPropertyChanged(nameof(ViewAllButtonText));
        OnPropertyChanged(nameof(CuscoName));
        OnPropertyChanged(nameof(CuscoDescription));
        OnPropertyChanged(nameof(WorldWonderBadge));
        OnPropertyChanged(nameof(MachuPicchuName));
        OnPropertyChanged(nameof(MachuPicchuDescription));
        OnPropertyChanged(nameof(SacredValleyName));
        OnPropertyChanged(nameof(SacredValleyDescription));
        OnPropertyChanged(nameof(LakeTiticacaName));
        OnPropertyChanged(nameof(LakeTiticacaDescription));
        OnPropertyChanged(nameof(ServiceExperienceTitle));
        OnPropertyChanged(nameof(AcclimatizationTitle));
        OnPropertyChanged(nameof(AcclimatizationDescription));
        OnPropertyChanged(nameof(KosherShabbatTitle));
        OnPropertyChanged(nameof(KosherShabbatDescription));
        OnPropertyChanged(nameof(PrivateTransportTitle));
        OnPropertyChanged(nameof(PrivateTransportDescription));
        OnPropertyChanged(nameof(MultilingualGuidesTitle));
        OnPropertyChanged(nameof(MultilingualGuidesDescription));
        OnPropertyChanged(nameof(AboutUsTitle));
        OnPropertyChanged(nameof(AboutUsStory));
        OnPropertyChanged(nameof(AboutBadgeCusco));
        OnPropertyChanged(nameof(AboutBadgeDispatch));
        OnPropertyChanged(nameof(AboutBadgeExperience));
        OnPropertyChanged(nameof(AboutBadgeSafety));
        OnPropertyChanged(nameof(SupportHotlineTitle));
        OnPropertyChanged(nameof(SupportHotlineSubtitle));
        OnPropertyChanged(nameof(WhatsAppActionLabel));
        OnPropertyChanged(nameof(CallOfficeActionLabel));
    }

    protected override void OnLanguageChanged()
    {
        Title = Localize(LocalizationKeys.NavHome);
        UpdateLocalizedTexts();
        InitializeCatalogCollections();

        if (!IsAuthenticated)
        {
            HeroTitle = CurrentLanguage switch
            {
                "en" => "Your Journey in Peru Begins Here",
                "es" => "Tu Viaje en Perú Comienza Aquí",
                _ => "המסע שלך בפרו מתחיל כאן"
            };
            HeroSubtitle = CurrentLanguage switch
            {
                "en" => "Bespoke itineraries, Machu Picchu permits, panoramic trains and luxury hotels",
                "es" => "Itinerarios a medida, permisos a Machu Picchu, trenes panorámicos y hoteles de lujo",
                _ => "טיולי בוטיק, אישורי כניסה למאצ'ו פיצ'ו, רכבות פנורמיות ומלונות יוקרה בהרי האנדים"
            };
        }
        else
        {
            CustomerGreeting = CurrentLanguage switch
            {
                "en" => string.IsNullOrWhiteSpace(CustomerName) ? "Hello 👋" : $"Hello, {CustomerName} 👋",
                "es" => string.IsNullOrWhiteSpace(CustomerName) ? "Hola 👋" : $"Hola, {CustomerName} 👋",
                _ => string.IsNullOrWhiteSpace(CustomerName) ? "שלום 👋" : $"שלום, {CustomerName} 👋"
            };
        }
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;
        IsAuthenticated = _apiClient.IsAuthenticated;

        try
        {
            if (IsAuthenticated)
            {
                var profileRes = await _apiClient.GetMyProfileAsync();
                if (profileRes.Success && profileRes.Data != null)
                {
                    var data = profileRes.Data;
                    var name = !string.IsNullOrWhiteSpace(data.HebrewName)
                        ? data.HebrewName
                        : (!string.IsNullOrWhiteSpace(data.FirstName) ? data.FirstName : string.Empty);

                    CustomerName = name;
                    CustomerGreeting = !string.IsNullOrWhiteSpace(name)
                        ? $"שלום, {name} 👋"
                        : "שלום 👋";
                }
                else
                {
                    CustomerGreeting = "שלום 👋";
                }

                var tripsRes = await _apiClient.GetCustomerTripsAsync(Guid.Empty);
                if (tripsRes.Success && tripsRes.Data != null && tripsRes.Data.Count > 0)
                {
                    var activeTrip = tripsRes.Data.FirstOrDefault();
                    if (activeTrip != null)
                    {
                        HasActiveTrip = true;
                        TripTitle = activeTrip.Title;
                        TripDates = $"{activeTrip.StartDate:dd.MM.yyyy} — {activeTrip.EndDate:dd.MM.yyyy}";
                        HeroImageUrl = _imageService.GetDestinationHeroImage(activeTrip.Title);

                        var totalDays = activeTrip.Days.Count;
                        if (totalDays > 0)
                        {
                            TripDestinationsSummary = string.Join(" · ", activeTrip.Days.Select(d => d.Destination).Distinct());
                            TripProgressText = $"יום 1 מתוך {totalDays}";
                            TripProgressValue = 1.0 / totalDays;

                            var firstDay = activeTrip.Days[0];
                            if (firstDay.Activities.Count > 0)
                            {
                                var firstAct = firstDay.Activities[0];
                                HasNextActivity = true;
                                NextActivityTime = $"{firstAct.StartTime:hh\\:mm}";
                                NextActivityTitle = firstAct.Title;
                                NextActivityLocation = firstAct.Location ?? "קוסקו";
                                NextActivityDetail = !string.IsNullOrWhiteSpace(firstAct.GuideName)
                                    ? $"מדריך: {firstAct.GuideName}"
                                    : "שירות מאושר ומסודר";
                            }
                        }
                    }
                    else
                    {
                        HasActiveTrip = false;
                        HasNextActivity = false;
                    }
                }
                else
                {
                    HasActiveTrip = false;
                    HasNextActivity = false;
                }
            }
            else
            {
                CustomerGreeting = "Tomer Group";
                CustomerName = string.Empty;
                HasActiveTrip = false;
                HasNextActivity = false;
                HeroImageUrl = _imageService.GetMachuPicchuImage();
                HeroTitle = CurrentLanguage switch
                {
                    "en" => "Your Journey in Peru Begins Here",
                    "es" => "Tu Viaje en Perú Comienza Aquí",
                    _ => "המסע שלך בפרו מתחיל כאן"
                };
                HeroSubtitle = CurrentLanguage switch
                {
                    "en" => "Bespoke itineraries, Machu Picchu permits, panoramic trains and luxury hotels",
                    "es" => "Itinerarios a medida, permisos a Machu Picchu, trenes panorámicos y hoteles de lujo",
                    _ => "טיולי בוטיק, אישורי כניסה למאצ'ו פיצ'ו, רכבות פנורמיות ומלונות יוקרה בהרי האנדים"
                };
            }
        }
        catch
        {
            HasActiveTrip = false;
            HasNextActivity = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OpenExploreAsync()
    {
        await Navigation.NavigateToAsync("//Explore");
    }

    [RelayCommand]
    public async Task OpenMyTripAsync()
    {
        await Navigation.NavigateToAsync("//MyTrip");
    }

    [RelayCommand]
    public async Task OpenTreksAsync()
    {
        await Navigation.NavigateToAsync("//Explore");
    }

    [RelayCommand]
    public async Task OpenToursAsync()
    {
        await Navigation.NavigateToAsync("//Explore");
    }

    [RelayCommand]
    public async Task OpenHotelsAsync()
    {
        await Navigation.NavigateToAsync("//Explore");
    }

    [RelayCommand]
    public async Task OpenActivitiesAsync()
    {
        await Navigation.NavigateToAsync("//Explore");
    }

    [RelayCommand]
    public async Task OpenTrekMapAsync(string? trekName)
    {
        var route = string.IsNullOrWhiteSpace(trekName)
            ? "TrekMap"
            : $"TrekMap?trekName={Uri.EscapeDataString(trekName)}";
        await Navigation.NavigateToAsync(route);
    }

    [RelayCommand]
    public async Task OpenWhatsAppAsync()
    {
#if USE_MAUI
        try
        {
            var phone = Brand.ContactPhone.Replace(" ", "").Replace("-", "").Replace("+", "");
            var uri = new Uri($"https://wa.me/{phone}?text={Uri.EscapeDataString("Hello Tomer Group, I would like information regarding travel and treks in Peru.")}");
            await Launcher.OpenAsync(uri);
        }
        catch
        {
            await Navigation.NavigateToAsync("More");
        }
#else
        await Task.CompletedTask;
#endif
    }

    [RelayCommand]
    public async Task OpenEmergencyCallAsync()
    {
#if USE_MAUI
        try
        {
            PhoneDialer.Open(EmergencyPhone);
        }
        catch
        {
            await Navigation.NavigateToAsync("More");
        }
#else
        await Task.CompletedTask;
#endif
    }

    [RelayCommand]
    public async Task OpenSupportAsync()
    {
        await Navigation.NavigateToAsync("More");
    }

    [RelayCommand]
    public void ChangeLanguage(string lang)
    {
        Localization.SetLanguage(lang);
        RefreshDirection();
        UpdateLocalizedTexts();
    }
}
