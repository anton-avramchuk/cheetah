using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Calendar.Application.Mapping;
using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Domain.Specifications;

namespace Cheetah.Modules.Calendar.Application.Events;

// Запросы возвращают РАЗВЁРНУТЫЕ экземпляры (occurrences): серии раскрываются экспандером
// в окне [from, to), разовые отдаются как единичный экземпляр. Сортировка — по времени начала.

public sealed record ListEventsByCalendarQuery(Guid CalendarId, DateTime FromUtc, DateTime ToUtc)
    : IQuery<IReadOnlyList<EventOccurrenceDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListEventsByCalendarQuery, IReadOnlyList<EventOccurrenceDto>>))]
public sealed class ListEventsByCalendarQueryHandler
    : IQueryHandler<ListEventsByCalendarQuery, IReadOnlyList<EventOccurrenceDto>>
{
    private readonly ICalendarEventRepository _events;
    private readonly IRecurrenceExpander _expander;

    public ListEventsByCalendarQueryHandler(ICalendarEventRepository events, IRecurrenceExpander expander)
    {
        _events = events;
        _expander = expander;
    }

    public async ValueTask<IReadOnlyList<EventOccurrenceDto>> HandleAsync(
        ListEventsByCalendarQuery query, CancellationToken ct = default)
    {
        var events = await _events.ListWithDetailsAsync(
            new EventsByCalendarInRangeSpecification(query.CalendarId, query.FromUtc, query.ToUtc), ct);
        return OccurrenceExpansion.Expand(_expander, events, query.FromUtc, query.ToUtc);
    }
}

public sealed record ListEventsByEntityQuery(string EntityType, Guid EntityId, DateTime FromUtc, DateTime ToUtc)
    : IQuery<IReadOnlyList<EventOccurrenceDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListEventsByEntityQuery, IReadOnlyList<EventOccurrenceDto>>))]
public sealed class ListEventsByEntityQueryHandler
    : IQueryHandler<ListEventsByEntityQuery, IReadOnlyList<EventOccurrenceDto>>
{
    private readonly ICalendarEventRepository _events;
    private readonly IRecurrenceExpander _expander;

    public ListEventsByEntityQueryHandler(ICalendarEventRepository events, IRecurrenceExpander expander)
    {
        _events = events;
        _expander = expander;
    }

    public async ValueTask<IReadOnlyList<EventOccurrenceDto>> HandleAsync(
        ListEventsByEntityQuery query, CancellationToken ct = default)
    {
        var events = await _events.ListWithDetailsAsync(
            new EventsByEntityInRangeSpecification(query.EntityType, query.EntityId, query.FromUtc, query.ToUtc), ct);
        return OccurrenceExpansion.Expand(_expander, events, query.FromUtc, query.ToUtc);
    }
}

public sealed record ListAgendaQuery(Guid UserId, DateTime FromUtc, DateTime ToUtc)
    : IQuery<IReadOnlyList<EventOccurrenceDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListAgendaQuery, IReadOnlyList<EventOccurrenceDto>>))]
public sealed class ListAgendaQueryHandler : IQueryHandler<ListAgendaQuery, IReadOnlyList<EventOccurrenceDto>>
{
    private readonly ICalendarEventRepository _events;
    private readonly IRecurrenceExpander _expander;

    public ListAgendaQueryHandler(ICalendarEventRepository events, IRecurrenceExpander expander)
    {
        _events = events;
        _expander = expander;
    }

    public async ValueTask<IReadOnlyList<EventOccurrenceDto>> HandleAsync(
        ListAgendaQuery query, CancellationToken ct = default)
    {
        var events = await _events.ListWithDetailsAsync(
            new EventsByAttendeeInRangeSpecification(query.UserId, query.FromUtc, query.ToUtc), ct);
        return OccurrenceExpansion.Expand(_expander, events, query.FromUtc, query.ToUtc);
    }
}

internal static class OccurrenceExpansion
{
    public static IReadOnlyList<EventOccurrenceDto> Expand(
        IRecurrenceExpander expander, IReadOnlyList<CalendarEvent> events, DateTime fromUtc, DateTime toUtc)
        => events
            .SelectMany(e => expander.Expand(e, fromUtc, toUtc).Select(occ => CalendarProjector.ToOccurrenceDto(e, occ)))
            .OrderBy(o => o.StartUtc)
            .ToArray();
}
