using ColorBook.Data.Models;

namespace ColorBook.Data.Repositories;

public interface IBookRepository
{
    Task<List<LibraryBookItem>> GetUserBooksAsync(string userId);
    Task<List<ShortLibraryBookItem>> GetUserBooksShortAsync(string userId, BookSortOrder sortOrder = BookSortOrder.LastActive, int page = 1, int pageSize = 20);
    Task<LibraryBookItem?> GetBookByIdAsync(string bookId, string userId);
    Task<DetailedLibraryBookItem?> GetBookDetailsByIdAsync(string bookId, string userId);
    Task<LibraryBookItem> CreateBookAsync(LibraryBookItem libraryBook);
    Task<LibraryBookItem?> UpdateBookAsync(LibraryBookItem libraryBook);
    Task<bool> DeleteBookAsync(string bookId, string userId);
    Task<bool> UpdatePageStatusAsync(string bookId, string userId, int pageNumber, PageStatus status);
}