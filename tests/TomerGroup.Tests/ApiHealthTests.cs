using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TomerGroup.Api.Controllers;
using TomerGroup.Core.DTOs;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class ApiHealthTests
{
    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthyWithTomerGroupBranding()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase("HealthTestDb")
            .Options;

        using var context = new TomerDbContext(options);
        var brandingService = new BrandingService(context);
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DatabaseProvider", "PostgreSQL" }
            })
            .Build();

        var controller = new HealthController(context, brandingService, config);

        // Act
        var actionResult = await controller.GetHealth();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<HealthStatusDto>>(okResult.Value);

        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("Healthy", apiResponse.Data.Status);
        Assert.Equal("Tomer Group", apiResponse.Data.Brand);
        Assert.Equal("Peru Travel Experience", apiResponse.Data.Tagline);
        Assert.Contains("PostgreSQL", apiResponse.Data.Database);
        Assert.True(apiResponse.Data.Subsystems.ContainsKey("API Engine"));
        Assert.True(apiResponse.Data.Subsystems.ContainsKey("Customer Data Isolation"));
    }
}

