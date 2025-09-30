using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ColorBook.Data.Models;

public class DetailedLibraryBookItem
{
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; } = string.Empty;

  public string Asin { get; set; } = string.Empty;

  public string UserId { get; set; } = string.Empty;

  public string Title { get; set; } = string.Empty;

  public string Author { get; set; } = string.Empty;

  public string Illustrator { get; set; } = string.Empty;

  public string Publisher { get; set; } = string.Empty;

  public string? ShortDescription { get; set; }

  public string? CoverImageUrl { get; set; }

  public int TotalPages { get; set; }

  public int CompletedPages { get; set; }

  public DateTime PurchasedAt { get; set; }

  public double ProgressPercentage { get; set; }

  // Computed property for Status
  public string Status => CompletedPages == 0 ? nameof(BookStatus.NotStarted) :
                          CompletedPages == TotalPages ? nameof(BookStatus.Completed) :
                          nameof(BookStatus.InProgress);
}

public enum BookStatus
{
  NotStarted,
  InProgress,
  Completed
}
