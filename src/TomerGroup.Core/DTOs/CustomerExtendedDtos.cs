namespace TomerGroup.Core.DTOs;

public class CustomerSensitiveDetailsDto
{
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string PassportName { get; set; } = string.Empty;

    // Unmasked Sensitive Fields (Requires authorized role or self, audit logged)
    public string PassportNumber { get; set; } = string.Empty;
    public DateTime? PassportExpiration { get; set; }
    public bool IsPassportExpiringSoon { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string IsraelIdNumber { get; set; } = string.Empty;

    public string? MedicalNotes { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? InsurancePolicyNumber { get; set; }
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string HebrewName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = "Israel";
    public string MaskedPassportNumber { get; set; } = string.Empty;
    public bool IsActiveInPeru { get; set; }
    public string? DietaryPreferences { get; set; }
    public int ActiveTripsCount { get; set; }
    public int TotalBookingsCount { get; set; }

    public string AvatarInitials
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
                return $"{FirstName[0]}{LastName[0]}".ToUpper();
            if (!string.IsNullOrWhiteSpace(FirstName))
                return $"{FirstName[0]}".ToUpper();
            if (!string.IsNullOrWhiteSpace(HebrewName))
                return $"{HebrewName[0]}";
            return "TG";
        }
    }
}

public class CustomerSearchFilterDto
{
    public string? Search { get; set; }
    public string? Country { get; set; }
    public string? DietaryPreference { get; set; }
    public bool? IsActiveInPeru { get; set; }
    public bool? HasExpiringPassport { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CustomerStatsDto
{
    public int TotalTravelers { get; set; }
    public int ActiveInPeru { get; set; }
    public int IsraeliTravelersCount { get; set; }
    public double IsraeliTravelersPercentage { get; set; }
    public int KosherTravelersCount { get; set; }
    public int VegetarianVeganCount { get; set; }
    public int ExpiringPassportCount { get; set; }
    public int TotalActiveBookings { get; set; }
}

public class UpdateCustomerProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string PassportName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = "Israel";
    public string? Language { get; set; }
    public string? ProfileImageUrl { get; set; }

    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiration { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? IsraelIdNumber { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? SpecialRequests { get; set; }
    public string? DietaryPreferences { get; set; }
    public string? MedicalNotes { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? InsurancePolicyNumber { get; set; }
}

