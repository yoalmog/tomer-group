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
    private string _description = "סיור מודרך מרתק עם מדריך מוסמך מטעם Tomer Group. כל הכניסות, התיאומים וההעברות מאורגנים מראש.";

    [ObservableProperty]
    private string _meetingPoint = "איסוף מלובי המלון בשעה הנקובה";

    [ObservableProperty]
    private string _whatToBring = "דרכון מקורי, שכבות לבוש (חם וקר), כובע, קרם הגנה ומים.";

    [ObservableProperty]
    private string _altitudeNotes = "יש להקפיד על שתיית מים מרובה והליכה בקצב מתון.";

    public ActivityDetailViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _imageService = imageService;
        Title = "פרטי פעילות";
        HeaderImage = _imageService.GetMachuPicchuImage();
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
    public async Task ContactSupportAsync()
    {
        await Navigation.NavigateToAsync("//More");
    }
}
