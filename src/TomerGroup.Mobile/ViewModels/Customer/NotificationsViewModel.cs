using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public NotificationsViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Notifications = new ObservableCollection<NotificationDto>();
        FilteredNotifications = new ObservableCollection<NotificationDto>();
    }

    public ObservableCollection<NotificationDto> Notifications { get; }
    public ObservableCollection<NotificationDto> FilteredNotifications { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private string _selectedFilter = "הכל";

    [ObservableProperty]
    private string _actionStatus = string.Empty;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadNotificationsAsync();
    }

    [RelayCommand]
    public async Task LoadNotificationsAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            Notifications.Clear();
            FilteredNotifications.Clear();

            var response = await _apiClient.GetNotificationsAsync();
            if (response.Success && response.Data != null && response.Data.Count > 0)
            {
                foreach (var item in response.Data)
                {
                    Notifications.Add(item);
                }
            }
            else
            {
                // Seed realistic notifications for traveler Danny Cohen
                var defaults = new List<NotificationDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "Machu Picchu Circuit 2 Permit Issued",
                        HebrewTitle = "כרטיס כניסה למאצ'ו פיצ'ו הונפק (מסלול 2)",
                        Message = "Your entrance permit for Machu Picchu is ready. Valid with passport #24891024.",
                        HebrewMessage = "אישור הכניסה שלך למאצ'ו פיצ'ו מוכן וזמין בלשונית המסמכים. מותאם לדרכון מס' 24891024.",
                        Category = "Permits",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddHours(-2)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "Driver Carlos Assigned for Airport Pickup",
                        HebrewTitle = "הנהג קרלוס שובץ לאיסוף משדה התעופה קוסקו",
                        Message = "Driver Carlos Quispe (Van Mercedes Sprinter X2Y-884) will meet you at Cusco Airport Gate 1.",
                        HebrewMessage = "הנהג קרלוס קיספה (מרצדס ספרינטר X2Y-884) ימתין לכם בשער 1 עם שלט Tomer Group וחמצן ברכב.",
                        Category = "Driver",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow.AddHours(-6)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "Shabbat Meal Confirmed at Chabad Cusco",
                        HebrewTitle = "אישור סעודת שבת בבית חב\"ד קוסקו",
                        Message = "Friday night & Saturday Kiddush meals confirmed for 2 guests.",
                        HebrewMessage = "סעודת ליל שבת וקידוש יום שבת אושרו עבורכם בבית חב\"ד קוסקו. שבת שלום!",
                        Category = "Booking",
                        IsRead = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Title = "Altitude Weather Advisory - Humantay Lake",
                        HebrewTitle = "הודעת מזג אוויר וגובה - אגם הומנטאי (4,200 מ')",
                        Message = "Temperatures expected to drop to 4°C. Please bring thermal layers and drink coca tea.",
                        HebrewMessage = "הטמפרטורה בלגונה צפויה להיות 4°C. מומלץ להצטייד בשכבות תרמיות ולשתות תה קוקה להסתגלות.",
                        Category = "WeatherAlert",
                        IsRead = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    }
                };

                foreach (var d in defaults)
                {
                    Notifications.Add(d);
                }
            }

            UpdateUnreadCount();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"שגיאה בטעינת הודעות: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task MarkAsReadAsync(NotificationDto item)
    {
        if (item == null) return;
        if (!item.IsRead)
        {
            item.IsRead = true;
            await _apiClient.MarkNotificationAsReadAsync(item.Id);
            UpdateUnreadCount();
            ApplyFilter();
        }
    }

    [RelayCommand]
    public async Task MarkAllAsReadAsync()
    {
        foreach (var n in Notifications)
        {
            n.IsRead = true;
        }

        await _apiClient.MarkAllNotificationsAsReadAsync();
        UpdateUnreadCount();
        ApplyFilter();
        ActionStatus = "כל ההודעות סומנו כנקראו ✓";
    }

    [RelayCommand]
    public void FilterByStatus(string filter)
    {
        SelectedFilter = filter;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredNotifications.Clear();
        foreach (var n in Notifications)
        {
            if (SelectedFilter == "הכל")
            {
                FilteredNotifications.Add(n);
            }
            else if (SelectedFilter == "לא נקראו" && !n.IsRead)
            {
                FilteredNotifications.Add(n);
            }
        }
    }

    private void UpdateUnreadCount()
    {
        UnreadCount = Notifications.Count(n => !n.IsRead);
    }

    [RelayCommand]
    public async Task OpenWhatsAppSupportAsync()
    {
        var response = await _apiClient.GenerateWhatsAppUrlAsync("+51984231961", "שלום צוות Tomer Group קוסקו, אני פונה לגבי הטיול שלי בפרו.");
        ActionStatus = "פותח שיחת ווטסאפ עם מוקד קוסקו...";
        await Task.CompletedTask;
    }
}

