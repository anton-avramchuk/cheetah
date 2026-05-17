namespace Cheetah.RateLimit;

/// <summary>
/// Distributed rate limiter — состояние разделяется между репликами приложения.
/// Конкретные реализации: <c>Cheetah.RateLimit.Redis</c> и т.п.
/// Для in-process ограничения используйте BCL <c>System.Threading.RateLimiting</c> напрямую.
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
