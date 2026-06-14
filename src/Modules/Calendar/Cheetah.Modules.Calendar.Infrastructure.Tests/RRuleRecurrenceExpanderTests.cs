using Cheetah.Modules.Calendar.Domain;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Domain.ValueObjects;
using Cheetah.Modules.Calendar.Infrastructure.Recurrence;
using Shouldly;

namespace Cheetah.Modules.Calendar.Infrastructure.Tests;

public class RRuleRecurrenceExpanderTests
{
    private readonly RRuleRecurrenceExpander _expander = new();

    // Понедельник 2026-01-05 09:00 UTC.
    private static readonly DateTime Start = new(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime WindowFrom = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime WindowTo = new(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

    private static CalendarEvent NewEvent(RecurrenceRule? rule)
        => CalendarEvent.Schedule(
            Guid.NewGuid(), "Standup", Start, Start.AddMinutes(30), Guid.NewGuid(),
            timeZoneId: "UTC", recurrence: rule);

    [Fact]
    public void NonRecurring_ReturnsSingleOccurrence_InWindow()
    {
        var e = NewEvent(null);
        var occ = _expander.Expand(e, WindowFrom, WindowTo).ToList();
        occ.Count.ShouldBe(1);
        occ[0].StartUtc.ShouldBe(Start);
    }

    [Fact]
    public void NonRecurring_OutsideWindow_ReturnsNothing()
    {
        var e = NewEvent(null);
        var occ = _expander.Expand(e, Start.AddDays(1), WindowTo).ToList();
        occ.ShouldBeEmpty();
    }

    [Fact]
    public void Daily_WithCount_ReturnsExactlyCount()
    {
        var e = NewEvent(new RecurrenceRule("FREQ=DAILY;COUNT=3"));
        var occ = _expander.Expand(e, WindowFrom, WindowTo).ToList();
        occ.Count.ShouldBe(3);
        occ.Select(o => o.StartUtc).ShouldBe(new[] { Start, Start.AddDays(1), Start.AddDays(2) });
    }

    [Fact]
    public void Weekly_ByDay_ExpandsSelectedWeekdays()
    {
        var e = NewEvent(new RecurrenceRule("FREQ=WEEKLY;BYDAY=MO,WE;COUNT=4"));
        var occ = _expander.Expand(e, WindowFrom, WindowTo).ToList();

        occ.Select(o => o.StartUtc).ShouldBe(new[]
        {
            new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc),  // Mon
            new DateTime(2026, 1, 7, 9, 0, 0, DateTimeKind.Utc),  // Wed
            new DateTime(2026, 1, 12, 9, 0, 0, DateTimeKind.Utc), // Mon
            new DateTime(2026, 1, 14, 9, 0, 0, DateTimeKind.Utc), // Wed
        });
    }

    [Fact]
    public void Until_BoundsTheSeries()
    {
        var e = NewEvent(new RecurrenceRule("FREQ=DAILY;UNTIL=20260107T090000Z"));
        var occ = _expander.Expand(e, WindowFrom, WindowTo).ToList();
        occ.Count.ShouldBe(3); // Jan 5, 6, 7 (включительно)
        occ[^1].StartUtc.ShouldBe(new DateTime(2026, 1, 7, 9, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ExDate_RemovesMatchingOccurrence()
    {
        var exDate = Start.AddDays(1); // Jan 6
        var e = NewEvent(new RecurrenceRule("FREQ=DAILY;COUNT=3", new[] { exDate }));
        var occ = _expander.Expand(e, WindowFrom, WindowTo).ToList();
        occ.Select(o => o.StartUtc).ShouldBe(new[] { Start, Start.AddDays(2) });
    }

    [Fact]
    public void Override_Cancellation_SkipsOccurrence()
    {
        var e = NewEvent(new RecurrenceRule("FREQ=DAILY;COUNT=3"));
        e.CancelOccurrence(RecurrenceKeys.FromUtc(Start.AddDays(1)));
        var occ = _expander.Expand(e, WindowFrom, WindowTo).ToList();
        occ.Select(o => o.StartUtc).ShouldBe(new[] { Start, Start.AddDays(2) });
    }

    [Fact]
    public void Override_Modification_MovesOccurrence()
    {
        var e = NewEvent(new RecurrenceRule("FREQ=DAILY;COUNT=3"));
        var movedStart = Start.AddDays(1).AddHours(3);
        e.OverrideOccurrence(RecurrenceKeys.FromUtc(Start.AddDays(1)), movedStart, movedStart.AddMinutes(30), "Moved");

        var occ = _expander.Expand(e, WindowFrom, WindowTo).ToList();
        occ.ShouldContain(o => o.StartUtc == movedStart && o.IsOverride);
        occ.Count.ShouldBe(3);
    }

    [Fact]
    public void Monthly_ByMonthDay_ExpandsAcrossMonths()
    {
        var e = NewEvent(new RecurrenceRule("FREQ=MONTHLY;BYMONTHDAY=5;COUNT=2"));
        var occ = _expander.Expand(e, WindowFrom, WindowTo).ToList();
        occ.Select(o => o.StartUtc).ShouldBe(new[]
        {
            new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc),
        });
    }
}
