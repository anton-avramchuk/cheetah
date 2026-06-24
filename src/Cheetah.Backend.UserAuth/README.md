# Cheetah.Backend.UserAuth

Проброс пользовательского токена (**on-behalf-of**) для **исходящих** HTTP-вызовов между
сервисами. Сервис A, обрабатывая запрос пользователя, при вызове сервиса B пробрасывает
тот же `Authorization`-заголовок — сервис B видит реального пользователя и его роли,
авторизация работает «как есть».

Сосед по контуру — [`Cheetah.Backend.ServiceAuth`](../Cheetah.Backend.ServiceAuth/README.md)
(machine-to-machine). Выбор потока — по контексту вызова:

| Контекст вызова | Поток | Модуль |
|-----------------|-------|--------|
| Синхронный вызов в рамках запроса пользователя | on-behalf-of (проброс) | **Cheetah.Backend.UserAuth** |
| Фон / события / Outbox (пользователя нет) | сервисный токен (client_credentials) | Cheetah.Backend.ServiceAuth |

## Состав

| Тип | Назначение |
|-----|-----------|
| `UserTokenForwardingHandler` | `DelegatingHandler`: копирует `Authorization` текущего `HttpContext` в исходящий запрос. Если у запроса уже есть `Authorization` — не перезаписывает. Нет `HttpContext` (фон) → ничего не делает. **Transient**. |
| `CrmBackendUserAuthModule` | Модуль: регистрирует обработчик и `IHttpContextAccessor`. |
| `AddUserTokenForwarding()` | Расширение `IHttpClientBuilder` для краткого подключения. |

## Зависимости

- `Cheetah.Core` (модульность, DI, `[Export]`).
- `FrameworkReference Microsoft.AspNetCore.App` (`IHttpContextAccessor`).

Зависит только от `CoreModule`.

## Подключение

1. Сделайте Client-модуль зависимым от `CrmBackendUserAuthModule`:

```csharp
[DependsOn(typeof(CoreModule), typeof(CrmBackendUserAuthModule), /* ... */)]
public partial class CheetahMyModuleClientModule : CrmModule { }
```

2. Повесьте обработчик на `HttpClient` клиента:

```csharp
services.AddHttpClient<IMyClient, HttpMyClient>(/* ... */)
        .AddUserTokenForwarding();
```

## Ограничения

- Работает только в контексте входящего HTTP-запроса. Для фоновых сценариев — `Cheetah.Backend.ServiceAuth`.
- Это **не** обмен токена (token exchange): целевому сервису уходит ровно тот же токен
  пользователя. Если нужно сужение прав/смена audience — это отдельный шаг (RFC 8693), пока не реализован.
- На один `HttpClient` вешается **либо** этот обработчик, **либо** `ServiceTokenHandler` —
  оба ставят `Authorization`. Не комбинируйте на одном клиенте.
