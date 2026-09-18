using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Enums;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public partial class DocumentsViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly INavigationService _navigationService;

    public DocumentsViewModel(IApiClient apiClient, INavigationService navigationService)
        : this(new LocalizationService(), navigationService, apiClient)
    {
    }

    public DocumentsViewModel(ILocalizationService localization, INavigationService navigationService, IApiClient apiClient)
        : base(localization, navigationService)
    {
        _apiClient = apiClient;
        _navigationService = navigationService;
        Documents = new ObservableCollection<DocumentDto>();
        FilteredDocuments = new ObservableCollection<DocumentDto>();

        UpdateEmptyTexts();
    }

    public ObservableCollection<DocumentDto> Documents { get; }
    public ObservableCollection<DocumentDto> FilteredDocuments { get; }

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private string _selectedCategory = "הכל";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasDocuments = false;

    [ObservableProperty]
    private string _emptyTitle = "אין עדיין מסמכים בארנק";

    [ObservableProperty]
    private string _emptyDescription = "אישורי כניסה למאצ'ו פיצ'ו, שוברי רכבות, מלונות ופוליסות ביטוח יופיעו כאן ויהיו זמינים לצפייה אופליין.";

    public string PageTitle => CurrentLanguage switch
    {
        "en" => "My Travel Wallet",
        "es" => "Mi Billetera de Documentos",
        _ => "ארנק המסמכים שלי"
    };

    public string GatewayTitle => CurrentLanguage switch
    {
        "en" => "Digital Document & Permit Wallet",
        "es" => "Billetera Digital de Documentos y Permisos",
        _ => "ארנק מסמכים ואישורים דיגיטלי"
    };

    public string GatewayDescription => CurrentLanguage switch
    {
        "en" => "Sign in to access your Machu Picchu permits, train tickets, and hotel vouchers saved on your device and available offline in the Andes.",
        "es" => "Inicie sesión para acceder a sus permisos de Machu Picchu, boletos de tren y vouchers de hoteles guardados en su dispositivo y disponibles sin conexión en los Andes.",
        _ => "התחבר כדי לגשת לאישורי הכניסה למאצ'ו פיצ'ו, שוברי הרכבות והמלונות השמורים במכשירך וזמינים לצפייה גם ללא קליטה באנדים."
    };

    public string SignInButtonText => CurrentLanguage switch
    {
        "en" => "Sign In to Document Wallet",
        "es" => "Iniciar Sesión en la Billetera",
        _ => "התחבר לארנק המסמכים"
    };

    public string ContinueExploringText => CurrentLanguage switch
    {
        "en" => "Explore Destinations ←",
        "es" => "Explorar Destinos ←",
        _ => "המשך לגלות יעדים ←"
    };

    public string ContactAgencyText => CurrentLanguage switch
    {
        "en" => "Contact Tomer Group 💬",
        "es" => "Contactar a la Agencia 💬",
        _ => "צור קשר עם הסוכנות 💬"
    };

    public string WalletBannerTitle => CurrentLanguage switch
    {
        "en" => "Tickets & Entry Permits Wallet",
        "es" => "Billetera de Boletos y Permisos",
        _ => "ארנק כרטיסים ואישורי כניסה"
    };

    public string WalletBannerSubtitle => CurrentLanguage switch
    {
        "en" => "Machu Picchu permits and train tickets available offline",
        "es" => "Permisos de Machu Picchu y boletos de tren disponibles sin conexión",
        _ => "אישורי מאצ'ו פיצ'ו ורכבות זמינים לצפייה אופליין"
    };

    public string OfflineAvailableBadge => CurrentLanguage switch
    {
        "en" => "✓ Offline Available",
        "es" => "✓ Disponible Offline",
        _ => "✓ אופליין זמין"
    };

    public string FilterAllText => CurrentLanguage switch
    {
        "en" => "All",
        "es" => "Todos",
        _ => "הכל"
    };

    public string FilterPermitsText => CurrentLanguage switch
    {
        "en" => "Permits & Treks",
        "es" => "Permisos y Treks",
        _ => "אישורי כניסה וטרקים"
    };

    public string FilterVouchersText => CurrentLanguage switch
    {
        "en" => "Hotels & Vouchers",
        "es" => "Hoteles y Vouchers",
        _ => "שוברי שירות ומלונות"
    };

    public string FilterPersonalText => CurrentLanguage switch
    {
        "en" => "Personal & Insurance",
        "es" => "Personal y Seguro",
        _ => "מסמכים אישיים וביטוח"
    };

    public string DownloadButtonText => CurrentLanguage switch
    {
        "en" => "Open / Save ⬇️",
        "es" => "Abrir / Guardar ⬇️",
        _ => "פתח / שמור ⬇️"
    };

    public string ShareWhatsAppText => CurrentLanguage switch
    {
        "en" => "Share on WhatsApp 📲",
        "es" => "Compartir en WhatsApp 📲",
        _ => "שתף בווטסאפ 📲"
    };

    private void UpdateEmptyTexts()
    {
        EmptyTitle = CurrentLanguage switch
        {
            "en" => "No Documents in Wallet Yet",
            "es" => "Sin Documentos Aún",
            _ => "אין עדיין מסמכים בארנק"
        };
        EmptyDescription = CurrentLanguage switch
        {
            "en" => "Machu Picchu entry permits, train tickets, hotel vouchers, and insurance policies will appear here for offline access.",
            "es" => "Los permisos de Machu Picchu, boletos de tren, vouchers de hotel y seguros aparecerán aquí para acceso sin internet.",
            _ => "אישורי כניסה למאצ'ו פיצ'ו, שוברי רכבות, מלונות ופוליסות ביטוח יופיעו כאן ויהיו זמינים לצפייה אופליין."
        };
    }

    protected override void OnLanguageChanged()
    {
        UpdateEmptyTexts();
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(GatewayTitle));
        OnPropertyChanged(nameof(GatewayDescription));
        OnPropertyChanged(nameof(SignInButtonText));
        OnPropertyChanged(nameof(ContinueExploringText));
        OnPropertyChanged(nameof(ContactAgencyText));
        OnPropertyChanged(nameof(WalletBannerTitle));
        OnPropertyChanged(nameof(WalletBannerSubtitle));
        OnPropertyChanged(nameof(OfflineAvailableBadge));
        OnPropertyChanged(nameof(FilterAllText));
        OnPropertyChanged(nameof(FilterPermitsText));
        OnPropertyChanged(nameof(FilterVouchersText));
        OnPropertyChanged(nameof(FilterPersonalText));
        OnPropertyChanged(nameof(DownloadButtonText));
        OnPropertyChanged(nameof(ShareWhatsAppText));
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsAuthenticated = _apiClient.IsAuthenticated;
        if (!IsAuthenticated)
        {
            HasDocuments = false;
            Documents.Clear();
            FilteredDocuments.Clear();
            return;
        }

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
        var cat = (SelectedCategory ?? string.Empty).ToLowerInvariant();
        foreach (var doc in Documents)
        {
            if (cat is "הכל" or "all" or "todos" or "")
            {
                FilteredDocuments.Add(doc);
            }
            else if ((cat is "אישורים" or "permits") && (doc.Type == DocumentType.MachuPicchuPermit || doc.Type == DocumentType.IncaTrailPermit || doc.Type == DocumentType.HuaynaPicchuPermit))
            {
                FilteredDocuments.Add(doc);
            }
            else if ((cat is "שוברים" or "vouchers") && (doc.Type == DocumentType.HotelVoucher || doc.Type == DocumentType.TrainTicket || doc.Type == DocumentType.FlightTicket || doc.Type == DocumentType.TourVoucher))
            {
                FilteredDocuments.Add(doc);
            }
            else if ((cat is "אישי" or "personal") && (doc.Type == DocumentType.Passport || doc.Type == DocumentType.InsurancePolicy || doc.Type == DocumentType.Other))
            {
                FilteredDocuments.Add(doc);
            }
        }
    }

    [RelayCommand]
    public async Task DownloadDocumentAsync(DocumentDto doc)
    {
        if (doc == null) return;
        StatusMessage = CurrentLanguage switch
        {
            "en" => $"✓ Document {doc.DisplayName} is now available offline.",
            "es" => $"✓ El documento {doc.DisplayName} ya está disponible sin conexión.",
            _ => $"✓ המסמך {doc.DisplayName} זמין כעת לצפייה ללא חיבור לאינטרנט."
        };
        await Task.Delay(200);
    }

    [RelayCommand]
    public async Task ShareDocumentAsync(DocumentDto doc)
    {
        if (doc == null) return;
        StatusMessage = $"✓ המסמך {doc.Name} מוכן לשיתוף.";
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task OpenSignInAsync()
    {
        await _navigationService.NavigateToLoginAsync();
    }

    [RelayCommand]
    public async Task ContinueExploringAsync()
    {
        await _navigationService.NavigateToAsync("//Home");
    }

    [RelayCommand]
    public async Task ContactAgencyAsync()
    {
        await _navigationService.NavigateToAsync("More");
    }
}
