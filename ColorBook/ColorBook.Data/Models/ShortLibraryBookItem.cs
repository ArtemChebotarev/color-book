using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ColorBook.Data.Models;

public class ShortLibraryBookItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public string? CoverImageUrl { get; set; }
    
    public int TotalPages { get; set; }
    
    public int CompletedPages { get; set; }
    
    public DateTime? LastAccessedAt { get; set; }
}

public enum BookSortOrder
{
    LastActive,
    RecentlyAdded,
    AlphabeticalAz
}
