using System.Text.Json;

namespace ProductService.Services.Cache;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null);

    Task RemoveAsync(string key);
}