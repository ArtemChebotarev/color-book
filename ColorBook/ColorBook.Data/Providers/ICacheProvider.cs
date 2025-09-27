namespace ColorBook.Data.Providers;

public interface ICacheProvider
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan ttl);
    Task RemoveAsync(string key);
}
