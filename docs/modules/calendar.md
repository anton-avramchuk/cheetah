# Cheetah.Modules.Calendar — проект модуля календаря

> Статус: реализовано. Код — в `src/Modules/Calendar/`, краткое руководство — в
> [`src/Modules/Calendar/README.md`](../../src/Modules/Calendar/README.md).
>
> **Отклонение от проекта:** `Ical.Net` недоступен в корпоративном NuGet-фиде (тянет `NodaTime`,
> которого там нет), поэтому RRULE-движок реализован собственный (`RRuleRecurrenceExpander`) за тем же
> интерфейсом `IRecurrenceExpander`. Покрытый поднабор: FREQ (DAILY/WEEKLY/MONTHLY/YEARLY), INTERVAL,
> COUNT, UNTIL, BYDAY (WEEKLY), BYMONTHDAY, EXDATE и override экземпляров. Полный iCal (BYSETPOS,
> ординальные BYDAY вида «2-й вторник», WKST≠MO) — follow-up: подменяется реализацией интерфейса.

## 1. Назначение и границы

**Что делает:** календарь уровня «аналог Google Calendar», но с двумя отличиями от обычного
персонального календаря:

1. **Привязка к произвольной сущности.** Любое событие (event) может быть привязано к сущности
   другого модуля — сделке, задаче, клиенту, заявке — через полиморфную ссылку
   `(EntityType, EntityId)`. Это даёт «таймлайн сущности»: все встречи/дедлайны/звонки по сделке.
2. **Рассылка оповещений перед событием.** На событие вешаются напоминания (reminders) с
   офсетом «за N до начала»; в нужный момент Calendar публикует `NotificationRequested`, и
   доставку (Email/Sms/Push) выполняет уже существующий конвейер `Notification`.

**Чего НЕ делает:**

- не доставляет уведомления сам — только формирует намерение (`NotificationRequested`).
  Каналы, контакты, шаблоны, opt-out — зона ответственности `Notification`/`Email`;
- не хранит бизнес-сущности потребителей — видит их как `(EntityType, EntityId)`;
- не управляет пользователями — участники адресуются `UserId` из `Identity` (логическая ссылка,
  без FK через границу модуля), контакты резолвит `Notification` из своей реплики;
- не реализует видеозвонки / вложения / совместное редактирование документов.

**Зафиксированные решения (по итогам уточнений):**

| Вопрос | Решение |
|---|---|
| Повторяющиеся события | **полный RRULE/iCal**: серия + правило, исключения (EXDATE) и override отдельных экземпляров |
| Форма модуля | **конкретный модуль** (как `Tags`): готовые сущности, своя БД, свои миграции |
| Получатели напоминаний | **участники = пользователи Identity** (`UserId`); рассылка через `NotificationRequested(RecipientUserId)` |
| Привязка к сущности | полиморфная пара `(EntityType, EntityId)` + реестр привязываемых типов (как в `Tags`) |
| Идентификатор | `Guid` |
| Хранилище | PostgreSQL (EF Core) |
| Часовые пояса | хранение в UTC + IANA TZ на событии; RRULE раскрывается в TZ события |

---

## 2. Архитектурная роль

```
   register types  ┌──────────────────────────────┐
  ┌──────────┐ →   │        Calendar Module        │
  │ CRM svc  │─────│  ┌─────────────────────────┐  │
  └──────────┘     │  │ Calendars               │  │ ← контейнеры событий (личные/сущностные/общие)
  ┌──────────┐ link│  ├─────────────────────────┤  │
  │ Deals    │─────│  │ Events (+ Recurrence)   │  │ ← разовые и серии (RRULE), привязка к (type,id)
  └──────────┘     │  ├─────────────────────────┤  │
                   │  │ Attendees               │  │ ← участники (UserId) + RSVP
                   │  ├─────────────────────────┤  │
                   │  │ Reminders               │  │ ← офсеты «за N до начала»
                   │  ├─────────────────────────┤  │
                   │  │ ReminderQueue (due rows)│  │ ← материализованные срабатывания (когда слать)
                   │  └────────────┬────────────┘  │
                   └───────────────┼───────────────┘
        scheduler scan (lock)      │ publish NotificationRequested (Outbox → Redis/Kafka)
                                   ▼
                          ┌──────────────┐   EmailRequested   ┌────────┐
                          │ Notification │ ─────────────────▶ │ Email  │
                          └──────────────┘                    └────────┘
```

