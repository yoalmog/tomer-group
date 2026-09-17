using TomerGroup.Core.Enums;

namespace TomerGroup.Core.Models;

public class Driver : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? LicenseNumber { get; set; }
    public string LicenseCategory { get; set; } = "A-IIIa Profesional";
    public DateTime? LicenseExpirationDate { get; set; }
    public double Rating { get; set; } = 4.9;
    public string? EmergencyContact { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Notes { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Transportation> Transportations { get; set; } = new List<Transportation>();
}

public class Vehicle : BaseEntity
{
    public string Model { get; set; } = string.Empty; // e.g., Mercedes-Benz Sprinter, Hyundai H1, Toyota Fortuner 4x4
    public string LicensePlate { get; set; } = string.Empty;
    public int PassengerCapacity { get; set; } = 4;
    public int LuggageCapacity { get; set; } = 4;
    public string Type { get; set; } = "Van"; // Airport Transfer, Private Car, Van, Bus
    public int Year { get; set; } = 2024;
    public bool HasAirConditioning { get; set; } = true;
    public bool HasOxygenOnBoard { get; set; } = true; // Essential safety equipment for high-altitude Andean driving
    public string? SoatPolicyNumber { get; set; } // Peruvian mandatory vehicle insurance
    public DateTime? SoatExpirationDate { get; set; }
    public DateTime? TechnicalInspectionExpirationDate { get; set; }
    public string? GpsDeviceId { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? AssignedDriverId { get; set; }
    public Driver? AssignedDriver { get; set; }
}

public class Guide : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? HebrewName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string CertificationNumber { get; set; } = string.Empty; // DIRCETUR Cusco official license
    public decimal DailyRate { get; set; } = 120;
    public Currency Currency { get; set; } = Currency.USD;
    public double Rating { get; set; } = 4.9;
    public bool FirstAidCertified { get; set; } = true;
    public bool IsJewishHeritageExpert { get; set; } = true;
    public List<string> Languages { get; set; } = new() { "Hebrew", "English", "Spanish" };
    public string Specialization { get; set; } = "Inca History, High Altitude Trekking & Jewish Heritage";
    public string? EmergencyContact { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public class Transportation : BaseEntity
{
    public string ServiceType { get; set; } = "Airport Transfer"; // Airport Transfer, Private Car, Van, Bus, Taxi, Train, Flight
    public string HebrewServiceType { get; set; } = "הסעה";
    public TransportationType Type { get; set; } = TransportationType.AirportTransfer;

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public Guid? BookingId { get; set; }
    public Booking? Booking { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string PickupLocation { get; set; } = string.Empty;
    public string DropoffLocation { get; set; } = string.Empty;
    public DateTime ScheduledPickupTime { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    public int PassengerCount { get; set; } = 1;

    // Train details (PeruRail / Inca Rail)
    public string? TrainCompany { get; set; } // PeruRail, Inca Rail
    public string? TrainService { get; set; } // Expedition, Vistadome, Vistadome Observatory, 360, Hiram Bingham
    public string? TrainNumber { get; set; }
    public string? TrainStationDeparture { get; set; }
    public string? TrainStationArrival { get; set; }

    // Flight details
    public string? Airline { get; set; } // LATAM, Sky Airline
    public string? FlightOrTrainNumber { get; set; }

    public TransportationStatus Status { get; set; } = TransportationStatus.Assigned;
    public string? Notes { get; set; }
}

