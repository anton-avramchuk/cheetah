# Cheetah.Modules.Calendar.Domain

Доменное ядро Calendar: агрегаты, value-объекты, спецификации и интерфейсы репозиториев/движка
повторений. Зависит от `Cheetah.Modules.Calendar.DomainEvents`, `…Shared`, `Cheetah.Core.Domain`,
`Cheetah.Core.Specification`, `Cheetah.Core.DataAccess`. **Не** зависит от Infrastructure/EF.

## Состав

| Тип | Назначение |
|---|---|
| `CalendarEvent` (агрегат) | Событие: период в UTC + IANA-таймзона, опц. серия (`RecurrenceRule`), привязка к сущности (`EntityType`/`EntityId`), участники/напоминания/override как граница инвариантов |
| `Calendar` | Контейнер событий |
| `EventAttendee`, `EventReminder`, `EventOccurrenceOverride` | Дочерние сущности события |
| `ReminderTrigger` (агрегат) | Материализованное срабатывание напоминания со своим жизненным циклом (горячий скан по `(Status, FireAtUtc)`) |
| `CalendarableEntityType` | Реестр типов сущностей, к которым можно привязывать события (как Tags) |
| `RecurrenceRule` (VO) | Строка RRULE (RFC 5545) + EXDATE |
| `RecurrenceKeys` | Канонический ключ экземпляра (RECURRENCE-ID) из UTC и обратно |
| `IRecurrenceExpander` / `Occurrence` | Раскрытие серии в экземпляры окна (реализация — в Infrastructure) |
| `ICalendarEventRepository`, `IReminderTriggerRepository` | Репозитории-интерфейсы (реализации — в Infrastructure) |
| `Specifications/*` | Спецификации фильтрации (окно, по календарю/сущности/участнику, due-триггеры и т.д.) |

## Правила

- Сеттеры приватные; изменение состояния — только через методы агрегата, публикующие доменные события.
- Вся фильтрация — через `Specification<T>`; репозитории принимают спецификации, не сырой LINQ.
- `IRecurrenceExpander` абстрагирует RRULE-движок, позволяя заменить его полноценным iCal без правок ядра.
