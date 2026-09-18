using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin,Manager,Operations,Sales")]
public class TreksController : ControllerBase
{
    private readonly TomerDbContext _context;

    public TreksController(TomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TrekRouteEntity>>>> GetAllTreks()
    {
        var treks = await _context.TrekRoutes
            .AsNoTracking()
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<TrekRouteEntity>>.Ok(treks));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TrekRouteEntity>>> GetTrekById(Guid id)
    {
        var trek = await _context.TrekRoutes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trek == null)
        {
            return NotFound(ApiResponse<TrekRouteEntity>.Fail("Trek route not found"));
        }

        return Ok(ApiResponse<TrekRouteEntity>.Ok(trek));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TrekRouteEntity>>> CreateTrek([FromBody] SaveTrekRouteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(ApiResponse<TrekRouteEntity>.Fail("Trek name is required"));
        }

        var trek = new TrekRouteEntity
        {
            Id = dto.Id ?? Guid.NewGuid(),
            TourId = dto.TourId,
            Name = dto.Name,
            Region = dto.Region,
            Difficulty = dto.Difficulty,
            DurationDays = dto.DurationDays,
            DistanceKm = dto.DistanceKm,
            MaxElevationMeters = dto.MaxElevationMeters,
            CoordinatesJson = dto.CoordinatesJson,
            WaypointsJson = dto.WaypointsJson,
            ElevationProfileJson = dto.ElevationProfileJson,
            OriginalGpxContent = dto.OriginalGpxContent,
            IsPublished = dto.IsPublished,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (trek.IsPublished)
        {
            trek.PublishedAt = DateTime.UtcNow;
            trek.PublishedByUserId = GetUserId();
        }

        _context.TrekRoutes.Add(trek);

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = GetUserId(),
            UserEmail = GetUserEmail(),
            Action = "CreateTrekRoute",
            TargetEntity = "TrekRoute",
            TargetEntityId = trek.Id,
            Description = $"Staff created trek route '{trek.Name}' ({trek.DistanceKm:F1} km, {trek.DurationDays} days)"
        });

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrekById), new { id = trek.Id }, ApiResponse<TrekRouteEntity>.Ok(trek));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TrekRouteEntity>>> UpdateTrek(Guid id, [FromBody] SaveTrekRouteDto dto)
    {
        var trek = await _context.TrekRoutes.FindAsync(id);
        if (trek == null)
        {
            return NotFound(ApiResponse<TrekRouteEntity>.Fail("Trek route not found"));
        }

        trek.Name = dto.Name;
        trek.Region = dto.Region;
        trek.Difficulty = dto.Difficulty;
        trek.DurationDays = dto.DurationDays;
        trek.DistanceKm = dto.DistanceKm;
        trek.MaxElevationMeters = dto.MaxElevationMeters;
        trek.CoordinatesJson = dto.CoordinatesJson;
        trek.WaypointsJson = dto.WaypointsJson;
        trek.ElevationProfileJson = dto.ElevationProfileJson;
        if (!string.IsNullOrEmpty(dto.OriginalGpxContent))
        {
            trek.OriginalGpxContent = dto.OriginalGpxContent;
        }
        trek.UpdatedAt = DateTime.UtcNow;

        if (dto.IsPublished && !trek.IsPublished)
        {
            trek.IsPublished = true;
            trek.PublishedAt = DateTime.UtcNow;
            trek.PublishedByUserId = GetUserId();
        }
        else if (!dto.IsPublished && trek.IsPublished)
        {
            trek.IsPublished = false;
        }

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = GetUserId(),
            UserEmail = GetUserEmail(),
            Action = "UpdateTrekRoute",
            TargetEntity = "TrekRoute",
            TargetEntityId = trek.Id,
            Description = $"Staff updated trek route '{trek.Name}'"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<TrekRouteEntity>.Ok(trek));
    }

    [HttpPost("{id}/publish")]
    public async Task<ActionResult<ApiResponse<TrekRouteEntity>>> PublishTrek(Guid id)
    {
        var trek = await _context.TrekRoutes.FindAsync(id);
        if (trek == null)
        {
            return NotFound(ApiResponse<TrekRouteEntity>.Fail("Trek route not found"));
        }

        trek.IsPublished = true;
        trek.PublishedAt = DateTime.UtcNow;
        trek.PublishedByUserId = GetUserId();
        trek.UpdatedAt = DateTime.UtcNow;

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = GetUserId(),
            UserEmail = GetUserEmail(),
            Action = "PublishTrekRoute",
            TargetEntity = "TrekRoute",
            TargetEntityId = trek.Id,
            Description = $"Published trek route '{trek.Name}' to live traveler catalog"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<TrekRouteEntity>.Ok(trek));
    }

    [HttpPost("{id}/unpublish")]
    public async Task<ActionResult<ApiResponse<TrekRouteEntity>>> UnpublishTrek(Guid id)
    {
        var trek = await _context.TrekRoutes.FindAsync(id);
        if (trek == null)
        {
            return NotFound(ApiResponse<TrekRouteEntity>.Fail("Trek route not found"));
        }

        trek.IsPublished = false;
        trek.UpdatedAt = DateTime.UtcNow;

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = GetUserId(),
            UserEmail = GetUserEmail(),
            Action = "UnpublishTrekRoute",
            TargetEntity = "TrekRoute",
            TargetEntityId = trek.Id,
            Description = $"Unpublished trek route '{trek.Name}'"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<TrekRouteEntity>.Ok(trek));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTrek(Guid id)
    {
        var trek = await _context.TrekRoutes.FindAsync(id);
        if (trek == null)
        {
            return NotFound(ApiResponse<bool>.Fail("Trek route not found"));
        }

        _context.TrekRoutes.Remove(trek);

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            UserId = GetUserId(),
            UserEmail = GetUserEmail(),
            Action = "DeleteTrekRoute",
            TargetEntity = "TrekRoute",
            TargetEntityId = id,
            Description = $"Deleted trek route '{trek.Name}'"
        });

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPost("import-gpx")]
    public ActionResult<ApiResponse<GpxImportResultDto>> ImportGpx([FromBody] string gpxContent)
    {
        if (string.IsNullOrWhiteSpace(gpxContent))
        {
            return BadRequest(ApiResponse<GpxImportResultDto>.Fail("GPX content cannot be empty"));
        }

        try
        {
            var doc = XDocument.Parse(gpxContent);
            XNamespace ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var trackPoints = doc.Descendants(ns + "trkpt").ToList();
            if (!trackPoints.Any())
            {
                trackPoints = doc.Descendants(ns + "rtept").ToList();
            }

            if (!trackPoints.Any())
            {
                return BadRequest(ApiResponse<GpxImportResultDto>.Fail("No track points (<trkpt> or <rtept>) found in GPX file"));
            }

            var coords = new List<TrekCoordinate>();
            double cumulativeDist = 0;
            double maxEle = double.MinValue;
            double minEle = double.MaxValue;
            double totalGain = 0;
            double? prevEle = null;

            for (int i = 0; i < trackPoints.Count; i++)
            {
                var pt = trackPoints[i];
                var latStr = pt.Attribute("lat")?.Value;
                var lonStr = pt.Attribute("lon")?.Value;

                if (double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                {
                    double ele = 0;
                    var eleElem = pt.Element(ns + "ele")?.Value;
                    if (!string.IsNullOrEmpty(eleElem) && double.TryParse(eleElem, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedEle))
                    {
                        ele = parsedEle;
                    }

                    if (i > 0)
                    {
                        var prev = coords[i - 1];
                        var step = CalculateHaversineDistance(prev.Latitude, prev.Longitude, lat, lon);
                        cumulativeDist += step;

                        if (prevEle.HasValue && ele > prevEle.Value)
                        {
                            totalGain += (ele - prevEle.Value);
                        }
                    }

                    prevEle = ele;
                    maxEle = Math.Max(maxEle, ele);
                    minEle = Math.Min(minEle, ele);

                    coords.Add(new TrekCoordinate(lat, lon, ele, cumulativeDist));
                }
            }

            // Extract waypoints
            var waypoints = new List<TrekWaypoint>();
            var wptElems = doc.Descendants(ns + "wpt").ToList();
            for (int i = 0; i < wptElems.Count; i++)
            {
                var w = wptElems[i];
                var latStr = w.Attribute("lat")?.Value;
                var lonStr = w.Attribute("lon")?.Value;
                var name = w.Element(ns + "name")?.Value ?? $"Waypoint {i + 1}";
                var desc = w.Element(ns + "desc")?.Value ?? string.Empty;

                if (double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                {
                    double ele = 0;
                    var eleElem = w.Element(ns + "ele")?.Value;
                    if (!string.IsNullOrEmpty(eleElem) && double.TryParse(eleElem, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedEle))
                    {
                        ele = parsedEle;
                    }

                    waypoints.Add(new TrekWaypoint
                    {
                        Name = name,
                        HebrewName = name,
                        Description = desc,
                        Latitude = lat,
                        Longitude = lon,
                        ElevationMeters = ele,
                        Type = WaypointType.Viewpoint
                    });
                }
            }

            var trackNameElem = doc.Descendants(ns + "trk").FirstOrDefault()?.Element(ns + "name")?.Value;
            var trackName = !string.IsNullOrEmpty(trackNameElem) ? trackNameElem : "Imported Trek Route";

            // Build elevation profile sample points
            var elevationProfile = new List<ElevationPoint>();
            int sampleInterval = Math.Max(1, coords.Count / 50);
            for (int i = 0; i < coords.Count; i += sampleInterval)
            {
                var pt = coords[i];
                elevationProfile.Add(new ElevationPoint(pt.DistanceFromStartKm, pt.ElevationMeters));
            }

            var result = new GpxImportResultDto
            {
                Success = true,
                TrackName = trackName,
                TotalDistanceKm = cumulativeDist,
                MaxElevationMeters = maxEle > double.MinValue ? (int)maxEle : 0,
                MinElevationMeters = minEle < double.MaxValue ? (int)minEle : 0,
                TotalAscentMeters = (int)totalGain,
                CoordinatePointsCount = coords.Count,
                WaypointsCount = waypoints.Count,
                CoordinatesJson = JsonSerializer.Serialize(coords),
                WaypointsJson = JsonSerializer.Serialize(waypoints),
                ElevationProfileJson = JsonSerializer.Serialize(elevationProfile)
            };

            return Ok(ApiResponse<GpxImportResultDto>.Ok(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<GpxImportResultDto>.Fail($"GPX Parsing failed: {ex.Message}"));
        }
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
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

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? "staff@tomergroup.com";
    }
}

[ApiController]
[Route("api/treks")]
public class PublicTreksController : ControllerBase
{
    private readonly TomerDbContext _context;

    public PublicTreksController(TomerDbContext context)
    {
        _context = context;
    }

    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<TrekRouteEntity>>>> GetPublishedTreks()
    {
        var publishedTreks = await _context.TrekRoutes
            .AsNoTracking()
            .Where(t => t.IsPublished)
            .OrderBy(t => t.Name)
            .ToListAsync();

        return Ok(ApiResponse<List<TrekRouteEntity>>.Ok(publishedTreks));
    }
}
