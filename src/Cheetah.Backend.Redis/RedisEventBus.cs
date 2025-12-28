using System.Collections.Concurrent;
using System.Text.Json;
using Cheetah.Core.DependencyInjection;
using StackExchange.Redis;

namespace Cheetah.Backend.Redis;

[Export(LifetimeType.Singleton, typeof(IRedisEventBus))]
public sealed class RedisEventBus : IRedisEventBus
{
    private readonly IRedisConnectionProvider _connectionProvider;
    private readonly ConcurrentDictionary<string, List<Action<RedisChannel, RedisValue>>> _subscriptions = new();

    public RedisEventBus(IRedisConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task PublishAsync<TEvent>(string channel, TEvent @event, string instanceName = "default", CancellationToken ct = default)
    {
        var connection = _connectionProvider.GetConnection(instanceName);
        var subscriber = connection.GetSubscriber();
        var serialized = JsonSerializer.Serialize(@event);
        await subscriber.PublishAsync(RedisChannel.Literal(channel), serialized);
    }

    public async Task SubscribeAsync<TEvent>(string channel, Func<TEvent, Task> handler, string instanceName = "default", CancellationToken ct = default)
    {
        var connection = _connectionProvider.GetConnection(instanceName);
        var subscriber = connection.GetSubscriber();

        Action<RedisChannel, RedisValue> redisHandler = async (ch, message) =>
        {
            var @event = JsonSerializer.Deserialize<TEvent>((string)message!);
            if (@event != null)
            {
                await handler(@event);
            }
        };

        var key = $"{instanceName}:{channel}";
        _subscriptions.AddOrUpdate(key,
            new List<Action<RedisChannel, RedisValue>> { redisHandler },
            (_, list) => { list.Add(redisHandler); return list; });

        await subscriber.SubscribeAsync(RedisChannel.Literal(channel), redisHandler);
    }

    public async Task UnsubscribeAsync(string channel, string instanceName = "default", CancellationToken ct = default)
    {
        var connection = _connectionProvider.GetConnection(instanceName);
        var subscriber = connection.GetSubscriber();

        var key = $"{instanceName}:{channel}";
        if (_subscriptions.TryRemove(key, out var handlers))
        {
            foreach (var handler in handlers)
            {
                await subscriber.UnsubscribeAsync(RedisChannel.Literal(channel), handler);
            }
        }
    }
}
