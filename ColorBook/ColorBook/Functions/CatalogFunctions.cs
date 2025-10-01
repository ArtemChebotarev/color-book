using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ColorBook.Data.Services;
using ColorBook.Data.Models;
using ColorBook.Helpers;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace ColorBook.Functions;

public class CatalogFunctions
{
    private readonly ICatalogService _catalogService;
    private readonly ILogger<CatalogFunctions> _logger;

    public CatalogFunctions(ICatalogService catalogService, ILogger<CatalogFunctions> logger)
    {
        _catalogService = catalogService;
        _logger = logger;
    }

    [Function("GetBookByAsinFromCatalog")]
    public async Task<HttpResponseData> GetBookByAsin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "catalog/books/{asin}")] HttpRequestData req,
        string asin)
    {
        _logger.LogInformation("Getting book by ASIN from catalog: {Asin}", asin);

        try
        {
            if (string.IsNullOrWhiteSpace(asin))
            {
                return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest,
                    "ASIN is required");
            }

            var book = await _catalogService.GetBookByAsin(asin);

            if (book == null)
            {
                return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.NotFound,
                    $"Book with ASIN '{asin}' not found");
            }

            return await HttpResponseHelper.CreateJsonResponse(req, HttpStatusCode.OK, book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting book by ASIN {Asin}", asin);
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "An error occurred while retrieving the book");
        }
    }

    [Function("GetCollections")]
    public async Task<HttpResponseData> GetCollections(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "catalog/collections")] HttpRequestData req)
    {
        _logger.LogInformation("Getting all catalog collections");

        try
        {
            var collections = await _catalogService.GetAllCollectionsShortAsync();

            var response = collections.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            );

            return await HttpResponseHelper.CreateJsonResponse(req, HttpStatusCode.OK, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting catalog collections");
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "An error occurred while retrieving catalog collections");
        }
    }

    [Function("GetCollection")]
    public async Task<HttpResponseData> GetCollection(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "catalog/collections/{collectionType}")] HttpRequestData req,
        string collectionType)
    {
        _logger.LogInformation("Getting collection: {CollectionType}", collectionType);

        try
        {
            if (!Enum.TryParse<CollectionType>(collectionType, true, out var collection))
            {
                return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest,
                    $"Invalid collection type: {collectionType}. Valid values are: {string.Join(", ", Enum.GetNames<CollectionType>())}");
            }

            var pageStr = req.Query["page"] ?? "1";
            var pageSizeStr = req.Query["pageSize"] ?? "10";

            if (!int.TryParse(pageStr, out var page) || page < 1)
                page = 1;

            if (!int.TryParse(pageSizeStr, out var pageSize) || pageSize < 1 || pageSize > 100)
                pageSize = 10;

            var items = await _catalogService.GetCollectionShortAsync(collection, page, pageSize);

            var response = new
            {
                CollectionType = collection.ToString(),
                Items = items,
                Page = page,
                PageSize = pageSize
            };

            return await HttpResponseHelper.CreateJsonResponse(req, HttpStatusCode.OK, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting collection {CollectionType}", collectionType);
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "An error occurred while retrieving the collection");
        }
    }
}
