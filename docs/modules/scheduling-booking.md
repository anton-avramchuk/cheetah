# Cheetah.Modules.Booking.* — модуль «Scheduling / Booking» (Calendly внутри CRM, расширяемый шаблон)

> Статус: **реализовано (MVP).** Код — в `src/Modules/Booking/` (10 сборок: 7 шаблонных + `.Default` +
> `Client` + 3 тестовых). Тесты зелёные: Domain 17 (слот-движок + инварианты), Application 13
> (анти-дабл-букинг, слоты, lifecycle), Client 5. Гайд по расширению — `src/Modules/Booking/README.md`.
> Предпосылка free/busy в Calendar реализована (`docs/modules/calendar-free-busy.md`). Документ —
> пошаговый план сборки модуля по канону `CLAUDE.md` (Events → Shared → Contracts → Domain →
> Infrastructure → Application → Api (+ Client)), своя БД PostgreSQL, общение через REST + события через шину.
>
> Источник: раздел [§12](../plans.md) общего плана. Это **п.5 рекомендуемого порядка реализации**
> (после Activities + готового free/busy в Calendar; вместе с Notes & Timeline закрывает Tier 2).
>
> **Tier 2.** Публичные страницы записи: внешний человек (часто лид/клиент) сам выбирает свободный
> слот у сотрудника и бронирует встречу. CRM-нативный аналог Calendly. Надстройка над модулем
> **Calendar** (источник занятости и итоговых событий) + **Notification** (подтверждения/напоминания)
> + опционально **Leads / Activities** (создать лид/встречу из брони).

---

## 0. Главное требование — расширяемость

> **Модуль обязан быть расширяемым: сущности и ViewModel должны допускать расширение полями
> приложения-наследника без форка модуля** — ровно как уже реализованные
> [`Cheetah.Modules.Activities`](./activities.md) и [`Cheetah.Modules.Customer`](../../src/Modules/Customer/README.md).

Поэтому Booking строится **не** как «конкретный» модуль (паттерн Deals — `sealed`-сущности и закрытые
DTO), а как **абстрактный шаблон-модуль**: модуль поставляет **только абстрактные базовые типы и
generic-хелперы**, а наследник дописывает свои `sealed`-типы со своими полями.

**Что это даёт наследнику:**

- `sealed class Booking : BookingBase` со своими полями (например, `Department`, `UtmSource`, `Amount`);
- `sealed class BookingType : BookingTypeBase` со своими настройками страницы записи;
- `sealed record BookingDto : BookingDtoBase` со своими полями в ответе API;
- свои `Create/Update`-Request-ы (наследники `…RequestBase`);
- рабочий слот-движок, публичные эндпоинты записи, анти-дабл-букинг и события — «из коробки», дописав
  ~8–10 классов (или подключив сборку `.Default`, см. §3.1).

**Три уровня расширяемости** (закрываются на этапе проектирования):

1. **Структурная (compile-time).** Наследование `BookingBase`/`BookingTypeBase`/`*DtoBase` +
   `ConfigureCustom` hook в EF-конфигурации + фабрика/проектор наследника. Сильная типизация, индексы
   по доп. полям.
2. **Динамическая (runtime, без миграций).** `intake`-вопросы (`BookingAnswer`) уже делают форму брони
   предметно-нейтральной; доп. атрибуты без схемы — через будущий
   [Custom Fields](../plans.md) (`jsonb` по `(EntityType="crm.booking", EntityId)`). На MVP —
   опциональная колонка `Attributes (jsonb)` на `BookingBase`/`BookingTypeBase` как «быстрый» карман.
3. **Поведенческая.** `virtual`-мутаторы сущности, `virtual`-методы эндпоинтов; **слот-движок —
   подменяемый** через порт `ISlotEngine` (наследник может заменить стратегию сетки слотов, не трогая
   остальное); политика «что создать на `BookingConfirmed`» (Lead/Activity) — точка расширения.

> **Почему шаблон, а не Deals-стиль.** Бизнесы по-разному размечают брони (отдел, канал, стоимость
> платной консультации, доп. поля гостя), поэтому фиксированная схема почти всегда требует форка.
> Шаблон-подход (как Activities) снимает это.

---

## ⚠️ 0.1. Предусловие-блокер — free/busy в Calendar

Booking берёт **занятость host'а** из Calendar и вычитает её из доступности. Сейчас
`ICalendarClient` (`src/Modules/Calendar/.../ICalendarClient.cs`) умеет только:

- `CreateEventAsync(calendarId, request)` — создать событие;
- `GetByEntityAsync(entityType, entityId, from, to)` — события **по сущности** (не по пользователю).

**Метода free/busy по пользователю нет.** Это зафиксированный follow-up Calendar
([plans.md §12.8 п.1](../plans.md)). **Детальный мини-план — [calendar-free-busy.md](./calendar-free-busy.md)**
(объём небольшой: переиспользует `EventsByAttendeeInRangeSpecification` + `IRecurrenceExpander`, уже
готовые в Calendar). До старта Booking нужно добавить в Calendar:

- `GetUserBusyQuery(Guid hostUserId, DateTimeOffset from, DateTimeOffset to) : IQuery<IReadOnlyList<BusyIntervalDto>>`
  — раскрытые занятые интервалы (с учётом RRULE-разворота и `EventOccurrenceOverride`), в UTC;
- эндпоинт `GET /api/calendar/users/{hostUserId}/busy?from=&to=`;
- метод клиента `ICalendarClient.GetUserBusyAsync(hostUserId, fromUtc, toUtc, ct)`.

> Это **первый шаг** плана реализации (Фаза 0, см. §11). Без него слот-движок не на чем строить —
> двойного источника истины по занятости заводить нельзя.

---

## 1. Назначение и границы

**Что делает:**

- сотрудник (**host**) заводит **тип встречи** (booking page): «Вводный звонок 30 мин» — длительность,
  тип (видео/телефон/офис), буферы до/после, минимальный запас по времени (min-notice), горизонт
  планирования (max-advance), intake-вопросы;
- ведёт **расписание доступности** host'а (рабочие часы по дням недели + исключения/выходные, таймзона);
- по публичной ссылке внешний **invitee** видит **свободные слоты** (доступность − занятость из
  Calendar − буферы) в своей таймзоне и бронирует;
