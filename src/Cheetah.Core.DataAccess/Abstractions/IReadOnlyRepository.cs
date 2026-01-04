using Cheetah.Core.Domain;

namespace Cheetah.Core.DataAccess.Abstractions;

public interface IReadOnlyRepository<TEntity, TKey> where TEntity : Entity<TKey>
{
    Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<TEntity?> GetByIdAsync(TKey key, CancellationToken cancellationToken = default);
}
