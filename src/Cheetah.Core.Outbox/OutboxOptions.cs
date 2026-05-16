namespace Cheetah.Core.Outbox;

public class OutboxOptions
{
    /// <summary>
    /// Размер batch'а при чтении из outbox.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Интервал между опросами outbox-таблицы.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Максимальное число retry-попыток. По достижении сообщение откладывается, пока его не обработают вручную.
    /// </summary>
    public int MaxRetries { get; set; } = 10;

    /// <summary>
    /// Базовая задержка для экспоненциального backoff: delay = BaseRetryDelay * 2^RetryCount.
    /// </summary>
    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Верхний предел задержки между попытками.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(10);
}