- при подтверждении создаётся `CalendarEvent` (через `Calendar.Client`), invitee и host получают
  подтверждение, ставятся напоминания; опционально создаётся `Lead`/`Activity`;
- self-service перенос/отмена по `ManageToken` из письма (без логина).

**Чего НЕ делает:**

- не хранит занятость сам — берёт free/busy из Calendar (источник истины по событиям);
- не доставляет письма/SMS — публикует события, доставка в Notification;
- не управляет RRULE-сериями — итоговая встреча создаётся как **разовое** событие Calendar;
- не управляет пользователями — `HostUserId` это логическая ссылка на Identity (без FK через границу).

**Связи (по `Id`, без FK через границу модуля):** `HostUserId` (Identity), `CalendarEventId?`
(Calendar), `CreatedLeadId?` (Leads), `CreatedActivityId?` (Activities).

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Форма модуля | **абстрактный шаблон** (как Activities/Customer) — расширяемые сущности/DTO. См. §0 |
| Идентификатор | `Guid` |
| Хранилище | PostgreSQL (EF Core); миграции — **у наследника** / в сборке `.Default` |
| Источник занятости | Calendar (`ICalendarClient.GetUserBusyAsync`) — единственный источник истины (§0.1) |
| Слот-движок | порт `ISlotEngine` — **чистая функция**, юнит-тесты без БД (§4.6) |
| Статус брони | enum `BookingStatus` под `IStateMachineEntity<BookingStatus>` (валидные переходы, §4.5) |
| Анти-дабл-букинг | `DistributedLock` по `booking:{host}:{slotUtc}` + повторная проверка занятости + уник. индекс (§6.2) |
| Invitee | **без аккаунта Identity** — контакты хранит сам Booking; в `CalendarEvent` организатор host |
| Публичные эндпоинты | анонимные, за `RateLimit` (+опц. captcha); управление по `ManageToken` |
| События | публикуются через `IEventBus` после `SaveChangesAsync` (паттерн Activities; Outbox — follow-up) |
| Эндпоинты | Customer-стиль (`EndpointsBase` + `IDispatcher` + `virtual`-методы) — generic-CQRS несовместим с генератором |
| «Быстрый» карман расширения | опц. `Attributes (jsonb)` на `BookingBase`/`BookingTypeBase` (MVP), полноценно — Custom Fields |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Free/busy из Calendar** (§0.1) — предпосылка №1. Форма `BusyIntervalDto`, поведение при
   недоступности Calendar (fail-closed: не показывать слоты vs fail-open).
2. **Поставлять ли «готовую» реализацию по умолчанию** — рекомендация: **гибрид**, сборка
   `Cheetah.Modules.Booking.Default` (sealed-типы + `DbContext` + миграции), чтобы работало «из
   коробки», оставаясь расширяемым (§3.1).
3. **Создание Lead/Activity из брони:** через Workflow-правило (когда появится Workflow) vs прямой
   оркестратор в Booking на MVP. Рекомендация MVP — прямой оркестратор + точка расширения
   `IBookingConfirmationHandler`.
4. **Шаг сетки слотов (`SlotStep`):** = длительности vs фиксированная сетка (15/30 мин). На MVP —
   параметр `BookingType.SlotStepMinutes?` (null → шаг = длительности); реализуется в `ISlotEngine`.
