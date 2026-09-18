using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class DestinationCard
{
    public string Title { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Elevation { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string EnglishTagline { get; set; } = string.Empty;
    public string SpanishTagline { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EnglishDescription { get; set; } = string.Empty;
    public string SpanishDescription { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string RecommendedDays { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new();

    public string DisplayTitle => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? HebrewTitle
        : Title;

    public string DisplaySubtitle => (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant() == "he"
        ? Title
        : Region;

    public string DisplayTagline
    {
        get
        {
            var lang = (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant();
            if (lang == "en" && !string.IsNullOrWhiteSpace(EnglishTagline)) return EnglishTagline;
            if (lang == "es" && !string.IsNullOrWhiteSpace(SpanishTagline)) return SpanishTagline;
            return Tagline;
        }
    }

    public string DisplayDescription
    {
        get
        {
            var lang = (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant();
            if (lang == "en" && !string.IsNullOrWhiteSpace(EnglishDescription)) return EnglishDescription;
            if (lang == "es" && !string.IsNullOrWhiteSpace(SpanishDescription)) return SpanishDescription;
            return Description;
        }
    }
}

public class PeruTravelTip
{
    public string Icon { get; set; } = "🏔️";
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string HebrewTitle { get; set; } = string.Empty;
    public string EnglishTitle { get; set; } = string.Empty;
    public string SpanishTitle { get; set; } = string.Empty;
    public string HebrewSubtitle { get; set; } = string.Empty;
    public string EnglishSubtitle { get; set; } = string.Empty;
    public string SpanishSubtitle { get; set; } = string.Empty;

    public string DisplayTitle
    {
        get
        {
            var lang = (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant();
            if (lang == "en" && !string.IsNullOrWhiteSpace(EnglishTitle)) return EnglishTitle;
            if (lang == "es" && !string.IsNullOrWhiteSpace(SpanishTitle)) return SpanishTitle;
            if (!string.IsNullOrWhiteSpace(HebrewTitle)) return HebrewTitle;
            return Title;
        }
    }

    public string DisplaySubtitle
    {
        get
        {
            var lang = (TomerGroup.Core.Localization.LocalizationService.LanguageGetter?.Invoke("he") ?? "he").ToLowerInvariant();
            if (lang == "en" && !string.IsNullOrWhiteSpace(EnglishSubtitle)) return EnglishSubtitle;
            if (lang == "es" && !string.IsNullOrWhiteSpace(SpanishSubtitle)) return SpanishSubtitle;
            if (!string.IsNullOrWhiteSpace(HebrewSubtitle)) return HebrewSubtitle;
            return Subtitle;
        }
    }
}

public partial class ExploreViewModel : BaseViewModel
{
    private readonly IDestinationImageService _imageService;
    private readonly List<DestinationCard> _allDestinations = new();

    [ObservableProperty]
    private string _selectedFilter = "הכל";

    [ObservableProperty]
    private ObservableCollection<DestinationCard> _destinations = new();

    [ObservableProperty]
    private ObservableCollection<PeruTravelTip> _travelTips = new();

    public string PageHeaderTitle => CurrentLanguage switch
    {
        "en" => "Discover Peru",
        "es" => "Descubre el Perú",
        _ => "גלה את פרו"
    };

    public string PageHeaderSubtitle => CurrentLanguage switch
    {
        "en" => "Destinations, treks, and unforgettable experiences",
        "es" => "Destinos, treks y experiencias inolvidables",
        _ => "יעדים, טרקים וחוויות בלתי נשכחות"
    };

    public string FilterAllText => CurrentLanguage switch
    {
        "en" => "All",
        "es" => "Todos",
        _ => "הכל"
    };

    public string FilterTreksText => CurrentLanguage switch
    {
        "en" => "Andean Treks 🥾",
        "es" => "Treks Andinos 🥾",
        _ => "טרקים באנדים 🥾"
    };

    public string FilterCuscoText => CurrentLanguage switch
    {
        "en" => "Cusco & Sacred Valley",
        "es" => "Cusco y Valle Sagrado",
        _ => "קוסקו והעמק הקדוש"
    };

    public string FilterMachuText => CurrentLanguage switch
    {
        "en" => "Machu Picchu",
        "es" => "Machu Picchu",
        _ => "מאצ'ו פיצ'ו"
    };

    public string FilterAndesText => CurrentLanguage switch
    {
        "en" => "Andes Mountains",
        "es" => "Cordillera de los Andes",
        _ => "הרי האנדים"
    };

    public string FilterAmazonText => CurrentLanguage switch
    {
        "en" => "Amazon Rainforest",
        "es" => "Selva Amazónica",
        _ => "יער האמזונס"
    };

    public string HighlightsLabel => CurrentLanguage switch
    {
        "en" => "✨ Key Highlights",
        "es" => "✨ Experiencias Clave",
        _ => "✨ חוויות מרכזיות"
    };

    public string TrekMapButtonText => CurrentLanguage switch
    {
        "en" => "Interactive Trek Map 🗺️",
        "es" => "Mapa de Trek Interactivo 🗺️",
        _ => "צפה במפת מסלול אינטראקטיבית 🗺️"
    };

    public string TipsSectionTitle => CurrentLanguage switch
    {
        "en" => "Essential Peru Travel Tips",
        "es" => "Consejos Esenciales para Viajar a Perú",
        _ => "טיפים חיוניים למטייל בפרו"
    };

    public string ContactCardTitle => CurrentLanguage switch
    {
        "en" => "Planning Your Journey to Peru?",
        "es" => "¿Planeando su Viaje a Perú?",
        _ => "מתכננים את הטיול שלכם לפרו?"
    };

    public string ContactCardDescription => CurrentLanguage switch
    {
        "en" => "Tomer Group destination specialists will design a custom itinerary with hotels, transfers and private guides.",
        "es" => "Los especialistas de Tomer Group diseñarán un itinerario personalizado con hoteles, traslados y guías.",
        _ => "מומחי היעד של Tomer Group ירכיבו עבורכם מסלול אישי מותאם במדויק כולל מלונות, העברות והדרכה בעברית."
    };

    public string ContactCardButtonText => CurrentLanguage switch
    {
        "en" => "Talk with Our Specialists 💬",
        "es" => "Hablar con Especialistas 💬",
        _ => "דברו עם צוות המומחים שלנו 💬"
    };

    public ExploreViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _imageService = imageService;
        Title = CurrentLanguage switch
        {
            "en" => "Explore Peru",
            "es" => "Explorar Perú",
            _ => "גלה את פרו"
        };
        InitializeData();
    }

    protected override void OnLanguageChanged()
    {
        Title = CurrentLanguage switch
        {
            "en" => "Explore Peru",
            "es" => "Explorar Perú",
            _ => "גלה את פרו"
        };
        InitializeData();
        OnPropertyChanged(nameof(PageHeaderTitle));
        OnPropertyChanged(nameof(PageHeaderSubtitle));
        OnPropertyChanged(nameof(FilterAllText));
        OnPropertyChanged(nameof(FilterTreksText));
        OnPropertyChanged(nameof(FilterCuscoText));
        OnPropertyChanged(nameof(FilterMachuText));
        OnPropertyChanged(nameof(FilterAndesText));
        OnPropertyChanged(nameof(FilterAmazonText));
        OnPropertyChanged(nameof(HighlightsLabel));
        OnPropertyChanged(nameof(TrekMapButtonText));
        OnPropertyChanged(nameof(TipsSectionTitle));
        OnPropertyChanged(nameof(ContactCardTitle));
        OnPropertyChanged(nameof(ContactCardDescription));
        OnPropertyChanged(nameof(ContactCardButtonText));
    }

    private void InitializeData()
    {
        _allDestinations.Clear();
        _allDestinations.AddRange(new[]
        {
            new DestinationCard
            {
                Title = "Salkantay Trek",
                HebrewTitle = "טרק סלקנטאי (5 ימים למאצ'ו פיצ'ו)",
                Region = "טרקים",
                Elevation = "4,630 מטר (מעבר סלקנטאי)",
                Tagline = "הטרק המרהיב בעולם אל פלא תבל",
                Description = "אחד מ-25 הטרקים הטובים בעולם לפי נשיונל ג'יאוגרפיק: קרחונים נישאים, אגם הומנטאי בצבע טורקיז וג'ונגל שופע המוביל למאצ'ו פיצ'ו.",
                ImageUrl = _imageService.GetMachuPicchuImage(),
                RecommendedDays = "4–5 ימים",
                Highlights = new() { "אגם הומנטאי", "מעבר סלקנטאי 4,630 מ'", "הוט טאבס בקוקאלמאיו" }
            },
            new DestinationCard
            {
                Title = "Inca Jungle Trek",
                HebrewTitle = "אינקה ג'אנגל (טרק אקסטרים למאצ'ו פיצ'ו)",
                Region = "טרקים",
                Elevation = "4,350 מטר עד 1,500 מטר",
                Tagline = "אופני הרים, ראפטינג, אומגות והליכה באינקה",
                Description = "שילוב מסעיר של אקסטרים ותרבות: רכיבת דאונהיל מאוכף האנדים לתוך יער הגשם, רפטינג במים סוערים, ומעיינות חמים.",
                ImageUrl = _imageService.GetSacredValleyImage(),
                RecommendedDays = "3–4 ימים",
                Highlights = new() { "רכיבת דאונהיל", "ראפטינג", "אומגות מעל הקניון" }
            },
            new DestinationCard
            {
                Title = "Ausangate & Rainbow Mountain Trek",
                HebrewTitle = "טרק אוסנגטה והר הצבעים",
                Region = "טרקים",
                Elevation = "5,200 מטר",
                Tagline = "מסע פראי בין לגונות קרחוניות והרים צבעוניים",
                Description = "טרק אנדים אותנטי סביב הר אוסנגטה המקודש, חציית לגונות טורקיז עוצרות נשימה, עדרי אלפקות ותצפית זריחה מרהיבה על ויניקונקה.",
                ImageUrl = _imageService.GetRainbowMountainImage(),
                RecommendedDays = "3–4 ימים",
                Highlights = new() { "שבעת הלגונות", "מעברי הרים בגובה 5,000 מ'", "זריחה בהר הצבעים" }
            },
            new DestinationCard
            {
                Title = "Machu Picchu",
                HebrewTitle = "מאצ'ו פיצ'ו",
                Region = "מאצ'ו פיצ'ו",
                Elevation = "2,430 מטר",
                Tagline = "פלא עולם נסתר בין ענני האנדים",
                Description = "המצודה האינקאית המסתורית, חוויית זריחה עוצרת נשימה, ומסלולי טיפוס מרהיבים לוואינה פיצ'ו.",
                ImageUrl = _imageService.GetMachuPicchuImage(),
                RecommendedDays = "1–2 ימים",
                Highlights = new() { "זריחה במצודה", "רכבת Vistadome", "וואינה פיצ'ו" }
            },
            new DestinationCard
            {
                Title = "Cusco",
                HebrewTitle = "קוסקו",
                Region = "קוסקו והעמק הקדוש",
                Elevation = "3,400 מטר",
                Tagline = "בירת אימפריית האינקה והלב התרבותי",
                Description = "סמטאות אבן היסטוריות, כיכר פלאזה דה ארמס, שוק סן פדרו, ומרכז חב\"ד קוסקו המארח מטיילים ישראלים.",
                ImageUrl = _imageService.GetCuscoImage(),
                RecommendedDays = "3–4 ימים",
                Highlights = new() { "פלאזה דה ארמס", "שוק סן פדרו", "בית חב\"ד" }
            },
            new DestinationCard
            {
                Title = "Sacred Valley",
                HebrewTitle = "העמק הקדוש",
                Region = "קוסקו והעמק הקדוש",
                Elevation = "2,870 מטר",
                Tagline = "טרסות אינקה חקלאיות וטבע מרהיב",
                Description = "עמק פסטורלי מושלם להתרגלות לגובה, מבצר אויאנטייטמבו העתיק, ומכרות המלח המרשימים במאראס.",
                ImageUrl = _imageService.GetSacredValleyImage(),
                RecommendedDays = "2–3 ימים",
                Highlights = new() { "אויאנטייטמבו", "מכרות מאראס", "טרסות מוראי" }
            },
            new DestinationCard
            {
                Title = "Rainbow Mountain",
                HebrewTitle = "הר שבעת הצבעים (ויניקונקה)",
                Region = "הרי האנדים",
                Elevation = "5,200 מטר",
                Tagline = "רכסי מינרלים מרהיבים בגובה רב",
                Description = "טרק אל פסגת האנדים המוזהבת והצבעונית, מראה גיאולוגי ייחודי בעולם הדורש התאקלמות טובה.",
                ImageUrl = _imageService.GetRainbowMountainImage(),
                RecommendedDays = "יום טיול",
                Highlights = new() { "רכסי ויניקונקה", "תצפית קרחונים", "נופי אלפקות" }
            },
            new DestinationCard
            {
                Title = "Lake Titicaca",
                HebrewTitle = "אגם טיטיקקה",
                Region = "הרי האנדים",
                Elevation = "3,812 מטר",
                Tagline = "האגם השייט הגבוה בעולם",
                Description = "איי האורוס הצפים העשויים מקני טוטורה, תרבות ילידית עתיקה באיי טקילה ומי טורקיז עמוקים.",
                ImageUrl = _imageService.GetLakeTiticacaImage(),
                RecommendedDays = "2 ימים",
                Highlights = new() { "האיים הצפים", "האי טקילה", "שקיעות באנדים" }
            },
            new DestinationCard
            {
                Title = "Arequipa & Colca Canyon",
                HebrewTitle = "ארקיפה וקניון קולקה",
                Region = "הרי האנדים",
                Elevation = "2,325 מטר",
                Tagline = "העיר הלבנה ומעוף הקונדורים",
                Description = "ארכיטקטורה קולוניאלית מאבן וולקנית לבנה, והקניון העמוק בעולם שמעליו דואים קונדורים ענקיים.",
                ImageUrl = _imageService.GetArequipaImage(),
                RecommendedDays = "2–3 ימים",
                Highlights = new() { "מנזר סנטה קטלינה", "קניון קולקה", "קונדורים ענקיים" }
            },
            new DestinationCard
            {
                Title = "Amazon Rainforest",
                HebrewTitle = "יער האמזונס (טמבופטה)",
                Region = "יער האמזונס",
                Elevation = "200 מטר",
                Tagline = "ממלכת המגוון הביולוגי של כדור הארץ",
                Description = "לודג'ים אקולוגיים, שייט נהרות לילי בעקבות קיימנים, וצפייה בתוכים ססגוניים בלב הג'ונגל.",
                ImageUrl = _imageService.GetAmazonImage(),
                RecommendedDays = "3–4 ימים",
                Highlights = new() { "שייט נהרות", "קיימנים וקופים", "לודג'ים בטבע" }
            }
        });

        TravelTips.Clear();
        TravelTips.Add(new PeruTravelTip
        {
            Icon = "🫁",
            Title = "התרגלות לגובה (Soroche)",
            Subtitle = "מומלץ לשהות ביום הראשון בעמק הקדוש הנמוך, לשתות חליטת קוקה ולנוח.",
            HebrewTitle = "התרגלות לגובה (Soroche)",
            EnglishTitle = "Altitude Acclimatization (Soroche)",
            SpanishTitle = "Aclimatación a la Altura (Soroche)",
            HebrewSubtitle = "מומלץ לשהות ביום הראשון בעמק הקדוש הנמוך, לשתות חליטת קוקה ולנוח.",
            EnglishSubtitle = "Spend your first night resting in the lower Sacred Valley, drink coca tea, and hydrate generously.",
            SpanishSubtitle = "Pase su primera noche descansando en el Valle Sagrado, tome mate de coca e hidrátese bien."
        });
        TravelTips.Add(new PeruTravelTip
        {
            Icon = "✡️",
            Title = "כשרות ושבת בפרו",
            Subtitle = "בתי חב\"ד פעילים בקוסקו ובלימה עם ארוחות שבת כשרות ואווירה ביתית.",
            HebrewTitle = "כשרות ושבת בפרו",
            EnglishTitle = "Kosher & Shabbat in Peru",
            SpanishTitle = "Comida Kosher y Shabat en Perú",
            HebrewSubtitle = "בתי חב\"ד פעילים בקוסקו ובלימה עם ארוחות שבת כשרות ואווירה ביתית.",
            EnglishSubtitle = "Chabad Houses in Cusco and Lima provide certified kosher meals and welcoming Shabbat hospitality.",
            SpanishSubtitle = "Las Casas de Jabad en Cusco y Lima brindan comidas kosher y hospitalidad para Shabat."
        });
        TravelTips.Add(new PeruTravelTip
        {
            Icon = "🛂",
            Title = "דרכונים וכרטיסים",
            Subtitle = "למאצ'ו פיצ'ו ולרכבות יש להציג דרכון מקורי בלבד. כל האישורים נשמרים באפליקציה.",
            HebrewTitle = "דרכונים וכרטיסים",
            EnglishTitle = "Original Passports Required",
            SpanishTitle = "Pasaportes Originales Obligatorios",
            HebrewSubtitle = "למאצ'ו פיצ'ו ולרכבות יש להציג דרכון מקורי בלבד. כל האישורים נשמרים באפליקציה.",
            EnglishSubtitle = "Physical original passports must be presented at train stations and the Machu Picchu entrance.",
            SpanishSubtitle = "Se deben presentar pasaportes físicos originales en las estaciones de tren y en el ingreso a Machu Picchu."
        });

        FilterDestinations("הכל");
    }

    public string AllFilterBg => SelectedFilter == "הכל" ? "#BC225E" : "#FFFFFF";
    public string AllFilterText => SelectedFilter == "הכל" ? "#FFFFFF" : "#334155";

    public string TreksFilterBg => (SelectedFilter == "טרקים" || SelectedFilter == "Treks") ? "#BC225E" : "#FFFFFF";
    public string TreksFilterText => (SelectedFilter == "טרקים" || SelectedFilter == "Treks") ? "#FFFFFF" : "#334155";

    public string CuscoFilterBg => SelectedFilter == "קוסקו והעמק הקדוש" ? "#BC225E" : "#FFFFFF";
    public string CuscoFilterText => SelectedFilter == "קוסקו והעמק הקדוש" ? "#FFFFFF" : "#334155";

    public string MachuFilterBg => SelectedFilter == "מאצ'ו פיצ'ו" ? "#BC225E" : "#FFFFFF";
    public string MachuFilterText => SelectedFilter == "מאצ'ו פיצ'ו" ? "#FFFFFF" : "#334155";

    public string AndesFilterBg => SelectedFilter == "הרי האנדים" ? "#BC225E" : "#FFFFFF";
    public string AndesFilterText => SelectedFilter == "הרי האנדים" ? "#FFFFFF" : "#334155";

    public string AmazonFilterBg => SelectedFilter == "יער האמזונס" ? "#BC225E" : "#FFFFFF";
    public string AmazonFilterText => SelectedFilter == "יער האמזונס" ? "#FFFFFF" : "#334155";

    [RelayCommand]
    public void FilterDestinations(string filter)
    {
        SelectedFilter = filter;
        Destinations.Clear();

        var filtered = filter switch
        {
            "הכל" => _allDestinations.AsEnumerable(),
            "טרקים" or "Treks" => _allDestinations.Where(d => d.Region == "טרקים" || d.Title.Contains("Trek") || d.HebrewTitle.Contains("טרק")),
            _ => _allDestinations.Where(d => d.Region == filter)
        };

        foreach (var dest in filtered)
        {
            Destinations.Add(dest);
        }

        OnPropertyChanged(nameof(AllFilterBg));
        OnPropertyChanged(nameof(AllFilterText));
        OnPropertyChanged(nameof(TreksFilterBg));
        OnPropertyChanged(nameof(TreksFilterText));
        OnPropertyChanged(nameof(CuscoFilterBg));
        OnPropertyChanged(nameof(CuscoFilterText));
        OnPropertyChanged(nameof(MachuFilterBg));
        OnPropertyChanged(nameof(MachuFilterText));
        OnPropertyChanged(nameof(AndesFilterBg));
        OnPropertyChanged(nameof(AndesFilterText));
        OnPropertyChanged(nameof(AmazonFilterBg));
        OnPropertyChanged(nameof(AmazonFilterText));
    }

    [RelayCommand]
    public async Task ContactTeamAsync()
    {
        await Navigation.NavigateToAsync("More");
    }

    [RelayCommand]
    public async Task OpenTrekMapAsync(string? trekName = null)
    {
        var route = string.IsNullOrWhiteSpace(trekName)
            ? "TrekMap"
            : $"TrekMap?trekName={Uri.EscapeDataString(trekName)}";
        await Navigation.NavigateToAsync(route);
    }
}
