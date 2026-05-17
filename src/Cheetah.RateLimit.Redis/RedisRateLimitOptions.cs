using Cheetah.RateLimit;

namespace Cheetah.RateLimit.Redis;

public class RedisRateLimitOptions
{
    /// <summary>Имя Redis-инстанса (см. Cheetah.Backend.Redis).</summary>
    public string InstanceName { get; set; } = "default";

    /// <summary>Префикс ключей в Redis (избежать коллизий с другими данными).</summary>
    public string KeyPrefix { get; set; } = "rl:";

    /// <summary>
    /// Именованные политики — настраиваются через секцию RateLimit:Policies:&lt;name&gt;.
    /// </summary>
    public Dictionary<string, RateLimitPolicy> Policies { get; set; } = new();
}
