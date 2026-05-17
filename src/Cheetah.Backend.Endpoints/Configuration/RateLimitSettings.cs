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
/// Два режима:
///   1) <b>Named policy</b> — задано <see cref="PolicyName"/>, <see cref="Limit"/>/<see cref="Window"/> = null.
///      Лимиты резолвятся из <c>RateLimit:Policies:&lt;PolicyName&gt;</c> в appsettings.json.
///      Удобно когда ops должны крутить лимиты без передеплоя.
///   2) <b>Inline</b> — <see cref="PolicyName"/> = null, заданы <see cref="Limit"/> + <see cref="Window"/>.
///      Лимит хардкоден в коде endpoint'а как часть бизнес-правила.
/// </summary>
public sealed record RateLimitSettings(
    string? PolicyName,
    long? Limit,
    TimeSpan? Window,
    RateLimitKeySource KeySource = RateLimitKeySource.UserOrIp)
{
    /// <summary>Конструктор named-policy.</summary>
    public RateLimitSettings(string policyName, RateLimitKeySource keySource = RateLimitKeySource.UserOrIp)
        : this(policyName, null, null, keySource) { }

    /// <summary>Конструктор inline.</summary>
    public RateLimitSettings(long limit, TimeSpan window, RateLimitKeySource keySource = RateLimitKeySource.UserOrIp)
        : this(null, limit, window, keySource) { }
}
