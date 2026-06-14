using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Notification.Domain.Abstractions;
using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Shared;

namespace Cheetah.Modules.Notification.Application.Services;

/// <summary>
/// Маршрутизация по категории + доступным контактам. Канал включается в план только если
/// у получателя есть нужный контакт (email/phone/token). Первый канал — основной, далее — фолбэк.
///
/// В реальной системе сюда добавятся пользовательские preferences, opt-out и quiet hours —
/// см. follow-up. Liveness провайдеров здесь НЕ участвует (это не реестр).
/// </summary>
[Export(LifetimeType.Scoped, typeof(IChannelRouter))]
public sealed class DefaultChannelRouter : IChannelRouter
{
    public IReadOnlyList<NotificationChannel> Resolve(
        NotificationCategory category,
        RecipientContact? contact,
        NotificationChannel? forceChannel)
    {
        if (contact is null)
            return Array.Empty<NotificationChannel>();

        if (forceChannel is { } forced)
            return Supports(contact, forced) ? new[] { forced } : Array.Empty<NotificationChannel>();

        // Предпочтительный порядок каналов по категории
        var preference = category switch
        {
            NotificationCategory.Otp => new[] { NotificationChannel.Sms, NotificationChannel.Push, NotificationChannel.Email },
            NotificationCategory.Transactional => new[] { NotificationChannel.Email, NotificationChannel.Sms },
            NotificationCategory.Marketing => new[] { NotificationChannel.Email },
            _ => new[] { NotificationChannel.Email, NotificationChannel.Push }
        };

        return preference.Where(c => Supports(contact, c)).ToArray();
    }

    private static bool Supports(RecipientContact contact, NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => !string.IsNullOrWhiteSpace(contact.Email),
        NotificationChannel.Sms => !string.IsNullOrWhiteSpace(contact.Phone),
        NotificationChannel.Push => !string.IsNullOrWhiteSpace(contact.PushToken),
        _ => false
    };
}
