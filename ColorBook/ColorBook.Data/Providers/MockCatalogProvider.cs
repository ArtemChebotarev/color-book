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
