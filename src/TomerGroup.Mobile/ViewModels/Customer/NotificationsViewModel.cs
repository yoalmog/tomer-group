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
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

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
            ErrorMessage = $"שגיאה בטעינת הודעות: {ex.Message}";
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

        IsEmpty = FilteredNotifications.Count == 0;
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
