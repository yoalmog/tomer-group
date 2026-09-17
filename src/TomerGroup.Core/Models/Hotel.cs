using TomerGroup.Core.Enums;

namespace TomerGroup.Core.Models;

public class Hotel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty; // Cusco, Urubamba, Aguas Calientes, Lima, etc.
    public int Stars { get; set; } = 4;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Website { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public List<string> RoomTypes { get; set; } = new(); // Standard, Superior, Suite, Family
    // Altitude & Comfort Features
    public bool HasOxygenEnrichedRooms { get; set; } = false;
    public bool HasOxygenConcentrators { get; set; } = false;
    public bool HasHeating { get; set; } = true;

    // Israeli / Kosher Features
    public bool IsKosherFriendly { get; set; } = true;
    public bool ShabbatFriendly { get; set; } = true; // Manual metal keys, lower floor options
    public bool WalkingDistanceToChabadCusco { get; set; } = false;

    public List<string> PhotoUrls { get; set; } = new();
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class HotelBooking : BaseEntity
{
    public Guid BookingId { get; set; }
    public Booking? Booking { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public string RoomType { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; } = 2;
    public int RoomCount { get; set; } = 1;
    public bool OxygenRoomRequested { get; set; } = false;

    public decimal AgencyCost { get; set; }
    public decimal SellingPrice { get; set; }
    public Currency Currency { get; set; } = Currency.USD;

    public string Status { get; set; } = "Confirmed";
    public string? ConfirmationNumber { get; set; }
    public string? SpecialRequests { get; set; }
    public string? Notes { get; set; }
}

