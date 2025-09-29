using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ColorBook.Data.Services;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace ColorBook.Functions;

public class WarmupFunctions
{
    private readonly ICatalogService _catalogService;
    private readonly ILogger<WarmupFunctions> _logger;

    public WarmupFunctions(ICatalogService catalogService, ILogger<WarmupFunctions> logger)
    {
        _catalogService = catalogService;
        _logger = logger;
    }

    [Function("WarmupCatalogCollections")]
    public async Task WarmupCatalogCollections(
        [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timerInfo)
    {
        _logger.LogInformation("Starting catalog collections warmup at {Time}", DateTime.UtcNow);

        try
        {
            if (_catalogService is CatalogService catalogServiceImpl)
            {
                await catalogServiceImpl.RefreshCachedCollectionsAsync();
                _logger.LogInformation("Successfully refreshed cached collections");
            }
            else
            {
                _logger.LogWarning("CatalogService does not support cache refresh");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during catalog collections warmup");
        }

        _logger.LogInformation("Completed catalog collections warmup at {Time}", DateTime.UtcNow);
    }

    [Function("ManualWarmupCatalogCollections")]
    public async Task<HttpResponseData> ManualWarmupCatalogCollections(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "warmup")] HttpRequestData req)
    {
        _logger.LogInformation("Starting manual catalog collections warmup at {Time}", DateTime.UtcNow);

        try
        {
            if (_catalogService is CatalogService catalogServiceImpl)
            {
                await catalogServiceImpl.RefreshCachedCollectionsAsync();
                _logger.LogInformation("Successfully refreshed cached collections");
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new
                {
                    Message = "Catalog collections warmup completed successfully",
                    Timestamp = DateTime.UtcNow
                });
                return response;
            }
            else
            {
                _logger.LogWarning("CatalogService does not support cache refresh");
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new
                {
                    Error = "CatalogService does not support cache refresh"
                });
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during catalog collections warmup");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new
            {
                Error = "An error occurred during catalog collections warmup"
            });
            return response;
        }
    }
}