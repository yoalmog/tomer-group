using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Finance,Sales")]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentDto request)
    {
        var userId = GetUserId();
        var result = await _paymentService.RecordPaymentAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetByBookingId(Guid bookingId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _paymentService.GetByBookingIdAsync(bookingId, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerPayments(Guid customerId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var result = await _paymentService.GetCustomerPaymentsAsync(customerId, userId, role);
        if (!result.Success)
        {
            return result.Message?.Contains("Access denied") == true ? Forbid() : NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin,Manager,Finance")]
    public async Task<IActionResult> GetFinancialSummary()
    {
        var result = await _paymentService.GetFinancialSummaryAsync();
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

