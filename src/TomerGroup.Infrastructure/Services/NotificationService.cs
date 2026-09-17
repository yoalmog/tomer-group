using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly TomerDbContext _context;

    public NotificationService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var list = await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => MapToDto(n))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<NotificationDto>>.Ok(list);
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted, cancellationToken);

        return ApiResponse<int>.Ok(count);
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted, cancellationToken);

        if (notification == null)
        {
            return ApiResponse<bool>.Fail("Notification not found");
        }

        if (notification.UserId != userId)
        {
            return ApiResponse<bool>.Fail("Access denied: You cannot modify another user's notification");
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "Notification marked as read");
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var n in unreadNotifications)
        {
            n.IsRead = true;
            n.ReadAt = now;
            n.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, $"{unreadNotifications.Count} notifications marked as read");
    }

    public async Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto request, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            HebrewTitle = string.IsNullOrWhiteSpace(request.HebrewTitle) ? request.Title : request.HebrewTitle,
            Message = request.Message,
            HebrewMessage = string.IsNullOrWhiteSpace(request.HebrewMessage) ? request.Message : request.HebrewMessage,
            Category = request.Category,
            ActionUrl = request.ActionUrl,
            IsRead = false
        };

        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<NotificationDto>.Ok(MapToDto(notification), "Notification created successfully");
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        Title = n.Title,
        HebrewTitle = string.IsNullOrEmpty(n.HebrewTitle) ? n.Title : n.HebrewTitle,
        Message = n.Message,
        HebrewMessage = string.IsNullOrEmpty(n.HebrewMessage) ? n.Message : n.HebrewMessage,
        Category = n.Category,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
        ActionUrl = n.ActionUrl
    };
}

