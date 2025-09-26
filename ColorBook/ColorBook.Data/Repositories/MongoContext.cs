using ColorBook.Data.Config;
using ColorBook.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ColorBook.Data.Repositories;

public interface IMongoContext
{
    IMongoCollection<LibraryBookItem> Books { get; }
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

    public IMongoCollection<LibraryBookItem> Books => _database.GetCollection<LibraryBookItem>("books");
    
    public IMongoDatabase Database => _database;
}
