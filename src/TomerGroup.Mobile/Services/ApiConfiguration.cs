using System;
#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
using Microsoft.Maui.Devices;
#endif

namespace TomerGroup.Mobile.Services;

public interface IApiConfiguration
{
    Uri BaseAddress { get; }
    string EnvironmentName { get; }
    bool IsProduction { get; }
}

public class ApiConfiguration : IApiConfiguration
{
    private readonly Uri _baseAddress;

    public ApiConfiguration()
    {
        // 1. Check for custom environment variable (e.g. CI/CD or staging config)
        var customUrl = Environment.GetEnvironmentVariable("TOMERGROUP_API_URL");
        if (!string.IsNullOrWhiteSpace(customUrl) && Uri.TryCreate(customUrl, UriKind.Absolute, out var parsedUri))
        {
            _baseAddress = parsedUri;
            EnvironmentName = "Custom";
            IsProduction = parsedUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
            return;
        }

#if RELEASE
        // Production API endpoint
        _baseAddress = new Uri("https://api.tomergroup.com/");
        EnvironmentName = "Production";
        IsProduction = true;
#else
        // Development / Debug endpoint resolved according to runtime device/emulator
        EnvironmentName = "Development";
        IsProduction = false;

#if ANDROID || IOS || MACCATALYST || WINDOWS || USE_MAUI
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                // Android Emulator routes to host development machine via 10.0.2.2
                _baseAddress = new Uri("http://10.0.2.2:5000/");
            }
            else
            {
                // Physical Android device defaults to local network or fallback production API
                _baseAddress = new Uri("https://api.tomergroup.com/");
            }
        }
        else if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            if (DeviceInfo.DeviceType == DeviceType.Virtual)
            {
                // iOS Simulator shares localhost with the macOS host
                _baseAddress = new Uri("http://localhost:5000/");
            }
            else
            {
                _baseAddress = new Uri("https://api.tomergroup.com/");
            }
        }
        else
        {
            // Windows desktop / MacCatalyst / CLI development
            _baseAddress = new Uri("http://localhost:5000/");
        }
#else
        _baseAddress = new Uri("http://localhost:5000/");
#endif
#endif
    }

    public Uri BaseAddress => _baseAddress;
    public string EnvironmentName { get; }
    public bool IsProduction { get; }
}

