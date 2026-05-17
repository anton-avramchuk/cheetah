# Cheetah.RateLimit.Redis

Реализация [Cheetah.RateLimit](../Cheetah.RateLimit/README.md) через Redis с sliding-window-counter алгоритмом.

## Зачем не использовать BCL

`System.Threading.RateLimiting` (BCL начиная с .NET 7) уже даёт `FixedWindow`, `SlidingWindow`, `TokenBucket` лимитеры — но только **in-process**. Если у тебя несколько реплик приложения и лимит "100 req/min на user" — каждая реплика будет считать свои 100. В сумме = 100 × число реплик.

Этот модуль нужен **только** если лимит должен соблюдаться поперёк реплик. Для в-процессорного ограничения используй BCL напрямую — там аккуратнее с аллокациями и есть готовый ASP.NET middleware.

## Алгоритм: sliding window counter

По образцу [Cloudflare](https://blog.cloudflare.com/counting-things-a-lot-of-different-things/):
- Считаем requests в **текущем** и **предыдущем** окнах
- При проверке: `effective = current + previous × (1 − elapsed_in_current / window)`
- Если `effective + 1 > limit` — отказ

Точность ≈ ±limit/2 на стыке окон. Гораздо лучше fixed-window (всплески на границе) и без памяти на хранение каждого timestamp (как у sliding-window-log). Атомарность через Lua-скрипт (INCR + GET + проверка в одной round-trip).

## Состав

| Тип | Назначение |
|-----|------------|
| `IDistributedRateLimiter` | `AcquireAsync(policyName, key)` или `AcquireAsync(key, limit, window)` |
| `RateLimitDecision` | `IsAllowed`, `RemainingPermits`, `Limit`, `RetryAfter` |
| `RedisRateLimitOptions` | `InstanceName`, `KeyPrefix`, `Policies: Dictionary<string, RateLimitPolicy>` |
| `RateLimitPolicy` | `Limit`, `Window` |
| `RedisDistributedRateLimiter` | Реализация на Lua + Redis |
| `CrmRateLimitRedisModule` | Зависит от `CrmBackendRedisModule` |

## Подключение

```csharp
[DependsOn(typeof(CrmBackendRedisModule))]
[DependsOn(typeof(CrmRateLimitRedisModule))]
public partial class MyAppModule : CrmModule { }
```

```json
"RateLimit": {
  "InstanceName": "default",
  "KeyPrefix": "rl:",
  "Policies": {
    "login": { "Limit": 5, "Window": "00:01:00" },
    "api": { "Limit": 100, "Window": "00:01:00" }
  }
}
```

## Использование

```csharp
public class LoginEndpoint(IDistributedRateLimiter limiter, ...)
{
    public async Task<IResult> Login(LoginRequest req)
    {
        var decision = await limiter.AcquireAsync("login", req.Email);
        if (!decision.IsAllowed)
        {
            return Results.StatusCode(429)
                .WithHeader("Retry-After", ((int)decision.RetryAfter.TotalSeconds).ToString());
        }
        // ... обычная логика
    }
}
```

Или с явными лимитами (override per-call):

```csharp
await limiter.AcquireAsync($"user:{userId}", limit: 60, window: TimeSpan.FromMinutes(1));
```

## Tradeoffs

- **+** Точность ±limit/2 на стыке окон — приемлемо для большинства случаев
- **+** Atomicity через Lua — не страдает от race conditions между репликами
- **+** ~2 TTL'нных ключа на (policy, key) в Redis — лёгкая нагрузка на память
- **−** Каждый AcquireAsync = 1 round-trip к Redis (~1ms в одной AZ). Для super-hot path можно добавить локальный pre-check (BCL in-process limiter) с тем же limit — если уже превышено локально, не идём в Redis
- **−** Redis недоступен → лимитер падает. Решение: при Redis-ошибке решите на месте — fail-open (пропустить) или fail-closed (отклонить). По умолчанию exception пробрасывается
