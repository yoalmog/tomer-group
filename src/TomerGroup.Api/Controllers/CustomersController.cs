using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Sales,Operations")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _customerService.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("summaries")]
    [Authorize(Roles = "Admin,Manager,Sales,Operations")]
    public async Task<IActionResult> GetSummaries([FromQuery] CustomerSearchFilterDto filter)
    {
        var result = await _customerService.GetSummariesAsync(filter);
        return Ok(result);
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager,Sales,Operations,Finance")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _customerService.GetTravelerStatsAsync();
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _customerService.GetByUserIdAsync(userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCustomerProfileDto request)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await _customerService.UpdateOwnProfileAsync(userId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _customerService.GetByIdAsync(id, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id}/sensitive")]
    public async Task<IActionResult> GetSensitiveDetails(Guid id)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _customerService.GetSensitiveDetailsAsync(id, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Sales")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto request)
    {
        var userId = GetUserId();
        var result = await _customerService.CreateAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto request)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var result = await _customerService.UpdateAsync(id, request, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var result = await _customerService.DeleteAsync(id, userId);
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
