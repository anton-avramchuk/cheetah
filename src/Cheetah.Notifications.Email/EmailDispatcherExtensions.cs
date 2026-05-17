namespace Cheetah.Notifications.Email;

public static class EmailDispatcherExtensions
{
    public static ValueTask SendEmailAsync(this INotificationDispatcher dispatcher, EmailMessage message, CancellationToken cancellationToken = default)
        => dispatcher.SendAsync(message, cancellationToken);
}
