using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase10AIAssistantTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_Phase10_{Guid.NewGuid()}")
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task GenerateItineraryDraftAsync_GeneratesValidAcclimatizationProgression()
    {
        using var context = CreateInMemoryContext();
        var aiService = new AIService(context);

        var staffUserId = Guid.NewGuid();
        var prompt = new GenerateItineraryPromptDto
        {
            Destination = "Cusco & Machu Picchu",
            Days = 5,
            TravelerProfile = "Backpacker",
            IsKosherRequired = false,
            IsShabbatObservant = false
        };

        var result = await aiService.GenerateItineraryDraftAsync(prompt, staffUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.Days.Count);
        Assert.True(result.Data.FollowsAcclimatizationRule);

        // Day 1 must descend to Sacred Valley (~2,870m) rather than sleeping high in Cusco
        var day1 = result.Data.Days[0];
        Assert.Equal(1, day1.DayNumber);
        Assert.True(day1.AltitudeMeters < 3000);
        Assert.Contains("2,870", day1.AcclimatizationNote);

        // Day 3 should visit Machu Picchu (2,430m)
        var day3 = result.Data.Days[2];
        Assert.Equal(3, day3.DayNumber);
        Assert.Equal(2430, day3.AltitudeMeters);
    }

    [Fact]
    public async Task GenerateItineraryDraftAsync_KosherAndShabbatObservant_IncludesChabadAndShabbatProvisions()
    {
        using var context = CreateInMemoryContext();
        var aiService = new AIService(context);

        var staffUserId = Guid.NewGuid();
        var prompt = new GenerateItineraryPromptDto
        {
            Destination = "Cusco & Sacred Valley",
            Days = 5,
            TravelerProfile = "Backpacker",
            IsKosherRequired = true,
            IsShabbatObservant = true
        };

        var result = await aiService.GenerateItineraryDraftAsync(prompt, staffUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.HasKosherArrangements);
        Assert.True(result.Data.HasShabbatArrangements);

        // Verify Shabbat day draft contains Chabad meals & mechanical key notice
        var shabbatDay = result.Data.Days.FirstOrDefault(d => d.DayNumber == 5);
        Assert.NotNull(shabbatDay);
        Assert.Contains("חב\"ד", shabbatDay.HebrewTitle);
        Assert.Contains("סעודות שבת", shabbatDay.HebrewDescription);
        Assert.Contains("Mechanical", shabbatDay.ShabbatNote);
    }

    [Fact]
    public async Task GenerateItineraryDraftAsync_CreatesAIRequestInDraftStatus_UnapprovedByDefault()
    {
        using var context = CreateInMemoryContext();
        var aiService = new AIService(context);

        var staffUserId = Guid.NewGuid();
        var prompt = new GenerateItineraryPromptDto
        {
            Destination = "Cusco & Lake Humantay",
            Days = 4,
            TravelerProfile = "Trekker"
        };

        var result = await aiService.GenerateItineraryDraftAsync(prompt, staffUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        var aiRequest = await context.AIRequests.FindAsync(result.Data.RequestId);
        Assert.NotNull(aiRequest);
        Assert.Equal("Draft", aiRequest.Status);
        Assert.False(aiRequest.IsApproved);
        Assert.Null(aiRequest.ApprovedByUserId);
        Assert.Equal("ItineraryDraft", aiRequest.SchemaType);
    }

    [Fact]
    public async Task DraftWhatsAppMessageAsync_AltitudeSickness_GeneratesHebrewMedicalResponseWithOxygenAdvice()
    {
        using var context = CreateInMemoryContext();
        var aiService = new AIService(context);

        var staffUserId = Guid.NewGuid();
        var prompt = new DraftWhatsAppPromptDto
        {
            CustomerName = "דני",
            InquiryTopic = "AltitudeSickness",
            Language = "he"
        };

        var result = await aiService.DraftWhatsAppMessageAsync(prompt, staffUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Contains("דני", result.Data.DraftedText);
        Assert.Contains("3,400 מ'", result.Data.DraftedText);
        Assert.Contains("קוקה", result.Data.DraftedText);
        Assert.Contains("חמצן", result.Data.DraftedText);
        Assert.Contains("+51 984 231961", result.Data.DraftedText);
    }

    [Fact]
    public async Task DraftWhatsAppMessageAsync_ShabbatMeals_GeneratesChabadMealConfirmation()
    {
        using var context = CreateInMemoryContext();
        var aiService = new AIService(context);

        var staffUserId = Guid.NewGuid();
        var prompt = new DraftWhatsAppPromptDto
        {
            CustomerName = "דני",
            InquiryTopic = "ShabbatMeals",
            Language = "he"
        };

        var result = await aiService.DraftWhatsAppMessageAsync(prompt, staffUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Contains("בית חב\"ד קוסקו", result.Data.DraftedText);
        Assert.Contains("הדלקת נרות", result.Data.DraftedText);
        Assert.Contains("הליכה רגלית", result.Data.DraftedText);
    }

    [Fact]
    public async Task ApproveRequestAsync_HumanInTheLoop_TransitionsStatusToApproved_AndLogsAudit()
    {
        using var context = CreateInMemoryContext();
        var aiService = new AIService(context);

        var staffUserId = Guid.NewGuid();
        var prompt = new GenerateItineraryPromptDto
        {
            Destination = "Machu Picchu Express",
            Days = 3
        };

        var draftRes = await aiService.GenerateItineraryDraftAsync(prompt, staffUserId);
        Assert.True(draftRes.Success);

        var managerUserId = Guid.NewGuid();
        var approveRes = await aiService.ApproveRequestAsync(draftRes.Data!.RequestId, managerUserId, new ApproveAIRequestDto
        {
            Notes = "Approved with private van and kosher boxed meals"
        });

        Assert.True(approveRes.Success);

        var updatedReq = await context.AIRequests.FindAsync(draftRes.Data.RequestId);
        Assert.NotNull(updatedReq);
        Assert.True(updatedReq.IsApproved);
        Assert.Equal("Approved", updatedReq.Status);
        Assert.Equal(managerUserId, updatedReq.ApprovedByUserId);
        Assert.NotNull(updatedReq.ApprovedAt);

        // Verify AuditLog
        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "AIApproval");
        Assert.NotNull(audit);
        Assert.Equal(managerUserId, audit.UserId);
        Assert.Contains(draftRes.Data.RequestId.ToString(), audit.EntityId);
    }

    [Fact]
    public async Task RejectRequestAsync_TransitionsStatusToRejected_AndLogsAudit()
    {
        using var context = CreateInMemoryContext();
        var aiService = new AIService(context);

        var staffUserId = Guid.NewGuid();
        var draftRes = await aiService.DraftWhatsAppMessageAsync(new DraftWhatsAppPromptDto
        {
            CustomerName = "דני",
            InquiryTopic = "TrainDelay"
        }, staffUserId);

        Assert.True(draftRes.Success);

        var managerUserId = Guid.NewGuid();
        var rejectRes = await aiService.RejectRequestAsync(draftRes.Data!.RequestId, managerUserId, "Custom wording required for VIP");

        Assert.True(rejectRes.Success);

        var updatedReq = await context.AIRequests.FindAsync(draftRes.Data.RequestId);
        Assert.NotNull(updatedReq);
        Assert.False(updatedReq.IsApproved);
        Assert.Equal("Rejected", updatedReq.Status);

        // Verify AuditLog
        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "AIRejection");
        Assert.NotNull(audit);
        Assert.Equal(managerUserId, audit.UserId);
    }
}

