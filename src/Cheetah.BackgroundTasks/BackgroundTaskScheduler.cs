using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cheetah.BackgroundTasks;

/// <summary>
/// Hosted service that discovers all registered <see cref="IBackgroundTask"/> instances
/// and runs each one concurrently according to its schedule.
/// Registered automatically by <see cref="CrmBackgroundTasksModule"/>.
/// </summary>
public sealed class BackgroundTaskScheduler : BackgroundService
{
    private readonly IEnumerable<IBackgroundTask> _tasks;
    private readonly ILogger<BackgroundTaskScheduler> _logger;

    public BackgroundTaskScheduler(
        IEnumerable<IBackgroundTask> tasks,
        ILogger<BackgroundTaskScheduler> logger)
    {
        _tasks = tasks;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var runningTasks = _tasks
            .Select(task => RunAsync(task, stoppingToken))
            .ToList();

        return Task.WhenAll(runningTasks);
    }

    private Task RunAsync(IBackgroundTask task, CancellationToken ct) => task switch
    {
        PeriodicBackgroundTask periodic => RunPeriodicAsync(periodic, ct),
        CronBackgroundTask cron         => RunCronAsync(cron, ct),
        _                               => RunOnceAsync(task, ct),
    };

    // ── Periodic ──────────────────────────────────────────────────────────────

    private async Task RunPeriodicAsync(PeriodicBackgroundTask task, CancellationToken ct)
    {
        if (task.InitialDelay > TimeSpan.Zero)
        {
            _logger.LogDebug("Background task {Task}: waiting {Delay} before first run",
                task.Name, task.InitialDelay);
            await Task.Delay(task.InitialDelay, ct);
        }

        using var timer = new PeriodicTimer(task.Period);

        while (await timer.WaitForNextTickAsync(ct))
            await ExecuteSafelyAsync(task, ct);
    }

    // ── Cron ──────────────────────────────────────────────────────────────────

    private async Task RunCronAsync(CronBackgroundTask task, CancellationToken ct)
    {
        CronExpression expression;
        try
        {
            expression = CronExpression.Parse(task.CronExpression, task.Format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Background task {Task}: invalid cron expression '{Expression}' — task will not run",
                task.Name, task.CronExpression);
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            var next = expression.GetNextOccurrence(DateTimeOffset.UtcNow, task.TimeZone);
            if (next is null)
            {
                _logger.LogWarning("Background task {Task}: cron expression '{Expression}' has no future occurrences",
                    task.Name, task.CronExpression);
                break;
            }

            var delay = next.Value - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                _logger.LogDebug("Background task {Task}: next run at {Next} (in {Delay:g})",
                    task.Name, next.Value, delay);

                try { await Task.Delay(delay, ct); }
                catch (OperationCanceledException) { break; }
            }

            await ExecuteSafelyAsync(task, ct);
        }
    }

    // ── One-shot (fallback for custom IBackgroundTask with no schedule) ────────

    private async Task RunOnceAsync(IBackgroundTask task, CancellationToken ct)
    {
        _logger.LogDebug("Background task {Task}: running once (no schedule defined)", task.Name);
        await ExecuteSafelyAsync(task, ct);
    }

    // ── Shared execution ──────────────────────────────────────────────────────

    private async Task ExecuteSafelyAsync(IBackgroundTask task, CancellationToken ct)
    {
        _logger.LogDebug("Executing background task {Task}", task.Name);
        var started = DateTimeOffset.UtcNow;
        try
        {
            await task.ExecuteAsync(ct);
            _logger.LogDebug("Background task {Task} completed in {Elapsed:g}",
                task.Name, DateTimeOffset.UtcNow - started);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background task {Task} failed after {Elapsed:g}",
                task.Name, DateTimeOffset.UtcNow - started);
        }
    }
}
