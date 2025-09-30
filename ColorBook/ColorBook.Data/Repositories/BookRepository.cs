using ColorBook.Data.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ColorBook.Data.Repositories;

public interface IBookRepository
{
    Task<List<LibraryBookItem>> GetUserBooksAsync(string userId);
    Task<List<ShortLibraryBookItem>> GetUserBooksShortAsync(string userId, BookSortOrder sortOrder = BookSortOrder.LastActive);
    Task<LibraryBookItem?> GetBookByIdAsync(string bookId, string userId);
    Task<DetailedLibraryBookItem?> GetBookDetailsByIdAsync(string bookId, string userId);
    Task<LibraryBookItem> CreateBookAsync(LibraryBookItem libraryBook);
    Task<LibraryBookItem?> UpdateBookAsync(LibraryBookItem libraryBook);
    Task<bool> DeleteBookAsync(string bookId, string userId);
    Task<bool> UpdatePageStatusAsync(string bookId, string userId, int pageNumber, PageStatus status);
}

public class BookRepository : IBookRepository
{
    private readonly IMongoContext _context;

    public BookRepository(IMongoContext context)
    {
        _context = context;
    }

    public async Task<List<LibraryBookItem>> GetUserBooksAsync(string userId)
    {
        var filter = Builders<LibraryBookItem>.Filter.Eq(b => b.UserId, userId);
        var books = await _context.Books
            .Find(filter)
            .SortByDescending(b => b.LastAccessedAt)
            .ThenByDescending(b => b.PurchasedAt)
            .ToListAsync();

        return books;
    }

    public async Task<List<ShortLibraryBookItem>> GetUserBooksShortAsync(string userId, BookSortOrder sortOrder = BookSortOrder.LastActive)
    {
        var filter = Builders<LibraryBookItem>.Filter.Eq(b => b.UserId, userId);
        
        // Build sort definition based on sortOrder
        SortDefinition<LibraryBookItem> sort = sortOrder switch
        {
            BookSortOrder.LastActive => Builders<LibraryBookItem>.Sort.Descending(b => b.LastAccessedAt).Descending(b => b.PurchasedAt),
            BookSortOrder.RecentlyAdded => Builders<LibraryBookItem>.Sort.Descending(b => b.PurchasedAt),
            BookSortOrder.AlphabeticalAz => Builders<LibraryBookItem>.Sort.Ascending(b => b.Title),
            _ => Builders<LibraryBookItem>.Sort.Descending(b => b.LastAccessedAt)
        };

        var books = await _context.Books
            .Find(filter)
            .Sort(sort)
            .Project(b => new ShortLibraryBookItem
            {
                Id = b.Id,
                Title = b.Title,
                CoverImageUrl = b.CoverImageUrl,
                TotalPages = b.TotalPages,
                CompletedPages = b.Pages.Count(p => p.Status == PageStatus.Completed),
                LastAccessedAt = b.LastAccessedAt
            })
            .ToListAsync();

        return books;
    }

    public async Task<LibraryBookItem?> GetBookByIdAsync(string bookId, string userId)
    {
        var filter = Builders<LibraryBookItem>.Filter.And(
            Builders<LibraryBookItem>.Filter.Eq(b => b.Id, bookId),
            Builders<LibraryBookItem>.Filter.Eq(b => b.UserId, userId)
        );

        return await _context.Books.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<DetailedLibraryBookItem?> GetBookDetailsByIdAsync(string bookId, string userId)
    {
        var filter = Builders<LibraryBookItem>.Filter.And(
            Builders<LibraryBookItem>.Filter.Eq(b => b.Id, bookId),
            Builders<LibraryBookItem>.Filter.Eq(b => b.UserId, userId)
        );

        return await _context.Books
            .Find(filter)
            .Project(b => new DetailedLibraryBookItem
            {
                Id = b.Id,
                Asin = b.Asin,
                UserId = b.UserId,
                Title = b.Title,
                Author = b.Author,
                Illustrator = b.Illustrator,
                Publisher = b.Publisher,
                ShortDescription = b.ShortDescription,
                CoverImageUrl = b.CoverImageUrl,
                TotalPages = b.TotalPages,
                CompletedPages = b.Pages.Count(p => p.Status == PageStatus.Completed),
                PurchasedAt = b.PurchasedAt,
                ProgressPercentage = b.TotalPages > 0 ? (double)b.Pages.Count(p => p.Status == PageStatus.Completed) / b.TotalPages * 100 : 0
                // Status is now computed automatically by the model
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LibraryBookItem> CreateBookAsync(LibraryBookItem libraryBook)
    {
        libraryBook.PurchasedAt = DateTime.UtcNow;
        libraryBook.LastAccessedAt = DateTime.UtcNow;
        
        // Initialize pages if not already set
        if (libraryBook.Pages.Count == 0 && libraryBook.TotalPages > 0)
        {
            libraryBook.Pages = Enumerable.Range(1, libraryBook.TotalPages)
                .Select(pageNum => new PageDetails
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    PageNumber = pageNum, 
                    Status = PageStatus.NotStarted
                })
                .ToList();
        }

        await _context.Books.InsertOneAsync(libraryBook);
        return libraryBook;
    }

    public async Task<LibraryBookItem?> UpdateBookAsync(LibraryBookItem libraryBook)
    {
        var filter = Builders<LibraryBookItem>.Filter.And(
            Builders<LibraryBookItem>.Filter.Eq(b => b.Id, libraryBook.Id),
            Builders<LibraryBookItem>.Filter.Eq(b => b.UserId, libraryBook.UserId)
        );

        libraryBook.LastAccessedAt = DateTime.UtcNow;
        
        var result = await _context.Books.ReplaceOneAsync(filter, libraryBook);
        return result.MatchedCount > 0 ? libraryBook : null;
    }

    public async Task<bool> DeleteBookAsync(string bookId, string userId)
    {
        var filter = Builders<LibraryBookItem>.Filter.And(
            Builders<LibraryBookItem>.Filter.Eq(b => b.Id, bookId),
            Builders<LibraryBookItem>.Filter.Eq(b => b.UserId, userId)
        );

        var result = await _context.Books.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdatePageStatusAsync(string bookId, string userId, int pageNumber, PageStatus status)
    {
        var filter = Builders<LibraryBookItem>.Filter.And(
            Builders<LibraryBookItem>.Filter.Eq(b => b.Id, bookId),
            Builders<LibraryBookItem>.Filter.Eq(b => b.UserId, userId),
            Builders<LibraryBookItem>.Filter.ElemMatch(b => b.Pages, p => p.PageNumber == pageNumber)
        );

        var completedAt = status == PageStatus.Completed ? DateTime.UtcNow : (DateTime?)null;
        var update = Builders<LibraryBookItem>.Update
            .Set("pages.$.status", status)
            .Set("pages.$.completedAt", completedAt)
            .Set(b => b.LastAccessedAt, DateTime.UtcNow);

        var result = await _context.Books.UpdateOneAsync(filter, update);
        return result.MatchedCount > 0;
    }
}
