using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class PackingChecklistItem : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General";

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}

public partial class PackingListViewModel : BaseViewModel
{
    private readonly ISecureStorageService _secureStorage;

    [ObservableProperty]
    private string _newItemName = string.Empty;

    [ObservableProperty]
    private string _progressText = "0/0 items packed (0%)";

    [ObservableProperty]
    private double _progressValue;

    public ObservableCollection<PackingChecklistItem> PackingItems { get; } = new();

    public string PageTitle => CurrentLanguage switch
    {
        "en" => "PERU PACKING LIST",
        "es" => "LISTA DE EQUIPAJE PERÚ",
        _ => "רשימת ציוד לפרו"
    };

    public string BackButtonText => CurrentLanguage switch
    {
        "en" => "← Back",
        "es" => "← Volver",
        _ => "← חזרה"
    };

    public string ProgressTitle => CurrentLanguage switch
    {
        "en" => "Packing Progress",
        "es" => "Progreso del Equipaje",
        _ => "התקדמות אריזה"
    };

    public string ProTipText => CurrentLanguage switch
    {
        "en" => "💡 Pro Tip: Weight limit on PeruRail & Inca Rail trains to Machu Picchu is strictly 5kg (11lbs) per bag. Store excess luggage securely at your Cusco hotel.",
        "es" => "💡 Consejo: El límite de peso en los trenes de PeruRail e Inca Rail a Machu Picchu es estrictamente de 5 kg por bolso. Guarde el exceso de equipaje en su hotel de Cusco.",
        _ => "💡 טיפ חשוב: מגבלת המשקל ברכבות פרו-רייל ואינקה-רייל למאצ'ו פיצ'ו היא 5 ק\"ג בלבד לאדם. ניתן לאחסן מזוודות גדולות בבטחה במלון שלכם בקוסקו ללא תשלום."
    };

    public string AddCustomPlaceholder => CurrentLanguage switch
    {
        "en" => "Add your own custom item...",
        "es" => "Agregue su propio artículo...",
        _ => "הוסף פריט אישי משלך לרשימה..."
    };

    public string AddButtonText => CurrentLanguage switch
    {
        "en" => "+ Add",
        "es" => "+ Agregar",
        _ => "+ הוסף"
    };

    public string CustomCategoryLabel => CurrentLanguage switch
    {
        "en" => "Custom",
        "es" => "Personalizado",
        _ => "אישי"
    };

