using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class ChatMessageItem
{
    public string Sender { get; set; } = "AI"; // "User" or "AI"
    public string Message { get; set; } = string.Empty;
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("HH:mm");
    public bool IsUser => Sender == "User";
    public bool IsAI => !IsUser;
}

public partial class CustomerAIAssistantViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService _secureStorage;

    [ObservableProperty]
    private string _userInput = string.Empty;

    [ObservableProperty]
    private bool _isThinking;

    private TripDto? _cachedTrip;

    public ObservableCollection<ChatMessageItem> Messages { get; } = new();

    public CustomerAIAssistantViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IApiClient apiClient,
        ISecureStorageService secureStorage)
        : base(localization, navigation)
    {
        _apiClient = apiClient;
        _secureStorage = secureStorage;

        Title = "Tomer AI Travel Concierge";

        // Initial welcome message
        Messages.Add(new ChatMessageItem
        {
            Sender = "AI",
            Message = "Shalom & Welcome! I am your Tomer Group Peru travel assistant. I can explain your itinerary, answer questions about packing and altitude, or provide Spanish translations for your journey."
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            var token = await _secureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                var profile = await _apiClient.GetMyProfileAsync();
                if (profile.Success && profile.Data != null)
                {
                    var trips = await _apiClient.GetCustomerTripsAsync(profile.Data.Id);
                    if (trips.Success && trips.Data != null && trips.Data.Any())
                    {
                        var activeTrip = trips.Data.FirstOrDefault(t => t.Status == TomerGroup.Core.Enums.TripStatus.InProgress || t.Status == TomerGroup.Core.Enums.TripStatus.Confirmed) ?? trips.Data.First();
                        var detail = await _apiClient.GetTripDetailsAsync(activeTrip.Id);
                        if (detail.Success)
                        {
                            _cachedTrip = detail.Data;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AI Assistant init: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput)) return;

        var text = UserInput.Trim();
        UserInput = string.Empty;

        Messages.Add(new ChatMessageItem
        {
            Sender = "User",
            Message = text
        });

        IsThinking = true;

        try
        {
            await Task.Delay(300); // Natural response pacing
            var response = GenerateSafeResponse(text);

            Messages.Add(new ChatMessageItem
            {
                Sender = "AI",
                Message = response
            });
        }
        finally
        {
            IsThinking = false;
        }
    }

    [RelayCommand]
    public void SendPredefinedPrompt(string promptText)
    {
        UserInput = promptText;
        _ = SendMessageAsync();
    }

    private string GenerateSafeResponse(string query)
    {
        var lower = query.ToLowerInvariant();

        // 1. Translation requests
        if (lower.Contains("translate") || lower.Contains("תרגם") || lower.Contains("spanish") || lower.Contains("ספרדית"))
        {
            if (lower.Contains("water") || lower.Contains("מים"))
                return "In Spanish: 'Agua sin gas, por favor' (Still water, please). In Hebrew: מים ללא גז, בבקשה.";
            if (lower.Contains("bill") || lower.Contains("חשבון"))
                return "In Spanish: 'La cuenta, por favor' (The check/bill, please). In Hebrew: חשבון בבקשה.";
            if (lower.Contains("bathroom") || lower.Contains("שירותים"))
                return "In Spanish: '¿Dónde está el baño?' (Where is the restroom?). In Hebrew: איפה השירותים?";
            if (lower.Contains("thank") || lower.Contains("תודה"))
                return "In Spanish: 'Muchas gracias' (Thank you very much). In Hebrew: תודה רבה.";

            return "Useful Peru travel phrases:\n• 'Hola, buenos días' (Hello, good morning)\n• '¿Cuánto cuesta?' (How much does it cost?)\n• 'La cuenta, por favor' (The bill, please)\n• 'Muchas gracias' (Thank you very much)\n• 'Ayuda, por favor' (Help, please)";
        }

        // 2. Altitude sickness & Soroche advice
        if (lower.Contains("altitude") || lower.Contains("soroche") || lower.Contains("גובה") || lower.Contains("סורוצ'ה") || lower.Contains("headache"))
        {
            return "Cusco sits at 3,400 meters. Key recommendations:\n1. Sip warm Muña or Coca tea.\n2. Avoid heavy meals and alcohol on day 1.\n3. Drink 3-4 liters of water daily.\n4. Walk slowly. If you need supplemental oxygen, all partner hotels offer oxygen concentrators.\nOur 24/7 doctor on call can be dispatched via Tomer Group operations (+51 984 231961).";
        }

        // 3. What to bring / Packing advice
        if (lower.Contains("bring") || lower.Contains("pack") || lower.Contains("מה להביא") || lower.Contains("לארוז") || lower.Contains("ציוד"))
        {
            return "For Andean day tours:\n• Layered clothing (cold morning, warm sunny afternoon)\n• Waterproof rain jacket or poncho\n• Sturdy hiking or walking shoes\n• Sun protection: SPF 50+ sunscreen, UV sunglasses, sun hat\n• Original physical passport (mandatory for Machu Picchu admission & trains)\n• Small daypack with bottled water and cash in Peruvian Soles (PEN)";
        }

        // 4. Machu Picchu information
        if (lower.Contains("machu picchu") || lower.Contains("מאצ'ו פיצ'ו"))
        {
            return "Machu Picchu Citadel is located at 2,430m in the cloud forest.\n• Temperature is warmer and more humid than Cusco (~22°C).\n• Mandatory: You MUST present your original passport matching your permit.\n• Standard circuits take 2.5 to 3 hours.\n• Trekking poles are only permitted with rubber protective tips.\n• Large backpacks (>40x35x20cm) must be checked at the entrance lockers.";
        }

        // 5. Itinerary / Next Up inquiries based on real customer data
        if (lower.Contains("tomorrow") || lower.Contains("today") || lower.Contains("itinerary") || lower.Contains("מחר") || lower.Contains("לו\"ז") || lower.Contains("מסלול"))
        {
            if (_cachedTrip != null && _cachedTrip.Days.Any())
            {
                var day1 = _cachedTrip.Days.First();
                var activities = day1.Activities.Select(a => $"• {a.StartTime:hh\\:mm} - {a.Title} ({a.Location})");
                return $"Your upcoming itinerary for {_cachedTrip.Title}:\nDay {day1.DayNumber}: {day1.Title}\n" + string.Join("\n", activities);
            }
            return "To view your exact itinerary, your booking must be confirmed with active dates. You can check the 'My Trip' tab for all day-by-day activities and pickup times.";
        }

        // 6. Security guardrail: NEVER invent financial/payment/driver data
        if (lower.Contains("pay") || lower.Contains("credit card") || lower.Contains("מחיר") || lower.Contains("תשלום"))
        {
            return "For security reasons, I do not process or alter payment details. Please check the 'Bookings' tab for verified invoices or contact our finance department via WhatsApp (+51 984 231961).";
        }

        // Default travel advice
        return "I am here to support your Peru adventure! You can ask me about:\n• Today's and tomorrow's itinerary\n• Recommended gear and what to bring\n• Altitude acclimatization (Soroche)\n• Quick Spanish translation phrases\n• Machu Picchu visitor guidelines";
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Navigation.GoBackAsync();
    }
}
