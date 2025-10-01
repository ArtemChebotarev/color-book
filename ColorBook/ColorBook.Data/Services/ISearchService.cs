using ColorBook.Data.Models;

namespace ColorBook.Data.Services;

public interface ISearchService
{
    Task<List<ShortCatalogBookItem>> SearchAsync(string query, string category, int page, int pageSize);
    Task<CatalogBookItem?> GetBookByAsin(string asin);
}
