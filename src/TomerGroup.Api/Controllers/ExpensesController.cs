using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Finance,Operations")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpPost]
    public async Task<IActionResult> RecordExpense([FromBody] RecordExpenseDto request)
    {
        var userId = GetUserId();
        var result = await _expenseService.RecordExpenseAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("trip/{tripId}")]
    public async Task<IActionResult> GetByTripId(Guid tripId)
    {
        var result = await _expenseService.GetByTripIdAsync(tripId);
        return Ok(result);
    }

    [HttpGet("trip/{tripId}/profitability")]
    public async Task<IActionResult> GetTripProfitability(Guid tripId)
    {
        var result = await _expenseService.GetTripProfitabilityAsync(tripId);
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
}

