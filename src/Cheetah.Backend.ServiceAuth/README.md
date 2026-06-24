# Cheetah.Backend.ServiceAuth

Machine-to-machine (M2M) аутентификация для **исходящих** HTTP-вызовов между сервисами.
Сервис получает собственный короткоживущий JWT у Identity по схеме `client_credentials`,
кэширует его и автоматически подставляет в заголовок `Authorization: Bearer` при вызовах
других сервисов.

Применяется там, где **нет контекста пользователя**: фоновые задачи, обработчики событий,
Outbox-диспетчер, server-to-server вызовы из Client-библиотек.

## Состав

| Тип | Назначение |
|-----|-----------|
| `IServiceTokenProvider` | Отдаёт действующий сервисный токен. |
| `CachingServiceTokenProvider` | Реализация: запрашивает токен у Identity, кэширует, обновляет за `RefreshSkew` до истечения. Потокобезопасна (параллельные вызовы делят один запрос на обновление). Зарегистрирована как **Singleton**. |
| `ServiceTokenHandler` | `DelegatingHandler`, подставляющий `Authorization: Bearer <token>` в исходящие запросы. Регистрируется как **Transient**. |
| `ServiceAuthOptions` | Настройки (секция `ServiceAuth`). |
| `CrmBackendServiceAuthModule` | Модуль: биндит опции (с `ValidateOnStart`), регистрирует провайдер/обработчик и именованный `HttpClient` для запроса токена. |

## Зависимости

- `Cheetah.Core` (модульность, DI, `[Export]`).
- `Microsoft.Extensions.Http` / `Options` / `Logging.Abstractions`.

Зависит только от `CoreModule`. Не тянет ASP.NET — это клиентская (исходящая) сторона.

## Подключение

1. Сделайте Client-модуль зависимым от `CrmBackendServiceAuthModule`:

```csharp
[DependsOn(typeof(CoreModule), typeof(CrmBackendServiceAuthModule), /* ... */)]
public partial class CheetahMyModuleClientModule : CrmModule { }
```

2. Повесьте обработчик на `HttpClient` клиента:

```csharp
services.AddHttpClient<IMyClient, HttpMyClient>(/* ... */)
        .AddHttpMessageHandler<ServiceTokenHandler>();
```

3. Конфигурация хоста:

```json
{
  "ServiceAuth": {
    "TokenEndpoint": "https://identity/api/auth/service-token",
    "ClientId": "svc-deals",
    "ClientSecret": "<секрет из секрет-менеджера>",
    "RefreshSkew": "00:00:30",
    "Timeout": "00:00:05"
  }
}
```

## Серверная сторона (выпуск токена)

Эндпоинт `POST /api/auth/service-token` живёт в `Cheetah.Modules.Identity.Api`.
Принимает `{ clientId, clientSecret }`, проверяет учётные данные через
`IServiceClientAuthenticator` (MVP-реализация — реестр в конфиге, секция `ServiceClients`)
и выпускает сервисный JWT (`sub = clientId`, claim `token_type=service`, роли клиента).
Подпись — тем же ключом, что и пользовательские токены (`Jwt:SecretKey`), время жизни —
`Jwt:ServiceTokenExpirationMinutes` (default 10 мин).

Конфиг реестра клиентов на стороне Identity:

```json
{
  "ServiceClients": {
    "Clients": [
      { "ClientId": "svc-deals", "ClientSecret": "<секрет>", "Roles": [ "ServiceAccount" ] }
    ]
  }
}
```

## Ограничения MVP / TODO

- **Секреты в конфиге** и подпись **симметричным** ключом (HMAC). Для прода: хранить хэши
  секретов в БД/секрет-менеджере; перейти на асимметричную подпись (RS256 + JWKS), чтобы
  выпускать токены мог только Identity.
- Нет ретраев/Polly на запросе токена и refresh-after-401 (можно добавить `Http.Resilience`).
- Пилотное подключение — `Cheetah.Modules.Deals.Client`.
