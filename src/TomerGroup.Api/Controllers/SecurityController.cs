using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecurityController : ControllerBase
{
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ISecuritySanitizerService _sanitizerService;
    private readonly ISecurityAuditService _auditService;

    public SecurityController(
        IRateLimitingService rateLimitingService,
        ISecuritySanitizerService sanitizerService,
        ISecurityAuditService auditService)
    {
        _rateLimitingService = rateLimitingService;
        _sanitizerService = sanitizerService;
        _auditService = auditService;
    }

    [HttpGet("rate-limit-status")]
    public IActionResult GetRateLimitStatus([FromQuery] string category = "General")
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var status = _rateLimitingService.CheckRateLimit(ip, category);
        return Ok(ApiResponse<RateLimitStatusDto>.Ok(status));
    }

    [HttpGet("audit-logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] string? entityName,
        [FromQuery] Guid? userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var filter = new AuditLogFilterDto
        {
            Action = action,
            EntityName = entityName,
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _auditService.GetAuditLogsAsync(filter, page, pageSize);
        return Ok(result);
    }

    [HttpPost("sanitize-preview")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult SanitizePreview([FromBody] SanitizePreviewRequest request)
    {
        var sanitized = _sanitizerService.SanitizeInput(request.RawInput);
        var maskedPassport = _sanitizerService.MaskPassport(request.PassportNumber);
        var isPathSafe = _sanitizerService.IsPathTraversalSafe(request.RawInput);

        return Ok(new
        {
            Original = request.RawInput,
            Sanitized = sanitized,
            MaskedPassport = maskedPassport,
            IsPathSafe = isPathSafe
        });
    }
}

public class SanitizePreviewRequest
{
    public string RawInput { get; set; } = string.Empty;
    public string? PassportNumber { get; set; }
}

