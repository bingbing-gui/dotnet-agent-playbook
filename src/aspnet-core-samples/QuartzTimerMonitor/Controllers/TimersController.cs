using Microsoft.AspNetCore.Mvc;
using Quartz;
using QuartzTimerMonitor.Models;

namespace QuartzTimerMonitor.Controllers;

[ApiController]
[Route("api/timers")]
public class TimersController(ISchedulerFactory schedulerFactory) : ControllerBase
{
    private const string TimerName = "demo-timer";
    private static readonly JobKey DemoJobKey = new("demo-timer-job");
    private static readonly TriggerKey DemoTriggerKey = new("demo-timer-trigger");

    [HttpPost("{name}/start")]
    public async Task<ActionResult<TimerStatusDto>> Start(string name, CancellationToken cancellationToken)
    {
        if (!IsDemoTimer(name))
        {
            return NotFound(new { message = $"Unknown timer: {name}" });
        }

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.ResumeTrigger(DemoTriggerKey, cancellationToken);
        return Ok(await ReadStatusAsync(cancellationToken));
    }

    [HttpPost("{name}/stop")]
    public async Task<ActionResult<TimerStatusDto>> Stop(string name, CancellationToken cancellationToken)
    {
        if (!IsDemoTimer(name))
        {
            return NotFound(new { message = $"Unknown timer: {name}" });
        }

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        await scheduler.PauseTrigger(DemoTriggerKey, cancellationToken);
        return Ok(await ReadStatusAsync(cancellationToken));
    }

    [HttpGet("{name}/status")]
    public async Task<ActionResult<TimerStatusDto>> Status(string name, CancellationToken cancellationToken)
    {
        if (!IsDemoTimer(name))
        {
            return NotFound(new { message = $"Unknown timer: {name}" });
        }

        return Ok(await ReadStatusAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TimerStatusDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(new[] { await ReadStatusAsync(cancellationToken) });
    }

    private async Task<TimerStatusDto> ReadStatusAsync(CancellationToken cancellationToken)
    {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var trigger = await scheduler.GetTrigger(DemoTriggerKey, cancellationToken);
        var triggerState = await scheduler.GetTriggerState(DemoTriggerKey, cancellationToken);
        var executingJobs = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);

        var isExecuting = executingJobs.Any(x => x.JobDetail.Key.Equals(DemoJobKey));
        var isRunning = triggerState is not TriggerState.Paused and not TriggerState.None;
        var intervalSeconds = trigger is ISimpleTrigger simpleTrigger
            ? (int)simpleTrigger.RepeatInterval.TotalSeconds
            : 10;

        var lastStartedAtUtc = executingJobs
            .Where(x => x.JobDetail.Key.Equals(DemoJobKey))
            .Select(x => (DateTimeOffset?)x.FireTimeUtc)
            .OrderByDescending(x => x)
            .FirstOrDefault();

        return new TimerStatusDto(
            TimerName,
            trigger is not null,
            isRunning,
            isExecuting,
            intervalSeconds,
            0,
            null,
            null,
            null,
            lastStartedAtUtc,
            trigger?.GetPreviousFireTimeUtc(),
            null,
            triggerState.ToString(),
            trigger?.GetNextFireTimeUtc(),
            trigger?.GetPreviousFireTimeUtc(),
            scheduler.IsStarted,
            scheduler.InStandbyMode);
    }

    private static bool IsDemoTimer(string name)
    {
        return string.Equals(name, TimerName, StringComparison.OrdinalIgnoreCase);
    }
}
