namespace Cheetah.Backend.Endpoints.Configuration;

/// <summary>
/// Источник ключа для distributed rate limiter'а на endpoint'е.
/// </summary>
public enum RateLimitKeySource
{
    /// <summary>HttpContext.User.Identity.Name; если пользователь не аутентифицирован — fallback на IP.</summary>
    UserOrIp = 0,

    /// <summary>HttpContext.User.Identity.Name; если null — отказ (для endpoint'ов где аутентификация обязательна).</summary>
    UserOnly = 1,

    /// <summary>HttpContext.Connection.RemoteIpAddress (с учётом X-Forwarded-For при правильно настроенном ForwardedHeaders middleware).</summary>
    Ip = 2,

    /// <summary>Endpoint global — один лимит на весь endpoint, без partitioning.</summary>
    Global = 3
}

/// <summary>
/// Метаданные для применения distributed rate limiter'а к endpoint'у.
/// Создаётся через <see cref="EndpointConfiguration.WithRateLimit"/>.
/// </summary>
public sealed record RateLimitSettings(string PolicyName, RateLimitKeySource KeySource = RateLimitKeySource.UserOrIp);
