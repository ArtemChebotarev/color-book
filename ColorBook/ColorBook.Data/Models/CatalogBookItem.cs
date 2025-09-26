namespace ColorBook.Data.Models;

public class CatalogBookItem
{
    public string Asin { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<Contributor> Authors { get; set; } = [];

    public List<Contributor> Illustrators { get; set; } = [];

    public string? Publisher { get; set; }
    
    public DateTime? PublicationDate { get; set; }

    public int? TotalPages { get; set; }

    public string? ShortDescription { get; set; }

    public double? CustomerReviewStarRating { get; set; }
}

public class Contributor
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // e.g. "author", "illustrator"
}
