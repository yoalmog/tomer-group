using TomerGroup.Core.DTOs;

namespace TomerGroup.Core.Interfaces;

public interface IPhoneAuthService
{
    Task<ApiResponse<bool>> SendVerificationCodeAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponseDto>> VerifyCodeAndLoginAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}

public interface ISecureStorageService
{
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
    Task ClearAllAsync();
}

