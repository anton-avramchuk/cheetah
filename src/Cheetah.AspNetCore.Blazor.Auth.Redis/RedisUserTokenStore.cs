using Cheetah.AspNetCore.Blazor.Auth;
using Cheetah.AspNetCore.Blazor.Auth.Abstractions;
using Cheetah.AspNetCore.Blazor.Auth.Tokens;
using Cheetah.Backend.Redis;
using Microsoft.Extensions.Options;

namespace Cheetah.AspNetCore.Blazor.Auth.Redis;

/// <summary>
/// Redis-реализация <see cref="IUserTokenStore"/>: токены сессии хранятся под ключом
/// <c>{KeyPrefix}{sessionId}</c> с TTL (запись истекает сама — отдельной очистки не нужно).
/// Подходит для нескольких инстансов BFF. Заменяет дефолтный in-memory стор.
/// </summary>
public sealed class RedisUserTokenStore(
    IRedisClient redis,
    IOptions<RedisUserTokenStoreOptions> options,
    IOptions<BffAuthOptions> bffOptions) : IUserTokenStore
{
    private readonly RedisUserTokenStoreOptions _options = options.Value;
    private readonly TimeSpan _ttl = options.Value.Ttl ?? bffOptions.Value.ExpireTimeSpan;

    private string Key(string sessionId) => _options.KeyPrefix + sessionId;

    public async ValueTask StoreAsync(string sessionId, BffTokenSet tokens, CancellationToken ct = default)
        => await redis.SetAsync(Key(sessionId), tokens, _ttl, _options.InstanceName, ct);

    public async ValueTask<BffTokenSet?> GetAsync(string sessionId, CancellationToken ct = default)
        => await redis.GetAsync<BffTokenSet>(Key(sessionId), _options.InstanceName, ct);

    public async ValueTask RemoveAsync(string sessionId, CancellationToken ct = default)
        => await redis.DeleteAsync(Key(sessionId), _options.InstanceName, ct);
}
