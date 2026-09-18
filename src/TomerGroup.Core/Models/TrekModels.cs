using System;
using System.Collections.Generic;
using System.Linq;

namespace TomerGroup.Core.Models;

public enum WaypointType
{
    Start,
    Finish,
    Campsite,
    MountainPass,
    Viewpoint,
    AncientRuins,
    EmergencyShelter,
    Town
}

public class TrekCoordinate
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ElevationMeters { get; set; }
    public double DistanceFromStartKm { get; set; }

    public TrekCoordinate() { }

    public TrekCoordinate(double lat, double lng, double elevation, double distanceKm = 0)
    {
        Latitude = lat;
        Longitude = lng;
        ElevationMeters = elevation;
        DistanceFromStartKm = distanceKm;
    }
}

public class TrekWaypoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ElevationMeters { get; set; }
    public WaypointType Type { get; set; } = WaypointType.Viewpoint;
    public string Description { get; set; } = string.Empty;
    public string HebrewDescription { get; set; } = string.Empty;
    public int DayNumber { get; set; } = 1;
}

public class ElevationPoint
{
    public double DistanceKm { get; set; }
    public double ElevationMeters { get; set; }
    public string Label { get; set; } = string.Empty;

    public ElevationPoint() { }

    public ElevationPoint(double distanceKm, double elevationMeters, string label = "")
    {
        DistanceKm = distanceKm;
        ElevationMeters = elevationMeters;
        Label = label;
    }
}

public class TrekRoute
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string Region { get; set; } = "Cusco";
    public double TotalDistanceKm { get; set; }
    public double ElevationGainMeters { get; set; }
    public double MaxElevationMeters { get; set; }
    public double MinElevationMeters { get; set; }
    public string Difficulty { get; set; } = "Challenging";
    public int EstimatedDurationDays { get; set; } = 5;
    public string Summary { get; set; } = string.Empty;
    public string HebrewSummary { get; set; } = string.Empty;
    public List<TrekCoordinate> Coordinates { get; set; } = new();
    public List<TrekWaypoint> Waypoints { get; set; } = new();
    public List<ElevationPoint> ElevationProfile { get; set; } = new();
}

public static class TrekCatalog
{
    private static readonly List<TrekRoute> _routes = new();

    static TrekCatalog()
    {
        _routes.Add(CreateSalkantayTrek());
        _routes.Add(CreateClassicIncaTrail());
        _routes.Add(CreateRainbowMountain());
        _routes.Add(CreateAusangateCircuit());
    }

    public static IReadOnlyList<TrekRoute> GetFlagshipTreks() => _routes;

    public static TrekRoute? GetRouteById(Guid id) => _routes.FirstOrDefault(r => r.Id == id);

    public static TrekRoute? GetRouteByName(string name) =>
        _routes.FirstOrDefault(r => r.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                                   r.HebrewName.Contains(name, StringComparison.OrdinalIgnoreCase));

