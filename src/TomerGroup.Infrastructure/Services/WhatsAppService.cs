using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly TomerDbContext _context;

    public WhatsAppService(TomerDbContext context)
    {
        _context = context;
    }

    public string SanitizePhoneNumber(string phone, string defaultCountryCode = "+972")
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        // Strip non-digit characters except leading '+'
        var trimmed = phone.Trim();
        var hasPlus = trimmed.StartsWith("+");
        var digitsOnly = Regex.Replace(trimmed, @"[^\d]", "");

        if (string.IsNullOrEmpty(digitsOnly))
        {
            return string.Empty;
        }

        var defaultCodeDigits = Regex.Replace(defaultCountryCode, @"[^\d]", "");

        // If local Israeli number (e.g. 0541234567 or 541234567)
        if (digitsOnly.StartsWith("05") && digitsOnly.Length == 10)
        {
            return "972" + digitsOnly.Substring(1);
        }

        // If 9 digits starting with 5 (Israeli mobile without leading 0)
        if (digitsOnly.Length == 9 && (digitsOnly.StartsWith("50") || digitsOnly.StartsWith("52") || digitsOnly.StartsWith("53") || digitsOnly.StartsWith("54") || digitsOnly.StartsWith("55") || digitsOnly.StartsWith("58")))
        {
            return "972" + digitsOnly;
        }

        // If Peruvian local mobile (9 digits starting with 9, e.g. 984123456)
        if (digitsOnly.Length == 9 && digitsOnly.StartsWith("9"))
        {
            return "51" + digitsOnly;
        }

        // Already has international prefix
        if (digitsOnly.StartsWith("972") || digitsOnly.StartsWith("51") || digitsOnly.StartsWith("1"))
        {
            return digitsOnly;
        }

        // Prepend default country code if missing
        return defaultCodeDigits + digitsOnly;
    }

    public string GenerateWhatsAppUrl(string phone, string message)
    {
        var cleanPhone = SanitizePhoneNumber(phone);
        var encodedText = Uri.EscapeDataString(message ?? string.Empty);
        return $"https://wa.me/{cleanPhone}?text={encodedText}";
    }

    public string BuildTemplatedMessage(string templateKey, Dictionary<string, string> parameters, string language = "he")
    {
        var template = GetDefaultTemplate(templateKey, language);
        if (template == null)
        {
            return string.Empty;
        }

        var result = template;
        foreach (var kv in parameters)
        {
            result = result.Replace($"{{{kv.Key}}}", kv.Value);
        }

        return result;
    }

    public async Task<ApiResponse<WhatsAppDispatchResultDto>> PrepareOrSendMessageAsync(SendWhatsAppMessageDto request, Guid? staffUserId = null, CancellationToken cancellationToken = default)
    {
        var phone = request.Phone;
        Customer? customer = null;

        if (request.CustomerId.HasValue && request.CustomerId != Guid.Empty)
        {
            customer = await _context.Customers.FindAsync(new object[] { request.CustomerId.Value }, cancellationToken);
            if (string.IsNullOrWhiteSpace(phone) && customer != null)
            {
                phone = string.IsNullOrWhiteSpace(customer.WhatsApp) ? customer.Phone : customer.WhatsApp;
            }
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return ApiResponse<WhatsAppDispatchResultDto>.Fail("Recipient phone number is required");
        }

        string content;
        if (!string.IsNullOrWhiteSpace(request.TemplateKey))
        {
            content = BuildTemplatedMessage(request.TemplateKey, request.TemplateParameters, request.Language);
            if (string.IsNullOrWhiteSpace(content))
            {
                content = request.CustomContent ?? string.Empty;
            }
        }
        else
        {
            content = request.CustomContent ?? string.Empty;
        }

        var cleanPhone = SanitizePhoneNumber(phone);
        var waUrl = GenerateWhatsAppUrl(cleanPhone, content);

        var messageEntity = new Message
        {
            CustomerId = request.CustomerId,
            StaffUserId = staffUserId,
            Channel = "WhatsApp",
            RecipientPhone = cleanPhone,
            Content = content,
            Language = request.Language,
            IsSent = true,
            SentAt = DateTime.UtcNow,
            Status = "Sent"
        };

        await _context.Messages.AddAsync(messageEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<WhatsAppDispatchResultDto>.Ok(new WhatsAppDispatchResultDto
        {
            RecipientPhone = cleanPhone,
            Content = content,
            WhatsAppUrl = waUrl,
            MessageId = messageEntity.Id,
            Status = "Sent"
        }, "WhatsApp message prepared and logged");
    }

    public async Task<ApiResponse<List<MessageTemplateDto>>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var dbTemplates = await _context.MessageTemplates
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .Select(t => new MessageTemplateDto
            {
                Id = t.Id,
                TemplateKey = t.TemplateKey,
                Name = t.Name,
                Language = t.Language,
                Category = t.Category,
                ContentPattern = t.ContentPattern
            })
            .ToListAsync(cancellationToken);

        if (dbTemplates.Count > 0)
        {
            return ApiResponse<List<MessageTemplateDto>>.Ok(dbTemplates);
        }

        // Return core operational templates
        var defaults = GetCoreTemplates();
        return ApiResponse<List<MessageTemplateDto>>.Ok(defaults);
    }

    public async Task<ApiResponse<List<MessageDto>>> GetMessageHistoryAsync(Guid? customerId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Messages
            .AsNoTracking()
            .Include(m => m.Customer)
            .Where(m => !m.IsDeleted);

        if (customerId.HasValue && customerId.Value != Guid.Empty)
        {
            query = query.Where(m => m.CustomerId == customerId.Value);
        }

        var list = await query
            .OrderByDescending(m => m.SentAt ?? m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                CustomerId = m.CustomerId,
                CustomerName = m.Customer != null ? $"{m.Customer.FirstName} {m.Customer.LastName}" : string.Empty,
                StaffUserId = m.StaffUserId,
                Channel = m.Channel,
                RecipientPhone = m.RecipientPhone,
                Content = m.Content,
                Language = m.Language,
                IsSent = m.IsSent,
                SentAt = m.SentAt,
                Status = m.Status
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<MessageDto>>.Ok(list);
    }

    private static string? GetDefaultTemplate(string key, string language)
    {
        var templates = GetCoreTemplates();
        return templates.FirstOrDefault(t => t.TemplateKey.Equals(key, StringComparison.OrdinalIgnoreCase) && t.Language.Equals(language, StringComparison.OrdinalIgnoreCase))?.ContentPattern;
    }

    private static List<MessageTemplateDto> GetCoreTemplates() => new()
    {
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "BookingConfirmation",
            Name = "אישור הזמנה (Booking Confirmation)",
            Language = "he",
            Category = "Bookings",
            ContentPattern = "שלום {CustomerName}, ההזמנה שלך {BookingCode} עם Tomer Group אושרה בהצלחה! היעד: {Destination}, תאריכים: {Dates}. צוות קוסקו לשירותכם 24/7: +51 984 231961."
        },
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "BookingConfirmation",
            Name = "Booking Confirmation",
            Language = "en",
            Category = "Bookings",
            ContentPattern = "Hello {CustomerName}, your booking {BookingCode} with Tomer Group is confirmed! Destination: {Destination}, Dates: {Dates}. Tomer Group Cusco desk: +51 984 231961."
        },
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "DriverPickupReminder",
            Name = "תזכורת איסוף נהג (Driver Pickup)",
            Language = "he",
            Category = "Transportation",
            ContentPattern = "היי {CustomerName}, הנהג שלך {DriverName} ימתין לך בשעה {PickupTime} במיקום: {PickupLocation}. רכב: {VehicleModel} (לוחית {PlateNumber}). טלפון נהג: {DriverPhone}."
        },
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "DriverPickupReminder",
            Name = "Driver Pickup Reminder",
            Language = "en",
            Category = "Transportation",
            ContentPattern = "Hi {CustomerName}, your driver {DriverName} will meet you at {PickupTime} at {PickupLocation}. Vehicle: {VehicleModel} (Plate {PlateNumber}). Driver phone: {DriverPhone}."
        },
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "MachuPicchuPermitIssued",
            Name = "הנפקת אישור מאצ'ו פיצ'ו (Machu Picchu Permit)",
            Language = "he",
            Category = "Permits",
            ContentPattern = "שלום {CustomerName}, כרטיס הכניסה למאצ'ו פיצ'ו הונפק! מסלול: {Circuit}, תאריך: {Date}, שעת כניסה: {EntrySlot}. חובה להציג דרכון מקורי מס' {PassportNumber} בכניסה!"
        },
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "PaymentReceipt",
            Name = "קבלה על תשלום (Payment Receipt)",
            Language = "he",
            Category = "Finance",
            ContentPattern = "תודה {CustomerName}, תשלום בסך {Amount} {Currency} התקבל בהצלחה (קבלה מס' {ReceiptNumber}). יתרה לתשלום: {RemainingBalance} {Currency}."
        },
        new()
        {
            Id = Guid.NewGuid(),
            TemplateKey = "EmergencySOS",
            Name = "התרעת חירום (Emergency Alert)",
            Language = "he",
            Category = "Emergency",
            ContentPattern = "קריאת חירום עבור המטייל {CustomerName} במיקום {Location}! סיוע רפואי/חילוץ של Tomer Group קוסקו הופעל. חמצן ורופא כונן בדרך."
        }
    };
}
