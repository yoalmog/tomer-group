using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GuidesController : ControllerBase
{
    private readonly IGuideService _guideService;

    public GuidesController(IGuideService guideService)
    {
        _guideService = guideService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Operations,Sales,Guide")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _guideService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Operations,Sales,Guide")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _guideService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> Create([FromBody] CreateGuideDto request)
    {
        var result = await _guideService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGuideDto request)
    {
        var result = await _guideService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/manifest")]
    [Authorize(Roles = "Admin,Manager,Operations,Guide")]
    public async Task<IActionResult> GetDailyManifest(Guid id, [FromQuery] DateTime? date)
    {
        var manifestDate = date ?? DateTime.UtcNow;
        var result = await _guideService.GetDailyManifestAsync(id, manifestDate);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin,Manager,Operations")]
    public async Task<IActionResult> AssignGuide([FromBody] AssignGuideToActivityDto request)
    {
        var userId = GetUserId();
        var result = await _guideService.AssignGuideToActivityAsync(request, userId);
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