    private static TrekRoute CreateSalkantayTrek()
    {
        var route = new TrekRoute
        {
            Id = Guid.Parse("aa11bb22-33cc-44dd-55ee-66ff77aa8801"),
            Name = "Salkantay Trek to Machu Picchu",
            HebrewName = "טרק סלקנטאי למאצ'ו פיצ'ו (5 ימים)",
            Region = "Cusco / Salkantay",
            TotalDistanceKm = 74.0,
            ElevationGainMeters = 2850,
            MaxElevationMeters = 4630,
            MinElevationMeters = 2040,
            Difficulty = "Challenging",
            EstimatedDurationDays = 5,
            Summary = "One of National Geographic's top treks in the world. Traverse turquoise alpine lakes, the dramatic 4,630m Salkantay Pass, cloud forests and arrive at Machu Picchu.",
            HebrewSummary = "אחד הטרקים היפים בעולם לפי נשיונל ג'יאוגרפיק. מסע מרהיב בין לגונת הומנטאי, מעבר סלקנטאי בגובה 4,630 מטר, יער גשם ומאצ'ו פיצ'ו."
        };

        // Key Waypoints
        route.Waypoints = new List<TrekWaypoint>
        {
            new TrekWaypoint
            {
                Name = "Mollepata (Trailhead)",
                HebrewName = "מולפטה (נקודת יציאה)",
                Latitude = -13.5606,
                Longitude = -72.5667,
                ElevationMeters = 2900,
                Type = WaypointType.Start,
                Description = "Departure town where mules and gear are arranged.",
                HebrewDescription = "עיירת היציאה לטרק, התארגנות עם הפרדות והציוד.",
                DayNumber = 1
            },
            new TrekWaypoint
            {
                Name = "Soraypampa Camp & Humantay Lake",
                HebrewName = "מחנה סורייפמפה ולגונת הומנטאי",
                Latitude = -13.3892,
                Longitude = -72.5714,
                ElevationMeters = 3900,
                Type = WaypointType.Campsite,
                Description = "First night camp beneath Mount Salkantay with side trek to turquoise Lake Humantay (4,200m).",
                HebrewDescription = "מחנה הלילה הראשון למרגלות הר סלקנטאי עם עלייה ללגונת הומנטאי הטורקיזית (4,200 מ').",
                DayNumber = 1
            },
            new TrekWaypoint
            {
                Name = "Salkantay Pass (Abra Salkantay)",
                HebrewName = "מעבר סלקנטאי (4,630 מ')",
                Latitude = -13.3340,
                Longitude = -72.5456,
                ElevationMeters = 4630,
                Type = WaypointType.MountainPass,
                Description = "The highest summit of the trek flanked by snow-capped glacial peaks.",
                HebrewDescription = "הנקודה הגבוהה ביותר בטרק בין קרחוני העד והפסגות המושלגות של האנדים.",
                DayNumber = 2
            },
            new TrekWaypoint
            {
                Name = "Chaullay Campsite",
                HebrewName = "מחנה צ'אולאי",
                Latitude = -13.2980,
                Longitude = -72.5840,
                ElevationMeters = 2900,
                Type = WaypointType.Campsite,
                Description = "Descent into the lush high jungle (Ceja de Selva).",
                HebrewDescription = "ירידה אל תוך יער העננים והצמחייה הטרופית של האנדים.",
                DayNumber = 2
            },
            new TrekWaypoint
            {
                Name = "Lucmabamba (Coffee & Organic Farms)",
                HebrewName = "לוקמאבמבה (חוות קפה אורגניות)",
                Latitude = -13.2185,
                Longitude = -72.6105,
                ElevationMeters = 2050,
                Type = WaypointType.Campsite,
                Description = "Organic local coffee roasting and tropical fruit orchards.",
                HebrewDescription = "אירוח בחוות קפה מקומיות, קליית פולי קפה ופירות טרופיים.",
                DayNumber = 3
            },
            new TrekWaypoint
            {
                Name = "Llactapata Ruins Viewpoint",
                HebrewName = "מצודת לקטפאטה (תצפית למאצ'ו פיצ'ו)",
                Latitude = -13.1950,
                Longitude = -72.5880,
                ElevationMeters = 2730,
                Type = WaypointType.Viewpoint,
                Description = "Ancient Inca site with direct rare frontal view across the canyon to Machu Picchu.",
                HebrewDescription = "שרידי אינקה המשקיפים ישירות ממעבר לקניון על מצודת מאצ'ו פיצ'ו.",
                DayNumber = 4
            },
            new TrekWaypoint
            {
                Name = "Aguas Calientes (Machu Picchu Pueblo)",
                HebrewName = "אגואס קליינטס (כפר מאצ'ו פיצ'ו)",
                Latitude = -13.1550,
                Longitude = -72.5255,
                ElevationMeters = 2040,
                Type = WaypointType.Town,
                Description = "Town nestled in the canyon below the citadel with thermal springs.",
                HebrewDescription = "העיירה בבסיס המצודה, מלונות נוחים ומעיינות חמים.",
                DayNumber = 4
            },
            new TrekWaypoint
            {
                Name = "Machu Picchu Sanctuary",
                HebrewName = "מצודת מאצ'ו פיצ'ו (סיום הטרק)",
                Latitude = -13.1631,
                Longitude = -72.5450,
                ElevationMeters = 2430,
                Type = WaypointType.Finish,
                Description = "The wonder of the world: grand guided exploration of the Inca Citadel.",
                HebrewDescription = "פלא העולם: סיור מודרך מקיף במצודת האינקה האגדית.",
                DayNumber = 5
            }
        };

        // Coordinates Polyline
        route.Coordinates = new List<TrekCoordinate>
        {
            new(-13.5606, -72.5667, 2900, 0),
            new(-13.5000, -72.5700, 3200, 6.5),
            new(-13.4400, -72.5730, 3550, 13.0),
            new(-13.3892, -72.5714, 3900, 19.0),
            new(-13.3750, -72.5600, 4200, 22.5),
            new(-13.3340, -72.5456, 4630, 28.0),
            new(-13.3100, -72.5650, 3800, 34.0),
            new(-13.2980, -72.5840, 2900, 41.0),
            new(-13.2600, -72.6000, 2450, 49.0),
            new(-13.2185, -72.6105, 2050, 56.0),
            new(-13.1950, -72.5880, 2730, 62.0),
            new(-13.1800, -72.5500, 2100, 66.5),
            new(-13.1550, -72.5255, 2040, 71.0),
            new(-13.1631, -72.5450, 2430, 74.0)
        };

        // Elevation Profile
        route.ElevationProfile = new List<ElevationPoint>
        {
            new(0, 2900, "Mollepata"),
            new(19, 3900, "Soraypampa"),
            new(28, 4630, "Salkantay Pass"),
            new(41, 2900, "Chaullay"),
            new(56, 2050, "Lucmabamba"),
            new(62, 2730, "Llactapata"),
            new(71, 2040, "Aguas Calientes"),
            new(74, 2430, "Machu Picchu")
        };

        return route;
    }

