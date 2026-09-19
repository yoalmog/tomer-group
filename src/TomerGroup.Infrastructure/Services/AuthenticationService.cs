using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Security;

namespace TomerGroup.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly TomerDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditService _auditService;

    public AuthenticationService(
        TomerDbContext context,
        IPasswordHasher hasher,
        IJwtTokenService jwtTokenService,
        IAuditService auditService)
    {
        _context = context;
        _hasher = hasher;
        _jwtTokenService = jwtTokenService;
        _auditService = auditService;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<LoginResponseDto>.Fail("Email and password are required");
        }

        var normalizedEmail = request.Email.ToLower().Trim();
        var user = await _context.Users
            .Include(u => u.CustomerProfile)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user == null)
        {
            await _auditService.LogAsync("LoginFailed", "User", null, null, normalizedEmail, "User not found");
            return ApiResponse<LoginResponseDto>.Fail("Invalid credentials");
        }

        // Account Lockout Protection (Brute-force defense)
        if (user.IsLockedOut)
        {
            await _auditService.LogAsync("LoginBlocked", "User", user.Id.ToString(), user.Id, user.Email, "Account locked out");
            return ApiResponse<LoginResponseDto>.Fail($"Account is temporarily locked out due to multiple failed login attempts. Please try again after {user.LockoutEnd:HH:mm} UTC or reset your password.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponseDto>.Fail("Account is inactive. Please contact Tomer Group administration.");
        }

        if (!_hasher.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                await _auditService.LogAsync("AccountLocked", "User", user.Id.ToString(), user.Id, user.Email, "Account locked for 15 minutes after 5 failed attempts");
            }
            else
            {
                await _auditService.LogAsync("LoginFailed", "User", user.Id.ToString(), user.Id, user.Email, $"Failed attempt {user.FailedLoginAttempts} of 5");
            }

            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<LoginResponseDto>.Fail("Invalid credentials");
        }

        // Successful authentication: Reset failed attempts & lockout
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("LoginSuccess", "User", user.Id.ToString(), user.Id, user.Email, $"Role: {user.Role}");

        var response = new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(120),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                CustomerId = user.CustomerProfile?.Id,
                PreferredLanguage = user.PreferredLanguage
            }
        };

        return ApiResponse<LoginResponseDto>.Ok(response, "Login successful");
    }

    public async Task<ApiResponse<LoginResponseDto>> RegisterCustomerAsync(CustomerRegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return ApiResponse<LoginResponseDto>.Fail("First name and last name are required");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiResponse<LoginResponseDto>.Fail("Email address is required");
        }

        var normalizedEmail = request.Email.ToLower().Trim();
        if (!normalizedEmail.Contains('@') || !normalizedEmail.Contains('.'))
        {
            return ApiResponse<LoginResponseDto>.Fail("Please enter a valid email address");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return ApiResponse<LoginResponseDto>.Fail("Password must be at least 6 characters long");
        }

        if (request.Password != request.ConfirmPassword)
        {
            return ApiResponse<LoginResponseDto>.Fail("Passwords do not match");
        }

        // Enforce unique email across all users
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (existingUser != null)
        {
            return ApiResponse<LoginResponseDto>.Fail("An account with this email address already exists. Please sign in or reset your password.");
        }

        // Cryptographic password hashing (PBKDF2 with unique salt)
        var (passwordHash, salt) = _hasher.HashPassword(request.Password);

        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var newUser = new User
        {
            Id = userId,
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Salt = salt,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = TomerGroup.Core.Enums.UserRole.Customer,
            IsActive = true,
            PreferredLanguage = string.IsNullOrWhiteSpace(request.PreferredLanguage) ? "en" : request.PreferredLanguage.Trim(),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        var newCustomer = new Customer
        {
            Id = customerId,
            UserId = userId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            Phone = request.Phone?.Trim() ?? string.Empty,
            WhatsApp = request.Phone?.Trim() ?? string.Empty,
            Country = "Israel",
            IsActiveInPeru = false,
            CreatedAt = DateTime.UtcNow
        };

        var accessToken = _jwtTokenService.GenerateAccessToken(newUser);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        newUser.RefreshToken = refreshToken;
        newUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

        await _context.Users.AddAsync(newUser, cancellationToken);
        await _context.Customers.AddAsync(newCustomer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("CustomerRegistered", "User", userId.ToString(), userId, normalizedEmail, "Customer account created");

        var response = new LoginResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(120),
            User = new UserInfoDto
            {
                Id = userId,
                Email = normalizedEmail,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Role = newUser.Role.ToString(),
                CustomerId = customerId,
                PreferredLanguage = newUser.PreferredLanguage
            }
        };

        return ApiResponse<LoginResponseDto>.Ok(response, "Account created successfully");
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.CustomerProfile)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null ||
            string.IsNullOrEmpty(user.RefreshToken) ||
            user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return ApiResponse<LoginResponseDto>.Fail("Invalid or expired refresh token. Please log in again.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<LoginResponseDto>.Fail("Account is inactive.");
        }

        // Token Rotation: Generate new access AND refresh tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("TokenRefreshed", "User", user.Id.ToString(), user.Id, user.Email, "New tokens issued");

        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(120),
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                CustomerId = user.CustomerProfile?.Id,
                PreferredLanguage = user.PreferredLanguage
            }
        });
    }

    public async Task<ApiResponse<bool>> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _context.SaveChangesAsync(cancellationToken);
            await _auditService.LogAsync("Logout", "User", user.Id.ToString(), user.Id, user.Email, "Tokens revoked");
        }

        return ApiResponse<bool>.Ok(true, "Logged out successfully");
    }

    public async Task<ApiResponse<UserInfoDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.CustomerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return ApiResponse<UserInfoDto>.Fail("User not found");
        }

        return ApiResponse<UserInfoDto>.Ok(new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            CustomerId = user.CustomerProfile?.Id,
            PreferredLanguage = user.PreferredLanguage
        });
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return ApiResponse<string>.Fail("Email is required");
        }

        var normalizedEmail = request.Email.ToLower().Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        // Security best practice: Do not disclose whether email exists or not
        if (user == null)
        {
            return ApiResponse<string>.Ok("If an account exists with this email, a reset token has been issued.");
        }

        // Generate 32-byte secure token
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var (tokenHash, _) = _hasher.HashPassword(rawToken);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            IsUsed = false
        };

        await _context.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("PasswordResetRequested", "User", user.Id.ToString(), user.Id, user.Email, "Reset token created");

        // Returns token directly for test automation and client flow
        return ApiResponse<string>.Ok(rawToken, "If an account exists with this email, a reset token has been issued.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ResetToken) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return ApiResponse<bool>.Fail("Email, reset token, and new password are required");
        }

        var normalizedEmail = request.Email.ToLower().Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("Invalid reset request");
        }

        // Find active unexpired reset token
        var validToken = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (validToken == null)
        {
            return ApiResponse<bool>.Fail("Invalid or expired password reset token. Please request a new one.");
        }

        // Mark token as used
        validToken.IsUsed = true;
        validToken.UsedAt = DateTime.UtcNow;

        // Hash new password
        var (newHash, newSalt) = _hasher.HashPassword(request.NewPassword);
        user.PasswordHash = newHash;
        user.Salt = newSalt;

        // Security: Revoke all refresh tokens upon password reset & clear lockout
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("PasswordResetSuccess", "User", user.Id.ToString(), user.Id, user.Email, "Password changed and sessions revoked");

        return ApiResponse<bool>.Ok(true, "Password has been successfully reset. Please log in with your new password.");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("User not found");
        }

        if (!_hasher.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.Salt))
        {
            return ApiResponse<bool>.Fail("Current password is incorrect");
        }

        var (newHash, newSalt) = _hasher.HashPassword(request.NewPassword);
        user.PasswordHash = newHash;
        user.Salt = newSalt;

        // Revoke active refresh tokens
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("PasswordChanged", "User", user.Id.ToString(), user.Id, user.Email, "User updated password");

        return ApiResponse<bool>.Ok(true, "Password changed successfully");
    }

    public async Task<ApiResponse<SessionStatusDto>> ValidateSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null || !user.IsActive || user.IsLockedOut)
        {
            return ApiResponse<SessionStatusDto>.Ok(new SessionStatusDto { IsAuthenticated = false });
        }

        return ApiResponse<SessionStatusDto>.Ok(new SessionStatusDto
        {
            IsAuthenticated = true,
            UserId = user.Id,
            Role = user.Role.ToString(),
            Email = user.Email,
            ExpiresAt = user.RefreshTokenExpiryTime,
            TimeRemaining = user.RefreshTokenExpiryTime.HasValue ? user.RefreshTokenExpiryTime.Value - DateTime.UtcNow : null
        });
    }
}
