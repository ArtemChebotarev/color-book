using ColorBook.Data.Config;
using ColorBook.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ColorBook.Data.Repositories;

public interface IMongoContext
{
    IMongoCollection<LibraryBookItem> Books { get; }
    IMongoCollection<CatalogCacheEntry> CatalogCache { get; }
    IMongoDatabase Database { get; }
    Task EnsureIndexesAsync();
}

public class MongoContext : IMongoContext
{
    private readonly IMongoDatabase _database;
    private bool _indexesEnsured;

    public MongoContext(IOptions<MongoSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<LibraryBookItem> Books 
        => _database.GetCollection<LibraryBookItem>("books");

    public IMongoCollection<CatalogCacheEntry> CatalogCache
        => _database.GetCollection<CatalogCacheEntry>("CatalogCache");

    public IMongoDatabase Database => _database;

    public async Task EnsureIndexesAsync()
    {
        if (_indexesEnsured) return;

        // TTL index for CatalogCache
        var cacheKeys = Builders<CatalogCacheEntry>.IndexKeys.Ascending(x => x.ExpireAt);
        var cacheOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
        var cacheModel = new CreateIndexModel<CatalogCacheEntry>(cacheKeys, cacheOptions);

        var keyIndex = Builders<CatalogCacheEntry>.IndexKeys.Ascending(x => x.Key);
        var keyOptions = new CreateIndexOptions { Unique = true };
        var keyModel = new CreateIndexModel<CatalogCacheEntry>(keyIndex, keyOptions);

        await CatalogCache.Indexes.CreateManyAsync(new[] { cacheModel, keyModel });

        _indexesEnsured = true;
    }
}


