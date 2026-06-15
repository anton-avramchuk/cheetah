using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Entities;

namespace Cheetah.Modules.Leads.Application.Leads;

// Справочники статусов/источников — конкретные сущности, не зависят от TLead, поэтому хендлеры
// закрытые и регистрируются source-генератором через [Export].

// ── Статусы ───────────────────────────────────────────────────────────────────────────────────

/// <summary>Справочник статусов лида (для UI/фильтров).</summary>
public sealed record ListLeadStatusesQuery : IQuery<IReadOnlyList<LeadStatusDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListLeadStatusesQuery, IReadOnlyList<LeadStatusDto>>))]
public sealed class ListLeadStatusesQueryHandler : IQueryHandler<ListLeadStatusesQuery, IReadOnlyList<LeadStatusDto>>
{
    private readonly IRepository<LeadStatus, Guid> _repository;
    public ListLeadStatusesQueryHandler(IRepository<LeadStatus, Guid> repository) => _repository = repository;

    public async ValueTask<IReadOnlyList<LeadStatusDto>> HandleAsync(
        ListLeadStatusesQuery query, CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(null, ct);
        return items
            .OrderBy(s => s.Order)
            .Select(s => new LeadStatusDto(s.Id, s.Code, s.Name, s.Order, s.IsTerminal, s.IsActive))
            .ToArray();
    }
}

// ── Источники ────────────────────────────────────────────────────────────────────────────────

/// <summary>Справочник источников лида (для UI/фильтров).</summary>
public sealed record ListLeadSourcesQuery : IQuery<IReadOnlyList<LeadSourceDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListLeadSourcesQuery, IReadOnlyList<LeadSourceDto>>))]
public sealed class ListLeadSourcesQueryHandler : IQueryHandler<ListLeadSourcesQuery, IReadOnlyList<LeadSourceDto>>
{
    private readonly IRepository<LeadSource, Guid> _repository;
    public ListLeadSourcesQueryHandler(IRepository<LeadSource, Guid> repository) => _repository = repository;

    public async ValueTask<IReadOnlyList<LeadSourceDto>> HandleAsync(
        ListLeadSourcesQuery query, CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(null, ct);
        return items
            .OrderBy(s => s.Order)
            .Select(s => new LeadSourceDto(s.Id, s.Code, s.Name, s.Order, s.IsActive))
            .ToArray();
    }
}
