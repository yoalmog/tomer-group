namespace TomerGroup.Core.Models;

public class Customer : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// The unique identifier of the authenticated user in Supabase Auth (auth.users.id).
    /// </summary>
    public string AuthUserId { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string PassportName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = "Israel";
    public string Language { get; set; } = "he";
    public string? ProfileImageUrl { get; set; }
    public string Status { get; set; } = "Active";

    // Sensitive identity fields (encrypted/protected in API responses)
    public string? PassportNumber { get; set; }
    public DateTime? PassportExpiration { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public string? IsraelIdNumber { get; set; } // Teudat Zehut (encrypted/protected)
    public string? MedicalNotes { get; set; } // Altitude sickness (Soroche), allergies, medications
    public string? InsuranceCompany { get; set; } // Travel/Rescue Insurance
    public string? InsurancePolicyNumber { get; set; }
    public bool IsActiveInPeru { get; set; } = false;

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? SpecialRequests { get; set; }
    public string? DietaryPreferences { get; set; } // e.g., Kosher Mehudar, Vegetarian, Vegan, Gluten-Free
    public string? Notes { get; set; } // Agency internal notes (never exposed to customer)

    public string MaskedPassportNumber
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PassportNumber)) return string.Empty;
            if (PassportNumber.Length <= 4) return "****";

            var dashIndex = PassportNumber.IndexOf('-');
            if (dashIndex > 0 && dashIndex < PassportNumber.Length - 4)
            {
                var prefix = PassportNumber[..(dashIndex + 1)];
                var starsCount = PassportNumber.Length - prefix.Length - 4;
                return prefix + new string('*', Math.Max(starsCount, 4)) + PassportNumber[^4..];
            }

            return new string('*', PassportNumber.Length - 4) + PassportNumber[^4..];
        }
    }

    public string MaskedIsraelId => string.IsNullOrWhiteSpace(IsraelIdNumber)
        ? string.Empty
        : IsraelIdNumber.Length > 3
            ? new string('*', IsraelIdNumber.Length - 3) + IsraelIdNumber[^3..]
            : "***";

    public bool IsPassportExpiringSoon => PassportExpiration.HasValue &&
        PassportExpiration.Value <= DateTime.UtcNow.AddMonths(6);

    // Navigation collections
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

