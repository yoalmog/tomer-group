using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Security;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class UsersController : ControllerBase
{
    private readonly TomerDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(TomerDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminUserDto>>>> GetUsers([FromQuery] UserRole? role = null, [FromQuery] string? search = null)
    {
        var query = _context.Users.AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(s) || u.FirstName.ToLower().Contains(s) || u.LastName.ToLower().Contains(s));
        }

        var users = await query
            .OrderBy(u => u.Role)
            .ThenBy(u => u.LastName)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                Phone = u.Phone ?? string.Empty,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<AdminUserDto>>.Ok(users));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetUserById(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(ApiResponse<AdminUserDto>.Fail("User not found"));

        return Ok(ApiResponse<AdminUserDto>.Ok(new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> CreateUser([FromBody] CreateAdminUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(ApiResponse<AdminUserDto>.Fail("Email and password are required"));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            return BadRequest(ApiResponse<AdminUserDto>.Fail("A user with this email already exists"));

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        var newUser = new User
        {
            Email = normalizedEmail,
            PasswordHash = hash,
            Salt = salt,
            FirstName = request.FirstName?.Trim() ?? string.Empty,
            LastName = request.LastName?.Trim() ?? string.Empty,
            Phone = request.Phone?.Trim() ?? string.Empty,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(newUser);

        // Audit Log
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = newUser.Id,
            UserEmail = newUser.Email,
            Action = "CREATE_USER",
            EntityName = "User",
            EntityId = newUser.Id.ToString(),
            MetadataJson = $"Created staff user {newUser.Email} with role {newUser.Role}"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<AdminUserDto>.Ok(new AdminUserDto
        {
            Id = newUser.Id,
            Email = newUser.Email,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Phone = newUser.Phone,
            Role = newUser.Role,
            IsActive = newUser.IsActive,
            CreatedAt = newUser.CreatedAt
        }, "Staff user created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateUser(Guid id, [FromBody] UpdateAdminUserDto request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(ApiResponse<AdminUserDto>.Fail("User not found"));

        user.FirstName = request.FirstName?.Trim() ?? user.FirstName;
        user.LastName = request.LastName?.Trim() ?? user.LastName;
        user.Phone = request.Phone?.Trim() ?? user.Phone;
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email,
            Action = "UPDATE_USER",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            MetadataJson = $"Updated user {user.Email} (Role: {user.Role}, Active: {user.IsActive})"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<AdminUserDto>.Ok(new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }, "User updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(ApiResponse<bool>.Fail("User not found"));

        user.IsActive = false;

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = user.Id,
            UserEmail = user.Email,
            Action = "DEACTIVATE_USER",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            MetadataJson = $"Deactivated user {user.Email}"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "User deactivated successfully"));
    }
}
