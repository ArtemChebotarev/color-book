using ColorBook.Data.Models;

namespace ColorBook.Data.Services;

public interface ICatalogService
{
    Task<(List<CatalogBookItem> Items, int TotalCount)> SearchAsync(string query, string category, int page, int pageSize);
    Task<Dictionary<CollectionType, List<CatalogBookItem>>> GetAllCollectionsAsync();
    Task<(List<CatalogBookItem> Items, int TotalCount)> GetCollectionAsync(CollectionType collectionType, int page, int pageSize);
}
