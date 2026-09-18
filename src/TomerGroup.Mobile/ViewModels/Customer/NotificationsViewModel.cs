using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class NotificationsViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public NotificationsViewModel(IApiClient apiClient, INavigationService navigationService)
        : this(new LocalizationService(), apiClient, navigationService)
    {
    }

    public NotificationsViewModel(ILocalizationService localizationService, IApiClient apiClient, INavigationService navigationService)
        : base(localizationService, navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Notifications = new ObservableCollection<NotificationDto>();
        FilteredNotifications = new ObservableCollection<NotificationDto>();
        Title = PageTitle;
    }

    public ObservableCollection<NotificationDto> Notifications { get; }
    public ObservableCollection<NotificationDto> FilteredNotifications { get; }

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private int _unreadCount;

    [ObservableProperty]
    private string _selectedFilter = "all";

    [ObservableProperty]
    private string _actionStatus = string.Empty;

    public string PageTitle => CurrentLanguage switch
    {
        "en" => "Notifications & Updates",
        "es" => "Notificaciones y Actualizaciones",
        _ => "הודעות ועדכונים"
    };

    public string HeroBannerTitle => CurrentLanguage switch
    {
        "en" => "Updates & Alert Center 🔔",
        "es" => "Centro de Actualizaciones y Alertas 🔔",
        _ => "מרכז עדכונים והתרעות 🔔"
    };

    public string HeroBannerSubtitle => CurrentLanguage switch
    {
        "en" => "Driver pickup updates, entrance ticket releases, itinerary adjustments, and urgent alerts from the Cusco team.",
        "es" => "Actualizaciones del conductor, emisión de boletos, ajustes de itinerario y alertas urgentes del equipo de Cusco.",
        _ => "עדכוני זמני איסוף נהג, הנפקת כרטיסי כניסה, שינויי מסלול והודעות דחופות מצוות קוסקו."
    };

    public string WhatsAppSupportButtonText => CurrentLanguage switch
    {
        "en" => "💬 Cusco WhatsApp Support (24/7 Assistance)",
        "es" => "💬 Soporte WhatsApp Cusco (Asistencia 24/7)",
        _ => "💬 מוקד ווטסאפ קוסקו (חירום וסיוע 24/7)"
    };

    public string FilterAllText => CurrentLanguage switch
    {
        "en" => "All",
        "es" => "Todo",
        _ => "הכל"
    };

    public string FilterUnreadText => CurrentLanguage switch
    {
        "en" => "Unread",
        "es" => "No leídos",
        _ => "לא נקראו"
    };

    public string MarkAllReadText => CurrentLanguage switch
    {
        "en" => "Mark all as read ✓",
        "es" => "Marcar todo como leído ✓",
        _ => "סמן הכל כנקרא ✓"
    };

    public string MarkAsReadButtonText => CurrentLanguage switch
    {
        "en" => "Mark as read ✓",
        "es" => "Marcar como leído ✓",
        _ => "סמן כנקרא ✓"
    };

    public string FormattedUnreadCount => CurrentLanguage switch
    {
        "en" => $"{UnreadCount} New",
        "es" => $"{UnreadCount} Nuevas",
        _ => $"{UnreadCount} חדשות"
    };

    protected override void OnLanguageChanged()
    {
        Title = PageTitle;
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(HeroBannerTitle));
        OnPropertyChanged(nameof(HeroBannerSubtitle));
        OnPropertyChanged(nameof(WhatsAppSupportButtonText));
        OnPropertyChanged(nameof(FilterAllText));
        OnPropertyChanged(nameof(FilterUnreadText));
        OnPropertyChanged(nameof(MarkAllReadText));
        OnPropertyChanged(nameof(MarkAsReadButtonText));
        OnPropertyChanged(nameof(FormattedUnreadCount));
        ApplyFilter();
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadNotificationsAsync();
    }

    [RelayCommand]
    public async Task LoadNotificationsAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            Notifications.Clear();
            FilteredNotifications.Clear();

            var response = await _apiClient.GetNotificationsAsync();
            if (response.Success && response.Data != null)
            {
                foreach (var item in response.Data)
                {
                    Notifications.Add(item);
                }
            }

            UpdateUnreadCount();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = CurrentLanguage switch
            {
                "en" => $"Error loading notifications: {ex.Message}",
                "es" => $"Error al cargar notificaciones: {ex.Message}",
                _ => $"שגיאה בטעינת הודעות: {ex.Message}"
            };
            UpdateUnreadCount();
            ApplyFilter();
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
        ActionStatus = CurrentLanguage switch
        {
            "en" => "All notifications marked as read ✓",
            "es" => "Todas las notificaciones marcadas como leídas ✓",
            _ => "כל ההודעות סומנו כנקראו ✓"
        };
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
        var filter = (SelectedFilter ?? string.Empty).ToLowerInvariant();
        foreach (var n in Notifications)
        {
            if (filter is "הכל" or "all" or "todo" or "")
            {
                FilteredNotifications.Add(n);
            }
            else if ((filter is "לא נקראו" or "unread" or "no leídos" or "no leidos") && !n.IsRead)
            {
                FilteredNotifications.Add(n);
            }
        }

        IsEmpty = FilteredNotifications.Count == 0;
    }

    private void UpdateUnreadCount()
    {
        UnreadCount = Notifications.Count(n => !n.IsRead);
        OnPropertyChanged(nameof(FormattedUnreadCount));
    }

    [RelayCommand]
    public async Task OpenWhatsAppSupportAsync()
    {
        var greeting = CurrentLanguage switch
        {
            "en" => "Hello Tomer Group Cusco dispatch, I am contacting you regarding my trip in Peru.",
            "es" => "Hola equipo de Tomer Group Cusco, me comunico sobre mi viaje en Perú.",
            _ => "שלום צוות Tomer Group קוסקו, אני פונה לגבי הטיול שלי בפרו."
        };
        var response = await _apiClient.GenerateWhatsAppUrlAsync("+51984231961", greeting);
        ActionStatus = CurrentLanguage switch
        {
            "en" => "Opening WhatsApp conversation with Cusco team...",
            "es" => "Abriendo chat de WhatsApp con el equipo de Cusco...",
            _ => "פותח שיחת ווטסאפ עם מוקד קוסקו..."
        };
        await Task.CompletedTask;
    }
}
