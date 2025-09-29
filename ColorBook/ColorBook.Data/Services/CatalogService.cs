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

    public async Task<List<ShortCatalogBookItem>> SearchAsync(
        string query, 
        string category, 
        int page, 
        int pageSize)
    {
        // Use the new SearchShortAsync method that requests fewer fields from the data source
        var items = await _catalogProvider.SearchShortAsync(query, category, page, pageSize);
        
        // Return the items directly without any filtering or conversion
        return items;
    }

    public async Task<CatalogBookItem?> GetBookByAsin(string asin)
    {
        return await _catalogProvider.GetDetailsAsync(asin);
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

        // Fallback to provider
        var result = await GetCollectionFromProviderAsync(collectionType, page, pageSize);
        
        // Cache the result if it's within cached pages range
        if (page <= CachedPagesCount)
        {
            var cacheKey = $"collection_{collectionType}_{page}_{pageSize}";
            var cachedResult = new CachedCollectionResult
            {
                Items = result.Items
            };
            await _cacheProvider.SetAsync(cacheKey, cachedResult, CacheTtl);
        }
        
        return result.Items;
    }

    private async Task<(List<CatalogBookItem> Items, int TotalCount)> GetCollectionFromProviderAsync(
        CollectionType collectionType, 
        int page, 
        int pageSize)
    {
        // TODO This filtration has to be on the Provider's Side.
        // I can't pull all books from amazon. Those should be either 3 different requests.
        var allBooks = await _catalogProvider.SearchAsync("", "", 1, int.MaxValue);
        
        var sortedBooks = collectionType switch
        {
            CollectionType.BestRated => allBooks
                .Where(b => b.CustomerReviewStarRating.HasValue)
                .OrderByDescending(b => b.CustomerReviewStarRating)
                .ThenByDescending(b => b.PublicationDate)
                .ToList(),
            
            CollectionType.NewlyReleased => allBooks
                .Where(b => b.PublicationDate.HasValue)
                .OrderByDescending(b => b.PublicationDate)
                .ThenByDescending(b => b.CustomerReviewStarRating ?? 0)
                .ToList(),
            
            CollectionType.MostColored => allBooks
                .Where(b => b.TotalPages.HasValue)
                .OrderByDescending(b => b.TotalPages)
                .ThenByDescending(b => b.CustomerReviewStarRating ?? 0)
                .ToList(),
            
            _ => allBooks.OrderBy(b => b.Title).ToList()
        };

        var collectionItems = sortedBooks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var collectionTotalCount = sortedBooks.Count;
        
        return (collectionItems, collectionTotalCount);
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
}

public class CachedCollectionResult
{
    public List<CatalogBookItem> Items { get; set; } = new();
}
