namespace Cheetah.Core.Mongo.UnitOfWork;

/// <summary>
/// Scoped unit of work for the MongoDB write path. Buffers Add/Update/Delete operations and flushes
/// them on <see cref="SaveChangesAsync"/> inside a multi-document transaction (one session +
/// transaction per distinct connection-string name), mirroring EF Core's SaveChanges flow so the
/// shared <see cref="Cheetah.Core.DataAccess.Abstractions.IRepository{TEntity,TKey}"/> contract
/// behaves the same on Mongo.
/// </summary>
/// <remarks>
/// Multi-document transactions require a replica set (or sharded cluster); they are not available on
/// a standalone <c>mongod</c>. As with the Dapper unit of work, atomicity spans a single connection:
/// writes to different connection-string names commit in separate transactions.
/// </remarks>
public interface IMongoUnitOfWork : IAsyncDisposable
{
    /// <summary>Buffers a write to be flushed on <see cref="SaveChangesAsync"/>.</summary>
    void Enqueue(PendingWrite write);

    /// <summary>
    /// Flushes all buffered writes, each connection inside its own transaction, and commits.
    /// Returns the total affected document count. On failure the active transaction is aborted.
    /// </summary>
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
