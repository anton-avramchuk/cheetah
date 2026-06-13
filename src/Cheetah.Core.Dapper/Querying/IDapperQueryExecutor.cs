namespace Cheetah.Core.Dapper.Querying;

/// <summary>
/// Low-level executor for hand-written SQL — the fast read path for projections,
/// reports and grids. Each call uses its own short-lived connection (it does not
/// enlist in the <see cref="UnitOfWork.IDapperUnitOfWork"/> transaction), which suits
/// stateless CQRS query handlers under high RPS.
/// </summary>
public interface IDapperQueryExecutor
{
    /// <summary>Executes a query and returns all rows mapped to <typeparamref name="TResult"/>.</summary>
    ValueTask<IReadOnlyList<TResult>> QueryAsync<TResult>(
        string sql, object? param = null, string? connectionStringName = null, CancellationToken cancellationToken = default);

    /// <summary>Executes a query and returns the first row, or <c>default</c> if none.</summary>
    ValueTask<TResult?> QueryFirstOrDefaultAsync<TResult>(
        string sql, object? param = null, string? connectionStringName = null, CancellationToken cancellationToken = default);

    /// <summary>Executes a query and returns a single scalar value.</summary>
    ValueTask<TResult?> ExecuteScalarAsync<TResult>(
        string sql, object? param = null, string? connectionStringName = null, CancellationToken cancellationToken = default);

    /// <summary>Executes a non-query statement (auto-committed) and returns affected row count.</summary>
    ValueTask<int> ExecuteAsync(
        string sql, object? param = null, string? connectionStringName = null, CancellationToken cancellationToken = default);
}
