namespace Cheetah.Notifications;

/// <summary>
/// Унифицированный sender для конкретного типа сообщения. Каждый канал (email, sms, push)
/// регистрирует свою реализацию INotificationSender&lt;TMessage&gt;.
/// </summary>
public interface INotificationSender<in TMessage>
{
    ValueTask SendAsync(TMessage message, CancellationToken cancellationToken = default);
}
