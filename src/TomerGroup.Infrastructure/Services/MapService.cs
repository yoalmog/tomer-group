using System;
using System.Collections.Generic;
using System.Web;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Infrastructure.Services;

public class MapService : IMapService
{
    private static readonly Dictionary<string, (double Lat, double Lng)> KnownPeruLocations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Cusco"] = (-13.5168, -71.9789),
        ["Cusco Plaza de Armas"] = (-13.5168, -71.9789),
        ["Sacred Valley"] = (-13.4222, -71.8489),
        ["Pisac"] = (-13.4222, -71.8489),
        ["Ollantaytambo"] = (-13.2625, -72.2644),
        ["Ollantaytambo Train Station"] = (-13.2625, -72.2644),
        ["Machu Picchu"] = (-13.1631, -72.5450),
        ["Machu Picchu Citadel"] = (-13.1631, -72.5450),
        ["Aguas Calientes"] = (-13.1550, -72.5255),
        ["Lake Titicaca"] = (-15.8402, -70.0219),
        ["Puno Port"] = (-15.8402, -70.0219),
        ["Arequipa"] = (-16.3988, -71.5369),
        ["Colca Canyon"] = (-15.6383, -71.6010),
        ["Rainbow Mountain"] = (-13.8694, -71.3031),
        ["Amazon"] = (-12.5933, -69.1891),
        ["Puerto Maldonado"] = (-12.5933, -69.1891)
    };

    public string GetMapNavigationUrl(double latitude, double longitude, string label)
    {
        var encodedLabel = HttpUtility.UrlEncode(label ?? "Destination");
        return $"https://www.google.com/maps/search/?api=1&query={latitude:F6},{longitude:F6}";
    }

    public (double Lat, double Lng) GetCoordinatesForDestination(string destinationName)
    {
        if (string.IsNullOrWhiteSpace(destinationName))
        {
            return KnownPeruLocations["Cusco"];
        }

        foreach (var kvp in KnownPeruLocations)
        {
            if (destinationName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return KnownPeruLocations["Cusco"];
    }

    public string GetDirectionsUrl(double originLat, double originLng, double destLat, double destLng)
    {
        return $"https://www.google.com/maps/dir/?api=1&origin={originLat:F6},{originLng:F6}&destination={destLat:F6},{destLng:F6}";
    }
}

