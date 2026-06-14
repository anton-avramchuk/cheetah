using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Calendar.Application.Mapping;
using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Domain.Abstractions;

namespace Cheetah.Modules.Calendar.Application.Events;

/// <summary>Событие по идентификатору со всеми участниками и напоминаниями.</summary>
public sealed record GetEventByIdQuery(Guid EventId) : IQuery<CalendarEventDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetEventByIdQuery, CalendarEventDto?>))]
public sealed class GetEventByIdQueryHandler : IQueryHandler<GetEventByIdQuery, CalendarEventDto?>
{
    private readonly ICalendarEventRepository _events;

    public GetEventByIdQueryHandler(ICalendarEventRepository events) => _events = events;

    public async ValueTask<CalendarEventDto?> HandleAsync(GetEventByIdQuery query, CancellationToken ct = default)
    {
        var @event = await _events.GetWithDetailsAsync(query.EventId, ct);
        return @event is null ? null : CalendarProjector.ToDto(@event);
    }
}
