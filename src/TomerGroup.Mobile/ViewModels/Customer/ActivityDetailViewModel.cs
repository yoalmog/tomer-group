using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

#if USE_MAUI
using Microsoft.Maui.Controls;
#endif

namespace TomerGroup.Mobile.ViewModels.Customer;

#if USE_MAUI
public partial class ActivityDetailViewModel : BaseViewModel, IQueryAttributable
#else
public partial class ActivityDetailViewModel : BaseViewModel
#endif
{
    private readonly IDestinationImageService _imageService;
    private readonly IMobileMapService _mapService;

    [ObservableProperty]
    private string _activityTitle = "פרטי פעילות";

    [ObservableProperty]
    private string _headerImage = string.Empty;

    [ObservableProperty]
    private string _destination = "Cusco";

    [ObservableProperty]
    private string _startTime = "08:00";

    [ObservableProperty]
    private string _duration = "3.5 שעות";

    [ObservableProperty]
    private string _location = "קוסקו, פרו";

    [ObservableProperty]
    private string _guideName = string.Empty;

    [ObservableProperty]
    private string _driverName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _meetingPoint = string.Empty;

    [ObservableProperty]
    private string _whatToBring = string.Empty;

    [ObservableProperty]
    private string _altitudeNotes = string.Empty;

    public string BackButtonText => CurrentLanguage switch
    {
        "en" => "← Back to Itinerary",
        "es" => "← Volver al Itinerario",
        _ => "← חזרה למסלול"
    };

    public string ConfirmedBadgeText => CurrentLanguage switch
    {
        "en" => "Verified Experience",
        "es" => "Experiencia Confirmada",
        _ => "חוויה מאושרת"
    };

    public string StartTimeLabel => CurrentLanguage switch
    {
        "en" => "⏰ Start Time",
        "es" => "⏰ Hora de Inicio",
        _ => "⏰ שעת התחלה"
    };

    public string EstimatedDurationText => CurrentLanguage switch
    {
        "en" => $"Est. duration: {Duration}",
        "es" => $"Duración est.: {Duration}",
        _ => $"משך משוער: {Duration}"
    };

    public string LocationLabel => CurrentLanguage switch
    {
        "en" => "📍 Location",
        "es" => "📍 Ubicación",
        _ => "📍 מיקום"
    };

    public string NavigateMapText => CurrentLanguage switch
    {
        "en" => "View Map 🗺️",
        "es" => "Ver Mapa 🗺️",
        _ => "נווט במפה 🗺️"
    };

    public string CertifiedGuideLabel => CurrentLanguage switch
    {
        "en" => "Certified Tomer Group Guide",
        "es" => "Guía Certificado de Tomer Group",
        _ => "מדריך מוסמך מטעם Tomer Group"
    };

    public string MeetingPointTitle => CurrentLanguage switch
    {
        "en" => "Meeting & Pickup Point",
        "es" => "Punto de Encuentro y Recojo",
        _ => "נקודת מפגש ואיסוף"
    };

    public string DescriptionTitle => CurrentLanguage switch
    {
        "en" => "Activity Description",
        "es" => "Descripción de la Actividad",
        _ => "תיאור הפעילות"
    };

    public string WhatToBringTitle => CurrentLanguage switch
    {
        "en" => "🎒 What to Bring?",
        "es" => "🎒 ¿Qué Llevar?",
        _ => "🎒 מה להביא איתך?"
    };

    public string AltitudeNotesTitle => CurrentLanguage switch
    {
        "en" => "🏔️ Altitude & Health Notes",
        "es" => "🏔️ Consejos de Altitud y Salud",
        _ => "🏔️ דגשי גובה ובריאות"
    };

    public string SupportButtonText => CurrentLanguage switch
    {
        "en" => "Questions about this activity? Chat with us on WhatsApp 💬",
        "es" => "¿Preguntas sobre la actividad? Escríbenos por WhatsApp 💬",
        _ => "שאלה על הפעילות? דברו איתנו בוואטסאפ 💬"
    };

    public ActivityDetailViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IDestinationImageService imageService,
        IMobileMapService? mapService = null)
        : base(localization, navigation)
    {
        _imageService = imageService;
        _mapService = mapService ?? new MobileMapService();
        Title = CurrentLanguage switch
        {
            "en" => "Activity Details",
            "es" => "Detalles de la Actividad",
            _ => "פרטי פעילות"
        };
        HeaderImage = _imageService.GetMachuPicchuImage();
        ApplyLocalizedDefaults();
    }

    private void ApplyLocalizedDefaults()
    {
        if (string.IsNullOrWhiteSpace(Description) || Description.Contains("סיור מודרך") || Description.Contains("Fascinating guided") || Description.Contains("Fascinante tour"))
        {
            Description = CurrentLanguage switch
            {
                "en" => "Fascinating guided tour with a certified Tomer Group specialist. All tickets, coordination, and transfers are pre-arranged.",
                "es" => "Fascinante tour guiado con un especialista certificado de Tomer Group. Todas las entradas, traslados y coordinaciones están organizadas.",
                _ => "סיור מודרך מרתק עם מדריך מוסמך מטעם Tomer Group. כל הכניסות, התיאומים וההעברות מאורגנים מראש."
            };
        }

        if (string.IsNullOrWhiteSpace(MeetingPoint) || MeetingPoint.Contains("איסוף מלובי") || MeetingPoint.Contains("Hotel lobby pickup") || MeetingPoint.Contains("Recojo del lobby"))
        {
            MeetingPoint = CurrentLanguage switch
            {
                "en" => "Hotel lobby pickup at the scheduled time",
                "es" => "Recojo del lobby del hotel a la hora programada",
                _ => "איסוף מלובי המלון בשעה הנקובה"
            };
        }

        if (string.IsNullOrWhiteSpace(WhatToBring) || WhatToBring.Contains("דרכון מקורי") || WhatToBring.Contains("Original passport") || WhatToBring.Contains("Pasaporte original"))
        {
            WhatToBring = CurrentLanguage switch
            {
                "en" => "Original passport, layered warm & rain clothing, hat, sunscreen, and water bottle.",
                "es" => "Pasaporte original, ropa abrigadora e impermeable en capas, gorra, bloqueador solar y agua.",
                _ => "דרכון מקורי, שכבות לבוש (חם וקר), כובע, קרם הגנה ומים."
            };
        }

        if (string.IsNullOrWhiteSpace(AltitudeNotes) || AltitudeNotes.Contains("יש להקפיד") || AltitudeNotes.Contains("Drink plenty of water") || AltitudeNotes.Contains("Beba abundante agua"))
        {
            AltitudeNotes = CurrentLanguage switch
            {
                "en" => "Drink plenty of water, pace yourself during walks, and enjoy hot coca tea.",
                "es" => "Beba abundante agua, camine a paso moderado y tome mate de coca caliente.",
                _ => "יש להקפיד על שתיית מים מרובה והליכה בקצב מתון."
            };
        }
    }

    protected override void OnLanguageChanged()
    {
        Title = CurrentLanguage switch
        {
            "en" => "Activity Details",
            "es" => "Detalles de la Actividad",
            _ => "פרטי פעילות"
        };
        ApplyLocalizedDefaults();
        OnPropertyChanged(nameof(BackButtonText));
        OnPropertyChanged(nameof(ConfirmedBadgeText));
        OnPropertyChanged(nameof(StartTimeLabel));
        OnPropertyChanged(nameof(EstimatedDurationText));
        OnPropertyChanged(nameof(LocationLabel));
        OnPropertyChanged(nameof(NavigateMapText));
        OnPropertyChanged(nameof(CertifiedGuideLabel));
        OnPropertyChanged(nameof(MeetingPointTitle));
        OnPropertyChanged(nameof(DescriptionTitle));
        OnPropertyChanged(nameof(WhatToBringTitle));
        OnPropertyChanged(nameof(AltitudeNotesTitle));
        OnPropertyChanged(nameof(SupportButtonText));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("title", out var titleObj) && titleObj is string titleStr)
        {
            ActivityTitle = Uri.UnescapeDataString(titleStr);
            HeaderImage = _imageService.GetActivityImage(ActivityTitle);
        }

        if (query.TryGetValue("location", out var locObj) && locObj is string locStr)
        {
            Location = Uri.UnescapeDataString(locStr);
        }

        if (query.TryGetValue("time", out var timeObj) && timeObj is string timeStr)
        {
            StartTime = timeStr;
        }

        if (query.TryGetValue("guide", out var guideObj) && guideObj is string guideStr)
        {
            GuideName = guideStr;
        }

        if (query.TryGetValue("driver", out var driverObj) && driverObj is string driverStr)
        {
            DriverName = driverStr;
        }
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Navigation.GoBackAsync();
    }

    [RelayCommand]
    public async Task OpenMapAsync()
    {
        var target = !string.IsNullOrWhiteSpace(Location) ? Location : Destination;
        await Navigation.NavigateToAsync($"TrekMap?trekName={Uri.EscapeDataString(target)}");
    }

    [RelayCommand]
    public async Task ContactSupportAsync()
    {
        await Navigation.NavigateToAsync("More");
    }
}
