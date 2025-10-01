using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ColorBook.Data.Services;
using ColorBook.Helpers;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace ColorBook.Functions;

public class SearchFunctions
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchFunctions> _logger;

    public SearchFunctions(ISearchService searchService, ILogger<SearchFunctions> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [Function("SearchCatalog")]
    public async Task<HttpResponseData> SearchCatalog(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search")] HttpRequestData req)
    {
        _logger.LogInformation("Searching catalog");

        try
        {
            var query = req.Query["query"] ?? "";
            var category = req.Query["category"] ?? "";
            var pageStr = req.Query["page"] ?? "1";
            var pageSizeStr = req.Query["pageSize"] ?? "10";

            if (!int.TryParse(pageStr, out var page) || page < 1)
                page = 1;

            if (!int.TryParse(pageSizeStr, out var pageSize) || pageSize < 1 || pageSize > 100)
                pageSize = 10;

            var items = await _searchService.SearchAsync(query, category, page, pageSize);

            var response = new
            {
                Items = items,
                Page = page,
                PageSize = pageSize
            };

            return await HttpResponseHelper.CreateJsonResponse(req, HttpStatusCode.OK, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching catalog");
            return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "An error occurred while searching the catalog");
        }
    }

    [Function("GetBookByAsin")]
    public async Task<HttpResponseData> GetBookByAsin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "search/books/{asin}")] HttpRequestData req,
        string asin)
    {
        _logger.LogInformation("Getting book by ASIN: {Asin}", asin);

        try
        {
            if (string.IsNullOrWhiteSpace(asin))
            {
                return await HttpResponseHelper.CreateErrorResponse(req, HttpStatusCode.BadRequest,
                    "ASIN is required");
            }

            var book = await _searchService.GetBookByAsin(asin);

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
}
