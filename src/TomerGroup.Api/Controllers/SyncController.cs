using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;

    public SyncController(ISyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Status = "Online",
            ServerTimestamp = DateTime.UtcNow,
            SyncEngine = "TomerGroup-DeltaSync-v1.0"
        });
    }

    [HttpPost("pull")]
    public async Task<IActionResult> PullDelta([FromBody] SyncPullRequestDto request)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _syncService.PullDeltaPackageAsync(request, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("push")]
    public async Task<IActionResult> PushChanges([FromBody] SyncPushRequestDto request)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _syncService.PushClientChangesAsync(request, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : BadRequest(result);
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

