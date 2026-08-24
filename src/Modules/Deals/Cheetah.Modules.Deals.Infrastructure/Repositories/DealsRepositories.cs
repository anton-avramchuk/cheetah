using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Core.Specification;
using Cheetah.Modules.Deals.Domain.Abstractions;
using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Domain.Specifications;
using Cheetah.Modules.Deals.Infrastructure.Persistence;
using Cheetah.Modules.Deals.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Deals.Infrastructure.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<Deal, Guid>), typeof(IDealRepository))]
public class DealRepository : EfRepository<DealsDbContext, Deal, Guid>, IDealRepository
{
    public DealRepository(DealsDbContext context) : base(context) { }

    public async ValueTask<IReadOnlyList<StageAggregate>> GetOpenBoardAsync(
        Guid pipelineId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(d => d.PipelineId == pipelineId && d.Status == DealStatus.Open)
            .GroupBy(d => d.StageId)
            .Select(g => new StageAggregate(g.Key, g.Count(), g.Sum(d => d.Value.Amount)))
            .ToListAsync(ct);

    public async ValueTask<List<Deal>> ListAsync(
        ISpecification<Deal> spec, int page, int size, CancellationToken ct = default)
    {
        var skip = Math.Max(0, page - 1) * size;
        return await DbSet.AsNoTracking()
            .Where(spec.ToExpression())
            .OrderByDescending(d => d.CreatedAt)
            // Тай-брейкер: у сделок одной пачки SaveChanges метка CreatedAt совпадает,
            // без него страницы дублируют и теряют строки.
            .ThenBy(d => d.Id)
            .Skip(skip)
            .Take(size)
            .ToListAsync(ct);
    }
}

[Export(LifetimeType.Scoped, typeof(IRepository<Pipeline, Guid>), typeof(IPipelineRepository))]
public class PipelineRepository : EfRepository<DealsDbContext, Pipeline, Guid>, IPipelineRepository
{
    public PipelineRepository(DealsDbContext context) : base(context) { }

    public async ValueTask<Pipeline?> GetWithStagesAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(p => p.Stages).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async ValueTask<Pipeline?> GetDefaultWithStagesAsync(CancellationToken ct = default)
        => await DbSet.Include(p => p.Stages).FirstOrDefaultAsync(p => p.IsDefault, ct);

    public async ValueTask<List<Pipeline>> ListWithStagesAsync(bool activeOnly, CancellationToken ct = default)
        => await DbSet.Include(p => p.Stages)
            .Where(p => !activeOnly || p.IsActive)
            .ToListAsync(ct);
}

[Export(LifetimeType.Scoped, typeof(IRepository<DealStageHistory, Guid>))]
public class DealStageHistoryRepository : EfRepository<DealsDbContext, DealStageHistory, Guid>
{
    public DealStageHistoryRepository(DealsDbContext context) : base(context) { }
}
