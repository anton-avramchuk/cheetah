namespace Cheetah.BackgroundTasks;

/// <summary>
/// Background task that runs repeatedly on a fixed time interval.
/// <para>
/// Usage:
/// <code>
/// [Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
/// public class CleanupTask : PeriodicBackgroundTask
/// {
///     public override TimeSpan Period => TimeSpan.FromHours(1);
///
///     public override async Task ExecuteAsync(CancellationToken ct)
///     {
///         // ...
///     }
/// }
/// </code>
/// </para>
/// </summary>
public abstract class PeriodicBackgroundTask : BackgroundTask
{
    /// <summary>
    /// How often the task runs. <b>Required</b>.
    /// </summary>
    public abstract TimeSpan Period { get; }

    /// <summary>
    /// Delay before the first execution. Defaults to <see cref="TimeSpan.Zero"/>
    /// (first tick fires immediately when the host starts).
    /// </summary>
    public virtual TimeSpan InitialDelay => TimeSpan.Zero;
}
