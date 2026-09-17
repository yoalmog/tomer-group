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

    private void UpdateLocalizedTexts()
    {
        AgencyContactPhone = LocalizationService.FormatPhoneNumber(Brand.ContactPhone);
        EmergencyPhone = LocalizationService.FormatPhoneNumber(Brand.EmergencyContact);
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
