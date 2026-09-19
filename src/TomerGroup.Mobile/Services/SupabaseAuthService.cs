using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TomerGroup.Core.DTOs;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Mobile.Services;

public interface ISupabaseAuthService
{
    Task<ApiResponse<LoginResponseDto>> SignUpAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string phone,
        string language = "he");

    Task<ApiResponse<LoginResponseDto>> SignInAsync(string email, string password);

    Task<ApiResponse<bool>> SignOutAsync();

    Task<ApiResponse<bool>> ResetPasswordForEmailAsync(string email);

    Task<bool> RestoreSessionAsync();

    bool IsAuthenticated { get; }
}

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IApiConfiguration _apiConfig;
    private readonly IApiClient _apiClient;
    private readonly ISecureStorageService _secureStorage;

    public SupabaseAuthService(
        HttpClient httpClient,
        IApiConfiguration apiConfig,
        IApiClient apiClient,
        ISecureStorageService secureStorage)
    {
        _httpClient = httpClient;
        _apiConfig = apiConfig;
        _apiClient = apiClient;
        _secureStorage = secureStorage;

        if (!string.IsNullOrWhiteSpace(_apiConfig.SupabaseUrl))
        {
            var baseUrl = _apiConfig.SupabaseUrl.TrimEnd('/');
            _httpClient.BaseAddress = new Uri($"{baseUrl}/auth/v1/");
            if (!string.IsNullOrWhiteSpace(_apiConfig.SupabaseAnonKey))
            {
                _httpClient.DefaultRequestHeaders.Add("apikey", _apiConfig.SupabaseAnonKey);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiConfig.SupabaseAnonKey);
            }
        }
    }

    public bool IsAuthenticated => _apiClient.IsAuthenticated;

    public async Task<ApiResponse<LoginResponseDto>> SignUpAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string phone,
        string language = "he")
    {
        try
        {
            // 1. Attempt Supabase Auth direct registration
            var signupPayload = new
            {
                email = email.Trim().ToLower(),
                password = password,
                data = new
                {
                    first_name = firstName.Trim(),
                    last_name = lastName.Trim(),
                    phone = phone.Trim(),
                    language = language
                }
            };

            var response = await _httpClient.PostAsJsonAsync("signup", signupPayload);
            if (response.IsSuccessStatusCode)
            {
                using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var root = jsonDoc.RootElement;

                var accessToken = root.TryGetProperty("access_token", out var tokenProp) ? tokenProp.GetString() : null;
                var refreshToken = root.TryGetProperty("refresh_token", out var refreshProp) ? refreshProp.GetString() : null;
                var authUserId = root.TryGetProperty("user", out var userProp) && userProp.TryGetProperty("id", out var idProp) ? idProp.GetString() : Guid.NewGuid().ToString();

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    _apiClient.SetAuthToken(accessToken);
                    await _secureStorage.SetAsync("auth_token", accessToken);
                    if (!string.IsNullOrWhiteSpace(refreshToken))
                    {
                        await _secureStorage.SetAsync("refresh_token", refreshToken);
                    }

                    // 2. Sync profile to ASP.NET Core API
                    await _apiClient.SyncSupabaseProfileAsync(new CreateCustomerProfileRequestDto
                    {
                        AuthUserId = authUserId ?? string.Empty,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Phone = phone,
                        Language = language
                    });

                    var loginResponse = new LoginResponseDto
                    {
                        Token = accessToken,
                        RefreshToken = refreshToken ?? string.Empty,
                        ExpiresAt = DateTime.UtcNow.AddHours(2),
                        User = new UserInfoDto
                        {
                            Id = Guid.TryParse(authUserId, out var parsedGuid) ? parsedGuid : Guid.NewGuid(),
                            Email = email,
                            FirstName = firstName,
                            LastName = lastName,
                            Role = "Customer",
                            PreferredLanguage = language
                        }
                    };

                    return ApiResponse<LoginResponseDto>.Ok(loginResponse, "Account created successfully with Supabase Cloud Auth");
                }
            }
        }
        catch
        {
            // If direct Supabase endpoint is blocked or offline, seamlessly fallback to API registration
        }

        // Resilient Fallback: Register through ASP.NET Core API
        var apiResult = await _apiClient.RegisterCustomerAsync(new CustomerRegisterRequestDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Password = password,
            ConfirmPassword = password,
            PreferredLanguage = language
        });

        if (apiResult.Success && apiResult.Data?.Token != null)
        {
            _apiClient.SetAuthToken(apiResult.Data.Token);
            await _secureStorage.SetAsync("auth_token", apiResult.Data.Token);
            if (!string.IsNullOrWhiteSpace(apiResult.Data.RefreshToken))
            {
                await _secureStorage.SetAsync("refresh_token", apiResult.Data.RefreshToken);
            }
        }

        return apiResult;
    }

    public async Task<ApiResponse<LoginResponseDto>> SignInAsync(string email, string password)
    {
        try
        {
            var loginPayload = new
            {
                email = email.Trim().ToLower(),
                password = password
            };

            var response = await _httpClient.PostAsJsonAsync("token?grant_type=password", loginPayload);
            if (response.IsSuccessStatusCode)
            {
                using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var root = jsonDoc.RootElement;

                var accessToken = root.TryGetProperty("access_token", out var tokenProp) ? tokenProp.GetString() : null;
                var refreshToken = root.TryGetProperty("refresh_token", out var refreshProp) ? refreshProp.GetString() : null;
                var authUserId = root.TryGetProperty("user", out var userProp) && userProp.TryGetProperty("id", out var idProp) ? idProp.GetString() : string.Empty;

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    _apiClient.SetAuthToken(accessToken);
                    await _secureStorage.SetAsync("auth_token", accessToken);
                    if (!string.IsNullOrWhiteSpace(refreshToken))
                    {
                        await _secureStorage.SetAsync("refresh_token", refreshToken);
                    }

                    var loginResponse = new LoginResponseDto
                    {
                        Token = accessToken,
                        RefreshToken = refreshToken ?? string.Empty,
                        ExpiresAt = DateTime.UtcNow.AddHours(2),
                        User = new UserInfoDto
                        {
                            Id = Guid.TryParse(authUserId, out var parsedGuid) ? parsedGuid : Guid.NewGuid(),
                            Email = email,
                            Role = "Customer"
                        }
                    };

                    return ApiResponse<LoginResponseDto>.Ok(loginResponse, "Signed in successfully");
                }
            }
        }
        catch
        {
            // Fallback to API direct authentication
        }

        // Resilient Fallback: Login through ASP.NET Core API
        var apiResult = await _apiClient.LoginAsync(email, password);
        if (apiResult.Success && apiResult.Data?.Token != null)
        {
            _apiClient.SetAuthToken(apiResult.Data.Token);
            await _secureStorage.SetAsync("auth_token", apiResult.Data.Token);
            if (!string.IsNullOrWhiteSpace(apiResult.Data.RefreshToken))
            {
                await _secureStorage.SetAsync("refresh_token", apiResult.Data.RefreshToken);
            }
        }

        return apiResult;
    }

    public async Task<ApiResponse<bool>> SignOutAsync()
    {
        _apiClient.SetAuthToken(null);
        await _secureStorage.RemoveAsync("auth_token");
        await _secureStorage.RemoveAsync("refresh_token");
        return ApiResponse<bool>.Ok(true, "Signed out successfully");
    }

    public async Task<ApiResponse<bool>> ResetPasswordForEmailAsync(string email)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("recover", new { email = email.Trim().ToLower() });
            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.Ok(true, "If an account exists with this email, recovery instructions have been sent.");
            }
        }
        catch
        {
            // Fallback to API
        }

        var apiResult = await _apiClient.ForgotPasswordAsync(email);
        return apiResult.Success
            ? ApiResponse<bool>.Ok(true, apiResult.Message ?? "Password reset instructions issued.")
            : ApiResponse<bool>.Fail(apiResult.Message ?? "Password reset failed");
    }

    public async Task<bool> RestoreSessionAsync()
    {
        try
        {
            var token = await _secureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _apiClient.SetAuthToken(token);
                return true;
            }
        }
        catch
        {
            // Secure storage access failure or cleared
        }

        return false;
    }
}
