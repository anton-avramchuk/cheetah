using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.Connections;
using MongoDB.Driver;

namespace Cheetah.Core.Mongo.UnitOfWork;

/// <summary>
/// Default <see cref="IMongoUnitOfWork"/>. Buffers writes in scope and, on
/// <see cref="SaveChangesAsync"/>, groups them by connection-string name and flushes each group in
/// its own session-backed transaction.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IMongoUnitOfWork))]
public sealed class MongoUnitOfWork : IMongoUnitOfWork
{
    private readonly IMongoDatabaseProvider _databaseProvider;
    private readonly List<PendingWrite> _pending = new();

    /// <summary>Initializes a new instance of the <see cref="MongoUnitOfWork"/> class.</summary>
    public MongoUnitOfWork(IMongoDatabaseProvider databaseProvider)
        => _databaseProvider = databaseProvider;

    /// <inheritdoc />
    public void Enqueue(PendingWrite write) => _pending.Add(write);

    /// <inheritdoc />
    public async ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_pending.Count == 0)
            return 0;

        var total = 0;
        try
        {
            foreach (var group in _pending.GroupBy(p => p.ConnectionName))
                total += await FlushGroupAsync(group.Key, group.ToList(), cancellationToken);
        }
        finally
        {
            _pending.Clear();
        }

        return total;
    }

    private async Task<int> FlushGroupAsync(string? connectionName, List<PendingWrite> writes, CancellationToken cancellationToken)
    {
        var database = await _databaseProvider.GetDatabaseAsync(connectionName, cancellationToken);
        using var session = await database.Client.StartSessionAsync(cancellationToken: cancellationToken);

        session.StartTransaction();
        try
        {
            var affected = 0;
            foreach (var write in writes)
                affected += (int)await write.ApplyAsync(database, session, cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            return affected;
        }
        catch
        {
            await session.AbortTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
