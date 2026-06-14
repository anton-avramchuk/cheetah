using Cheetah.Core.Domain;

namespace Cheetah.Modules.Notification.Domain.Entities;

/// <summary>
/// Одно логическое уведомление (намерение «уведомить пользователя X шаблоном Y»),
/// которое веером раскладывается в попытки доставки <see cref="NotificationDispatch"/> по каналам.
///
/// <para>Назван <c>NotificationMessage</c>, а не <c>Notification</c>, чтобы тип не конфликтовал
/// с сегментом неймспейса <c>Cheetah.Modules.Notification</c>.</para>
///
/// <para><see cref="Entity{TId}.Id"/> = NotificationId из события — это и есть ключ идемпотентности:
/// повторная доставка NotificationRequested отбрасывается по наличию агрегата с таким Id.</para>
/// </summary>
public class NotificationMessage : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid RecipientUserId { get; private set; }
    public string TemplateKey { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string? CorrelationId { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private NotificationMessage() { } // EF

    public static NotificationMessage Create(
        Guid notificationId,
        Guid recipientUserId,
        string templateKey,
        string category,
        string? correlationId)
    {
        if (notificationId == Guid.Empty)
            throw new ArgumentException("NotificationId cannot be empty", nameof(notificationId));
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        return new NotificationMessage
        {
            Id = notificationId,
            RecipientUserId = recipientUserId,
            TemplateKey = templateKey,
            Category = category,
            CorrelationId = correlationId
        };
    }
}
