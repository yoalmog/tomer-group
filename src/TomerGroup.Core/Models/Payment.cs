using TomerGroup.Core.Enums;

namespace TomerGroup.Core.Models;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public decimal ExchangeRateToUsd { get; set; } = 1.0m;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;
    public string? ReferenceNumber { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;

    public Guid BookingId { get; set; }
    public Booking? Booking { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string? Notes { get; set; }
}

public class Expense : BaseEntity
{
    public string Category { get; set; } = "Tickets"; // Hotel, Transport, Guide, Driver, Tickets, Food, Other
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public decimal ExchangeRateToUsd { get; set; } = 1.0m;
    public decimal AmountInUsd => ExchangeRateToUsd > 0 ? Amount * ExchangeRateToUsd : Amount;
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public Guid? BookingId { get; set; }
    public Booking? Booking { get; set; }

    public string? SupplierName { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? Notes { get; set; }
}

