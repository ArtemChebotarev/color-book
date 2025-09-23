using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ColorBook.Functions;

public class HealthFunctions
{
    private readonly ILogger<HealthFunctions> _logger;

    public HealthFunctions(ILogger<HealthFunctions> logger)
    {
        _logger = logger;
    }

    [Function("HealthFunction")]
    public async Task<HttpResponseData> GetHealth(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        _logger.LogInformation("Health check requested");

        var response = req.CreateResponse(HttpStatusCode.OK);
        
        var healthStatus = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "ColorBook API",
            version = "1.0.0"
        };

        await response.WriteAsJsonAsync(healthStatus);
        return response;
    }
}
