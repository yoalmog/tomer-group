using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Models;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Agency;

public partial class AgencyTrekEditorViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public ObservableCollection<TrekRouteEntity> TrekRoutes { get; } = new();

    [ObservableProperty]
    private TrekRouteEntity? _selectedTrek;

    // Trek Properties for editing
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _region = "Cusco, Peru";

    [ObservableProperty]
    private string _difficulty = "Challenging";

    [ObservableProperty]
    private int _durationDays = 4;

    [ObservableProperty]
    private double _distanceKm = 45.0;

    [ObservableProperty]
    private int _maxElevationMeters = 4630;

    [ObservableProperty]
    private string _coordinatesJson = "[]";

    [ObservableProperty]
    private string _waypointsJson = "[]";

    [ObservableProperty]
    private string _elevationProfileJson = "[]";

    [ObservableProperty]
    private bool _isPublished = false;

    // GPX Input
    [ObservableProperty]
    private string _gpxInputText = string.Empty;

    [ObservableProperty]
    private string _mapHtml = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public AgencyTrekEditorViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadTreksAsync();
    }

    [RelayCommand]
    public async Task LoadTreksAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            TrekRoutes.Clear();
            var res = await _apiClient.GetAdminTreksAsync();
            if (res.Success && res.Data != null)
            {
                foreach (var t in res.Data)
                {
                    TrekRoutes.Add(t);
                }

                if (SelectedTrek == null && TrekRoutes.Any())
                {
                    SelectTrek(TrekRoutes.First());
                }
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Error loading treks: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void SelectTrek(TrekRouteEntity trek)
    {
        if (trek == null) return;
        SelectedTrek = trek;
        Name = trek.Name;
        Region = trek.Region;
        Difficulty = trek.Difficulty;
        DurationDays = trek.DurationDays;
        DistanceKm = trek.DistanceKm;
        MaxElevationMeters = trek.MaxElevationMeters;
        CoordinatesJson = trek.CoordinatesJson;
        WaypointsJson = trek.WaypointsJson;
        ElevationProfileJson = trek.ElevationProfileJson;
        IsPublished = trek.IsPublished;

        GenerateMapHtml();
    }

    [RelayCommand]
    public void NewTrek()
    {
        SelectedTrek = null;
        Name = "New Trek Route";
        Region = "Cusco, Peru";
        Difficulty = "Challenging";
        DurationDays = 4;
        DistanceKm = 0;
        MaxElevationMeters = 0;
        CoordinatesJson = "[]";
        WaypointsJson = "[]";
        ElevationProfileJson = "[]";
        IsPublished = false;
        GpxInputText = string.Empty;
        GenerateMapHtml();
    }

    [RelayCommand]
    public async Task ImportGpxAsync()
    {
        if (string.IsNullOrWhiteSpace(GpxInputText))
        {
            HasError = true;
            ErrorMessage = "Please paste valid GPX XML content";
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;
        StatusMessage = "Parsing GPX track...";

        try
        {
            var res = await _apiClient.ImportGpxAsync(GpxInputText.Trim());
            if (res.Success && res.Data != null)
            {
                var data = res.Data;
                Name = string.IsNullOrEmpty(Name) || Name == "New Trek Route" ? data.TrackName : Name;
                DistanceKm = Math.Round(data.TotalDistanceKm, 1);
                MaxElevationMeters = data.MaxElevationMeters;
                CoordinatesJson = data.CoordinatesJson;
                WaypointsJson = data.WaypointsJson;
                ElevationProfileJson = data.ElevationProfileJson;

                StatusMessage = $"Imported {data.CoordinatePointsCount} points, {data.WaypointsCount} waypoints ({DistanceKm} km)";
                GenerateMapHtml();
            }
            else
            {
                HasError = true;
                ErrorMessage = res.Message ?? "Failed to parse GPX content";
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
    public async Task SaveTrekAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            HasError = true;
            ErrorMessage = "Trek name is required";
            return;
        }

        IsBusy = true;
        HasError = false;
        StatusMessage = "Saving trek...";

        try
        {
            var dto = new SaveTrekRouteDto
            {
                Id = SelectedTrek?.Id,
                Name = Name,
                Region = Region,
                Difficulty = Difficulty,
                DurationDays = DurationDays,
                DistanceKm = DistanceKm,
                MaxElevationMeters = MaxElevationMeters,
                CoordinatesJson = CoordinatesJson,
                WaypointsJson = WaypointsJson,
                ElevationProfileJson = ElevationProfileJson,
                OriginalGpxContent = GpxInputText,
                IsPublished = IsPublished
            };

            if (SelectedTrek == null)
            {
                var res = await _apiClient.SaveTrekRouteAsync(dto);
                if (res.Success && res.Data != null)
                {
                    SelectedTrek = res.Data;
                    TrekRoutes.Add(res.Data);
                    StatusMessage = "Trek saved successfully";
                }
                else
                {
                    HasError = true;
                    ErrorMessage = res.Message ?? "Failed to create trek";
                }
            }
            else
            {
                var res = await _apiClient.UpdateTrekRouteAsync(SelectedTrek.Id, dto);
                if (res.Success && res.Data != null)
                {
                    SelectedTrek = res.Data;
                    StatusMessage = "Trek updated successfully";
                }
                else
                {
                    HasError = true;
                    ErrorMessage = res.Message ?? "Failed to update trek";
                }
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
    public async Task PublishTrekAsync()
    {
        if (SelectedTrek == null) return;

        IsBusy = true;
        try
        {
            var res = await _apiClient.PublishTrekRouteAsync(SelectedTrek.Id);
            if (res.Success)
            {
                IsPublished = true;
                if (SelectedTrek != null) SelectedTrek.IsPublished = true;
                StatusMessage = "Trek published to live traveler catalog!";
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
    public async Task UnpublishTrekAsync()
    {
        if (SelectedTrek == null) return;

        IsBusy = true;
        try
        {
            var res = await _apiClient.UnpublishTrekRouteAsync(SelectedTrek.Id);
            if (res.Success)
            {
                IsPublished = false;
                if (SelectedTrek != null) SelectedTrek.IsPublished = false;
                StatusMessage = "Trek route unpublished";
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

    private void GenerateMapHtml()
    {
        if (string.IsNullOrEmpty(CoordinatesJson) || CoordinatesJson == "[]")
        {
            MapHtml = "<html><body style='background:#12141C;color:#94A3B8;font-family:sans-serif;text-align:center;padding-top:60px;'><h3>טען או הדבק קובץ GPX לתצוגת מפה אינטראקטיבית</h3></body></html>";
            return;
        }

        try
        {
            var coords = JsonSerializer.Deserialize<List<TrekCoordinate>>(CoordinatesJson);
            if (coords == null || coords.Count == 0)
            {
                MapHtml = "<html><body style='background:#12141C;color:#94A3B8;font-family:sans-serif;text-align:center;padding-top:60px;'><h3>אין נקודות ציון במסלול</h3></body></html>";
                return;
            }

            var centerLat = coords.Average(c => c.Latitude).ToString(CultureInfo.InvariantCulture);
            var centerLng = coords.Average(c => c.Longitude).ToString(CultureInfo.InvariantCulture);

            var polylinePoints = string.Join(",", coords.Select(c => $"[{c.Latitude.ToString(CultureInfo.InvariantCulture)},{c.Longitude.ToString(CultureInfo.InvariantCulture)}]"));

            MapHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <style>
        html, body, #map {{ height: 100%; width: 100%; margin: 0; padding: 0; background: #0F1015; }}
        .leaflet-popup-content {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; }}
    </style>
</head>
<body>
    <div id='map'></div>
    <script>
        var map = L.map('map').setView([{centerLat}, {centerLng}], 12);
        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 18,
            attribution: '© OpenStreetMap'
        }}).addTo(map);

        var points = [{polylinePoints}];
        var polyline = L.polyline(points, {{
            color: '#E11D68',
            weight: 4,
            opacity: 0.9,
            smoothFactor: 1
        }}).addTo(map);

        map.fitBounds(polyline.getBounds(), {{ padding: [20, 20] }});
    </script>
</body>
</html>";
        }
        catch
        {
            MapHtml = "<html><body style='background:#12141C;color:#EF4444;font-family:sans-serif;text-align:center;padding-top:60px;'><h3>שגיאה בטעינת נקודות מפה</h3></body></html>";
        }
    }
}

