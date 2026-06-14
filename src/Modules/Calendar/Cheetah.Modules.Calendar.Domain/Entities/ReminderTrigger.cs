using Cheetah.Core.Domain;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Domain.Entities;

/// <summary>
/// Материализованное срабатывание напоминания: «в момент <see cref="FireAtUtc"/> отправить
/// напоминание <see cref="ReminderId"/> по экземпляру <see cref="OccurrenceKey"/>».
/// Отдельный агрегат со своим жизненным циклом — его сканирует и гасит планировщик
/// (горячий запрос по <c>(Status, FireAtUtc)</c>), поэтому не грузим ради этого весь event.
///
/// <para>Уникальность <c>(EventId, ReminderId, OccurrenceKey)</c> защищает от дублей при
/// пересчёте серии.</para>
/// </summary>
public class ReminderTrigger : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid EventId { get; private set; }
    public Guid ReminderId { get; private set; }
    public string OccurrenceKey { get; private set; } = null!;
    public DateTime FireAtUtc { get; private set; }
    public ReminderTriggerStatus Status { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private ReminderTrigger() { } // EF

    public static ReminderTrigger Create(Guid eventId, Guid reminderId, string occurrenceKey, DateTime fireAtUtc)
        => new()
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            ReminderId = reminderId,
            OccurrenceKey = occurrenceKey,
            FireAtUtc = DateTime.SpecifyKind(fireAtUtc, DateTimeKind.Utc),
            Status = ReminderTriggerStatus.Pending
        };

    public void MarkSent()
    {
        Status = ReminderTriggerStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Пропустить (событие/экземпляр отменены к моменту срабатывания).</summary>
    public void Skip() => Status = ReminderTriggerStatus.Skipped;

    /// <summary>Отменить при пересчёте серии (правило/напоминание изменились).</summary>
    public void Cancel() => Status = ReminderTriggerStatus.Cancelled;
}
