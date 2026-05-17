namespace Cheetah.Audit.Kafka;

public class KafkaAuditOptions
{
    /// <summary>Список Kafka-брокеров через запятую (например "broker1:9092,broker2:9092").</summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>Топик, в который улетают AuditEntry.</summary>
    public string Topic { get; set; } = "audit.events";

    /// <summary>ClientId — попадает в Kafka logs/метрики для диагностики.</summary>
    public string ClientId { get; set; } = "cheetah-audit";

    /// <summary>Acks: "all" (gw), "1" (только leader), "0" (без подтверждения).</summary>
    public string Acks { get; set; } = "all";

    /// <summary>Сколько сообщений вычитывать за один тик.</summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>Интервал polling если ничего не пришло.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>Максимум retry перед exponential backoff cap.</summary>
    public int MaxRetries { get; set; } = 10;

    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(10);

    // Опциональные SASL/SSL поля для облачных Kafka (Confluent Cloud, Yandex MQ).
    public string? SaslUsername { get; set; }
    public string? SaslPassword { get; set; }
    public string? SaslMechanism { get; set; }       // PLAIN, SCRAM-SHA-256, ...
    public string? SecurityProtocol { get; set; }    // SASL_SSL, SSL, PLAINTEXT
}
