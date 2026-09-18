using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyAIAssistantViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public AgencyAIAssistantViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        DraftDays = new ObservableCollection<ItineraryDayDraftDto>();
    }

    public ObservableCollection<ItineraryDayDraftDto> DraftDays { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Prompt Inputs
    [ObservableProperty]
    private string _destination = "קוסקו, עמק הקדוש ומאצ'ו פיצ'ו";

    [ObservableProperty]
    private int _days = 5;

    [ObservableProperty]
    private string _travelerProfile = "Backpacker";

    [ObservableProperty]
    private bool _isKosherRequired = true;

    [ObservableProperty]
    private bool _isShabbatObservant = true;

    [ObservableProperty]
    private string _additionalNotes = "זוג ישראלים אחרי צבא, מעוניינים במלון קרוב לחב\"ד והסתגלות גובה הדרגתית.";

    // Generated Result
    [ObservableProperty]
    private ItineraryDraftResultDto? _currentDraft;

    [ObservableProperty]
    private bool _hasDraft;

    [ObservableProperty]
    private string _draftStatus = "Draft (ממתין לאישור צוות)";

    [ObservableProperty]
    private bool _isDraftApproved;

    // WhatsApp Assistant
    [ObservableProperty]
    private string _whatsAppTopic = "AltitudeSickness";

    [ObservableProperty]
    private string _whatsAppDraftText = string.Empty;

    [RelayCommand]
    public async Task GenerateItineraryDraftAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = "ה-AI של Tomer Group מגבש הצעת מסלול מותאמת אישית...";

        try
        {
            var prompt = new GenerateItineraryPromptDto
            {
                Destination = Destination,
                Days = Days,
                TravelerProfile = TravelerProfile,
                IsKosherRequired = IsKosherRequired,
                IsShabbatObservant = IsShabbatObservant,
                AdditionalNotes = AdditionalNotes
            };

            var response = await _apiClient.GenerateItineraryDraftAsync(prompt);
            if (response.Success && response.Data != null)
            {
                CurrentDraft = response.Data;
                DraftDays.Clear();
                foreach (var d in response.Data.Days)
                {
                    DraftDays.Add(d);
                }

                HasDraft = true;
                DraftStatus = "טיוטה נוצרה ע\"י AI - ממתינה לבדיקה ואישור אנושי (Human-in-the-Loop)";
                IsDraftApproved = false;
                StatusMessage = "הצעת המסלול גובשה בהצלחה!";
            }
            else
            {
                ErrorMessage = response.Message ?? "שגיאה בגיבוש טיוטת המסלול";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"שגיאה: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ApproveDraftAsync()
    {
        if (CurrentDraft == null) return;

        IsBusy = true;
        try
        {
            var response = await _apiClient.ApproveAIRequestAsync(CurrentDraft.RequestId, "Approved by Agency Manager for booking conversion");
            if (response.Success)
            {
                IsDraftApproved = true;
                DraftStatus = "✓ מאושר ומסודר ע\"י הצוות (Human Approved) - מוכן לפתיחת תיק טיול!";
                StatusMessage = "ההצעה אושרה בהצלחה ותועדה ביומן הביקורת.";
            }
            else
            {
                ErrorMessage = response.Message ?? "שגיאה באישור הטיוטה";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"שגיאה: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task RejectDraftAsync()
    {
        if (CurrentDraft == null) return;

        IsBusy = true;
        try
        {
            HasDraft = false;
            IsDraftApproved = false;
            DraftDays.Clear();
            DraftStatus = "הטיוטה נדחתה לבקשת המנהל. ניתן להפיק טיוטה חדשה עם הנחיות מעודכנות.";
            StatusMessage = "הטיוטה הוסרה.";
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"שגיאה בדחיית הטיוטה: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DraftWhatsAppResponseAsync(string topic)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            WhatsAppTopic = topic;
            var response = await _apiClient.DraftAIWhatsAppMessageAsync(new DraftWhatsAppPromptDto
            {
                CustomerName = "דני כהן",
                InquiryTopic = topic,
                Language = "he"
            });

            if (response.Success && response.Data != null)
            {
                WhatsAppDraftText = response.Data.DraftedText;
                StatusMessage = "תשובת ווטסאפ נוסחה בהצלחה בייעוץ AI.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"שגיאה בניסוח הודעה: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

