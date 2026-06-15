# Cheetah.Modules.Calendar.Infrastructure

Инфраструктура Calendar: EF Core, реализации репозиториев, RRULE-движок, миграции и Outbox.
Зависит от `…Domain`, `CrmEntityFrameworkModule` + `CrmEntityFrameworkPostgreSqlModule`,
`CrmOutbox*` (через `CheetahCalendarInfrastructureModule`).

## Состав

| Тип | Назначение |
|---|---|
| `CalendarDbContext` | Контекст; таблицы в схеме `CalendarConstants.Schema` |
| `Persistence/Configurations/*` | EF-конфигурации (owned `RecurrenceRule`, backing-fields коллекций, индексы; `Ignore(DomainEvents)`) |
| `Repositories/CalendarRepositories.cs` | `EfRepository`-реализации `ICalendarEventRepository`, `IReminderTriggerRepository` и generic-репозиториев |
| `Recurrence/RRuleParser` | Парсер поднабора RRULE (FREQ/INTERVAL/COUNT/UNTIL/BYDAY/BYMONTHDAY) |
| `Recurrence/RRuleRecurrenceExpander` | Реализация `IRecurrenceExpander`: раскрытие в таймзоне события (корректно для DST), EXDATE и override поверх |
| `Migrations/*` | Начальная миграция схемы |

## Подключение

```csharp
[DependsOn(typeof(CheetahCalendarInfrastructureModule))]
public partial class MyBootstrapModule : CrmModule { }
```

`appsettings.json`:
```json
{ "ConnectionStrings": { "Calendar": "Host=localhost;Database=calendar;Username=postgres;Password=..." } }
```

## Особенности

- Горячий индекс `(Status, FireAtUtc)` на `ReminderTriggers` под скан «пора слать».
- Уникальный индекс `(EventId, ReminderId, OccurrenceKey)` защищает от дублей при пересчёте серии.
- Outbox/DeadLetter подключены поверх того же `CalendarDbContext` — публикация `NotificationRequested`
  фиксируется атомарно с гашением триггера (`AddPostgresOutboxStore` + `AddDeadLetterStore`).
