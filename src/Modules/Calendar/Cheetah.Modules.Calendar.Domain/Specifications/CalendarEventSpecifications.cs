using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Domain.Specifications;

/// <summary>События конкретного календаря (кроме отменённых).</summary>
public sealed class EventsByCalendarSpecification : Specification<CalendarEvent>
{
    private readonly Guid _calendarId;
    public EventsByCalendarSpecification(Guid calendarId) => _calendarId = calendarId;
    public override Expression<Func<CalendarEvent, bool>> ToExpression()
        => e => e.CalendarId == _calendarId && e.Status != EventStatus.Cancelled;
}

/// <summary>События, привязанные к конкретной сущности (кроме отменённых).</summary>
public sealed class EventsByEntitySpecification : Specification<CalendarEvent>
{
    private readonly string _entityType;
    private readonly Guid _entityId;
    public EventsByEntitySpecification(string entityType, Guid entityId)
    {
        _entityType = entityType;
        _entityId = entityId;
    }
    public override Expression<Func<CalendarEvent, bool>> ToExpression()
        => e => e.EntityType == _entityType && e.EntityId == _entityId && e.Status != EventStatus.Cancelled;
}

/// <summary>
/// События, потенциально пересекающие окно [from, to): либо серии (раскрываются экспандером),
/// либо разовые, чей период пересекает окно. Точное попадание экземпляров в окно — за экспандером.
/// </summary>
public sealed class EventsOverlappingRangeSpecification : Specification<CalendarEvent>
{
    private readonly DateTime _fromUtc;
    private readonly DateTime _toUtc;
    public EventsOverlappingRangeSpecification(DateTime fromUtc, DateTime toUtc)
    {
        _fromUtc = fromUtc;
        _toUtc = toUtc;
    }
    public override Expression<Func<CalendarEvent, bool>> ToExpression()
        => e => e.Status != EventStatus.Cancelled
                && (e.Recurrence != null || (e.StartUtc < _toUtc && e.EndUtc >= _fromUtc));
}

/// <summary>События календаря, потенциально попадающие в окно [from, to) (серии + пересекающие разовые).</summary>
public sealed class EventsByCalendarInRangeSpecification : Specification<CalendarEvent>
{
    private readonly Guid _calendarId;
    private readonly DateTime _fromUtc;
    private readonly DateTime _toUtc;
    public EventsByCalendarInRangeSpecification(Guid calendarId, DateTime fromUtc, DateTime toUtc)
    {
        _calendarId = calendarId;
        _fromUtc = fromUtc;
        _toUtc = toUtc;
    }
    public override Expression<Func<CalendarEvent, bool>> ToExpression()
        => e => e.CalendarId == _calendarId && e.Status != EventStatus.Cancelled
                && (e.Recurrence != null || (e.StartUtc < _toUtc && e.EndUtc >= _fromUtc));
}

/// <summary>События сущности, потенциально попадающие в окно [from, to).</summary>
public sealed class EventsByEntityInRangeSpecification : Specification<CalendarEvent>
{
    private readonly string _entityType;
    private readonly Guid _entityId;
    private readonly DateTime _fromUtc;
    private readonly DateTime _toUtc;
    public EventsByEntityInRangeSpecification(string entityType, Guid entityId, DateTime fromUtc, DateTime toUtc)
    {
        _entityType = entityType;
        _entityId = entityId;
        _fromUtc = fromUtc;
        _toUtc = toUtc;
    }
    public override Expression<Func<CalendarEvent, bool>> ToExpression()
        => e => e.EntityType == _entityType && e.EntityId == _entityId && e.Status != EventStatus.Cancelled
                && (e.Recurrence != null || (e.StartUtc < _toUtc && e.EndUtc >= _fromUtc));
}

/// <summary>События участника, потенциально попадающие в окно [from, to) (повестка).</summary>
public sealed class EventsByAttendeeInRangeSpecification : Specification<CalendarEvent>
{
    private readonly Guid _userId;
    private readonly DateTime _fromUtc;
    private readonly DateTime _toUtc;
    public EventsByAttendeeInRangeSpecification(Guid userId, DateTime fromUtc, DateTime toUtc)
    {
        _userId = userId;
        _fromUtc = fromUtc;
        _toUtc = toUtc;
    }
    public override Expression<Func<CalendarEvent, bool>> ToExpression()
        => e => e.Status != EventStatus.Cancelled
                && e.Attendees.Any(a => a.UserId == _userId)
                && (e.Recurrence != null || (e.StartUtc < _toUtc && e.EndUtc >= _fromUtc));
}

/// <summary>Все повторяющиеся события (для фоновой материализации горизонта напоминаний).</summary>
public sealed class RecurringEventsSpecification : Specification<CalendarEvent>
{
    public override Expression<Func<CalendarEvent, bool>> ToExpression()
        => e => e.Recurrence != null && e.Status != EventStatus.Cancelled;
}

/// <summary>События, в которых пользователь — участник (для повестки). Фильтр по диапазону — поверх.</summary>
public sealed class EventsByAttendeeSpecification : Specification<CalendarEvent>
{
    private readonly Guid _userId;
    public EventsByAttendeeSpecification(Guid userId) => _userId = userId;
    public override Expression<Func<CalendarEvent, bool>> ToExpression()
        => e => e.Status != EventStatus.Cancelled && e.Attendees.Any(a => a.UserId == _userId);
}