    private static TrekRoute CreateClassicIncaTrail()
    {
        var route = new TrekRoute
        {
            Id = Guid.Parse("bb22cc33-44dd-55ee-66ff-77aa88bb9902"),
            Name = "Classic Inca Trail 4D/3N",
            HebrewName = "שביל האינקה הקלאסי (4 ימים)",
            Region = "Cusco / Sacred Valley",
            TotalDistanceKm = 43.0,
            ElevationGainMeters = 2450,
            MaxElevationMeters = 4215,
            MinElevationMeters = 2400,
            Difficulty = "Moderate-Challenging",
            EstimatedDurationDays = 4,
            Summary = "The legendary ancient royal path constructed by the Incas. Cross high passes, cloud forests, ancient stairways, arriving at the Sun Gate (Inti Punku).",
            HebrewSummary = "השביל המלכותי ההיסטורי של אימפריית האינקה. חציית שער האישה המתה, מדרגות אבן בנות 500 שנה וכניסה למאצ'ו פיצ'ו דרך שער השמש."
        };

        route.Waypoints = new List<TrekWaypoint>
        {
            new TrekWaypoint { Name = "Km 82 (Piscacucho)", HebrewName = "קילומטר 82 (פיסקקוצ'ו)", Latitude = -13.2380, Longitude = -72.3780, ElevationMeters = 2600, Type = WaypointType.Start, Description = "Official Inca Trail checkpoint and Urubamba river crossing.", DayNumber = 1 },
            new TrekWaypoint { Name = "Wayllabamba Camp", HebrewName = "מחנה וואילאבמבה", Latitude = -13.2350, Longitude = -72.4350, ElevationMeters = 3000, Type = WaypointType.Campsite, Description = "Traditional Andean hamlet at the base of the mountain pass.", DayNumber = 1 },
            new TrekWaypoint { Name = "Dead Woman's Pass (Warmiwañusqa)", HebrewName = "מעבר האישה המתה (4,215 מ')", Latitude = -13.2500, Longitude = -72.4850, ElevationMeters = 4215, Type = WaypointType.MountainPass, Description = "Highest point on the Inca Trail with panoramic views across the Andes.", DayNumber = 2 },
            new TrekWaypoint { Name = "Pacaymayo Campsite", HebrewName = "מחנה פקאימאיו", Latitude = -13.2450, Longitude = -72.5100, ElevationMeters = 3600, Type = WaypointType.Campsite, Description = "Valley campsite nestled along glacial stream.", DayNumber = 2 },
            new TrekWaypoint { Name = "Wiñay Wayna Ruins", HebrewName = "מצודת וויניאיוואינה", Latitude = -13.1920, Longitude = -72.5350, ElevationMeters = 2650, Type = WaypointType.AncientRuins, Description = "Terraced agricultural wonder perched above the Urubamba canyon.", DayNumber = 3 },
            new TrekWaypoint { Name = "Sun Gate (Inti Punku)", HebrewName = "שער השמש (אינטי פונקו)", Latitude = -13.1680, Longitude = -72.5380, ElevationMeters = 2720, Type = WaypointType.Viewpoint, Description = "First awe-inspiring panoramic view of Machu Picchu at sunrise.", DayNumber = 4 },
            new TrekWaypoint { Name = "Machu Picchu Citadel", HebrewName = "מאצ'ו פיצ'ו", Latitude = -13.1631, Longitude = -72.5450, ElevationMeters = 2430, Type = WaypointType.Finish, Description = "Final destination of the Inca trail.", DayNumber = 4 }
        };

        route.Coordinates = new List<TrekCoordinate>
        {
            new(-13.2380, -72.3780, 2600, 0),
            new(-13.2360, -72.4100, 2800, 6.0),
            new(-13.2350, -72.4350, 3000, 12.0),
            new(-13.2420, -72.4600, 3700, 16.5),
            new(-13.2500, -72.4850, 4215, 21.0),
            new(-13.2450, -72.5100, 3600, 26.0),
            new(-13.2200, -72.5250, 3100, 31.0),
            new(-13.1920, -72.5350, 2650, 36.0),
            new(-13.1680, -72.5380, 2720, 41.5),
            new(-13.1631, -72.5450, 2430, 43.0)
        };

        route.ElevationProfile = new List<ElevationPoint>
        {
            new(0, 2600, "Km 82"),
            new(12, 3000, "Wayllabamba"),
            new(21, 4215, "Warmiwañusqa Pass"),
            new(26, 3600, "Pacaymayo"),
            new(36, 2650, "Wiñay Wayna"),
            new(41.5, 2720, "Sun Gate"),
            new(43, 2430, "Machu Picchu")
        };

        return route;
    }

