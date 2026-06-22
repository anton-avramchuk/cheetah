# Cheetah.AspNetCore.Blazor.Auth

BFF-аутентификация для **Blazor Server**: cookie для сессии браузера + серверный стор JWT, чтобы
ходить к другим микросервисам как `Authorization: Bearer`. Получение и обновление токена — через
единственный шов `IBffAuthenticator`, который реализует приложение (удалённый Identity-сервис,
локальная БД или внешний провайдер вроде Keycloak/OIDC). Фреймворк не привязан к конкретной схеме.

## Зачем так

В Blazor Server два «слоя» аутентификации:
- **браузер ↔ сервер** — обычная **cookie**-сессия (внутри SignalR-контура нельзя выставить cookie,
  поэтому вход/выход идут через нативный POST на HTTP-эндпоинты);
- **сервер (BFF) ↔ микросервисы** — **JWT** на каждый вызов.

Внутри Blazor-контура `HttpContext` недоступен, поэтому токены нельзя достать из cookie «на лету».
Решение: токены лежат в **серверном сторе**, а cookie несёт только identity + `session_id`
(claim `BffClaimTypes.SessionId`). Внутри контура текущая сессия берётся из `ClaimsPrincipal`
(через `AuthenticationStateProvider`), а по `session_id` — токены из стора.

## Поток

```
            POST /account/login (нативная форма)
browser ───────────────────────────────────► BFF
                                               │ IBffAuthenticator.AuthenticateAsync(creds)
                                               │   → BffUser + BffTokenSet (access/refresh)
                                               │ tokenStore.Store(sessionId, tokens)
                                               │ SignIn(cookie: identity + session_id)
            ◄───────────── 302 ───────────────┘
component → HttpClient(BearerTokenHandler) ──► microservice
            handler: IAccessTokenProvider.GetAccessTokenAsync()
              → session_id из principal → tokenStore.Get → (если протух) IBffAuthenticator.Refresh
              → Authorization: Bearer <jwt>;  при 401 — refresh + один повтор
```

## Что предоставляет

- **`IBffAuthenticator`** (шов, реализует приложение): `AuthenticateAsync(creds)` и
  `RefreshAsync(refreshToken)`. Возвращает `BffAuthResult` (`BffUser` + `BffTokenSet`).
- **`IUserTokenStore`** — стор токенов по `session_id`. **Реализацию выбираете сами** (см. шаг 2 ниже):
  `InMemoryUserTokenStore` для single-instance или модуль `CrmBlazorAuthRedisModule` (Redis) для нескольких
  инстансов. Дефолтной регистрации НЕТ — без неё DI не сможет построить `IAccessTokenProvider`.
- **`IAccessTokenProvider`** (Scoped) — отдаёт действующий access-токен текущего пользователя,
  при необходимости обновляя протухший по refresh (circuit-safe, без `HttpContext`).
- **`BearerTokenHandler`** (DelegatingHandler) — подставляет Bearer и повторяет запрос один раз при 401.
- **`BffAuthenticationStateProvider`** — server-side auth-state с ревалидацией: если сессия больше
  не содержит токенов (logout / refresh провалился) — auth-state сбрасывается.
- **Эндпоинты** `/account/login` и `/account/logout` (маппятся автоматически в
  `OnApplicationInitialization`).
- **`BffClaimTypes.SessionId`**, **`BffAuthOptions`** (секция `BffAuth`).

## Подключение

1) Зависимость бутстраппера хоста:

```csharp
[DependsOn(typeof(CrmBlazorAuthModule))]
public partial class AppBootstrapperModule : CrmModule { }
```

2) **Зарегистрируйте `IUserTokenStore` — обязательно, дефолтной регистрации нет.** Выберите реализацию:

```csharp
// single-instance BFF:
builder.Services.AddSingleton<IUserTokenStore, InMemoryUserTokenStore>();
```

либо для нескольких инстансов подключите Redis-модуль (он сам регистрирует стор):

```csharp
[DependsOn(typeof(CrmBlazorAuthModule))]
[DependsOn(typeof(CrmBlazorAuthRedisModule))] // пакет Cheetah.AspNetCore.Blazor.Auth.Redis
public partial class AppBootstrapperModule : CrmModule { }
```

> Без зарегистрированного `IUserTokenStore` DI не сможет построить `IAccessTokenProvider` —
> приложение упадёт на старте. Это сделано осознанно: выбор хранилища (память/Redis) — за приложением.

3) Реализуйте шов в приложении (пример — поход в Identity-микросервис):

```csharp
[Export(LifetimeType.Scoped, typeof(IBffAuthenticator))]
public sealed class IdentityServiceAuthenticator(IHttpClientFactory http) : IBffAuthenticator
{
    public async Task<BffAuthResult> AuthenticateAsync(BffCredentials c, CancellationToken ct)
    {
        // POST creds в Identity-сервис → access/refresh + профиль
        // return BffAuthResult.Success(new BffUser(id, userName, email, roles), new BffTokenSet(access, refresh, expiresAt));
        // или BffAuthResult.Fail("Неверный логин или пароль");
    }

    public async Task<BffTokenSet?> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        // POST refresh → новый BffTokenSet, либо null если refresh истёк
    }
}
```

4) Зарегистрируйте HttpClient к микросервису с Bearer-handler:

```csharp
builder.Services.AddBffHttpClient("catalog", c => c.BaseAddress = new Uri("https://catalog"));
// или для типизированного клиента:
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>().AddBffBearerToken();
```

5) В `Program.cs` хоста — middleware в правильном порядке (cookie пишется в HttpContext):

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();        // после UseRouting/UseAuthentication
// форма логина делает POST /account/login; эндпоинты уже замаплены модулем
```

> Микросервисы на приёмной стороне валидируют этот JWT обычным `Cheetah.Backend.Jwt`
> (`CrmBackendJwtModule`, JwtBearer) — общий issuer/secret. Этот модуль — только BFF-сторона.

## Конфигурация (`BffAuth`)

| Ключ | По умолчанию | Назначение |
|------|--------------|------------|
| `LoginPath` | `/login` | Куда редиректит при 401/Challenge и ошибке входа |
| `AccessDeniedPath` | `/access-denied` | Страница «доступ запрещён» |
| `CookieName` | `cheetah.bff.auth` | Имя auth-cookie |
| `ExpireTimeSpan` | `08:00:00` | Время жизни cookie |
| `SlidingExpiration` | `true` | Скользящее продление |
| `RefreshSkewSeconds` | `30` | Запас до истечения access-токена для упреждающего refresh |
| `RevalidationInterval` | `00:05:00` | Интервал ревалидации auth-state |

## Зависимости

- `Cheetah.Core`, `Cheetah.AspNetCore`, `Cheetah.Core.Security`
  (`CrmBlazorAuthModule` → `[DependsOn(CoreModule, CrmAspNetCoreModule, CrmCoreSecurityModule)]`).
- `Microsoft.AspNetCore.App` (cookie-auth, endpoints, `AuthenticationStateProvider`,
  `RevalidatingServerAuthenticationStateProvider`).
- **`IBffAuthenticator` обязателен** — без реализации в приложении вход работать не будет.
- **`IUserTokenStore` обязателен** — дефолтной регистрации нет; приложение само выбирает
  `InMemoryUserTokenStore` (single-instance) или `CrmBlazorAuthRedisModule` (Redis, несколько инстансов).
