using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain;
using Cheetah.Core.Specification;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.EntityFramework.Repositories;

public class EfRepository<TDbContext, TEntity, TKey> : IRepository<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : Entity<TKey>
{
    protected TDbContext DbContext { get; }

    protected DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    public EfRepository(TDbContext dbContext) => DbContext = dbContext;

    public async ValueTask<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync([id], cancellationToken);

    public async ValueTask<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        => await DbSet.Where(spec.ToExpression()).FirstOrDefaultAsync(cancellationToken);

    public async ValueTask<List<TEntity>> GetAllAsync(ISpecification<TEntity>? spec = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(cancellationToken);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(spec.ToExpression(), cancellationToken);

    public IQueryable<TEntity> AsQueryable() => DbSet;

    public IQueryable<TEntity> AsNoTrackingQueryable() => DbSet.AsNoTracking();

    public void Add(TEntity entity) => DbSet.Add(entity);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Delete(TEntity entity) => DbSet.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await DbContext.SaveChangesAsync(cancellationToken);
}

public class EfRepository<TDbContext, TEntity> : EfRepository<TDbContext, TEntity, Guid>, IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : Entity<Guid>
{
    public EfRepository(TDbContext dbContext) : base(dbContext)
    {
    }
}
