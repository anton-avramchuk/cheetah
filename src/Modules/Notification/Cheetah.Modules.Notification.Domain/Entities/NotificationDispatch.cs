using Cheetah.Core.Domain;
using Cheetah.Modules.Notification.Shared;

namespace Cheetah.Modules.Notification.Domain.Entities;

/// <summary>
/// Попытка доставки уведомления по конкретному каналу. Живёт собственным репозиторием
/// (как TagAssignment в Tags): канальные статус-события (EmailDelivered/Failed) адресуют
/// её по <see cref="Entity{TId}.Id"/> = DispatchId, не загружая весь агрегат.
/// </summary>
public class NotificationDispatch : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid NotificationId { get; private set; }
    public string Channel { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public string? ProviderMessageId { get; private set; }
    public string? Error { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private NotificationDispatch() { } // EF

    public static NotificationDispatch Create(Guid notificationId, NotificationChannel channel)
        => new()
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            Channel = channel.ToString(),
            Status = DispatchStatus.Pending.ToString()
        };

    public void MarkSent(string? providerMessageId)
    {
        Status = DispatchStatus.Sent.ToString();
        ProviderMessageId = providerMessageId;
        Error = null;
    }

    public void MarkDelivered(string? providerMessageId)
    {
        Status = DispatchStatus.Delivered.ToString();
        if (providerMessageId is not null)
            ProviderMessageId = providerMessageId;
        Error = null;
    }

    public void MarkFailed(string? error)
    {
        Status = DispatchStatus.Failed.ToString();
        Error = error;
    }

    public void MarkBounced(string? error)
    {
        Status = DispatchStatus.Bounced.ToString();
        Error = error;
    }

    public void MarkSuppressed(string reason)
    {
        Status = DispatchStatus.Suppressed.ToString();
        Error = reason;
    }

    /// <summary>Достигнут ли терминальный статус (для идемпотентной обработки статус-событий).</summary>
    public bool IsTerminal =>
        Status == DispatchStatus.Delivered.ToString()
        || Status == DispatchStatus.Failed.ToString()
        || Status == DispatchStatus.Bounced.ToString()
        || Status == DispatchStatus.Suppressed.ToString();
}
