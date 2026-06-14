using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Calendar.Application.Mapping;
using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Application.Registry;

/// <summary>Все зарегистрированные привязываемые типы.</summary>
public sealed record GetCalendarableTypesQuery : IQuery<IReadOnlyList<CalendarableEntityTypeDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCalendarableTypesQuery, IReadOnlyList<CalendarableEntityTypeDto>>))]
public sealed class GetCalendarableTypesQueryHandler
    : IQueryHandler<GetCalendarableTypesQuery, IReadOnlyList<CalendarableEntityTypeDto>>
{
    private readonly IRepository<CalendarableEntityType, Guid> _types;

    public GetCalendarableTypesQueryHandler(IRepository<CalendarableEntityType, Guid> types) => _types = types;

    public async ValueTask<IReadOnlyList<CalendarableEntityTypeDto>> HandleAsync(
        GetCalendarableTypesQuery query, CancellationToken ct = default)
    {
        var items = await _types.GetAllAsync(cancellationToken: ct);
        return items.Select(CalendarProjector.ToDto).ToArray();
    }
}
