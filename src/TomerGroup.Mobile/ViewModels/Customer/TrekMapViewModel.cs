using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
#endif
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Core.Models;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class TrekMapViewModel : BaseViewModel
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
    , IQueryAttributable
#endif
{
    private readonly IMobileMapService _mapService;
    private readonly IGpxService _gpxService;

    [ObservableProperty]
    private TrekRoute _selectedRoute;

    [ObservableProperty]
    private TrekWaypoint? _selectedWaypoint;

    [ObservableProperty]
    private string _mapHtml = string.Empty;

    [ObservableProperty]
    private double _userLatitude;

    [ObservableProperty]
    private double _userLongitude;

    [ObservableProperty]
    private bool _hasUserLocation;

    [ObservableProperty]
    private string _userLocationStatus = string.Empty;

    [ObservableProperty]
    private bool _showElevationProfile = true;

    public ObservableCollection<TrekRoute> AvailableRoutes { get; } = new();

    public string BackButtonText => CurrentLanguage switch
    {
        "en" => "← Back",
        "es" => "← Volver",
        _ => "← חזור"
    };

    public string LocateMeButtonText => CurrentLanguage switch
    {
        "en" => "📍 My Location",
        "es" => "📍 Mi Ubicación",
        _ => "📍 מיקומי"
    };

    public string ElevationProfileButtonText => CurrentLanguage switch
    {
        "en" => "📈 Elevation Profile",
        "es" => "📈 Perfil de Altura",
        _ => "📈 פרופיל גבהים"
    };

    public string ExternalMapButtonText => CurrentLanguage switch
    {
        "en" => "🗺️ Open in Maps",
        "es" => "🗺️ Abrir en Mapas",
        _ => "🗺️ פתח במפות ניווט"
    };

    public string DistanceMetricLabel => CurrentLanguage switch
    {
        "en" => "Distance",
        "es" => "Distancia",
        _ => "מרחק"
    };

    public string MaxAltitudeMetricLabel => CurrentLanguage switch
    {
        "en" => "Max Altitude",
        "es" => "Altitud Máx",
        _ => "רום שיא"
    };

    public string ElevationGainMetricLabel => CurrentLanguage switch
    {
        "en" => "Elevation Gain",
        "es" => "Desnivel",
        _ => "טיפוס מצטבר"
    };

    public string DurationMetricLabel => CurrentLanguage switch
    {
        "en" => "Est. Duration",
        "es" => "Duración Est.",
        _ => "משך משוער"
    };

    public string WaypointsTitle => CurrentLanguage switch
    {
        "en" => "Waypoints & Elevations:",
        "es" => "Puntos de Ruta y Alturas:",
        _ => "נקודות ציון וגבהים במסלול:"
    };

    public string DistanceFormatted => $"{SelectedRoute?.TotalDistanceKm:F0} {(CurrentLanguage == "he" ? "ק״מ" : "km")}";
    public string MaxElevationFormatted => $"{SelectedRoute?.MaxElevationMeters:N0} {(CurrentLanguage == "he" ? "מ׳" : "m")}";
    public string ElevationGainFormatted => $"+{SelectedRoute?.ElevationGainMeters:N0} {(CurrentLanguage == "he" ? "מ׳" : "m")}";
    public string DurationFormatted => CurrentLanguage switch
    {
        "en" => $"{SelectedRoute?.EstimatedDurationDays} Days",
        "es" => $"{SelectedRoute?.EstimatedDurationDays} Días",
        _ => $"{SelectedRoute?.EstimatedDurationDays} ימים"
    };

    public TrekMapViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IMobileMapService mapService,
        IGpxService gpxService)
        : base(localization, navigation)
    {
        _mapService = mapService;
        _gpxService = gpxService;

        var treks = TrekCatalog.GetFlagshipTreks();
        foreach (var t in treks)
        {
            AvailableRoutes.Add(t);
        }

        _selectedRoute = AvailableRoutes.FirstOrDefault() ?? new TrekRoute();
        Title = CurrentLanguage switch
        {
            "en" => "Interactive Trek Map",
            "es" => "Mapa de Treks Interactivo",
            _ => "מפת טרקים אינטראקטיבית"
        };

        GenerateMapHtml();
    }

    protected override void OnLanguageChanged()
    {
        Title = CurrentLanguage switch
        {
            "en" => "Interactive Trek Map",
            "es" => "Mapa de Treks Interactivo",
            _ => "מפת טרקים אינטראקטיבית"
        };
        OnPropertyChanged(nameof(BackButtonText));
        OnPropertyChanged(nameof(LocateMeButtonText));
        OnPropertyChanged(nameof(ElevationProfileButtonText));
        OnPropertyChanged(nameof(ExternalMapButtonText));
        OnPropertyChanged(nameof(DistanceMetricLabel));
        OnPropertyChanged(nameof(MaxAltitudeMetricLabel));
        OnPropertyChanged(nameof(ElevationGainMetricLabel));
        OnPropertyChanged(nameof(DurationMetricLabel));
        OnPropertyChanged(nameof(WaypointsTitle));
        OnPropertyChanged(nameof(DistanceFormatted));
        OnPropertyChanged(nameof(MaxElevationFormatted));
        OnPropertyChanged(nameof(ElevationGainFormatted));
        OnPropertyChanged(nameof(DurationFormatted));
        OnPropertyChanged(nameof(SelectedRoute));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("trekName", out var nameObj) && nameObj is string name && !string.IsNullOrWhiteSpace(name))
        {
            var match = AvailableRoutes.FirstOrDefault(r =>
                r.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                r.HebrewName.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                SelectRoute(match);
            }
        }
        else if (query.TryGetValue("trekId", out var idObj) && idObj is string idStr && Guid.TryParse(idStr, out var id))
        {
            var match = AvailableRoutes.FirstOrDefault(r => r.Id == id);
            if (match != null)
            {
                SelectRoute(match);
            }
        }
    }

    [RelayCommand]
    public void SelectRoute(TrekRoute route)
    {
        if (route == null) return;
        SelectedRoute = route;
        SelectedWaypoint = route.Waypoints.FirstOrDefault();
        GenerateMapHtml();
        OnPropertyChanged(nameof(SelectedRoute));
    }

    [RelayCommand]
    public void SelectWaypoint(TrekWaypoint waypoint)
    {
        SelectedWaypoint = waypoint;
    }

    [RelayCommand]
    public void ToggleElevationProfile()
    {
        ShowElevationProfile = !ShowElevationProfile;
    }

    [RelayCommand]
    public async Task LocateMeAsync()
    {
        try
        {
            UserLocationStatus = "מאתר מיקום GPS...";
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                UserLatitude = location.Latitude;
                UserLongitude = location.Longitude;
                HasUserLocation = true;
                UserLocationStatus = string.Create(CultureInfo.InvariantCulture, $"מיקום נוכחי: {location.Latitude:F4}, {location.Longitude:F4}");
                GenerateMapHtml();
            }
            else
            {
                UserLocationStatus = "לא ניתן לאתר מיקום (GPS כבוי או אין קליטה באנדים)";
            }
#else
            UserLocationStatus = "GPS לא נתמך בסביבה זו";
            await Task.CompletedTask;
#endif
        }
        catch (Exception ex)
        {
            UserLocationStatus = $"שגיאת מיקום: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task OpenExternalMapAsync()
    {
        if (SelectedWaypoint != null)
        {
            await _mapService.OpenCoordinatesAsync(SelectedWaypoint.Latitude, SelectedWaypoint.Longitude, SelectedWaypoint.HebrewName);
        }
        else if (SelectedRoute.Waypoints.Count > 0)
        {
            var start = SelectedRoute.Waypoints[0];
            await _mapService.OpenCoordinatesAsync(start.Latitude, start.Longitude, SelectedRoute.HebrewName);
        }
        else
        {
            await _mapService.OpenDestinationAsync(SelectedRoute.Region);
        }
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Navigation.GoBackAsync();
    }

    private void GenerateMapHtml()
    {
        if (SelectedRoute == null || SelectedRoute.Coordinates.Count == 0)
        {
            MapHtml = "<html><body style='background:#FAF9FB;font-family:sans-serif;text-align:center;padding-top:40px;'><h3>אין נתוני מסלול זמינים</h3></body></html>";
            return;
        }

        var centerLat = SelectedRoute.Coordinates.Average(c => c.Latitude);
        var centerLng = SelectedRoute.Coordinates.Average(c => c.Longitude);

        var coordsJson = JsonSerializer.Serialize(SelectedRoute.Coordinates.Select(c => new double[] { c.Latitude, c.Longitude }));
        var waypointsJson = JsonSerializer.Serialize(SelectedRoute.Waypoints.Select(w => new
        {
            name = IsHebrewSelected ? w.HebrewName : w.Name,
            lat = w.Latitude,
            lng = w.Longitude,
            ele = w.ElevationMeters,
            type = w.Type.ToString(),
            desc = IsHebrewSelected ? w.HebrewDescription : w.Description,
            day = w.DayNumber
        }));

        var userMarkerScript = HasUserLocation
            ? string.Create(CultureInfo.InvariantCulture, $@"
                var userMarker = L.circleMarker([{UserLatitude:F6}, {UserLongitude:F6}], {{
                    radius: 9,
                    fillColor: '#3B82F6',
                    color: '#FFFFFF',
                    weight: 3,
                    opacity: 1,
                    fillOpacity: 0.9
                }}).addTo(map).bindPopup('<b>המיקום הנוכחי שלך</b>');")
            : string.Empty;

        MapHtml = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <style>
        html, body, #map {{ height: 100%; width: 100%; margin: 0; padding: 0; background: #0A0A0C; }}
        .leaflet-popup-content-wrapper {{ border-radius: 12px; font-family: sans-serif; box-shadow: 0 4px 14px rgba(0,0,0,0.25); }}
        .leaflet-popup-content {{ margin: 12px 14px; font-size: 13px; line-height: 1.4; color: #0F172A; }}
        .badge {{ display: inline-block; padding: 2px 8px; border-radius: 6px; font-size: 10px; font-weight: bold; background: #BC225E; color: white; margin-bottom: 4px; }}
        .alt {{ color: #059669; font-weight: bold; font-size: 12px; }}
    </style>
</head>
<body>
    <div id='map'></div>
    <script>
        var map = L.map('map', {{ zoomControl: true }}).setView([{centerLat.ToString(CultureInfo.InvariantCulture)}, {centerLng.ToString(CultureInfo.InvariantCulture)}], 11);

        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 18,
            attribution: '&copy; OpenStreetMap &copy; Tomer Group Peru'
        }}).addTo(map);

        var coords = {coordsJson};
        var waypoints = {waypointsJson};

        var polyline = L.polyline(coords, {{
            color: '#BC225E',
            weight: 5,
            opacity: 0.9,
            lineJoin: 'round'
        }}).addTo(map);

        map.fitBounds(polyline.getBounds(), {{ padding: [30, 30] }});

        waypoints.forEach(function(wp, idx) {{
            var color = '#BC225E';
            var iconSymbol = '📍';
            if (wp.type === 'Start') {{ color = '#10B981'; iconSymbol = '🟢'; }}
            else if (wp.type === 'Finish') {{ color = '#F59E0B'; iconSymbol = '🏁'; }}
            else if (wp.type === 'MountainPass') {{ color = '#881337'; iconSymbol = '⛰️'; }}
            else if (wp.type === 'Campsite') {{ color = '#3B82F6'; iconSymbol = '⛺'; }}
            else if (wp.type === 'AncientRuins') {{ color = '#7C3AED'; iconSymbol = '🏰'; }}

            var marker = L.circleMarker([wp.lat, wp.lng], {{
                radius: 8,
                fillColor: color,
                color: '#FFFFFF',
                weight: 2,
                opacity: 1,
                fillOpacity: 0.95
            }}).addTo(map);

            var popupContent = '<div>' +
                '<div class=""badge"">' + wp.type + (wp.day ? ' • Day ' + wp.day : '') + '</div>' +
                '<div style=""font-weight:bold;font-size:14px;margin-bottom:2px;"">' + iconSymbol + ' ' + wp.name + '</div>' +
                '<div class=""alt"">' + wp.ele + ' meters (' + Math.round(wp.ele * 3.28084) + ' ft)</div>' +
                '<div style=""margin-top:4px;color:#475569;"">' + wp.desc + '</div>' +
                '</div>';

            marker.bindPopup(popupContent);
        }});

        {userMarkerScript}
    </script>
</body>
</html>";
    }
}
