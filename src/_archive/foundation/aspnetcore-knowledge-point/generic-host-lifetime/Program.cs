
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
    IHostApplicationLifetime _lifetime;
    ILogger _log;

    public SampleHostedService(IHostApplicationLifetime lifetime, ILogger<SampleHostedService> logger)
    {
        _lifetime = lifetime;
        _lifetime.ApplicationStarted.Register(OnStarted);
        _lifetime.ApplicationStopping.Register(OnStopping);
        _lifetime.ApplicationStopped.Register(OnStopped);
        _log = logger;
    }

    private void OnStopped()
    {
        _log.LogInformation("OnStopped has been called.");
    }

    private void OnStopping()
    {
        _log.LogInformation("OnStopping has been called.");
    }

    private void OnStarted()
    {
        _log.LogInformation("OnStarted has been called.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Start Sample Host");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("End Sample Host");
        return Task.CompletedTask;
    }
}