    public PackingListViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        ISecureStorageService secureStorage)
        : base(localization, navigation)
    {
        _secureStorage = secureStorage;
        Title = PageTitle;
        LoadDefaultItems();
    }

    protected override void OnLanguageChanged()
    {
        Title = PageTitle;
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(BackButtonText));
        OnPropertyChanged(nameof(ProgressTitle));
        OnPropertyChanged(nameof(ProTipText));
        OnPropertyChanged(nameof(AddCustomPlaceholder));
        OnPropertyChanged(nameof(AddButtonText));
        OnPropertyChanged(nameof(CustomCategoryLabel));

        // Preserve checked states by index where possible
        var checkedIndexes = PackingItems.Select((item, idx) => (item.IsChecked, idx)).Where(x => x.IsChecked).Select(x => x.idx).ToHashSet();
        LoadDefaultItems();
        for (int i = 0; i < PackingItems.Count; i++)
        {
            if (checkedIndexes.Contains(i))
            {
                PackingItems[i].IsChecked = true;
            }
        }
        UpdateProgress();
    }

    private void LoadDefaultItems()
    {
        PackingItems.Clear();

        (string name, string category)[] defaults = CurrentLanguage switch
        {
            "en" => new[]
            {
                ("Original Passport (Required for MP & Trains)", "Documents"),
                ("Printed Tour Vouchers & Tickets", "Documents"),
                ("TAM Immigration Card & Insurance", "Documents"),
                ("Waterproof Rain Jacket or Poncho", "Clothing"),
                ("Thermal Layers / Fleece (Cold Nights)", "Clothing"),
                ("Sturdy Broken-in Hiking Shoes", "Gear"),
                ("Daypack (20-30L capacity)", "Gear"),
                ("High SPF Sunscreen & UV Sunglasses", "Health & Altitude"),
                ("Insect Repellent (DEET for Machu Picchu)", "Health & Altitude"),
                ("Personal Altitude Medication (Sorojchi/Acetazolamide)", "Health & Altitude"),
                ("Universal Travel Adapter (Type A/C) & Power Bank", "Gear"),
                ("Reusable Water Bottle or Hydration Bladder", "Gear")
            },
            "es" => new[]
            {
                ("Pasaporte Original (Requerido para MP y Trenes)", "Documentos"),
                ("Vouchers Impresos y Boletos de Entrada", "Documentos"),
                ("Tarjeta Andina TAM y Seguro de Viaje", "Documentos"),
                ("Chaqueta Impermeable o Poncho para Lluvia", "Ropa"),
                ("Ropa Térmica / Polar (Noches Frías)", "Ropa"),
                ("Calzado de Trekking Cómodo y Usado", "Equipo"),
                ("Mochila de Día (20-30L)", "Equipo"),
                ("Bloqueador Solar SPF Alto y Lentes UV", "Salud y Altura"),
                ("Repelente de Insectos con DEET (Machu Picchu)", "Salud y Altura"),
                ("Medicación Personal para el Soroche / Altura", "Salud y Altura"),
                ("Adaptador Universal y Batería Portátil", "Equipo"),
                ("Botella Reutilizable o Bolsa de Hidratación", "Equipo")
            },
            _ => new[]
            {
                ("דרכון מקורי בתוקף (חובה למאצ'ו פיצ'ו ולרכבות)", "מסמכים"),
                ("שוברים מודפסים וכרטיסי עלייה לרכבת", "מסמכים"),
                ("טופס כניסה לפרו וביטוח נסיעות וחילוץ", "מסמכים"),
                ("מעיל גשם איכותי אטום למים או פונצ'ו", "ביגוד"),
                ("שכבות תרמיות / פליז חם (לילות קרים באנדים)", "ביגוד"),
                ("נעלי הליכה נוחות (שכבר הלכתם בהן)", "ציוד"),
                ("תיק יום נוח (נפח 20-30 ליטר)", "ציוד"),
                ("קרם הגנה SPF גבוה ומשקפי שמש איכותיים", "בריאות וגבהים"),
                ("חומר דוחה יתושים (למאצ'ו פיצ'ו והיער)", "בריאות וגבהים"),
                ("כדורי גבהים אישיים (סורוצ'ה / קלמנרקס)", "בריאות וגבהים"),
                ("מתאם חשמל בינלאומי ומטען נייד (Power Bank)", "ציוד"),
                ("בקבוק מים רב-פעמי או שלוקר מים", "ציוד")
            }
        };

        foreach (var (name, category) in defaults)
        {
            var item = new PackingChecklistItem
            {
                Name = name,
                Category = category,
                IsChecked = false
            };
            item.PropertyChanged += (s, e) => UpdateProgress();
            PackingItems.Add(item);
        }

        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (PackingItems.Count == 0)
        {
            ProgressText = CurrentLanguage switch
            {
                "en" => "No items",
                "es" => "Sin artículos",
                _ => "אין פריטים ברשימה"
            };
            ProgressValue = 0;
            return;
        }

        var checkedCount = PackingItems.Count(i => i.IsChecked);
        var total = PackingItems.Count;
        var pct = (int)((double)checkedCount / total * 100);

        ProgressValue = (double)checkedCount / total;
        ProgressText = CurrentLanguage switch
        {
            "en" => $"{checkedCount}/{total} packed ({pct}%)",
            "es" => $"{checkedCount}/{total} empacados ({pct}%)",
            _ => $"{checkedCount}/{total} פריטים נארזו ({pct}%)"
        };
    }

    [RelayCommand]
    public void ToggleItem(PackingChecklistItem item)
    {
        if (item != null)
        {
            item.IsChecked = !item.IsChecked;
            UpdateProgress();
        }
    }

    [RelayCommand]
    public void AddCustomItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemName)) return;

        var item = new PackingChecklistItem
        {
            Name = NewItemName.Trim(),
            Category = CustomCategoryLabel,
            IsChecked = false
        };
        item.PropertyChanged += (s, e) => UpdateProgress();
        PackingItems.Add(item);

        NewItemName = string.Empty;
        UpdateProgress();
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Navigation.GoBackAsync();
    }
}

