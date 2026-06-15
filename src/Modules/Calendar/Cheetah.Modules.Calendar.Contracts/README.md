# Cheetah.Modules.Calendar.Contracts

DTO и Request'ы Calendar — транспортные контракты HTTP-слоя. Зависит от `Cheetah.Core`,
`Cheetah.Modules.Calendar.Shared` и `Cheetah.Contracts` (`ICrmRequest`/`ICrmResponse`,
`[FromRoute]`).

## Состав

| Группа | Типы |
|---|---|
| Ответы (`ICrmResponse`) | `CalendarDto`, `CalendarEventDto` (+ вложенные `EventAttendeeDto`, `EventReminderDto`), `EventOccurrenceDto`, `CalendarableEntityTypeDto` |
| Запросы (`ICrmRequest`) | `CreateCalendarRequest`, `GetCalendarByIdRequest`, `ListEventsByCalendarRequest`, `CreateEventRequest`, `GetEventByIdRequest`, `UpdateEventDetailsRequest`, `RescheduleEventRequest`, `SetRecurrenceRequest`, `CancelEventRequest`, `ListEventsByEntityRequest`, `ListAgendaRequest`, `CancelOccurrenceRequest`, `OverrideOccurrenceRequest`, `AddAttendeeRequest`, `RespondToInviteRequest`, `AddReminderRequest`, `RemoveReminderRequest`, `CalendarRegistrySyncRequest`, `GetCalendarableTypesRequest` |

## Соглашения

- Сущность адресуется парой `EntityType` (string) + `EntityId` (Guid); RRULE — строка RFC 5545.
- Маршрутные поля запросов помечены `[property: FromRoute]`. Для GET/DELETE их биндит
  `[AsParameters]` по имени; для POST/PATCH — `MergeRouteValuesInto` в сгенерированном эндпоинте.
  Поэтому один Request покрывает и `{id}` из пути, и тело.
- Имена полей Request'ов совпадают с полями соответствующих CQRS-команд/запросов — Mapster
  соединяет их по соглашению (профиль в `Cheetah.Modules.Calendar.Api`).
