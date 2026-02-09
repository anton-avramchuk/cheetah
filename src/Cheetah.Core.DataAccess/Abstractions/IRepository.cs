using Cheetah.Core.Domain;
using Cheetah.Core.Specification;

namespace Cheetah.Core.DataAccess.Abstractions;

public interface IRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity> : IRepository<TEntity, Guid>, IReadOnlyRepository<TEntity>
    where TEntity : Entity<Guid>
{
}
