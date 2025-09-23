using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ColorBook.Middleware;

public class LoggingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var functionName = context.FunctionDefinition.Name;
        var invocationId = context.InvocationId;

        _logger.LogInformation("Function {FunctionName} started. InvocationId: {InvocationId}", 
            functionName, invocationId);

        try
        {
            await next(context);
            
            stopwatch.Stop();
            _logger.LogInformation("Function {FunctionName} completed successfully in {ElapsedMs}ms. InvocationId: {InvocationId}", 
                functionName, stopwatch.ElapsedMilliseconds, invocationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Function {FunctionName} failed after {ElapsedMs}ms. InvocationId: {InvocationId}", 
                functionName, stopwatch.ElapsedMilliseconds, invocationId);
            throw;
        }
    }
}
