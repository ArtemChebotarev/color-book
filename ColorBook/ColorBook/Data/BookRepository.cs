using MongoDB.Driver;
using ColorBook.Models;
using MongoDB.Bson;

namespace ColorBook.Data;

public interface IBookRepository
{
    Task<List<BookItem>> GetUserBooksAsync(string userId);
    Task<BookItem?> GetBookByIdAsync(string bookId, string userId);
    Task<BookItem> CreateBookAsync(BookItem book);
    Task<BookItem?> UpdateBookAsync(BookItem book);
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

    public async Task<List<BookItem>> GetUserBooksAsync(string userId)
    {
        var filter = Builders<BookItem>.Filter.Eq(b => b.UserId, userId);
        var books = await _context.Books
            .Find(filter)
            .SortByDescending(b => b.LastAccessedAt)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();

        return books;
    }

    public async Task<BookItem?> GetBookByIdAsync(string bookId, string userId)
    {
        var filter = Builders<BookItem>.Filter.And(
            Builders<BookItem>.Filter.Eq(b => b.Id, bookId),
            Builders<BookItem>.Filter.Eq(b => b.UserId, userId)
        );

        return await _context.Books.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<BookItem> CreateBookAsync(BookItem book)
    {
        book.CreatedAt = DateTime.UtcNow;
        book.LastAccessedAt = DateTime.UtcNow;
        
        // Initialize pages if not already set
        if (book.Pages.Count == 0 && book.TotalPages > 0)
        {
            book.Pages = Enumerable.Range(1, book.TotalPages)
                .Select(pageNum => new PageDetails
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    PageNumber = pageNum, 
                    Status = PageStatus.NotStarted
                })
                .ToList();
        }

        await _context.Books.InsertOneAsync(book);
        return book;
    }

    public async Task<BookItem?> UpdateBookAsync(BookItem book)
    {
        var filter = Builders<BookItem>.Filter.And(
            Builders<BookItem>.Filter.Eq(b => b.Id, book.Id),
            Builders<BookItem>.Filter.Eq(b => b.UserId, book.UserId)
        );

        book.LastAccessedAt = DateTime.UtcNow;
        
        var result = await _context.Books.ReplaceOneAsync(filter, book);
        return result.MatchedCount > 0 ? book : null;
    }

    public async Task<bool> DeleteBookAsync(string bookId, string userId)
    {
        var filter = Builders<BookItem>.Filter.And(
            Builders<BookItem>.Filter.Eq(b => b.Id, bookId),
            Builders<BookItem>.Filter.Eq(b => b.UserId, userId)
        );

        var result = await _context.Books.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdatePageStatusAsync(string bookId, string userId, int pageNumber, PageStatus status)
    {
        var filter = Builders<BookItem>.Filter.And(
            Builders<BookItem>.Filter.Eq(b => b.Id, bookId),
            Builders<BookItem>.Filter.Eq(b => b.UserId, userId),
            Builders<BookItem>.Filter.ElemMatch(b => b.Pages, p => p.PageNumber == pageNumber)
        );

        var completedAt = status == PageStatus.Completed ? DateTime.UtcNow : (DateTime?)null;
        var update = Builders<BookItem>.Update
            .Set("pages.$.status", status)
            .Set("pages.$.completedAt", completedAt)
            .Set(b => b.LastAccessedAt, DateTime.UtcNow);

        var result = await _context.Books.UpdateOneAsync(filter, update);
        return result.MatchedCount > 0;
    }
}
