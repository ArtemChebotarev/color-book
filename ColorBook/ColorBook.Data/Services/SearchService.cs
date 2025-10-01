using ColorBook.Data.Models;
using ColorBook.Data.Providers;

namespace ColorBook.Data.Services;

public class SearchService : ISearchService
{
    private readonly ICatalogProvider _catalogProvider;

    public SearchService(ICatalogProvider catalogProvider)
    {
        _catalogProvider = catalogProvider;
    }

    public async Task<List<ShortCatalogBookItem>> SearchAsync(string query, string category, int page, int pageSize)
    {
        return await _catalogProvider.SearchShortAsync(query, category, page, pageSize);
    }

    public async Task<CatalogBookItem?> GetBookByAsin(string asin)
    {
        return await _catalogProvider.GetDetailsAsync(asin);
    }
}
