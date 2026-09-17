using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class GuideService : IGuideService
{
    private readonly TomerDbContext _context;

    public GuideService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<GuideDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var guides = await _context.Guides
            .AsNoTracking()
            .Where(g => !g.IsDeleted)
            .OrderBy(g => g.FullName)
            .Select(g => MapToDto(g))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<GuideDto>>.Ok(guides);
    }

    public async Task<ApiResponse<GuideDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var guide = await _context.Guides
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (guide == null)
        {
            return ApiResponse<GuideDto>.Fail("Guide not found");
        }

        return ApiResponse<GuideDto>.Ok(MapToDto(guide));
    }

    public async Task<ApiResponse<GuideDto>> CreateAsync(CreateGuideDto request, CancellationToken cancellationToken = default)
    {
        var guide = new Guide
        {
            FullName = request.FullName,
            HebrewName = request.HebrewName,
            Phone = request.Phone,
            WhatsApp = request.WhatsApp,
            Email = request.Email,
            CertificationNumber = request.CertificationNumber,
            DailyRate = request.DailyRate,
            Currency = request.Currency,
            FirstAidCertified = request.FirstAidCertified,
            IsJewishHeritageExpert = request.IsJewishHeritageExpert,
            Languages = request.Languages,
            Specialization = request.Specialization,
            EmergencyContact = request.EmergencyContact,
            IsAvailable = request.IsAvailable,
            Notes = request.Notes
        };

        await _context.Guides.AddAsync(guide, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<GuideDto>.Ok(MapToDto(guide), "Guide created successfully");
    }

    public async Task<ApiResponse<GuideDto>> UpdateAsync(Guid id, UpdateGuideDto request, CancellationToken cancellationToken = default)
    {
        var guide = await _context.Guides.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (guide == null)
        {
            return ApiResponse<GuideDto>.Fail("Guide not found");
        }

        guide.FullName = request.FullName;
        guide.HebrewName = request.HebrewName;
        guide.Phone = request.Phone;
        guide.WhatsApp = request.WhatsApp;
        guide.Email = request.Email;
        guide.CertificationNumber = request.CertificationNumber;
        guide.DailyRate = request.DailyRate;
        guide.Currency = request.Currency;
        guide.Rating = request.Rating;
        guide.FirstAidCertified = request.FirstAidCertified;
        guide.IsJewishHeritageExpert = request.IsJewishHeritageExpert;
        guide.Languages = request.Languages;
        guide.Specialization = request.Specialization;
        guide.EmergencyContact = request.EmergencyContact;
        guide.IsAvailable = request.IsAvailable;
        guide.Notes = request.Notes;
        guide.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<GuideDto>.Ok(MapToDto(guide), "Guide updated successfully");
    }

    public async Task<ApiResponse<GuideDailyManifestDto>> GetDailyManifestAsync(Guid guideId, DateTime date, CancellationToken cancellationToken = default)
    {
        var guide = await _context.Guides.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guideId, cancellationToken);
        if (guide == null)
        {
            return ApiResponse<GuideDailyManifestDto>.Fail("Guide not found");
        }

        var targetDate = date.Date;

        var activities = await _context.Activities
            .AsNoTracking()
            .Include(a => a.TripDay)
                .ThenInclude(td => td!.Trip)
                    .ThenInclude(t => t!.Customer)
            .Where(a => a.GuideId == guideId && a.TripDay != null && a.TripDay.Date.Date == targetDate)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);

        var manifest = new GuideDailyManifestDto
        {
            GuideId = guide.Id,
            GuideName = guide.FullName,
            Date = targetDate,
            Assignments = activities.Select(a =>
            {
                var customer = a.TripDay?.Trip?.Customer;
                return new GuideManifestItemDto
                {
                    ActivityId = a.Id,
                    Title = a.Title,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Location = a.Location,
                    Instructions = a.Instructions,
                    CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : string.Empty,
                    CustomerPhone = customer?.Phone ?? string.Empty,
                    CustomerWhatsApp = customer?.WhatsApp ?? string.Empty,
                    DietaryRestrictions = customer?.DietaryPreferences,
                    MedicalNotes = customer?.MedicalNotes,
                    Status = a.Status
                };
            }).ToList()
        };

        return ApiResponse<GuideDailyManifestDto>.Ok(manifest);
    }

    public async Task<ApiResponse<bool>> AssignGuideToActivityAsync(AssignGuideToActivityDto request, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var activity = await _context.Activities
            .Include(a => a.TripDay)
            .FirstOrDefaultAsync(a => a.Id == request.ActivityId, cancellationToken);

        if (activity == null)
        {
            return ApiResponse<bool>.Fail("Activity not found");
        }

        var guide = await _context.Guides.FirstOrDefaultAsync(g => g.Id == request.GuideId, cancellationToken);
        if (guide == null)
        {
            return ApiResponse<bool>.Fail("Guide not found");
        }

        if (!guide.IsAvailable)
        {
            return ApiResponse<bool>.Fail("Guide is currently marked as unavailable");
        }

        // Conflict detection: verify guide has no overlapping activities on that same date
        if (activity.TripDay != null)
        {
            var activityDate = activity.TripDay.Date.Date;
            var conflictingActivities = await _context.Activities
                .AsNoTracking()
                .Include(a => a.TripDay)
                .Where(a => a.GuideId == request.GuideId &&
                            a.Id != request.ActivityId &&
                            a.TripDay != null &&
                            a.TripDay.Date.Date == activityDate &&
                            a.Status != Core.Enums.ActivityStatus.Cancelled)
                .ToListAsync(cancellationToken);

            foreach (var existing in conflictingActivities)
            {
                // Check time overlap: (StartA < EndB) and (EndA > StartB)
                if (activity.StartTime < existing.EndTime && activity.EndTime > existing.StartTime)
                {
                    return ApiResponse<bool>.Fail($"Schedule conflict: Guide {guide.FullName} is already assigned to '{existing.Title}' ({existing.StartTime:hh\\:mm}-{existing.EndTime:hh\\:mm}) on {activityDate:yyyy-MM-dd}");
                }
            }
        }

        activity.GuideId = request.GuideId;
        activity.Guide = guide;
        activity.UpdatedAt = DateTime.UtcNow;

        // Log audit
        var audit = new AuditLog
        {
            Action = "AssignGuideToActivity",
            EntityName = "Activity",
            EntityId = activity.Id.ToString(),
            UserId = requestingUserId,
            UserEmail = "staff@tomergroup.com",
            MetadataJson = $"Assigned guide {guide.FullName} ({guide.CertificationNumber}) to activity '{activity.Title}'"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, $"Guide {guide.FullName} assigned successfully");
    }

    private static GuideDto MapToDto(Guide g) => new()
    {
        Id = g.Id,
        FullName = g.FullName,
        HebrewName = g.HebrewName,
        Phone = g.Phone,
        WhatsApp = g.WhatsApp,
        Email = g.Email,
        CertificationNumber = g.CertificationNumber,
        DailyRate = g.DailyRate,
        Currency = g.Currency,
        Rating = g.Rating,
        FirstAidCertified = g.FirstAidCertified,
        IsJewishHeritageExpert = g.IsJewishHeritageExpert,
        Languages = g.Languages,
        Specialization = g.Specialization,
        EmergencyContact = g.EmergencyContact,
        IsAvailable = g.IsAvailable,
        PhotoUrl = g.PhotoUrl,
        Notes = g.Notes
    };
}

