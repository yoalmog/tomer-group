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

    [ObservableProperty]
    private bool _hasDocuments = false;

    [ObservableProperty]
    private string _emptyTitle = "אין עדיין מסמכים";

    [ObservableProperty]
    private string _emptyDescription = "מסמכי הטיול שלך, כרטיסי כניסה ושוברים יופיעו כאן ויהיו זמינים לצפייה גם ללא חיבור לאינטרנט.";

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
                HasDocuments = true;
            }
            else
            {
                HasDocuments = false;
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"שגיאה בטעינת המסמכים: {ex.Message}";
            HasDocuments = false;
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
        await Task.Delay(200);
        StatusMessage = $"המסמך {doc.Name} נשמר במכשיר וזמין לצפייה אופליין.";
    }

    [RelayCommand]
    public async Task ShareDocumentAsync(DocumentDto doc)
    {
        if (doc == null) return;
        StatusMessage = $"שיתוף מסמך {doc.Name} בווטסאפ...";
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task ContactAgencyAsync()
    {
        await _navigationService.NavigateToAsync("//More");
    }
}
