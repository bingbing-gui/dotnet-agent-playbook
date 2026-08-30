namespace QuartzTimerMonitor.Models;

public sealed record TimerStatusDto(
    string Name,
    bool IsRegistered,
    bool IsRunning,
    bool IsExecuting,
    int IntervalSeconds,
    long TotalRounds,
    double? LastRoundDurationMs,
    double? CurrentRoundElapsedMs,
    double? CurrentRoundProgressPercent,
    DateTimeOffset? LastStartedAtUtc,
    DateTimeOffset? LastCompletedAtUtc,
    string? LastError,
    string? TriggerState,
    DateTimeOffset? NextFireTimeUtc,
    DateTimeOffset? PreviousFireTimeUtc,
    bool IsSchedulerStarted,
    bool InStandbyMode);
