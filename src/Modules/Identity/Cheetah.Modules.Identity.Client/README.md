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
| `CheetahIdentityClientModule` | Регистрирует типизированный `HttpClient` + опции |

## Подключение

```csharp
[DependsOn(typeof(CheetahIdentityClientModule))]
public partial class MyModule : CrmModule { }
```

`appsettings.json`:
```json
{ "Identity": { "Client": { "BaseUrl": "https://identity.internal" } } }
```

Опции (`Identity:Client`) валидируются на старте (`ValidateOnStart`) — приложение не поднимется
с пустым/невалидным `BaseUrl`.

> Auth к Identity API пока не настроен (TODO: bearer-токен).
