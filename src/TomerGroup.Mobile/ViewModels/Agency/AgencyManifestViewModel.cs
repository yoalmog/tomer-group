using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyManifestViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public AgencyManifestViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        GuideAssignments = new ObservableCollection<GuideManifestItemDto>();
        DriverTransfers = new ObservableCollection<DriverManifestItemDto>();
    }

    public ObservableCollection<GuideManifestItemDto> GuideAssignments { get; }
    public ObservableCollection<DriverManifestItemDto> DriverTransfers { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.UtcNow;

    [ObservableProperty]
    private string _activeTab = "מדריכים"; // מדריכים / הסעות

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadManifestAsync();
    }

    [RelayCommand]
    public async Task LoadManifestAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            GuideAssignments.Clear();
            DriverTransfers.Clear();

            // Fetch live guides to populate manifests
            var guidesResponse = await _apiClient.GetGuidesAsync();
            if (guidesResponse.Success && guidesResponse.Data != null)
            {
                foreach (var g in guidesResponse.Data)
                {
                    var manifest = await _apiClient.GetGuideManifestAsync(g.Id, SelectedDate);
                    if (manifest.Success && manifest.Data?.Assignments != null)
                    {
                        foreach (var act in manifest.Data.Assignments)
                        {
                            GuideAssignments.Add(act);
                        }
                    }
                }
            }

            // Fetch live drivers to populate transfer manifests
            var driversResponse = await _apiClient.GetDriversAsync();
            if (driversResponse.Success && driversResponse.Data != null)
            {
                foreach (var d in driversResponse.Data)
                {
                    var manifest = await _apiClient.GetDriverManifestAsync(d.Id, SelectedDate);
                    if (manifest.Success && manifest.Data?.Transfers != null)
                    {
                        foreach (var tr in manifest.Data.Transfers)
                        {
                            DriverTransfers.Add(tr);
                        }
                    }
                }
            }

            IsEmpty = (ActiveTab == "מדריכים" ? GuideAssignments.Count : DriverTransfers.Count) == 0;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading manifest: {ex.Message}";
            IsEmpty = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void SelectTab(string tab)
    {
        ActiveTab = tab;
        IsEmpty = (ActiveTab == "מדריכים" ? GuideAssignments.Count : DriverTransfers.Count) == 0;
    }
}
