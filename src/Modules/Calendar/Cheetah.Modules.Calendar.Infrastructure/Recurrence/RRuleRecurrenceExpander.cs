using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Calendar.Domain;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Infrastructure.Recurrence;

/// <summary>
/// Раскрытие событий в конкретные экземпляры по RFC 5545 RRULE. Реализация за интерфейсом
/// <see cref="IRecurrenceExpander"/> — её можно заменить полноценным iCal-движком (напр. Ical.Net),
/// не трогая остальной модуль.
///
/// <para>Поддерживаемый практический поднабор RRULE: FREQ (DAILY/WEEKLY/MONTHLY/YEARLY),
/// INTERVAL, COUNT, UNTIL, BYDAY (для WEEKLY), BYMONTHDAY (для MONTHLY/YEARLY). Раскрытие идёт
/// в таймзоне события (корректно для перехода на летнее/зимнее время), затем переводится в UTC.
/// EXDATE и override отдельных экземпляров применяются поверх.</para>
/// </summary>
[Export(LifetimeType.Singleton, typeof(IRecurrenceExpander))]
public sealed class RRuleRecurrenceExpander : IRecurrenceExpander
{
    private const int MaxIterations = 100_000;

    public IEnumerable<Occurrence> Expand(CalendarEvent @event, DateTime fromUtc, DateTime toUtc)
    {
        fromUtc = DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc);
        toUtc = DateTime.SpecifyKind(toUtc, DateTimeKind.Utc);

        var overrides = @event.Overrides.ToDictionary(o => o.OccurrenceKey);
        var exDates = @event.Recurrence is null
            ? new HashSet<string>()
            : @event.Recurrence.ExDatesUtc.Select(RecurrenceKeys.FromUtc).ToHashSet();

        // Разовое событие — максимум один экземпляр.
        if (@event.Recurrence is null)
        {
            foreach (var occ in EmitOne(@event, @event.StartUtc, overrides, exDates, fromUtc, toUtc))
                yield return occ;
            yield break;
        }

        var tz = ResolveTimeZone(@event.TimeZoneId);
        var duration = @event.EndUtc - @event.StartUtc;
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(@event.StartUtc, tz); // Unspecified kind

        var rule = RRuleParser.Parse(@event.Recurrence.RRule);

