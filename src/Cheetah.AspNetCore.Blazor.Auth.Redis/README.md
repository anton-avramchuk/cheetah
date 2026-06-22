# Cheetah.AspNetCore.Blazor.Auth.Redis

Redis-реализация `IUserTokenStore` для `Cheetah.AspNetCore.Blazor.Auth`, чтобы BFF можно было
масштабировать на несколько инстансов: токены сессии хранятся в Redis под ключом
`{KeyPrefix}{sessionId}` с **TTL** (запись истекает сама, отдельная очистка не нужна).

> В базовом Auth-модуле дефолтной регистрации `IUserTokenStore` нет — этот модуль её и
> предоставляет (через `services.Replace`). Подключайте его вместо ручной регистрации in-memory стора.

## Что предоставляет

- **`RedisUserTokenStore`** — обёртка над `IRedisClient` (`Cheetah.Backend.Redis`): JSON-сериализация
  `BffTokenSet`, запись с TTL через `SetAsync(key, value, expiry)`.
- **`CrmBlazorAuthRedisModule`** — регистрирует `IUserTokenStore` как Redis-стор
  (`services.Replace(...)`, поэтому перетирает и ручную in-memory регистрацию, если она была),
  код приложения и остальной Auth-модуль не меняются.
- **`RedisUserTokenStoreOptions`** (секция `BffAuth:Redis`).

## Подключение

```csharp
[DependsOn(typeof(CrmBlazorAuthModule))]
[DependsOn(typeof(CrmBlazorAuthRedisModule))] // ← добавить этот модуль
public partial class AppBootstrapperModule : CrmModule { }
```

Настройка подключения Redis — стандартная для `Cheetah.Backend.Redis` (секция `Redis:Instances`):

```json
{
  "Redis": {
    "Instances": {
      "default": { "ConnectionString": "localhost:6379", "Database": 0 }
    }
  },
  "BffAuth": { "ExpireTimeSpan": "08:00:00" },
  "BffAuth:Redis": { "KeyPrefix": "bff:tokens:", "InstanceName": "default" }
}
```

## Конфигурация (`BffAuth:Redis`)

| Ключ | По умолчанию | Назначение |
|------|--------------|------------|
| `KeyPrefix` | `bff:tokens:` | Префикс ключей; итоговый ключ = `KeyPrefix + sessionId` |
| `InstanceName` | `default` | Имя инстанса из `Redis:Instances` |
| `Ttl` | `null` | TTL записи; если не задано — берётся `BffAuth:ExpireTimeSpan` (живёт столько же, сколько cookie) |

## Зависимости

- `Cheetah.Core`, `Cheetah.AspNetCore.Blazor.Auth`, `Cheetah.Backend.Redis`
  (`CrmBlazorAuthRedisModule` → `[DependsOn(CoreModule, CrmBlazorAuthModule, CrmBackendRedisModule)]`).
- Подключайте **вместо** дефолтного in-memory — модуль сам делает `Replace` регистрации `IUserTokenStore`.
