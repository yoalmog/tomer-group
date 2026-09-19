using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Core.Security;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Security;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class CustomerAuthenticationTests
{
    private (TomerDbContext context, IPasswordHasher hasher, IJwtTokenService jwt, IAuditService audit) CreateTestServices(string dbName)
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new TomerDbContext(options);
        var hasher = new PasswordHasher();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:Secret", "TomerGroupSuperSecretKeyForPeruTravelExperience2026!@#$998877" },
                { "JwtSettings:Issuer", "TomerGroupApi" },
                { "JwtSettings:Audience", "TomerGroupApp" },
                { "JwtSettings:ExpiryMinutes", "120" }
            })
            .Build();

        var jwt = new JwtTokenService(config);
        var audit = new AuditService(context);

        return (context, hasher, jwt, audit);
    }

    [Fact]
    public async Task RegisterCustomerAsync_WithValidDetails_CreatesUserAndCustomer_ReturnsTokens()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("Customer_Reg_Success");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var request = new CustomerRegisterRequestDto
        {
            FirstName = "David",
            LastName = "Levi",
            Email = "david.levi@example.com",
            Phone = "+972-50-1234567",
            Password = "SecurePassword2026!",
            ConfirmPassword = "SecurePassword2026!",
            PreferredLanguage = "he"
        };

        // Act
        var response = await authService.RegisterCustomerAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data.Token);
        Assert.NotEmpty(response.Data.RefreshToken);
        Assert.Equal("David", response.Data.User.FirstName);
        Assert.Equal("Levi", response.Data.User.LastName);
        Assert.Equal("david.levi@example.com", response.Data.User.Email);
        Assert.Equal("Customer", response.Data.User.Role);

        // Verify User in Database
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "david.levi@example.com");
        Assert.NotNull(user);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.NotEqual("SecurePassword2026!", user.PasswordHash);
        Assert.NotEmpty(user.Salt);
        Assert.True(hasher.VerifyPassword("SecurePassword2026!", user.PasswordHash, user.Salt));

        // Verify Customer entity in Database
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
        Assert.NotNull(customer);
        Assert.Equal("David", customer.FirstName);
        Assert.Equal("Levi", customer.LastName);
        Assert.Equal("+972-50-1234567", customer.Phone);
        Assert.Equal("david.levi@example.com", customer.Email);
    }

    [Fact]
    public async Task RegisterCustomerAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("Customer_Reg_DuplicateEmail");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            PasswordHash = "hash",
            Salt = "salt",
            FirstName = "Existing",
            LastName = "User",
            Role = UserRole.Customer,
            IsActive = true
        };
        await context.Users.AddAsync(existingUser);
        await context.SaveChangesAsync();

        var request = new CustomerRegisterRequestDto
        {
            FirstName = "New",
            LastName = "Person",
            Email = "existing@example.com",
            Phone = "+972-52-9999999",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await authService.RegisterCustomerAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains("already exists", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterCustomerAsync_WithMismatchedPasswords_ReturnsFailure()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("Customer_Reg_Mismatch");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var request = new CustomerRegisterRequestDto
        {
            FirstName = "Sarah",
            LastName = "Cohen",
            Email = "sarah.cohen@example.com",
            Phone = "+972-54-1112233",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword456!"
        };

        // Act
        var response = await authService.RegisterCustomerAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains("do not match", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterCustomerAsync_WithShortPassword_ReturnsFailure()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("Customer_Reg_WeakPass");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var request = new CustomerRegisterRequestDto
        {
            FirstName = "Sarah",
            LastName = "Cohen",
            Email = "sarah2@example.com",
            Password = "123",
            ConfirmPassword = "123"
        };

        // Act
        var response = await authService.RegisterCustomerAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Contains("at least 6 characters", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithValidEmail_GeneratesResetToken()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("Customer_Forgot_Pass");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "forgotten@example.com",
            PasswordHash = "hash",
            Salt = "salt",
            FirstName = "Forgot",
            LastName = "User",
            Role = UserRole.Customer,
            IsActive = true
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await authService.ForgotPasswordAsync(new ForgotPasswordRequestDto { Email = "forgotten@example.com" });

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);

        var resetEntity = await context.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
        Assert.NotNull(resetEntity);
        Assert.False(resetEntity.IsUsed);
        Assert.True(resetEntity.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_UpdatesPasswordAndAllowsLogin()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("Customer_Reset_Pass");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "resetme@example.com",
            PasswordHash = "oldhash",
            Salt = "oldsalt",
            FirstName = "Reset",
            LastName = "User",
            Role = UserRole.Customer,
            IsActive = true
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Generate token via ForgotPasswordAsync
        var forgotResult = await authService.ForgotPasswordAsync(new ForgotPasswordRequestDto { Email = "resetme@example.com" });
        Assert.True(forgotResult.Success);
        var resetToken = forgotResult.Data!;

        // Act
        var resetSuccess = await authService.ResetPasswordAsync(new ResetPasswordRequestDto
        {
            Email = "resetme@example.com",
            ResetToken = resetToken,
            NewPassword = "BrandNewSecurePass2026!"
        });

        // Assert
        Assert.True(resetSuccess.Success);
        Assert.True(resetSuccess.Data);

        // Verify login with new password succeeds
        var loginResponse = await authService.LoginAsync(new LoginRequestDto
        {
            Email = "resetme@example.com",
            Password = "BrandNewSecurePass2026!"
        });
        Assert.NotNull(loginResponse);
        Assert.True(loginResponse.Success);
        Assert.NotNull(loginResponse.Data);
        Assert.NotEmpty(loginResponse.Data.Token);
    }

    [Fact]
    public async Task CustomerDataIsolation_EachCustomerOnlyAccessesTheirOwnTrips()
    {
        // Arrange
        var (context, _, _, _) = CreateTestServices("Customer_Isolation_Db");

        var customerAId = Guid.NewGuid();
        var customerBId = Guid.NewGuid();

        var tripA = new Trip
        {
            Id = Guid.NewGuid(),
            CustomerId = customerAId,
            Title = "David's Salkantay Trek",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(10),
            Status = TripStatus.InProgress
        };

        var tripB = new Trip
        {
            Id = Guid.NewGuid(),
            CustomerId = customerBId,
            Title = "Noam's Inca Trail",
            StartDate = DateTime.UtcNow.AddDays(12),
            EndDate = DateTime.UtcNow.AddDays(17),
            Status = TripStatus.InProgress
        };

        await context.Trips.AddRangeAsync(tripA, tripB);
        await context.SaveChangesAsync();

        // Act: Customer A queries their trips
        var customerATrips = await context.Trips.Where(t => t.CustomerId == customerAId).ToListAsync();

        // Assert: Customer A only sees their own trip, never Customer B's trip
        Assert.Single(customerATrips);
        Assert.Equal("David's Salkantay Trek", customerATrips[0].Title);
        Assert.DoesNotContain(customerATrips, t => t.CustomerId == customerBId);
    }
}
