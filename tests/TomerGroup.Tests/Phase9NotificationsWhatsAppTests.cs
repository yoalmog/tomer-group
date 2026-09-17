using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase9NotificationsWhatsAppTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_Phase9_{Guid.NewGuid()}")
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task NotificationService_CreateAndGetUnreadNotifications_ReturnsCorrectListAndCount()
    {
        using var context = CreateInMemoryContext();
        var notificationService = new NotificationService(context);

        var userId = Guid.NewGuid();

        // Create 2 unread notifications and 1 read notification
        await notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = "Machu Picchu Permit Ready",
            HebrewTitle = "כרטיס כניסה למאצ'ו פיצ'ו מוכן",
            Message = "Your Circuit 2 permit is confirmed.",
            HebrewMessage = "אישור הכניסה שלך למסלול 2 אושר.",
            Category = "Permits"
        });

        await notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = "Driver Assigned",
            HebrewTitle = "הנהג שובץ",
            Message = "Driver Carlos is assigned.",
            HebrewMessage = "הנהג קרלוס שובץ להעברה.",
            Category = "Driver"
        });

        var n3 = new Notification
        {
            UserId = userId,
            Title = "Welcome to Cusco",
            HebrewTitle = "ברוכים הבאים לקוסקו",
            Message = "Welcome!",
            HebrewMessage = "ברוכים הבאים!",
            Category = "General",
            IsRead = true
        };
        await context.Notifications.AddAsync(n3);
        await context.SaveChangesAsync();

        // 1. Verify unread count is 2
        var unreadCountRes = await notificationService.GetUnreadCountAsync(userId);
        Assert.True(unreadCountRes.Success);
        Assert.Equal(2, unreadCountRes.Data);

        // 2. Verify GetUserNotificationsAsync with unreadOnly=true returns only 2
        var unreadListRes = await notificationService.GetUserNotificationsAsync(userId, unreadOnly: true);
        Assert.True(unreadListRes.Success);
        Assert.NotNull(unreadListRes.Data);
        Assert.Equal(2, unreadListRes.Data.Count);

        // 3. Verify GetUserNotificationsAsync with unreadOnly=false returns all 3
        var allListRes = await notificationService.GetUserNotificationsAsync(userId, unreadOnly: false);
        Assert.True(allListRes.Success);
        Assert.NotNull(allListRes.Data);
        Assert.Equal(3, allListRes.Data.Count);
    }

    [Fact]
    public async Task NotificationService_MarkAsRead_UpdatesIsReadAndReadAt()
    {
        using var context = CreateInMemoryContext();
        var notificationService = new NotificationService(context);

        var userId = Guid.NewGuid();
        var createRes = await notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = "Flight Reminder",
            Message = "LATAM flight LA2015 departs at 08:30."
        });

        Assert.True(createRes.Success);
        Assert.NotNull(createRes.Data);
        Assert.False(createRes.Data.IsRead);

        var markRes = await notificationService.MarkAsReadAsync(createRes.Data.Id, userId);
        Assert.True(markRes.Success);

        var updated = await context.Notifications.FindAsync(createRes.Data.Id);
        Assert.NotNull(updated);
        Assert.True(updated.IsRead);
        Assert.NotNull(updated.ReadAt);
    }

    [Fact]
    public async Task NotificationService_MarkAllAsRead_MarksAllUserNotificationsAsRead()
    {
        using var context = CreateInMemoryContext();
        var notificationService = new NotificationService(context);

        var userId = Guid.NewGuid();
        for (int i = 0; i < 3; i++)
        {
            await notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Title = $"Alert {i + 1}",
                Message = $"Message {i + 1}"
            });
        }

        var markAllRes = await notificationService.MarkAllAsReadAsync(userId);
        Assert.True(markAllRes.Success);

        var unreadCount = await notificationService.GetUnreadCountAsync(userId);
        Assert.Equal(0, unreadCount.Data);
    }

    [Fact]
    public async Task NotificationService_CustomerIsolation_CannotMarkOthersNotification()
    {
        using var context = CreateInMemoryContext();
        var notificationService = new NotificationService(context);

        var dannyUserId = Guid.NewGuid();
        var noamUserId = Guid.NewGuid();

        var createRes = await notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = dannyUserId,
            Title = "Danny's Private Alert",
            Message = "Confidential passport update"
        });

        Assert.True(createRes.Success);
        Assert.NotNull(createRes.Data);

        // Noam tries to mark Danny's notification
        var attemptRes = await notificationService.MarkAsReadAsync(createRes.Data.Id, noamUserId);
        Assert.False(attemptRes.Success);
        Assert.Contains("Access denied", attemptRes.Message);
    }

    [Fact]
    public void WhatsAppService_SanitizePhoneNumber_FormatsCorrectly()
    {
        using var context = CreateInMemoryContext();
        var waService = new WhatsAppService(context);

        // Israeli local with leading 0
        Assert.Equal("972541234567", waService.SanitizePhoneNumber("054-123-4567"));

        // Israeli local without leading 0
        Assert.Equal("972524448899", waService.SanitizePhoneNumber("524448899"));

        // Israeli with international plus and spaces
        Assert.Equal("972507773322", waService.SanitizePhoneNumber("+972 50 777 3322"));

        // Peruvian mobile with dashes
        Assert.Equal("51984555666", waService.SanitizePhoneNumber("984-555-666"));

        // Peruvian full international with +51
        Assert.Equal("51984231961", waService.SanitizePhoneNumber("+51 984 231 961"));
    }

    [Fact]
    public void WhatsAppService_GenerateWhatsAppUrl_EncodesHebrewTextProperly()
    {
        using var context = CreateInMemoryContext();
        var waService = new WhatsAppService(context);

        var url = waService.GenerateWhatsAppUrl("+972 54 123 4567", "שלום דני! שובר המלון מוכן.");

        Assert.StartsWith("https://wa.me/972541234567?text=", url);
        Assert.Contains(Uri.EscapeDataString("שלום דני!"), url);
    }

    [Fact]
    public void WhatsAppService_BuildTemplatedMessage_ReplacesPlaceholders()
    {
        using var context = CreateInMemoryContext();
        var waService = new WhatsAppService(context);

        var parameters = new Dictionary<string, string>
        {
            { "CustomerName", "דני כהן" },
            { "DriverName", "קרלוס קיספה" },
            { "PickupTime", "14:30" },
            { "PickupLocation", "שער 1 נמל התעופה קוסקו" },
            { "VehicleModel", "מרצדס ספרינטר" },
            { "PlateNumber", "X2Y-884" },
            { "DriverPhone", "+51 984 555 666" }
        };

        var message = waService.BuildTemplatedMessage("DriverPickupReminder", parameters, language: "he");

        Assert.NotEmpty(message);
        Assert.Contains("דני כהן", message);
        Assert.Contains("קרלוס קיספה", message);
        Assert.Contains("14:30", message);
        Assert.Contains("X2Y-884", message);
        Assert.DoesNotContain("{CustomerName}", message);
        Assert.DoesNotContain("{DriverName}", message);
    }

    [Fact]
    public async Task WhatsAppService_PrepareOrSendMessageAsync_LogsMessageRecordInDatabase()
    {
        using var context = CreateInMemoryContext();
        var waService = new WhatsAppService(context);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Danny",
            LastName = "Cohen",
            WhatsApp = "+972541234567"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var staffUserId = Guid.NewGuid();
        var request = new SendWhatsAppMessageDto
        {
            CustomerId = customer.Id,
            TemplateKey = "BookingConfirmation",
            Language = "he",
            TemplateParameters = new Dictionary<string, string>
            {
                { "CustomerName", "דני כהן" },
                { "BookingCode", "TG-2026-00482" },
                { "Destination", "קוסקו, עמק הקדוש ומאצ'ו פיצ'ו" },
                { "Dates", "18.09.2026 - 25.09.2026" }
            }
        };

        var result = await waService.PrepareOrSendMessageAsync(request, staffUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("972541234567", result.Data.RecipientPhone);
        Assert.Contains("TG-2026-00482", result.Data.Content);
        Assert.StartsWith("https://wa.me/972541234567?text=", result.Data.WhatsAppUrl);

        // Verify Message record logged in DB
        var loggedMessage = await context.Messages.FirstOrDefaultAsync(m => m.Id == result.Data.MessageId);
        Assert.NotNull(loggedMessage);
        Assert.Equal("WhatsApp", loggedMessage.Channel);
        Assert.Equal(staffUserId, loggedMessage.StaffUserId);
        Assert.True(loggedMessage.IsSent);
    }
}

