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
    string SupabaseUrl { get; }
    string SupabaseAnonKey { get; }
}

public class ApiConfiguration : IApiConfiguration
{
    // Public Production HTTPS Endpoints
    public const string DefaultRenderProductionUrl = "https://tomergroup-api.onrender.com/";
    public const string CustomDomainProductionUrl = "https://api.tomergroup.com/";

    // Public Supabase Configuration (anon key is safe for client applications)
    public const string DefaultSupabaseUrl = "https://tomergroup.supabase.co";
    public const string DefaultSupabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.tomergroup_anon_public_key";

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
        }
#if RELEASE
        else
        {
            // Release builds MUST NEVER use localhost or local IPs
            _baseAddress = new Uri(DefaultRenderProductionUrl);
            EnvironmentName = "Production";
            IsProduction = true;
        }
#else
        else
        {
            // In Debug mode, default to the public HTTPS cloud API unless explicitly configured
            var devCustomUrl = Environment.GetEnvironmentVariable("TOMERGROUP_DEV_API_URL");
            if (!string.IsNullOrWhiteSpace(devCustomUrl) && Uri.TryCreate(devCustomUrl, UriKind.Absolute, out var parsedDevUri))
            {
                _baseAddress = parsedDevUri;
                EnvironmentName = "Development-Custom";
                IsProduction = false;
            }
            else
            {
                // Default to Render HTTPS cloud backend so testing works without a local server running
                _baseAddress = new Uri(DefaultRenderProductionUrl);
                EnvironmentName = "Development-Cloud";
                IsProduction = false;
            }
        }
#endif

        SupabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? DefaultSupabaseUrl;
        SupabaseAnonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? DefaultSupabaseAnonKey;
    }

    public Uri BaseAddress => _baseAddress;
    public string EnvironmentName { get; }
    public bool IsProduction { get; }
    public string SupabaseUrl { get; }
    public string SupabaseAnonKey { get; }
}
