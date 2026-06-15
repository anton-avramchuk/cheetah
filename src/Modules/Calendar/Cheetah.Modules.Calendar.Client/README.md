# Cheetah.Modules.Calendar.Client

HTTP-клиент к Calendar.Api для server-to-server интеграции + авто-регистрация привязываемых
типов при старте. Используется другими .NET-модулями/сервисами. Опирается на контракты
`Cheetah.Modules.Calendar.Contracts`.

## Состав

| Тип | Назначение |
|---|---|
| `ICalendarClient` | Контракт: `SyncRegistryAsync`, `CreateEventAsync`, `GetByEntityAsync` |
| `HttpCalendarClient` | Реализация поверх типизированного `HttpClient` |
| `CalendarRegistrationSyncService` | Hosted-сервис: при старте отправляет объявленные типы в Calendar |
| `CalendarableTypeRegistrationExtensions.AddCalendarableEntityType` | Объявление типа сущности сервиса |
| `CalendarClientOptions` | `BaseUrl`, `OwnerService`, `Timeout`, `ContinueOnFailure` (валидируются на старте) |
| `CheetahCalendarClientModule` | Регистрирует `HttpClient`, опции и hosted-сервис |

## Подключение

```csharp
[DependsOn(typeof(CheetahCalendarClientModule))]
public partial class MyModule : CrmModule { }

// объявить свои привязываемые типы — уйдут в Calendar при старте
services.AddCalendarableEntityType("crm.deal", "Сделка", o =>
{
    o.DefaultColor = "#3b82f6";
    o.AllowMultiplePerEntity = true;
});
```

`appsettings.json`:
```json
{ "Calendar": { "Client": { "BaseUrl": "https://calendar.api", "OwnerService": "crm" } } }
```

## Особенности

- `Calendar:Client` валидируется на старте (`ValidateOnStart`): без валидного `BaseUrl`/`OwnerService`
  хост не поднимется.
- `OwnerService` проставляется во все регистрируемые типы при синхронизации.
- `ContinueOnFailure=true` (по умолчанию): ошибка регистрации при старте логируется, но не валит хост.
