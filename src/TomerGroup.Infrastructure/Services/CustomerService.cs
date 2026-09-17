using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly TomerDbContext _context;

    public CustomerService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
        {
            return ApiResponse<CustomerDto>.Fail("Customer not found");
        }

        // Strict Customer Isolation Guard (Section 34)
        if (requestingRole == "Customer" && customer.UserId != requestingUserId)
        {
            return ApiResponse<CustomerDto>.Fail("Access denied: You are not authorized to access another customer's data");
        }

        var isStaff = requestingRole != "Customer";
        var dto = MapToDto(customer, includeNotes: isStaff);
        return ApiResponse<CustomerDto>.Ok(dto);
    }

    public async Task<ApiResponse<CustomerSensitiveDetailsDto>> GetSensitiveDetailsAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
        {
            return ApiResponse<CustomerSensitiveDetailsDto>.Fail("Customer not found");
        }

        // Authorization: Only the customer themselves OR authorized staff (Admin, Manager, Sales, Operations)
        var isAuthorizedStaff = requestingRole is "Admin" or "Manager" or "Sales" or "Operations";
        var isSelf = customer.UserId == requestingUserId;

        if (!isSelf && !isAuthorizedStaff)
        {
            return ApiResponse<CustomerSensitiveDetailsDto>.Fail("Access denied: You do not have permission to view sensitive identity data.");
        }

        // Section 43: Audit Logging for sensitive data access
        var audit = new AuditLog
        {
            UserId = requestingUserId,
            Action = "ViewSensitiveCustomerData",
            EntityName = "Customer",
            EntityId = customer.Id.ToString(),
            MetadataJson = $"{{\"action\":\"ViewSensitivePassport\",\"customer\":\"{customer.FirstName} {customer.LastName}\",\"customerId\":\"{customer.Id}\"}}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var sensitiveDto = new CustomerSensitiveDetailsDto
        {
            CustomerId = customer.Id,
            FullName = $"{customer.FirstName} {customer.LastName}".Trim(),
            HebrewName = customer.HebrewName,
            PassportName = customer.PassportName,
            PassportNumber = customer.PassportNumber ?? string.Empty,
            PassportExpiration = customer.PassportExpiration,
            IsPassportExpiringSoon = customer.IsPassportExpiringSoon,
            DateOfBirth = customer.DateOfBirth,
            IsraelIdNumber = customer.IsraelIdNumber ?? string.Empty,
            MedicalNotes = customer.MedicalNotes,
            InsuranceCompany = customer.InsuranceCompany,
            InsurancePolicyNumber = customer.InsurancePolicyNumber
        };

        return ApiResponse<CustomerSensitiveDetailsDto>.Ok(sensitiveDto);
    }

    public async Task<ApiResponse<CustomerDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (customer == null)
        {
            return ApiResponse<CustomerDto>.Fail("Customer profile not found for the specified user");
        }

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer, includeNotes: false));
    }

    public async Task<ApiResponse<PagedResult<CustomerDto>>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                c.HebrewName.ToLower().Contains(search) ||
                c.PassportName.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                c.Phone.Contains(search) ||
                c.WhatsApp.Contains(search) ||
                (c.PassportNumber != null && c.PassportNumber.ToLower().Contains(search)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c, true))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<CustomerDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<CustomerDto>>.Ok(result);
    }

    public async Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetSummariesAsync(CustomerSearchFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Include(c => c.Trips)
            .Include(c => c.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                c.HebrewName.ToLower().Contains(search) ||
                c.PassportName.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                c.Phone.Contains(search) ||
                c.WhatsApp.Contains(search) ||
                (c.PassportNumber != null && c.PassportNumber.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            query = query.Where(c => c.Country.ToLower() == filter.Country.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filter.DietaryPreference))
        {
            var diet = filter.DietaryPreference.Trim().ToLower();
            query = query.Where(c => c.DietaryPreferences != null && c.DietaryPreferences.ToLower().Contains(diet));
        }

        if (filter.IsActiveInPeru.HasValue)
        {
            query = query.Where(c => c.IsActiveInPeru == filter.IsActiveInPeru.Value);
        }

        if (filter.HasExpiringPassport.HasValue && filter.HasExpiringPassport.Value)
        {
            var threshold = DateTime.UtcNow.AddMonths(6);
            query = query.Where(c => c.PassportExpiration.HasValue && c.PassportExpiration.Value <= threshold);
        }

        var total = await query.CountAsync(cancellationToken);
        var customers = await query
            .OrderByDescending(c => c.IsActiveInPeru)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var summaries = customers.Select(c => new CustomerSummaryDto
        {
            Id = c.Id,
            UserId = c.UserId,
            FirstName = c.FirstName,
            LastName = c.LastName,
            HebrewName = c.HebrewName,
            Phone = c.Phone,
            WhatsApp = c.WhatsApp,
            Email = c.Email,
            Country = c.Country,
            MaskedPassportNumber = c.MaskedPassportNumber,
            IsActiveInPeru = c.IsActiveInPeru,
            DietaryPreferences = c.DietaryPreferences,
            ActiveTripsCount = c.Trips.Count(t => t.Status == TripStatus.InProgress || t.Status == TripStatus.Confirmed),
            TotalBookingsCount = c.Bookings.Count
        }).ToList();

        var result = new PagedResult<CustomerSummaryDto>
        {
            Items = summaries,
            TotalCount = total,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponse<PagedResult<CustomerSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerDto request, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            HebrewName = request.HebrewName,
            PassportName = request.PassportName,
            Phone = request.Phone,
            WhatsApp = request.WhatsApp,
            Email = request.Email,
            Country = request.Country,
            PassportNumber = request.PassportNumber,
            PassportExpiration = request.PassportExpiration,
            DateOfBirth = request.DateOfBirth,
            IsraelIdNumber = request.IsraelIdNumber,
            MedicalNotes = request.MedicalNotes,
            InsuranceCompany = request.InsuranceCompany,
            InsurancePolicyNumber = request.InsurancePolicyNumber,
            IsActiveInPeru = request.IsActiveInPeru,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            SpecialRequests = request.SpecialRequests,
            DietaryPreferences = request.DietaryPreferences,
            Notes = request.Notes
        };

        await _context.Customers.AddAsync(customer, cancellationToken);

        var audit = new AuditLog
        {
            UserId = creatorId,
            Action = "CustomerCreate",
            EntityName = "Customer",
            EntityId = customer.Id.ToString(),
            MetadataJson = $"{{\"action\":\"CreateCustomer\",\"email\":\"{customer.Email}\",\"name\":\"{customer.FirstName} {customer.LastName}\"}}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer, true), "Customer created successfully");
    }

    public async Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerDto request, Guid editorId, string editorRole = "Staff", CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<CustomerDto>.Fail("Customer not found");
        }

        // Isolation Guard
        if (editorRole == "Customer" && customer.UserId != editorId)
        {
            return ApiResponse<CustomerDto>.Fail("Access denied: You cannot edit another customer's profile.");
        }

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.HebrewName = request.HebrewName;
        customer.PassportName = request.PassportName;
        customer.Phone = request.Phone;
        customer.WhatsApp = request.WhatsApp;
        customer.Email = request.Email;
        customer.Country = request.Country;
        customer.PassportNumber = request.PassportNumber ?? customer.PassportNumber;
        customer.PassportExpiration = request.PassportExpiration ?? customer.PassportExpiration;
        customer.DateOfBirth = request.DateOfBirth ?? customer.DateOfBirth;
        customer.IsraelIdNumber = request.IsraelIdNumber ?? customer.IsraelIdNumber;
        customer.MedicalNotes = request.MedicalNotes ?? customer.MedicalNotes;
        customer.InsuranceCompany = request.InsuranceCompany ?? customer.InsuranceCompany;
        customer.InsurancePolicyNumber = request.InsurancePolicyNumber ?? customer.InsurancePolicyNumber;
        customer.EmergencyContactName = request.EmergencyContactName;
        customer.EmergencyContactPhone = request.EmergencyContactPhone;
        customer.SpecialRequests = request.SpecialRequests;
        customer.DietaryPreferences = request.DietaryPreferences;

        // Staff-only fields: Customers cannot modify their own internal agency notes or active status
        if (editorRole != "Customer")
        {
            customer.Notes = request.Notes;
            customer.IsActiveInPeru = request.IsActiveInPeru;
        }

        customer.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            UserId = editorId,
            Action = "CustomerUpdate",
            EntityName = "Customer",
            EntityId = customer.Id.ToString(),
            MetadataJson = $"{{\"action\":\"UpdateCustomer\",\"editorRole\":\"{editorRole}\",\"customerId\":\"{customer.Id}\"}}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var isStaff = editorRole != "Customer";
        return ApiResponse<CustomerDto>.Ok(MapToDto(customer, isStaff), "Customer updated successfully");
    }

    public async Task<ApiResponse<CustomerDto>> UpdateOwnProfileAsync(Guid userId, UpdateCustomerProfileDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<CustomerDto>.Fail("Customer profile not found");
        }

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.HebrewName = request.HebrewName;
        customer.PassportName = request.PassportName;
        customer.Phone = request.Phone;
        customer.WhatsApp = request.WhatsApp;
        customer.Email = request.Email;
        customer.Country = request.Country;
        
        if (!string.IsNullOrWhiteSpace(request.PassportNumber))
            customer.PassportNumber = request.PassportNumber;
        if (request.PassportExpiration.HasValue)
            customer.PassportExpiration = request.PassportExpiration;
        if (request.DateOfBirth.HasValue)
            customer.DateOfBirth = request.DateOfBirth;
        if (!string.IsNullOrWhiteSpace(request.IsraelIdNumber))
            customer.IsraelIdNumber = request.IsraelIdNumber;

        customer.EmergencyContactName = request.EmergencyContactName;
        customer.EmergencyContactPhone = request.EmergencyContactPhone;
        customer.SpecialRequests = request.SpecialRequests;
        customer.DietaryPreferences = request.DietaryPreferences;
        customer.MedicalNotes = request.MedicalNotes;
        customer.InsuranceCompany = request.InsuranceCompany;
        customer.InsurancePolicyNumber = request.InsurancePolicyNumber;
        customer.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            UserId = userId,
            Action = "CustomerSelfUpdate",
            EntityName = "Customer",
            EntityId = customer.Id.ToString(),
            MetadataJson = $"{{\"action\":\"SelfUpdateProfile\",\"email\":\"{customer.Email}\"}}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer, false), "Profile updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .Include(c => c.Trips)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer == null)
        {
            return ApiResponse<bool>.Fail("Customer not found");
        }

        // Section 54: Safety check — do not delete customers with active or upcoming trips
        var hasActiveTrips = customer.Trips.Any(t => t.Status == TripStatus.InProgress || t.Status == TripStatus.Confirmed || t.Status == TripStatus.Draft);
        if (hasActiveTrips)
        {
            return ApiResponse<bool>.Fail("Cannot delete traveler who has active, upcoming, or confirmed trips.");
        }

        var hasUnpaidBookings = customer.Bookings.Any(b => b.PaymentStatus != PaymentStatus.Paid && b.Status != BookingStatus.Cancelled);
        if (hasUnpaidBookings)
        {
            return ApiResponse<bool>.Fail("Cannot delete traveler who has open or unpaid bookings.");
        }

        // Soft deletion
        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            UserId = deletedBy,
            Action = "CustomerDelete",
            EntityName = "Customer",
            EntityId = customer.Id.ToString(),
            MetadataJson = $"{{\"action\":\"SoftDeleteCustomer\",\"customer\":\"{customer.FirstName} {customer.LastName}\",\"customerId\":\"{customer.Id}\"}}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Customer deleted successfully");
    }

    public async Task<ApiResponse<CustomerStatsDto>> GetTravelerStatsAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _context.Customers.AsNoTracking().ToListAsync(cancellationToken);
        var total = customers.Count;
        var activeInPeru = customers.Count(c => c.IsActiveInPeru);
        
        var israeliCount = customers.Count(c =>
            string.Equals(c.Country, "Israel", StringComparison.OrdinalIgnoreCase) ||
            c.Phone.StartsWith("+972") ||
            c.WhatsApp.StartsWith("+972"));

        var kosherCount = customers.Count(c =>
            c.DietaryPreferences != null &&
            (c.DietaryPreferences.Contains("Kosher", StringComparison.OrdinalIgnoreCase) ||
             c.DietaryPreferences.Contains("כשר")));

        var vegCount = customers.Count(c =>
            c.DietaryPreferences != null &&
            (c.DietaryPreferences.Contains("Vegetarian", StringComparison.OrdinalIgnoreCase) ||
             c.DietaryPreferences.Contains("Vegan", StringComparison.OrdinalIgnoreCase) ||
             c.DietaryPreferences.Contains("צמחוני") ||
             c.DietaryPreferences.Contains("טבעוני")));

        var threshold = DateTime.UtcNow.AddMonths(6);
        var expiringPassports = customers.Count(c =>
            c.PassportExpiration.HasValue && c.PassportExpiration.Value <= threshold);

        var activeBookings = await _context.Bookings
            .AsNoTracking()
            .CountAsync(b => b.Status == BookingStatus.Confirmed, cancellationToken);

        var stats = new CustomerStatsDto
        {
            TotalTravelers = total,
            ActiveInPeru = activeInPeru,
            IsraeliTravelersCount = israeliCount,
            IsraeliTravelersPercentage = total > 0 ? Math.Round((double)israeliCount / total * 100, 1) : 0,
            KosherTravelersCount = kosherCount,
            VegetarianVeganCount = vegCount,
            ExpiringPassportCount = expiringPassports,
            TotalActiveBookings = activeBookings
        };

        return ApiResponse<CustomerStatsDto>.Ok(stats);
    }

    private static CustomerDto MapToDto(Customer c, bool includeNotes) => new()
    {
        Id = c.Id,
        UserId = c.UserId,
        FirstName = c.FirstName,
        LastName = c.LastName,
        HebrewName = c.HebrewName,
        PassportName = c.PassportName,
        Phone = c.Phone,
        WhatsApp = c.WhatsApp,
        Email = c.Email,
        Country = c.Country,
        MaskedPassportNumber = c.MaskedPassportNumber,
        MaskedIsraelId = c.MaskedIsraelId,
        PassportExpiration = c.PassportExpiration,
        IsPassportExpiringSoon = c.IsPassportExpiringSoon,
        DateOfBirth = c.DateOfBirth,
        MedicalNotes = c.MedicalNotes,
        InsuranceCompany = c.InsuranceCompany,
        InsurancePolicyNumber = c.InsurancePolicyNumber,
        IsActiveInPeru = c.IsActiveInPeru,
        EmergencyContactName = c.EmergencyContactName,
        EmergencyContactPhone = c.EmergencyContactPhone,
        SpecialRequests = c.SpecialRequests,
        DietaryPreferences = c.DietaryPreferences,
        Notes = includeNotes ? c.Notes : null
    };
}
