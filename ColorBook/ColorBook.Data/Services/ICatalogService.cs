using ColorBook.Data.Models;

namespace ColorBook.Data.Services;

public interface ICatalogService
{
    Task<CatalogBookItem?> GetBookByAsin(string asin);
    Task<Dictionary<CollectionType, List<CatalogBookItem>>> GetAllCollectionsAsync();
    Task<Dictionary<CollectionType, List<ShortCatalogBookItem>>> GetAllCollectionsShortAsync();
    Task<List<CatalogBookItem>> GetCollectionAsync(CollectionType collectionType, int page, int pageSize);
    Task<List<ShortCatalogBookItem>> GetCollectionShortAsync(CollectionType collectionType, int page, int pageSize);
}
