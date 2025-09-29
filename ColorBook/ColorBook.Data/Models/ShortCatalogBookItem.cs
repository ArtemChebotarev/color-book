namespace ColorBook.Data.Models;

public class ShortCatalogBookItem
{
    public string Asin { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? PublicationDate { get; set; }
}
