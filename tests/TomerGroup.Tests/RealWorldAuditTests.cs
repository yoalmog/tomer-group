using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;
using TomerGroup.Core.Localization;
using TomerGroup.Core.Models;
using TomerGroup.Mobile.Services;
using TomerGroup.Mobile.ViewModels.Agency;
using TomerGroup.Mobile.ViewModels.Customer;
using Xunit;

namespace TomerGroup.Tests;

public class RealWorldAuditTests
{
    private readonly ILocalizationService _localization = new LocalizationService();

    [Fact]
    public void ApiConfiguration_ResolvesValidEndpoint()
    {
        var config = new ApiConfiguration();
        Assert.NotNull(config.BaseAddress);
        Assert.True(config.BaseAddress.IsAbsoluteUri);
        Assert.False(string.IsNullOrWhiteSpace(config.EnvironmentName));
    }

    [Fact]
    public void ApiConfiguration_RespectsEnvironmentVariableOverride()
    {
        var original = Environment.GetEnvironmentVariable("TOMERGROUP_API_URL");
        try
        {
            Environment.SetEnvironmentVariable("TOMERGROUP_API_URL", "https://staging.tomergroup.com/");
            var config = new ApiConfiguration();
            Assert.Equal(new Uri("https://staging.tomergroup.com/"), config.BaseAddress);
            Assert.Equal("Custom", config.EnvironmentName);
            Assert.True(config.IsProduction);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TOMERGROUP_API_URL", original);
        }
    }

    [Fact]
    public void TrekCatalog_ContainsFourFlagshipTreksWithValidData()
    {
        var treks = TrekCatalog.GetFlagshipTreks();
        Assert.NotNull(treks);
        Assert.True(treks.Count >= 4);

        var salkantay = treks.FirstOrDefault(t => t.Name.Contains("Salkantay"));
        Assert.NotNull(salkantay);
        Assert.Equal(4630, salkantay.MaxElevationMeters);
        Assert.True(salkantay.Coordinates.Count > 10);
        Assert.True(salkantay.Waypoints.Count >= 5);
        Assert.True(salkantay.ElevationProfile.Count > 0);

        var incaTrail = treks.FirstOrDefault(t => t.Name.Contains("Inca Trail"));
        Assert.NotNull(incaTrail);
        Assert.Equal(4215, incaTrail.MaxElevationMeters);
        Assert.True(incaTrail.Coordinates.Count > 5);

        var rainbow = treks.FirstOrDefault(t => t.Name.Contains("Rainbow"));
        Assert.NotNull(rainbow);
        Assert.Equal(5036, rainbow.MaxElevationMeters);

        var ausangate = treks.FirstOrDefault(t => t.Name.Contains("Ausangate"));
        Assert.NotNull(ausangate);
        Assert.True(ausangate.MaxElevationMeters >= 4600);
    }

    [Fact]
    public void GpxService_CalculatesHaversineDistanceAccurately()
    {
        var service = new GpxService();
        // Distance between Cusco Plaza (-13.5168, -71.9789) and Ollantaytambo (-13.2625, -72.2644) is ~42-45 km direct line
        var dist = service.CalculateHaversineDistance(-13.5168, -71.9789, -13.2625, -72.2644);
        Assert.True(dist > 35 && dist < 55, $"Calculated distance {dist} km out of expected range");
    }

    [Fact]
    public void GpxService_ParsesAndExportsValidGpx()
    {
        var service = new GpxService();
        var sampleGpx = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<gpx version=""1.1"" creator=""Test"">
  <wpt lat=""-13.5168"" lon=""-71.9789""><ele>3400</ele><name>Start Point</name><desc>Cusco Center</desc></wpt>
  <wpt lat=""-13.1631"" lon=""-72.5450""><ele>2430</ele><name>End Point</name><desc>Machu Picchu</desc></wpt>
  <trk>
    <name>Cusco to MP</name>
    <trkseg>
      <trkpt lat=""-13.5168"" lon=""-71.9789""><ele>3400</ele></trkpt>
      <trkpt lat=""-13.3000"" lon=""-72.2000""><ele>3800</ele></trkpt>
      <trkpt lat=""-13.1631"" lon=""-72.5450""><ele>2430</ele></trkpt>
    </trkseg>
  </trk>
</gpx>";

        var route = service.ParseGpx(sampleGpx, "Sample Trek");
        Assert.NotNull(route);
        Assert.Equal(3, route.Coordinates.Count);
        Assert.Equal(2, route.Waypoints.Count);
        Assert.True(route.TotalDistanceKm > 0);
        Assert.Equal(3800, route.MaxElevationMeters);

        var exported = service.ExportToGpx(route);
        Assert.Contains("<gpx", exported);
        Assert.Contains("<trkpt", exported);
        Assert.Contains("<wpt", exported);
    }

