using Cheetah.Core.Domain;

namespace Cheetah.Core.DataAccess;

public interface IRepository<TEntity, TKey> where TEntity : Entity<TKey>
{
    Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
