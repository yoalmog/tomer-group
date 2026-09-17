namespace TomerGroup.Core.Models;

public class Destination : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public string SpanishName { get; set; } = string.Empty;
    public string Region { get; set; } = "Cusco"; // Cusco, Puno, Lima, Arequipa, Madre de Dios
    public int AltitudeMeters { get; set; } = 3400; // e.g. 3,400 for Cusco, 2,430 for Machu Picchu, 5,036 for Rainbow Mountain
    public string? Description { get; set; }
    public string? AltitudeWarning { get; set; }
    public bool IsPopular { get; set; } = true;
    public string? ImageUrl { get; set; }
}

