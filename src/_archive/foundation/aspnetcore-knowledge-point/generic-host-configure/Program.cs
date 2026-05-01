public class Program
{
    public static async Task Main(string[] args)
    {
        await new HostBuilder()
            .ConfigureHostConfiguration(configHost =>
            {
                // Add environment variables
                configHost.AddEnvironmentVariables();

                // Add command-line arguments
                configHost.AddCommandLine(args);

                // Add in-memory configuration for host-level
                configHost.AddInMemoryCollection(new Dictionary<string, string>
                {
                     { "Application.Name", "Host-Set-01" },
                     { "Application.Version", "Host-Set-1.0.0" }
                });
            })
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                // Add JSON file configuration
                configApp.AddJsonFile("appsettings.json", optional: true);

                // Add environment variables for app-level
                configApp.AddEnvironmentVariables();

                // Add in-memory configuration for app-level
                configApp.AddInMemoryCollection(new Dictionary<string, string>
                {
                     { "Application.Name", "App-Set-01" },
                     { "Application.Version", "App-Set-1.0.0" }
                });
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Register the hosted service
                services.AddHostedService<SampleHostedService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build()
            .RunAsync();
    }
}
public class SampleHostedService : IHostedService
{
    private readonly ILogger<SampleHostedService> _logger;
    private readonly IConfiguration _config;

    public SampleHostedService(IConfiguration config, ILogger<SampleHostedService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Testing Configuration Priority ===");

        _logger.LogInformation("Application.Name: {0}", _config["Application.Name"]);
        _logger.LogInformation("Application.Version: {0}", _config["Application.Version"]);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("End of Host");
        return Task.CompletedTask;
    }
}
