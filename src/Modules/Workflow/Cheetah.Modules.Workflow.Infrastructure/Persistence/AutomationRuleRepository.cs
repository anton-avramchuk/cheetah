using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Core.Specification;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Workflow.Infrastructure.Persistence;

/// <summary>
/// EF-репозиторий правила с child-aware загрузкой (триггеры + действия). Наследует общий
/// <see cref="EfRepository{TContext,TEntity,TKey}"/> и добавляет <c>Include</c> детей.
/// </summary>
public sealed class AutomationRuleRepository<TContext, TRule> : EfRepository<TContext, TRule, Guid>, IAutomationRuleRepository<TRule>
    where TContext : DbContext
    where TRule : AutomationRuleBase
{
    public AutomationRuleRepository(TContext dbContext) : base(dbContext)
    {
    }

    public async ValueTask<TRule?> GetByIdAsync(Guid id, bool includeChildren, CancellationToken ct = default)
        => await Query(includeChildren).FirstOrDefaultAsync(r => r.Id == id, ct);

    public async ValueTask<IReadOnlyList<TRule>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, bool includeChildren, CancellationToken ct = default)
        => await Query(includeChildren).Where(r => ids.Contains(r.Id)).ToListAsync(ct);

    public async ValueTask<IReadOnlyList<TRule>> ListAsync(ISpecification<TRule>? spec, bool includeChildren, CancellationToken ct = default)
    {
        var query = Query(includeChildren);
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    private IQueryable<TRule> Query(bool includeChildren)
        => includeChildren
            ? DbSet.Include(r => r.Triggers).Include(r => r.Actions)
            : DbSet.AsQueryable();
}
