using System;
using System.Collections.Generic;
using System.Globalization;
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
        ["קוסקו"] = (-13.5168, -71.9789),
        ["Cusco Plaza de Armas"] = (-13.5168, -71.9789),
        ["כיכר הנשק"] = (-13.5168, -71.9789),
        ["Plaza de Armas"] = (-13.5168, -71.9789),

        ["Sacred Valley"] = (-13.4222, -71.8489),
        ["העמק הקדוש"] = (-13.4222, -71.8489),
        ["Valle Sagrado"] = (-13.4222, -71.8489),

        ["Pisac"] = (-13.4222, -71.8489),
        ["פיסאק"] = (-13.4222, -71.8489),

        ["Ollantaytambo"] = (-13.2625, -72.2644),
        ["אולנטייטמבו"] = (-13.2625, -72.2644),

        ["Machu Picchu"] = (-13.1631, -72.5450),
        ["מאצ'ו פיצ'ו"] = (-13.1631, -72.5450),
        ["Aguas Calientes"] = (-13.1550, -72.5255),
        ["אגואס קליינטס"] = (-13.1550, -72.5255),

        ["Lake Titicaca"] = (-15.8402, -70.0219),
        ["אגם טיטיקקה"] = (-15.8402, -70.0219),
        ["Puno"] = (-15.8402, -70.0219),
        ["פונו"] = (-15.8402, -70.0219),

        ["Arequipa"] = (-16.3988, -71.5369),
        ["ארקיפה"] = (-16.3988, -71.5369),

        ["Colca Canyon"] = (-15.6383, -71.6010),
        ["קניון קולקה"] = (-15.6383, -71.6010),
        ["Cañón del Colca"] = (-15.6383, -71.6010),

        ["Rainbow Mountain"] = (-13.8694, -71.3031),
        ["הר הצבעים"] = (-13.8694, -71.3031),
        ["Vinicunca"] = (-13.8694, -71.3031),
        ["ויניקונקה"] = (-13.8694, -71.3031),

        ["Amazon"] = (-12.5933, -69.1891),
        ["אמזונס"] = (-12.5933, -69.1891),
        ["Puerto Maldonado"] = (-12.5933, -69.1891),
        ["פורטו מלדונדו"] = (-12.5933, -69.1891),

        ["Lima"] = (-12.0464, -77.0428),
        ["לימה"] = (-12.0464, -77.0428),
        ["Miraflores"] = (-12.1219, -77.0297),
        ["מיראפלורס"] = (-12.1219, -77.0297),

        ["Huaraz"] = (-9.5261, -77.5288),
        ["וואראז"] = (-9.5261, -77.5288),

        ["Ica"] = (-14.0678, -75.7286),
        ["Huacachina"] = (-14.0875, -75.7633),
        ["וואקאצ'ינה"] = (-14.0875, -75.7633),

        ["Salkantay"] = (-13.3340, -72.5456),
        ["סלקנטאי"] = (-13.3340, -72.5456),
        ["Inca Trail"] = (-13.2500, -72.4500),
        ["שביל האינקה"] = (-13.2500, -72.4500)
    };

    public string GetMapNavigationUrl(double latitude, double longitude, string label)
    {
        return string.Create(CultureInfo.InvariantCulture, $"https://www.google.com/maps/search/?api=1&query={latitude:F6},{longitude:F6}");
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

    public bool TryGetCoordinates(string destinationName, out (double Lat, double Lng) coords)
    {
        if (string.IsNullOrWhiteSpace(destinationName))
        {
            coords = KnownLocations["Cusco"];
            return true;
        }

        foreach (var kvp in KnownLocations)
        {
            if (destinationName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                coords = kvp.Value;
                return true;
            }
        }

        coords = default;
        return false;
    }

    public async Task<bool> OpenCoordinatesAsync(double latitude, double longitude, string label)
    {
        var url = GetMapNavigationUrl(latitude, longitude, label);
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
        try
        {
            var location = new Microsoft.Maui.Devices.Sensors.Location(latitude, longitude);
            var options = new Microsoft.Maui.ApplicationModel.MapLaunchOptions
            {
                Name = label ?? "Tomer Group Activity"
            };
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
            return true;
        }
        catch
        {
            try
            {
                return await Microsoft.Maui.ApplicationModel.Launcher.Default.OpenAsync(new Uri(url));
            }
            catch
            {
                return false;
            }
        }
#else
        await Task.CompletedTask;
        return true;
#endif
    }

    public async Task<bool> OpenDestinationAsync(string destinationName)
    {
        if (TryGetCoordinates(destinationName, out var coords))
        {
            return await OpenCoordinatesAsync(coords.Lat, coords.Lng, destinationName);
        }

        var placeQuery = HttpUtility.UrlEncode((destinationName ?? "Cusco") + ", Peru");
        var searchUrl = $"https://www.google.com/maps/search/?api=1&query={placeQuery}";
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
        try
        {
            return await Microsoft.Maui.ApplicationModel.Launcher.Default.OpenAsync(new Uri(searchUrl));
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
}
