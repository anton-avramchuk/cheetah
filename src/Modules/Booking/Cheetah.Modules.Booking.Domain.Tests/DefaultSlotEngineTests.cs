using Cheetah.Modules.Booking.Domain.Slots;
using Shouldly;

namespace Cheetah.Modules.Booking.Domain.Tests;

public class DefaultSlotEngineTests
{
    private static readonly ISlotEngine Engine = new DefaultSlotEngine();

    // 2026-01-05 — понедельник; работаем в UTC, окно по умолчанию Пн 09:00–12:00.
    private static readonly DateTimeOffset Now = new(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DayFrom = new(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DayTo = new(2026, 1, 6, 0, 0, 0, TimeSpan.Zero);

    private static DateTimeOffset At(int hour, int minute = 0) => new(2026, 1, 5, hour, minute, 0, TimeSpan.Zero);

    private static TestSchedule MondayNineToTwelve(string tz = "UTC")
    {
        var s = TestSchedule.New(tz);
        s.AddWeeklyRule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0));
        return s;
    }

    [Fact]
    public void WeeklyWindow_ProducesHourlySlots()
    {
        var type = TestBookingType.New(durationMinutes: 60);
        var slots = Engine.ComputeSlots(type, MondayNineToTwelve(), Array.Empty<BusyInterval>(), DayFrom, DayTo, Now);

        slots.Count.ShouldBe(3);
        slots[0].StartUtc.ShouldBe(At(9));
        slots[0].EndUtc.ShouldBe(At(10));
        slots[2].StartUtc.ShouldBe(At(11));
    }

    [Fact]
    public void BusyInterval_BlocksOverlappingSlot()
    {
        var type = TestBookingType.New(durationMinutes: 60);
        var busy = new[] { new BusyInterval(At(10), At(10, 30)) };
        var slots = Engine.ComputeSlots(type, MondayNineToTwelve(), busy, DayFrom, DayTo, Now);

        slots.Count.ShouldBe(2);
        slots.ShouldNotContain(s => s.StartUtc == At(10));
        slots[0].StartUtc.ShouldBe(At(9));
        slots[1].StartUtc.ShouldBe(At(11));
    }

    [Fact]
    public void BufferAfter_BlocksSlotAdjacentToBusy()
    {
        // Занятость сразу после окна. Без буфера слот 11:00–12:00 свободен; с буфером после — занят.
        var busy = new[] { new BusyInterval(At(12), At(12, 30)) };

        var noBuffer = Engine.ComputeSlots(
            TestBookingType.New(durationMinutes: 60), MondayNineToTwelve(), busy, DayFrom, DayTo, Now);
        noBuffer.Count.ShouldBe(3);

        var withBuffer = Engine.ComputeSlots(
            TestBookingType.New(durationMinutes: 60, bufferAfter: 15), MondayNineToTwelve(), busy, DayFrom, DayTo, Now);
        withBuffer.Count.ShouldBe(2);
        withBuffer.ShouldNotContain(s => s.StartUtc == At(11));
    }

    [Fact]
    public void MinNotice_TrimsTooEarlySlots()
    {
        var now = At(9, 30);                       // сейчас 09:30
        var type = TestBookingType.New(durationMinutes: 60, minNoticeMinutes: 60); // earliest = 10:30
        var slots = Engine.ComputeSlots(type, MondayNineToTwelve(), Array.Empty<BusyInterval>(), DayFrom, DayTo, now);

        slots.ShouldHaveSingleItem();
        slots[0].StartUtc.ShouldBe(At(11)); // 09:00 и 10:00 отсечены min-notice
    }

    [Fact]
    public void MaxAdvance_LimitsHorizon()
    {
        var rangeTo = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);
        var type = TestBookingType.New(durationMinutes: 60, maxAdvanceDays: 3); // горизонт до 08.01
        var slots = Engine.ComputeSlots(type, MondayNineToTwelve(), Array.Empty<BusyInterval>(), DayFrom, rangeTo, Now);

        slots.Count.ShouldBe(3);
        slots.ShouldAllBe(s => s.StartUtc.Date == new DateTime(2026, 1, 5)); // только понедельник 05.01
    }

    [Fact]
    public void DateOverride_Unavailable_BlocksWholeDay()
    {
        var schedule = MondayNineToTwelve();
        schedule.AddDateOverride(new DateOnly(2026, 1, 5), isUnavailable: true, null, null);

        var slots = Engine.ComputeSlots(
            TestBookingType.New(durationMinutes: 60), schedule, Array.Empty<BusyInterval>(), DayFrom, DayTo, Now);

        slots.ShouldBeEmpty();
    }

    [Fact]
    public void DateOverride_CustomWindow_ReplacesWeeklyRule()
    {
        var schedule = MondayNineToTwelve();
        schedule.AddDateOverride(new DateOnly(2026, 1, 5), isUnavailable: false,
            new TimeOnly(14, 0), new TimeOnly(16, 0));

        var slots = Engine.ComputeSlots(
            TestBookingType.New(durationMinutes: 60), schedule, Array.Empty<BusyInterval>(), DayFrom, DayTo, Now);

        slots.Count.ShouldBe(2);
        slots[0].StartUtc.ShouldBe(At(14));
        slots[1].StartUtc.ShouldBe(At(15));
    }

    [Fact]
    public void SlotStep_ControlsGranularity()
    {
        var schedule = TestSchedule.New();
        schedule.AddWeeklyRule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0));
        var type = TestBookingType.New(durationMinutes: 60, slotStep: 30);

        var slots = Engine.ComputeSlots(type, schedule, Array.Empty<BusyInterval>(), DayFrom, DayTo, Now);

        slots.Count.ShouldBe(3); // 09:00, 09:30, 10:00 (10:30+60 выходит за 11:00)
        slots[1].StartUtc.ShouldBe(At(9, 30));
    }

    [Fact]
    public void CollapsedWindow_ReturnsEmpty()
    {
        var rangeFrom = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var rangeTo = new DateTimeOffset(2026, 2, 2, 0, 0, 0, TimeSpan.Zero);
        var type = TestBookingType.New(durationMinutes: 60, maxAdvanceDays: 5); // горизонт до 10.01 < окна

        var slots = Engine.ComputeSlots(type, MondayNineToTwelve(), Array.Empty<BusyInterval>(), rangeFrom, rangeTo, Now);

        slots.ShouldBeEmpty();
    }

    [Fact]
    public void ScheduleTimeZone_ConvertedToUtc()
    {
        // Окно задано в UTC+2 (Etc/GMT-2): локальное Пн 09:00 = 07:00 UTC.
        var schedule = TestSchedule.New("Etc/GMT-2");
        schedule.AddWeeklyRule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0));
        var type = TestBookingType.New(durationMinutes: 60);

        var slots = Engine.ComputeSlots(type, schedule, Array.Empty<BusyInterval>(), DayFrom, DayTo, Now);

        slots.ShouldHaveSingleItem();
        slots[0].StartUtc.ShouldBe(At(7));
        slots[0].EndUtc.ShouldBe(At(8));
    }
}
