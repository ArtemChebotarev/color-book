using System.Text.Json;
using ColorBook.Data.Models;
using ColorBook.Data.Providers;
using ColorBook.Data.Repositories;
using MongoDB.Driver;

public class MongoCacheProvider : ICacheProvider
{
    private readonly IMongoCollection<CatalogCacheEntry> _collection;

    public MongoCacheProvider(IMongoContext context)
    {
        _collection = context.CatalogCache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var filter = Builders<CatalogCacheEntry>.Filter.Eq(x => x.Key, key);
        var entry = await _collection.Find(filter).FirstOrDefaultAsync();
        return entry == null 
            ? default 
            : JsonSerializer.Deserialize<T>(entry.Value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var json = JsonSerializer.Serialize(value);
        var expireAt = DateTime.UtcNow.Add(ttl);

        var filter = Builders<CatalogCacheEntry>.Filter.Eq(x => x.Key, key);
        var update = Builders<CatalogCacheEntry>.Update
            .Set(x => x.Value, json)
            .Set(x => x.ExpireAt, expireAt)
            .SetOnInsert(x => x.Key, key);

        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task RemoveAsync(string key)
    {
        var filter = Builders<CatalogCacheEntry>.Filter.Eq(x => x.Key, key);
        await _collection.DeleteOneAsync(filter);
    }
}
