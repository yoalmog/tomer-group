using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Sales,Operations,Finance")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _tripService.GetAllTripsAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _tripService.GetByIdAsync(id, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(Guid customerId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _tripService.GetByCustomerIdAsync(customerId, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("customer/{customerId}/dashboard")]
    public async Task<IActionResult> GetCustomerDashboard(Guid customerId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _tripService.GetCustomerHomeDashboardAsync(customerId, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Sales,Operations")]
    public async Task<IActionResult> Create([FromBody] CreateTripDto request)
    {
        var result = await _tripService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Sales,Operations")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTripDto request)
    {
        var result = await _tripService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("{tripId}/days")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> AddTripDay(Guid tripId, [FromBody] CreateTripDayDto request)
    {
        var userId = GetUserId();
        var result = await _tripService.AddTripDayAsync(tripId, request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("days/{dayId}/activities")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> AddActivity(Guid dayId, [FromBody] CreateActivityDto request)
    {
        var userId = GetUserId();
        var result = await _tripService.AddActivityAsync(dayId, request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("activities/{activityId}/status")]
    [Authorize(Roles = "Admin,Manager,Operations,Guide,Driver")]
    public async Task<IActionResult> UpdateActivityStatus(Guid activityId, [FromBody] UpdateActivityStatusDto request)
    {
        var userId = GetUserId();
        var result = await _tripService.UpdateActivityStatusAsync(activityId, request.Status, userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("destinations")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDestinations()
    {
        var result = await _tripService.GetDestinationsAsync();
        return Ok(result);
    }

    [HttpPost("destinations")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> CreateDestination([FromBody] CreateDestinationDto request)
    {
        var userId = GetUserId();
        var result = await _tripService.CreateDestinationAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}
