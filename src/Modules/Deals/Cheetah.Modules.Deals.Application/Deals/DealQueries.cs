using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Deals.Application.Mapping;
using Cheetah.Modules.Deals.Contracts;
using Cheetah.Modules.Deals.Domain.Abstractions;
using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Domain.Specifications;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Application.Deals;

// ── Сделка по Id ─────────────────────────────────────────────────────────────────────────

public sealed record GetDealByIdQuery(Guid DealId) : IQuery<DealDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetDealByIdQuery, DealDto?>))]
public sealed class GetDealByIdQueryHandler : IQueryHandler<GetDealByIdQuery, DealDto?>
{
    private readonly IRepository<Deal, Guid> _deals;
    public GetDealByIdQueryHandler(IRepository<Deal, Guid> deals) => _deals = deals;

    public async ValueTask<DealDto?> HandleAsync(GetDealByIdQuery query, CancellationToken ct = default)
    {
        var deal = await _deals.GetByIdAsync(query.DealId, ct);
        return deal is null ? null : DealProjector.ToDto(deal);
    }
}

// ── Список сделок ────────────────────────────────────────────────────────────────────────

public sealed record ListDealsQuery(
    Guid? OwnerId, Guid? PipelineId, Guid? StageId, Guid? CustomerId, DealStatus? Status,
    int Page, int Size) : IQuery<IReadOnlyList<DealListItemDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListDealsQuery, IReadOnlyList<DealListItemDto>>))]
public sealed class ListDealsQueryHandler : IQueryHandler<ListDealsQuery, IReadOnlyList<DealListItemDto>>
{
    private readonly IDealRepository _deals;
    public ListDealsQueryHandler(IDealRepository deals) => _deals = deals;

    public async ValueTask<IReadOnlyList<DealListItemDto>> HandleAsync(ListDealsQuery query, CancellationToken ct = default)
    {
        var spec = new DealsFilterSpecification(
            query.OwnerId, query.PipelineId, query.StageId, query.CustomerId, query.Status);
        var size = query.Size <= 0 ? 50 : query.Size;
        var deals = await _deals.ListAsync(spec, query.Page, size, ct);
        return deals.Select(DealProjector.ToListItem).ToArray();
    }
}

// ── История стадий ───────────────────────────────────────────────────────────────────────

public sealed record GetDealHistoryQuery(Guid DealId) : IQuery<IReadOnlyList<DealHistoryDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetDealHistoryQuery, IReadOnlyList<DealHistoryDto>>))]
public sealed class GetDealHistoryQueryHandler : IQueryHandler<GetDealHistoryQuery, IReadOnlyList<DealHistoryDto>>
{
    private readonly IRepository<DealStageHistory, Guid> _history;
    public GetDealHistoryQueryHandler(IRepository<DealStageHistory, Guid> history) => _history = history;

    public async ValueTask<IReadOnlyList<DealHistoryDto>> HandleAsync(GetDealHistoryQuery query, CancellationToken ct = default)
    {
        var rows = await _history.GetAllAsync(new StageHistoryByDealSpecification(query.DealId), ct);
        return rows.OrderBy(h => h.CreatedAt).Select(DealProjector.ToDto).ToArray();
    }
}

// ── Kanban-доска ─────────────────────────────────────────────────────────────────────────

public sealed record GetDealBoardQuery(Guid PipelineId) : IQuery<BoardDto>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetDealBoardQuery, BoardDto>))]
public sealed class GetDealBoardQueryHandler : IQueryHandler<GetDealBoardQuery, BoardDto>
{
    private readonly IDealRepository _deals;
    public GetDealBoardQueryHandler(IDealRepository deals) => _deals = deals;

    public async ValueTask<BoardDto> HandleAsync(GetDealBoardQuery query, CancellationToken ct = default)
    {
        var aggregates = await _deals.GetOpenBoardAsync(query.PipelineId, ct);
        var columns = aggregates
            .Select(a => new BoardColumnDto(a.StageId, a.Count, a.Sum))
            .ToArray();
        return new BoardDto(query.PipelineId, columns);
    }
}
