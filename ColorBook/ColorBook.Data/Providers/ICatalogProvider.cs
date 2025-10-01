using ColorBook.Data.Models;

namespace ColorBook.Data.Providers;

public interface ICatalogProvider
{
    Task<List<CatalogBookItem>> SearchAsync(
        string query,
        string category,
        int page,
        int pageSize);

    Task<List<ShortCatalogBookItem>> SearchShortAsync(
        string query,
        string category,
        int page,
        int pageSize);

    Task<CatalogBookItem?> GetDetailsAsync(string asin);
    
    Task<List<CatalogBookItem>> SearchCollectionAsync(
        CollectionType collectionType,
        int page,
        int pageSize);
}