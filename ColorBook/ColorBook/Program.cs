using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ColorBook.Config;
using ColorBook.Data;
using ColorBook.Middleware;
using ColorBook.Validators;

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

        // Register custom services
        services.AddSingleton<IMongoContext, MongoContext>();
        services.AddScoped<IBookRepository, BookRepository>();

        // Register validators
        services.AddScoped<IBookValidator, BookValidator>();
    })
    .Build();

host.Run();