using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain;

namespace Cheetah.Core.Grid;

public interface IGridRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    ValueTask<GridResult<TViewModel>> GetGridAsync<TViewModel>(
        GridRequest request,
        CancellationToken ct = default)
        where TViewModel : class;
}

public interface IGridRepository<TEntity> : IGridRepository<TEntity, Guid>, IRepository<TEntity>
    where TEntity : Entity<Guid>
{
}
