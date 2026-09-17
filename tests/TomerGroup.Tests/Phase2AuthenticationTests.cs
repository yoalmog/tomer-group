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

public class Phase2AuthenticationTests
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
                { "JwtSettings:ExpiryMinutes", "60" }
            })
            .Build();

        var jwt = new JwtTokenService(config);
        var audit = new AuditService(context);

        return (context, hasher, jwt, audit);
    }

    [Fact]
    public async Task BruteForceLockout_After5FailedAttempts_LocksAccount()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("Lockout_Db");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var (hash, salt) = hasher.HashPassword("CorrectPassword2026!");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "traveler@israel.com",
            PasswordHash = hash,
            Salt = salt,
            Role = UserRole.Customer,
            IsActive = true
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act: 4 failed password attempts -> Account still unlocked
        for (int i = 1; i <= 4; i++)
        {
            var failResult = await authService.LoginAsync(new LoginRequestDto
            {
                Email = "traveler@israel.com",
                Password = "WrongPassword!"
            });

            Assert.False(failResult.Success);
            Assert.Equal(i, user.FailedLoginAttempts);
            Assert.False(user.IsLockedOut);
        }

        // 5th failed attempt -> Locks account
        var fifthResult = await authService.LoginAsync(new LoginRequestDto
        {
            Email = "traveler@israel.com",
            Password = "WrongPassword!"
        });

        Assert.False(fifthResult.Success);
        Assert.True(user.IsLockedOut, "User MUST be locked out after 5 consecutive failed attempts");
        Assert.NotNull(user.LockoutEnd);

        // 6th attempt with CORRECT password while locked -> MUST still be blocked!
        var blockedResult = await authService.LoginAsync(new LoginRequestDto
        {
            Email = "traveler@israel.com",
            Password = "CorrectPassword2026!"
        });

        Assert.False(blockedResult.Success);
        Assert.Contains("temporarily locked out", blockedResult.Message);
    }

    [Fact]
    public async Task PhoneAuth_FullFlow_SendCodeAndVerify()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("PhoneAuth_Db");
        var phoneAuth = new PhoneAuthService(context, hasher, jwt, audit);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "danny@israel.com",
            FirstName = "Danny",
            LastName = "Cohen",
            Phone = "+972 54 123 4567",
            Role = UserRole.Customer,
            IsActive = true
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act 1: Send verification code
        var sendResult = await phoneAuth.SendVerificationCodeAsync("+972 54 123 4567");
        Assert.True(sendResult.Success);

        // Act 2: Verify code using canonical dev/test code "123456"
        var verifyResult = await phoneAuth.VerifyCodeAndLoginAsync("+972 54 123 4567", "123456");
        Assert.True(verifyResult.Success);
        Assert.NotNull(verifyResult.Data);
        Assert.NotEmpty(verifyResult.Data.Token);
        Assert.True(user.PhoneNumberConfirmed);
        Assert.Equal("Danny", verifyResult.Data.User.FirstName);
    }

    [Fact]
    public async Task PhoneAuth_RejectsInvalidCode_AndTracksAttempts()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("PhoneAuth_Fail_Db");
        var phoneAuth = new PhoneAuthService(context, hasher, jwt, audit);

        await phoneAuth.SendVerificationCodeAsync("+51 984 111 222");

        // Act & Assert 1: Wrong code attempt 1
        var fail1 = await phoneAuth.VerifyCodeAndLoginAsync("+51 984 111 222", "000000");
        Assert.False(fail1.Success);
        Assert.Contains("2 attempt(s) remaining", fail1.Message);

        // Act & Assert 2: Wrong code attempt 2
        var fail2 = await phoneAuth.VerifyCodeAndLoginAsync("+51 984 111 222", "000000");
        Assert.False(fail2.Success);
        Assert.Contains("1 attempt(s) remaining", fail2.Message);

        // Act & Assert 3: Wrong code attempt 3
        var fail3 = await phoneAuth.VerifyCodeAndLoginAsync("+51 984 111 222", "000000");
        Assert.False(fail3.Success);
        Assert.Contains("0 attempt(s) remaining", fail3.Message);

        // Act & Assert 4: 4th attempt -> Code invalidated
        var fail4 = await phoneAuth.VerifyCodeAndLoginAsync("+51 984 111 222", "123456");
        Assert.False(fail4.Success);
        Assert.Contains("Too many failed attempts", fail4.Message);
    }

    [Fact]
    public async Task PasswordRecovery_FullFlow_ResetAndRevokeSessions()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("PasswordRecovery_Db");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var (oldHash, oldSalt) = hasher.HashPassword("OldPassword123!");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "recovery@israel.com",
            PasswordHash = oldHash,
            Salt = oldSalt,
            Role = UserRole.Customer,
            RefreshToken = "ActiveRefreshToken123",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
            FailedLoginAttempts = 4
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act 1: Forgot password request
        var forgotResult = await authService.ForgotPasswordAsync(new ForgotPasswordRequestDto
        {
            Email = "recovery@israel.com"
        });

        Assert.True(forgotResult.Success);
        var resetToken = forgotResult.Data;
        Assert.NotNull(resetToken);
        Assert.NotEmpty(resetToken!);

        // Act 2: Reset password
        var resetResult = await authService.ResetPasswordAsync(new ResetPasswordRequestDto
        {
            Email = "recovery@israel.com",
            ResetToken = resetToken!,
            NewPassword = "BrandNewPassword2026!#"
        });

        Assert.True(resetResult.Success);
        Assert.Null(user.RefreshToken); // Security: Refresh token MUST be revoked on password reset
        Assert.Equal(0, user.FailedLoginAttempts); // Security: Failed attempts reset

        // Act 3: Login with OLD password -> MUST FAIL
        var oldLogin = await authService.LoginAsync(new LoginRequestDto
        {
            Email = "recovery@israel.com",
            Password = "OldPassword123!"
        });
        Assert.False(oldLogin.Success);

        // Act 4: Login with NEW password -> MUST SUCCEED
        var newLogin = await authService.LoginAsync(new LoginRequestDto
        {
            Email = "recovery@israel.com",
            Password = "BrandNewPassword2026!#"
        });
        Assert.True(newLogin.Success);
        Assert.NotNull(newLogin.Data?.Token);

        // Act 5: Reusing the same reset token -> MUST FAIL (Single-use token)
        var reuseResult = await authService.ResetPasswordAsync(new ResetPasswordRequestDto
        {
            Email = "recovery@israel.com",
            ResetToken = resetToken!,
            NewPassword = "AnotherPassword999!"
        });
        Assert.False(reuseResult.Success);
        Assert.Contains("Invalid or expired", reuseResult.Message);
    }

    [Fact]
    public async Task RefreshTokenRotation_GeneratesNewPairAndRevokesOld()
    {
        // Arrange
        var (context, hasher, jwt, audit) = CreateTestServices("RefreshRotation_Db");
        var authService = new AuthenticationService(context, hasher, jwt, audit);

        var (hash, salt) = hasher.HashPassword("Pass123!");
        var initialRefreshToken = jwt.GenerateRefreshToken();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "refreshtest@tomergroup.com",
            PasswordHash = hash,
            Salt = salt,
            Role = UserRole.Manager,
            RefreshToken = initialRefreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7),
            IsActive = true
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act: Refresh token
        var refreshResult = await authService.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            UserId = user.Id,
            RefreshToken = initialRefreshToken
        });

        // Assert
        Assert.True(refreshResult.Success);
        Assert.NotNull(refreshResult.Data);
        Assert.NotEmpty(refreshResult.Data.Token);
        Assert.NotEmpty(refreshResult.Data.RefreshToken);
        Assert.NotEqual(initialRefreshToken, refreshResult.Data.RefreshToken); // MUST rotate to new refresh token

        // Reusing OLD refresh token -> MUST FAIL
        var reuseOldResult = await authService.RefreshTokenAsync(new RefreshTokenRequestDto
        {
            UserId = user.Id,
            RefreshToken = initialRefreshToken
        });

        Assert.False(reuseOldResult.Success);
    }

    [Fact]
    public void All8Roles_HaveDistinctRolePoliciesDefined()
    {
        // Assert all 8 roles
        Assert.Equal("Admin", RolePolicies.Admin);
        Assert.Equal("Manager", RolePolicies.Manager);
        Assert.Equal("Sales", RolePolicies.Sales);
        Assert.Equal("Operations", RolePolicies.Operations);
        Assert.Equal("Finance", RolePolicies.Finance);
        Assert.Equal("Guide", RolePolicies.Guide);
        Assert.Equal("Driver", RolePolicies.Driver);
        Assert.Equal("Customer", RolePolicies.Customer);

        // Assert policy names
        Assert.Equal("RequireAdmin", RolePolicies.RequireAdmin);
        Assert.Equal("RequireManagerOrAdmin", RolePolicies.RequireManagerOrAdmin);
        Assert.Equal("RequireSales", RolePolicies.RequireSales);
        Assert.Equal("RequireOperations", RolePolicies.RequireOperations);
        Assert.Equal("RequireFinance", RolePolicies.RequireFinance);
        Assert.Equal("RequireGuide", RolePolicies.RequireGuide);
        Assert.Equal("RequireDriver", RolePolicies.RequireDriver);
        Assert.Equal("RequireCustomer", RolePolicies.RequireCustomer);
        Assert.Equal("RequireStaff", RolePolicies.RequireStaff);
    }
}
