using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cheetah.Core.EntityFramework;

public interface ICrmDbContext : IDisposable, IInfrastructure<IServiceProvider>
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    EntityEntry Entry(object entity);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    EntityEntry Add(object entity);
    EntityEntry Update(object entity);
    EntityEntry Remove(object entity);

    EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;

    EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;

    EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;

    IDbContextTransaction CreateTransaction();
}