namespace Cheetah.Core.Outbox;

/// <summary>
/// Источник пробуждения OutboxProcessor'a. Реализация может слушать LISTEN/NOTIFY,
/// очередь в памяти или любой другой механизм.
/// </summary>
public interface IOutboxNotifier
{
    /// <summary>
    /// Ожидает следующего сигнала о появлении новых сообщений в outbox.
    /// Возвращает управление, когда сигнал получен или истёк CancellationToken.
    /// </summary>
    ValueTask WaitForSignalAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Заглушка по умолчанию: никогда не сигналит. OutboxProcessor пробуждается только по polling-таймеру.
/// </summary>
internal sealed class NullOutboxNotifier : IOutboxNotifier
{
    public ValueTask WaitForSignalAsync(CancellationToken cancellationToken)
        => new(Task.Delay(Timeout.Infinite, cancellationToken));
}
