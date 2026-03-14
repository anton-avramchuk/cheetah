namespace Cheetah.BackgroundTasks;

/// <summary>
/// Marker interface for all background tasks managed by <see cref="BackgroundTaskScheduler"/>.
/// Register implementations via DI using <see cref="Cheetah.Core.DependencyInjection.ExportAttribute"/>.
/// </summary>
public interface IBackgroundTask
{
    /// <summary>Human-readable task name used in logs.</summary>
    string Name { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
