using ColorBook.Data.Models;

namespace ColorBook.Data.Services;

public interface ICatalogService
{
    Task<List<ShortCatalogBookItem>> SearchAsync(string query, string category, int page, int pageSize);
    Task<CatalogBookItem?> GetBookByAsin(string asin);
    Task<Dictionary<CollectionType, List<CatalogBookItem>>> GetAllCollectionsAsync();
    Task<List<CatalogBookItem>> GetCollectionAsync(CollectionType collectionType, int page, int pageSize);
}
