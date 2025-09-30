using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ColorBook.Data.Models;
using ColorBook.Validators;
using ColorBook.Helpers;
using System.Net;
using ColorBook.Data.Repositories;
using ColorBook.Models.Library;

namespace ColorBook.Functions;

public class LibraryFunctions
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<LibraryFunctions> _logger;
    private readonly IBookValidator _bookValidator;

    public LibraryFunctions(
        IBookRepository bookRepository, 
        ILogger<LibraryFunctions> logger,
        IBookValidator bookValidator)
    {
        _bookRepository = bookRepository;
        _logger = logger;
        _bookValidator = bookValidator;
    }
    
    [Function("GetBooks")]
    public async Task<HttpResponseData> GetBooks(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "library/books")] HttpRequestData req)
    {
        var userId = req.Query["userId"];
        var sortOrder = req.Query["sort"];
        
        if (string.IsNullOrEmpty(userId))
        {
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
        }

        _logger.LogInformation("Getting books for user {UserId}", userId);

        try
        {
            var parsedSortOrder = Enum.TryParse<BookSortOrder>(sortOrder, true, out var sort) 
                ? sort 
                : BookSortOrder.LastActive;
                
            var books = await _bookRepository.GetUserBooksShortAsync(userId, parsedSortOrder);
            return await HttpResponseHelper.CreateJsonResponse(req, HttpStatusCode.OK, books);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting books for user {UserId}", userId);
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError, 
                "An error occurred while retrieving books");
        }
    }

    [Function("CreateBook")]
    public async Task<HttpResponseData> CreateBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "library/books")] HttpRequestData req,
        [FromBody] LibraryBookItemRequest libraryBookRequest)
    {
        var userId = req.Query["userId"];
        
        if (string.IsNullOrEmpty(userId))
        {
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
        }

        _logger.LogInformation("Creating book for user {UserId}", userId);

        try
        {
            var libraryBook = libraryBookRequest.ToLibraryBookItem(userId);
            
            var (isValid, errorMessage) = _bookValidator.Validate(libraryBook);
            if (!isValid)
            {
                return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest, errorMessage!);
            }

            var createdBook = await _bookRepository.CreateBookAsync(libraryBook);
            return await HttpResponseHelper.CreateJsonResponse(req, HttpStatusCode.Created, createdBook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book for user {UserId}", userId);
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError, 
                "An error occurred while creating the book");
        }
    }

    [Function("GetBook")]
    public async Task<HttpResponseData> GetBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "library/books/{bookId}")] HttpRequestData req,
        string bookId)
    {
        var userId = req.Query["userId"];
        
        if (string.IsNullOrEmpty(userId))
        {
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
        }

        if (string.IsNullOrEmpty(bookId))
        {
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest, "Book ID is required");
        }

        _logger.LogInformation("Getting book {BookId} for user {UserId}", bookId, userId);

        try
        {
            var book = await _bookRepository.GetBookDetailsByIdAsync(bookId, userId);
            
            if (book == null)
            {
                return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.NotFound, "Book not found");
            }

            return await HttpResponseHelper.CreateJsonResponse(req, HttpStatusCode.OK, book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book {BookId} for user {UserId}", bookId, userId);
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError, 
                "An error occurred while retrieving the book");
        }
    }
}
