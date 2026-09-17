using TomerGroup.Core.Enums;

namespace TomerGroup.Core.Models;

public class Document : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public DocumentType Type { get; set; } = DocumentType.TravelDocument;
    public string FileExtension { get; set; } = "pdf"; // pdf, jpg, png
    public string MimeType { get; set; } = "application/pdf";
    public string StoragePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Circuit { get; set; } // Circuit 1, Circuit 2 Classic, Circuit 3, etc.
    public string? PermitPassportNumber { get; set; } // Required for Machu Picchu and Inca Trail government tickets

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }

    public Guid? BookingId { get; set; }
    public Booking? Booking { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpirationDate { get; set; }
    public bool IsCustomerVisible { get; set; } = true;
}