Свой PostgreSQL, REST (+ gRPC при необходимости), события через шину — всё по `CLAUDE.md`.

---

## 3. Доменная модель

### 3.1. Агрегаты и сущности

| Тип | Базовый | Роль |
|---|---|---|
| `Calendar` | `AggregateRoot<Guid>` | контейнер событий; тип (Personal/Entity/Shared), владелец, TZ по умолчанию, цвет |
| `CalendarEvent` | `AggregateRoot<Guid>` | событие: заголовок, описание, локация, период, TZ, привязка, признак серии |
| `EventOccurrenceOverride` | `Entity<Guid>` (child of event) | переопределение/отмена конкретного экземпляра серии (RECURRENCE-ID) |
| `EventAttendee` | `Entity<Guid>` (child of event) | участник: `UserId`, роль, статус RSVP |
| `EventReminder` | `Entity<Guid>` (child of event) | правило напоминания: офсет до начала + канал-подсказка |
| `ReminderTrigger` | `AggregateRoot<Guid>` | материализованное срабатывание: «послать reminder X по экземпляру серии на момент T» |
| `CalendarableEntityType` | `AggregateRoot<Guid>` | реестр типов сущностей, к которым можно привязывать события |

**Почему `CalendarEvent` — отдельный агрегат, а Attendee/Reminder/Override — дети.**
Участники, напоминания и override-ы не имеют смысла без события и всегда меняются вместе с ним
в одной транзакции → это его инвариантная граница. А `ReminderTrigger` — отдельный агрегат:
он живёт по своему жизненному циклу (его сканирует и «гасит» планировщик), пишется/читается
независимо от события и оптимизирован под выборку «что пора слать».

### 3.2. `CalendarEvent` — ключевые поля

```csharp
public sealed class CalendarEvent : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    public Guid CalendarId { get; private set; }

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Location { get; private set; }

    // Период. Для all-day хранится дата без времени, IsAllDay = true.
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public string TimeZoneId { get; private set; } = "UTC";   // IANA, напр. "Europe/Moscow"
    public bool IsAllDay { get; private set; }

    // Привязка к сущности другого модуля (опционально).
    public string? EntityType { get; private set; }           // напр. "crm.deal"
    public Guid? EntityId { get; private set; }

    // Повторение. Null → разовое событие.
    public RecurrenceRule? Recurrence { get; private set; }   // value object вокруг RRULE

    public Guid OrganizerUserId { get; private set; }
    public EventStatus Status { get; private set; }           // Confirmed / Tentative / Cancelled

    private readonly List<EventAttendee> _attendees = new();
    private readonly List<EventReminder> _reminders = new();
    private readonly List<EventOccurrenceOverride> _overrides = new();
    public IReadOnlyCollection<EventAttendee> Attendees => _attendees;
    public IReadOnlyCollection<EventReminder> Reminders => _reminders;
    public IReadOnlyCollection<EventOccurrenceOverride> Overrides => _overrides;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
}
```

Мутаторы (каждый поднимает доменное событие и помечает напоминания «грязными» — см. §5):
`Schedule/Create`, `Reschedule(start,end,tz)`, `ChangeDetails`, `LinkToEntity/UnlinkEntity`,
`SetRecurrence/ClearRecurrence`, `AddAttendee/RemoveAttendee/RespondAttendee`,
`AddReminder/RemoveReminder`, `OverrideOccurrence/CancelOccurrence`, `Cancel`.

