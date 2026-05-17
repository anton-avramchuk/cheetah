namespace Cheetah.Notifications.Sms;

public static class SmsDispatcherExtensions
{
    public static ValueTask SendSmsAsync(this INotificationDispatcher dispatcher, SmsMessage message, CancellationToken cancellationToken = default)
        => dispatcher.SendAsync(message, cancellationToken);
}