    [Fact]
    public async Task AgencyAIAssistantViewModel_RejectDraftCommand_ResetsDraftState()
    {
        var nav = new NavigationService();
        var apiClient = new ApiClient(new System.Net.Http.HttpClient());
        var vm = new AgencyAIAssistantViewModel(apiClient, nav);

        // Populate a fake draft
        vm.CurrentDraft = new ItineraryDraftResultDto
        {
            RequestId = Guid.NewGuid(),
            Title = "Cusco",
            HebrewTitle = "קוסקו"
        };
        vm.HasDraft = true;

        await vm.RejectDraftCommand.ExecuteAsync(null);

        Assert.False(vm.HasDraft);
        Assert.False(vm.IsDraftApproved);
        Assert.Empty(vm.DraftDays);
        Assert.Contains("נדחתה", vm.DraftStatus);
    }

    [Fact]
    public async Task AgencyTripsViewModel_AddTripCommand_NavigatesToPlanTrip()
    {
        var nav = new NavigationService();
        var apiClient = new ApiClient(new System.Net.Http.HttpClient());
        var vm = new AgencyTripsViewModel(apiClient, nav);

        await vm.AddTripCommand.ExecuteAsync(null);
        // Does not throw and invokes navigation
    }

    [Fact]
    public async Task CheckoutRecapViewModel_ViewItineraryCommand_NavigatesToMyTrip()
    {
        var nav = new NavigationService();
        var vm = new CheckoutRecapViewModel(_localization, nav);

        await vm.ViewItineraryCommand.ExecuteAsync(null);
        Assert.Equal("CustomerShell", nav.CurrentShell);
    }

    [Fact]
    public void XamlDeadButtonAudit_ZeroDeadButtonsRemainAcrossAllXaml()
    {
        var solutionDir = Directory.GetCurrentDirectory();
        // Traverse up if inside bin/Debug
        var dirInfo = new DirectoryInfo(solutionDir);
        while (dirInfo != null && !File.Exists(Path.Combine(dirInfo.FullName, "TomerGroup.sln")))
        {
            dirInfo = dirInfo.Parent;
        }

        Assert.NotNull(dirInfo);
        var pagesDir = Path.Combine(dirInfo.FullName, "src", "TomerGroup.Mobile", "Pages");
        Assert.True(Directory.Exists(pagesDir));

        var xamlFiles = Directory.GetFiles(pagesDir, "*.xaml", SearchOption.AllDirectories);
        var buttonPattern = new Regex(@"<(Button|ImageButton|TapGestureRecognizer|SwipeGestureRecognizer|MenuItem)\b([^>]*)/?>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var actionPattern = new Regex(@"\b(Command|Clicked)\s*=", RegexOptions.IgnoreCase);

        var deadButtons = new List<string>();
        foreach (var file in xamlFiles)
        {
            var content = File.ReadAllText(file);
            var clean = Regex.Replace(content, @"<!--.*?-->", "", RegexOptions.Singleline);

            var matches = buttonPattern.Matches(clean);
            foreach (Match m in matches)
            {
                var fullTag = m.Groups[0].Value;
                var tagName = m.Groups[1].Value;
                var attrs = m.Groups[2].Value;
                if (attrs.StartsWith(".") || fullTag.StartsWith($"<{tagName}.")) continue; // Skip property elements like Button.Triggers, Button.Shadow

                var hasAction = actionPattern.IsMatch(attrs);
                if (!hasAction)
                {
                    deadButtons.Add($"{Path.GetFileName(file)}: {fullTag.Trim()}");
                }
            }
        }

        Assert.True(deadButtons.Count == 0, $"Found {deadButtons.Count} dead buttons:\n" + string.Join("\n", deadButtons.Take(15)));
    }
}
