using Confluent.Kafka;

namespace Cheetah.Backend.Events.Kafka;

/// <summary>
/// Фабрика IProducer — отделена ради тестируемости (in-memory подмена).
/// </summary>
public interface IKafkaProducerFactory
{
    IProducer<string, string> Create(KafkaEventBusOptions options);
}

public sealed class DefaultKafkaProducerFactory : IKafkaProducerFactory
{
    public IProducer<string, string> Create(KafkaEventBusOptions options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers,
            ClientId = options.ClientId,
            Acks = options.Acks switch
            {
                "0" => Acks.None,
                "1" => Acks.Leader,
                _ => Acks.All
            },
            EnableIdempotence = true
        };

        if (!string.IsNullOrEmpty(options.SecurityProtocol))
            config.SecurityProtocol = Enum.Parse<SecurityProtocol>(options.SecurityProtocol, ignoreCase: true);
        if (!string.IsNullOrEmpty(options.SaslMechanism))
            config.SaslMechanism = Enum.Parse<SaslMechanism>(options.SaslMechanism.Replace("-", ""), ignoreCase: true);
        if (!string.IsNullOrEmpty(options.SaslUsername)) config.SaslUsername = options.SaslUsername;
        if (!string.IsNullOrEmpty(options.SaslPassword)) config.SaslPassword = options.SaslPassword;

        return new ProducerBuilder<string, string>(config).Build();
    }
}
