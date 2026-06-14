using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Domain.Abstractions;

/// <summary>Конкретный экземпляр события на временной шкале после раскрытия серии.</summary>
/// <param name="StartUtc">Начало экземпляра (UTC).</param>
/// <param name="EndUtc">Конец экземпляра (UTC).</param>
/// <param name="OccurrenceKey">Канонический ключ исходного вхождения (RECURRENCE-ID).</param>
/// <param name="IsOverride">Экземпляр переопределён <c>EventOccurrenceOverride</c>.</param>
public readonly record struct Occurrence(
    DateTime StartUtc,
    DateTime EndUtc,
    string OccurrenceKey,
    bool IsOverride);

/// <summary>
/// Раскрывает событие в конкретные экземпляры в заданном окне с учётом RRULE, EXDATE и override.
/// Реализация — в Infrastructure (изолирует зависимость от iCal-движка). Для разового события
/// возвращает не более одного экземпляра, попадающего в окно.
/// </summary>
public interface IRecurrenceExpander
{
    /// <summary>
    /// Возвращает экземпляры события, начало которых попадает в полуинтервал [fromUtc, toUtc).
    /// Отменённые override-ом и попавшие в EXDATE экземпляры исключаются.
    /// </summary>
    IEnumerable<Occurrence> Expand(CalendarEvent @event, DateTime fromUtc, DateTime toUtc);
}
