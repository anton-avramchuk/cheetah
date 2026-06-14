using System.Security.Cryptography;
using System.Text;
using Cheetah.BackgroundTasks;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.DistributedLock;
using Cheetah.Modules.Calendar.Application.Options;
using Cheetah.Modules.Calendar.Domain;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Shared;
using Cheetah.Modules.Notification.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Calendar.Application.BackgroundTasks;

/// <summary>
/// Скан «пора слать»: выбирает наступившие <see cref="ReminderTrigger"/>, резолвит получателей
/// по <c>Target</c> и публикует <see cref="NotificationRequested"/> на каждого. Под distributed-lock
/// (единичный исполнитель в кластере). Публикация ДО SaveChanges → OutboxEventBus кладёт событие в
/// OutboxMessages того же DbContext: один commit фиксирует гашение триггера и outbox-строку атомарно.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
public sealed class DispatchDueRemindersTask : PeriodicBackgroundTask
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CalendarReminderOptions _options;
    private readonly ILogger<DispatchDueRemindersTask> _logger;

    public DispatchDueRemindersTask(
        IServiceScopeFactory scopeFactory,
        IOptions<CalendarReminderOptions> options,
        ILogger<DispatchDueRemindersTask> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public override TimeSpan Period => _options.DispatchPeriod;

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        // Лок опционален: если провайдер не подключён — работаем без него (одно-инстансный режим).
        var lockProvider = sp.GetService<IDistributedLockProvider>();
        IDistributedLock? heldLock = null;
        if (lockProvider is not null)
        {
            heldLock = await lockProvider.TryAcquireAsync(_options.DispatchLockKey, cancellationToken);
            if (heldLock is null)
                return; // другой инстанс уже сканирует
        }

        try
        {
            var triggers = sp.GetRequiredService<IReminderTriggerRepository>();
            var events = sp.GetRequiredService<ICalendarEventRepository>();
            var eventBus = sp.GetRequiredService<IEventBus>();

            var now = DateTime.UtcNow;
            var due = await triggers.GetDueAsync(now, _options.DispatchBatchSize, cancellationToken);
            if (due.Count == 0)
                return;

            foreach (var trigger in due)
            {
                var @event = await events.GetWithDetailsAsync(trigger.EventId, cancellationToken);
                if (@event is null || @event.Status == EventStatus.Cancelled)
                {
                    trigger.Skip();
                    continue;
                }

                var reminder = @event.Reminders.FirstOrDefault(r => r.Id == trigger.ReminderId);
                if (reminder is null || IsOccurrenceSuppressed(@event, trigger.OccurrenceKey))
                {
                    trigger.Skip();
                    continue;
                }

                var recipients = ResolveRecipients(@event, reminder.Target);
                var data = BuildData(@event, trigger);

                foreach (var userId in recipients)
                {
                    await eventBus.PublishAsync(
                        new NotificationRequested(
                            NotificationId: DeterministicGuid(trigger.Id, userId),
                            RecipientUserId: userId,
                            TemplateKey: CalendarTemplates.Reminder,
                            Category: CalendarNotificationCategories.System,
                            Data: data,
                            ForceChannel: reminder.ForceChannel),
                        cancellationToken);
                }

                trigger.MarkSent();
            }

            await triggers.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Calendar reminders dispatched: {Count} triggers", due.Count);
        }
        finally
        {
            if (heldLock is not null)
                await heldLock.DisposeAsync();
        }
    }

    private static bool IsOccurrenceSuppressed(CalendarEvent @event, string occurrenceKey)
    {
        if (@event.Overrides.Any(o => o.OccurrenceKey == occurrenceKey && o.IsCancelled))
            return true;
        if (@event.Recurrence is not null
            && @event.Recurrence.ExDatesUtc.Any(d => RecurrenceKeys.FromUtc(d) == occurrenceKey))
            return true;
        return false;
    }

    private static IReadOnlyCollection<Guid> ResolveRecipients(CalendarEvent @event, ReminderTarget target)
        => target switch
        {
            ReminderTarget.Organizer => new[] { @event.OrganizerUserId },
            ReminderTarget.AcceptedAttendees => @event.Attendees
                .Where(a => a.Response == AttendeeResponse.Accepted)
                .Select(a => a.UserId).Distinct().ToArray(),
            _ => @event.Attendees.Select(a => a.UserId).Distinct().ToArray()
        };

    private static Dictionary<string, string> BuildData(CalendarEvent @event, ReminderTrigger trigger)
    {
        RecurrenceKeys.TryParse(trigger.OccurrenceKey, out var occurrenceStart);
        return new Dictionary<string, string>
        {
            ["eventId"] = @event.Id.ToString(),
            ["title"] = @event.Title,
            ["location"] = @event.Location ?? string.Empty,
            ["startUtc"] = occurrenceStart.ToString("O"),
            ["timeZone"] = @event.TimeZoneId,
            ["entityType"] = @event.EntityType ?? string.Empty,
            ["entityId"] = @event.EntityId?.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Детерминированный GUID из (triggerId, userId) — сквозной ключ идемпотентности конвейера:
    /// повтор после краша между publish и commit отбрасывается Notification по NotificationId.
    /// </summary>
    private static Guid DeterministicGuid(Guid triggerId, Guid userId)
    {
        Span<byte> buffer = stackalloc byte[32];
        triggerId.TryWriteBytes(buffer[..16]);
        userId.TryWriteBytes(buffer[16..]);
        Span<byte> hash = stackalloc byte[16];
        MD5.HashData(buffer, hash);
        return new Guid(hash);
    }
}
