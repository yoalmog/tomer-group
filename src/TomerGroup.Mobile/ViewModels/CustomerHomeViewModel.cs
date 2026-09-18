using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

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

        UpdateLocalizedTexts();
    }

    public string SignInButtonText => CurrentLanguage switch
    {
        "en" => "Sign In",
        "es" => "Iniciar Sesión",
        _ => "התחבר / Sign In"
    };

    public string HeroSignInButtonText => CurrentLanguage switch
    {
        "en" => "Sign In to My Trip",
        "es" => "Iniciar Sesión en Mi Viaje",
        _ => "התחבר לטיול שלי"
    };

    public string ViewFullItineraryButtonText => CurrentLanguage switch
    {
        "en" => "View Full Itinerary",
        "es" => "Ver Itinerario Completo",
        _ => "לצפייה במסלול הטיול המלא"
    };

    public string DiscoverButtonText => CurrentLanguage switch
    {
        "en" => "Discover Destinations & Experiences",
        "es" => "Descubrir Destinos y Experiencias",
        _ => "גלה יעדים וחוויות"
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

    public string ItineraryTabLabel => CurrentLanguage switch
    {
        "en" => "Itinerary",
        "es" => "Itinerario",
        _ => "המסלול"
    };

    public string BookingsTabLabel => CurrentLanguage switch
    {
        "en" => "Bookings",
        "es" => "Reservas",
        _ => "הזמנות"
    };

    public string DocumentsTabLabel => CurrentLanguage switch
    {
        "en" => "Documents",
        "es" => "Documentos",
        _ => "מסמכים"
    };

    public string ExploreTabLabel => CurrentLanguage switch
    {
        "en" => "Explore",
        "es" => "Explorar",
        _ => "גלה עוד"
    };

    public string MyDayChipLabel => CurrentLanguage switch
    {
        "en" => "My Day",
        "es" => "Mi Día",
        _ => "היום שלי"
    };

    public string AIAssistantChipLabel => CurrentLanguage switch
    {
        "en" => "AI Travel Guide",
        "es" => "Guía AI",
        _ => "עוזר טיולים AI"
    };

    public string PackingListChipLabel => CurrentLanguage switch
    {
        "en" => "Packing List",
        "es" => "Equipaje",
        _ => "רשימת ציוד"
    };

    public string TripMemoriesChipLabel => CurrentLanguage switch
    {
        "en" => "Memories",
        "es" => "Recuerdos",
        _ => "יומן חוויות"
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
        "en" => "The Tomer Group Service Experience",
        "es" => "La Experiencia Tomer Group",
        _ => "חוויית השירות של Tomer Group"
    };

    public string AcclimatizationTitle => CurrentLanguage switch
    {
        "en" => "🫁 Altitude Acclimatization",
        "es" => "🫁 Aclimatación a la Altitud",
        _ => "🫁 התאקלמות לגובה"
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
        "es" => "✡️ Kosher y Shabat",
        _ => "✡️ כשרות ושבת"
    };

    public string KosherShabbatDescription => CurrentLanguage switch
    {
        "en" => "Glatt kosher Shabbat meals coordinated with local Chabad centers in Cusco and Lima.",
        "es" => "Comidas de Shabat coordinadas con las sedes locales de Jabad en Cusco y Lima.",
        _ => "סיוע בתיאום ארוחות שבת כשרות בקוסקו ולימה עם בתי חב\"ד המקומיים."
    };

    public string SupportHotlineTitle => CurrentLanguage switch
    {
        "en" => "24/7 Field Support in Peru",
        "es" => "Soporte en Perú 24/7",
        _ => "מוקד שירות וסיוע 24/7 בפרו"
    };

    public string SupportHotlineSubtitle => CurrentLanguage switch
    {
        "en" => "Our dedicated field team is available anytime via WhatsApp and phone",
        "es" => "Nuestro equipo local está disponible a toda hora por WhatsApp y teléfono",
        _ => "צוות השטח שלנו זמין עבורכם בכל שעה בוואטסאפ ובטלפון"
    };

    private void UpdateLocalizedTexts()
    {
        AgencyContactPhone = LocalizationService.FormatPhoneNumber(Brand.ContactPhone);
        EmergencyPhone = LocalizationService.FormatPhoneNumber(Brand.EmergencyContact);
        OnPropertyChanged(nameof(SignInButtonText));
        OnPropertyChanged(nameof(HeroSignInButtonText));
        OnPropertyChanged(nameof(ViewFullItineraryButtonText));
        OnPropertyChanged(nameof(DiscoverButtonText));
        OnPropertyChanged(nameof(NextActivityLabel));
        OnPropertyChanged(nameof(TimeLabel));
        OnPropertyChanged(nameof(ActivityDetailsButtonText));
        OnPropertyChanged(nameof(JourneyProgressLabel));
        OnPropertyChanged(nameof(ItineraryTabLabel));
        OnPropertyChanged(nameof(BookingsTabLabel));
        OnPropertyChanged(nameof(DocumentsTabLabel));
        OnPropertyChanged(nameof(ExploreTabLabel));
        OnPropertyChanged(nameof(MyDayChipLabel));
        OnPropertyChanged(nameof(AIAssistantChipLabel));
        OnPropertyChanged(nameof(PackingListChipLabel));
        OnPropertyChanged(nameof(TripMemoriesChipLabel));
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
        OnPropertyChanged(nameof(SupportHotlineTitle));
        OnPropertyChanged(nameof(SupportHotlineSubtitle));
    }

    protected override void OnLanguageChanged()
    {
        Title = Localize(LocalizationKeys.NavHome);
        UpdateLocalizedTexts();
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
                // 1. Fetch real customer profile
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

                // 2. Fetch real customer trips
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
                // Public unauthenticated state
                CustomerGreeting = "Tomer Group";
                CustomerName = string.Empty;
                HasActiveTrip = false;
                HasNextActivity = false;
                HeroImageUrl = _imageService.GetMachuPicchuImage();
                HeroTitle = "המסע שלך בפרו מתחיל כאן";
                HeroSubtitle = "טיולי בוטיק, אישורי כניסה למאצ'ו פיצ'ו, רכבות פנורמיות ומלונות יוקרה בהרי האנדים";
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
    public async Task OpenBookingsAsync()
    {
        await Navigation.NavigateToAsync("//Bookings");
    }

    [RelayCommand]
    public async Task OpenDocumentsAsync()
    {
        await Navigation.NavigateToAsync("Documents");
    }

    [RelayCommand]
    public async Task OpenProfileAsync()
    {
        await Navigation.NavigateToAsync("//Profile");
    }

    [RelayCommand]
    public async Task OpenMyDayAsync()
    {
        await Navigation.NavigateToAsync("MyDay");
    }

    [RelayCommand]
    public async Task OpenCustomerAIAssistantAsync()
    {
        await Navigation.NavigateToAsync("CustomerAIAssistant");
    }

    [RelayCommand]
    public async Task OpenPackingListAsync()
    {
        await Navigation.NavigateToAsync("PackingList");
    }

    [RelayCommand]
    public async Task OpenTripMemoriesAsync()
    {
        await Navigation.NavigateToAsync("TripMemories");
    }

    [RelayCommand]
    public async Task OpenLoginAsync()
    {
        await Navigation.NavigateToLoginAsync();
    }

    [RelayCommand]
    public async Task OpenSupportAsync()
    {
        await Navigation.NavigateToAsync("More");
    }

    [RelayCommand]
    public void ToggleLanguage()
    {
        var next = Localization.CurrentLanguage switch
        {
            "he" => "en",
            "en" => "es",
            _ => "he"
        };
        Localization.SetLanguage(next);
        RefreshDirection();
        UpdateLocalizedTexts();
    }
}
