using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Application.Abstractions;

/// <summary>
/// Пересобирает материализованные срабатывания напоминаний (<see cref="ReminderTrigger"/>)
/// для события на горизонт. Идемпотентен по ключу <c>(EventId, ReminderId, OccurrenceKey)</c>:
/// уже отправленные не трогает, лишние/устаревшие — отменяет, недостающие — создаёт.
/// SaveChanges НЕ вызывает — фиксирует вызывающий (для атомарности с остальными изменениями).
/// </summary>
public interface IReminderScheduler
{
    ValueTask RebuildAsync(CalendarEvent @event, DateTime horizonEndUtc, CancellationToken cancellationToken = default);
}
