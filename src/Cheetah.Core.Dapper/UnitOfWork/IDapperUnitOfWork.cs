using System.Data.Common;

namespace Cheetah.Core.Dapper.UnitOfWork;

/// <summary>
/// Scoped unit of work for the Dapper write path. Shares one open connection per
/// connection-string name across all repositories in the current scope, buffers
/// writes, and flushes them atomically (one transaction per connection) on
/// <see cref="SaveChangesAsync"/>. Mirrors EF Core's change-tracking + SaveChanges flow
/// so the existing <see cref="Cheetah.Core.DataAccess.Abstractions.IRepository{TEntity,TKey}"/>
/// contract behaves the same on Dapper.
/// </summary>
public interface IDapperUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Returns the shared, open connection for the given connection-string name,
    /// creating it on first use. Use for read queries that should observe pending
    /// (uncommitted) changes within the active transaction, if any.
    /// </summary>
    ValueTask<DbConnection> GetConnectionAsync(string? connectionName = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the active transaction for a connection, or <c>null</c> if none is open.</summary>
    DbTransaction? GetTransaction(string? connectionName = null);

    /// <summary>Buffers a write to be flushed on <see cref="SaveChangesAsync"/>.</summary>
    void Enqueue(PendingOperation operation);

    /// <summary>
    /// Flushes all buffered writes inside a transaction (per connection) and commits.
    /// Returns the total number of affected rows. On failure every transaction is rolled back.
    /// </summary>
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
