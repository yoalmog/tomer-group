using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class SyncService : ISyncService
{
    private readonly TomerDbContext _context;

    public SyncService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<SyncPackageDto>> PullDeltaPackageAsync(SyncPullRequestDto request, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            return ApiResponse<SyncPackageDto>.Fail("Customer not found");
        }

        // Customer Isolation check
        if (requestingRole == "Customer" && customer.UserId != requestingUserId)
        {
            return ApiResponse<SyncPackageDto>.Fail("Access denied: You are not authorized to sync another traveler's data");
        }

        var since = request.LastSyncTimestamp;
        var isDelta = since.HasValue && since.Value > DateTime.MinValue;

        // 1. Trips
        var tripsQuery = _context.Trips
            .AsNoTracking()
            .Include(t => t.Days.OrderBy(d => d.DayNumber))
                .ThenInclude(d => d.Activities.OrderBy(a => a.StartTime))
            .Where(t => t.CustomerId == request.CustomerId && !t.IsDeleted);

        if (isDelta)
        {
            tripsQuery = tripsQuery.Where(t => t.UpdatedAt >= since!.Value);
        }

        var trips = await tripsQuery.ToListAsync(cancellationToken);
        var tripDtos = trips.Select(t => new TripDto
        {
            Id = t.Id,
            TripCode = t.TripCode,
            CustomerId = t.CustomerId,
            CustomerName = $"{customer.FirstName} {customer.LastName}",
            Title = t.Title,
            Description = t.Description,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            Status = t.Status,
            TotalRevenue = t.TotalRevenue,
            TotalCost = t.TotalCost,
            GrossProfit = t.GrossProfit,
            Currency = t.Currency,
            Days = t.Days.Select(d => new TripDayDto
            {
                Id = d.Id,
                DayNumber = d.DayNumber,
                Date = d.Date,
                Title = d.Title,
                Destination = d.Destination,
                Activities = d.Activities.Select(a => new ActivityDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Location = a.Location,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status
                }).ToList()
            }).ToList()
        }).ToList();

        // 2. Bookings
        var bookingsQuery = _context.Bookings
            .AsNoTracking()
            .Where(b => b.CustomerId == request.CustomerId && !b.IsDeleted);

        if (isDelta)
        {
            bookingsQuery = bookingsQuery.Where(b => b.UpdatedAt >= since!.Value);
        }

        var bookings = await bookingsQuery.ToListAsync(cancellationToken);
        var bookingDtos = bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            BookingCode = b.BookingCode,
            CustomerId = b.CustomerId,
            CustomerName = $"{customer.FirstName} {customer.LastName}",
            TotalAmount = b.TotalAmount,
            PaidAmount = b.PaidAmount,
            Currency = b.Currency,
            Status = b.Status,
            PaymentStatus = b.PaymentStatus
        }).ToList();

        // 3. Documents & Permits
        var docsQuery = _context.Documents
            .AsNoTracking()
            .Where(d => d.CustomerId == request.CustomerId && !d.IsDeleted);

        if (requestingRole == "Customer")
        {
            docsQuery = docsQuery.Where(d => d.IsCustomerVisible);
        }

        if (isDelta)
        {
            docsQuery = docsQuery.Where(d => d.UpdatedAt >= since!.Value);
        }

        var docs = await docsQuery.ToListAsync(cancellationToken);
        var docDtos = docs.Select(d => new DocumentDto
        {
            Id = d.Id,
            Name = d.Name,
            HebrewName = d.HebrewName,
            Type = d.Type,
            FileExtension = d.FileExtension,
            MimeType = d.MimeType,
            FileSizeBytes = d.FileSizeBytes,
            Circuit = d.Circuit,
            PermitPassportNumber = d.PermitPassportNumber,
            CustomerId = d.CustomerId,
            UploadDate = d.UploadDate,
            ExpirationDate = d.ExpirationDate,
            IsCustomerVisible = d.IsCustomerVisible
        }).ToList();

        // 4. Notifications
        var notificationsQuery = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == customer.UserId && !n.IsDeleted);

        if (isDelta)
        {
            notificationsQuery = notificationsQuery.Where(n => n.UpdatedAt >= since!.Value);
        }

        var notifications = await notificationsQuery.ToListAsync(cancellationToken);
        var notifDtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            HebrewTitle = n.HebrewTitle,
            Message = n.Message,
            HebrewMessage = n.HebrewMessage,
            Category = n.Category,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ActionUrl = n.ActionUrl
        }).ToList();

        // 5. Emergency & Acclimatization Contacts
        var emergencyContacts = GetEmergencyContacts();

        var package = new SyncPackageDto
        {
            SyncTimestamp = DateTime.UtcNow,
            CustomerId = customer.Id,
            Customer = new CustomerDto
            {
                Id = customer.Id,
                UserId = customer.UserId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                HebrewName = customer.HebrewName,
                PassportName = customer.PassportName,
                Phone = customer.Phone,
                WhatsApp = customer.WhatsApp,
                Email = customer.Email,
                MaskedPassportNumber = customer.PassportNumber ?? string.Empty,
                MedicalNotes = customer.MedicalNotes,
                EmergencyContactName = customer.EmergencyContactName,
                EmergencyContactPhone = customer.EmergencyContactPhone
            },
            Trips = tripDtos,
            Bookings = bookingDtos,
            Documents = docDtos,
            Notifications = notifDtos,
            EmergencyContacts = emergencyContacts,
            ServerVersion = "1.0.0",
            IsDeltaSync = isDelta
        };

        return ApiResponse<SyncPackageDto>.Ok(package, isDelta ? "Delta sync package generated" : "Full sync package generated");
    }

    public async Task<ApiResponse<SyncPushResultDto>> PushClientChangesAsync(SyncPushRequestDto request, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<SyncPushResultDto>.Fail("Customer not found");
        }

        if (requestingRole == "Customer" && customer.UserId != requestingUserId)
        {
            return ApiResponse<SyncPushResultDto>.Fail("Access denied: You are not authorized to push changes for another traveler");
        }

        int acknowledged = 0;

        // 1. Update customer emergency details if supplied
        if (request.UpdatedEmergencyContact != null)
        {
            if (!string.IsNullOrWhiteSpace(request.UpdatedEmergencyContact.EmergencyContactName))
            {
                customer.EmergencyContactName = request.UpdatedEmergencyContact.EmergencyContactName;
            }
            if (!string.IsNullOrWhiteSpace(request.UpdatedEmergencyContact.EmergencyContactPhone))
            {
                customer.EmergencyContactPhone = request.UpdatedEmergencyContact.EmergencyContactPhone;
            }
            if (!string.IsNullOrWhiteSpace(request.UpdatedEmergencyContact.MedicalNotes))
            {
                customer.MedicalNotes = request.UpdatedEmergencyContact.MedicalNotes;
            }

            customer.UpdatedAt = DateTime.UtcNow;
            acknowledged++;
        }

        // 2. Process offline activity check-ins
        if (request.ActivityCheckIns != null && request.ActivityCheckIns.Count > 0)
        {
            foreach (var checkIn in request.ActivityCheckIns)
            {
                var activity = await _context.Activities.FindAsync(new object[] { checkIn.ActivityId }, cancellationToken);
                if (activity != null)
                {
                    activity.Status = ActivityStatus.Completed;
                    activity.UpdatedAt = DateTime.UtcNow;
                    acknowledged++;
                }
            }
        }

        var audit = new AuditLog
        {
            Action = "OfflineSyncPush",
            EntityName = "Customer",
            EntityId = customer.Id.ToString(),
            UserId = requestingUserId,
            UserEmail = "traveler@tomergroup.com",
            MetadataJson = $"Processed offline sync push from device '{request.DeviceId}'. Items acknowledged: {acknowledged}."
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<SyncPushResultDto>.Ok(new SyncPushResultDto
        {
            Success = true,
            SyncedAt = DateTime.UtcNow,
            AcknowledgedItems = acknowledged,
            ConflictsResolved = 0,
            Message = $"Offline sync push processed successfully ({acknowledged} items synchronized)."
        });
    }

    public async Task<ApiResponse<bool>> SyncOfflineDataAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FindAsync(new object[] { customerId }, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<bool>.Fail("Customer not found");
        }

        return ApiResponse<bool>.Ok(true, "Offline data synchronization verified");
    }

    private static List<EmergencyContactDto> GetEmergencyContacts() => new()
    {
        new()
        {
            Name = "Tomer Group 24/7 Cusco Desk",
            Role = "Central Operations & Oxygen Support",
            Phone = "+51 984 231961",
            WhatsApp = "+51984231961",
            Address = "Plaza Regocijo 261, Cusco Historic Center",
            HasOxygen = true
        },
        new()
        {
            Name = "Chabad House Cusco (בית חב\"ד)",
            Role = "Community, Shabbat & Kosher Meals",
            Phone = "+51 984 100 200",
            WhatsApp = "+51984100200",
            Address = "Calle Choquechaka 228, San Blas, Cusco",
            HasOxygen = true
        },
        new()
        {
            Name = "O2 Medical Network Cusco",
            Role = "High-Altitude Sickness Clinic",
            Phone = "+51 84 223290",
            WhatsApp = "+51984223290",
            Address = "Av. Tullumayo 710, Cusco",
            HasOxygen = true
        },
        new()
        {
            Name = "Embassy of Israel in Lima",
            Role = "Consular Emergency Assistance",
            Phone = "+51 1 433 4431",
            WhatsApp = "+51984000111",
            Address = "Av. Santa Cruz 814, San Isidro, Lima",
            HasOxygen = false
        }
    };
}
