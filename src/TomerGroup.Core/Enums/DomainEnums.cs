namespace TomerGroup.Core.Enums;

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Sales = 3,
    Operations = 4,
    Finance = 5,
    Guide = 6,
    Driver = 7,
    Customer = 8
}

public enum BookingStatus
{
    Inquiry = 1,
    Pending = 2,
    Confirmed = 3,
    PartiallyPaid = 4,
    Paid = 5,
    Completed = 6,
    Cancelled = 7
}

public enum TripStatus
{
    Draft = 1,
    Scheduled = 2,
    Confirmed = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6
}

public enum ActivityStatus
{
    Scheduled = 1,
    Confirmed = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}

public enum TransportationStatus
{
    Assigned = 1,
    Confirmed = 2,
    OnTheWay = 3,
    Arrived = 4,
    PassengerPickedUp = 5,
    Completed = 6,
    Cancelled = 7
}

public enum PaymentStatus
{
    Pending = 1,
    Partial = 2,
    Paid = 3,
    Refunded = 4,
    Cancelled = 5
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    CreditCard = 3,
    Other = 4
}

public enum Currency
{
    USD = 1,
    PEN = 2,
    ILS = 3
}

public enum DocumentType
{
    FlightTicket = 1,
    HotelConfirmation = 2,
    TrainTicket = 3,
    MachuPicchuTicket = 4,
    TourVoucher = 5,
    Invoice = 6,
    Receipt = 7,
    TravelDocument = 8,
    MachuPicchuPermit = 9,
    IncaTrailPermit = 10,
    HuaynaPicchuPermit = 11,
    HotelVoucher = 12,
    Passport = 13,
    InsurancePolicy = 14,
    Other = 15
}

public enum LanguageCode
{
    Hebrew,   // "he"
    English,  // "en"
    Spanish   // "es"
}

public enum TourCategory
{
    Trek = 1,
    DayTour = 2,
    Cultural = 3,
    Adventure = 4,
    Scenic = 5,
    Expedition = 6
}

public enum TransportationType
{
    AirportTransfer = 1,
    PrivateVan = 2,
    Train = 3,
    DomesticFlight = 4,
    PrivateCar = 5,
    Bus = 6
}

