# Cheetah.RateLimit

Core-абстракции для distributed rate limiting. Здесь только контракты, ноль внешних зависимостей. Конкретные реализации:

- [Cheetah.RateLimit.Redis](../Cheetah.RateLimit.Redis/README.md) — distributed sliding-window-counter поверх Redis

Для **in-process** ограничения используй `System.Threading.RateLimiting` (BCL) — там есть `FixedWindow`, `SlidingWindow`, `TokenBucket`, `Concurrency` лимитеры и ASP.NET middleware. Этот модуль закрывает только то, чего у BCL нет: согласованность лимитов между репликами.

## Состав

| Тип | Назначение |
|-----|------------|
| `IDistributedRateLimiter` | `AcquireAsync(policyName, key)` / `AcquireAsync(key, limit, window)` |
| `RateLimitDecision` | `IsAllowed`, `RemainingPermits`, `Limit`, `RetryAfter` |
| `RateLimitPolicy` | `Limit`, `Window` — настраиваются провайдерами через свои options |
| `CrmRateLimitModule` | Core-модуль (только абстракции) |

## Интеграция с endpoints

`Cheetah.Backend.Endpoints` использует эту абстракцию для декларативной защиты сгенерированных endpoint'ов:

```csharp
public class LoginEndpoint : CommandEndpoint<LoginRequest, LoginCommand>
{
    public override string Route => "/api/auth/login";

    protected override void Configure(EndpointConfiguration cfg)
        => cfg.AllowAnonymousAccess()
              .WithRateLimit("login", RateLimitKeySource.Ip);  // 5 попыток в минуту по IP
}
```

Source Generator в `Cheetah.Generators.Endpoints` подставит соответствующий `RateLimitFilter` в сгенерированный `MapPost(...)`. Если `IDistributedRateLimiter` не зарегистрирован (например в dev-окружении без Redis) — fail-open (limiter пропускается).
