using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Calendar.Application.Mapping;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Application.Calendars;

/// <summary>Календарь по идентификатору.</summary>
public sealed record GetCalendarByIdQuery(Guid CalendarId) : IQuery<CalendarDto?>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetCalendarByIdQuery, CalendarDto?>))]
public sealed class GetCalendarByIdQueryHandler : IQueryHandler<GetCalendarByIdQuery, CalendarDto?>
{
    private readonly IRepository<Domain.Entities.Calendar, Guid> _calendars;

    public GetCalendarByIdQueryHandler(IRepository<Domain.Entities.Calendar, Guid> calendars)
        => _calendars = calendars;

    public async ValueTask<CalendarDto?> HandleAsync(GetCalendarByIdQuery query, CancellationToken ct = default)
    {
        var calendar = await _calendars.GetByIdAsync(query.CalendarId, ct);
        return calendar is null ? null : CalendarProjector.ToDto(calendar);
    }
}
