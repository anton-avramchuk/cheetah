using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Shared;

namespace Cheetah.Modules.Notification.Domain.Abstractions;

/// <summary>
/// Решает, по каким каналам и в каком порядке доставлять уведомление. Это «данные + политика»
/// (категория, предпочтения, доступные контакты), а НЕ runtime-реестр живых провайдеров —
/// в event-архитектуре подписка канала на своё событие и есть его регистрация.
/// </summary>
public interface IChannelRouter
{
    /// <summary>
    /// Возвращает упорядоченный список каналов (первый — основной, далее фолбэк).
    /// Пустой список = доставлять некуда (нет контактов/подходящих каналов) → Suppressed.
    /// </summary>
    IReadOnlyList<NotificationChannel> Resolve(
        NotificationCategory category,
        RecipientContact? contact,
        NotificationChannel? forceChannel);
}
