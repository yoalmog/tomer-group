using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Operations,Sales,Driver")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _driverService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Operations,Sales,Driver")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _driverService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> Create([FromBody] CreateDriverDto request)
    {
        var result = await _driverService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverDto request)
    {
        var result = await _driverService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/manifest")]
    [Authorize(Roles = "Admin,Manager,Operations,Driver")]
    public async Task<IActionResult> GetDailyManifest(Guid id, [FromQuery] DateTime? date)
    {
        var manifestDate = date ?? DateTime.UtcNow;
        var result = await _driverService.GetDailyManifestAsync(id, manifestDate);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> AssignDriver([FromBody] AssignDriverToTransferDto request)
    {
        var userId = GetUserId();
        var result = await _driverService.AssignDriverToTransferAsync(request, userId);
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
}

