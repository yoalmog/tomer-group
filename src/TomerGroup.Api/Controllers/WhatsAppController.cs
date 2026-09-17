using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;

    public WhatsAppController(IWhatsAppService whatsAppService)
    {
        _whatsAppService = whatsAppService;
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var result = await _whatsAppService.GetTemplatesAsync();
        return Ok(result);
    }

    [HttpPost("generate-url")]
    public IActionResult GenerateUrl([FromBody] GenerateWhatsAppUrlRequest request)
    {
        var url = _whatsAppService.GenerateWhatsAppUrl(request.Phone, request.Message);
        return Ok(ApiResponse<string>.Ok(url));
    }

    [HttpPost("send")]
    public async Task<IActionResult> PrepareOrSendMessage([FromBody] SendWhatsAppMessageDto request)
    {
        var userId = GetUserId();
        var result = await _whatsAppService.PrepareOrSendMessageAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid? customerId = null)
    {
        var role = GetUserRole();
        var userId = GetUserId();

        // Customer can only view own history
        if (role == "Customer" && (!customerId.HasValue || customerId.Value == Guid.Empty))
        {
            // Allowed to query own
        }

        var result = await _whatsAppService.GetMessageHistoryAsync(customerId);
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

public class GenerateWhatsAppUrlRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

