using Cheetah.Modules.Booking.Domain.Entities;

namespace Cheetah.Modules.Booking.Domain.Slots;

/// <summary>Занятый интервал host'а (UTC) — приходит из Calendar free/busy.</summary>
public readonly record struct BusyInterval(DateTimeOffset StartUtc, DateTimeOffset EndUtc);

/// <summary>Свободный слот (UTC), пригодный для брони.</summary>
public readonly record struct Slot(DateTimeOffset StartUtc, DateTimeOffset EndUtc);

/// <summary>
/// Ядро модуля: вычисляет свободные слоты как доступность ∩ горизонт ∩ min-notice − занятость −
/// буферы. Чистая функция без I/O — юнит-тестируется без БД. Подменяема наследником (точка
/// расширяемости стратегии сетки слотов).
/// </summary>
public interface ISlotEngine
{
    /// <summary>
    /// Возвращает свободные слоты host'а в окне <c>[rangeFromUtc, rangeToUtc)</c>, отсечённом по
    /// min-notice и горизонту планирования относительно <paramref name="nowUtc"/>. Буферы расширяют
    /// проверяемый интервал занятости, но не сам слот.
    /// </summary>
    IReadOnlyList<Slot> ComputeSlots(
        BookingTypeBase type,
        AvailabilityScheduleBase schedule,
        IReadOnlyList<BusyInterval> busy,
        DateTimeOffset rangeFromUtc,
        DateTimeOffset rangeToUtc,
        DateTimeOffset nowUtc);
}