        var emitted = 0;
        var iterations = 0;
        foreach (var localCandidate in GenerateLocalStarts(localStart, rule))
        {
            if (++iterations > MaxIterations)
                yield break;

            var startUtc = ToUtcSafe(localCandidate, tz);

            // UNTIL — граница серии (включительно); дальше не идём.
            if (rule.UntilUtc is { } until && startUtc > until)
                yield break;

            emitted++;
            // COUNT считает ВСЕ сгенерированные экземпляры серии, не только попавшие в окно.
            if (rule.Count is { } count && emitted > count)
                yield break;

            // Раз окно перейдено — последующие экземпляры только позже, можно прекращать.
            if (startUtc >= toUtc)
                yield break;

            var endUtc = startUtc + duration;
            var key = RecurrenceKeys.FromUtc(startUtc);

            if (exDates.Contains(key))
                continue;

            if (overrides.TryGetValue(key, out var ovr))
            {
                if (ovr.IsCancelled)
                    continue;

                var ovrStart = ovr.NewStartUtc ?? startUtc;
                var ovrEnd = ovr.NewEndUtc ?? ovrStart + duration;
                if (ovrStart < fromUtc || ovrStart >= toUtc)
                    continue;
                yield return new Occurrence(ovrStart, ovrEnd, key, IsOverride: true);
                continue;
            }

            if (startUtc < fromUtc)
                continue;

            yield return new Occurrence(startUtc, endUtc, key, IsOverride: false);
        }
    }

    private static IEnumerable<Occurrence> EmitOne(
        CalendarEvent @event, DateTime startUtc,
        IReadOnlyDictionary<string, EventOccurrenceOverride> overrides, HashSet<string> exDates,
        DateTime fromUtc, DateTime toUtc)
    {
        startUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
        var key = RecurrenceKeys.FromUtc(startUtc);
        if (exDates.Contains(key))
            yield break;

        var endUtc = @event.EndUtc;
        var isOverride = false;
        if (overrides.TryGetValue(key, out var ovr))
        {
            if (ovr.IsCancelled)
                yield break;
            startUtc = ovr.NewStartUtc ?? startUtc;
            endUtc = ovr.NewEndUtc ?? endUtc;
            isOverride = true;
        }

        if (startUtc < toUtc && endUtc >= fromUtc)
            yield return new Occurrence(startUtc, endUtc, key, isOverride);
    }

    private static IEnumerable<DateTime> GenerateLocalStarts(DateTime localStart, RRuleParser.Rule rule)
    {
        var interval = rule.Interval;
        switch (rule.Frequency)
        {
            case RRuleParser.Freq.Daily:
                for (var d = localStart; ; d = d.AddDays(interval))
                    yield return d;

            case RRuleParser.Freq.Weekly:
            {
                var days = rule.ByDay.Count > 0 ? rule.ByDay : new List<DayOfWeek> { localStart.DayOfWeek };
                var ordered = days.Distinct().OrderBy(WeekOrder).ToList();
                var weekStart = StartOfWeek(localStart);
                for (var w = weekStart; ; w = w.AddDays(7 * interval))
                    foreach (var dow in ordered)
                    {
                        var day = w.AddDays((WeekOrder(dow)));
                        if (day < localStart)
                            continue;
                        yield return day;
                    }
            }

            case RRuleParser.Freq.Monthly:
            {
                var monthDays = rule.ByMonthDay.Count > 0 ? rule.ByMonthDay : new List<int> { localStart.Day };
                var ordered = monthDays.Distinct().OrderBy(x => x).ToList();
                var monthAnchor = new DateTime(localStart.Year, localStart.Month, 1,
                    localStart.Hour, localStart.Minute, localStart.Second, DateTimeKind.Unspecified);
                for (var m = monthAnchor; ; m = m.AddMonths(interval))
                    foreach (var md in ordered)
                    {
                        if (md < 1 || md > DateTime.DaysInMonth(m.Year, m.Month))
                            continue;
                        var date = new DateTime(m.Year, m.Month, md,
                            localStart.Hour, localStart.Minute, localStart.Second, DateTimeKind.Unspecified);
                        if (date < localStart)
                            continue;
                        yield return date;
                    }
            }

            case RRuleParser.Freq.Yearly:
            {
                // BYMONTHDAY раскрывается внутри месяца события (BYMONTH движок не поддерживает),
                // шаг — год * INTERVAL. Без BYMONTHDAY — годовщина исходной даты.
                var monthDays = rule.ByMonthDay.Count > 0 ? rule.ByMonthDay : new List<int> { localStart.Day };
                var ordered = monthDays.Distinct().OrderBy(x => x).ToList();
                for (var year = localStart.Year; ; year += interval)
                    foreach (var md in ordered)
                    {
                        if (md < 1 || md > DateTime.DaysInMonth(year, localStart.Month))
                            continue;
                        var date = new DateTime(year, localStart.Month, md,
                            localStart.Hour, localStart.Minute, localStart.Second, DateTimeKind.Unspecified);
                        if (date < localStart)
                            continue;
                        yield return date;
                    }
            }

            default:
                yield break;
        }
    }

    /// <summary>
    /// Перевод локального времени экземпляра в UTC. Если время попадает в «дыру» весеннего
    /// перехода (несуществующее локальное время), сдвигаем вперёд до ближайшего валидного — как
    /// делает Google Calendar, — иначе <see cref="TimeZoneInfo.ConvertTimeToUtc(DateTime, TimeZoneInfo)"/>
    /// бросает исключение и роняет всё раскрытие серии. Неоднозначное время осеннего перехода
    /// конвертер разрешает сам (стандартное смещение) и не бросает.
    /// </summary>
    private static DateTime ToUtcSafe(DateTime local, TimeZoneInfo tz)
    {
        var probe = local;
        // Шаг 30 минут покрывает и часовые, и получасовые переходы; «дыра» конечна — цикл завершится.
        while (tz.IsInvalidTime(probe))
            probe = probe.AddMinutes(30);
        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(probe, tz), DateTimeKind.Utc);
    }

    // iCal default WKST=MO: порядок дней внутри недели от понедельника.
    private static int WeekOrder(DayOfWeek d) => ((int)d + 6) % 7;

    private static DateTime StartOfWeek(DateTime dt)
        => dt.AddDays(-WeekOrder(dt.DayOfWeek));

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
