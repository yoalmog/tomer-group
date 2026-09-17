using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/settings/[controller]")]
public class BrandingController : ControllerBase
{
    private readonly IBrandingService _brandingService;

    public BrandingController(IBrandingService brandingService)
    {
        _brandingService = brandingService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetBranding()
    {
        var settings = await _brandingService.GetSettingsAsync();
        return Ok(ApiResponse<BrandSettings>.Ok(settings));
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBranding([FromBody] BrandSettings newSettings)
    {
        var updated = await _brandingService.UpdateSettingsAsync(newSettings);
        return Ok(ApiResponse<BrandSettings>.Ok(updated, "Branding settings updated successfully"));
    }
}

