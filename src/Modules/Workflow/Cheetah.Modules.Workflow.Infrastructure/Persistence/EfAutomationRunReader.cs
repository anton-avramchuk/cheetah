using Cheetah.Core.Specification;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Workflow.Infrastructure.Persistence;

/// <summary>
/// EF-реализация <see cref="IAutomationRunReader"/>: отбор — доменная спецификация, сортировка и
/// страница — на стороне БД, чтение без трекинга. Шаги прогона подтягиваются <c>Include</c>:
/// проекция в DTO их разворачивает, и без явной загрузки это был бы N+1.
/// </summary>
public sealed class EfAutomationRunReader<TContext> : IAutomationRunReader
    where TContext : DbContext
{
    private readonly TContext _context;

    public EfAutomationRunReader(TContext context) => _context = context;

    public async ValueTask<IReadOnlyList<AutomationRun>> ListPageAsync(
        ISpecification<AutomationRun>? spec, int skip, int take, CancellationToken ct = default)
    {
        var query = _context.Set<AutomationRun>().AsNoTracking().Include(r => r.Steps);

        var filtered = spec is null ? (IQueryable<AutomationRun>)query : query.Where(spec.ToExpression());

        return await filtered
            .OrderByDescending(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }
}
