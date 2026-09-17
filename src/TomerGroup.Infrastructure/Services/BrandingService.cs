using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class BrandingService : IBrandingService
{
    private readonly TomerDbContext _context;

    public BrandingService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<BrandSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _context.BrandSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings == null)
        {
            settings = BrandSettings.CreateDefault();
            await _context.BrandSettings.AddAsync(settings, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return settings;
    }

    public async Task<BrandSettings> UpdateSettingsAsync(BrandSettings newSettings, CancellationToken cancellationToken = default)
    {
        var existing = await _context.BrandSettings.FirstOrDefaultAsync(cancellationToken);
        if (existing == null)
        {
            await _context.BrandSettings.AddAsync(newSettings, cancellationToken);
        }
        else
        {
            existing.AgencyName = newSettings.AgencyName;
            existing.AppName = newSettings.AppName;
            existing.Tagline = newSettings.Tagline;
            existing.LogoUrl = newSettings.LogoUrl;
            existing.PrimaryColor = newSettings.PrimaryColor;
            existing.SecondaryColor = newSettings.SecondaryColor;
            existing.AccentColor = newSettings.AccentColor;
            existing.ContactPhone = newSettings.ContactPhone;
            existing.WhatsApp = newSettings.WhatsApp;
            existing.Email = newSettings.Email;
            existing.Website = newSettings.Website;
            existing.Address = newSettings.Address;
            existing.EmergencyContact = newSettings.EmergencyContact;
            existing.SocialLinks = newSettings.SocialLinks;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return existing ?? newSettings;
    }
}

public class AuditService : IAuditService
{
    private readonly TomerDbContext _context;

    public AuditService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string action, string entityName, string? entityId, Guid? userId, string userEmail, string? metadata = null)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId,
            UserEmail = userEmail,
            MetadataJson = metadata,
            CreatedAt = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}

