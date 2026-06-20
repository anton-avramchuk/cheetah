using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Core.Specification;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Persistence;

/// <summary>
/// EF-репозиторий флага с child-aware загрузкой агрегата. Наследует общий <see cref="EfRepository{TContext,TEntity,TKey}"/>
/// и добавляет <c>Include</c> детей для операций замены правил/вариантов/override.
/// </summary>
public sealed class FeatureFlagRepository<TContext, TFlag> : EfRepository<TContext, TFlag, Guid>, IFeatureFlagRepository<TFlag>
    where TContext : DbContext
    where TFlag : FeatureFlagBase
{
    public FeatureFlagRepository(TContext dbContext) : base(dbContext)
    {
    }

    public async ValueTask<TFlag?> GetByKeyAsync(string key, bool includeChildren, CancellationToken ct = default)
        => await (includeChildren ? WithChildren() : DbSet.AsQueryable())
            .FirstOrDefaultAsync(f => f.Key == key, ct);

    public async ValueTask<IReadOnlyList<TFlag>> ListAsync(ISpecification<TFlag>? spec, bool includeChildren, CancellationToken ct = default)
    {
        var query = includeChildren ? WithChildren() : DbSet.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    private IQueryable<TFlag> WithChildren()
        => DbSet.Include(f => f.Rules).Include(f => f.Variants).Include(f => f.Overrides);
}
