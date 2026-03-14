namespace Cheetah.BackgroundTasks;

/// <summary>
/// Base class for all background tasks. Inherit from
/// <see cref="PeriodicBackgroundTask"/> or <see cref="CronBackgroundTask"/>
/// to specify the execution schedule.
/// </summary>
public abstract class BackgroundTask : IBackgroundTask
{
    /// <inheritdoc/>
    public virtual string Name => GetType().Name;

    /// <inheritdoc/>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken);
}
