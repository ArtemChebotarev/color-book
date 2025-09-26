using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ColorBook.Data.Models;

public class PageDetails
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public int PageNumber { get; set; }
    
    public PageStatus Status { get; set; } = PageStatus.NotStarted;
    
    public DateTime? CompletedAt { get; set; }
}

public enum PageStatus
{
    NotStarted,
    InProgress,
    Completed
}