### 3.3. `RecurrenceRule` (value object, RRULE/iCal)

```csharp
public sealed class RecurrenceRule : ValueObject
{
    public string RRule { get; }                       // канонический RFC 5545, напр. "FREQ=WEEKLY;BYDAY=TU;UNTIL=..."
    public IReadOnlyList<DateTime> ExDatesUtc { get; }  // EXDATE — исключённые экземпляры
    // Override отдельных экземпляров живёт в EventOccurrenceOverride (RECURRENCE-ID),
    // т.к. это уже не «правило», а изменённые данные конкретного вхождения.
}
```

- Парсинг/раскрытие RRULE — **в Infrastructure** через библиотеку (`Ical.Net`), за интерфейсом
  `IRecurrenceExpander` (Domain). Domain хранит строку RRULE и инварианты, но не тянет iCal-зависимость.
- `IRecurrenceExpander.Expand(event, fromUtc, toUtc)` → последовательность `Occurrence(StartUtc, EndUtc, RecurrenceId)`
  с учётом EXDATE и override; используется и для отдачи календаря на фронт, и для планировщика напоминаний.

### 3.4. `EventReminder` и `ReminderTrigger` (ядро «оповещений перед событием»)

```csharp
public sealed class EventReminder : Entity<Guid>
{
    public Guid EventId { get; private set; }
    public TimeSpan OffsetBeforeStart { get; private set; } // напр. 15 мин, 1 день
    public ReminderTarget Target { get; private set; }       // Organizer / AllAttendees / AcceptedAttendees
    public string? ForceChannel { get; private set; }        // подсказка Notification (обычно null → решает роутер)
}

public sealed class ReminderTrigger : AggregateRoot<Guid>
{
    public Guid EventId { get; private set; }
    public Guid ReminderId { get; private set; }
    public string OccurrenceKey { get; private set; } = null!; // RECURRENCE-ID экземпляра (для серий)
    public DateTime FireAtUtc { get; private set; }            // момент срабатывания = OccurrenceStart - Offset
    public ReminderTriggerStatus Status { get; private set; }  // Pending / Sent / Skipped / Cancelled
    public DateTime? SentAt { get; private set; }
    // уникальность (EventId, ReminderId, OccurrenceKey) — защита от дублей при пересчёте
}
```

**Почему материализуем `ReminderTrigger`, а не считаем «на лету».** Скан «что пора слать» должен
быть дешёвым индексным запросом `WHERE Status=Pending AND FireAtUtc <= now`. Для серий с RRULE
вычислять следующее срабатывание на каждом тике дорого и плохо индексируется. Поэтому держим
**горизонт материализации** (rolling window, напр. 60 дней вперёд): фоновая задача раскрывает
серии в конкретные `ReminderTrigger` на горизонт, а скан напоминаний работает по плоской таблице.

### 3.5. `CalendarableEntityType` (реестр привязываемых типов, как в Tags)

Calendar не знает заранее про «сделку» или «задачу». Потребитель **на старте регистрирует** свои
типы через `Client` (см. §8), точь-в-точь как `Tags`:

```csharp
services.AddCalendarableEntityType("crm.deal", "Сделка", o =>
{
    o.DefaultColor = "#3b82f6";
    o.AllowMultiplePerEntity = true;     // несколько событий на одну сущность
});
```

Регистрация задаёт допустимые `EntityType`, человекочитаемое имя и политики; все операции
привязки валидируются по этому каталогу.

---

## 4. Слои и состав сборок

Стандартные 8 сборок + Client (есть server-to-server потребность — регистрация типов):

