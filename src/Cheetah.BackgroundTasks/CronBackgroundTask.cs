using Cronos;

namespace Cheetah.BackgroundTasks;

/// <summary>
/// Background task that runs on a cron schedule.
/// Supports standard 5-field cron expressions and optionally 6-field with seconds.
/// <para>
/// Usage:
/// <code>
/// [Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
/// public class DailyReportTask : CronBackgroundTask
/// {
///     // Every day at 08:00 UTC
///     public override string CronExpression => "0 8 * * *";
///
///     public override async Task ExecuteAsync(CancellationToken ct) { ... }
/// }
/// </code>
/// </para>
/// </summary>
public abstract class CronBackgroundTask : BackgroundTask
{
    /// <summary>
    /// Cron expression defining the execution schedule. <b>Required</b>.
    /// <list type="bullet">
    ///   <item>5-field standard: <c>"0 8 * * *"</c> — every day at 08:00</item>
    ///   <item>6-field with seconds: <c>"0 0 8 * * *"</c> — every day at 08:00:00</item>
    /// </list>
    /// </summary>
    public abstract string CronExpression { get; }

    /// <summary>
    /// Time zone used to evaluate the cron expression. Defaults to UTC.
    /// </summary>
    public virtual TimeZoneInfo TimeZone => TimeZoneInfo.Utc;

    /// <summary>
    /// Cron format: standard 5-field or includes seconds (6-field).
    /// Defaults to <see cref="CronFormat.Standard"/>.
    /// </summary>
    public virtual CronFormat Format => CronFormat.Standard;
}
