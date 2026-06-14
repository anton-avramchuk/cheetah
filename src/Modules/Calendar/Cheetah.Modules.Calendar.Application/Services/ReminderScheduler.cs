using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Calendar.Application.Abstractions;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Domain.Specifications;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Application.Services;

/// <inheritdoc cref="IReminderScheduler"/>
[Export(LifetimeType.Scoped, typeof(IReminderScheduler))]
public sealed class ReminderScheduler : IReminderScheduler
{
    private readonly IReminderTriggerRepository _triggers;
    private readonly IRecurrenceExpander _expander;

    public ReminderScheduler(IReminderTriggerRepository triggers, IRecurrenceExpander expander)
    {
        _triggers = triggers;
        _expander = expander;
    }

    public async ValueTask RebuildAsync(CalendarEvent @event, DateTime horizonEndUtc, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Текущие ожидающие срабатывания события — основа для idempotent upsert.
        var existing = await _triggers.GetAllAsync(new PendingTriggersByEventSpecification(@event.Id), cancellationToken);
        var existingByKey = existing.ToDictionary(t => (t.ReminderId, t.OccurrenceKey));

        // Событие отменено или без напоминаний — гасим всё ожидающее.
        if (@event.Status == EventStatus.Cancelled || @event.Reminders.Count == 0)
        {
            foreach (var t in existing)
                t.Cancel();
            return;
        }

        var desired = new HashSet<(Guid ReminderId, string OccurrenceKey)>();

        foreach (var occ in _expander.Expand(@event, now, horizonEndUtc))
        {
            foreach (var reminder in @event.Reminders)
            {
                var fireAt = occ.StartUtc - reminder.OffsetBeforeStart;
                if (fireAt < now)
                    continue; // момент уже прошёл — не плодим просроченные

                var key = (reminder.Id, occ.OccurrenceKey);
                desired.Add(key);

                if (!existingByKey.ContainsKey(key))
                    _triggers.Add(ReminderTrigger.Create(@event.Id, reminder.Id, occ.OccurrenceKey, fireAt));
            }
        }

        // Ожидающие, которые больше не нужны (напоминание удалено / экземпляр отменён) — отменяем.
        foreach (var t in existing)
            if (!desired.Contains((t.ReminderId, t.OccurrenceKey)))
                t.Cancel();
    }
}
