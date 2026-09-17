using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;

namespace TomerGroup.Core.DTOs;

public class UpdateTourDto : CreateTourDto
{
    public bool IsActive { get; set; } = true;
}

public class HotelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int Stars { get; set; } = 4;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Website { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool HasOxygenEnrichedRooms { get; set; }
    public bool HasOxygenConcentrators { get; set; }
    public bool HasHeating { get; set; } = true;
    public bool IsKosherFriendly { get; set; } = true;
    public bool ShabbatFriendly { get; set; } = true;
    public bool WalkingDistanceToChabadCusco { get; set; }
    public List<string> RoomTypes { get; set; } = new();
    public List<string> PhotoUrls { get; set; } = new();
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateHotelDto
{
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int Stars { get; set; } = 4;
    public string Phone { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Website { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool HasOxygenEnrichedRooms { get; set; }
    public bool HasOxygenConcentrators { get; set; }
    public bool HasHeating { get; set; } = true;
    public bool IsKosherFriendly { get; set; } = true;
    public bool ShabbatFriendly { get; set; } = true;
    public bool WalkingDistanceToChabadCusco { get; set; }
    public List<string> RoomTypes { get; set; } = new();
    public List<string> PhotoUrls { get; set; } = new();
    public string? Notes { get; set; }
}

public class UpdateHotelDto : CreateHotelDto
{
    public bool IsActive { get; set; } = true;
}

public class HotelBookingDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; } = 2;
    public int RoomCount { get; set; } = 1;
    public bool OxygenRoomRequested { get; set; }
    public decimal AgencyCost { get; set; }
    public decimal SellingPrice { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public string Status { get; set; } = "Confirmed";
    public string? ConfirmationNumber { get; set; }
    public string? SpecialRequests { get; set; }
    public string? Notes { get; set; }
}

public class CreateHotelBookingDto
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid HotelId { get; set; }
    public string RoomType { get; set; } = "Standard";
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; } = 2;
    public int RoomCount { get; set; } = 1;
    public bool OxygenRoomRequested { get; set; }
    public decimal AgencyCost { get; set; }
    public decimal SellingPrice { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public string? SpecialRequests { get; set; }
    public string? Notes { get; set; }
}

public class VehicleDto
{
    public Guid Id { get; set; }
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int PassengerCapacity { get; set; }
    public int LuggageCapacity { get; set; }
    public string Type { get; set; } = "Van";
    public int Year { get; set; }
    public bool HasAirConditioning { get; set; }
    public bool IsActive { get; set; }
    public Guid? AssignedDriverId { get; set; }
    public string? AssignedDriverName { get; set; }
}

public class CreateVehicleDto
{
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int PassengerCapacity { get; set; } = 4;
    public int LuggageCapacity { get; set; } = 4;
    public string Type { get; set; } = "Van";
    public int Year { get; set; } = 2024;
    public bool HasAirConditioning { get; set; } = true;
    public Guid? AssignedDriverId { get; set; }
}

public class TransportationDto
{
    public Guid Id { get; set; }
    public string ServiceType { get; set; } = "Airport Transfer";
    public string HebrewServiceType { get; set; } = "הסעה";
    public TransportationType Type { get; set; } = TransportationType.AirportTransfer;
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? DriverWhatsApp { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehiclePlate { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DropoffLocation { get; set; } = string.Empty;
    public DateTime ScheduledPickupTime { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    public int PassengerCount { get; set; } = 1;
    public string? TrainCompany { get; set; }
    public string? TrainService { get; set; }
    public string? TrainNumber { get; set; }
    public string? TrainStationDeparture { get; set; }
    public string? TrainStationArrival { get; set; }
    public string? Airline { get; set; }
    public string? FlightOrTrainNumber { get; set; }
    public TransportationStatus Status { get; set; } = TransportationStatus.Assigned;
    public string? Notes { get; set; }
}

public class CreateTransportationDto
{
    public string ServiceType { get; set; } = "Airport Transfer";
    public string HebrewServiceType { get; set; } = "הסעה";
    public TransportationType Type { get; set; } = TransportationType.AirportTransfer;
    public Guid? DriverId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? CustomerId { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DropoffLocation { get; set; } = string.Empty;
    public DateTime ScheduledPickupTime { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    public int PassengerCount { get; set; } = 1;
    public string? TrainCompany { get; set; }
    public string? TrainService { get; set; }
    public string? TrainNumber { get; set; }
    public string? TrainStationDeparture { get; set; }
    public string? TrainStationArrival { get; set; }
    public string? Airline { get; set; }
    public string? FlightOrTrainNumber { get; set; }
    public string? Notes { get; set; }
}

public class UpdateTransportationStatusDto
{
    public TransportationStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class BookingDetailedDto : BookingDto
{
    public List<HotelBookingDto> HotelBookings { get; set; } = new();
    public List<TransportationDto> Transportations { get; set; } = new();
}

