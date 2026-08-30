using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using Quartz;
using QuartzTimerMonitor;

namespace QuartzTimerMonitor.Jobs;

[DisallowConcurrentExecution]
public class NamedTimerJob : IJob
{
    public const string TimerNameDataKey = "timerName";

    private readonly ILogger<NamedTimerJob> _logger;
    private readonly IHubContext<TimerHub> _hubContext;

    public NamedTimerJob(ILogger<NamedTimerJob> logger, IHubContext<TimerHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var timerName = context.MergedJobDataMap.GetString(TimerNameDataKey) ?? "demo-timer";
        var cancellationToken = context.CancellationToken;
        var stopwatch = Stopwatch.StartNew();
        string? errorMessage = null;
        var startedAtUtc = DateTimeOffset.UtcNow;

        try
        {
            await _hubContext.Clients.All.SendAsync("TimerRoundStarted", new
            {
                timerName,
                startedAtUtc
            }, cancellationToken);

            _logger.LogInformation("Timer {TimerName} round started.", timerName);

            const int totalSteps = 10;
            const int delayPerStepMs = 300;
            for (var step = 1; step <= totalSteps; step++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(delayPerStepMs), cancellationToken);
            }

            stopwatch.Stop();

            await _hubContext.Clients.All.SendAsync("TimerRoundCompleted", new
            {
                timerName,
                completedAtUtc = DateTimeOffset.UtcNow,
                error = (string?)null
            }, cancellationToken);

            _logger.LogInformation(
                "Timer {TimerName} round finished. DurationMs={DurationMs:F2}",
                timerName,
                stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            stopwatch.Stop();
            await _hubContext.Clients.All.SendAsync("TimerRoundCompleted", new
            {
                timerName,
                completedAtUtc = DateTimeOffset.UtcNow,
                error = errorMessage
            }, CancellationToken.None);
            _logger.LogError(ex, "Timer {TimerName} round failed.", timerName);
            throw;
        }
    }
}
