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

    public PackingListViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        ISecureStorageService secureStorage)
        : base(localization, navigation)
    {
        _secureStorage = secureStorage;
        Title = "Smart Packing List";
        LoadDefaultItems();
    }

    private void LoadDefaultItems()
    {
        PackingItems.Clear();

        var defaults = new[]
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
            ProgressText = "No items";
            ProgressValue = 0;
            return;
        }

        var checkedCount = PackingItems.Count(i => i.IsChecked);
        var total = PackingItems.Count;
        var pct = (int)((double)checkedCount / total * 100);

        ProgressValue = (double)checkedCount / total;
        ProgressText = $"{checkedCount}/{total} packed ({pct}%)";
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
            Category = "Custom",
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

