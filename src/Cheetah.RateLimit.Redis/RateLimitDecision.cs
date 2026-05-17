namespace Cheetah.RateLimit.Redis;

/// <summary>
/// Результат попытки получить permit от distributed rate limiter'а.
/// </summary>
public sealed record RateLimitDecision(
    bool IsAllowed,
    long RemainingPermits,
    long Limit,
    TimeSpan RetryAfter);
