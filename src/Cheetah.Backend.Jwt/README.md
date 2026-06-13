# Cheetah.Backend.Jwt

Готовая JWT-аутентификация для приложения: генерация токенов + настройка `JwtBearer` и authorization middleware. Реализует [Cheetah.Backend.Jwt.Abstractions](../Cheetah.Backend.Jwt.Abstractions/README.md). Зависит от `Cheetah.Core`, `CrmAspNetCoreModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmBackendJwtModule` | Модуль: биндит `JwtOptions`, настраивает `AddJwtBearer` + authorization, в `OnApplicationInitialization` подключает `UseAuthentication`/`UseAuthorization` |
| `JwtTokenGenerator` (`IJwtTokenGenerator`) | Генерация подписанного токена |
| `JwtOptions` | Опции из секции `Jwt`: `SecretKey` (≥32), `Issuer`, `Audience`, `ExpirationMinutes` (по умолчанию 60) |

## Конфигурация

```json
{
  "Jwt": {
    "SecretKey": "<минимум 32 символа>",
    "Issuer": "cheetah",
    "Audience": "cheetah-clients",
    "ExpirationMinutes": 60
  }
}
```

Опции валидируются на старте (`ValidateDataAnnotations().ValidateOnStart()`).

## Поведение

- Валидация токена: issuer, audience, lifetime, signing key; `ClockSkew = 0`.
- Глобальный `FallbackPolicy` требует аутентифицированного пользователя — все endpoint'ы защищены по умолчанию; публичные помечаются `AllowAnonymous` (см. [Cheetah.Backend.Endpoints](../Cheetah.Backend.Endpoints/README.md)).

## Подключение

```csharp
[DependsOn(typeof(CrmBackendJwtModule))]
public partial class MyAppModule : CrmModule { }
```
