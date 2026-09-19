using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CustomerRegisterRequestDto request)
    {
        var result = await _authService.RegisterCustomerAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            await _authService.LogoutAsync(userId);
        }

        return Ok(ApiResponse<bool>.Ok(true, "Logged out successfully"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<UserInfoDto>.Fail("Invalid token"));
        }

        var result = await _authService.GetCurrentUserAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("profile")]
    [Authorize]
    public async Task<IActionResult> SyncProfile(
        [FromBody] CreateCustomerProfileRequestDto? request,
        [FromServices] TomerGroup.Infrastructure.Data.TomerDbContext context,
        CancellationToken cancellationToken)
    {
        var authUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? string.Empty;

        var email = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst("email")?.Value
            ?? request?.Email
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(authUserId) && string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(ApiResponse<CustomerProfileDto>.Fail("Unauthorized: No identity claim found in token"));
        }

        var customer = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            context.Customers,
            c => c.AuthUserId == authUserId || (!string.IsNullOrWhiteSpace(email) && c.Email.ToLower() == email.ToLower()),
            cancellationToken);

        if (customer == null)
        {
            var firstName = !string.IsNullOrWhiteSpace(request?.FirstName) ? request.FirstName : (User.FindFirst("name")?.Value ?? "Traveler");
            var lastName = request?.LastName ?? string.Empty;

            var userGuid = Guid.NewGuid();
            var newCustomer = new TomerGroup.Core.Models.Customer
            {
                Id = Guid.NewGuid(),
                UserId = userGuid,
                AuthUserId = authUserId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = request?.Phone ?? string.Empty,
                Language = request?.Language ?? "he",
                Country = "Israel",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Customers.AddAsync(newCustomer, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            customer = newCustomer;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(customer.AuthUserId) && !string.IsNullOrWhiteSpace(authUserId))
            {
                customer.AuthUserId = authUserId;
            }
            if (request != null && !string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(customer.Phone))
            {
                customer.Phone = request.Phone;
            }
            customer.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        var profileDto = new CustomerProfileDto
        {
            Id = customer.Id,
            AuthUserId = customer.AuthUserId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            HebrewName = customer.HebrewName,
            PassportName = customer.PassportName,
            Email = customer.Email,
            Phone = customer.Phone,
            WhatsApp = customer.WhatsApp,
            Country = customer.Country,
            Language = customer.Language,
            ProfileImageUrl = customer.ProfileImageUrl,
            Status = customer.Status,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };

        return Ok(ApiResponse<CustomerProfileDto>.Ok(profileDto, "Customer profile synced with cloud backend"));
    }
}


