using MongoDB.Driver;

namespace Cheetah.Core.Mongo.UnitOfWork;

/// <summary>
/// A buffered write captured by a repository and flushed by <see cref="IMongoUnitOfWork"/> inside a
/// transaction. The <see cref="ApplyAsync"/> delegate resolves its own typed collection from the
/// supplied database (which belongs to the same client/session) and returns the affected count.
/// </summary>
public sealed class PendingWrite
{
    /// <summary>Logical connection-string name the write targets (<c>null</c> = default).</summary>
    public required string? ConnectionName { get; init; }

    /// <summary>Executes the write against the given database within the active session.</summary>
    public required Func<IMongoDatabase, IClientSessionHandle, CancellationToken, Task<long>> ApplyAsync { get; init; }
}