    private static TrekRoute CreateRainbowMountain()
    {
        var route = new TrekRoute
        {
            Id = Guid.Parse("cc33dd44-55ee-66ff-77aa-88bb99cc0003"),
            Name = "Rainbow Mountain (Vinicunca)",
            HebrewName = "הר הצבעים (ויניקונקה)",
            Region = "Cusco / Pitumarca",
            TotalDistanceKm = 8.5,
            ElevationGainMeters = 436,
            MaxElevationMeters = 5036,
            MinElevationMeters = 4600,
            Difficulty = "Challenging Altitude",
            EstimatedDurationDays = 1,
            Summary = "The world-famous multicolored mineral mountain in the high Andes. Hike through llama and alpaca pastures up to 5,036 meters.",
            HebrewSummary = "ההר הצבעוני המפורסם בעולם בלב האנדים. טיפוס לצד עדרי למות ואלפקות עד לפסגה בגובה 5,036 מטר."
        };

        route.Waypoints = new List<TrekWaypoint>
        {
            new TrekWaypoint { Name = "Cusipata Trailhead", HebrewName = "נקודת יציאה (קוסיפאטה)", Latitude = -13.8820, Longitude = -71.3200, ElevationMeters = 4600, Type = WaypointType.Start, Description = "High altitude trailhead parking and oxygen station.", DayNumber = 1 },
            new TrekWaypoint { Name = "Red Valley Junction", HebrewName = "פיצול העמק האדום", Latitude = -13.8740, Longitude = -71.3100, ElevationMeters = 4850, Type = WaypointType.Viewpoint, Description = "Stunning viewpoint of the vibrant ochre and crimson valley.", DayNumber = 1 },
            new TrekWaypoint { Name = "Vinicunca Rainbow Ridge", HebrewName = "פסגת הר הצבעים (5,036 מ')", Latitude = -13.8694, Longitude = -71.3031, ElevationMeters = 5036, Type = WaypointType.Finish, Description = "The iconic turquoise, gold, lavender, and pink mineral ridges facing Ausangate glacier.", DayNumber = 1 }
        };

        route.Coordinates = new List<TrekCoordinate>
        {
            new(-13.8820, -71.3200, 4600, 0),
            new(-13.8780, -71.3150, 4750, 2.2),
            new(-13.8740, -71.3100, 4850, 4.8),
            new(-13.8710, -71.3060, 4960, 6.8),
            new(-13.8694, -71.3031, 5036, 8.5)
        };

        route.ElevationProfile = new List<ElevationPoint>
        {
            new(0, 4600, "Trailhead"),
            new(4.8, 4850, "Red Valley"),
            new(8.5, 5036, "Rainbow Summit")
        };

        return route;
    }

