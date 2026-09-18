namespace TomerGroup.Core.Models;

public class TrekRouteEntity : BaseEntity
{
    public Guid? TourId { get; set; }
    public Tour? Tour { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = "Cusco, Peru";
    public string Difficulty { get; set; } = "Challenging"; // Easy, Moderate, Challenging, Demanding
    public int DurationDays { get; set; } = 4;
    public double DistanceKm { get; set; } = 45.0;
    public int MaxElevationMeters { get; set; } = 4630;

    /// <summary>
    /// JSON serialized list of TrekCoordinate [ { latitude, longitude, elevationMeters, orderIndex } ]
    /// </summary>
    public string CoordinatesJson { get; set; } = "[]";

    /// <summary>
    /// JSON serialized list of TrekWaypoint [ { name, description, latitude, longitude, elevationMeters, type, orderIndex } ]
    /// </summary>
    public string WaypointsJson { get; set; } = "[]";

    /// <summary>
    /// JSON serialized list of ElevationPoint [ { distanceKm, elevationMeters, waypointName } ]
    /// </summary>
    public string ElevationProfileJson { get; set; } = "[]";

    /// <summary>
    /// Original GPX XML string preserved for administrative review & auditing.
    /// </summary>
    public string? OriginalGpxContent { get; set; }

    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }
}

