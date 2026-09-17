using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Mobile.Services;

namespace TomerGroup.Mobile.ViewModels.Customer;

public class MemoryCardItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string DateFormatted { get; set; } = DateTime.UtcNow.ToString("MMM dd, yyyy");
    public int Rating { get; set; } = 5;
    public string RatingStars => new string('★', Rating) + new string('☆', Math.Max(0, 5 - Rating));
}

public partial class TripMemoriesViewModel : BaseViewModel
{
    private readonly IDestinationImageService _imageService;

    [ObservableProperty]
    private string _newMemoryTitle = string.Empty;

    [ObservableProperty]
    private string _newMemoryNote = string.Empty;

    [ObservableProperty]
    private int _selectedRating = 5;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<MemoryCardItem> Memories { get; } = new();

    public TripMemoriesViewModel(
        ILocalizationService localization,
        INavigationService navigation,
        IDestinationImageService imageService)
        : base(localization, navigation)
    {
        _imageService = imageService;
        Title = "Trip Memories & Reviews";
        LoadSampleMemories();
    }

    private void LoadSampleMemories()
    {
        Memories.Clear();
        Memories.Add(new MemoryCardItem
        {
            Title = "Sunrise at Sun Gate (Inti Punku)",
            Note = "First glimpse of Machu Picchu through the clearing morning clouds. Unforgettable spiritual moment with the family.",
            ImageUrl = _imageService.GetDestinationHeroImage("Machu Picchu"),
            DateFormatted = DateTime.UtcNow.AddDays(-2).ToString("MMM dd, yyyy"),
            Rating = 5
        });

        Memories.Add(new MemoryCardItem
        {
            Title = "Weaving Community in Chinchero",
            Note = "Learned ancient natural dyeing techniques using cochineal and mountain herbs. Warmest hospitality from local Quechua weavers.",
            ImageUrl = _imageService.GetDestinationHeroImage("Sacred Valley"),
            DateFormatted = DateTime.UtcNow.AddDays(-4).ToString("MMM dd, yyyy"),
            Rating = 5
        });
    }

    [RelayCommand]
    public void SetRating(int rating)
    {
        SelectedRating = Math.Clamp(rating, 1, 5);
    }

    [RelayCommand]
    public void AddMemory()
    {
        if (string.IsNullOrWhiteSpace(NewMemoryTitle)) return;

        Memories.Insert(0, new MemoryCardItem
        {
            Title = NewMemoryTitle.Trim(),
            Note = NewMemoryNote.Trim(),
            ImageUrl = _imageService.GetDestinationHeroImage("Cusco"),
            DateFormatted = DateTime.UtcNow.ToString("MMM dd, yyyy"),
            Rating = SelectedRating
        });

        NewMemoryTitle = string.Empty;
        NewMemoryNote = string.Empty;
        SelectedRating = 5;
        StatusMessage = "Memory saved to your private Peru journal! ✨";
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Navigation.GoBackAsync();
    }
}
