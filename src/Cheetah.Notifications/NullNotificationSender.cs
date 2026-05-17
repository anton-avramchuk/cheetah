using Microsoft.Extensions.Logging;

namespace Cheetah.Notifications;

/// <summary>
/// No-op sender для dev/test: только логирует попытку отправки.
/// Зарегистрируйте через services.AddScoped&lt;INotificationSender&lt;TMessage&gt;, NullNotificationSender&lt;TMessage&gt;&gt;()
/// если хотите явно "проглатывать" сообщения в среде, где провайдер недоступен.
/// </summary>
public sealed class NullNotificationSender<TMessage> : INotificationSender<TMessage>
{
    private readonly ILogger<NullNotificationSender<TMessage>> _logger;

    public NullNotificationSender(ILogger<NullNotificationSender<TMessage>> logger) => _logger = logger;

    public ValueTask SendAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("NullNotificationSender: пропущено сообщение типа {Type}: {Message}",
            typeof(TMessage).Name, message);
        return ValueTask.CompletedTask;
    }
}
