# Cheetah.Modules.Calendar.Application

Прикладной слой Calendar: CQRS, материализация напоминаний и фоновые задачи рассылки.
Зависит от `…Domain`, `…Contracts`, `…DomainEvents`, `Notification.DomainEvents` (Calendar —
**продюсер** `NotificationRequested`), CQRS/Events/BackgroundTasks/DistributedLock/DataAccess.
**Не** зависит от Infrastructure (репозитории-интерфейсы живут в Domain).

## Состав

| Группа | Содержимое |
|---|---|
| `Calendars/`, `Events/`, `Attendees/`, `Reminders/`, `Registry/` | Команды и запросы (создание/перенос/повторения/участники/напоминания/реестр/экземпляры серии) |
| `Services/ReminderScheduler` (`IReminderScheduler`) | Идемпотентный upsert материализованных `ReminderTrigger` на горизонт |
| `BackgroundTasks/MaterializeRemindersTask` | Периодический досев горизонта для бесконечных серий (под distributed-lock) |
| `BackgroundTasks/DispatchDueRemindersTask` | Скан наступивших триггеров → `NotificationRequested` на получателей (под distributed-lock, через Outbox) |
| `Options/CalendarReminderOptions` | `HorizonDays`, `DispatchBatchSize`, периоды/ключи локов |
| `Mapping/CalendarProjector` | Проекция доменных сущностей в DTO (без Mapster) |

## Конвейер напоминаний

1. Команды и `MaterializeRemindersTask` материализуют `ReminderTrigger` на `HorizonDays` вперёд.
2. `DispatchDueRemindersTask` выбирает `Status=Pending AND FireAtUtc<=now`, резолвит получателей
   по `ReminderTarget` и публикует `NotificationRequested` (идемпотентность — детерминированный
   `NotificationId`, атомарность — Outbox того же DbContext).

## Конфигурация

```jsonc
{ "Calendar": { "Reminders": { "HorizonDays": 60, "DispatchBatchSize": 200,
  "DispatchPeriod": "00:01:00", "MaterializePeriod": "01:00:00" } } }
```

Для единичного исполнителя скана в кластере подключите `Cheetah.DistributedLock.Postgres`
(без него фоновые задачи работают в одно-инстансном режиме).
