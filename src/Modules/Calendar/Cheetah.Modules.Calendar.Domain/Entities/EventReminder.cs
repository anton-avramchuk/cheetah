using Cheetah.Core.Domain;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Domain.Entities;

/// <summary>
/// Правило напоминания: «за <see cref="OffsetBeforeStart"/> до начала уведомить
/// <see cref="Target"/>». Часть агрегата <see cref="CalendarEvent"/>. Конкретные
/// срабатывания материализуются в <see cref="ReminderTrigger"/>.
/// </summary>
public class EventReminder : Entity<Guid>
{
    public Guid EventId { get; private set; }
    public TimeSpan OffsetBeforeStart { get; private set; }
    public ReminderTarget Target { get; private set; }

    /// <summary>Жёстко заданный канал-подсказка для Notification (обычно null → решает роутер).</summary>
    public string? ForceChannel { get; private set; }

    private EventReminder() { } // EF

    internal static EventReminder Create(Guid eventId, TimeSpan offsetBeforeStart, ReminderTarget target, string? forceChannel)
    {
        if (offsetBeforeStart < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(offsetBeforeStart), "Offset cannot be negative");

        return new EventReminder
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            OffsetBeforeStart = offsetBeforeStart,
            Target = target,
            ForceChannel = forceChannel
        };
    }
}