```
Calendar.DomainEvents   → Core.Events         события EventScheduled/Rescheduled/Cancelled,
                                               AttendeeInvited/Responded, ReminderDue (внутр.)
Calendar.Shared         → Core                 enum'ы (CalendarType, EventStatus, RSVP,
                                               ReminderTarget, TriggerStatus), константы, ключи шаблонов
Calendar.Contracts      → Core + Shared        DTO/Request (даты как DateTimeOffset/строки, RRULE — строка)
Calendar.Domain         → DomainEvents + Spec  агрегаты, VO RecurrenceRule, IRecurrenceExpander,
                                               спецификации
Calendar.Infrastructure → Domain + EF + EF.PG  DbContext, конфигурации, репозитории, Ical.Net-экспандер,
                                               миграции, фоновые задачи (планировщик)
Calendar.Application    → Domain + Contracts    CQRS-handler'ы, проекция occurrences,
                          + CQRS + Events        материализация ReminderTrigger, публикация NotificationRequested
Calendar.Api            → Application + AspNetCore  Minimal API /api/calendars/**, /api/calendar-events/**
Calendar.Client         → Contracts            HTTP-клиент + авто-регистрация типов при старте
Tests: Domain.Tests, Application.Tests, Client.Tests
```

Зависимости строго по `CLAUDE.md`: Application зависит только от Domain (репозитории —
интерфейсы в Domain), Infrastructure отдельно. Фильтрация — только через `Specification<T>`.

---

## 5. Конвейер напоминаний (главная механика)

### Шаг 1. Изменение события → пометка напоминаний к пересчёту

Любой мутатор, влияющий на тайминг или участников (`Reschedule`, `SetRecurrence`,
`AddReminder`, `RemoveReminder`, `Cancel`, `AddAttendee`…), поднимает доменное событие и логически
инвалидирует материализованные `ReminderTrigger` события. Обработчик в Application
**пересобирает** триггеры на горизонт для затронутого события (idempotent upsert по ключу
`(EventId, ReminderId, OccurrenceKey)`; уже отправленные — не трогаем, отменённые экземпляры → `Cancelled`).

### Шаг 2. Фоновая материализация горизонта

`MaterializeRemindersTask : PeriodicBackgroundTask` (из `Cheetah.BackgroundTasks`), напр. раз в час:

- берёт серии, у которых материализовано меньше, чем на горизонт (`HorizonDays`);
- через `IRecurrenceExpander` раскрывает ближайшие вхождения и докидывает `ReminderTrigger`.

Это «подметание» гарантирует, что у бесконечных серий всегда есть готовые триггеры на окно вперёд.

### Шаг 3. Фоновый скан «пора слать» → публикация `NotificationRequested`

`DispatchDueRemindersTask : PeriodicBackgroundTask`, напр. раз в минуту:

```
под распределённой блокировкой (Cheetah.DistributedLock.Postgres — чтобы в кластере
скан выполнял один инстанс):

  due = ReminderTriggerByDueSpecification(now, batchSize)   // Status=Pending AND FireAtUtc<=now
  для каждого trigger:
      event = load(trigger.EventId)
      если event отменён / occurrence в EXDATE / override Cancelled → trigger.Skip(); continue
      recipients = resolve(event, reminder.Target)          // UserId организатора/участников
      для каждого userId:
          eventBus.Publish(new NotificationRequested(
              NotificationId: deterministicGuid(trigger.Id, userId), // идемпотентность сквозная
              RecipientUserId: userId,
              TemplateKey: "calendar.reminder",
              Category: "System",
              Data: { title, startLocal, location, entityType, entityId, ... },
              ForceChannel: reminder.ForceChannel))
      trigger.MarkSent()
  SaveChangesAsync()   // агрегаты + Outbox-строки атомарно (Outbox → Redis/Kafka)
```

**Почему так безопасно:**
- **Идемпотентность** — `NotificationId = deterministicGuid(triggerId, userId)`; повтор (краш между
  publish и commit) отбрасывается на стороне `Notification` по `NotificationId` (см. её README).
- **Атомарность** — публикация ДО `SaveChangesAsync`: `OutboxEventBus` кладёт событие в
  `OutboxMessages` того же `CalendarDbContext`, один commit фиксирует `trigger.Sent` + outbox-строку.
