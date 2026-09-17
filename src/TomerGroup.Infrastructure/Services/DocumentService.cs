using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;

namespace TomerGroup.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly TomerDbContext _context;
    private static readonly ConcurrentDictionary<string, byte[]> _inMemoryStorage = new();
    private static readonly string[] _allowedExtensions = { "pdf", "jpg", "jpeg", "png" };
    private const long _maxFileSizeBytes = 15 * 1024 * 1024; // 15 MB

    public DocumentService(TomerDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DocumentDto>> UploadAsync(UploadDocumentDto request, Guid uploadedByUserId, CancellationToken cancellationToken = default)
    {
        var ext = request.FileExtension.ToLower().TrimStart('.');
        if (!_allowedExtensions.Contains(ext))
        {
            return ApiResponse<DocumentDto>.Fail($"File extension '.{ext}' is not supported. Allowed formats: PDF, JPG, PNG.");
        }

        var fileSize = request.FileSizeBytes > 0 ? request.FileSizeBytes : (request.FileBytes?.Length ?? 1024);
        if (fileSize > _maxFileSizeBytes)
        {
            return ApiResponse<DocumentDto>.Fail($"File size exceeds the 15 MB limit. Provided size: {fileSize / (1024 * 1024.0):F1} MB.");
        }

        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<DocumentDto>.Fail("Customer not found");
        }

        var storagePath = $"documents/{request.CustomerId}/{Guid.NewGuid()}.{ext}";
        if (request.FileBytes != null && request.FileBytes.Length > 0)
        {
            _inMemoryStorage[storagePath] = request.FileBytes;
        }
        else
        {
            // Default sample byte buffer for ticket/permit simulation
            _inMemoryStorage[storagePath] = System.Text.Encoding.UTF8.GetBytes($"%PDF-1.4 Tomer Group Voucher: {request.Name}");
        }

        var mimeType = ext == "pdf" ? "application/pdf" : $"image/{ext}";

        var document = new Document
        {
            Name = request.Name,
            HebrewName = request.HebrewName,
            Type = request.Type,
            FileExtension = ext,
            MimeType = mimeType,
            StoragePath = storagePath,
            FileSizeBytes = fileSize,
            Circuit = request.Circuit,
            PermitPassportNumber = request.PermitPassportNumber ?? customer.PassportNumber,
            CustomerId = request.CustomerId,
            TripId = request.TripId,
            BookingId = request.BookingId,
            UploadDate = DateTime.UtcNow,
            ExpirationDate = request.ExpirationDate,
            IsCustomerVisible = request.IsCustomerVisible
        };

        await _context.Documents.AddAsync(document, cancellationToken);

        var audit = new AuditLog
        {
            Action = "UploadDocument",
            EntityName = "Document",
            EntityId = document.Id.ToString(),
            UserId = uploadedByUserId,
            UserEmail = "staff@tomergroup.com",
            MetadataJson = $"Uploaded {document.Type} '{document.Name}' for customer {customer.FirstName} {customer.LastName}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        document.Customer = customer;
        return ApiResponse<DocumentDto>.Ok(MapToDto(document), "Document uploaded successfully");
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetCustomerDocumentsAsync(Guid customerId, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        if (requestingRole == "Customer")
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
            if (customer == null || customer.UserId != requestingUserId)
            {
                return ApiResponse<List<DocumentDto>>.Fail("Access denied: You are not authorized to view another customer's documents");
            }
        }

        var query = _context.Documents
            .AsNoTracking()
            .Include(d => d.Customer)
            .Where(d => d.CustomerId == customerId && !d.IsDeleted);

        // Security check: Travelers can only see documents marked IsCustomerVisible = true
        if (requestingRole == "Customer")
        {
            query = query.Where(d => d.IsCustomerVisible);
        }

        var documents = await query
            .OrderByDescending(d => d.UploadDate)
            .Select(d => MapToDto(d))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<DocumentDto>>.Ok(documents);
    }

    public async Task<ApiResponse<DocumentDto>> GetByIdAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents
            .AsNoTracking()
            .Include(d => d.Customer)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (document == null)
        {
            return ApiResponse<DocumentDto>.Fail("Document not found");
        }

        // Customer Isolation check
        if (requestingRole == "Customer")
        {
            if (document.Customer?.UserId != requestingUserId)
            {
                return ApiResponse<DocumentDto>.Fail("Access denied: You are not authorized to view another customer's document");
            }

            if (!document.IsCustomerVisible)
            {
                return ApiResponse<DocumentDto>.Fail("Document not found");
            }
        }

        return ApiResponse<DocumentDto>.Ok(MapToDto(document));
    }

    public async Task<ApiResponse<DocumentContentDto>> GetContentAsync(Guid id, Guid requestingUserId, string requestingRole, CancellationToken cancellationToken = default)
    {
        var docResult = await GetByIdAsync(id, requestingUserId, requestingRole, cancellationToken);
        if (!docResult.Success || docResult.Data == null)
        {
            return ApiResponse<DocumentContentDto>.Fail(docResult.Message ?? "Document not found");
        }

        var doc = await _context.Documents.FindAsync(new object[] { id }, cancellationToken);
        if (doc == null)
        {
            return ApiResponse<DocumentContentDto>.Fail("Document not found");
        }

        byte[] bytes;
        if (_inMemoryStorage.TryGetValue(doc.StoragePath, out var storedBytes))
        {
            bytes = storedBytes;
        }
        else
        {
            bytes = System.Text.Encoding.UTF8.GetBytes($"%PDF-1.4 Tomer Group Voucher: {doc.Name}");
        }

        return ApiResponse<DocumentContentDto>.Ok(new DocumentContentDto
        {
            FileName = $"{doc.Name}.{doc.FileExtension}",
            ContentType = doc.MimeType,
            FileBytes = bytes
        });
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (document == null)
        {
            return ApiResponse<bool>.Fail("Document not found");
        }

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;

        var audit = new AuditLog
        {
            Action = "DeleteDocument",
            EntityName = "Document",
            EntityId = document.Id.ToString(),
            UserId = requestingUserId,
            UserEmail = "staff@tomergroup.com",
            MetadataJson = $"Deleted document {document.Name}"
        };
        await _context.AuditLogs.AddAsync(audit, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Document deleted successfully");
    }

    private static DocumentDto MapToDto(Document d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        HebrewName = d.HebrewName,
        Type = d.Type,
        FileExtension = d.FileExtension,
        MimeType = d.MimeType,
        FileSizeBytes = d.FileSizeBytes,
        Circuit = d.Circuit,
        PermitPassportNumber = d.PermitPassportNumber,
        CustomerId = d.CustomerId,
        CustomerName = d.Customer != null ? $"{d.Customer.FirstName} {d.Customer.LastName}" : string.Empty,
        TripId = d.TripId,
        BookingId = d.BookingId,
        UploadDate = d.UploadDate,
        ExpirationDate = d.ExpirationDate,
        IsCustomerVisible = d.IsCustomerVisible
    };
}

