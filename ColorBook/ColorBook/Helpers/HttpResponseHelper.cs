using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace ColorBook.Helpers;

public static class HttpResponseHelper
{
    public static async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData req, 
        HttpStatusCode statusCode, 
        string message)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteStringAsync(message);
        return response;
    }

    public static async Task<HttpResponseData> CreateJsonResponse<T>(
        HttpRequestData req, 
        HttpStatusCode statusCode, 
        T data)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(data);
        return response;
    }
}

