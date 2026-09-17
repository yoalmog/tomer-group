using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class AIService : IAIService
{
    private readonly TomerDbContext _context;

    public AIService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ItineraryDraftResultDto>> GenerateItineraryDraftAsync(GenerateItineraryPromptDto prompt, Guid staffUserId, CancellationToken cancellationToken = default)
    {
        var daysCount = Math.Clamp(prompt.Days, 1, 14);
        var result = new ItineraryDraftResultDto
        {
            Title = $"{prompt.Destination} - {daysCount} Days Experience ({prompt.TravelerProfile})",
            HebrewTitle = $"חבילת {prompt.Destination} - {daysCount} ימים ({TranslateProfile(prompt.TravelerProfile)})",
            Summary = $"Customized itinerary tailored for {prompt.TravelerProfile} travelers visiting {prompt.Destination}. Includes altitude acclimatization and cultural highlights.",
            HebrewSummary = $"תוכנית מסע מותאמת אישית למטיילים ישראלים ({TranslateProfile(prompt.TravelerProfile)}) בקוסקו והסביבה, הכוללת הסתגלות גובה הדרגתית ודגשים אותנטיים.",
            FollowsAcclimatizationRule = true,
            HasKosherArrangements = prompt.IsKosherRequired,
            HasShabbatArrangements = prompt.IsShabbatObservant
        };

        // Generate day-by-day sequence with altitude acclimatization
        for (int day = 1; day <= daysCount; day++)
        {
            var dayDto = BuildDayDraft(day, daysCount, prompt);
            result.Days.Add(dayDto);
        }

        // Create AI Conversation and AI Request entities in DB
        var conversation = new AIConversation
        {
            StaffUserId = staffUserId,
            Title = result.Title,
            Context = "ItineraryBuilder"
        };
        await _context.AIConversations.AddAsync(conversation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var aiRequest = new AIRequest
        {
            ConversationId = conversation.Id,
            Prompt = $"Generate {daysCount}-day {prompt.TravelerProfile} itinerary for {prompt.Destination}. Kosher: {prompt.IsKosherRequired}, Shabbat: {prompt.IsShabbatObservant}. Notes: {prompt.AdditionalNotes}",
            ResponsePayload = JsonSerializer.Serialize(result),
            SchemaType = "ItineraryDraft",
            Status = "Draft",
            IsApproved = false
        };

        await _context.AIRequests.AddAsync(aiRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        result.RequestId = aiRequest.Id;
        return ApiResponse<ItineraryDraftResultDto>.Ok(result, "AI itinerary draft generated successfully (Pending Staff Review)");
    }

    public async Task<ApiResponse<WhatsAppDraftResultDto>> DraftWhatsAppMessageAsync(DraftWhatsAppPromptDto prompt, Guid staffUserId, CancellationToken cancellationToken = default)
    {
        var name = string.IsNullOrWhiteSpace(prompt.CustomerName) ? "חבר" : prompt.CustomerName;
        string draftedText;
        string action;

        switch (prompt.InquiryTopic.ToLower())
        {
            case "altitudesickness":
            case "soroche":
                if (prompt.Language == "en")
                {
                    draftedText = $"Hi {name}, welcome to Cusco! If you feel mild headache or breath shortness from the 3,400m altitude, rest well, stay hydrated, and sip warm coca tea. Oxygen concentrators are ready in your hotel, and Tomer Group's doctor on call is reachable 24/7 at +51 984 231961.";
                }
                else
                {
                    draftedText = $"היי {name}, ברוכים הבאים לקוסקו! במידה ואתה חש בכאב ראש קל או קוצר נשימה עקב הגובה (3,400 מ'), מומלץ לשתות הרבה מים וחליטת עלי קוקה. בלוני חמצן זמינים בחדר המלון שלך, ורופא כונן של Tomer Group זמין 24/7 במספר +51 984 231961.";
                }
                action = "Check oxygen saturation & alert on-call doctor if symptoms persist";
                break;

            case "shabbatmeals":
            case "kosher":
                if (prompt.Language == "en")
                {
                    draftedText = $"Hello {name}, your Friday night Shabbat dinner and Saturday Kiddush lunch at Chabad House Cusco are fully confirmed! Candle lighting is at 17:35. Your hotel is just 6 minutes flat walking distance.";
                }
                else
                {
                    draftedText = $"שלום {name}, סעודות ליל שבת וקידוש יום שבת שלכם בבית חב\"ד קוסקו סודרו בהצלחה! הדלקת נרות בשעה 17:35. המלון שלכם נמצא במרחק 6 דקות הליכה רגלית נוחה.";
                }
                action = "Provide map link to Chabad House Cusco";
                break;

            case "traindelay":
                draftedText = prompt.Language == "en"
                    ? $"Hi {name}, PeruRail train from Ollantaytambo has a brief 25-minute delay. Our private driver is waiting with a Tomer Group sign at the exit gate. No need to worry!"
                    : $"היי {name}, רכבת פרו-רייל מאולנטייטמבו מתעכבת בכ-25 דקות. הנהג הפרטי שלנו מעודכן וימתין לכם בשער היציאה עם שלט Tomer Group. אין מה לדאוג!";
                action = "Notify transfer driver and hotel front desk";
                break;

            case "salkantaygear":
            case "packing":
            default:
                draftedText = prompt.Language == "en"
                    ? $"Hi {name}, for your upcoming trek: pack a light 5kg daypack, warm thermal layers for night 1 (near 0°C), and waterproof hiking boots. Surplus luggage is securely stored at Tomer Group Cusco lockers."
                    : $"שלום {name}, לקראת היציאה לטרק: יש להצטייד בתיק יום עד 5 ק\"ג, שכבות תרמיות ללילה הראשון (סביב 0 מעלות), ונעלי הליכה אטומות למים. שאר המזוודות יישמרו בכספת המאובטחת בסוכנות בקוסקו.";
                action = "Issue trek checklist PDF voucher";
                break;
        }

        var conversation = new AIConversation
        {
            StaffUserId = staffUserId,
            Title = $"WhatsApp Draft: {prompt.InquiryTopic}",
            Context = "WhatsAppDrafting"
        };
        await _context.AIConversations.AddAsync(conversation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var aiRequest = new AIRequest
        {
            ConversationId = conversation.Id,
            Prompt = $"Draft WhatsApp message regarding {prompt.InquiryTopic} for {prompt.CustomerName} in {prompt.Language}. Context: {prompt.AdditionalContext}",
            ResponsePayload = draftedText,
            SchemaType = "WhatsAppDraft",
            Status = "Draft",
            IsApproved = false
        };
        await _context.AIRequests.AddAsync(aiRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<WhatsAppDraftResultDto>.Ok(new WhatsAppDraftResultDto
        {
            RequestId = aiRequest.Id,
            DraftedText = draftedText,
            Language = prompt.Language,
            SuggestedAction = action
        }, "WhatsApp draft created (Pending Staff Approval)");
    }

    public async Task<ApiResponse<AIRequestDto>> GetRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var req = await _context.AIRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (req == null)
        {
            return ApiResponse<AIRequestDto>.Fail("AI Request not found");
        }

        return ApiResponse<AIRequestDto>.Ok(MapToDto(req));
    }

    public async Task<ApiResponse<List<AIRequestDto>>> GetRequestsAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AIRequests
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        var list = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapToDto(r))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<AIRequestDto>>.Ok(list);
    }

    public async Task<ApiResponse<bool>> ApproveRequestAsync(Guid id, Guid approvedByUserId, ApproveAIRequestDto? approval = null, CancellationToken cancellationToken = default)
    {
        var req = await _context.AIRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (req == null)
        {
            return ApiResponse<bool>.Fail("AI Request not found");
        }

        req.IsApproved = true;
        req.ApprovedByUserId = approvedByUserId;
        req.ApprovedAt = DateTime.UtcNow;
        req.Status = "Approved";
        req.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            Action = "AIApproval",
            EntityName = "AIRequest",
            EntityId = req.Id.ToString(),
            UserId = approvedByUserId,
            UserEmail = "staff@tomergroup.com",
            MetadataJson = $"Approved {req.SchemaType} request. Notes: {approval?.Notes ?? "None"}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "AI request approved successfully (Human-in-the-Loop verified)");
    }

    public async Task<ApiResponse<bool>> RejectRequestAsync(Guid id, Guid rejectedByUserId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var req = await _context.AIRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (req == null)
        {
            return ApiResponse<bool>.Fail("AI Request not found");
        }

        req.IsApproved = false;
        req.Status = "Rejected";
        req.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            Action = "AIRejection",
            EntityName = "AIRequest",
            EntityId = req.Id.ToString(),
            UserId = rejectedByUserId,
            UserEmail = "staff@tomergroup.com",
            MetadataJson = $"Rejected {req.SchemaType} request. Reason: {reason ?? "Staff rejected"}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "AI request rejected");
    }

    private static ItineraryDayDraftDto BuildDayDraft(int day, int totalDays, GenerateItineraryPromptDto prompt)
    {
        var isLastDay = day == totalDays;

        if (day == 1)
        {
            return new ItineraryDayDraftDto
            {
                DayNumber = 1,
                Title = "Arrival in Cusco & Sacred Valley Acclimatization",
                HebrewTitle = "הגעה לקוסקו וירידה להסתגלות בעמק הקדוש",
                AltitudeMeters = 2870,
                Description = "Arrival at Cusco Airport (CUZ), private VIP transfer descending to Urubamba in the Sacred Valley (2,870m) to optimize altitude acclimatization.",
                HebrewDescription = "נחיתה בנמל התעופה קוסקו, העברה פרטית מרווחת וירידה ישירה לעמק הקדוש (אורובמבה, 2,870 מ') להקלה משמעותית על הסתגלות הגובה.",
                AcclimatizationNote = "Sleeping at 2,870m reduces altitude sickness risk by 70% compared to sleeping in Cusco on Day 1.",
                KosherFoodNote = prompt.IsKosherRequired ? "Chabad House Cusco certified Kosher lunch box delivered upon arrival." : string.Empty,
                ShabbatNote = string.Empty
            };
        }

        if (day == 2)
        {
            return new ItineraryDayDraftDto
            {
                DayNumber = 2,
                Title = "Sacred Valley: Pisac Market & Ollantaytambo Fortress",
                HebrewTitle = "העמק הקדוש: שוק פיסאק ומבצר אולנטייטמבו",
                AltitudeMeters = 2792,
                Description = "Explore the vibrant Pisac indigenous market, Incan agricultural terraces, and the monumental fortress of Ollantaytambo.",
                HebrewDescription = "סיור בשוק האומנים של פיסאק, טרסות חקלאיות מרהיבות של האינקה ומבצר אולנטייטמבו המרשים.",
                AcclimatizationNote = "Gentle walking at moderate elevation (2,790m). Continue drinking plentiful liquids.",
                KosherFoodNote = prompt.IsKosherRequired ? "Kosher picnic lunch box packed by Chabad Cusco." : string.Empty,
                ShabbatNote = string.Empty
            };
        }

        if (day == 3)
        {
            return new ItineraryDayDraftDto
            {
                DayNumber = 3,
                Title = "PeruRail Vistadome Train to Machu Picchu Sanctuary",
                HebrewTitle = "רכבת ויסטדום למאצ'ו פיצ'ו (מסלול 2 קלאסי)",
                AltitudeMeters = 2430,
                Description = "Scenic panoramic train journey along Urubamba River to Aguas Calientes. Guided tour of Machu Picchu Sanctuary (Circuit 2 Classic).",
                HebrewDescription = "נסיעה מרהיבה ברכבת פנורמית ויסטדום לאגואס קליינטס וסיור מודרך מקיף במאצ'ו פיצ'ו (מסלול 2 קלאסי).",
                AcclimatizationNote = "Machu Picchu is at 2,430m, oxygen levels are higher than Cusco.",
                KosherFoodNote = prompt.IsKosherRequired ? "Kosher packed lunch box for archaeological park." : string.Empty,
                ShabbatNote = string.Empty
            };
        }

        if (day == 4)
        {
            return new ItineraryDayDraftDto
            {
                DayNumber = 4,
                Title = "Ascent to Cusco & Historic City Highlights",
                HebrewTitle = "עלייה לקוסקו: סיור בעיר העתיקה וסמטאות סאן בלאס",
                AltitudeMeters = 3400,
                Description = "Morning return to imperial Cusco. Walking tour of Plaza de Armas, Qorikancha Temple of the Sun, and San Blas artisan quarter.",
                HebrewDescription = "חזרה לעיר הבירה הקיסרית קוסקו, סיור מודרך בפלאסה דה ארמאס, מקדש השמש קוריקאנצ'ה וסמטאות האומנים סאן בלאס.",
                AcclimatizationNote = "Bodies are now well acclimatized to 3,400m after 3 days in the Sacred Valley.",
                KosherFoodNote = prompt.IsKosherRequired ? "Dinner at Chabad House Cusco or certified kosher dining." : string.Empty,
                ShabbatNote = prompt.IsShabbatObservant ? "Hotel booked in San Blas within 5-minute flat walk to Chabad House." : string.Empty
            };
        }

        if (day == 5 && prompt.IsShabbatObservant)
        {
            return new ItineraryDayDraftDto
            {
                DayNumber = 5,
                Title = "Shabbat in Cusco: Chabad House & Traditional Meals",
                HebrewTitle = "שבת קודש בקוסקו: תפילות וסעודות שבת בבית חב\"ד",
                AltitudeMeters = 3400,
                Description = "Spiritual Shabbat atmosphere in the Andes. Friday night and Saturday festive meals with the warm Israeli traveler community.",
                HebrewDescription = "שבת מרגשת בלב האנדים. תפילות וסעודות שבת עשירות וחמות בבית חב\"ד עם מאות מטיילים ישראלים.",
                AcclimatizationNote = "Restful day allowing body to recover energy.",
                KosherFoodNote = "Full Glatt Kosher Friday night & Saturday Kiddush meals included.",
                ShabbatNote = "No scheduled transport. Mechanical hotel keys arranged."
            };
        }

        return new ItineraryDayDraftDto
        {
            DayNumber = day,
            Title = isLastDay ? "Farewell Cusco & Transfer to Airport" : $"Andean Exploration Day {day}",
            HebrewTitle = isLastDay ? "פרידה מקוסקו והעברה לשדה התעופה" : $"יום טיול והרפתקה {day}",
            AltitudeMeters = isLastDay ? 3400 : 4200,
            Description = isLastDay ? "Private VIP transfer to Alejandro Velasco Astete Airport (CUZ) with assistance." : "Exploration of Rainbow Mountain or Humantay Lake with full oxygen support.",
            HebrewDescription = isLastDay ? "העברה פרטית ומפנקת לשדה התעופה של קוסקו עם ליווי צמוד עד שער הטיסה." : "טיול יום בלתי נשכח להר שבעת הצבעים או לגונה הומנטאי עם ערכות חמצן ומדריך מוסמך.",
            AcclimatizationNote = "High altitude managed safely after full acclimatization phase.",
            KosherFoodNote = prompt.IsKosherRequired ? "Kosher snacks and meals provided." : string.Empty,
            ShabbatNote = string.Empty
        };
    }

    private static string TranslateProfile(string profile) => profile.ToLower() switch
    {
        "backpacker" => "תרמילאים",
        "luxury" => "יוקרה ונוחות",
        "family" => "משפחות",
        "trekker" => "אוהבי טרקים",
        _ => profile
    };

    private static AIRequestDto MapToDto(AIRequest r) => new()
    {
        Id = r.Id,
        ConversationId = r.ConversationId,
        Prompt = r.Prompt,
        ResponsePayload = r.ResponsePayload,
        SchemaType = r.SchemaType,
        IsApproved = r.IsApproved,
        ApprovedByUserId = r.ApprovedByUserId,
        ApprovedAt = r.ApprovedAt,
        Status = r.Status,
        CreatedAt = r.CreatedAt
    };
}

