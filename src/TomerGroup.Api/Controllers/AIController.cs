using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Operations,Sales")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("itinerary/draft")]
    public async Task<IActionResult> GenerateItineraryDraft([FromBody] GenerateItineraryPromptDto request)
    {
        var userId = GetUserId();
        var result = await _aiService.GenerateItineraryDraftAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("whatsapp/draft")]
    public async Task<IActionResult> DraftWhatsAppMessage([FromBody] DraftWhatsAppPromptDto request)
    {
        var userId = GetUserId();
        var result = await _aiService.DraftWhatsAppMessageAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests([FromQuery] string? status = null)
    {
        var result = await _aiService.GetRequestsAsync(status);
        return Ok(result);
    }

    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetRequestById(Guid id)
    {
        var result = await _aiService.GetRequestByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("requests/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ApproveAIRequestDto? approval = null)
    {
        var userId = GetUserId();
        var result = await _aiService.ApproveRequestAsync(id, userId, approval);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("requests/{id}/reject")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectAIRequestDto? reject = null)
    {
        var userId = GetUserId();
        var result = await _aiService.RejectRequestAsync(id, userId, reject?.Reason);
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

public class RejectAIRequestDto
{
    public string? Reason { get; set; }
}

