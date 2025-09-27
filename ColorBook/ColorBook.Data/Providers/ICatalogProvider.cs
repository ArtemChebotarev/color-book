using ColorBook.Data.Models;

namespace ColorBook.Data.Providers;

public interface ICatalogProvider
{
    Task<(List<CatalogBookItem> Items, int TotalCount)> SearchAsync(
        string query,
        string category,
        int page,
        int pageSize);

    Task<CatalogBookItem?> GetDetailsAsync(string asin);
}