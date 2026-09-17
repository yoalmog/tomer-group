using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TomerGroup.Api.Controllers;
using TomerGroup.Core.DTOs;
using TomerGroup.Infrastructure.Configuration;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase15ProductionHardeningTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TomerDbContext(options);
    }

    private IConfiguration CreateTestConfiguration(Dictionary<string, string?>? customValues = null)
    {
        var values = new Dictionary<string, string?>
        {
            { "DatabaseProvider", "InMemory" },
            { "ConnectionStrings:DefaultConnection", "Host=localhost;Database=tomergroup_db;Username=postgres;Password=postgres" },
            { "JwtSettings:Secret", "TomerGroupSuperSecretKeyForPeruTravelExperience2026!@#$998877" },
            { "JwtSettings:Issuer", "TomerGroupApi" },
            { "JwtSettings:Audience", "TomerGroupApp" },
            { "BrandSettings:AgencyName", "Tomer Group" },
            { "BrandSettings:Tagline", "Peru Travel Experience" },
            { "BrandSettings:PrimaryColor", "#BC225E" },
            { "BrandSettings:ContactPhone", "+51 84 223 456" }
        };

        if (customValues != null)
        {
            foreach (var kv in customValues)
            {
                values[kv.Key] = kv.Value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    [Fact]
    public async Task HealthController_GetHealth_ReturnsFullSubsystemsAndHealthyStatus()
    {
        var context = CreateInMemoryContext();
        var config = CreateTestConfiguration();
        var branding = new BrandingService(context);

        var controller = new HealthController(context, branding, config);

        var actionResult = await controller.GetHealth() as OkObjectResult;
        Assert.NotNull(actionResult);
        var apiResp = actionResult.Value as ApiResponse<HealthStatusDto>;
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);

        var health = apiResp.Data!;
        Assert.Equal("Healthy", health.Status);
        Assert.Equal("Tomer Group", health.Brand);
        Assert.Contains("AI Assistant Engine", health.Subsystems.Keys);
        Assert.Contains("Offline Sync Gateway", health.Subsystems.Keys);
        Assert.Contains("WhatsApp Integration", health.Subsystems.Keys);
        Assert.Contains("Security Rate Limiting & OWASP Headers", health.Subsystems.Keys);
        Assert.Contains("Executive Analytics Engine", health.Subsystems.Keys);
    }

    [Fact]
    public void HealthController_GetLiveness_ReturnsProcessStatsAndLiveStatus()
    {
        var context = CreateInMemoryContext();
        var config = CreateTestConfiguration();
        var branding = new BrandingService(context);

        var controller = new HealthController(context, branding, config);

        var actionResult = controller.GetLiveness() as OkObjectResult;
        Assert.NotNull(actionResult);
        var apiResp = actionResult.Value as ApiResponse<LivenessStatusDto>;
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);
        Assert.Equal("Live", apiResp.Data!.Status);
        Assert.True(apiResp.Data.MemoryUsageMb > 0);
        Assert.True(apiResp.Data.ProcessId > 0);
    }

    [Fact]
    public async Task HealthController_GetReadiness_WithInMemoryDb_ReturnsReadyAndSubsystems()
    {
        var context = CreateInMemoryContext();
        var config = CreateTestConfiguration();
        var branding = new BrandingService(context);

        var controller = new HealthController(context, branding, config);

        var actionResult = await controller.GetReadiness() as OkObjectResult;
        Assert.NotNull(actionResult);
        var apiResp = actionResult.Value as ApiResponse<ReadinessStatusDto>;
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Success);
        Assert.Equal("Ready", apiResp.Data!.Status);
        Assert.Equal("Connected", apiResp.Data.Database);
        Assert.Equal("Ready", apiResp.Data.Subsystems["OfflineSync"]);
        Assert.Equal("Enforced", apiResp.Data.Subsystems["SecurityHeaders"]);
    }

    [Fact]
    public void ConfigurationValidator_ValidSettings_PassesWithZeroErrors()
    {
        var config = CreateTestConfiguration();
        var validation = ConfigurationValidator.ValidateProductionSettings(config);

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void ConfigurationValidator_InsecureJwtSecret_ReportsValidationError()
    {
        var config = CreateTestConfiguration(new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", "short-secret" } // only 12 chars
        });

        var validation = ConfigurationValidator.ValidateProductionSettings(config);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, e => e.Contains("too short") || e.Contains("32 characters"));
    }

    [Fact]
    public void ConfigurationValidator_InvalidHexColor_ReportsValidationError()
    {
        var config = CreateTestConfiguration(new Dictionary<string, string?>
        {
            { "BrandSettings:PrimaryColor", "not-a-hex-color" }
        });

        var validation = ConfigurationValidator.ValidateProductionSettings(config);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, e => e.Contains("hex color"));
    }
}
