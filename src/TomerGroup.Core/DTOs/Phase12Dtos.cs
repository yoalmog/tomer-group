namespace TomerGroup.Core.DTOs;

public class DateRangeFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DestinationVolumeDto
{
    public string Destination { get; set; } = string.Empty;
    public string HebrewDestination { get; set; } = string.Empty;
    public int TripsCount { get; set; }
    public int TravelersCount { get; set; }
    public decimal RevenueUsd { get; set; }
    public double PercentageOfTotal { get; set; }
}

public class MonthlyRevenueSummaryDto
{
    public string MonthName { get; set; } = string.Empty; // e.g. "2026-05"
    public string HebrewMonthName { get; set; } = string.Empty; // e.g. "מאי 2026"
    public decimal RevenueUsd { get; set; }
    public decimal ExpensesUsd { get; set; }
    public decimal NetProfitUsd => RevenueUsd - ExpensesUsd;
    public int BookingsCount { get; set; }
}

public class ExecutiveAnalyticsReportDto
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit => TotalRevenue - TotalExpenses;
    public decimal ProfitMarginPercentage => TotalRevenue > 0 ? Math.Round((NetProfit / TotalRevenue) * 100m, 2) : 0;

    public int TotalBookingsCount { get; set; }
    public int ActiveTripsCount { get; set; }
    public int CompletedTripsCount { get; set; }
    public int TotalTravelersCount { get; set; }
    public int TotalHotelNightsBooked { get; set; }

    public List<DestinationVolumeDto> DestinationBreakdowns { get; set; } = new();
    public List<MonthlyRevenueSummaryDto> MonthlyRevenues { get; set; } = new();
    public List<ExpenseBreakdownDto> ExpenseBreakdowns { get; set; } = new();
}

public class ExpenseBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public string HebrewCategory { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

public class ExportReportResultDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
}

