using Microsoft.EntityFrameworkCore;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Models;
using TomerGroup.Infrastructure.Data;
using TomerGroup.Infrastructure.Services;
using Xunit;

namespace TomerGroup.Tests;

public class Phase8DocumentsTests
{
    private TomerDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TomerDbContext>()
            .UseInMemoryDatabase(databaseName: $"TomerGroup_Phase8_{Guid.NewGuid()}")
            .Options;
        return new TomerDbContext(options);
    }

    [Fact]
    public async Task UploadAsync_ValidPdf_SucceedsAndBindsCustomerPassport()
    {
        using var context = CreateInMemoryContext();
        var documentService = new DocumentService(context);

        var customerUserId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = customerUserId,
            FirstName = "Danny",
            LastName = "Cohen",
            PassportNumber = "24891024"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var request = new UploadDocumentDto
        {
            Name = "Machu Picchu Circuit 2 - Permit",
            HebrewName = "כרטיס כניסה למאצ'ו פיצ'ו - מסלול 2",
            Type = DocumentType.MachuPicchuPermit,
            FileExtension = "pdf",
            FileSizeBytes = 450 * 1024,
            Circuit = "Circuit 2 Classic",
            CustomerId = customer.Id,
            IsCustomerVisible = true
        };

        var staffUserId = Guid.NewGuid();
        var result = await documentService.UploadAsync(request, staffUserId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("application/pdf", result.Data.MimeType);
        Assert.Equal("24891024", result.Data.PermitPassportNumber);
        Assert.Equal("Circuit 2 Classic", result.Data.Circuit);
        Assert.True(result.Data.IsCustomerVisible);

        // Verify audit log created
        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "UploadDocument");
        Assert.NotNull(audit);
        Assert.Equal(staffUserId, audit.UserId);
    }

    [Fact]
    public async Task UploadAsync_InvalidExtension_FailsValidation()
    {
        using var context = CreateInMemoryContext();
        var documentService = new DocumentService(context);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Noam",
            LastName = "Levi",
            PassportNumber = "31928472"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var request = new UploadDocumentDto
        {
            Name = "Executable Script",
            Type = DocumentType.Other,
            FileExtension = "exe",
            FileSizeBytes = 2048,
            CustomerId = customer.Id
        };

        var result = await documentService.UploadAsync(request, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("not supported", result.Message);
    }

    [Fact]
    public async Task UploadAsync_FileSizeExceeds15MB_FailsValidation()
    {
        using var context = CreateInMemoryContext();
        var documentService = new DocumentService(context);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Yael",
            LastName = "Shani",
            PassportNumber = "48920193"
        };
        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        var request = new UploadDocumentDto
        {
            Name = "Gigantic High-Res Scan",
            Type = DocumentType.Passport,
            FileExtension = "jpg",
            FileSizeBytes = 20 * 1024 * 1024, // 20 MB (> 15 MB)
            CustomerId = customer.Id
        };

        var result = await documentService.UploadAsync(request, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("exceeds the 15 MB limit", result.Message);
    }

    [Fact]
    public async Task GetCustomerDocumentsAsync_StaffRole_ReturnsAllDocumentsIncludingHidden()
    {
        using var context = CreateInMemoryContext();
        var documentService = new DocumentService(context);

        var customerUserId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = customerUserId,
            FirstName = "Danny",
            LastName = "Cohen",
            PassportNumber = "24891024"
        };
        await context.Customers.AddAsync(customer);

        var visibleDoc = new Document
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Name = "Customer Voucher",
            Type = DocumentType.HotelVoucher,
            FileExtension = "pdf",
            MimeType = "application/pdf",
            StoragePath = "docs/1.pdf",
            IsCustomerVisible = true
        };

        var internalDoc = new Document
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Name = "Internal Cost Sheet",
            Type = DocumentType.Other,
            FileExtension = "pdf",
            MimeType = "application/pdf",
            StoragePath = "docs/2.pdf",
            IsCustomerVisible = false
        };

        await context.Documents.AddRangeAsync(visibleDoc, internalDoc);
        await context.SaveChangesAsync();

        var staffUserId = Guid.NewGuid();
        var result = await documentService.GetCustomerDocumentsAsync(customer.Id, staffUserId, "Operations");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetCustomerDocumentsAsync_CustomerRole_StrictIsolation_OnlyOwnVisibleDocuments()
    {
        using var context = CreateInMemoryContext();
        var documentService = new DocumentService(context);

        var customerUserId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = customerUserId,
            FirstName = "Danny",
            LastName = "Cohen",
            PassportNumber = "24891024"
        };
        await context.Customers.AddAsync(customer);

        var visibleDoc = new Document
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Name = "Customer Voucher",
            Type = DocumentType.HotelVoucher,
            FileExtension = "pdf",
            MimeType = "application/pdf",
            StoragePath = "docs/1.pdf",
            IsCustomerVisible = true
        };

        var internalDoc = new Document
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Name = "Internal Cost Sheet",
            Type = DocumentType.Other,
            FileExtension = "pdf",
            MimeType = "application/pdf",
            StoragePath = "docs/2.pdf",
            IsCustomerVisible = false
        };

        await context.Documents.AddRangeAsync(visibleDoc, internalDoc);
        await context.SaveChangesAsync();

        // Query as customer
        var result = await documentService.GetCustomerDocumentsAsync(customer.Id, customerUserId, "Customer");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        // Only visible document should be returned
        Assert.Single(result.Data);
        Assert.Equal("Customer Voucher", result.Data[0].Name);
    }

    [Fact]
    public async Task GetCustomerDocumentsAsync_CustomerRole_OtherCustomer_AccessDenied()
    {
        using var context = CreateInMemoryContext();
        var documentService = new DocumentService(context);

        var dannyUserId = Guid.NewGuid();
        var danny = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = dannyUserId,
            FirstName = "Danny",
            LastName = "Cohen"
        };

        var noamUserId = Guid.NewGuid();
        var noam = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = noamUserId,
            FirstName = "Noam",
            LastName = "Levi"
        };

        await context.Customers.AddRangeAsync(danny, noam);
        await context.SaveChangesAsync();

        // Noam tries to view Danny's documents
        var result = await documentService.GetCustomerDocumentsAsync(danny.Id, noamUserId, "Customer");

        Assert.False(result.Success);
        Assert.Contains("Access denied", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesDocument_AndLogsAudit()
    {
        using var context = CreateInMemoryContext();
        var documentService = new DocumentService(context);

        var doc = new Document
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Name = "Temporary Voucher",
            Type = DocumentType.TourVoucher,
            FileExtension = "pdf",
            MimeType = "application/pdf",
            StoragePath = "docs/temp.pdf",
            IsDeleted = false
        };

        await context.Documents.AddAsync(doc);
        await context.SaveChangesAsync();

        var staffUserId = Guid.NewGuid();
        var result = await documentService.DeleteAsync(doc.Id, staffUserId);

        Assert.True(result.Success);

        var updatedDoc = await context.Documents.FindAsync(doc.Id);
        Assert.NotNull(updatedDoc);
        Assert.True(updatedDoc.IsDeleted);

        var audit = await context.AuditLogs.FirstOrDefaultAsync(a => a.Action == "DeleteDocument");
        Assert.NotNull(audit);
        Assert.Equal(doc.Id.ToString(), audit.EntityId);
    }
}

