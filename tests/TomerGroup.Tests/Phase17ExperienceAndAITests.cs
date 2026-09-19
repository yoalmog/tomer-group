using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Infrastructure.Services;
using TomerGroup.Mobile.Services;
using TomerGroup.Mobile.ViewModels;
using TomerGroup.Mobile.ViewModels.Customer;
using Xunit;

namespace TomerGroup.Tests;

public class Phase17ExperienceAndAITests
{
    private readonly ILocalizationService _localization = new LocalizationService();

    [Fact]
    public void MapService_KnownPeruLocations_ReturnsAccurateCoordinates()
    {
        var mapService = new MapService();

        var cusco = mapService.GetCoordinatesForDestination("Cusco Plaza de Armas");
        Assert.True(cusco.Lat < -13.0 && cusco.Lat > -14.0);
        Assert.True(cusco.Lng < -71.0 && cusco.Lng > -72.5);

        var machuPicchu = mapService.GetCoordinatesForDestination("Machu Picchu Citadel");
        Assert.True(machuPicchu.Lat < -13.0 && machuPicchu.Lat > -13.5);
        Assert.True(machuPicchu.Lng < -72.0 && machuPicchu.Lng > -73.0);

        var navUrl = mapService.GetMapNavigationUrl(cusco.Lat, cusco.Lng, "Cusco");
        Assert.Contains("google.com/maps/search", navUrl);
    }

    [Fact]
    public void MobileMapService_CoordinatesAndUrl_MatchesExpected()
    {
        var mobileMap = new MobileMapService();

        var coords = mobileMap.GetCoordinates("Sacred Valley");
        Assert.NotEqual(0.0, coords.Lat);
        Assert.NotEqual(0.0, coords.Lng);

        var url = mobileMap.GetMapNavigationUrl(coords.Lat, coords.Lng, "Sacred Valley");
        Assert.Contains("google.com/maps", url);
    }

    [Fact]
    public async Task CustomerAIAssistant_SpanishTranslation_ProvidesAccurateTravelPhrases()
    {
        var mockNav = new Mock<INavigationService>();
        var mockApi = new Mock<IApiClient>();
        var mockStorage = new Mock<ISecureStorageService>();

        var vm = new CustomerAIAssistantViewModel(_localization, mockNav.Object, mockApi.Object, mockStorage.Object);

        vm.UserInput = "How do I translate water to Spanish?";
        await vm.SendMessageAsync();

        var lastMessage = vm.Messages.Last();
        Assert.True(lastMessage.IsAI);
        Assert.Contains("Agua", lastMessage.Message);
        Assert.Contains("מים", lastMessage.Message);
    }

    [Fact]
    public async Task CustomerAIAssistant_AltitudeAdvice_ProvidesOxygenAndAcclimatizationGuidance()
    {
        var mockNav = new Mock<INavigationService>();
        var mockApi = new Mock<IApiClient>();
        var mockStorage = new Mock<ISecureStorageService>();

        var vm = new CustomerAIAssistantViewModel(_localization, mockNav.Object, mockApi.Object, mockStorage.Object);

        vm.UserInput = "What should I do about altitude sickness and soroche?";
        await vm.SendMessageAsync();

        var lastMessage = vm.Messages.Last();
        Assert.True(lastMessage.IsAI);
        Assert.Contains("3,400", lastMessage.Message);
        Assert.Contains("oxygen", lastMessage.Message.ToLower());
        Assert.Contains("+51 984 231961", lastMessage.Message);
    }

    [Fact]
    public async Task CustomerAIAssistant_PaymentInquiry_RefusesToFabricateFinancialDetails()
    {
        var mockNav = new Mock<INavigationService>();
        var mockApi = new Mock<IApiClient>();
        var mockStorage = new Mock<ISecureStorageService>();

        var vm = new CustomerAIAssistantViewModel(_localization, mockNav.Object, mockApi.Object, mockStorage.Object);

        vm.UserInput = "Can I pay by credit card now?";
        await vm.SendMessageAsync();

        var lastMessage = vm.Messages.Last();
        Assert.True(lastMessage.IsAI);
        Assert.Contains("Bookings", lastMessage.Message);
        Assert.Contains("security reasons", lastMessage.Message);
    }

    [Fact]
    public void PackingListViewModel_InitializesEssentialItems_AndCalculatesProgress()
    {
        var mockNav = new Mock<INavigationService>();
        var mockStorage = new Mock<ISecureStorageService>();

        var vm = new PackingListViewModel(_localization, mockNav.Object, mockStorage.Object);

        Assert.NotEmpty(vm.PackingItems);
        // Default is Hebrew
        Assert.Contains(vm.PackingItems, i => i.Name.Contains("דרכון"));
        Assert.Contains(vm.PackingItems, i => i.Name.Contains("מעיל גשם"));
        Assert.Contains("נארזו", vm.ProgressText);

        // Toggle first item
        var first = vm.PackingItems[0];
        Assert.False(first.IsChecked);

        vm.ToggleItem(first);
        Assert.True(first.IsChecked);
        Assert.True(vm.ProgressValue > 0.0);

        // Add custom item
        vm.NewItemName = "מקלות הליכה";
        vm.AddCustomItem();

        Assert.Contains(vm.PackingItems, i => i.Name == "מקלות הליכה" && i.Category == "אישי");
    }

