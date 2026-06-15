# Cheetah.Modules.Calendar

Календарь уровня Google Calendar с двумя особенностями: **привязка событий к произвольной
сущности** другого модуля и **рассылка напоминаний перед событиями** через конвейер Notification.

Полное проектное описание: [`docs/modules/calendar.md`](../../../docs/modules/calendar.md).

## Сборки

| Сборка | Назначение |
|---|---|
| [`…DomainEvents`](Cheetah.Modules.Calendar.DomainEvents/README.md) | События `CalendarEventScheduled/Rescheduled/Cancelled`, `EventAttendeeInvited/Responded` |
| [`…Shared`](Cheetah.Modules.Calendar.Shared/README.md) | Enum'ы (`CalendarType`, `EventStatus`, `AttendeeRole/Response`, `ReminderTarget`, `ReminderTriggerStatus`), константы, ключи шаблонов |
| [`…Contracts`](Cheetah.Modules.Calendar.Contracts/README.md) | DTO/Request; сущность адресуется парой `EntityType` (string) + `EntityId` (Guid), RRULE — строка |
| [`…Domain`](Cheetah.Modules.Calendar.Domain/README.md) | Агрегаты (`CalendarEvent`, `Calendar`, `ReminderTrigger`, `CalendarableEntityType`), VO `RecurrenceRule`, `IRecurrenceExpander`, спецификации, репозитории-интерфейсы |
| [`…Infrastructure`](Cheetah.Modules.Calendar.Infrastructure/README.md) | EF Core `CalendarDbContext`, конфигурации, репозитории, RRULE-экспандер, миграции, Outbox |
| [`…Application`](Cheetah.Modules.Calendar.Application/README.md) | CQRS, материализация `ReminderTrigger`, фоновые задачи рассылки |
| [`…Api`](Cheetah.Modules.Calendar.Api/README.md) | Minimal API на декларативных эндпоинтах + генератор (`/api/calendars/**`, `/api/calendar-events/**`) |
| [`…Client`](Cheetah.Modules.Calendar.Client/README.md) | HTTP-клиент + авто-регистрация привязываемых типов при старте |

## Конвейер напоминаний (ядро)

1. Команды (создание/перенос/изменение напоминаний/повторения) и фоновая задача
   `MaterializeRemindersTask` материализуют срабатывания `ReminderTrigger` на горизонт (`HorizonDays`).
2. `DispatchDueRemindersTask` под distributed-lock сканирует наступившие триггеры
   (`Status=Pending AND FireAtUtc<=now`), резолвит получателей по `Target` и публикует
   `NotificationRequested` на каждого — доставку выполняет модуль Notification.
3. Идемпотентность — `NotificationId = DeterministicGuid(triggerId, userId)`; атомарность —
   публикация через Outbox того же `CalendarDbContext`.

## Привязка к сущности

Потребитель регистрирует свои типы при старте через Client (как в Tags):

```csharp
services.AddCalendarableEntityType("crm.deal", "Сделка", o =>
{
    o.DefaultColor = "#3b82f6";
    o.AllowMultiplePerEntity = true;
});
```

Таймлайн сущности: `GET /api/calendar-events/by-entity/{entityType}/{entityId}?from=&to=`.

## Повторения (RRULE)

`RecurrenceRule` хранит строку RFC 5545 + EXDATE; раскрытие — за `IRecurrenceExpander`.
Текущая реализация (`RRuleRecurrenceExpander`) покрывает практический поднабор:
FREQ (DAILY/WEEKLY/MONTHLY/YEARLY), INTERVAL, COUNT, UNTIL, BYDAY (WEEKLY), BYMONTHDAY.
Раскрытие идёт в таймзоне события (корректно для DST). Интерфейс позволяет заменить движок
полноценным iCal (напр. Ical.Net), не трогая остальной модуль.

## Конфигурация

```jsonc
{
  "Calendar": {
    "Reminders": {
      "HorizonDays": 60,
      "DispatchBatchSize": 200,
      "DispatchPeriod": "00:01:00",
      "MaterializePeriod": "01:00:00"
    },
    "Client": { "BaseUrl": "https://calendar.api", "OwnerService": "crm" }
  }
}
```

Для единичного исполнителя скана в кластере подключите `Cheetah.DistributedLock.Postgres`
(без него задачи работают в одно-инстансном режиме).
