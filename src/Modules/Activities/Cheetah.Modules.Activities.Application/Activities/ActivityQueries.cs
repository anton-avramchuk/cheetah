using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Activities.Application.Abstractions;
using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Domain.Specifications;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Application.Activities;

// ── Активность по Id ──────────────────────────────────────────────────────────────────────

/// <summary>Получить активность по идентификатору (null, если не найдена).</summary>
public sealed record GetActivityByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : ActivityDtoBase;

public class GetActivityByIdQueryHandler<TActivity, TDto> : IQueryHandler<GetActivityByIdQuery<TDto>, TDto?>
    where TActivity : ActivityBase
    where TDto : ActivityDtoBase
{
    private readonly IRepository<TActivity, Guid> _repository;
    private readonly IActivityProjector<TActivity, TDto> _projector;

    public GetActivityByIdQueryHandler(IRepository<TActivity, Guid> repository, IActivityProjector<TActivity, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetActivityByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var activity = await _repository.GetByIdAsync(query.Id, ct);
        return activity is null ? null : _projector.ToDto(activity);
    }
}

// ── Список активностей ────────────────────────────────────────────────────────────────────

/// <summary>Список активностей с комбинированным фильтром (любой критерий опционален).</summary>
public sealed record ListActivitiesQuery<TDto>(
    Guid? AssigneeId, ActivityStatus? Status, string? EntityType, Guid? EntityId, DateTimeOffset? DueBefore)
    : IQuery<IReadOnlyList<TDto>>
    where TDto : ActivityDtoBase;

public class ListActivitiesQueryHandler<TActivity, TDto> : IQueryHandler<ListActivitiesQuery<TDto>, IReadOnlyList<TDto>>
    where TActivity : ActivityBase
    where TDto : ActivityDtoBase
{
    private readonly IRepository<TActivity, Guid> _repository;
    private readonly IActivityProjector<TActivity, TDto> _projector;

    public ListActivitiesQueryHandler(IRepository<TActivity, Guid> repository, IActivityProjector<TActivity, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListActivitiesQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = new ActivitiesFilterSpecification<TActivity>(
            query.AssigneeId, query.Status, query.EntityType, query.EntityId, query.DueBefore);

        var items = await _repository.GetAllAsync(spec, ct);
        return items.Select(_projector.ToDto).ToArray();
    }
}
