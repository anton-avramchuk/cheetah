namespace Cheetah.Backend.Events.Kafka;

public class KafkaEventBusOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>Префикс топика; конкретный топик = "{TopicPrefix}{EventTypeName}".</summary>
    public string TopicPrefix { get; set; } = "events.";

    public string ClientId { get; set; } = "cheetah";
    public string Acks { get; set; } = "all";

    // Опциональные SASL/SSL для облачных Kafka.
    public string? SaslUsername { get; set; }
    public string? SaslPassword { get; set; }
    public string? SaslMechanism { get; set; }
    public string? SecurityProtocol { get; set; }
}
