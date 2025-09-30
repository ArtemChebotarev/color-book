using ColorBook.Data.Models;

namespace ColorBook.Models.Library;

public class LibraryBookItemRequest
{
    public string Asin { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public string? CoverImageUrl { get; set; }
    
    public string Author { get; set; } = string.Empty;
    
    public string Illustrator { get; set; } = string.Empty;
    
    public string Publisher { get; set; } = string.Empty;
    
    public string? ShortDescription { get; set; }
    
    public int TotalPages { get; set; }
    
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    
    public LibraryBookItem ToLibraryBookItem(string userId)
    {
        return new LibraryBookItem
        {
            Asin = Asin,
            UserId = userId,
            Title = Title,
            Author = Author,
            Illustrator = Illustrator,
            Publisher = Publisher,
            ShortDescription = ShortDescription,
            CoverImageUrl = CoverImageUrl,
            TotalPages = TotalPages,
            PurchasedAt = PurchasedAt,
            Pages = new List<PageDetails>()
        };
    }
}