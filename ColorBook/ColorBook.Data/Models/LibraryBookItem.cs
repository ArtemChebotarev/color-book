using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ColorBook.Data.Models;

public class LibraryBookItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public string Author { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? CoverImageUrl { get; set; }
    
    public int TotalPages { get; set; }
    
    public List<PageDetails> Pages { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastAccessedAt { get; set; }
    
    // Computed properties
    public int CompletedPages => Pages.Count(p => p.Status == PageStatus.Completed);
    
    public double ProgressPercentage => TotalPages > 0 ? (double)CompletedPages / TotalPages * 100 : 0;
    
    public bool IsCompleted => CompletedPages == TotalPages && TotalPages > 0;
}
