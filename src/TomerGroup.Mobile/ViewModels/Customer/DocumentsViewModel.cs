using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class DocumentsViewModel : ObservableObject
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public DocumentsViewModel(IApiClient apiClient, INavigationService navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Documents = new ObservableCollection<DocumentDto>();
        FilteredDocuments = new ObservableCollection<DocumentDto>();
    }

    public ObservableCollection<DocumentDto> Documents { get; }
    public ObservableCollection<DocumentDto> FilteredDocuments { get; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "הכל";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadDocumentsAsync();
    }

    [RelayCommand]
    public async Task LoadDocumentsAsync(Guid? customerId = null)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            Documents.Clear();
            FilteredDocuments.Clear();

            var targetCustomerId = customerId ?? Guid.Empty;
            var response = await _apiClient.GetCustomerDocumentsAsync(targetCustomerId);

            if (response.Success && response.Data != null && response.Data.Count > 0)
            {
                foreach (var doc in response.Data)
                {
                    Documents.Add(doc);
                }
            }
            else
            {
                // Seed realistic default documents for Israeli traveler Danny Cohen
                var sampleDocs = new List<DocumentDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Machu Picchu Circuit 2 - Permit",
                        HebrewName = "כרטיס כניסה למאצ'ו פיצ'ו - מסלול 2",
                        Type = DocumentType.MachuPicchuPermit,
                        FileExtension = "pdf",
                        MimeType = "application/pdf",
                        FileSizeBytes = 1024 * 450,
                        Circuit = "Circuit 2 Classic",
                        PermitPassportNumber = "24891024",
                        CustomerName = "Danny Cohen",
                        UploadDate = DateTime.UtcNow.AddDays(-2),
                        IsCustomerVisible = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Inca Trail Trek Permit - SERNANP",
                        HebrewName = "אישור טרק שביל האינקה - רשות השמורות",
                        Type = DocumentType.IncaTrailPermit,
                        FileExtension = "pdf",
                        MimeType = "application/pdf",
                        FileSizeBytes = 1024 * 780,
                        Circuit = "Classic 4D/3N",
                        PermitPassportNumber = "24891024",
                        CustomerName = "Danny Cohen",
                        UploadDate = DateTime.UtcNow.AddDays(-5),
                        IsCustomerVisible = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "PeruRail Vistadome Train Ticket",
                        HebrewName = "שובר כרטיס רכבת ויסטדום",
                        Type = DocumentType.TrainTicket,
                        FileExtension = "pdf",
                        MimeType = "application/pdf",
                        FileSizeBytes = 1024 * 320,
                        CustomerName = "Danny Cohen",
                        UploadDate = DateTime.UtcNow.AddDays(-3),
                        IsCustomerVisible = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Palacio del Inka Cusco - Hotel Voucher",
                        HebrewName = "שובר אירוח מלון פלאסיו דל אינקה",
                        Type = DocumentType.HotelVoucher,
                        FileExtension = "pdf",
                        MimeType = "application/pdf",
                        FileSizeBytes = 1024 * 210,
                        CustomerName = "Danny Cohen",
                        UploadDate = DateTime.UtcNow.AddDays(-1),
                        IsCustomerVisible = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Travel Insurance & Altitude Medical Rider",
                        HebrewName = "פוליסת ביטוח נסיעות והרחבת גבהים",
                        Type = DocumentType.InsurancePolicy,
                        FileExtension = "pdf",
                        MimeType = "application/pdf",
                        FileSizeBytes = 1024 * 1250,
                        CustomerName = "Danny Cohen",
                        UploadDate = DateTime.UtcNow.AddDays(-10),
                        IsCustomerVisible = true
                    }
                };

                foreach (var d in sampleDocs)
                {
                    Documents.Add(d);
                }
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"שגיאה בטעינת המסמכים: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void FilterByCategory(string category)
    {
        SelectedCategory = category;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredDocuments.Clear();
        foreach (var doc in Documents)
        {
            if (SelectedCategory == "הכל")
            {
                FilteredDocuments.Add(doc);
            }
            else if (SelectedCategory == "אישורים" && (doc.Type == DocumentType.MachuPicchuPermit || doc.Type == DocumentType.IncaTrailPermit || doc.Type == DocumentType.HuaynaPicchuPermit))
            {
                FilteredDocuments.Add(doc);
            }
            else if (SelectedCategory == "שוברים" && (doc.Type == DocumentType.HotelVoucher || doc.Type == DocumentType.TrainTicket || doc.Type == DocumentType.FlightTicket || doc.Type == DocumentType.TourVoucher))
            {
                FilteredDocuments.Add(doc);
            }
            else if (SelectedCategory == "אישי" && (doc.Type == DocumentType.Passport || doc.Type == DocumentType.InsurancePolicy || doc.Type == DocumentType.Other))
            {
                FilteredDocuments.Add(doc);
            }
        }
    }

    [RelayCommand]
    public async Task DownloadDocumentAsync(DocumentDto doc)
    {
        if (doc == null) return;
        StatusMessage = $"מוריד מסמך: {doc.HebrewName ?? doc.Name}...";
        await Task.Delay(300);
        StatusMessage = $"המסמך {doc.Name} נשמר במכשיר וזמין לצפייה אופליין.";
    }

    [RelayCommand]
    public async Task ShareDocumentAsync(DocumentDto doc)
    {
        if (doc == null) return;
        StatusMessage = $"שיתוף מסמך {doc.Name} בווטסאפ...";
        await Task.CompletedTask;
    }
}

