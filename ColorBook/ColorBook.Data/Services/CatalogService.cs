using ColorBook.Data.Models;
using ColorBook.Data.Providers;

namespace ColorBook.Data.Services;

public class CatalogService : ICatalogService
{
    private readonly ICatalogProvider _catalogProvider;
    private readonly ICacheProvider _cacheProvider;
    private const int CachedPagesCount = 2; // Cache first 2 pages of each collection
    private const int DefaultPageSize = 10;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public CatalogService(ICatalogProvider catalogProvider, ICacheProvider cacheProvider)
    {
        _catalogProvider = catalogProvider;
        _cacheProvider = cacheProvider;
    }

    public async Task<CatalogBookItem?> GetBookByAsin(string asin)
    {
        // First, try to get from book-specific cache
        var bookCacheKey = $"book_{asin}";
        var cachedBook = await _cacheProvider.GetAsync<CatalogBookItem>(bookCacheKey);

        if (cachedBook != null)
        {
            return cachedBook;
        }

        // Fallback to provider
        var book = await _catalogProvider.GetDetailsAsync(asin);

        if (book != null)
        {
            // Cache the book for future lookups
            await _cacheProvider.SetAsync(bookCacheKey, book, CacheTtl);
        }

        return book;
    }

    public async Task<Dictionary<CollectionType, List<CatalogBookItem>>> GetAllCollectionsAsync()
    {
        var collections = new Dictionary<CollectionType, List<CatalogBookItem>>();
        
        foreach (CollectionType collectionType in Enum.GetValues<CollectionType>())
        {
            var items = await GetCollectionAsync(collectionType, 1, DefaultPageSize);
            collections[collectionType] = items;
        }
        
        return collections;
    }

    public async Task<Dictionary<CollectionType, List<ShortCatalogBookItem>>> GetAllCollectionsShortAsync()
    {
        var collections = new Dictionary<CollectionType, List<ShortCatalogBookItem>>();
        
        foreach (CollectionType collectionType in Enum.GetValues<CollectionType>())
        {
            var items = await GetCollectionAsync(collectionType, 1, DefaultPageSize);
            var shortItems = items.Select(item => new ShortCatalogBookItem
            {
                Asin = item.Asin,
                CoverImageUrl = item.CoverImageUrl,
                Title = item.Title,
                PublicationDate = item.PublicationDate
            }).ToList();
            
            collections[collectionType] = shortItems;
        }
        
        return collections;
    }

    public async Task<List<CatalogBookItem>> GetCollectionAsync(
        CollectionType collectionType, 
        int page, 
        int pageSize)
    {
        // Try to get from cache first if it's within cached pages range
        if (page <= CachedPagesCount)
        {
            var cacheKey = $"collection_{collectionType}_{page}_{pageSize}";
            var cachedResult = await _cacheProvider.GetAsync<CachedCollectionResult>(cacheKey);
            
            if (cachedResult != null)
            {
                return cachedResult.Items;
            }
        }

        // Fallback to provider - now using provider's GetCollectionAsync method
        var items = await _catalogProvider.SearchCollectionAsync(collectionType, page, pageSize);
        
        // Cache the result if it's within cached pages range
        if (page <= CachedPagesCount)
        {
            await SetCollectionCacheAsync(collectionType, page, pageSize, items);
        }
        
        return items;
    }
    
    public async Task<List<ShortCatalogBookItem>> GetCollectionShortAsync(
        CollectionType collectionType, 
        int page, 
        int pageSize)
    {
        var items = await GetCollectionAsync(collectionType, page, pageSize);
        
        return items.Select(item => new ShortCatalogBookItem
        {
            Asin = item.Asin,
            CoverImageUrl = item.CoverImageUrl,
            Title = item.Title,
            PublicationDate = item.PublicationDate
        }).ToList();
    }

    public async Task RefreshCachedCollectionsAsync()
    {
        foreach (CollectionType collectionType in Enum.GetValues<CollectionType>())
        {
            for (int page = 1; page <= CachedPagesCount; page++)
            {
                // Remove existing cache entries
                var cacheKey = $"collection_{collectionType}_{page}_{DefaultPageSize}";
                await _cacheProvider.RemoveAsync(cacheKey);
                
                // Refresh cache
                await GetCollectionAsync(collectionType, page, DefaultPageSize);
            }
        }
    }

    private async Task SetCollectionCacheAsync(CollectionType collectionType, int page, int pageSize, List<CatalogBookItem> items)
    {
        var cacheKey = $"collection_{collectionType}_{page}_{pageSize}";
        var cachedResult = new CachedCollectionResult
        {
            Items = items
        };
        
        await _cacheProvider.SetAsync(cacheKey, cachedResult, CacheTtl);
        
        // Also cache individual books for GetBookByAsin performance
        foreach (var book in items)
        {
            var bookCacheKey = $"book_{book.Asin}";
            await _cacheProvider.SetAsync(bookCacheKey, book, CacheTtl);
        }
    }
}

public class CachedCollectionResult
{
    public List<CatalogBookItem> Items { get; set; } = new();
}
