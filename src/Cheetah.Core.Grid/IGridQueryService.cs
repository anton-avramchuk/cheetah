using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;

namespace Cheetah.Core.Grid;

/// <summary>
/// Service for executing grid queries with filtering, sorting, and pagination
/// All operations are executed at database level via IQueryable
/// </summary>
public interface IGridQueryService
{
    /// <summary>
    /// Executes a grid query with filtering, sorting, pagination, and projection
    /// </summary>
    /// <typeparam name="TEntity">Source entity type</typeparam>
    /// <typeparam name="TViewModel">Target view model type</typeparam>
    /// <param name="queryable">Source queryable (should be AsNoTracking for read-only)</param>
    /// <param name="request">Grid request with pagination, sorting, and filtering</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Grid result with data and total count</returns>
    ValueTask<GridResult<TViewModel>> ExecuteAsync<TEntity, TViewModel>(
        IQueryable<TEntity> queryable,
        GridRequest request,
        CancellationToken ct = default)
        where TEntity : class
        where TViewModel : class;
}
