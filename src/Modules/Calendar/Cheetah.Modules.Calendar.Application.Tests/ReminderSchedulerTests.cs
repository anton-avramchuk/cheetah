using Cheetah.Modules.Calendar.Application.Services;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Shared;
using Shouldly;

namespace Cheetah.Modules.Calendar.Application.Tests;

public class ReminderSchedulerTests
{
    private static CalendarEvent EventWithReminder(TimeSpan offset, out Guid reminderId)
    {
        var start = DateTime.UtcNow.AddHours(2);
        var e = CalendarEvent.Schedule(Guid.NewGuid(), "E", start, start.AddHours(1), Guid.NewGuid(), timeZoneId: "UTC");
        var reminder = e.AddReminder(offset, ReminderTarget.AllAttendees, null);
        reminderId = reminder.Id;
        return e;
    }

    [Fact]
    public async Task Rebuild_CreatesTriggerForFutureOccurrence_AndSkipsPastFireAt()
    {
        var now = DateTime.UtcNow;
        var e = EventWithReminder(TimeSpan.FromMinutes(15), out _);

        // A: срабатывание в будущем (создаём); B: fireAt уже прошёл (пропускаем).
        var occurrences = new[]
        {
            new Occurrence(now.AddHours(1), now.AddHours(1).AddMinutes(30), "A", false),
            new Occurrence(now.AddMinutes(5), now.AddMinutes(35), "B", false),
        };

        var triggers = new FakeTriggerRepository();
        var scheduler = new ReminderScheduler(triggers, new FakeExpander(occurrences));

        await scheduler.RebuildAsync(e, now.AddDays(60));

        triggers.Items.Count.ShouldBe(1);
        triggers.Items[0].OccurrenceKey.ShouldBe("A");
        triggers.Items[0].Status.ShouldBe(ReminderTriggerStatus.Pending);
    }

    [Fact]
    public async Task Rebuild_CancelledEvent_CancelsExistingPendingTriggers()
    {
        var e = EventWithReminder(TimeSpan.FromMinutes(15), out var reminderId);
        var existing = ReminderTrigger.Create(e.Id, reminderId, "A", DateTime.UtcNow.AddHours(1));
        var triggers = new FakeTriggerRepository();
        triggers.Add(existing);

        e.Cancel();
        var scheduler = new ReminderScheduler(triggers, new FakeExpander(Array.Empty<Occurrence>()));

        await scheduler.RebuildAsync(e, DateTime.UtcNow.AddDays(60));

        existing.Status.ShouldBe(ReminderTriggerStatus.Cancelled);
    }

    [Fact]
    public async Task Rebuild_IsIdempotent_NoDuplicateTriggers()
    {
        var now = DateTime.UtcNow;
        var e = EventWithReminder(TimeSpan.FromMinutes(15), out _);
        var occurrences = new[] { new Occurrence(now.AddHours(1), now.AddHours(1).AddMinutes(30), "A", false) };
        var triggers = new FakeTriggerRepository();
        var scheduler = new ReminderScheduler(triggers, new FakeExpander(occurrences));

        await scheduler.RebuildAsync(e, now.AddDays(60));
        await scheduler.RebuildAsync(e, now.AddDays(60));

        triggers.Items.Count(t => t.Status == ReminderTriggerStatus.Pending).ShouldBe(1);
    }
}
