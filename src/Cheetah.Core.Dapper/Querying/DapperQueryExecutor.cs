using Cheetah.Core.Dapper.Connections;
using Cheetah.Core.DependencyInjection;
using Dapper;

namespace Cheetah.Core.Dapper.Querying;

/// <summary>
/// Default <see cref="IDapperQueryExecutor"/> using a fresh connection per call.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IDapperQueryExecutor))]
public class DapperQueryExecutor : IDapperQueryExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>Initializes a new instance of the <see cref="DapperQueryExecutor"/> class.</summary>
    public DapperQueryExecutor(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<TResult>> QueryAsync<TResult>(
        string sql, object? param = null, string? connectionStringName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(connectionStringName, cancellationToken);
        var result = await connection.QueryAsync<TResult>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc />
    public async ValueTask<TResult?> QueryFirstOrDefaultAsync<TResult>(
        string sql, object? param = null, string? connectionStringName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(connectionStringName, cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<TResult>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async ValueTask<TResult?> ExecuteScalarAsync<TResult>(
        string sql, object? param = null, string? connectionStringName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(connectionStringName, cancellationToken);
        return await connection.ExecuteScalarAsync<TResult>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async ValueTask<int> ExecuteAsync(
        string sql, object? param = null, string? connectionStringName = null, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(connectionStringName, cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition(sql, param, cancellationToken: cancellationToken));
    }
}
