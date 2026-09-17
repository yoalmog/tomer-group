using System.Net;
using System.Net.Http.Headers;
using TomerGroup.Core.Interfaces;

namespace TomerGroup.Mobile.Services;

public class TokenRefreshHandler : DelegatingHandler
{
    private readonly ISecureStorageService _secureStorage;
    private readonly INavigationService _navigationService;

    public TokenRefreshHandler(ISecureStorageService secureStorage, INavigationService navigationService)
    {
        _secureStorage = secureStorage;
        _navigationService = navigationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _secureStorage.GetAsync("access_token");
        if (!string.IsNullOrEmpty(token) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If session expired (401 Unauthorized), transition cleanly
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshToken = await _secureStorage.GetAsync("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
            {
                await _secureStorage.ClearAllAsync();
                await _navigationService.NavigateToLoginAsync();
            }
        }

        return response;
    }
}

