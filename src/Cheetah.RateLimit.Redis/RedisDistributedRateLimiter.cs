using Cheetah.Backend.Redis;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Cheetah.RateLimit.Redis;

/// <summary>
/// Distributed sliding-window-counter rate limiter поверх Redis.
///
/// Алгоритм (по образцу Cloudflare):
///   current_window  = floor(now / windowSec)
///   previous_window = current_window - 1
///   elapsed_in_current = (now - current_window * windowSec) / windowSec   // 0..1
///   weighted_count = INCR(current) + GET(previous) * (1 - elapsed_in_current)
/// Если weighted_count > limit — отказ.
/// Точность ≈ ±limit/2 в худшем случае на стыке окон; гораздо лучше fixed window
/// и без памяти на хранение каждого timestamp (как sliding-window-log).
///
/// Атомарность через Lua-скрипт: INCR + EXPIRE + GET previous + проверка лимита
/// выполняются в одной round-trip без race conditions между репликами.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IDistributedRateLimiter))]
public sealed class RedisDistributedRateLimiter : IDistributedRateLimiter
{
    // KEYS[1] = current_window_key, KEYS[2] = previous_window_key
    // ARGV[1] = limit, ARGV[2] = window_seconds, ARGV[3] = elapsed_seconds_in_current_window
    // Возвращает {allowed (0/1), used (after this call), retry_after_ms}
    private const string SlidingWindowLua = @"
local limit = tonumber(ARGV[1])
local window = tonumber(ARGV[2])
local elapsed = tonumber(ARGV[3])

local prev = tonumber(redis.call('GET', KEYS[2]) or '0')
local curr = tonumber(redis.call('GET', KEYS[1]) or '0')

-- Сколько previous-окна ещё ""перетекает"" в текущее
local weight = (window - elapsed) / window
local effective = curr + math.floor(prev * weight + 0.5)

if effective + 1 > limit then
  local retry_ms = math.floor((window - elapsed) * 1000)
  return {0, effective, retry_ms}
end

curr = redis.call('INCR', KEYS[1])
-- TTL = 2*window чтобы previous_window дожил до конца своей значимости
redis.call('EXPIRE', KEYS[1], window * 2)

effective = curr + math.floor(prev * weight + 0.5)
return {1, effective, 0}
";

    private readonly IRedisConnectionProvider _redis;
    private readonly RedisRateLimitOptions _options;

    public RedisDistributedRateLimiter(IRedisConnectionProvider redis, IOptions<RedisRateLimitOptions> options)
    {
        _redis = redis;
        _options = options.Value;
    }

    public ValueTask<RateLimitDecision> AcquireAsync(string policyName, string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Policies.TryGetValue(policyName, out var policy))
            throw new InvalidOperationException($"RateLimit policy '{policyName}' not configured.");
        return AcquireAsync(key, policy.Limit, policy.Window, cancellationToken);
    }

    public async ValueTask<RateLimitDecision> AcquireAsync(string key, long limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit));
        if (window <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(window));

        var db = _redis.GetDatabase(_options.InstanceName);
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowMs = (long)window.TotalMilliseconds;
        var currentWindow = nowMs / windowMs;
        var previousWindow = currentWindow - 1;
        var elapsedInCurrent = (nowMs - currentWindow * windowMs) / 1000.0; // секунды

        var currentKey = $"{_options.KeyPrefix}{key}:{currentWindow}";
        var previousKey = $"{_options.KeyPrefix}{key}:{previousWindow}";

        var result = (RedisValue[])(await db.ScriptEvaluateAsync(
            SlidingWindowLua,
            new RedisKey[] { currentKey, previousKey },
            new RedisValue[] { limit, (long)window.TotalSeconds, elapsedInCurrent }
        ).ConfigureAwait(false))!;

        var allowed = (long)result[0] == 1;
        var used = (long)result[1];
        var retryAfterMs = (long)result[2];

        return new RateLimitDecision(
            IsAllowed: allowed,
            RemainingPermits: Math.Max(0, limit - used),
            Limit: limit,
            RetryAfter: TimeSpan.FromMilliseconds(retryAfterMs));
    }
}
