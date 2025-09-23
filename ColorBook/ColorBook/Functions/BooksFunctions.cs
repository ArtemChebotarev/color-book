using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ColorBook.Data;
using ColorBook.Models;
using ColorBook.Validators;
using ColorBook.Helpers;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace ColorBook.Functions;

public class BooksFunctions
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<BooksFunctions> _logger;
    private readonly IBookValidator _bookValidator;

    public BooksFunctions(
        IBookRepository bookRepository, 
        ILogger<BooksFunctions> logger,
        IBookValidator bookValidator)
    {
        _bookRepository = bookRepository;
        _logger = logger;
        _bookValidator = bookValidator;
    }
    
    [Function("GetBooks")]
    public async Task<HttpResponseData> GetBooks(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books")] HttpRequestData req,
        [FromQuery] string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
        }

        _logger.LogInformation("Getting books for user {UserId}", userId);

        try
        {
            var books = await _bookRepository.GetUserBooksAsync(userId);
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "books")] HttpRequestData req,
        [FromQuery] string userId,
        [Microsoft.Azure.Functions.Worker.Http.FromBody] BookItem book)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.Unauthorized, "User not authenticated");
        }

        _logger.LogInformation("Creating book for user {UserId}", userId);

        try
        {
            var (isValid, errorMessage) = _bookValidator.Validate(book);
            if (!isValid)
            {
                return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest, errorMessage!);
            }

            book.UserId = userId;
            book.Author ??= string.Empty;

            var createdBook = await _bookRepository.CreateBookAsync(book);
            return await HttpResponseHelper.CreateJsonResponse(req, HttpStatusCode.Created, createdBook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book for user {UserId}", userId);
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError, 
                "An error occurred while creating the book");
        }
    }
}
