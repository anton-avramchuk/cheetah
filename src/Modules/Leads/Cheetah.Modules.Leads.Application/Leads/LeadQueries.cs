using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Leads.Application.Abstractions;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Domain.Specifications;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Application.Leads;

// ── Лид по Id ─────────────────────────────────────────────────────────────────────────────────

/// <summary>Получить лид по идентификатору (null, если не найден).</summary>
public sealed record GetLeadByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : LeadDtoBase;

public class GetLeadByIdQueryHandler<TLead, TDto> : IQueryHandler<GetLeadByIdQuery<TDto>, TDto?>
    where TLead : LeadBase
    where TDto : LeadDtoBase
{
    private readonly IRepository<TLead, Guid> _repository;
    private readonly ILeadProjector<TLead, TDto> _projector;

    public GetLeadByIdQueryHandler(IRepository<TLead, Guid> repository, ILeadProjector<TLead, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetLeadByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var lead = await _repository.GetByIdAsync(query.Id, ct);
        return lead is null ? null : _projector.ToDto(lead);
    }
}

// ── Список лидов ─────────────────────────────────────────────────────────────────────────────

/// <summary>Список лидов с комбинированным фильтром (любой критерий опционален).</summary>
public sealed record ListLeadsQuery<TDto>(LeadStatus? Status, LeadSource? Source, Guid? OwnerId)
    : IQuery<IReadOnlyList<TDto>>
    where TDto : LeadDtoBase;

public class ListLeadsQueryHandler<TLead, TDto> : IQueryHandler<ListLeadsQuery<TDto>, IReadOnlyList<TDto>>
    where TLead : LeadBase
    where TDto : LeadDtoBase
{
    private readonly IRepository<TLead, Guid> _repository;
    private readonly ILeadProjector<TLead, TDto> _projector;

    public ListLeadsQueryHandler(IRepository<TLead, Guid> repository, ILeadProjector<TLead, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListLeadsQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = new LeadsFilterSpecification<TLead>(query.Status, query.Source, query.OwnerId);
        var items = await _repository.GetAllAsync(spec, ct);
        return items.Select(_projector.ToDto).ToArray();
    }
}
