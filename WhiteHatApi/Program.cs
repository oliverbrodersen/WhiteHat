using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();         
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        // In ConfigureServices
        services.AddStackExchangeRedisCache(options => {
            options.Configuration = Environment.GetEnvironmentVariable("RedisConnectionString");
            options.InstanceName = "WhiteHatProxy_";
        });
    })
    .Build();

host.Run();
