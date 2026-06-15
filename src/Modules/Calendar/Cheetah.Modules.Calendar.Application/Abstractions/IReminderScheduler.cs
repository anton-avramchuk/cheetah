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
    /// <param name="eventIsNew">
    /// Событие только что создано в текущей транзакции — ожидающих триггеров у него заведомо нет,
    /// поэтому загрузка существующих пропускается (экономит round-trip на горячем пути создания).
    /// </param>
    ValueTask RebuildAsync(
        CalendarEvent @event, DateTime horizonEndUtc, bool eventIsNew = false, CancellationToken cancellationToken = default);
}
