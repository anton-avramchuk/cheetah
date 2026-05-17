using System.Text.Json;
using Cheetah.Core.Events;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Backend.Events.Kafka;

/// <summary>
/// IEventBus поверх Confluent.Kafka. Publish-only: Subscribe не реализован, так как Kafka
/// consumer-group семантика плохо мапится на IEventBus.Subscribe (нужен отдельный hosted-service
/// с consumer group / offset management). Хочешь Kafka-handler — пиши свой BackgroundService.
///
/// Регистрируется как keyed IEventBus("kafka") в CrmBackendEventsKafkaModule.
/// </summary>
public sealed class CrmKafkaEventBus : IEventBus, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IProducer<string, string> _producer;
    private readonly KafkaEventBusOptions _options;
    private readonly ILogger<CrmKafkaEventBus> _logger;

    public CrmKafkaEventBus(
        IKafkaProducerFactory factory,
        IOptions<KafkaEventBusOptions> options,
        ILogger<CrmKafkaEventBus> logger)
    {
        _options = options.Value;
        _logger = logger;
        _producer = factory.Create(_options);
    }

    public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var topic = GetTopic<TEvent>();
        var message = BuildMessage(@event);
        await _producer.ProduceAsync(topic, message, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var topic = GetTopic<TEvent>();
        // Параллельная отправка — Confluent.Kafka producer thread-safe и сам батчит внутри.
        var tasks = new List<Task<DeliveryResult<string, string>>>();
        foreach (var e in events)
        {
            tasks.Add(_producer.ProduceAsync(topic, BuildMessage(e), cancellationToken));
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        throw new NotSupportedException(
            "Kafka event bus is publish-only. Для consumer-сценариев напишите свой BackgroundService " +
            "с явным consumer-group, offset management и партиционированием.");
    }

    private string GetTopic<TEvent>() where TEvent : IEvent
        => $"{_options.TopicPrefix}{typeof(TEvent).Name}";

    private static Message<string, string> BuildMessage<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var json = JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions);
        return new Message<string, string>
        {
            Key = @event.EventId.ToString(),
            Value = json
        };
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
