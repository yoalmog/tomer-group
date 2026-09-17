using TomerGroup.Core.Interfaces;

namespace TomerGroup.Mobile.Services;

public class SecureStorageService : ISecureStorageService
{
    private readonly Dictionary<string, string> _vault = new();

    public Task SetAsync(string key, string value)
    {
        lock (_vault)
        {
            _vault[key] = value;
        }
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        lock (_vault)
        {
            _vault.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }
    }

    public Task RemoveAsync(string key)
    {
        lock (_vault)
        {
            _vault.Remove(key);
        }
        return Task.CompletedTask;
    }

    public Task ClearAllAsync()
    {
        lock (_vault)
        {
            _vault.Clear();
        }
        return Task.CompletedTask;
    }
}

