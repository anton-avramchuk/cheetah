using Cheetah.Core.Events;

namespace Cheetah.Modules.Notification.DomainEvents;

/// <summary>
/// Намерение уведомить пользователя. Публикуется любым модулем-продюсером ПОСЛЕ SaveChangesAsync
/// (через IEventBus). Notification сам решает каналы, резолвит контакты из локальной реплики
/// и рендерит шаблон — поэтому здесь НЕТ PII (email/телефона), только RecipientUserId.
///
/// <para><see cref="NotificationId"/> — сквозной ключ идемпотентности на весь конвейер:
/// Notification по нему отбрасывает повторную доставку события.</para>
/// </summary>
/// <param name="NotificationId">Идемпотентный ключ уведомления (генерирует продюсер).</param>
/// <param name="RecipientUserId">Кого уведомляем; контакты резолвит Notification.</param>
/// <param name="TemplateKey">Ключ шаблона, напр. "order.shipped", "auth.otp".</param>
/// <param name="Category">Категория — влияет на opt-out и unsubscribe-заголовки.</param>
/// <param name="Data">Merge-поля для подстановки в шаблон.</param>
/// <param name="ForceChannel">Жёстко заданный канал (редкий случай); иначе решает роутер.</param>
/// <param name="CorrelationId">Сквозная корреляция для трассировки.</param>
public record NotificationRequested(
    Guid NotificationId,
    Guid RecipientUserId,
    string TemplateKey,
    string Category,
    IReadOnlyDictionary<string, string> Data,
    string? ForceChannel = null,
    string? CorrelationId = null) : EventBase;
