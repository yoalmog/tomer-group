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
    private string _errorMessage = string.Empty;

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
        ErrorMessage = string.Empty;

        try
        {
            await Task.CompletedTask;
            GuideAssignments.Clear();
            DriverTransfers.Clear();

            // Load default operational mission sheet
            GuideAssignments.Add(new GuideManifestItemDto
            {
                ActivityId = Guid.NewGuid(),
                Title = "סיור מודרך מאצ'ו פיצ'ו (Circuit 2)",
                StartTime = new TimeSpan(8, 30, 0),
                EndTime = new TimeSpan(12, 30, 0),
                Location = "Machu Picchu Sanctuary Gate",
                CustomerName = "Danny Cohen (2 מטיילים)",
                CustomerPhone = "+972 54 123 4567",
                CustomerWhatsApp = "+972541234567",
                DietaryRestrictions = "כשרות מהדרין - חב\"ד (Glatt Kosher)",
                Instructions = "דרכונים מקוריים חובה. איסוף מתחנת הרכבת אגואס קליינטס.",
                Status = Core.Enums.ActivityStatus.Confirmed
            });

            GuideAssignments.Add(new GuideManifestItemDto
            {
                ActivityId = Guid.NewGuid(),
                Title = "סיור עיר קוסקו וסקסייוומאן",
                StartTime = new TimeSpan(14, 0, 0),
                EndTime = new TimeSpan(18, 0, 0),
                Location = "Plaza Regocijo, Cusco",
                CustomerName = "Yossi Levi (משפחה - 4 נפשות)",
                CustomerPhone = "+972 52 987 6543",
                CustomerWhatsApp = "+972529876543",
                DietaryRestrictions = "צמחוני",
                MedicalNotes = "רגישות לגובה (Soroche) - הליכה בקצב מתון",
                Status = Core.Enums.ActivityStatus.Scheduled
            });

            DriverTransfers.Add(new DriverManifestItemDto
            {
                TransportationId = Guid.NewGuid(),
                ServiceType = "Airport Transfer CUZ",
                HebrewServiceType = "איסוף מנמל התעופה קוסקו למלון פלאסיו דל אינקה",
                ScheduledPickupTime = DateTime.UtcNow.Date.AddHours(10).AddMinutes(30),
                PickupLocation = "CUZ Airport Terminal",
                DropoffLocation = "Palacio del Inka, Plazoleta Santo Domingo",
                PassengerCount = 2,
                FlightOrTrainNumber = "LATAM LA-2014",
                CustomerName = "Danny Cohen",
                CustomerPhone = "+972 54 123 4567",
                CustomerWhatsApp = "+972541234567",
                VehicleModel = "Mercedes-Benz Sprinter 2024",
                VehiclePlate = "X4T-892",
                Status = Core.Enums.TransportationStatus.Assigned
            });

            DriverTransfers.Add(new DriverManifestItemDto
            {
                TransportationId = Guid.NewGuid(),
                ServiceType = "Sacred Valley Day Transfer",
                HebrewServiceType = "הסעה יומית עמק קדוש, פיסאק ואולאנטייטמבו",
                ScheduledPickupTime = DateTime.UtcNow.Date.AddHours(7).AddMinutes(30),
                PickupLocation = "Hotel Casa Andina Premium",
                DropoffLocation = "Ollantaytambo Train Station",
                PassengerCount = 4,
                CustomerName = "Yossi Levi",
                CustomerPhone = "+972 52 987 6543",
                CustomerWhatsApp = "+972529876543",
                VehicleModel = "Hyundai H1 Grand Starex",
                VehiclePlate = "B9Z-415",
                Status = Core.Enums.TransportationStatus.Confirmed
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading manifest: {ex.Message}";
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
    }
}
