using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Models;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencySupportViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public ObservableCollection<SupportTicketSummaryDto> AllTickets { get; } = new();
    public ObservableCollection<SupportTicketSummaryDto> FilteredTickets { get; } = new();

    [ObservableProperty]
    private SupportTicketSummaryDto? _selectedTicketSummary;

    [ObservableProperty]
    private SupportTicket? _currentTicket;

    public ObservableCollection<SupportTicketMessage> TicketMessages { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _currentStatusFilter = "All";

    // Reply state
    [ObservableProperty]
    private string _replyText = string.Empty;

    [ObservableProperty]
    private bool _isInternalNote = false;

    // Detail modal
    [ObservableProperty]
    private bool _isDetailModalOpen;

    public AgencySupportViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadTicketsAsync();
    }

    [RelayCommand]
    public async Task LoadTicketsAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            AllTickets.Clear();
            FilteredTickets.Clear();

            var res = await _apiClient.GetSupportTicketsAsync();
            if (res.Success && res.Data != null)
            {
                foreach (var t in res.Data)
                {
                    AllTickets.Add(t);
                }
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading tickets: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void SetStatusFilter(string status)
    {
        CurrentStatusFilter = status;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredTickets.Clear();
        foreach (var t in AllTickets)
        {
            if (CurrentStatusFilter == "All" || t.Status.Equals(CurrentStatusFilter, StringComparison.OrdinalIgnoreCase))
            {
                FilteredTickets.Add(t);
            }
        }
    }

    [RelayCommand]
    public async Task OpenTicketDetailAsync(SupportTicketSummaryDto summary)
    {
        if (summary == null) return;
        SelectedTicketSummary = summary;
        IsBusy = true;

        try
        {
            var res = await _apiClient.GetSupportTicketByIdAsync(summary.Id);
            if (res.Success && res.Data != null)
            {
                CurrentTicket = res.Data;
                TicketMessages.Clear();
                foreach (var m in res.Data.Messages)
                {
                    TicketMessages.Add(m);
                }
                IsDetailModalOpen = true;
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void CloseDetailModal()
    {
        IsDetailModalOpen = false;
        ReplyText = string.Empty;
        IsInternalNote = false;
    }

    [RelayCommand]
    public async Task SendReplyAsync()
    {
        if (CurrentTicket == null || string.IsNullOrWhiteSpace(ReplyText)) return;

        IsBusy = true;
        try
        {
            var dto = new SupportTicketReplyDto
            {
                MessageText = ReplyText.Trim(),
                IsInternalNote = IsInternalNote
            };

            var res = await _apiClient.ReplyToSupportTicketAsync(CurrentTicket.Id, dto);
            if (res.Success && res.Data != null)
            {
                TicketMessages.Add(res.Data);
                ReplyText = string.Empty;
                IsInternalNote = false;
            }
            else
            {
                HasError = true;
                ErrorMessage = res.Message ?? "Failed to send message";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task UpdateTicketStatusAsync(string newStatus)
    {
        if (CurrentTicket == null || !Enum.TryParse<SupportTicketStatus>(newStatus, true, out var status)) return;

        IsBusy = true;
        try
        {
            var dto = new UpdateSupportTicketStatusDto
            {
                Status = status
            };

            var res = await _apiClient.UpdateSupportTicketStatusAsync(CurrentTicket.Id, dto);
            if (res.Success && res.Data != null)
            {
                CurrentTicket = res.Data;
                if (SelectedTicketSummary != null)
                {
                    SelectedTicketSummary.Status = status.ToString();
                }
                ApplyFilter();
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

