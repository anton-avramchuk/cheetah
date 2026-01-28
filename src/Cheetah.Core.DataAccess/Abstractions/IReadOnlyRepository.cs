using Cheetah.Core.Domain;
using Cheetah.Core.Specification;

namespace Cheetah.Core.DataAccess.Abstractions;

public interface IReadOnlyRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    ValueTask<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
    ValueTask<List<TEntity>> GetAllAsync(ISpecification<TEntity>? spec = null, CancellationToken cancellationToken = default);
    ValueTask<bool> ExistsAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
    IQueryable<TEntity> AsQueryable();
    IQueryable<TEntity> AsNoTrackingQueryable();
}
