using Cheetah.Backend.Redis;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Backend.Events.Redis;

[Export(LifetimeType.Singleton, typeof(IEventBus))]
public sealed class CrmRedisEventBus : IEventBus
{
    private readonly IRedisEventBus _redisEventBus;
    private readonly IServiceProvider _serviceProvider;
    private readonly RedisEventBusOptions _options;
    private readonly ILogger<CrmRedisEventBus> _logger;
    private readonly Dictionary<Type, List<Type>> _subscriptions = new();
    private readonly object _lock = new();

    public CrmRedisEventBus(
        IRedisEventBus redisEventBus,
        IServiceProvider serviceProvider,
        IOptions<RedisEventBusOptions> options,
        ILogger<CrmRedisEventBus> logger)
    {
        _redisEventBus = redisEventBus;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var channel = GetChannelName<TEvent>();
        await _redisEventBus.PublishAsync(channel, @event, _options.InstanceName, cancellationToken);
    }

    public async ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var channel = GetChannelName<TEvent>();
        var tasks = events.Select(e => _redisEventBus.PublishAsync(channel, e, _options.InstanceName, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_lock)
        {
            if (!_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType] = new List<Type>();

                var channel = GetChannelName<TEvent>();

                _redisEventBus.SubscribeAsync<TEvent>(
                    channel,
                    async (@event) => await HandleEventAsync(@event, eventType),
                    _options.InstanceName
                ).GetAwaiter().GetResult();
            }

            if (!_subscriptions[eventType].Contains(handlerType))
            {
                _subscriptions[eventType].Add(handlerType);
            }
        }
    }

    private async Task HandleEventAsync<TEvent>(TEvent @event, Type eventType)
        where TEvent : IEvent
    {
        List<Type> handlerTypes;
        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                return;
            }

            handlerTypes = handlers.ToList();
        }

        using var scope = _serviceProvider.CreateScope();

        foreach (var handlerType in handlerTypes)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler is IEventHandler<TEvent> eventHandler)
            {
                try
                {
                    await eventHandler.HandleAsync(@event, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // Остальные обработчики того же события всё равно отрабатывают: сбой одного
                    // подписчика не должен отменять доставку другим. Но молча терять событие нельзя —
                    // расхождение состояний между модулями иначе не расследовать.
                    _logger.LogError(ex,
                        "Event handler {HandlerType} failed for event {EventType}; the event was dropped.",
                        handlerType.FullName, eventType.FullName);
                }
            }
        }
    }

    private string GetChannelName<TEvent>() where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        return $"{_options.ChannelPrefix}{eventType.Name}";
    }
}
