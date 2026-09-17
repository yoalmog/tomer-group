using CommunityToolkit.Mvvm.ComponentModel;
using TomerGroup.Core.DTOs;

namespace TomerGroup.Mobile.Services;

public interface IOfflineSyncManager
{
    bool IsOnline { get; }
    DateTime? LastSyncTime { get; }
    string SyncStatusText { get; }
    SyncPackageDto? CachedPackage { get; }
    Task<bool> SynchronizeAsync(Guid customerId, bool forceFull = false);
    Task<bool> CheckConnectivityAsync();
}

public partial class OfflineSyncManager : ObservableObject, IOfflineSyncManager
{
    private readonly IApiClient _apiClient;

    public OfflineSyncManager(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [ObservableProperty]
    private bool _isOnline = true;

    [ObservableProperty]
    private DateTime? _lastSyncTime;

    [ObservableProperty]
    private string _syncStatusText = "מחובר לשרת Tomer Group";

    [ObservableProperty]
    private SyncPackageDto? _cachedPackage;

    public async Task<bool> CheckConnectivityAsync()
    {
        IsOnline = await _apiClient.CheckServerConnectivityAsync();
        SyncStatusText = IsOnline
            ? "מחובר ומסונכרן ✓ (כל הנתונים זמינים גם אופליין)"
            : "מצב לא מקוון בהרי האנדים 📶 (הנתונים זמינים מהזיכרון המקומי)";
        return IsOnline;
    }

    public async Task<bool> SynchronizeAsync(Guid customerId, bool forceFull = false)
    {
        await CheckConnectivityAsync();
        if (!IsOnline)
        {
            // Offline - rely on cached local package
            return false;
        }

        try
        {
            var request = new SyncPullRequestDto
            {
                CustomerId = customerId,
                LastSyncTimestamp = forceFull ? null : LastSyncTime,
                DeviceId = "Mobile-Andes-Device"
            };

            var response = await _apiClient.PullDeltaSyncPackageAsync(request);
            if (response.Success && response.Data != null)
            {
                if (CachedPackage == null || forceFull || !response.Data.IsDeltaSync)
                {
                    CachedPackage = response.Data;
                }
                else
                {
                    // Merge delta into cached package
                    MergeDeltaPackage(response.Data);
                }

                LastSyncTime = response.Data.SyncTimestamp;
                SyncStatusText = $"סונכרן בהצלחה ב-{LastSyncTime:HH:mm} (שמור מקומית לטרקים)";
                return true;
            }

            return false;
        }
        catch
        {
            IsOnline = false;
            SyncStatusText = "מצב לא מקוון (שמור מקומית לטרקים)";
            return false;
        }
    }

    private void MergeDeltaPackage(SyncPackageDto delta)
    {
        if (CachedPackage == null)
        {
            CachedPackage = delta;
            return;
        }

        if (delta.Customer != null)
        {
            CachedPackage.Customer = delta.Customer;
        }

        // Merge Trips
        foreach (var trip in delta.Trips)
        {
            var existingIndex = CachedPackage.Trips.FindIndex(t => t.Id == trip.Id);
            if (existingIndex >= 0)
            {
                CachedPackage.Trips[existingIndex] = trip;
            }
            else
            {
                CachedPackage.Trips.Add(trip);
            }
        }

        // Merge Bookings
        foreach (var booking in delta.Bookings)
        {
            var existingIndex = CachedPackage.Bookings.FindIndex(b => b.Id == booking.Id);
            if (existingIndex >= 0)
            {
                CachedPackage.Bookings[existingIndex] = booking;
            }
            else
            {
                CachedPackage.Bookings.Add(booking);
            }
        }

        // Merge Documents
        foreach (var doc in delta.Documents)
        {
            var existingIndex = CachedPackage.Documents.FindIndex(d => d.Id == doc.Id);
            if (existingIndex >= 0)
            {
                CachedPackage.Documents[existingIndex] = doc;
            }
            else
            {
                CachedPackage.Documents.Add(doc);
            }
        }

        // Merge Notifications
        foreach (var notif in delta.Notifications)
        {
            var existingIndex = CachedPackage.Notifications.FindIndex(n => n.Id == notif.Id);
            if (existingIndex >= 0)
            {
                CachedPackage.Notifications[existingIndex] = notif;
            }
            else
            {
                CachedPackage.Notifications.Add(notif);
            }
        }

        CachedPackage.SyncTimestamp = delta.SyncTimestamp;
    }
}

