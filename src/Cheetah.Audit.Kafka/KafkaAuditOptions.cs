namespace Cheetah.Audit.Kafka;

/// <summary>
/// Настройки только publisher-цикла. Конфигурация самой Kafka (BootstrapServers, SASL, ...)
/// живёт в KafkaEventBusOptions (Cheetah.Backend.Events.Kafka) — секция "KafkaEventBus".
/// </summary>
public class KafkaAuditOptions
{
    public int BatchSize { get; set; } = 100;
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);
    public int MaxRetries { get; set; } = 10;
    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(10);
}
