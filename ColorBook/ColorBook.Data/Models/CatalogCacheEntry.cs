using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ColorBook.Data.Models;

public class CatalogCacheEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    [BsonElement("key")]
    public string Key { get; set; } = string.Empty;
    
    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;
    
    [BsonElement("expireAt")]
    public DateTime ExpireAt { get; set; }
}
