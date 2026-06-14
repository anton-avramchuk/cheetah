using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Domain.Specifications;

/// <summary>
/// «Пора слать»: ожидающие срабатывания, чей момент уже наступил. Горячий путь скана —
/// ложится на индекс <c>(Status, FireAtUtc)</c>.
/// </summary>
public sealed class DueReminderTriggersSpecification : Specification<ReminderTrigger>
{
    private readonly DateTime _nowUtc;
    public DueReminderTriggersSpecification(DateTime nowUtc) => _nowUtc = nowUtc;
    public override Expression<Func<ReminderTrigger, bool>> ToExpression()
        => t => t.Status == ReminderTriggerStatus.Pending && t.FireAtUtc <= _nowUtc;
}

/// <summary>Ожидающие триггеры конкретного события (для пересчёта серии).</summary>
public sealed class PendingTriggersByEventSpecification : Specification<ReminderTrigger>
{
    private readonly Guid _eventId;
    public PendingTriggersByEventSpecification(Guid eventId) => _eventId = eventId;
    public override Expression<Func<ReminderTrigger, bool>> ToExpression()
        => t => t.EventId == _eventId && t.Status == ReminderTriggerStatus.Pending;
}

/// <summary>Все триггеры события (для проверки существования по ключу при upsert).</summary>
public sealed class TriggersByEventSpecification : Specification<ReminderTrigger>
{
    private readonly Guid _eventId;
    public TriggersByEventSpecification(Guid eventId) => _eventId = eventId;
    public override Expression<Func<ReminderTrigger, bool>> ToExpression()
        => t => t.EventId == _eventId;
}
