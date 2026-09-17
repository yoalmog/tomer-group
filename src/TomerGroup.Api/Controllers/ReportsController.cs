using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Finance")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var filter = new DateRangeFilterDto
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _reportService.GetExecutiveAnalyticsAsync(filter);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format = "csv", [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var filter = new DateRangeFilterDto
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _reportService.ExportReportAsync(filter, format);
        if (!result.Success || result.Data == null)
        {
            return BadRequest(result);
        }

        return File(result.Data.FileBytes, result.Data.ContentType, result.Data.FileName);
    }
}
