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
    private Guid _customerId = Guid.Empty;

    public ProfileViewModel(
        IApiClient apiClient,
        ISecureStorageService secureStorage,
        ILocalizationService localizationService)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;
        _localizationService = localizationService;
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
    private string _firstName = "Danny";

    [ObservableProperty]
    private string _lastName = "Cohen";

    [ObservableProperty]
    private string _hebrewName = "דני כהן";

    [ObservableProperty]
    private string _passportName = "DANNY COHEN";

    [ObservableProperty]
    private string _phone = "+972 54 123 4567";

    [ObservableProperty]
    private string _whatsApp = "+972 54 123 4567";

    [ObservableProperty]
    private string _email = "danny@israel.com";

    [ObservableProperty]
    private string _country = "Israel";

    [ObservableProperty]
    private string _maskedPassportNumber = "IL-****2711";

    [ObservableProperty]
    private string _unmaskedPassportNumber = string.Empty;

    [ObservableProperty]
    private bool _isPassportRevealed = false;

    [ObservableProperty]
    private DateTime? _passportExpiration = DateTime.UtcNow.AddYears(3);

    [ObservableProperty]
    private bool _isPassportExpiringSoon = false;

    [ObservableProperty]
    private DateTime? _dateOfBirth = new DateTime(1992, 5, 14);

    [ObservableProperty]
    private string _israelIdNumber = "038291048";

    [ObservableProperty]
    private string _emergencyContactName = "Sarah Cohen";

    [ObservableProperty]
    private string _emergencyContactPhone = "+972 54 999 1122";

    [ObservableProperty]
    private string _specialRequests = "High altitude acclimatization support, window seats on PeruRail train";

    [ObservableProperty]
    private string _dietaryPreferences = "כשר למהדרין (Kosher Mehudar)";

    [ObservableProperty]
    private string _selectedDietaryOption = "כשר למהדרין (Kosher Mehudar)";

    [ObservableProperty]
    private string _medicalNotes = "Mild altitude sickness on day 1 in Cusco, taking Sorojchi pills, drinking coca tea";

    [ObservableProperty]
    private string _insuranceCompany = "Harel Travel Insurance (PassportCard Rescue)";

    [ObservableProperty]
    private string _insurancePolicyNumber = "HR-2026-98104";

    [ObservableProperty]
    private bool _isActiveInPeru = true;

    [ObservableProperty]
    private bool _isEditing = false;

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccessMessage = false;

    public string DisplayPassportNumber => IsPassportRevealed && !string.IsNullOrEmpty(UnmaskedPassportNumber)
        ? UnmaskedPassportNumber
        : MaskedPassportNumber;

    public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();

    [RelayCommand]
    public async Task InitializeAsync()
    {
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
            StatusMessage = $"Offline fallback profile: {ex.Message}";
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

            // Fallback unmasking demo
            UnmaskedPassportNumber = "IL-39482711";
            IsPassportRevealed = true;
            OnPropertyChanged(nameof(DisplayPassportNumber));
        }
        catch
        {
            UnmaskedPassportNumber = "IL-39482711";
            IsPassportRevealed = true;
            OnPropertyChanged(nameof(DisplayPassportNumber));
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
}

