using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ColorBook.Config;
using ColorBook.Models;

namespace ColorBook.Data;

public interface IMongoContext
{
    IMongoCollection<BookItem> Books { get; }
    IMongoDatabase Database { get; }
}

public class MongoContext : IMongoContext
{
    private readonly IMongoDatabase _database;

    public MongoContext(IOptions<MongoSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<BookItem> Books => _database.GetCollection<BookItem>("books");
    
    public IMongoDatabase Database => _database;
}
