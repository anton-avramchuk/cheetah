# Cheetah.Modules.Calendar.Api

HTTP-слой Calendar на **декларативных эндпоинтах** ([`Cheetah.Backend.Endpoints`](../../../Cheetah.Backend.Endpoints/README.md))
+ генератор регистрации ([`Cheetah.Generators.Endpoints`](../../../Cheetah.Generators.Endpoints/README.md)).
Зависит от `…Application`, `…Contracts`, `Cheetah.Backend.Endpoints`, `Cheetah.Mapping.Mapster`,
`Cheetah.AspNetCore(.Contracts)`, `Cheetah.Core.CQRS`.

## Состав

| Папка/файл | Назначение |
|---|---|
| `Endpoints/*` | Эндпоинт-классы — наследники `QueryEndpoint`/`CreateCommandEndpoint`/`CommandEndpoint`/… Только метаданные (маршрут, имя, теги); тело генерируется |
| `Mapping/CalendarMappingProfile` | Mapster-профиль Request → Command/Query |
| `CheetahCalendarApiModule` | Модуль; `ConfigureServices` регистрирует профиль. `OnApplicationInitialization` **генерируется** кодогеном |

## Как это работает

- `Cheetah.Generators.Endpoints` находит эндпоинт-классы и эмитит `OnApplicationInitialization`
  с `MapGet/MapPost/...` → маппинг Request→CQRS → `IDispatcher` → ответ.
- Маршрутные `{id}` биндятся из пути: GET/DELETE — `[AsParameters]`, POST/PATCH — `MergeRouteValuesInto`.
- Кастомную инициализацию (если нужна) добавляют через сгенерированный partial-хук
  `OnApplicationInitializationCustom` — **не** переопределяя `OnApplicationInitialization`.

## Маршруты (основное)

| Метод | URL |
|---|---|
| POST/GET | `api/calendars`, `api/calendars/{calendarId}` |
| GET | `api/calendars/{calendarId}/events`, `api/calendar-events/by-entity/{entityType}/{entityId}`, `api/agenda/{userId}` |
| GET | `api/calendar/users/{hostUserId}/busy` — занятые интервалы пользователя (free/busy) в окне `[from, to)` |
| POST/GET/PATCH/DELETE | `api/calendar-events/{eventId}` (+ `/reschedule`, `/recurrence`, `/occurrences/{key}/cancel|override`, `/attendees`, `/reminders`) |
| POST/GET | `api/calendar/registry/sync`, `api/calendar/registry` |

## Добавить эндпоинт

```csharp
public sealed class GetCalendarByIdEndpoint
    : QueryOrNotFoundEndpoint<GetCalendarByIdRequest, GetCalendarByIdQuery, CalendarDto, CalendarDto>
{
    public override string Route => "api/calendars/{calendarId:guid}";
    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetCalendar").WithTags("Calendars");
}
```

Затем зарегистрировать пару Request→Query/Command в `CalendarMappingProfile`.
