using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Mobile.Services;

public interface IMobileMapService : IMapService
{
    Task<bool> OpenCoordinatesAsync(double latitude, double longitude, string label);
    Task<bool> OpenDestinationAsync(string destinationName);
    (double Lat, double Lng) GetCoordinates(string destinationName);
}

public class MobileMapService : IMobileMapService
{
    private static readonly Dictionary<string, (double Lat, double Lng)> KnownLocations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Cusco"] = (-13.5168, -71.9789),
        ["Cusco Plaza de Armas"] = (-13.5168, -71.9789),
        ["Sacred Valley"] = (-13.4222, -71.8489),
        ["Pisac"] = (-13.4222, -71.8489),
        ["Ollantaytambo"] = (-13.2625, -72.2644),
        ["Machu Picchu"] = (-13.1631, -72.5450),
        ["Lake Titicaca"] = (-15.8402, -70.0219),
        ["Arequipa"] = (-16.3988, -71.5369),
        ["Colca Canyon"] = (-15.6383, -71.6010),
        ["Rainbow Mountain"] = (-13.8694, -71.3031),
        ["Amazon"] = (-12.5933, -69.1891)
    };

    public string GetMapNavigationUrl(double latitude, double longitude, string label)
    {
        return $"https://www.google.com/maps/search/?api=1&query={latitude:F6},{longitude:F6}";
    }

    public (double Lat, double Lng) GetCoordinates(string destinationName)
    {
        if (string.IsNullOrWhiteSpace(destinationName))
            return KnownLocations["Cusco"];

        foreach (var kvp in KnownLocations)
        {
            if (destinationName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        return KnownLocations["Cusco"];
    }

    public async Task<bool> OpenCoordinatesAsync(double latitude, double longitude, string label)
    {
        var url = GetMapNavigationUrl(latitude, longitude, label);
#if USE_MAUI
        try
        {
            return await Microsoft.Maui.ApplicationModel.Launcher.Default.OpenAsync(new Uri(url));
        }
        catch
        {
            return false;
        }
#else
        await Task.CompletedTask;
        return true;
#endif
    }

    public async Task<bool> OpenDestinationAsync(string destinationName)
    {
        var coords = GetCoordinates(destinationName);
        return await OpenCoordinatesAsync(coords.Lat, coords.Lng, destinationName);
    }
}

