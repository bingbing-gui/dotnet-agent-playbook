await Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<SampleHostedService>();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        }).Build()
        .RunAsync();

public class SampleHostedService : IHostedService
{
    ILogger _log;

    public SampleHostedService(ILogger<SampleHostedService> logger)
    {
        _log = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogDebug("Start Sample Host");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogDebug("End Sample Host");
        return Task.CompletedTask;
    }
}

