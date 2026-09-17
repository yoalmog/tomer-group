using System.Text;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly TomerDbContext _context;

    public ReportService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ExecutiveAnalyticsReportDto>> GetExecutiveAnalyticsAsync(DateRangeFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        // 1. Queries with Date Filtering
        var bookingsQuery = _context.Bookings.AsNoTracking().Where(b => !b.IsDeleted);
        var tripsQuery = _context.Trips.AsNoTracking().Include(t => t.Days).Where(t => !t.IsDeleted);
        var expensesQuery = _context.Expenses.AsNoTracking().Where(e => !e.IsDeleted);
        var hotelBookingsQuery = _context.HotelBookings.AsNoTracking().Where(hb => !hb.IsDeleted);
        var customersQuery = _context.Customers.AsNoTracking().Where(c => !c.IsDeleted);

        if (filter?.StartDate.HasValue == true)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= filter.StartDate.Value);
            tripsQuery = tripsQuery.Where(t => t.CreatedAt >= filter.StartDate.Value);
            expensesQuery = expensesQuery.Where(e => e.CreatedAt >= filter.StartDate.Value);
        }

        if (filter?.EndDate.HasValue == true)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= filter.EndDate.Value);
            tripsQuery = tripsQuery.Where(t => t.CreatedAt <= filter.EndDate.Value);
            expensesQuery = expensesQuery.Where(e => e.CreatedAt <= filter.EndDate.Value);
        }

        var bookings = await bookingsQuery.ToListAsync(cancellationToken);
        var trips = await tripsQuery.ToListAsync(cancellationToken);
        var expenses = await expensesQuery.ToListAsync(cancellationToken);
        var hotelBookings = await hotelBookingsQuery.ToListAsync(cancellationToken);
        var totalTravelers = await customersQuery.CountAsync(cancellationToken);

        // 2. Financial KPIs
        var totalRevenue = bookings.Sum(b => b.TotalAmount);
        var totalExpenses = expenses.Sum(e => e.AmountInUsd);

        // 3. Operational KPIs
        var activeTrips = trips.Count(t => t.Status == TripStatus.InProgress || t.Status == TripStatus.Confirmed || t.Status == TripStatus.Scheduled);
        var completedTrips = trips.Count(t => t.Status == TripStatus.Completed);
        var totalHotelNights = hotelBookings.Sum(hb => Math.Max(1, (hb.CheckOutDate - hb.CheckInDate).Days));

        // 4. Destination Volume Breakdown
        var destinationGroups = new Dictionary<string, (int trips, int travelers, decimal revenue)>();
        foreach (var trip in trips)
        {
            var dest = trip.Days.FirstOrDefault()?.Destination;
            if (string.IsNullOrWhiteSpace(dest))
            {
                dest = "Cusco & Sacred Valley";
            }

            if (!destinationGroups.ContainsKey(dest))
            {
                destinationGroups[dest] = (0, 0, 0);
            }

            var curr = destinationGroups[dest];
            destinationGroups[dest] = (curr.trips + 1, curr.travelers + 1, curr.revenue + trip.TotalRevenue);
        }

        var destinationBreakdowns = destinationGroups.Select(kv => new DestinationVolumeDto
        {
            Destination = kv.Key,
            HebrewDestination = TranslateDestination(kv.Key),
            TripsCount = kv.Value.trips,
            TravelersCount = kv.Value.travelers,
            RevenueUsd = kv.Value.revenue,
            PercentageOfTotal = trips.Count > 0 ? Math.Round((double)kv.Value.trips / trips.Count * 100, 1) : 0
        }).OrderByDescending(d => d.TripsCount).ToList();

        // 5. Monthly Revenue Breakdown
        var monthlyGroups = bookings
            .GroupBy(b => b.CreatedAt.ToString("yyyy-MM"))
            .Select(g => new MonthlyRevenueSummaryDto
            {
                MonthName = g.Key,
                HebrewMonthName = FormatHebrewMonth(g.Key),
                RevenueUsd = g.Sum(b => b.TotalAmount),
                ExpensesUsd = expenses.Where(e => e.CreatedAt.ToString("yyyy-MM") == g.Key).Sum(e => e.AmountInUsd),
                BookingsCount = g.Count()
            })
            .OrderBy(m => m.MonthName)
            .ToList();

        // 6. Expense Category Breakdown
        var expenseCategoryBreakdowns = expenses
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseBreakdownDto
            {
                Category = g.Key.ToString(),
                HebrewCategory = TranslateExpenseCategory(g.Key),
                TotalAmount = g.Sum(e => e.AmountInUsd),
                PercentageOfTotal = totalExpenses > 0 ? Math.Round((g.Sum(e => e.AmountInUsd) / totalExpenses) * 100m, 1) : 0
            })
            .OrderByDescending(e => e.TotalAmount)
            .ToList();

        var report = new ExecutiveAnalyticsReportDto
        {
            GeneratedAt = DateTime.UtcNow,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            TotalBookingsCount = bookings.Count,
            ActiveTripsCount = activeTrips,
            CompletedTripsCount = completedTrips,
            TotalTravelersCount = Math.Max(totalTravelers, trips.Count),
            TotalHotelNightsBooked = totalHotelNights,
            DestinationBreakdowns = destinationBreakdowns,
            MonthlyRevenues = monthlyGroups,
            ExpenseBreakdowns = expenseCategoryBreakdowns
        };

        return ApiResponse<ExecutiveAnalyticsReportDto>.Ok(report);
    }

    public async Task<ApiResponse<ExportReportResultDto>> ExportReportAsync(DateRangeFilterDto? filter = null, string format = "csv", CancellationToken cancellationToken = default)
    {
        var analyticsResult = await GetExecutiveAnalyticsAsync(filter, cancellationToken);
        if (!analyticsResult.Success || analyticsResult.Data == null)
        {
            return ApiResponse<ExportReportResultDto>.Fail("Failed to retrieve report data for export");
        }

        var data = analyticsResult.Data;
        var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");

        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            // Binary simulated PDF voucher report
            var pdfContent = $"%PDF-1.4 Tomer Group Executive Report {dateStr} Revenue:${data.TotalRevenue} Expenses:${data.TotalExpenses} Profit:${data.NetProfit}";
            var pdfBytes = Encoding.UTF8.GetBytes(pdfContent);

            return ApiResponse<ExportReportResultDto>.Ok(new ExportReportResultDto
            {
                FileName = $"TomerGroup_Executive_Report_{dateStr}.pdf",
                ContentType = "application/pdf",
                FileBytes = pdfBytes
            });
        }

        // CSV Export with UTF-8 BOM for Microsoft Excel compatibility
        var sb = new StringBuilder();
        sb.AppendLine("Tomer Group - Peru Travel Experience");
        sb.AppendLine($"Executive Management Report - Generated: {data.GeneratedAt:dd/MM/yyyy HH:mm} UTC");
        sb.AppendLine();
        sb.AppendLine("Metric,Value");
        sb.AppendLine($"Total Revenue (USD),${data.TotalRevenue:N2}");
        sb.AppendLine($"Total Expenses (USD),${data.TotalExpenses:N2}");
        sb.AppendLine($"Net Gross Profit (USD),${data.NetProfit:N2}");
        sb.AppendLine($"Profit Margin Percentage,{data.ProfitMarginPercentage:F2}%");
        sb.AppendLine($"Total Bookings Count,{data.TotalBookingsCount}");
        sb.AppendLine($"Active Trips Count,{data.ActiveTripsCount}");
        sb.AppendLine($"Completed Trips Count,{data.CompletedTripsCount}");
        sb.AppendLine($"Total Travelers Serviced,{data.TotalTravelersCount}");
        sb.AppendLine($"Hotel Room Nights Booked,{data.TotalHotelNightsBooked}");
        sb.AppendLine();
        sb.AppendLine("Destination Volume Breakdown");
        sb.AppendLine("Destination,Hebrew Name,Trips Count,Travelers Count,Revenue (USD),Share %");
        foreach (var dest in data.DestinationBreakdowns)
        {
            sb.AppendLine($"\"{dest.Destination}\",\"{dest.HebrewDestination}\",{dest.TripsCount},{dest.TravelersCount},${dest.RevenueUsd:N2},{dest.PercentageOfTotal}%");
        }
        sb.AppendLine();
        sb.AppendLine("Monthly Financial Trends");
        sb.AppendLine("Month,Hebrew Month,Revenue (USD),Expenses (USD),Net Profit (USD),Bookings");
        foreach (var m in data.MonthlyRevenues)
        {
            sb.AppendLine($"\"{m.MonthName}\",\"{m.HebrewMonthName}\",${m.RevenueUsd:N2},${m.ExpensesUsd:N2},${m.NetProfitUsd:N2},{m.BookingsCount}");
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fullBytes = new byte[preamble.Length + csvBytes.Length];
        Buffer.BlockCopy(preamble, 0, fullBytes, 0, preamble.Length);
        Buffer.BlockCopy(csvBytes, 0, fullBytes, preamble.Length, csvBytes.Length);

        return ApiResponse<ExportReportResultDto>.Ok(new ExportReportResultDto
        {
            FileName = $"TomerGroup_Executive_Report_{dateStr}.csv",
            ContentType = "text/csv; charset=utf-8",
            FileBytes = fullBytes
        });
    }

    private static string TranslateDestination(string dest) => dest.ToLower() switch
    {
        var d when d.Contains("machu") => "מאצ'ו פיצ'ו",
        var d when d.Contains("sacred") || d.Contains("urubamba") => "העמק הקדוש",
        var d when d.Contains("rainbow") || d.Contains("vinicunca") => "הר שבעת הצבעים",
        var d when d.Contains("salkantay") => "טרק הסלקנטאי",
        var d when d.Contains("cusco") => "קוסקו והסביבה",
        _ => dest
    };

    private static string TranslateExpenseCategory(string cat) => (cat ?? string.Empty).ToLower() switch
    {
        var c when c.Contains("hotel") => "מלונות ואירוח",
        var c when c.Contains("transport") => "הסעות ורכבות",
        var c when c.Contains("guide") => "מדריכים וליווי",
        var c when c.Contains("ticket") => "כרטיסים ושמורות",
        var c when c.Contains("food") => "אוכל וכשרות",
        _ => "שונות"
    };

    private static string FormatHebrewMonth(string ym)
    {
        var parts = ym.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out var month))
        {
            var year = parts[0];
            var monthName = month switch
            {
                1 => "ינואר",
                2 => "פברואר",
                3 => "מרץ",
                4 => "אפריל",
                5 => "מאי",
                6 => "יוני",
                7 => "יולי",
                8 => "אוגוסט",
                9 => "ספטמבר",
                10 => "אוקטובר",
                11 => "נובמבר",
                12 => "דצמבר",
                _ => ym
            };
            return $"{monthName} {year}";
        }
        return ym;
    }
}
