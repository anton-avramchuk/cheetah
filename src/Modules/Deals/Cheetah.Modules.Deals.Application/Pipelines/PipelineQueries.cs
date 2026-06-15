using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Deals.Application.Mapping;
using Cheetah.Modules.Deals.Contracts;
using Cheetah.Modules.Deals.Domain.Abstractions;

namespace Cheetah.Modules.Deals.Application.Pipelines;

// ── Воронка по Id ────────────────────────────────────────────────────────────────────────

public sealed record GetPipelineByIdQuery(Guid PipelineId) : IQuery<PipelineDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetPipelineByIdQuery, PipelineDto?>))]
public sealed class GetPipelineByIdQueryHandler : IQueryHandler<GetPipelineByIdQuery, PipelineDto?>
{
    private readonly IPipelineRepository _pipelines;
    public GetPipelineByIdQueryHandler(IPipelineRepository pipelines) => _pipelines = pipelines;

    public async ValueTask<PipelineDto?> HandleAsync(GetPipelineByIdQuery query, CancellationToken ct = default)
    {
        var pipeline = await _pipelines.GetWithStagesAsync(query.PipelineId, ct);
        return pipeline is null ? null : DealProjector.ToDto(pipeline);
    }
}

// ── Список воронок ───────────────────────────────────────────────────────────────────────

public sealed record ListPipelinesQuery(bool ActiveOnly) : IQuery<IReadOnlyList<PipelineDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListPipelinesQuery, IReadOnlyList<PipelineDto>>))]
public sealed class ListPipelinesQueryHandler : IQueryHandler<ListPipelinesQuery, IReadOnlyList<PipelineDto>>
{
    private readonly IPipelineRepository _pipelines;
    public ListPipelinesQueryHandler(IPipelineRepository pipelines) => _pipelines = pipelines;

    public async ValueTask<IReadOnlyList<PipelineDto>> HandleAsync(ListPipelinesQuery query, CancellationToken ct = default)
    {
        var pipelines = await _pipelines.ListWithStagesAsync(query.ActiveOnly, ct);
        return pipelines.Select(DealProjector.ToDto).ToArray();
    }
}
