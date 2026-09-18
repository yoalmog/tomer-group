using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using TomerGroup.Core.Models;

namespace TomerGroup.Mobile.Services;

public interface IGpxService
{
    TrekRoute ParseGpx(string gpxContent, string routeName = "Custom Imported Trek");
    string ExportToGpx(TrekRoute route);
    double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2);
}

public class GpxService : IGpxService
{
    public TrekRoute ParseGpx(string gpxContent, string routeName = "Custom Imported Trek")
    {
        if (string.IsNullOrWhiteSpace(gpxContent))
        {
            throw new ArgumentException("GPX content cannot be empty", nameof(gpxContent));
        }

        var doc = XDocument.Parse(gpxContent);
        XNamespace ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        var route = new TrekRoute
        {
            Id = Guid.NewGuid(),
            Name = routeName,
            HebrewName = routeName,
            Region = "Cusco",
            Difficulty = "Custom"
        };

        var coordinates = new List<TrekCoordinate>();
        var waypoints = new List<TrekWaypoint>();

        // Parse track points <trkpt lat="..." lon="...">
        var trackPoints = doc.Descendants(ns + "trkpt").ToList();
        if (!trackPoints.Any())
        {
            // Fallback to route points <rtept>
            trackPoints = doc.Descendants(ns + "rtept").ToList();
        }

        double cumulativeDistance = 0;
        double maxEle = double.MinValue;
        double minEle = double.MaxValue;
        double totalGain = 0;
        double? previousEle = null;

        for (int i = 0; i < trackPoints.Count; i++)
        {
            var pt = trackPoints[i];
            var latAttr = pt.Attribute("lat")?.Value;
            var lonAttr = pt.Attribute("lon")?.Value;

            if (double.TryParse(latAttr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                double.TryParse(lonAttr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
            {
                double ele = 0;
                var eleElem = pt.Element(ns + "ele")?.Value;
                if (!string.IsNullOrEmpty(eleElem) && double.TryParse(eleElem, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedEle))
                {
                    ele = parsedEle;
                }

                if (i > 0)
                {
                    var prevCoord = coordinates[i - 1];
                    var stepDist = CalculateHaversineDistance(prevCoord.Latitude, prevCoord.Longitude, lat, lon);
                    cumulativeDistance += stepDist;

                    if (previousEle.HasValue && ele > previousEle.Value)
                    {
                        totalGain += (ele - previousEle.Value);
                    }
                }

                previousEle = ele;
                maxEle = Math.Max(maxEle, ele);
                minEle = Math.Min(minEle, ele);

                coordinates.Add(new TrekCoordinate(lat, lon, ele, cumulativeDistance));
            }
        }

        // Parse standalone waypoints <wpt lat="..." lon="...">
        var wptElems = doc.Descendants(ns + "wpt").ToList();
        for (int i = 0; i < wptElems.Count; i++)
        {
            var w = wptElems[i];
            var latAttr = w.Attribute("lat")?.Value;
            var lonAttr = w.Attribute("lon")?.Value;
            var name = w.Element(ns + "name")?.Value ?? $"Waypoint {i + 1}";
            var desc = w.Element(ns + "desc")?.Value ?? string.Empty;

            if (double.TryParse(latAttr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                double.TryParse(lonAttr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
            {
                double ele = 0;
                var eleElem = w.Element(ns + "ele")?.Value;
                if (!string.IsNullOrEmpty(eleElem) && double.TryParse(eleElem, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedEle))
                {
                    ele = parsedEle;
                }

                var type = i == 0 ? WaypointType.Start : (i == wptElems.Count - 1 ? WaypointType.Finish : WaypointType.Viewpoint);

                waypoints.Add(new TrekWaypoint
                {
                    Name = name,
                    HebrewName = name,
                    Latitude = lat,
                    Longitude = lon,
                    ElevationMeters = ele,
                    Type = type,
                    Description = desc,
                    HebrewDescription = desc
                });
            }
        }

        route.Coordinates = coordinates;
        route.Waypoints = waypoints;
        route.TotalDistanceKm = Math.Round(cumulativeDistance, 1);
        route.ElevationGainMeters = Math.Round(totalGain, 0);
        route.MaxElevationMeters = maxEle > double.MinValue ? Math.Round(maxEle, 0) : 0;
        route.MinElevationMeters = minEle < double.MaxValue ? Math.Round(minEle, 0) : 0;

        // Build elevation profile sample points (e.g. 10 sampled points along distance)
        if (coordinates.Count > 0)
        {
            var step = Math.Max(1, coordinates.Count / 10);
            for (int i = 0; i < coordinates.Count; i += step)
            {
                route.ElevationProfile.Add(new ElevationPoint(
                    Math.Round(coordinates[i].DistanceFromStartKm, 1),
                    Math.Round(coordinates[i].ElevationMeters, 0),
                    $"Km {coordinates[i].DistanceFromStartKm:F1}"));
            }
        }

        return route;
    }

    public string ExportToGpx(TrekRoute route)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<gpx version=\"1.1\" creator=\"TomerGroup Travel Platform\" xmlns=\"http://www.topografix.com/GPX/1/1\">");
        sb.AppendLine($"  <metadata><name>{route.Name}</name><desc>{route.Summary}</desc></metadata>");

        foreach (var wp in route.Waypoints)
        {
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"  <wpt lat=\"{wp.Latitude:F6}\" lon=\"{wp.Longitude:F6}\"><ele>{wp.ElevationMeters:F1}</ele><name>{wp.Name}</name><desc>{wp.Description}</desc></wpt>"));
        }

        sb.AppendLine("  <trk>");
        sb.AppendLine($"    <name>{route.Name}</name>");
        sb.AppendLine("    <trkseg>");
        foreach (var pt in route.Coordinates)
        {
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"      <trkpt lat=\"{pt.Latitude:F6}\" lon=\"{pt.Longitude:F6}\"><ele>{pt.ElevationMeters:F1}</ele></trkpt>"));
        }
        sb.AppendLine("    </trkseg>");
        sb.AppendLine("  </trk>");
        sb.AppendLine("</gpx>");

        return sb.ToString();
    }

    public double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = (lat2 - lat1) * (Math.PI / 180.0);
        var dLon = (lon2 - lon1) * (Math.PI / 180.0);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * (Math.PI / 180.0)) * Math.Cos(lat2 * (Math.PI / 180.0)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}

