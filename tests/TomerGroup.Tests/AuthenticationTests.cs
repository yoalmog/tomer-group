using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Security;
using Xunit;

namespace TomerGroup.Tests;

public class AuthenticationTests
{
    private readonly IConfiguration _configuration;

    public AuthenticationTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", "TomerGroupSuperSecretKeyForPeruTravelExperience2026!@#$998877" },
            { "JwtSettings:Issuer", "TomerGroupApi" },
            { "JwtSettings:Audience", "TomerGroupApp" },
            { "JwtSettings:ExpiryMinutes", "60" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public void PasswordHasher_ShouldCorrectlyHashAndVerify()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "SecureTravelerPass2026!@#";

        // Act
        var (hash, salt) = hasher.HashPassword(password);

        // Assert
        Assert.NotEmpty(hash);
        Assert.NotEmpty(salt);
        Assert.True(hasher.VerifyPassword(password, hash, salt));
        Assert.False(hasher.VerifyPassword("WrongPassword123!", hash, salt));
    }

    [Fact]
    public void JwtTokenService_ShouldGenerateValidTokenWithRoleClaims()
    {
        // Arrange
        var jwtService = new JwtTokenService(_configuration);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@tomergroup.com",
            FirstName = "Yossi",
            LastName = "Cohen",
            Role = UserRole.Admin,
            PreferredLanguage = "he"
        };

        // Act
        var token = jwtService.GenerateAccessToken(user);

        // Assert
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("TomerGroupApi", jwtToken.Issuer);
        Assert.Equal(user.Id.ToString(), jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("admin@tomergroup.com", jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void RefreshToken_ShouldBeUniqueAndSecure()
    {
        // Arrange
        var jwtService = new JwtTokenService(_configuration);

        // Act
        var token1 = jwtService.GenerateRefreshToken();
        var token2 = jwtService.GenerateRefreshToken();

        // Assert
        Assert.NotEmpty(token1);
        Assert.NotEmpty(token2);
        Assert.NotEqual(token1, token2);
    }
}

