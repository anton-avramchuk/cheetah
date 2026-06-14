using Cheetah.Core.Domain;

namespace Cheetah.Modules.Calendar.Domain.ValueObjects;

/// <summary>
/// Правило повторения события (RFC 5545 RRULE) + список исключённых дат (EXDATE).
/// Сама строка RRULE парсится и раскрывается в Infrastructure через <c>IRecurrenceExpander</c>,
/// поэтому Domain не зависит от iCal-библиотеки и хранит правило как канонический текст.
///
/// <para>Override отдельных экземпляров серии живёт в <c>EventOccurrenceOverride</c>
/// (RECURRENCE-ID) — это уже изменённые данные вхождения, а не часть «правила».</para>
/// </summary>
public sealed class RecurrenceRule : ValueObject
{
    public string RRule { get; private set; }

    /// <summary>Исключённые экземпляры серии (в UTC), сериализуются вместе с правилом.</summary>
    public IReadOnlyList<DateTime> ExDatesUtc { get; private set; }

    private RecurrenceRule()
    {
        RRule = null!;
        ExDatesUtc = Array.Empty<DateTime>();
    }

    public RecurrenceRule(string rrule, IReadOnlyList<DateTime>? exDatesUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rrule);
        RRule = rrule.Trim();
        ExDatesUtc = exDatesUtc is { Count: > 0 }
            ? exDatesUtc.Select(d => DateTime.SpecifyKind(d, DateTimeKind.Utc)).Distinct().OrderBy(d => d).ToArray()
            : Array.Empty<DateTime>();
    }

    /// <summary>Возвращает копию правила с другим набором EXDATE.</summary>
    public RecurrenceRule WithExDates(IReadOnlyList<DateTime>? exDatesUtc)
        => new(RRule, exDatesUtc);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return RRule;
        foreach (var d in ExDatesUtc)
            yield return d;
    }
}
