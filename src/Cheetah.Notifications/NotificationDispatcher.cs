using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Notifications;

[Export(LifetimeType.Scoped, typeof(INotificationDispatcher))]
public sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IServiceProvider _services;

    public NotificationDispatcher(IServiceProvider services) => _services = services;

    public ValueTask SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        var sender = _services.GetService<INotificationSender<TMessage>>();
        if (sender is null)
        {
            throw new InvalidOperationException(
                $"No INotificationSender<{typeof(TMessage).Name}> registered. " +
                $"Подключите соответствующий канальный модуль (Cheetah.Notifications.Email / Sms / ...).");
        }
        return sender.SendAsync(message, cancellationToken);
    }
}
