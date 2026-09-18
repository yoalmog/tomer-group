#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
#endif
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
[QueryProperty(nameof(CustomerIdString), "customerId")]
#endif
public partial class AgencyCustomer360ViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _customerIdString = string.Empty;

    public Guid CustomerId
    {
        get => Guid.TryParse(CustomerIdString, out var g) ? g : Guid.Empty;
        set => CustomerIdString = value.ToString();
    }

    [ObservableProperty]
    private Customer360Dto? _profile;

    [ObservableProperty]
    private int _selectedTabIndex = 0; // 0=Overview, 1=Trips, 2=Bookings, 3=Docs, 4=Payments, 5=Support, 6=Audit

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Collections for Tabs
    public ObservableCollection<TripSummaryDto> Trips { get; } = new();
    public ObservableCollection<BookingSummaryDto> Bookings { get; } = new();
    public ObservableCollection<DocumentItemDto> Documents { get; } = new();
    public ObservableCollection<PaymentItemDto> Payments { get; } = new();
    public ObservableCollection<SupportTicketSummaryDto> SupportTickets { get; } = new();
    public ObservableCollection<AdminActivityFeedItemDto> ActivityLog { get; } = new();

    public AgencyCustomer360ViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
    }

    partial void OnCustomerIdStringChanged(string value)
    {
        if (Guid.TryParse(value, out var id) && id != Guid.Empty)
        {
            _ = LoadCustomer360Async(id);
        }
    }

    [RelayCommand]
    public async Task LoadCustomer360Async(Guid? id = null)
    {
        var targetId = id ?? CustomerId;
        if (targetId == Guid.Empty)
        {
            // If empty, try searching for the first customer
            var searchRes = await _apiClient.SearchGlobalAsync("a");
            if (searchRes.Success && searchRes.Data?.Customers.Any() == true)
            {
                targetId = searchRes.Data.Customers.First().Id;
            }
            else
            {
                return;
            }
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var res = await _apiClient.GetCustomer360Async(targetId);
            if (res.Success && res.Data != null)
            {
                Profile = res.Data;

                Trips.Clear();
                foreach (var t in Profile.Trips) Trips.Add(t);

                Bookings.Clear();
                foreach (var b in Profile.Bookings) Bookings.Add(b);

                Documents.Clear();
                foreach (var d in Profile.Documents) Documents.Add(d);

                Payments.Clear();
                foreach (var p in Profile.Payments) Payments.Add(p);

                SupportTickets.Clear();
                foreach (var st in Profile.SupportTickets) SupportTickets.Add(st);

                ActivityLog.Clear();
                foreach (var a in Profile.ActivityLog) ActivityLog.Add(a);
            }
            else
            {
                HasError = true;
                ErrorMessage = res.Message ?? "Failed to load customer profile";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void SelectTab(int index)
    {
        SelectedTabIndex = index;
    }

    [RelayCommand]
    public void VerifyDocument(DocumentItemDto doc)
    {
        if (doc == null) return;
        doc.Status = "Verified";
        if (Profile != null)
        {
            Profile.VerifiedDocumentsCount++;
        }
    }

    [RelayCommand]
    public void RejectDocument(DocumentItemDto doc)
    {
        if (doc == null) return;
        doc.Status = "Rejected";
        doc.RejectionReason = "Unclear photo / expired passport";
    }

    [RelayCommand]
    public async Task OpenWhatsAppAsync()
    {
        if (Profile == null || string.IsNullOrWhiteSpace(Profile.Phone)) return;
        var cleanPhone = Profile.Phone.Replace("+", "").Replace(" ", "").Replace("-", "");
        var url = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString($"שלום {Profile.FullName}, כאן צוות טומר גרופ קוסקו")}";
        try
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            await Launcher.Default.OpenAsync(new Uri(url));
#else
            await Task.CompletedTask;
#endif
        }
        catch
        {
            // Ignore if launcher fails on simulator
        }
    }
}