    private static TrekRoute CreateAusangateCircuit()
    {
        var route = new TrekRoute
        {
            Id = Guid.Parse("dd44ee55-66ff-77aa-88bb-99cc00dd1104"),
            Name = "Ausangate 7 Lakes Circuit",
            HebrewName = "מעגל שבע הלגונות באוסנגטה",
            Region = "Cusco / Ocongate",
            TotalDistanceKm = 14.5,
            ElevationGainMeters = 520,
            MaxElevationMeters = 4650,
            MinElevationMeters = 4300,
            Difficulty = "Moderate-High Altitude",
            EstimatedDurationDays = 1,
            Summary = "Pristine glacial circuit beneath the sacred Apu Ausangate (6,384m), visiting seven crystal lagoons ranging from emerald green to deep turquoise.",
            HebrewSummary = "מסלול שבע הלגונות הקרחוניות המרהיבות למרגלות האפו הקדוש אוסנגטה (6,384 מ'). מים צלולים ורחצה במעיינות חמים בפצ'אנטה."
        };

        route.Waypoints = new List<TrekWaypoint>
        {
            new TrekWaypoint { Name = "Pacchanta Thermal Springs", HebrewName = "פצ'אנטה ומעיינות חמים", Latitude = -13.7380, Longitude = -71.2250, ElevationMeters = 4300, Type = WaypointType.Start, Description = "High altitude village famous for natural thermal baths.", DayNumber = 1 },
            new TrekWaypoint { Name = "Laguna Azulcocha", HebrewName = "לגונה אסולקוצ'ה (הכחולה)", Latitude = -13.7250, Longitude = -71.2100, ElevationMeters = 4480, Type = WaypointType.Viewpoint, Description = "First deep blue glacial lagoon.", DayNumber = 1 },
            new TrekWaypoint { Name = "Laguna Otorongococha", HebrewName = "לגונה אוטורונגוקוצ'ה (היגואר)", Latitude = -13.7150, Longitude = -71.2000, ElevationMeters = 4550, Type = WaypointType.Viewpoint, Description = "Lagoon named for its unique shoreline pattern resembling jaguar spots.", DayNumber = 1 },
            new TrekWaypoint { Name = "Laguna Comercocha Viewpoint", HebrewName = "לגונה קומרקוצ'ה (הירוקה)", Latitude = -13.7050, Longitude = -71.1920, ElevationMeters = 4650, Type = WaypointType.Finish, Description = "Highest point on the 7 lakes loop directly facing Mt. Ausangate.", DayNumber = 1 }
        };

        route.Coordinates = new List<TrekCoordinate>
        {
            new(-13.7380, -71.2250, 4300, 0),
            new(-13.7300, -71.2180, 4390, 3.5),
            new(-13.7250, -71.2100, 4480, 6.8),
            new(-13.7150, -71.2000, 4550, 10.2),
            new(-13.7050, -71.1920, 4650, 14.5)
        };

        route.ElevationProfile = new List<ElevationPoint>
        {
            new(0, 4300, "Pacchanta"),
            new(6.8, 4480, "Azulcocha"),
            new(10.2, 4550, "Otorongococha"),
            new(14.5, 4650, "Comercocha Pass")
        };

        return route;
    }
}

