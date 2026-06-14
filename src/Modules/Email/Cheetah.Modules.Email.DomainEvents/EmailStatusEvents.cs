using Cheetah.Core.Events;

namespace Cheetah.Modules.Email.DomainEvents;

/// <summary>Письмо принято шлюзом (или подтверждён delivery вебхуком). Publish: Email.</summary>
public record EmailDelivered(
    Guid DispatchId,
    Guid NotificationId,
    string ProviderMessageId,
    DateTimeOffset DeliveredAt) : EventBase;

/// <summary>
/// Окончательная неудача отправки (шлюз отверг адрес, исчерпаны ретраи). Publish: Email.
/// Это БИЗНЕС-сбой (не транспортный DLQ): Notification переводит Dispatch в Failed и,
/// по политике, запускает фолбэк на другой канал.
/// </summary>
public record EmailFailed(
    Guid DispatchId,
    Guid NotificationId,
    EmailFailureReason Reason,
    bool IsPermanent,
    string? Detail) : EventBase;

/// <summary>Bounce/complaint из вебхука провайдера. Publish: Email. Обязателен к обработке.</summary>
public record EmailBounced(
    Guid DispatchId,
    string ToAddress,
    BounceType Type,
    DateTimeOffset BouncedAt) : EventBase;

/// <summary>Причина неудачи отправки письма.</summary>
public enum EmailFailureReason
{
    InvalidAddress,
    Rejected,
    Throttled,
    GatewayError
}

/// <summary>Тип отбоя (bounce) письма.</summary>
public enum BounceType
{
    Hard,
    Soft,
    Complaint
}
