using TomerGroup.Core.Enums;

namespace TomerGroup.Core.DTOs;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string TypeName => Type.ToString();
    public string FileExtension { get; set; } = "pdf";
    public string MimeType { get; set; } = "application/pdf";
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted => FileSizeBytes < 1024 * 1024 
        ? $"{FileSizeBytes / 1024.0:F1} KB" 
        : $"{FileSizeBytes / (1024.0 * 1024.0):F1} MB";
    public string? Circuit { get; set; }
    public string? PermitPassportNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? TripId { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime UploadDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsCustomerVisible { get; set; } = true;
    public string DownloadUrl => $"/api/documents/{Id}/download";
}

public class UploadDocumentDto
{

    public string Name { get; set; } = string.Empty;
    public string HebrewName { get; set; } = string.Empty;
    public DocumentType Type { get; set; } = DocumentType.TravelDocument;

    public string FileExtension { get; set; } = "pdf";
    public string? Circuit { get; set; }
    public string? PermitPassportNumber { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? TripId { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsCustomerVisible { get; set; } = true;
    public byte[]? FileBytes { get; set; }
    public long FileSizeBytes { get; set; }
}

public class DocumentContentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
}

