using StackExchange.Redis;

namespace Cheetah.Backend.Redis;

public interface IRedisConnectionProvider
{
    IConnectionMultiplexer GetConnection(string instanceName = "default");
    IDatabase GetDatabase(string instanceName = "default");
}
