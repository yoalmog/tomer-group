using TomerGroup.Core.Enums;

namespace TomerGroup.Core.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public decimal ExchangeRateToUsd { get; set; }
    public decimal AmountInUsd => ExchangeRateToUsd > 0 ? Amount * ExchangeRateToUsd : Amount;
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ReferenceNumber { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class RecordPaymentDto
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public decimal ExchangeRateToUsd { get; set; } = 1.0m;
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "Tickets";
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public decimal ExchangeRateToUsd { get; set; }
    public decimal AmountInUsd => ExchangeRateToUsd > 0 ? Amount * ExchangeRateToUsd : Amount;
    public DateTime Date { get; set; }
    public Guid? TripId { get; set; }
    public string? TripTitle { get; set; }
    public Guid? BookingId { get; set; }
    public string? SupplierName { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? Notes { get; set; }
}

public class RecordExpenseDto
{
    public string Category { get; set; } = "Tickets";
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public decimal ExchangeRateToUsd { get; set; } = 1.0m;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public Guid? TripId { get; set; }
    public Guid? BookingId { get; set; }
    public string? SupplierName { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? Notes { get; set; }
}

public class TripProfitabilityDto
{
    public Guid TripId { get; set; }
    public string TripCode { get; set; } = string.Empty;
    public string TripTitle { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit => TotalRevenue - TotalCost;
    public decimal MarginPercentage => TotalRevenue > 0 ? Math.Round((GrossProfit / TotalRevenue) * 100, 2) : 0;
    public Currency Currency { get; set; } = Currency.USD;
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public List<ExpenseDto> Expenses { get; set; } = new();
}

public class AgencyFinancialSummaryDto
{
    public decimal TotalRevenueUsd { get; set; }
    public decimal TotalCollectedUsd { get; set; }
    public decimal TotalOutstandingUsd { get; set; }
    public decimal TotalExpensesUsd { get; set; }
    public decimal NetProfitUsd => TotalRevenueUsd - TotalExpensesUsd;
    public decimal MarginPercentage => TotalRevenueUsd > 0 ? Math.Round((NetProfitUsd / TotalRevenueUsd) * 100, 2) : 0;
    public List<PaymentDto> RecentPayments { get; set; } = new();
}

