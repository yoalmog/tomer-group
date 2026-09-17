using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelsController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _hotelService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _hotelService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> Create([FromBody] CreateHotelDto request)
    {
        var result = await _hotelService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHotelDto request)
    {
        var result = await _hotelService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("bookings")]
    [Authorize(Roles = "Admin,Manager,Sales,Operations")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateHotelBookingDto request)
    {
        var userId = GetUserId();
        var result = await _hotelService.CreateHotelBookingAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("bookings/customer/{customerId}")]
    public async Task<IActionResult> GetCustomerHotelBookings(Guid customerId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _hotelService.GetCustomerHotelBookingsAsync(customerId, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpPatch("bookings/{id}/status")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> UpdateBookingStatus(Guid id, [FromBody] string status)
    {
        var result = await _hotelService.UpdateHotelBookingStatusAsync(id, status);
        if (!result.Success)
        {
            return NotFound(result);
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

