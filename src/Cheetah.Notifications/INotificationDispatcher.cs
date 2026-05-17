namespace Cheetah.Notifications;

/// <summary>
/// Фасад для прикладного кода: одна точка отправки сообщений любых каналов.
/// Резолвит соответствующий INotificationSender&lt;TMessage&gt; из DI.
/// Удобные типизированные методы (SendEmailAsync, SendSmsAsync) подключаются вместе с каналом
/// как extension methods (см. Cheetah.Notifications.Email / .Sms).
/// </summary>
public interface INotificationDispatcher
{
    ValueTask SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
}
