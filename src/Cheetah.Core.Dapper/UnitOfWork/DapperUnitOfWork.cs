using System.Data.Common;
using Cheetah.Core.Dapper.Connections;
using Cheetah.Core.DependencyInjection;
using Dapper;

namespace Cheetah.Core.Dapper.UnitOfWork;

/// <summary>
/// Default scoped <see cref="IDapperUnitOfWork"/>.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IDapperUnitOfWork))]
public sealed class DapperUnitOfWork : IDapperUnitOfWork
{
    private const string DefaultKey = "";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly Dictionary<string, DbConnection> _connections = new();
    private readonly Dictionary<string, DbTransaction> _transactions = new();
    private readonly List<PendingOperation> _pending = new();

    /// <summary>Initializes a new instance of the <see cref="DapperUnitOfWork"/> class.</summary>
    public DapperUnitOfWork(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    private static string Key(string? name) => name ?? DefaultKey;

    /// <inheritdoc />
    public async ValueTask<DbConnection> GetConnectionAsync(
        string? connectionName = null, CancellationToken cancellationToken = default)
    {
        var key = Key(connectionName);
        if (_connections.TryGetValue(key, out var existing))
            return existing;

        var connection = await _connectionFactory.CreateOpenConnectionAsync(connectionName, cancellationToken);
        _connections[key] = connection;
        return connection;
    }

    /// <inheritdoc />
    public DbTransaction? GetTransaction(string? connectionName = null)
        => _transactions.GetValueOrDefault(Key(connectionName));

    /// <inheritdoc />
    public void Enqueue(PendingOperation operation) => _pending.Add(operation);

    /// <inheritdoc />
    public async ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_pending.Count == 0)
            return 0;

        var groups = _pending.GroupBy(op => Key(op.ConnectionName)).ToList();
        var started = new List<DbTransaction>();
        var affected = 0;

        try
        {
            foreach (var group in groups)
            {
                var connection = await GetConnectionAsync(group.Key == DefaultKey ? null : group.Key, cancellationToken);
                var transaction = await connection.BeginTransactionAsync(cancellationToken);
                _transactions[group.Key] = transaction;
                started.Add(transaction);

                foreach (var op in group)
                    affected += await ExecuteAsync(connection, transaction, op, cancellationToken);
            }

            foreach (var transaction in started)
                await transaction.CommitAsync(cancellationToken);

            _pending.Clear();
            return affected;
        }
        catch
        {
            foreach (var transaction in started)
            {
                try { await transaction.RollbackAsync(cancellationToken); }
                catch { /* preserve the original exception */ }
            }

            throw;
        }
        finally
        {
            foreach (var transaction in started)
                await transaction.DisposeAsync();
            _transactions.Clear();
        }
    }

    private static async Task<int> ExecuteAsync(
        DbConnection connection, DbTransaction transaction, PendingOperation op, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(op.Sql, op.Parameters, transaction, cancellationToken: cancellationToken);

        if (op.GeneratedKeyProperty is null)
            return await connection.ExecuteAsync(command);

        var key = await connection.ExecuteScalarAsync(command);
        if (key is not null && key is not DBNull)
        {
            var targetType = Nullable.GetUnderlyingType(op.GeneratedKeyProperty.PropertyType)
                             ?? op.GeneratedKeyProperty.PropertyType;
            op.GeneratedKeyProperty.SetValue(op.Parameters, Convert.ChangeType(key, targetType));
        }

        return 1;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var transaction in _transactions.Values)
            await transaction.DisposeAsync();

        foreach (var connection in _connections.Values)
            await connection.DisposeAsync();

        _transactions.Clear();
        _connections.Clear();
    }
}
