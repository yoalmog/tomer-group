using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransportationController : ControllerBase
{
    private readonly ITransportationService _transportationService;

    public TransportationController(ITransportationService transportationService)
    {
        _transportationService = transportationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Operations,Driver")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _transportationService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _transportationService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Operations,Sales")]
    public async Task<IActionResult> Create([FromBody] CreateTransportationDto request)
    {
        var result = await _transportationService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Manager,Operations,Driver")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTransportationStatusDto request)
    {
        var userId = GetUserId();
        var result = await _transportationService.UpdateStatusAsync(id, request, userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerTransfers(Guid customerId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _transportationService.GetCustomerTransfersAsync(customerId, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("vehicles")]
    [Authorize(Roles = "Admin,Manager,Operations,Driver")]
    public async Task<IActionResult> GetVehicles()
    {
        var result = await _transportationService.GetVehiclesAsync();
        return Ok(result);
    }

    [HttpPost("vehicles")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto request)
    {
        var result = await _transportationService.CreateVehicleAsync(request);
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