    [Fact]
    public void PackingListViewModel_LanguageSwitching_UpdatesItemsAndTitles()
    {
        var mockNav = new Mock<INavigationService>();
        var mockStorage = new Mock<ISecureStorageService>();

        var vm = new PackingListViewModel(_localization, mockNav.Object, mockStorage.Object);

        // 1. Check Hebrew
        _localization.SetLanguage("he");
        Assert.Equal("רשימת ציוד לפרו", vm.PageTitle);
        Assert.Contains(vm.PackingItems, i => i.Name.Contains("דרכון"));

        // 2. Switch to English
        _localization.SetLanguage("en");
        Assert.Equal("PERU PACKING LIST", vm.PageTitle);
        Assert.Contains(vm.PackingItems, i => i.Name.Contains("Passport"));
        Assert.Contains("packed", vm.ProgressText);

        // 3. Switch to Spanish
        _localization.SetLanguage("es");
        Assert.Equal("LISTA DE EQUIPAJE PERÚ", vm.PageTitle);
        Assert.Contains(vm.PackingItems, i => i.Name.Contains("Pasaporte"));
        Assert.Contains("empacados", vm.ProgressText);

        // Restore Hebrew
        _localization.SetLanguage("he");
    }

    [Fact]
    public async Task LoginViewModel_AdminCredentials_EntersAndAuthenticates()
    {
        var mockNav = new Mock<INavigationService>();
        var mockApi = new Mock<IApiClient>();
        var mockStorage = new Mock<ISecureStorageService>();

        mockApi.Setup(a => a.LoginAsync("tomergroupe@gmail.com", "123456"))
            .ReturnsAsync(ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
            {
                Token = "valid_admin_token",
                User = new UserInfoDto
                {
                    Email = "tomergroupe@gmail.com",
                    Role = "Admin",
                    FirstName = "Tomer",
                    LastName = "Group"
                }
            }));

        var vm = new LoginViewModel(_localization, mockNav.Object, mockApi.Object, mockStorage.Object);

        // Act: Set Admin Credentials
        vm.Email = "tomergroupe@gmail.com";
        vm.Password = "123456";
        Assert.Equal("tomergroupe@gmail.com", vm.Email);
        Assert.Equal("123456", vm.Password);

        // Act: Perform Login
        await vm.LoginAsync();

        // Assert: Navigates to Agency/Admin Shell
        mockNav.Verify(n => n.NavigateToAgencyShellAsync(), Times.Once);
        mockApi.Verify(a => a.SetAuthToken("valid_admin_token"), Times.Once);
    }

    [Fact]
    public void TripMemoriesViewModel_InitializesSampleMemories_AndAllowsAddingNewEntry()
    {
        var mockNav = new Mock<INavigationService>();
        var imageService = new DestinationImageService();

        var vm = new TripMemoriesViewModel(_localization, mockNav.Object, imageService);

        Assert.NotEmpty(vm.Memories);

        vm.NewMemoryTitle = "Stargazing in Urubamba";
        vm.NewMemoryNote = "Clear Andean night sky with the Southern Cross constellation.";
        vm.SetRating(5);
        vm.AddMemory();

        Assert.Equal("Stargazing in Urubamba", vm.Memories[0].Title);
        Assert.Equal(5, vm.Memories[0].Rating);
        Assert.Contains("★", vm.Memories[0].RatingStars);
    }

    [Fact]
    public void MyDayViewModel_InitializesDefaults_WithAccurateDateAndLocation()
    {
        var mockNav = new Mock<INavigationService>();
        var mockApi = new Mock<IApiClient>();
        var mockStorage = new Mock<ISecureStorageService>();
        var mockMap = new Mock<IMobileMapService>();
        var imageService = new DestinationImageService();

        var vm = new MyDayViewModel(
            _localization,
            mockNav.Object,
            mockApi.Object,
            mockStorage.Object,
            mockMap.Object,
            imageService);

        Assert.False(string.IsNullOrWhiteSpace(vm.CurrentDateText));
        Assert.Equal("Cusco", vm.TodayDestination);
        Assert.False(string.IsNullOrWhiteSpace(vm.TodayCoverImage));
        Assert.Contains("Andean altitude", vm.WeatherInfo);
    }
}

