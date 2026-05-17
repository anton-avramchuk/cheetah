namespace Cheetah.DistributedLock;

/// <summary>
/// Удерживаемый distributed-lock. Освобождается при DisposeAsync.
/// </summary>
public interface IDistributedLock : IAsyncDisposable
{
    string Key { get; }
}
