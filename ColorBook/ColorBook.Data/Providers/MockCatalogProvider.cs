using ColorBook.Data.Models;
using System.Text.Json;

namespace ColorBook.Data.Providers;

public class MockCatalogProvider : ICatalogProvider
{
    private readonly string _dataFilePath;
    private List<CatalogBookItem>? _cachedBooks;
    private readonly object _lock = new();

    public MockCatalogProvider()
    {
        _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "mock-catalog.json");
    }

    public async Task<List<CatalogBookItem>> SearchAsync(
        string query, 
        string category, 
        int page, 
        int pageSize)
    {
        var books = await GetBooksAsync();
        
        // Filter by query and category
        var filteredBooks = books.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(query))
        {
            filteredBooks = filteredBooks.Where(b => 
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Authors.Any(a => a.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (b.ShortDescription?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        
        if (!string.IsNullOrWhiteSpace(category) && category != "all")
        {
            // For mock purposes, we'll categorize based on title keywords
            filteredBooks = filteredBooks.Where(b => 
                b.Title.Contains(category, StringComparison.OrdinalIgnoreCase));
        }
        
        var pagedBooks = filteredBooks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
            
        return pagedBooks;
    }

    public async Task<List<ShortCatalogBookItem>> SearchShortAsync(
        string query, 
        string category, 
        int page, 
        int pageSize)
    {
        var books = await GetBooksAsync();
        
        // Filter by query and category
        var filteredBooks = books.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(query))
        {
            filteredBooks = filteredBooks.Where(b => 
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Authors.Any(a => a.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (b.ShortDescription?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        
        if (!string.IsNullOrWhiteSpace(category) && category != "all")
        {
            // For mock purposes, we'll categorize based on title keywords
            filteredBooks = filteredBooks.Where(b => 
                b.Title.Contains(category, StringComparison.OrdinalIgnoreCase));
        }
        
        var pagedBooks = filteredBooks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(book => new ShortCatalogBookItem
            {
                Asin = book.Asin,
                CoverImageUrl = book.CoverImageUrl,
                Title = book.Title,
                PublicationDate = book.PublicationDate
            })
            .ToList();
            
        return pagedBooks;
    }

    public async Task<CatalogBookItem?> GetDetailsAsync(string asin)
    {
        var books = await GetBooksAsync();
        return books.FirstOrDefault(b => b.Asin == asin);
    }

    // ToDo when integtading with Amazon: call SeacrhAsync with category "best-rated", "newly-released", "most-colored"
    public async Task<List<CatalogBookItem>> SearchCollectionAsync(
        CollectionType collectionType,
        int page,
        int pageSize)
    {
        var books = await GetBooksAsync();
        
        var sortedBooks = collectionType switch
        {
            CollectionType.BestRated => books
                .Where(b => b.CustomerReviewStarRating.HasValue)
                .OrderByDescending(b => b.CustomerReviewStarRating)
                .ThenByDescending(b => b.PublicationDate)
                .ToList(),
            
            CollectionType.NewlyReleased => books
                .Where(b => b.PublicationDate.HasValue)
                .OrderByDescending(b => b.PublicationDate)
                .ThenByDescending(b => b.CustomerReviewStarRating ?? 0)
                .ToList(),
            
            CollectionType.MostColored => books
                .Where(b => b.TotalPages.HasValue)
                .OrderByDescending(b => b.TotalPages)
                .ThenByDescending(b => b.CustomerReviewStarRating ?? 0)
                .ToList(),
            
            _ => books.OrderBy(b => b.Title).ToList()
        };

        return sortedBooks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    private async Task<List<CatalogBookItem>> GetBooksAsync()
    {
        if (_cachedBooks != null)
            return _cachedBooks;

        lock (_lock)
        {
            if (_cachedBooks != null)
                return _cachedBooks;

            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    _cachedBooks = JsonSerializer.Deserialize<List<CatalogBookItem>>(json) ?? new List<CatalogBookItem>();
                }
                else
                {
                    _cachedBooks = new List<CatalogBookItem>();
                }
            }
            catch
            {
                _cachedBooks = new List<CatalogBookItem>();
            }
        }

        return _cachedBooks;
    }
}
