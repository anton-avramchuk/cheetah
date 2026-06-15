using Cheetah.Core.Domain;

namespace Cheetah.Modules.Activities.Domain.Entities;

/// <summary>
/// Напоминание о приближении срока активности — дитя агрегата <see cref="ActivityBase"/>.
/// Конкретный (не расширяемый) тип: напоминания редко требуют доменного расширения,
/// поэтому, в отличие от самой активности, базы-наследования здесь нет.
/// </summary>
public sealed class ActivityReminder : Entity<Guid>
{
    public Guid ActivityId { get; private set; }
    public TimeSpan OffsetBeforeDue { get; private set; }
    public string Channel { get; private set; } = null!;
    public bool Sent { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }

    private ActivityReminder() { } // EF

    internal static ActivityReminder Create(Guid activityId, TimeSpan offsetBeforeDue, string channel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channel);
        if (offsetBeforeDue < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(offsetBeforeDue), "Offset must be non-negative.");

        return new ActivityReminder
        {
            Id = Guid.NewGuid(),
            ActivityId = activityId,
            OffsetBeforeDue = offsetBeforeDue,
            Channel = channel.Trim(),
            Sent = false
        };
    }

    public void MarkSent()
    {
        Sent = true;
        SentAt = DateTimeOffset.UtcNow;
    }
}
