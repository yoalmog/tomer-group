using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class MoreViewModel : BaseViewModel
{
    private readonly ISecureStorageService _secureStorage;
    private readonly IOfflineSyncManager _syncManager;

    [ObservableProperty]
    private string _agencyPhone = "+51 84 223 456";

    [ObservableProperty]
    private string _agencyWhatsApp = "+51 984 123 456";

    [ObservableProperty]
    private string _emergencyPhone = "+51 984 999 888";

    [ObservableProperty]
    private string _chabadAddress = "Calle San Agustín 415, Cusco (2 min from Plaza de Armas)";

    [ObservableProperty]
    private string _chabadPhone = "+51 84 236842";

    [ObservableProperty]
    private string _selectedLanguage = "he";

    [ObservableProperty]
    private string _syncStatusMessage = string.Empty;

    public MoreViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        ISecureStorageService secureStorage,
        IOfflineSyncManager syncManager)
        : base(localization, navigation)
    {
        _secureStorage = secureStorage;
        _syncManager = syncManager;
        Title = Localize(LocalizationKeys.NavMore);
        SelectedLanguage = localization.CurrentLanguage;
    }

    [RelayCommand]
    public void ChangeLanguage(string lang)
    {
        SelectedLanguage = lang;
        Localization.SetLanguage(lang);
        RefreshDirection();
    }

    [RelayCommand]
    public async Task TriggerOfflineSyncAsync()
    {
        IsBusy = true;
        try
        {
            var success = await _syncManager.SynchronizeAsync(Guid.Empty, true);
            SyncStatusMessage = success 
                ? "✓ כל המסמכים, אישורי הכניסה ומסלול הטיול עודכנו לאופליין בהצלחה" 
                : "שים לב: הנתונים נשמרו מקומית וייסונכרנו בעת חיבור לרשת";
        }
        catch (Exception ex)
        {
            SyncStatusMessage = $"שגיאת סנכרון: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SignOutAsync()
    {
        await _secureStorage.RemoveAsync("auth_token");
        await _secureStorage.RemoveAsync("refresh_token");
        await Navigation.NavigateToLoginAsync();
    }
}
