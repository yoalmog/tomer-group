using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Models;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyTasksViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public ObservableCollection<StaffTask> AllTasks { get; } = new();
    public ObservableCollection<StaffTask> FilteredTasks { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _currentStatusFilter = "Open";

    [ObservableProperty]
    private bool _dueSoonOnly = false;

    // Create Task Modal State
    [ObservableProperty]
    private bool _isCreateModalOpen;

    [ObservableProperty]
    private string _newTaskTitle = string.Empty;

    [ObservableProperty]
    private string _newTaskDescription = string.Empty;

    [ObservableProperty]
    private DateTime _newTaskDueDate = DateTime.UtcNow.AddDays(1);

    [ObservableProperty]
    private StaffTaskPriority _newTaskPriority = StaffTaskPriority.Medium;

    public AgencyTasksViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadTasksAsync();
    }

    [RelayCommand]
    public async Task LoadTasksAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            AllTasks.Clear();
            FilteredTasks.Clear();

            StaffTaskStatus? filterStatus = null;
            if (CurrentStatusFilter != "All" && Enum.TryParse<StaffTaskStatus>(CurrentStatusFilter, true, out var parsed))
            {
                filterStatus = parsed;
            }

            var res = await _apiClient.GetStaffTasksAsync(filterStatus, DueSoonOnly);
            if (res.Success && res.Data != null)
            {
                foreach (var t in res.Data)
                {
                    AllTasks.Add(t);
                    FilteredTasks.Add(t);
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading tasks: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SetStatusFilterAsync(string status)
    {
        CurrentStatusFilter = status;
        await LoadTasksAsync();
    }

    [RelayCommand]
    public async Task ToggleDueSoonAsync()
    {
        DueSoonOnly = !DueSoonOnly;
        await LoadTasksAsync();
    }

    [RelayCommand]
    public void OpenCreateModal()
    {
        NewTaskTitle = string.Empty;
        NewTaskDescription = string.Empty;
        NewTaskDueDate = DateTime.UtcNow.AddDays(1);
        NewTaskPriority = StaffTaskPriority.Medium;
        IsCreateModalOpen = true;
    }

    [RelayCommand]
    public void CloseCreateModal()
    {
        IsCreateModalOpen = false;
    }

    [RelayCommand]
    public async Task CreateTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

        IsBusy = true;
        try
        {
            var dto = new CreateStaffTaskDto
            {
                Title = NewTaskTitle.Trim(),
                Description = NewTaskDescription.Trim(),
                DueDate = NewTaskDueDate,
                Priority = NewTaskPriority
            };

            var res = await _apiClient.CreateStaffTaskAsync(dto);
            if (res.Success && res.Data != null)
            {
                IsCreateModalOpen = false;
                await LoadTasksAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = res.Message ?? "Failed to create task";
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
    public async Task StartTaskAsync(StaffTask task)
    {
        await UpdateStatusInternalAsync(task, StaffTaskStatus.InProgress);
    }

    [RelayCommand]
    public async Task CompleteTaskAsync(StaffTask task)
    {
        await UpdateStatusInternalAsync(task, StaffTaskStatus.Completed);
    }

    private async Task UpdateStatusInternalAsync(StaffTask task, StaffTaskStatus newStatus)
    {
        if (task == null) return;

        IsBusy = true;
        try
        {
            var dto = new UpdateStaffTaskStatusDto
            {
                Status = newStatus,
                CompletionNotes = newStatus == StaffTaskStatus.Completed ? "Completed on mobile ops console" : null
            };

            var res = await _apiClient.UpdateStaffTaskStatusAsync(task.Id, dto);
            if (res.Success)
            {
                await LoadTasksAsync();
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
