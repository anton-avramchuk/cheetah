using Cheetah.Core.Domain;

namespace Cheetah.Core.DataAccess;

/// <summary>
/// Generic repository interface
/// </summary>
public interface IRepository<TEntity, in TId> where TEntity : AggregateRoot<TId>
{
    ValueTask<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    ValueTask AddAsync(TEntity entity, CancellationToken ct = default);
    ValueTask UpdateAsync(TEntity entity, CancellationToken ct = default);
    ValueTask DeleteAsync(TEntity entity, CancellationToken ct = default);
    ValueTask<List<TEntity>> ListAsync(CancellationToken ct = default);
    ValueTask SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>
/// Generic repository interface
/// </summary>
public interface IRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : AggregateRoot<Guid>
{
    
}