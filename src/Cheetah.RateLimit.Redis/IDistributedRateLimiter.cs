namespace Cheetah.RateLimit.Redis;

/// <summary>
/// Distributed rate limiter — состояние в Redis, разделяется между репликами приложения.
/// Для in-process ограничения используйте BCL System.Threading.RateLimiting напрямую.
/// </summary>
public interface IDistributedRateLimiter
{
    /// <summary>
    /// Попытаться получить permit с дефолтными лимитами политики.
    /// </summary>
    ValueTask<RateLimitDecision> AcquireAsync(string policyName, string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Попытаться получить permit с явными лимитами (override).
    /// </summary>
    ValueTask<RateLimitDecision> AcquireAsync(string key, long limit, TimeSpan window, CancellationToken cancellationToken = default);
}