- **Без дублей в кластере** — распределённый лок на скан; плюс `(EventId,ReminderId,OccurrenceKey)`
  unique-constraint на сам триггер.
- **Без PII в Calendar** — шлём только `RecipientUserId`; email/телефон резолвит `Notification`.

### Шаг 4. Шаблоны

Шаблон `calendar.reminder` (subject/body с merge-полями) живёт в `Notification` (его
`ITemplateRenderer`), а не в Calendar — Calendar лишь поставляет `Data`. Ключ шаблона —
константа в `Calendar.Shared` (`CalendarTemplates.Reminder`), используется обеими сторонами.

---

## 6. Связь с привязанной сущностью (жизненный цикл)

- При `LinkToEntity(entityType, entityId)` валидируем `entityType` по реестру `CalendarableEntityType`.
- **Удаление сущности-владельца.** Подписка на доменные события других модулей (напр.
  `DealDeleted`) — опциональна и добавляется точечно: handler отменяет/архивирует связанные события
  (мягко, `Cancel` + `RemovedAt`). По умолчанию из коробки нет жёсткого FK через границу модуля —
  как с `OwnerId` в `Customer`.
- Запрос «таймлайн сущности»: `ListEventsByEntityQuery(entityType, entityId, fromUtc, toUtc)` →
  раскрывает серии в occurrences в окне и отдаёт плоский список.

---

## 7. CQRS / API (эскиз)

**Commands:** `CreateCalendar`, `CreateEvent`, `UpdateEventDetails`, `RescheduleEvent`,
`SetEventRecurrence`, `CancelEvent`, `AddAttendee`, `RespondToInvite`, `AddReminder`,
`RemoveReminder`, `OverrideOccurrence`, `CancelOccurrence`, `LinkEventToEntity`.

**Queries:** `GetEventById`, `ListEventsByCalendar(range)`, `ListEventsByEntity(type,id,range)`,
`ListMyAgenda(userId,range)`, `GetOccurrences(eventId,range)`.

**Minimal API (только Minimal API, эндпоинты в `OnApplicationInitialization`):**

```
POST   /api/calendars
POST   /api/calendars/{calendarId}/events
PATCH  /api/calendar-events/{eventId}
POST   /api/calendar-events/{eventId}/reschedule
POST   /api/calendar-events/{eventId}/recurrence
DELETE /api/calendar-events/{eventId}                 (Cancel + soft-delete)
POST   /api/calendar-events/{eventId}/attendees
POST   /api/calendar-events/{eventId}/attendees/me/response   (RSVP)
POST   /api/calendar-events/{eventId}/reminders
GET    /api/calendars/{calendarId}/events?from=&to=
GET    /api/calendar-events/by-entity/{entityType}/{entityId}?from=&to=
GET    /api/agenda?from=&to=                           (повестка текущего пользователя)
```

Маппинг Request↔Command — через `IObjectMapper` (`CrmMapsterModule`). DTO дат — `DateTimeOffset`,
RRULE — строка; VO наружу не протекают.

---

## 8. Client и регистрация типов (как в Tags)

```csharp
public interface ICalendarClient
{
    ValueTask<Guid> CreateEventAsync(CreateEventRequest request, CancellationToken ct = default);
    ValueTask<IReadOnlyList<EventOccurrenceDto>> GetByEntityAsync(
        string entityType, Guid entityId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
}
```

`CalendarRegistrationSyncService : BackgroundService` (в Client) при старте пушит
зарегистрированные `CalendarableEntityType` в сервис Calendar — повторяя проверенный паттерн
`TagsRegistrationSyncService` / `RemoteRegistrySyncService`.

---

## 9. Хранилище (PostgreSQL, EF Core)

Таблицы: `Calendars`, `CalendarEvents`, `EventAttendees`, `EventReminders`,
`EventOccurrenceOverrides`, `ReminderTriggers`, `CalendarableEntityTypes`.

