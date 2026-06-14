using Cheetah.Core.Domain;

namespace Cheetah.Modules.Calendar.Domain.Entities;

/// <summary>
/// Переопределение конкретного экземпляра серии (аналог iCal RECURRENCE-ID): либо отмена
/// одного вхождения, либо изменение его времени/заголовка, не затрагивая остальную серию.
/// Идентифицируется <see cref="OccurrenceKey"/> исходного вхождения. Часть агрегата <see cref="CalendarEvent"/>.
/// </summary>
public class EventOccurrenceOverride : Entity<Guid>
{
    public Guid EventId { get; private set; }

    /// <summary>Ключ исходного вхождения серии, которое переопределяем.</summary>
    public string OccurrenceKey { get; private set; } = null!;

    public bool IsCancelled { get; private set; }
    public DateTime? NewStartUtc { get; private set; }
    public DateTime? NewEndUtc { get; private set; }
    public string? NewTitle { get; private set; }

    private EventOccurrenceOverride() { } // EF

    internal static EventOccurrenceOverride Cancellation(Guid eventId, string occurrenceKey)
        => new()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            OccurrenceKey = occurrenceKey,
            IsCancelled = true
        };

    internal static EventOccurrenceOverride Modification(
        Guid eventId, string occurrenceKey, DateTime? newStartUtc, DateTime? newEndUtc, string? newTitle)
        => new()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            OccurrenceKey = occurrenceKey,
            IsCancelled = false,
            NewStartUtc = newStartUtc is { } s ? DateTime.SpecifyKind(s, DateTimeKind.Utc) : null,
            NewEndUtc = newEndUtc is { } e ? DateTime.SpecifyKind(e, DateTimeKind.Utc) : null,
            NewTitle = newTitle
        };
}
