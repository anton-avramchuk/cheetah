using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.RateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// IEndpointFilter, который перед вызовом handler'a проверяет distributed rate limiter
/// и возвращает 429 при превышении. Выставляет заголовки X-RateLimit-Limit / X-RateLimit-Remaining
/// и Retry-After (стандарт для клиентов — Stripe, GitHub).
///
/// Регистрируется автоматически Source Generator'ом если в EndpointConfiguration вызван
/// WithRateLimit(...). Реализация IDistributedRateLimiter должна быть зарегистрирована в DI
/// (см. Cheetah.RateLimit.Redis).
/// </summary>
public sealed class RateLimitFilter : IEndpointFilter
{
    private readonly RateLimitSettings _settings;

    public RateLimitFilter(RateLimitSettings settings) => _settings = settings;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var limiter = http.RequestServices.GetService<IDistributedRateLimiter>();
        if (limiter is null)
        {
            // Реализация не подключена — fail-open. Альтернатива: throw, но это слишком жёстко
            // для случаев когда RateLimit metadata оставлена, а transport ещё не настроен.
            return await next(context);
        }

        var key = ResolveKey(http, _settings.KeySource);
        if (key is null)
        {
            // UserOnly + анонимный запрос — отказ.
            return Results.StatusCode(StatusCodes.Status401Unauthorized);
        }

        var decision = await limiter.AcquireAsync(_settings.PolicyName, key);

        http.Response.Headers["X-RateLimit-Limit"] = decision.Limit.ToString();
        http.Response.Headers["X-RateLimit-Remaining"] = decision.RemainingPermits.ToString();

        if (!decision.IsAllowed)
        {
            http.Response.Headers["Retry-After"] = ((int)decision.RetryAfter.TotalSeconds).ToString();
            return Results.StatusCode(StatusCodes.Status429TooManyRequests);
        }

        return await next(context);
    }

    private static string? ResolveKey(HttpContext http, RateLimitKeySource source) => source switch
    {
        RateLimitKeySource.Global => "_global",
        RateLimitKeySource.Ip => http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        RateLimitKeySource.UserOnly => http.User.Identity?.IsAuthenticated == true ? http.User.Identity.Name : null,
        RateLimitKeySource.UserOrIp =>
            (http.User.Identity?.IsAuthenticated == true ? http.User.Identity.Name : null)
            ?? http.Connection.RemoteIpAddress?.ToString()
            ?? "unknown",
        _ => "unknown"
    };
}
