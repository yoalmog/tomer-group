using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Api.Controllers;
using TomerGroup.Api.Middleware;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase13SecurityTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public void RateLimiter_EnforcesAuthEndpointLimit_AndResets()
    {
        var limiter = new RateLimitingService();
        var clientIp = "192.168.1.100";

        // Auth endpoint allows 5 requests
        for (int i = 0; i < 5; i++)
        {
            var status = limiter.CheckRateLimit(clientIp, "Auth");
            Assert.True(status.IsAllowed, $"Request {i + 1} should be allowed");
            Assert.Equal(4 - i, status.RemainingRequests);
            Assert.Equal(5, status.LimitPerWindow);
        }

        // 6th request must be blocked
        var blockedStatus = limiter.CheckRateLimit(clientIp, "Auth");
        Assert.False(blockedStatus.IsAllowed, "6th auth attempt should be blocked");
        Assert.Equal(0, blockedStatus.RemainingRequests);
        Assert.True(blockedStatus.WindowResetSeconds > 0, "WindowResetSeconds should be > 0");

        // Reset for that client key
        limiter.ResetLimit(clientIp);
        var allowedAfterReset = limiter.CheckRateLimit(clientIp, "Auth");
        Assert.True(allowedAfterReset.IsAllowed, "Request after reset should be allowed");
        Assert.Equal(4, allowedAfterReset.RemainingRequests);
    }

    [Fact]
    public void SecuritySanitizer_MasksPassportAndIsraeliId_Correctly()
    {
        var sanitizer = new SecuritySanitizerService();

        // Passport masking
        Assert.Equal("****1024", sanitizer.MaskPassport("24891024"));
        Assert.Equal("******5678", sanitizer.MaskPassport("AB12345678"));
        Assert.Equal("****", sanitizer.MaskPassport("123"));
        Assert.Equal(string.Empty, sanitizer.MaskPassport(null));

        // Teudat Zehut (Israeli ID) masking
        Assert.Equal("****8472", sanitizer.MaskIsraelId("31928472"));
        Assert.Equal("******9012", sanitizer.MaskIsraelId("0394789012"));
        Assert.Equal(string.Empty, sanitizer.MaskIsraelId(string.Empty));

        // Credit Card masking
        Assert.Equal("**** **** **** 4321", sanitizer.MaskCreditCard("4580 1234 5678 4321"));
    }

    [Fact]
    public void SecuritySanitizer_SanitizesXssAndPathTraversal_PreventingAttacks()
    {
        var sanitizer = new SecuritySanitizerService();

        // XSS sanitization
        var dirtyHtml = "<script>alert('pwned')</script>Hello <b>World</b>";
        var cleaned = sanitizer.SanitizeInput(dirtyHtml);
        Assert.DoesNotContain("<script>", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", cleaned, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hello bWorld/b", cleaned); // angle brackets removed

        // Directory traversal sanitization check
        Assert.False(sanitizer.IsPathTraversalSafe("../../etc/passwd"));
        Assert.False(sanitizer.IsPathTraversalSafe("..\\..\\windows\\system32\\config.sys"));
        Assert.False(sanitizer.IsPathTraversalSafe("vouchers/../../secret.pdf"));
        Assert.True(sanitizer.IsPathTraversalSafe("voucher_12345_confirmed.pdf"));
    }

    [Fact]
    public async Task SecurityAudit_LogsEvent_AndRetrievesWithFilters()
    {
        var context = CreateInMemoryContext();
        var auditService = new SecurityAuditService(context);

        var userId = Guid.NewGuid();
        var userEmail = "security.admin@tomergroup.pe";
        var ip = "200.48.100.5";

        // Seed 3 different audit logs
        context.AuditLogs.AddRange(
            new TomerGroup.Core.Models.AuditLog
            {
                Id = Guid.NewGuid(),
                Action = "LOGIN_SUCCESS",
                EntityName = "User",
                EntityId = userId.ToString(),
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ip,
                MetadataJson = "{\"Client\":\"Mobile-Android\"}",
                CreatedAt = DateTime.UtcNow
            },
            new TomerGroup.Core.Models.AuditLog
            {
                Id = Guid.NewGuid(),
                Action = "RATE_LIMIT_EXCEEDED",
                EntityName = "RateLimit",
                IpAddress = "192.168.1.55",
                MetadataJson = "{\"Endpoint\":\"/api/auth/login\"}",
                CreatedAt = DateTime.UtcNow
            },
            new TomerGroup.Core.Models.AuditLog
            {
                Id = Guid.NewGuid(),
                Action = "PASSPORT_VIEWED",
                EntityName = "Customer",
                EntityId = Guid.NewGuid().ToString(),
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ip,
                MetadataJson = "{\"Reason\":\"VoucherGeneration\"}",
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        // Query by action filter
        var rateLimitFilter = new AuditLogFilterDto { Action = "RATE_LIMIT_EXCEEDED" };
        var rateLimitResult = await auditService.GetAuditLogsAsync(rateLimitFilter);

        Assert.True(rateLimitResult.Success);
        Assert.NotNull(rateLimitResult.Data);
        Assert.Single(rateLimitResult.Data.Items);
        Assert.Equal("RATE_LIMIT_EXCEEDED", rateLimitResult.Data.Items[0].Action);
        Assert.Equal("192.168.1.55", rateLimitResult.Data.Items[0].IpAddress);

        // Query by user id filter
        var userFilter = new AuditLogFilterDto { UserId = userId };
        var userResult = await auditService.GetAuditLogsAsync(userFilter);

        Assert.True(userResult.Success);
        Assert.NotNull(userResult.Data);
        Assert.Equal(2, userResult.Data.Items.Count);
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_InjectsExpectedOwaspHeaders()
    {
        var middleware = new SecurityHeadersMiddleware((innerHttpContext) =>
        {
            innerHttpContext.Response.StatusCode = 200;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Response.Body = new System.IO.MemoryStream();

        await middleware.InvokeAsync(context);
        await context.Response.StartAsync();

        Assert.True(context.Response.Headers.ContainsKey("X-Content-Type-Options"));
        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);

        Assert.True(context.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);

        Assert.True(context.Response.Headers.ContainsKey("X-XSS-Protection"));
        Assert.Equal("1; mode=block", context.Response.Headers["X-XSS-Protection"]);

        Assert.True(context.Response.Headers.ContainsKey("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);

        Assert.True(context.Response.Headers.ContainsKey("Content-Security-Policy"));
        Assert.True(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
    }

    [Fact]
    public void SecurityController_RateLimitAndSanitizePreview_ReturnsExpectedPayload()
    {
        var limiter = new RateLimitingService();
        var sanitizer = new SecuritySanitizerService();
        var context = CreateInMemoryContext();
        var audit = new SecurityAuditService(context);

        var controller = new SecurityController(limiter, sanitizer, audit);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var rateLimitActionResult = controller.GetRateLimitStatus("Auth") as OkObjectResult;
        Assert.NotNull(rateLimitActionResult);
        var apiResp = rateLimitActionResult.Value as ApiResponse<RateLimitStatusDto>;
        Assert.NotNull(apiResp);
        Assert.True(apiResp.Data!.IsAllowed);

        var previewRequest = new SanitizePreviewRequest
        {
            RawInput = "<b>test</b><script>bad</script>",
            PassportNumber = "IL98765432"
        };
        var previewResult = controller.SanitizePreview(previewRequest) as OkObjectResult;
        Assert.NotNull(previewResult);
    }
}
