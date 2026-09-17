using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService _secureStorage;
    private readonly ILocalizationService _localizationService;
    private readonly INavigationService _navigationService;
    private Guid _customerId = Guid.Empty;

    public ProfileViewModel(
        IApiClient apiClient,
        ISecureStorageService secureStorage,
        ILocalizationService localizationService,
        INavigationService navigationService)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;
        _localizationService = localizationService;
        _navigationService = navigationService;
        DietaryOptions = new List<string>
        {
            "כשר למהדרין (Kosher Mehudar)",
            "כשר רגיל (Kosher Standard)",
            "צמחוני (Vegetarian)",
            "טבעוני (Vegan)",
            "ללא גלוטן (Gluten-Free)",
            "ללא הגבלות (No Restrictions)"
        };
    }

    public List<string> DietaryOptions { get; }

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _hebrewName = string.Empty;

    [ObservableProperty]
    private string _passportName = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _whatsApp = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _country = "Israel";

    [ObservableProperty]
    private string _maskedPassportNumber = string.Empty;

    [ObservableProperty]
    private string _unmaskedPassportNumber = string.Empty;

    [ObservableProperty]
    private bool _isPassportRevealed = false;

    [ObservableProperty]
    private DateTime? _passportExpiration;

    [ObservableProperty]
    private bool _isPassportExpiringSoon = false;

    [ObservableProperty]
    private DateTime? _dateOfBirth;

    [ObservableProperty]
    private string _israelIdNumber = string.Empty;

    [ObservableProperty]
    private string _emergencyContactName = string.Empty;

    [ObservableProperty]
    private string _emergencyContactPhone = string.Empty;

    [ObservableProperty]
    private string _specialRequests = string.Empty;

    [ObservableProperty]
    private string _dietaryPreferences = string.Empty;

    [ObservableProperty]
    private string _selectedDietaryOption = string.Empty;

    [ObservableProperty]
    private string _medicalNotes = string.Empty;

    [ObservableProperty]
    private string _insuranceCompany = string.Empty;

    [ObservableProperty]
    private string _insurancePolicyNumber = string.Empty;

    [ObservableProperty]
    private bool _isActiveInPeru = true;

    [ObservableProperty]
    private bool _isEditing = false;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccessMessage = false;

    public string DisplayPassportNumber => IsPassportRevealed && !string.IsNullOrEmpty(UnmaskedPassportNumber)
        ? UnmaskedPassportNumber
        : MaskedPassportNumber;

    public string Initials
    {
        get
        {
            var first = !string.IsNullOrEmpty(FirstName) ? FirstName[0] : 'T';
            var last = !string.IsNullOrEmpty(LastName) ? LastName[0] : 'G';
            return $"{first}{last}".ToUpper();
        }
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsAuthenticated = _apiClient.IsAuthenticated;
        if (!IsAuthenticated)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var response = await _apiClient.GetMyProfileAsync();
            if (response.Success && response.Data != null)
            {
                var c = response.Data;
                _customerId = c.Id;
                FirstName = c.FirstName;
                LastName = c.LastName;
                HebrewName = c.HebrewName;
                PassportName = c.PassportName;
                Phone = c.Phone;
                WhatsApp = c.WhatsApp;
                Email = c.Email;
                Country = c.Country;
                MaskedPassportNumber = c.MaskedPassportNumber;
                PassportExpiration = c.PassportExpiration;
                IsPassportExpiringSoon = c.IsPassportExpiringSoon;
                DateOfBirth = c.DateOfBirth;
                MedicalNotes = c.MedicalNotes ?? string.Empty;
                InsuranceCompany = c.InsuranceCompany ?? string.Empty;
                InsurancePolicyNumber = c.InsurancePolicyNumber ?? string.Empty;
                IsActiveInPeru = c.IsActiveInPeru;
                EmergencyContactName = c.EmergencyContactName ?? string.Empty;
                EmergencyContactPhone = c.EmergencyContactPhone ?? string.Empty;
                SpecialRequests = c.SpecialRequests ?? string.Empty;
                DietaryPreferences = c.DietaryPreferences ?? string.Empty;
                SelectedDietaryOption = DietaryPreferences;
                OnPropertyChanged(nameof(Initials));
                OnPropertyChanged(nameof(DisplayPassportNumber));
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"שגיאה בטעינת הפרופיל: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ToggleEditMode()
    {
        IsEditing = !IsEditing;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    public async Task ToggleRevealPassportAsync()
    {
        if (IsPassportRevealed)
        {
            IsPassportRevealed = false;
            OnPropertyChanged(nameof(DisplayPassportNumber));
            return;
        }

        IsBusy = true;
        try
        {
            if (_customerId != Guid.Empty)
            {
                var response = await _apiClient.GetSensitiveDetailsAsync(_customerId);
                if (response.Success && response.Data != null)
                {
                    UnmaskedPassportNumber = response.Data.PassportNumber;
                    IsraelIdNumber = response.Data.IsraelIdNumber;
                    IsPassportRevealed = true;
                    OnPropertyChanged(nameof(DisplayPassportNumber));
                    return;
                }
            }
        }
        catch
        {
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveProfileAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            DietaryPreferences = SelectedDietaryOption;

            var dto = new UpdateCustomerProfileDto
            {
                FirstName = FirstName,
                LastName = LastName,
                HebrewName = HebrewName,
                PassportName = PassportName,
                Phone = Phone,
                WhatsApp = WhatsApp,
                Email = Email,
                Country = Country,
                PassportNumber = IsPassportRevealed ? UnmaskedPassportNumber : null,
                PassportExpiration = PassportExpiration,
                DateOfBirth = DateOfBirth,
                IsraelIdNumber = IsraelIdNumber,
                EmergencyContactName = EmergencyContactName,
                EmergencyContactPhone = EmergencyContactPhone,
                SpecialRequests = SpecialRequests,
                DietaryPreferences = DietaryPreferences,
                MedicalNotes = MedicalNotes,
                InsuranceCompany = InsuranceCompany,
                InsurancePolicyNumber = InsurancePolicyNumber
            };

            var result = await _apiClient.UpdateMyProfileAsync(dto);
            if (result.Success)
            {
                IsSuccessMessage = true;
                StatusMessage = _localizationService.GetString(LocalizationKeys.ProfileUpdatedSuccess);
                IsEditing = false;
            }
            else
            {
                IsSuccessMessage = false;
                StatusMessage = result.Message ?? "Failed to save profile changes";
            }
        }
        catch (Exception ex)
        {
            IsSuccessMessage = false;
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        _apiClient.SetAuthToken(null);
        await _secureStorage.RemoveAsync("auth_token");
        await _secureStorage.RemoveAsync("refresh_token");
        IsAuthenticated = false;
        // LOGOUT -> PUBLIC HOME (Never force user to login screen)
        await _navigationService.NavigateToCustomerShellAsync();
    }

    [RelayCommand]
    public async Task OpenSignInAsync()
    {
        await _navigationService.NavigateToLoginAsync();
    }

    [RelayCommand]
    public void ChangeLanguage(string lang)
    {
        _localizationService.SetLanguage(lang);
    }
}
