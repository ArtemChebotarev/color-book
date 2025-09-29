using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ColorBook.Data.Config;
using ColorBook.Data.Repositories;
using ColorBook.Middleware;
using ColorBook.Validators;
using ColorBook.Data.Providers;
using ColorBook.Data.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(workerApplication =>
    {
        // Configure middleware pipeline
        workerApplication.UseMiddleware<LoggingMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        // Configure options
        services.Configure<MongoSettings>(
            context.Configuration.GetSection(MongoSettings.SectionName));

        // Register services
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register memory cache
        services.AddMemoryCache();

        // Register custom services
        services.AddSingleton<IMongoContext, MongoContext>();
        services.AddScoped<IBookRepository, BookRepository>();

        // Register catalog services
        services.AddScoped<ICatalogProvider, MockCatalogProvider>();

        // Register cache provider based on configuration
        var useMongoCache = context.Configuration.GetValue<bool>("UseMongoCache", false);
        if (useMongoCache)
        {
            services.AddScoped<ICacheProvider, MongoCacheProvider>();
        }
        else
        {
            services.AddScoped<ICacheProvider, InMemoryCacheProvider>();
        }
        
        services.AddScoped<ICatalogService, CatalogService>();

        // Register validators
        services.AddScoped<IBookValidator, BookValidator>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var mongoContext = scope.ServiceProvider.GetRequiredService<IMongoContext>();
    await mongoContext.EnsureIndexesAsync();
}

host.Run();