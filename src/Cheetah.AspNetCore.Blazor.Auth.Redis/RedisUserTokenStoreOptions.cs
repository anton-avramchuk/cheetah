namespace Cheetah.AspNetCore.Blazor.Auth.Redis;

/// <summary>Настройки Redis-стора токенов (секция <c>BffAuth:Redis</c>).</summary>
public sealed class RedisUserTokenStoreOptions
{
    public const string SectionName = "BffAuth:Redis";

    /// <summary>Префикс ключей в Redis. Итоговый ключ = <c>KeyPrefix + sessionId</c>.</summary>
    public string KeyPrefix { get; set; } = "bff:tokens:";

    /// <summary>Имя инстанса Redis из конфигурации <c>Redis:Instances</c>.</summary>
    public string InstanceName { get; set; } = "default";

    /// <summary>
    /// TTL записи. Если не задано — берётся <see cref="BffAuthOptions.ExpireTimeSpan"/>
    /// (запись живёт столько же, сколько cookie-сессия).
    /// </summary>
    public TimeSpan? Ttl { get; set; }
}