Ключевые индексы:

| Таблица | Индекс | Зачем |
|---|---|---|
| `CalendarEvents` | `(CalendarId, StartUtc)` | выборка календаря на диапазон |
| `CalendarEvents` | `(EntityType, EntityId)` | таймлайн сущности |
| `EventAttendees` | `(UserId, EventId)` | повестка пользователя |
| `ReminderTriggers` | `(Status, FireAtUtc)` | **горячий путь** скана «пора слать» |
| `ReminderTriggers` | unique `(EventId, ReminderId, OccurrenceKey)` | защита от дублей |

`RecurrenceRule` — owned type (RRULE-строка + EXDATE как `jsonb`/`text[]`).
В каждой EF-конфигурации обязателен `builder.Ignore(e => e.DomainEvents)`.
Подключение Outbox (как в `Notification`/`Email`) — `AddOutbox`-миграция для атомарной публикации.
Миграции и `IDesignTimeDbContextFactory` — внутри `Calendar.Infrastructure` (модуль конкретный).

---

## 10. Зависимости модуля

| Зависимость | Зачем |
|---|---|
| `Cheetah.Core.*` (Domain/CQRS/Events/Specification/DataAccess) | каркас |
| `Cheetah.Core.EntityFramework` + `.PostgreSql` | хранилище |
| `Cheetah.Core.Outbox(.PostgreSql)` | атомарная публикация `NotificationRequested` |
| `Cheetah.BackgroundTasks` | планировщик материализации и скана напоминаний |
| `Cheetah.DistributedLock.Postgres` | единичный исполнитель скана в кластере |
| `Cheetah.Modules.Notification.DomainEvents` | контракт `NotificationRequested` (только Events!) |
| `Ical.Net` (Infrastructure) | парсинг/раскрытие RRULE за `IRecurrenceExpander` |
| `Cheetah.AspNetCore` / `CrmMapsterModule` | Minimal API + маппинг |

Calendar зависит от Notification **только на уровне `DomainEvents`** (контракт события) — по
правилу «другие модули зависят только от Events».

---

## 11. Открытые вопросы / follow-up

- **Free/busy и конфликты** (наложение встреч участника) — вне MVP, добавляется запросом
  `GetUserBusyQuery` поверх `EventAttendees`.
- **Приглашения наружу по email** (гости без аккаунта Identity) — сейчас участники только `UserId`;
  внешние адресаты потребуют отдельной ветки доставки (как обсуждалось — отложено).
- **iCal экспорт/импорт (.ics) и CalDAV** — модель совместима (храним RRULE), но это отдельный слайс.
- **Часовые пояса при переносе DST** — раскрытие RRULE в TZ события через `Ical.Net`; покрыть тестами
  переход летнее/зимнее время.
- **Inbox-идемпотентность** на стороне Calendar (если подписываемся на удаление сущностей) — по
  тому же паттерну, что отмечен follow-up в `Notification`.

---

## 12. Порядок реализации

1. `DomainEvents` → `Shared` (enum'ы, ключи шаблонов) → `Contracts`.
2. `Domain`: агрегаты, `RecurrenceRule`, `IRecurrenceExpander`, спецификации (+ Domain.Tests).
3. `Infrastructure`: DbContext, конфигурации, репозитории, `Ical.Net`-экспандер, миграции (Initial + AddOutbox).
4. `Application`: CQRS события/календари, материализация `ReminderTrigger`, проекция occurrences (+ Application.Tests).
5. Фоновые задачи: `MaterializeRemindersTask`, `DispatchDueRemindersTask` (+ distributed lock).
6. `Api`: Minimal API.
7. `Client` + `CalendarRegistrationSyncService` (+ Client.Tests).
8. Все проекты — в `Cheetah.slnx` в папке `/Modules/Calendar/`.
</content>
</invoke>
