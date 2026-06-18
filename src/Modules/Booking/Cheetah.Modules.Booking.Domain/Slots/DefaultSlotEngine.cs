using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Booking.Domain.Entities;

namespace Cheetah.Modules.Booking.Domain.Slots;

/// <summary>
/// Дефолтная реализация <see cref="ISlotEngine"/>. Перебирает календарные даты в таймзоне расписания,
/// внутри дневных окон доступности нарезает слоты с шагом <see cref="BookingTypeBase.EffectiveSlotStepMinutes"/>
/// и оставляет те, что попадают в окно <c>[from, to)</c> и чей расширенный буферами интервал не
/// пересекается с занятостью. Чистая, без I/O.
/// </summary>
[Export(LifetimeType.Singleton, typeof(ISlotEngine))]
public sealed class DefaultSlotEngine : ISlotEngine
{
    public IReadOnlyList<Slot> ComputeSlots(
        BookingTypeBase type,
        AvailabilityScheduleBase schedule,
        IReadOnlyList<BusyInterval> busy,
        DateTimeOffset rangeFromUtc,
        DateTimeOffset rangeToUtc,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(schedule);

        var earliest = nowUtc.AddMinutes(type.MinNoticeMinutes);
        var latest = nowUtc.AddDays(type.MaxAdvanceDays);

        var windowStart = Max(rangeFromUtc, earliest);
        var windowEnd = Min(rangeToUtc, latest);
        if (windowEnd <= windowStart)
            return Array.Empty<Slot>();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZoneId);
        var duration = TimeSpan.FromMinutes(type.DurationMinutes);
        var step = TimeSpan.FromMinutes(type.EffectiveSlotStepMinutes);
        var bufferBefore = TimeSpan.FromMinutes(type.BufferBeforeMinutes);
        var bufferAfter = TimeSpan.FromMinutes(type.BufferAfterMinutes);

        var localFrom = TimeZoneInfo.ConvertTime(windowStart, tz);
        var localTo = TimeZoneInfo.ConvertTime(windowEnd, tz);

        var slots = new List<Slot>();

        for (var date = DateOnly.FromDateTime(localFrom.DateTime);
             date <= DateOnly.FromDateTime(localTo.DateTime);
             date = date.AddDays(1))
        {
            foreach (var (winStart, winEnd) in ResolveDayWindows(schedule, date))
            {
                var dayStart = date.ToDateTime(winStart, DateTimeKind.Unspecified);
                var dayEnd = date.ToDateTime(winEnd, DateTimeKind.Unspecified);

                for (var localStart = dayStart; localStart + duration <= dayEnd; localStart += step)
                {
                    if (tz.IsInvalidTime(localStart))
                        continue; // несуществующее локальное время (переход на летнее время)

                    var slotStartUtc = new DateTimeOffset(localStart, tz.GetUtcOffset(localStart));
                    var slotEndUtc = slotStartUtc + duration;

                    if (slotStartUtc < windowStart || slotEndUtc > windowEnd)
                        continue;

                    if (OverlapsBusy(slotStartUtc - bufferBefore, slotEndUtc + bufferAfter, busy))
                        continue;

                    slots.Add(new Slot(slotStartUtc, slotEndUtc));
                }
            }
        }

        slots.Sort((a, b) => a.StartUtc.CompareTo(b.StartUtc));
        return slots;
    }

    /// <summary>
    /// Дневные окна доступности: исключение на дату перекрывает недельное правило (целиком закрывает
    /// день или задаёт особое окно); иначе — окна недельного правила по дню недели.
    /// </summary>
    private static IEnumerable<(TimeOnly Start, TimeOnly End)> ResolveDayWindows(
        AvailabilityScheduleBase schedule, DateOnly date)
    {
        var ovr = schedule.DateOverrides.FirstOrDefault(o => o.Date == date);
        if (ovr is not null)
        {
            if (ovr.IsUnavailable || ovr.StartTime is null || ovr.EndTime is null)
                yield break;

            yield return (ovr.StartTime.Value, ovr.EndTime.Value);
            yield break;
        }

        foreach (var rule in schedule.WeeklyRules)
            if (rule.DayOfWeek == date.DayOfWeek)
                yield return (rule.StartTime, rule.EndTime);
    }

    private static bool OverlapsBusy(DateTimeOffset start, DateTimeOffset end, IReadOnlyList<BusyInterval> busy)
    {
        for (var i = 0; i < busy.Count; i++)
            if (busy[i].StartUtc < end && busy[i].EndUtc > start)
                return true;
        return false;
    }

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;
    private static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
