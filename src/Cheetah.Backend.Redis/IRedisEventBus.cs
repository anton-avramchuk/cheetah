namespace Cheetah.Backend.Redis;

public interface IRedisEventBus
{
    Task PublishAsync<TEvent>(string channel, TEvent @event, string instanceName = "default", CancellationToken ct = default);

    Task SubscribeAsync<TEvent>(string channel, Func<TEvent, Task> handler, string instanceName = "default", CancellationToken ct = default);

    Task UnsubscribeAsync(string channel, string instanceName = "default", CancellationToken ct = default);
}
