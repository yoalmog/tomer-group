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
public partial class BookingDetailViewModel : BaseViewModel, IQueryAttributable
#else
public partial class BookingDetailViewModel : BaseViewModel
#endif
{
    private readonly IDestinationImageService _imageService;

    [ObservableProperty]
    private string _bookingTitle = "פרטי הזמנה ושובר";

    [ObservableProperty]
    private string _bookingType = "חבילת שירות";

    [ObservableProperty]
    private string _confirmationCode = "TG-2026-CONFIRMED";

    [ObservableProperty]
    private string _status = "מאושר ומסודר";

    [ObservableProperty]
    private string _headerImage = string.Empty;

    [ObservableProperty]
    private string _dates = "ספטמבר 2026";

    [ObservableProperty]
    private string _provider = "Tomer Group Peru Travel Experience";

    [ObservableProperty]
    private string _location = "קוסקו, פרו";

    [ObservableProperty]
    private string _travelers = "2 נוסעים";

    [ObservableProperty]
    private string _specialRequests = "חדר מועשר בחמצן, ארוחות שבת כשרות בתיאום עם חב\"ד, העברות פרטיות בלבד";

    [ObservableProperty]
    private string _voucherNotes = "שובר זה מהווה אישור רשמי של Tomer Group. יש להציג שובר זה ודרכונים מקוריים בעת קבלת השירות.";

    public BookingDetailViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _imageService = imageService;
        Title = "פרטי שובר";
        HeaderImage = _imageService.GetMachuPicchuImage();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("title", out var titleObj) && titleObj is string titleStr)
        {
            BookingTitle = Uri.UnescapeDataString(titleStr);
            HeaderImage = _imageService.GetActivityImage(BookingTitle, BookingType);
        }

        if (query.TryGetValue("type", out var typeObj) && typeObj is string typeStr)
        {
            BookingType = Uri.UnescapeDataString(typeStr);
        }

        if (query.TryGetValue("code", out var codeObj) && codeObj is string codeStr)
        {
            ConfirmationCode = Uri.UnescapeDataString(codeStr);
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