5. **Round-robin / коллективные встречи** (несколько host'ов на тип) — **вне MVP** (single-host),
   расширение позже (поле `HostUserId` → коллекция).
6. **Soft-delete** `BookingType`/`Booking` (`IRemovedAtEntity`) vs физическое удаление + query-filter —
   по образцу Customer: `Remove()` ставит `RemovedAt`.
7. **Имена контрактов локов/фоновых задач** (`IDistributedLock`/`IBackgroundTask`, формат расписания) —
   свериться с `Cheetah.DistributedLock(.Postgres)` / `Cheetah.BackgroundTasks` при реализации.

---

## 2. Архитектурная роль

```
   ┌──────────────────────────────────────────────────────────────┐
   │                     Booking Module (шаблон)                  │
   │  ┌────────────────────┐        ┌───────────────────────────┐ │
   │  │  BookingTypeBase   │        │  AvailabilityScheduleBase │ │
   │  │ (страница записи)  │        │  1──* WeeklyAvailabilityRule
   │  └─────────┬──────────┘        │  1──* AvailabilityDateOverride
   │            │                   └─────────────┬─────────────┘ │
   │            │   ┌──────────────┐               │               │
   │            └──▶│ ISlotEngine  │◀──────────────┘  (чистая ф-я) │
   │                └──────┬───────┘                               │
   │                       │ доступность − занятость − буферы      │
   │                       ▼                                       │
   │  ┌────────────────────────────┐                              │
   │  │  BookingBase 1──* Answer   │ (StateMachine BookingStatus) │
   │  └─────────┬──────────────────┘                              │
   │            │ наследует → sealed Booking/BookingType + Dto     │
   └────────────┼─────────────────────────────────────────────────┘
                │  ▲ GetUserBusyAsync / CreateEventAsync
                │  │ (Calendar.Client)              ┌──────────────┐
                │  └────────────────────────────────│   Calendar   │
                │ publish (IEventBus → Redis/Kafka) └──────────────┘
                ▼
   BookingConfirmed / Rescheduled / Cancelled
                ├──▶ Notification (письма invitee+host, напоминания)
                ├──▶ Leads      (создать лид из брони)
                ├──▶ Activities (встреча по сделке/контакту)
                └──▶ Timeline / Analytics
   ┌──────────────────────────────────────────────────────────────┐
   │ Анти-дабл-букинг: DistributedLock(booking:{host}:{slotUtc})   │
   │ + повторная проверка занятости внутри лока + уник. индекс     │
   └──────────────────────────────────────────────────────────────┘
```

---

## 3. Структура проектов

```
src/Modules/Booking/
├── Cheetah.Modules.Booking.DomainEvents/   # BookingConfirmed/Rescheduled/Cancelled/NoShow (Core.Events)
├── Cheetah.Modules.Booking.Shared/          # enums (BookingStatus, LocationKind), SlotDto-конвенции
├── Cheetah.Modules.Booking.Contracts/       # ABSTRACT DTO/Request базы + публичные SlotDto/PublicPageDto
├── Cheetah.Modules.Booking.Domain/          # abstract BookingBase/BookingTypeBase/AvailabilityScheduleBase,
│                                            #   ISlotEngine (+ реализация), generic-спеки
├── Cheetah.Modules.Booking.Infrastructure/  # abstract DbContextBase/ConfigBase, AddBookingInfrastructure<>,
│                                            #   (опц.) фоновая задача напоминаний/no-show
├── Cheetah.Modules.Booking.Application/      # generic handlers, IBookingFactory/Projector, оркестрация подтверждения
├── Cheetah.Modules.Booking.Api/             # abstract BookingEndpointsBase<> (публичные+приватные), ApiModuleBase
├── (опц.) Cheetah.Modules.Booking.Default/  # sealed Booking/BookingType + Contracts + DbContext + миграции «из коробки»
├── Cheetah.Modules.Booking.Client/          # HTTP-клиент server-to-server (если кто-то создаёт брони программно)
└── Tests/
    ├── Cheetah.Modules.Booking.Domain.Tests/        # ОБЯЗАТЕЛЬНО покрывает ISlotEngine
    ├── Cheetah.Modules.Booking.Application.Tests/
    └── Cheetah.Modules.Booking.Client.Tests/
```

**Порядок зависимостей (строго, как в Activities/Customer):**

```
DomainEvents (Core.Events)
   ↓
Shared (Core)
   ↓
Contracts (Core + Shared)                      ← ABSTRACT базовые DTO/Request + публичные SlotDto
   ↓
Domain (DomainEvents + Specification)          ← abstract сущности, ISlotEngine, generic-спеки
   ↓
Infrastructure (Domain + EF + EF.PostgreSql)   ← abstract DbContextBase/ConfigBase, AddBookingInfrastructure<>
Application (Domain + Contracts + CQRS + Events
            + Calendar.Client)                  ← generic handlers, оркестрация подтверждения брони
Api (Application + Contracts + AspNetCore)      ← abstract EndpointsBase<>, ApiModuleBase
   ↓
Default (наследует всё) + Client (Contracts)
```

> Канон `CLAUDE.md`: **Application зависит только на Domain** (не на Infrastructure); фильтрация —
> только через спецификации, не raw LINQ в хендлерах. Зависимость Application на `Calendar.Client` —
> это server-to-server клиент (Contracts-уровень), допустимо (как Booking→Calendar в плане §12.5).

### 3.1. Решение по «готовой реализации»

Рекомендация — **гибрид** (как у Activities): абстрактный шаблон + отдельная сборка
`Cheetah.Modules.Booking.Default` (sealed `Booking`/`BookingType`, конкретные DTO/Request,
`BookingDbContext` + миграции, готовые Api/Infrastructure/Application-регистрации). Приложение либо
подключает `.Default`, либо собирает свой набор по образцу §11.

---

## 4. Доменная модель (абстрактные базы)

### 4.1. Сводка

| Тип | Базовый | Роль |
|---|---|---|
| **`BookingTypeBase`** | `AggregateRoot<Guid>` + audit (+опц. `IRemovedAtEntity`) | тип встречи / публичная страница |
| **`AvailabilityScheduleBase`** | `AggregateRoot<Guid>` | недельная доступность host'а |
| **`WeeklyAvailabilityRule`** | `Entity<Guid>` (child) | окно в дне недели |
| **`AvailabilityDateOverride`** | `Entity<Guid>` (child) | исключение на дату |
| **`BookingBase`** | `AggregateRoot<Guid>` + `IStateMachineEntity<BookingStatus>` + audit | сама бронь |
| **`BookingAnswer`** | `Entity<Guid>` (child) | ответ на intake-вопрос |
| `ISlotEngine` | Domain (порт + реализация) | вычисление свободных слотов (чистая функция) |
| generic `Specification<T>` | Domain | `BookingTypeBySlug`, `ScheduleByHost`, `ActiveBookingsByHostInRange`, `BookingByManageToken` |

> Базовые классы ядра (сверено по репозиторию, как в Activities): аудит-интерфейсы
> `ICreateAtEntity`/`IUpdatedAtEntity` объявляют **`DateTimeOffset?`** (не `DateTime`) — использовать
> его. `AddDomainEvent`/`DomainEvents`/`ClearDomainEvents` — на `AggregateRoot`.

### 4.2. Shared — enums и конвенции

```csharp
namespace Cheetah.Modules.Booking.Shared;

public enum BookingStatus  { Confirmed = 0, Rescheduled = 1, Cancelled = 2, NoShow = 3, Completed = 4 }
public enum LocationKind   { Video = 0, Phone = 1, InPerson = 2 }

// Конвенция полиморфной привязки — общая с Tags/Notes/Activities (сквозное решение №1 плана).
public static class EntityRefKeys
{
    public const string Booking = "crm.booking";   // EntityType итогового CalendarEvent / Custom Fields
}
```

### 4.3. `BookingTypeBase` и `AvailabilityScheduleBase` — точки расширения

```csharp
public abstract class BookingTypeBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid HostUserId { get; protected set; }
    public string Slug { get; protected set; } = null!;   // уникальный URL публичной страницы
    public string Name { get; protected set; } = null!;
    public int DurationMinutes { get; protected set; }
    public LocationKind LocationKind { get; protected set; }
    public string? LocationDetails { get; protected set; }
    public int BufferBeforeMinutes { get; protected set; }
    public int BufferAfterMinutes { get; protected set; }
    public int MinNoticeMinutes { get; protected set; }    // минимальный запас по времени
    public int MaxAdvanceDays { get; protected set; }      // горизонт планирования
    public int? SlotStepMinutes { get; protected set; }    // null → шаг = DurationMinutes (§1.2 п.4)
    public string? Color { get; protected set; }
    public bool IsActive { get; protected set; }
    public string? Attributes { get; protected set; }      // jsonb — «быстрый» карман (MVP)

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected BookingTypeBase() { } // EF + наследник

    protected void InitializeCore(Guid id, Guid hostUserId, string slug, string name,
        int durationMinutes, LocationKind locationKind, int maxAdvanceDays, int minNoticeMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (durationMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        Id = id; HostUserId = hostUserId; Slug = slug.Trim().ToLowerInvariant(); Name = name.Trim();
        DurationMinutes = durationMinutes; LocationKind = locationKind;
        MaxAdvanceDays = maxAdvanceDays; MinNoticeMinutes = minNoticeMinutes; IsActive = true;
    }

    public virtual void Deactivate() => IsActive = false;
}

public abstract class AvailabilityScheduleBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<WeeklyAvailabilityRule> _weekly = new();
    private readonly List<AvailabilityDateOverride> _overrides = new();

    public Guid HostUserId { get; protected set; }
    public string TimeZoneId { get; protected set; } = "UTC";   // таймзона host'а (IANA)
    public IReadOnlyList<WeeklyAvailabilityRule> WeeklyRules => _weekly;
    public IReadOnlyList<AvailabilityDateOverride> DateOverrides => _overrides;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected AvailabilityScheduleBase() { }
    // фабрика наследника + AddWeeklyRule/AddOverride …
}

// Child-entities — конкретные (расширение редко требуется; по образцу ActivityReminder в Activities).
public sealed class WeeklyAvailabilityRule : Entity<Guid>
{
    public Guid ScheduleId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
}

public sealed class AvailabilityDateOverride : Entity<Guid>
{
    public Guid ScheduleId { get; private set; }
    public DateOnly Date { get; private set; }
    public bool IsUnavailable { get; private set; }
    // если !IsUnavailable — список окон на дату (хранится сериализованно или дочерней таблицей)
}
```

### 4.4. `BookingBase` — агрегат брони (фрагмент)

По образцу `Booking` из [plans.md §12.4](../plans.md), адаптировано под расширяемость (`abstract` +
`InitializeCore` вместо публичной фабрики `Reserve`, `virtual`-мутаторы).

```csharp
public abstract class BookingBase : AggregateRoot<Guid>,
    IStateMachineEntity<BookingStatus>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<BookingAnswer> _answers = new();

    public Guid BookingTypeId { get; protected set; }
    public Guid HostUserId { get; protected set; }
    public string InviteeName { get; protected set; } = null!;
    public string InviteeEmail { get; protected set; } = null!;
    public string? InviteePhone { get; protected set; }
    public string InviteeTimeZone { get; protected set; } = "UTC";
    public DateTimeOffset StartUtc { get; protected set; }
    public DateTimeOffset EndUtc { get; protected set; }
    public BookingStatus Status { get; protected set; }
    public Guid? CalendarEventId { get; protected set; }
    public Guid? CreatedLeadId { get; protected set; }
    public Guid? CreatedActivityId { get; protected set; }
    public string ManageToken { get; protected set; } = null!;   // одноразовый неугадываемый токен
    public string? CancelReason { get; protected set; }
    public string? Attributes { get; protected set; }            // jsonb — карман расширения (MVP)
    public IReadOnlyList<BookingAnswer> Answers => _answers;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public BookingStatus State => Status;   // IStateMachineEntity

    protected BookingBase() { } // EF + наследник

    /// <summary>Инициализация ядра — вызывается фабрикой/Create наследника (замена new + Reserve).</summary>
    protected void InitializeCore(Guid id, BookingTypeBase type, DateTimeOffset startUtc,
        string inviteeName, string inviteeEmail, string inviteeTimeZone, string? inviteePhone,
        IEnumerable<BookingAnswer> answers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inviteeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(inviteeEmail);
        Id = id; BookingTypeId = type.Id; HostUserId = type.HostUserId;
        StartUtc = startUtc; EndUtc = startUtc.AddMinutes(type.DurationMinutes);
        InviteeName = inviteeName.Trim(); InviteeEmail = inviteeEmail.Trim();
        InviteePhone = inviteePhone; InviteeTimeZone = inviteeTimeZone;
        Status = BookingStatus.Confirmed;
        ManageToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        _answers.AddRange(answers);
        AddDomainEvent(new BookingConfirmedIntegrationEvent(
            Id, type.Id, type.HostUserId, StartUtc, EndUtc, InviteeName, InviteeEmail));
    }

    public void AttachCalendarEvent(Guid calendarEventId) => CalendarEventId = calendarEventId;
    public void AttachLead(Guid leadId) => CreatedLeadId = leadId;
    public void AttachActivity(Guid activityId) => CreatedActivityId = activityId;

    public virtual void Reschedule(DateTimeOffset newStartUtc, int durationMinutes)
    {
        EnsureActive();
        StartUtc = newStartUtc; EndUtc = newStartUtc.AddMinutes(durationMinutes);
        Status = BookingStatus.Rescheduled;
        AddDomainEvent(new BookingRescheduledIntegrationEvent(Id, StartUtc, EndUtc));
    }

    public virtual void Cancel(string reason, bool byInvitee)
    {
        EnsureActive();
        Status = BookingStatus.Cancelled; CancelReason = reason;
        AddDomainEvent(new BookingCancelledIntegrationEvent(Id, reason, byInvitee));
    }

    public virtual void MarkNoShow()
    {
        if (Status is BookingStatus.Confirmed or BookingStatus.Rescheduled)
            Status = BookingStatus.NoShow;
    }

    public virtual void Complete()
    {
        if (Status is BookingStatus.Confirmed or BookingStatus.Rescheduled)
            Status = BookingStatus.Completed;
    }

    private void EnsureActive()
    {
        if (Status is BookingStatus.Cancelled or BookingStatus.NoShow or BookingStatus.Completed)
            throw new InvalidOperationException($"Booking {Id} is not active ({Status}).");
    }
}

public sealed class BookingAnswer : Entity<Guid>
{
    public Guid BookingId { get; private set; }
    public string Question { get; private set; } = null!;
    public string? Value { get; private set; }
}
```

### 4.5. StateMachine — конфигурация (в Application)

```csharp
services.AddStateMachine<BookingStatus>(sm => sm
    .From(BookingStatus.Confirmed).To(BookingStatus.Rescheduled, BookingStatus.Cancelled,
                                      BookingStatus.NoShow, BookingStatus.Completed)
    .From(BookingStatus.Rescheduled).To(BookingStatus.Rescheduled, BookingStatus.Cancelled,
                                        BookingStatus.NoShow, BookingStatus.Completed));
// Cancelled / NoShow / Completed — терминальные (нет исходящих переходов).
```

> API StateMachine свериться с `src/Cheetah.Core.StateMachine` (`AddStateMachine<TState>`,
> `IStateMachineValidator<TState>.Validate`, `IStateMachineEntity<TState>`,
> `InvalidStateTransitionException`). Валидатор вызывается в хендлерах перед мутатором.

### 4.6. `ISlotEngine` — ядро модуля (чистая функция, юнит-тесты без БД)

Главный расширяемый и **обязательно тестируемый** компонент. Порт в Domain + дефолтная реализация;
наследник может подменить стратегию сетки слотов.

```csharp
public interface ISlotEngine
{
    /// <summary>Доступность ∩ горизонт ∩ min-notice − занятость − буферы → слоты в таймзоне invitee.</summary>
    IReadOnlyList<SlotDto> ComputeSlots(
        BookingTypeBase type,
        AvailabilityScheduleBase schedule,
        IReadOnlyList<BusyInterval> busy,     // из Calendar, в UTC
        DateTimeOffset rangeFromUtc,
        DateTimeOffset rangeToUtc,
        string inviteeTimeZone,
        DateTimeOffset nowUtc);
}

public readonly record struct BusyInterval(DateTimeOffset StartUtc, DateTimeOffset EndUtc);
```

Псевдоалгоритм (из [plans.md §12.3](../plans.md)):

```
earliest = now + MinNotice ;  latest = now + MaxAdvance
window   = [max(rangeFrom, earliest) .. min(rangeTo, latest)]
для каждого дня D в window (в TZ расписания):
   дневные_окна = DateOverride(D) ?? WeeklyRules(D.DayOfWeek)
   для каждого окна W:
      t = W.Start
      пока t + Duration ≤ W.End:
         slotUtc  = [t .. t+Duration] → UTC
         expanded = slotUtc, расширенный на BufferBefore/BufferAfter
         если slotUtc ⊂ window и expanded ∩ busy = ∅: slots.add(slotUtc)
         t += SlotStep (= SlotStepMinutes ?? Duration)
вернуть slots, отрендеренные в inviteeTz
```

> Буферы расширяют **проверяемый** интервал, но не сам слот: invitee видит чистые 30 минут, а заняты
> 30 + буферы. `busy` приходят из Calendar уже в UTC. Конвертация TZ — через `TimeZoneInfo`
> (IANA-идентификаторы; на Windows может потребоваться `TimeZoneConverter` — свериться с
> `Directory.Packages.props`).

### 4.7. Domain — спецификации (фильтрация только через них)

```csharp
public sealed class BookingTypeBySlugSpecification(string slug) : Specification<BookingTypeBase>
{
    public override Expression<Func<BookingTypeBase, bool>> ToExpression()
        => t => t.Slug == slug && t.IsActive;
}

public sealed class ScheduleByHostSpecification(Guid hostUserId) : Specification<AvailabilityScheduleBase>
{
    public override Expression<Func<AvailabilityScheduleBase, bool>> ToExpression()
        => s => s.HostUserId == hostUserId;
}

// Активные брони host'а в окне — для повторной проверки занятости и уникальности слота.
public sealed class ActiveBookingsByHostInRangeSpecification<TBooking>(
        Guid hostUserId, DateTimeOffset fromUtc, DateTimeOffset toUtc)
    : Specification<TBooking> where TBooking : BookingBase
{
    public override Expression<Func<TBooking, bool>> ToExpression()
        => b => b.HostUserId == hostUserId
             && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Rescheduled)
             && b.StartUtc < toUtc && b.EndUtc > fromUtc;
}

public sealed class BookingByManageTokenSpecification<TBooking>(string token)
    : Specification<TBooking> where TBooking : BookingBase
{
    public override Expression<Func<TBooking, bool>> ToExpression() => b => b.ManageToken == token;
}
```

---

## 5. Contracts — расширяемые ViewModel + публичные DTO

Абстрактные `record`-базы (наследник добавляет поля через `init`). Публичные DTO слотов/страницы —
**конкретные** (наследнику расширять нечего, форма ответа фиксирована). Все DTO реализуют `ICrmResponse`.

```csharp
// ── расширяемые ──
public abstract record BookingDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public Guid BookingTypeId { get; init; }
    public Guid HostUserId { get; init; }
    public string InviteeName { get; init; } = null!;
    public string InviteeEmail { get; init; } = null!;
    public string? InviteePhone { get; init; }
    public DateTimeOffset StartUtc { get; init; }
    public DateTimeOffset EndUtc { get; init; }
    public BookingStatus Status { get; init; }
    public Guid? CalendarEventId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}

public abstract record BookingTypeDtoBase : ICrmResponse { /* Slug, Name, DurationMinutes, … */ }

public abstract record CreateBookingTypeRequestBase
{
    public string Slug { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int DurationMinutes { get; init; }
    public LocationKind LocationKind { get; init; }
    public int MinNoticeMinutes { get; init; }
    public int MaxAdvanceDays { get; init; }
    // буферы, цвет, slotStep …
}

// ── публичные конкретные ──
public sealed record SlotDto(DateTimeOffset StartUtc, DateTimeOffset EndUtc, string InviteeLocalTime);

public sealed record PublicBookingPageDto(string Slug, string Name, int DurationMinutes,
    LocationKind LocationKind, string? LocationDetails, IReadOnlyList<string> Questions);

public sealed record CreatePublicBookingRequest(DateTimeOffset StartUtc, string InviteeName,
    string InviteeEmail, string? InviteePhone, string InviteeTimeZone,
    IReadOnlyList<BookingAnswerDto> Answers);

public sealed record BookingAnswerDto(string Question, string? Value);
```

---

## 6. Application — generic CQRS + оркестрация подтверждения

Generic-handler'ы закрываются конкретными типами наследника (как в Activities). Создание сущностей —
через `IBookingFactory` (нельзя `new` абстракцию); проекция — через `IBookingProjector`.

```csharp
public interface IBookingFactory<TBooking, TBookingType>
    where TBooking : BookingBase where TBookingType : BookingTypeBase
{
    TBooking Create(TBookingType type, DateTimeOffset startUtc, CreatePublicBookingRequest request);
}

public interface IBookingProjector<TBooking, TDto>
    where TBooking : BookingBase where TDto : BookingDtoBase { TDto ToDto(TBooking booking); }

// Точка расширения «что сделать на подтверждённой брони» (Lead/Activity/собственное).
public interface IBookingConfirmationHandler<TBooking> where TBooking : BookingBase
{
    ValueTask OnConfirmedAsync(TBooking booking, BookingTypeBase type, CancellationToken ct);
}
```

### 6.1. Публичный слот-запрос

```csharp
public record GetAvailableSlotsQuery(string Slug, DateOnly From, DateOnly To, string InviteeTimeZone)
    : IQuery<IReadOnlyList<SlotDto>>;

// Handler: BookingTypeBySlug → ScheduleByHost → Calendar.GetUserBusyAsync(host, fromUtc, toUtc)
//        → ISlotEngine.ComputeSlots(...). Без записи в БД, AsNoTracking.
```

### 6.2. Команда создания брони (анти-дабл-букинг)

```csharp
public record CreateBookingCommand<TCreateRequest>(string Slug, TCreateRequest Request) : ICommand<Guid>;

// Канон хендлера (из plans.md §12.5, адаптировано под шаблон):
// 1. type = BookingTypeBySlug(slug) ?? NotFound
// 2. await using lock = await _lock.AcquireAsync($"booking:{type.HostUserId}:{startUtc:O}", ct)
//        ?? throw Conflict("Slot is being booked by someone else.")
// 3. busy = await _calendar.GetUserBusyAsync(type.HostUserId, startUtc, endUtc, ct)
//    if (busy overlaps) throw Conflict("Slot is no longer available.")   // повторная проверка внутри лока
// 4. booking = _factory.Create(type, startUtc, request);  _repo.Add(booking);  await SaveChangesAsync
// 5. eventId = await _calendar.CreateEventAsync(hostCalendarId, новое разовое событие);
//    booking.AttachCalendarEvent(eventId); await SaveChangesAsync
// 6. await _confirmationHandler.OnConfirmedAsync(booking, type, ct);   // опц. Lead/Activity
// 7. publish booking.DomainEvents через IEventBus; ClearDomainEvents
```

> Защита от гонки двух invitee на один слот: `DistributedLock` + повторная проверка занятости **внутри
> лока** + **уникальный индекс** `(HostUserId, StartUtc)` среди активных броней (§7). Если нужны
> компенсации (бронь создана, Calendar упал) — обернуть в `Cheetah.Saga` (follow-up); для MVP
> достаточно лока + проверки + идемпотентного создания события.

### 6.3. Прочие команды/запросы

```csharp
RescheduleBookingCommand(string ManageToken, DateTimeOffset NewStartUtc) : ICommand;  // self-service по токену
CancelBookingCommand(string ManageToken, string Reason, bool ByInvitee) : ICommand;
MarkNoShowCommand(Guid BookingId) : ICommand;                                          // host
Create/Update/DeactivateBookingTypeCommand<TRequest> …
Upsert AvailabilityScheduleCommand …

GetPublicPageQuery(string Slug) : IQuery<PublicBookingPageDto?>;
GetBookingByIdQuery<TDto>(Guid Id) : IQuery<TDto?>;
ListBookingsQuery<TDto>(Guid? HostUserId, BookingStatus? Status, DateTimeOffset? From, DateTimeOffset? To,
    int Page, int Size) : IQuery<IReadOnlyList<TDto>>;
```

**Регистрация** (открытые generic — через extension-метод, не `[Export]`):

```csharp
services.AddBookingApplication<Booking, BookingType, CreateBookingTypeRequest,
    BookingDto, BookingFactory, BookingProjector, BookingConfirmationHandler>();
```

---

## 7. Infrastructure — EF Core + индексы + (опц.) фоновые задачи

```csharp
public abstract class BookingConfigurationBase<TBooking> : IEntityTypeConfiguration<TBooking>
    where TBooking : BookingBase
{
    public void Configure(EntityTypeBuilder<TBooking> b)
    {
        b.ToTable("Bookings", "booking");
        b.Property(x => x.InviteeName).HasMaxLength(200).IsRequired();
        b.Property(x => x.InviteeEmail).HasMaxLength(320).IsRequired();
        b.Property(x => x.InviteeTimeZone).HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.ManageToken).HasMaxLength(64).IsRequired();
        b.Property(x => x.Attributes).HasColumnType("jsonb");
        b.HasMany(x => x.Answers).WithOne().HasForeignKey(a => a.BookingId).OnDelete(DeleteBehavior.Cascade);

        // Анти-дабл-букинг: уникальность активного слота host'а.
        b.HasIndex(x => new { x.HostUserId, x.StartUtc })
            .IsUnique()
            .HasFilter("\"Status\" IN (0,1)");   // Confirmed/Rescheduled (Npgsql partial index)
        b.HasIndex(x => x.ManageToken).IsUnique();
        b.HasIndex(x => new { x.HostUserId, x.Status, x.StartUtc });  // списки host'а

        b.Ignore(x => x.DomainEvents);  // CRITICAL
        ConfigureCustom(b);             // hook наследника: индексы/колонки доп. полей
    }

    protected virtual void ConfigureCustom(EntityTypeBuilder<TBooking> b) { }
}
// Аналогично BookingTypeConfigurationBase<> (уник. индекс по Slug) и AvailabilityScheduleConfigurationBase<>.
```

```csharp
public abstract class BookingDbContextBase<TContext, TBooking, TBookingType, TSchedule> : DbContext
    where TContext : DbContext where TBooking : BookingBase
    where TBookingType : BookingTypeBase where TSchedule : AvailabilityScheduleBase
{
    public DbSet<TBooking> Bookings => Set<TBooking>();
    public DbSet<TBookingType> BookingTypes => Set<TBookingType>();
    public DbSet<TSchedule> Schedules => Set<TSchedule>();
    protected BookingDbContextBase(DbContextOptions<TContext> o) : base(o) { }
    // OnModelCreating → ApplyConfiguration(CreateXxxConfiguration()) (абстрактные методы)
}
```

> Из Customer/Activities: **абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`,
> `IDesignTimeDbContextFactory` и миграции принадлежат наследнику (или сборке `.Default`).

**Регистрация:** `services.AddBookingInfrastructure<BookingDbContext, Booking, BookingType, Schedule>();`
— `AddDbContext` (`UseNpgsql`), репозитории `IRepository<…>`, `IDistributedLock`, регистрация
`ISlotEngine`-реализации.

**(Опц.) фоновые задачи** (один исполнитель в кластере, `DistributedLock`):
`DispatchBookingRemindersTask` (напоминания invitee/host — через тот же механизм, что Calendar) и
`MarkPastBookingsNoShowTask`/`CompleteTask`. На MVP можно отдать напоминания целиком Notification по
событию `BookingConfirmed` (как в Activities — follow-up).

---

## 8. Api (публичные + приватные эндпоинты)

Эндпоинты — Customer-стиль (`EndpointsBase` + `IDispatcher` + `virtual`-методы), как фактически сделано
в Activities (генератор `Cheetah.Backend.Endpoints` требует закрытых пар Request↔Command, что
несовместимо с generic-CQRS шаблона). Модуль-классы `partial`.

**Публичные эндпоинты** (анонимные, за `RateLimit` (+опц. captcha)):

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| GET | `/api/public/booking/{slug}` | `GetPublicPageQuery` |
| GET | `/api/public/booking/{slug}/slots?from=&to=&tz=` | `GetAvailableSlotsQuery` |
| POST | `/api/public/booking/{slug}` | `CreateBookingCommand` `{ start, invitee*, answers[] }` |
| GET | `/api/public/booking/manage/{token}` | бронь по `ManageToken` |
| POST | `/api/public/booking/manage/{token}/reschedule` | `RescheduleBookingCommand` |
| POST | `/api/public/booking/manage/{token}/cancel` | `CancelBookingCommand` |

**Приватные эндпоинты** (host, за `Cheetah.Permissions`):

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| GET/POST/PUT/DELETE | `/api/booking-types` | CRUD `BookingType` |
| GET/PUT | `/api/availability` | расписание доступности host'а (upsert) |
| GET | `/api/bookings?hostUserId=&status=&from=&to=&page=&size=` | `ListBookingsQuery` |
| GET | `/api/bookings/{id}` | `GetBookingByIdQuery` |
| POST | `/api/bookings/{id}/no-show` | `MarkNoShowCommand` |

`CheetahBookingApiModuleBase<…>` маппит эндпоинты в `OnApplicationInitialization`. Публичные роуты
помечаются `AllowAnonymous` + политикой `RateLimit`; приватные — авторизацией.

---

## 9. События (публикует Booking)

```csharp
namespace Cheetah.Modules.Booking.DomainEvents;

BookingConfirmedIntegrationEvent(Guid BookingId, Guid BookingTypeId, Guid HostUserId,
    DateTimeOffset StartUtc, DateTimeOffset EndUtc, string InviteeName, string InviteeEmail) : EventBase;
BookingRescheduledIntegrationEvent(Guid BookingId, DateTimeOffset StartUtc, DateTimeOffset EndUtc) : EventBase;
BookingCancelledIntegrationEvent(Guid BookingId, string Reason, bool ByInvitee) : EventBase;
BookingNoShowIntegrationEvent(Guid BookingId) : EventBase;
```

Потребители: **Notification** (письма invitee+host, напоминания), **Leads** (создать лид из брони),
**Activities** (встреча по сделке/контакту), Timeline/Analytics.

**Целостность.** Подписка на `BookingCancelled`/`BookingRescheduled` синхронизирует итоговое
`CalendarEvent` (отменить/перенести через `Calendar.Client`). Идемпотентность — через
`Cheetah.Core.Inbox`.

---

## 10. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | **`ISlotEngine`** (главное): буферы расширяют проверку но не слот; min-notice/max-advance отсекают; занятость вычитается; TZ-конвертация invitee≠host; `SlotStep` = длительность / фикс. сетка; DateOverride перекрывает WeeklyRule; пустое расписание → нет слотов. Инварианты `BookingBase` (терминальные статусы, `EnsureActive`, генерация `ManageToken`). Наследование (sealed-типы создаются фабрикой) |
| `Application.Tests` | хендлеры с моками `IRepository`/`IEventBus`/`ICalendarClient`/`IDistributedLock` (Moq): анти-дабл-букинг (лок не взят → Conflict; занятость появилась → Conflict); создание `CalendarEvent` + `AttachCalendarEvent`; публикация событий; reschedule/cancel по токену; StateMachine-валидатор |
| `Client.Tests` | сериализация запросов/ответов, обработка 404/409 |
| (Default) | интеграционный smoke: завести тип+расписание → получить слоты → забронировать → проверить уник. индекс/событие; миграция применяется |

---

## 11. План реализации (пошагово)

> Каждый шаг = отдельный коммит. После каждого слоя — `dotnet build` + `dotnet sln add` в
> `Cheetah.slnx`, папка `/Modules/Booking/`. Пакеты — через `Directory.Packages.props`
> (`PackageReference` без `Version`).

**Фаза 0 — предпосылка (Calendar free/busy) + каркас**
1. **В Calendar:** `GetUserBusyQuery` + эндпоинт `GET /api/calendar/users/{hostUserId}/busy` +
   `ICalendarClient.GetUserBusyAsync` + `BusyIntervalDto` + тесты (§0.1). **Без этого шага дальше нельзя.**
2. Создать проекты Booking по §3 (8 шаблонных + опц. `.Default` + `Client` + 3 тестовых), ссылки строго
   по порядку зависимостей. Добавить в `Cheetah.slnx`.

**Фаза 1 — контракты**
3. `DomainEvents`: 4 интеграционных события (§9).
4. `Shared`: enums (`BookingStatus`, `LocationKind`) + `EntityRefKeys` (§4.2).
5. `Contracts`: абстрактные `*DtoBase`/`*RequestBase` + публичные `SlotDto`/`PublicBookingPageDto`/
   `CreatePublicBookingRequest` (§5).

**Фаза 2 — домен (ядро ценности модуля)**
6. `Domain`: `BookingTypeBase`, `AvailabilityScheduleBase` (+child `WeeklyAvailabilityRule`/
   `AvailabilityDateOverride`), `BookingBase` (+child `BookingAnswer`) с `InitializeCore`/мутаторами (§4.3–4.4).
7. `Domain`: **`ISlotEngine` + дефолтная реализация** (§4.6) — чистый алгоритм слотов.
8. `Domain`: generic-спецификации (§4.7).
9. `Domain.Tests`: **сначала `ISlotEngine`** (домен без БД) + инварианты `BookingBase`.

**Фаза 3 — инфраструктура**
10. `Infrastructure`: `*ConfigurationBase<>` (+`ConfigureCustom`, уник. индексы анти-дабл-букинга §7),
    `BookingDbContextBase<>`.
11. `Infrastructure`: `AddBookingInfrastructure<>` (`AddDbContext`, репозитории, `IDistributedLock`,
    регистрация `ISlotEngine`). (Опц.) фоновые задачи напоминаний/no-show.

**Фаза 4 — приложение**
12. `Application`: `IBookingFactory`/`IBookingProjector`/`IBookingConfirmationHandler`, generic команды/
    запросы + хендлеры (§6), оркестрация подтверждения с локом и Calendar (§6.2).
13. `Application`: `AddStateMachine<BookingStatus>` (§4.5) + `AddBookingApplication<>`.
14. `Application.Tests`: анти-дабл-букинг, создание события, reschedule/cancel по токену.

**Фаза 5 — API + (Default) + клиент**
15. `Api`: `BookingEndpointsBase<>` (публичные `AllowAnonymous`+`RateLimit` и приватные) +
    `CheetahBookingApiModuleBase<>` (§8).
16. (Опц.) `.Default`: sealed `Booking`/`BookingType`, конкретные Contracts, фабрика/проектор,
    `BookingDbContext` + `IDesignTimeDbContextFactory` + **миграция** `InitialBooking` (схема `booking`),
    готовые регистрации.
17. `Client`: `IBookingClient` + реализация (если брони создаются программно), `Client.Tests`.

**Фаза 6 — интеграция**
18. Подписка на `BookingCancelled`/`BookingRescheduled` → синхронизация `CalendarEvent`; идемпотентность
    через Inbox. (Опц.) `IBookingConfirmationHandler` → создать Lead/Activity (§1.2 п.3).
19. End-to-end: тип+расписание → слоты (с учётом занятости host'а из Calendar) → бронь → событие в
    Calendar → reschedule/cancel по токену.

**Фаза 7 — финал**
20. README модуля (это **шаблонный** модуль → README обязателен по чек-листу `CLAUDE.md`: назначение,
    точки расширения, extension-методы, пример наследования — по образцу Activities/Customer README).
21. Обновить статус этого плана на «реализовано», добавить ссылку на код; обновить `MEMORY.md`.

---

## 12. Зависимости от инфраструктуры

| Инфраструктурный модуль | Использование в Booking |
|---|---|
| `Cheetah.Core.Domain` | `AggregateRoot`/`Entity`, аудит-интерфейсы (`DateTimeOffset?`) |
| `Cheetah.Core.StateMachine` | переходы `BookingStatus` |
| `Cheetah.Core.DataAccess` (`IRepository<T>`) | репозитории + спецификации |
| `Cheetah.Core.Specification` | вся фильтрация (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` | `ICommand`/`IQuery`/`IDispatcher`/`IEventBus` |
| `Cheetah.Core.Inbox` | идемпотентность синхронизации `CalendarEvent` по событиям |
| `Cheetah.DistributedLock(.Postgres)` | **анти-дабл-букинг** слота (ключевое) |
| `Cheetah.RateLimit` | защита публичных анонимных эндпоинтов записи |
| `Cheetah.BackgroundTasks` | (опц.) напоминания/no-show (один исполнитель + лок) |
| `Cheetah.Core.Security` | генерация `ManageToken` (`RandomNumberGenerator`) |
| `CrmEntityFrameworkModule` + `…PostgreSqlModule` | EF Core + Npgsql (partial unique index) |
| `Cheetah.AspNetCore` / `Cheetah.Backend.Endpoints` | эндпоинты (Customer-стиль) |
| `Cheetah.Permissions` | авторизация приватных эндпоинтов host'а |
| **`Cheetah.Modules.Calendar` (Client)** | **free/busy host'а (§0.1) + создание итогового события** |
| `Cheetah.Modules.Leads` / `Activities` (Client) | (опц.) создать лид/встречу из брони (`IBookingConfirmationHandler`) |
| `Cheetah.Saga` | (follow-up) компенсации при сбое создания события Calendar |

---

## 13. Отличия от исходного эскиза плана (`docs/plans.md` §12)

1. **Абстрактный шаблон вместо конкретного модуля** — по требованию расширяемости (§0). Эскиз §12
   давал `sealed Booking` с публичной фабрикой `Reserve`; здесь — `BookingBase`/`BookingTypeBase` +
   `InitializeCore` + generic-хелперы по образцу Activities/Customer.
2. **Аудит-поля — `DateTimeOffset?`** (сверено по `ICreateAtEntity`/`IUpdatedAtEntity` ядра), а не
   `DateTime` из эскиза.
3. **`ISlotEngine` — порт в Domain + подменяемая реализация** (точка расширения сетки слотов), а не
   просто внутренний сервис.
4. **События — через `IEventBus` после `SaveChangesAsync`** (паттерн Activities), Outbox — follow-up.
5. **Эндпоинты — Customer-стиль** (`EndpointsBase` + `virtual`), не raw Minimal API из эскиза:
   generic-CQRS несовместим с декларативным генератором.
6. **Опциональная сборка `.Default`** — чтобы модуль работал «из коробки», оставаясь расширяемым.
7. **`IBookingConfirmationHandler`** как явная точка расширения «что создать на подтверждённой брони»
   (Lead/Activity/своё) вместо хардкода оркестрации.
8. **`Attributes (jsonb)`** как «быстрый» карман расширения на MVP до появления Custom Fields.

**Предпосылка (блокер, §0.1):** free/busy в Calendar (`GetUserBusyQuery` / `GetUserBusyAsync`) — должна
быть реализована **до** старта Booking.

**Отложено (follow-up):** Saga-компенсации при сбое Calendar; round-robin/коллективные встречи;
интеграция внешних календарей (Google/Outlook free-busy); полноценная динамическая расширяемость через
Custom Fields; gRPC для горячих публичных слот-запросов; транзакционный Outbox.
