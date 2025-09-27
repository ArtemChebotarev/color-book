using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ColorBook.Data.Providers;

public class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            if (cachedValue is string json)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<T>(json);
                    return Task.FromResult(result);
                }
                catch
                {
                    // If deserialization fails, remove the invalid cache entry
                    _cache.Remove(key);
                }
            }
        }
        
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var json = JsonSerializer.Serialize(value);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };
        
        _cache.Set(key, json, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
