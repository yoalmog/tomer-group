using System.Collections.ObjectModel;
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
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string RecommendedDays { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new();
}

public class PeruTravelTip
{
    public string Icon { get; set; } = "🏔️";
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
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

    public ExploreViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _imageService = imageService;
        Title = "גלה את פרו";
        InitializeData();
    }

    private void InitializeData()
    {
        _allDestinations.Clear();
        _allDestinations.AddRange(new[]
        {
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
            Subtitle = "מומלץ לשהות ביום הראשון בעמק הקדוש הנמוך, לשתות חליטת קוקה ולנוח."
        });
        TravelTips.Add(new PeruTravelTip
        {
            Icon = "✡️",
            Title = "כשרות ושבת בפרו",
            Subtitle = "בתי חב\"ד פעילים בקוסקו ובלימה עם ארוחות שבת כשרות ואווירה ביתית."
        });
        TravelTips.Add(new PeruTravelTip
        {
            Icon = "🛂",
            Title = "דרכונים וכרטיסים",
            Subtitle = "למאצ'ו פיצ'ו ולרכבות יש להציג דרכון מקורי בלבד. כל האישורים נשמרים באפליקציה."
        });

        FilterDestinations("הכל");
    }

    [RelayCommand]
    public void FilterDestinations(string filter)
    {
        SelectedFilter = filter;
        Destinations.Clear();

        var filtered = filter == "הכל" 
            ? _allDestinations 
            : _allDestinations.Where(d => d.Region == filter);

        foreach (var dest in filtered)
        {
            Destinations.Add(dest);
        }
    }

    [RelayCommand]
    public async Task ContactTeamAsync()
    {
        await Navigation.NavigateToAsync("//More");
    }
}

