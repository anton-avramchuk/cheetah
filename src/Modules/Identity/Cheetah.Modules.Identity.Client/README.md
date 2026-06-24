# Cheetah.Modules.Identity.Client

HTTP-клиент к Identity API для server-to-server интеграции. Используется другими .NET-модулями/
сервисами, которым нужны данные пользователей Identity (например, модуль Tags держит локальную
реплику пользователей).

Опирается на контракты `Cheetah.Modules.Identity.Contracts` (`UserGridViewModel`).

## Состав

| Тип | Назначение |
|---|---|
| `IIdentityUsersClient` | Контракт клиента: `GetUsersAsync()` — полный список пользователей |
| `HttpIdentityUsersClient` | Реализация поверх `HttpClient` (`GET api/users?PageSize=0`) |
| `CheetahIdentityClientModule` | Регистрирует типизированный `HttpClient` + опции; навешивает `ServiceTokenHandler` |

## Аутентификация (server-to-server)

Это S2S-клиент: вызовы идут **без контекста пользователя** (например, фоновый синк реплики
участников в Teams/Tags). Поэтому модуль навешивает на `HttpClient`
[`ServiceTokenHandler`](../../../Cheetah.Backend.ServiceAuth/README.md) — каждый исходящий запрос
несёт сервисный токен (machine-to-machine, схема `client_credentials`) в заголовке
`Authorization: Bearer`. Модуль зависит от `CrmBackendServiceAuthModule`, поэтому хост **обязан**
настроить секцию `ServiceAuth`.

## Подключение

```csharp
[DependsOn(typeof(CheetahIdentityClientModule))]
public partial class MyModule : CrmModule { }
```

`appsettings.json`:
```json
{
  "Identity": { "Client": { "BaseUrl": "https://identity.internal" } },
  "ServiceAuth": {
    "TokenEndpoint": "https://identity.internal/api/auth/service-token",
    "ClientId": "<id этого сервиса как клиента Identity>",
    "ClientSecret": "<секрет>"
  }
}
```

Опции (`Identity:Client` и `ServiceAuth`) валидируются на старте (`ValidateOnStart`) — приложение
не поднимется с пустым/невалидным `BaseUrl` или незаполненными реквизитами сервисного клиента.
