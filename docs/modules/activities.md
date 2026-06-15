# Cheetah.Modules.Activities.* — модуль «Задачи и активности» (расширяемый шаблон)

> Статус: **реализовано (MVP абстрактного шаблона).** Код — в `src/Modules/Activities/`. 9 проектов
> (7 шаблонных + 2 тестовых) собираются; тесты зелёные: Domain (18), Application (11) = 29. Сводка
> реализации и отличий от плана — в §14. Краткий гайд по расширению — `src/Modules/Activities/README.md`.
> Документ — пошаговый план сборки модуля по канону `CLAUDE.md`
> (Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client)),
> своя БД PostgreSQL, общение через REST/gRPC + события через шину.
>
> Источник: разделы [§2](../plans.md) и [§A](../plans.md) общего плана. Это **п.2 рекомендуемого
> порядка реализации** (после Deals).
>
> **Tier 1.** Активности цепляются ко всем сущностям CRM (`EntityType` + `EntityId`) — это «что нужно
> сделать»: задачи, звонки, встречи, письма-напоминания.

---

## 0. Главное требование — расширяемость

> **Модуль обязан быть расширяемым: сущности и ViewModel должны допускать расширение полями
> приложения-наследника без форка модуля.**

Поэтому Activities строится **не** как «конкретный» модуль (паттерн Deals — `sealed`-сущности и
закрытые DTO), а как **абстрактный шаблон-модуль** по образцу уже реализованного
[`Cheetah.Modules.Customer`](../../src/Modules/Customer/README.md): модуль поставляет **только
абстрактные базовые типы и generic-хелперы**, а наследник дописывает свои `sealed`-типы, добавляя
собственные поля.

**Что это даёт наследнику:**

- `sealed class Activity : ActivityBase` со своими полями (например, `CallOutcome`, `MeetingUrl`);
- `sealed record ActivityDto : ActivityDtoBase` со своими полями в ответе API;
- свои `Create/Update`-Request-ы (наследники `…RequestBase`);
- рабочий CRUD, фоновые задачи и события — «из коробки», дописав ~6–7 классов.

**Три уровня расширяемости** (закрываются на этапе проектирования, см. §1.1):

1. **Структурная (compile-time).** Наследование `ActivityBase`/`ActivityDtoBase` + `ConfigureCustom`
   hook в EF-конфигурации + фабрика/проектор наследника. Сильная типизация, индексы по доп. полям.
2. **Динамическая (runtime, без миграций).** Полиморфная привязка `(EntityType, EntityId)` уже делает
   модуль предметно-нейтральным; доп. атрибуты без схемы — через будущий модуль
   [Custom Fields](../plans.md) (`jsonb`-значения по `(EntityType, EntityId)`). В Activities на MVP —
   опциональная колонка `Attributes (jsonb)` на `ActivityBase` как «быстрый» карман расширения.
3. **Поведенческая.** `virtual`-мутаторы сущности, `virtual`-методы эндпоинтов, набор статусов/типов
   как **данные-расширяемые** точки (см. §4.5 — `ActivityType`/`ActivityStatus` enum vs справочник).

> **Почему не `Deals`-стиль.** Deals намеренно сделан конкретным (одна фиксированная схема сделки).
> Activities же по природе цепляется к произвольным доменам и почти всегда требует доп. полей под
> конкретный бизнес (исход звонка, ссылка на встречу, тип письма) — поэтому шаблон-подход оправдан.

---

## 1. Назначение и границы

**Что делает:** хранит активности — задачи (Task), звонки (Call), встречи (Meeting),
письма-напоминания (Email) — привязанные к произвольной сущности (`EntityType` + `EntityId`).
У активности есть тип, статус, приоритет, исполнитель (`AssigneeId`), владелец (`OwnerId`), срок
(`DueAt`), результат (`Result`), напоминания (`ActivityReminder`).

**Чего НЕ делает:**

- не управляет календарной раскладкой и RRULE — для повторяющихся встреч ссылается на Calendar-событие
  (`CalendarEventId?`);
- не шлёт уведомления сам — публикует `ActivityDueIntegrationEvent`, доставку делает Notification;
- не управляет пользователями — `AssigneeId`/`OwnerId` это логические ссылки на Identity (без FK через
  границу модуля).

**Связи (по `Id`, без FK через границу модуля):** `AssigneeId`, `OwnerId`, `(EntityType, EntityId)`,
`CalendarEventId?`.

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Форма модуля | **абстрактный шаблон** (как Customer) — расширяемые сущности/DTO. См. §0 |
| Идентификатор | `Guid` |
| Хранилище | PostgreSQL (EF Core); миграции — **у наследника** |
| Привязка к сущности | полиморфная `(EntityType: string, EntityId: Guid)` — единая конвенция с Tags/Notes |
| Статус активности | enum `ActivityStatus` под `IStateMachineEntity<ActivityStatus>` (валидные переходы) |
| Напоминания | child-entity `ActivityReminderBase` (агрегат `ActivityBase`) |
| Дедлайны/напоминания | фоновые задачи `Cheetah.BackgroundTasks` + `DistributedLock` (один исполнитель) |
| События | публикуются транзакционным Outbox (как в Deals/Calendar) |
| Эндпоинты | декларативные (`Cheetah.Backend.Endpoints` + генератор), методы `virtual` |
| «Быстрый» карман расширения | опц. `Attributes (jsonb)` на `ActivityBase` (MVP), полноценно — Custom Fields |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Поставлять ли «готовую» реализацию по умолчанию.** Customer не поставляет — наследник пишет всё
   сам. Рекомендация для Activities: **гибрид** — отдельная сборка
   `Cheetah.Modules.Activities.Default` с `sealed Activity`, конкретными Contracts, `DbContext` и
   миграциями, чтобы модуль работал «из коробки», оставаясь расширяемым (наследник может не
   подключать Default и собрать свой набор). Решение — §3.1.
2. **`EntityType`/`ActivityType`/`ActivityStatus` — enum vs справочник.** На MVP — enum (как в плане).
   Если потребуется добавлять типы без передеплоя — вынести в справочник-таблицу (точка расширения).
3. **`EntityDeletedIntegrationEvent`** — общий контракт (рекомендация плана, сквозное решение №2) vs
   per-module. Нужен для очистки висячих активностей удалённой сущности.
4. **Имена контрактов фоновых задач/локов** (`IBackgroundTask`/`IDistributedLock`, формат расписания)
   — свериться с `Cheetah.BackgroundTasks` и `Cheetah.DistributedLock.Postgres` при реализации.
5. **Soft-delete** активностей (`IRemovedAtEntity`) vs физическое удаление + query-filter. По образцу
   Customer: `Remove()` ставит `RemovedAt`, глобальный фильтр по умолчанию выключен.

---

## 2. Архитектурная роль

```
   ┌─────────────────────────────────────────────────────────┐
   │                   Activities Module (шаблон)            │
   │  ┌────────────────────┐      ┌────────────────────────┐ │
   │  │  ActivityBase      │ 1──* │  ActivityReminderBase  │ │ ← агрегат + напоминания
   │  │ (abstract,         │      └────────────────────────┘ │
   │  │  StateMachine по   │                                  │
   │  │  ActivityStatus)   │   (EntityType, EntityId)  ───────┼──▶ deal/customer/contact/lead/…
   │  └─────────┬──────────┘                                  │
   │            │ наследует                                   │
   │   sealed Activity (поля приложения) + ActivityDto (поля) │
   └────────────┼────────────────────────────────────────────┘
                │ publish (Outbox → Redis/Kafka)
                ▼
   ActivityCreated / Completed / Due / Overdue
                │
                ├──▶ Notification  (ActivityDue/Overdue → письмо/пуш)
                ├──▶ Timeline / Search / Analytics
                └──◀ EntityDeleted (очистка висячих активностей)
   ┌─────────────────────────────────────────────────────────┐
   │ Infrastructure: фоновые задачи (DistributedLock)        │
   │   ScanOverdueActivitiesTask · DispatchActivityRemindersTask
   └─────────────────────────────────────────────────────────┘
```

---

## 3. Структура проектов

```
src/Modules/Activities/
├── Cheetah.Modules.Activities.DomainEvents/   # ActivityCreated/Completed/Due/Overdue (Core.Events)
├── Cheetah.Modules.Activities.Shared/          # enums (ActivityType/Status/Priority), EntityRefKeys
├── Cheetah.Modules.Activities.Contracts/       # ABSTRACT DTO/Request базы (ActivityDtoBase, …RequestBase)
├── Cheetah.Modules.Activities.Domain/          # abstract ActivityBase/ActivityReminderBase, generic-спеки
├── Cheetah.Modules.Activities.Infrastructure/  # abstract DbContextBase/ConfigBase, AddActivitiesInfrastructure<>, фоновые задачи
├── Cheetah.Modules.Activities.Application/      # generic handlers, IActivityFactory/Projector, AddActivitiesApplication<>
├── Cheetah.Modules.Activities.Api/             # abstract ActivityEndpointsBase<>, ApiModuleBase
├── (опц.) Cheetah.Modules.Activities.Default/  # sealed Activity + конкретные Contracts + DbContext + миграции «из коробки»
├── Cheetah.Modules.Activities.Client/          # HTTP-клиент server-to-server (Workflow/Booking → Activity)
└── Tests/
    ├── Cheetah.Modules.Activities.Domain.Tests/
    ├── Cheetah.Modules.Activities.Application.Tests/
    └── Cheetah.Modules.Activities.Client.Tests/
```

**Порядок зависимостей (строго, как в Customer):**

```
DomainEvents (Core.Events)
   ↓
Shared (Core)
   ↓
Contracts (Core + Shared)                      ← ABSTRACT базовые DTO/Request
   ↓
Domain (DomainEvents + Specification)          ← abstract ActivityBase, generic-спеки
   ↓
Infrastructure (Domain + EF + EF.PostgreSql)   ← abstract DbContextBase/ConfigBase, AddActivitiesInfrastructure<>
Application (Domain + Contracts + CQRS + Events) ← generic handlers, AddActivitiesApplication<>
Api (Application + Contracts + AspNetCore)      ← abstract EndpointsBase<>, ApiModuleBase
   ↓
Default (наследует всё) + Client (Contracts)
```

> Канон `CLAUDE.md`: **Application зависит только на Domain** (не на Infrastructure); фильтрация —
> только через спецификации, не raw LINQ в хендлерах.

### 3.1. Решение по «готовой реализации»

Рекомендация — **гибрид**: модуль поставляет абстрактный шаблон + отдельную сборку
`Cheetah.Modules.Activities.Default`, дающую рабочий модуль «из коробки» (sealed `Activity`,
конкретные DTO/Request, `ActivitiesDbContext` + миграции, готовые Api-модули). Приложение либо
подключает `.Default`, либо пишет свой набор по образцу §11. Это снимает главный минус Customer
(«ничего не работает, пока не допишешь 7 классов»), сохраняя расширяемость.

---

## 4. Доменная модель (абстрактные базы)

### 4.1. Сводка

| Тип | Базовый | Роль |
|---|---|---|
| **`ActivityBase`** | `AggregateRoot<Guid>` + `IStateMachineEntity<ActivityStatus>` + `ICreateAtEntity` + `IUpdatedAtEntity` (+опц. `IRemovedAtEntity`) | агрегат активности |
| **`ActivityReminderBase`** | `Entity<Guid>` (child) | напоминание (`OffsetBeforeDue`, `Channel`, `Sent`) |
| generic `Specification<TActivity>` | Domain | `ActivitiesByEntity`, `OpenActivitiesByAssignee`, `OverdueActivities`, `DueRemindersDue` |

> Базовые классы ядра (сверено по репозиторию): `Entity<TId>`/`AggregateRoot<TId>` (есть короткая
> форма `AggregateRoot` для `Guid`); аудит-интерфейсы `ICreateAtEntity`/`IUpdatedAtEntity` объявляют
> **`DateTimeOffset?`** (не `DateTime`) — использовать его. `AddDomainEvent`/`DomainEvents`/
> `ClearDomainEvents` — на `AggregateRoot`.

### 4.2. Shared — enums и конвенции

```csharp
namespace Cheetah.Modules.Activities.Shared;

public enum ActivityType     { Task = 0, Call = 1, Meeting = 2, Email = 3 }
public enum ActivityStatus   { Open = 0, InProgress = 1, Done = 2, Canceled = 3 }
public enum ActivityPriority { Low = 0, Normal = 1, High = 2, Urgent = 3 }

// Конвенция полиморфной привязки — общая с Tags/Notes/CustomFields (сквозное решение №1 плана).
public static class EntityRefKeys
{
    public const string Deal     = "crm.deal";
    public const string Customer = "crm.customer";
    public const string Contact  = "crm.contact";
    public const string Lead     = "crm.lead";
}
```

### 4.3. `ActivityBase` — точки расширения

Принципы абстрактности (из Customer README, применённые к Activities):

- **Нельзя `new` абстрактную сущность** в generic-handler → создание через `protected InitializeCore(...)`
  (вызывается фабрикой наследника) + `IActivityFactory`.
- **Мутаторы — `virtual`**, чтобы наследник мог дополнить поведение (например, при `Complete`
  заполнить свой `CallOutcome`).
- **Доп. поля наследника** объявляются в `sealed Activity : ActivityBase` с `private set`; запись —
  через методы наследника или расширенную фабрику.

```csharp
public abstract class ActivityBase : AggregateRoot<Guid>,
    IStateMachineEntity<ActivityStatus>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<ActivityReminderBase> _reminders = new();

    public ActivityType Type { get; protected set; }
    public string Title { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public ActivityStatus Status { get; protected set; }
    public ActivityPriority Priority { get; protected set; }
    public Guid AssigneeId { get; protected set; }
    public Guid OwnerId { get; protected set; }

    // полиморфная привязка к произвольной сущности
    public string EntityType { get; protected set; } = null!;
    public Guid EntityId { get; protected set; }

    public DateTimeOffset? DueAt { get; protected set; }
    public DateTimeOffset? CompletedAt { get; protected set; }
    public string? Result { get; protected set; }
    public Guid? CalendarEventId { get; protected set; }

    // «Быстрый» карман расширения без миграций (MVP, опционально; полноценно — Custom Fields).
    public string? Attributes { get; protected set; } // jsonb

    public IReadOnlyList<ActivityReminderBase> Reminders => _reminders;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ActivityStatus State => Status;   // IStateMachineEntity

    protected ActivityBase() { } // EF + наследник

    /// <summary>Инициализация ядра — вызывается фабрикой/Create наследника (замена new).</summary>
    protected void InitializeCore(Guid id, ActivityType type, string title, Guid assigneeId,
        Guid ownerId, string entityType, Guid entityId, DateTimeOffset? dueAt,
        ActivityPriority priority, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        Id = id; Type = type; Title = title.Trim(); Status = ActivityStatus.Open;
        Priority = priority; AssigneeId = assigneeId; OwnerId = ownerId;
        EntityType = entityType; EntityId = entityId; DueAt = dueAt; Description = description;
        AddDomainEvent(new ActivityCreatedIntegrationEvent(Id, entityType, entityId, assigneeId, dueAt));
    }

    public void AddReminder(ActivityReminderBase reminder)
    {
        if (DueAt is null) throw new InvalidOperationException("Reminder requires DueAt.");
        _reminders.Add(reminder);
    }

    public virtual void Start() { if (Status == ActivityStatus.Open) Status = ActivityStatus.InProgress; }

    public virtual void Complete(Guid completedBy, string? result = null)
    {
        if (Status is ActivityStatus.Done or ActivityStatus.Canceled) return;
        Status = ActivityStatus.Done; CompletedAt = DateTimeOffset.UtcNow; Result = result;
        AddDomainEvent(new ActivityCompletedIntegrationEvent(Id, completedBy));
    }

    public virtual void Cancel() => Status = ActivityStatus.Canceled;
    public virtual void Reassign(Guid newAssigneeId) => AssigneeId = newAssigneeId;
    public void LinkCalendarEvent(Guid calendarEventId) => CalendarEventId = calendarEventId;

    /// <summary>Вызывается фоновой задачей скана просрочек.</summary>
    public bool TryMarkOverdue(DateTimeOffset now)
    {
        if (Status == ActivityStatus.Open && DueAt is { } due && due < now)
        {
            AddDomainEvent(new ActivityOverdueIntegrationEvent(Id, AssigneeId));
            return true;
        }
        return false;
    }
}
```

### 4.4. StateMachine — конфигурация (в Application)

```csharp
services.AddStateMachine<ActivityStatus>(sm => sm
    .From(ActivityStatus.Open).To(ActivityStatus.InProgress, ActivityStatus.Done, ActivityStatus.Canceled)
    .From(ActivityStatus.InProgress).To(ActivityStatus.Done, ActivityStatus.Canceled)
    .From(ActivityStatus.Done).To(ActivityStatus.Open)        // переоткрытие
    .From(ActivityStatus.Canceled).To(ActivityStatus.Open));
```

> API StateMachine свериться с `src/Cheetah.Core.StateMachine` (`AddStateMachine<TState>`,
> `IStateMachineValidator<TState>.Validate`, `IStateMachineEntity<TState>`,
> `InvalidStateTransitionException`). Валидатор вызывается в хендлерах перед мутатором.

### 4.5. Domain — спецификации (generic, фильтрация только через них)

```csharp
public sealed class ActivitiesByEntitySpecification<TActivity>(string entityType, Guid entityId)
    : Specification<TActivity> where TActivity : ActivityBase
{
    public override Expression<Func<TActivity, bool>> ToExpression()
        => a => a.EntityType == entityType && a.EntityId == entityId;
}

public sealed class OpenActivitiesByAssigneeSpecification<TActivity>(Guid assigneeId)
    : Specification<TActivity> where TActivity : ActivityBase
{
    public override Expression<Func<TActivity, bool>> ToExpression()
        => a => a.AssigneeId == assigneeId &&
                (a.Status == ActivityStatus.Open || a.Status == ActivityStatus.InProgress);
}

public sealed class OverdueActivitiesSpecification<TActivity>(DateTimeOffset now)
    : Specification<TActivity> where TActivity : ActivityBase
{
    public override Expression<Func<TActivity, bool>> ToExpression()
        => a => a.Status == ActivityStatus.Open && a.DueAt != null && a.DueAt < now;
}
```

> Комбинаторы `And`/`Or`/`Not` — проверить наличие в `Cheetah.Core.Specification`; если нет — добавить
> (нужны для `ListActivitiesQuery` с комбинацией фильтров). Тот же открытый вопрос был у Deals.

---

## 5. Contracts — расширяемые ViewModel

Абстрактные `record`-базы (наследник добавляет свои поля через `init`-свойства). На границе API — без
VO (если появятся), типы — примитивы. Все DTO реализуют `ICrmResponse` (как в Deals).

```csharp
public abstract record ActivityDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public ActivityType Type { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public ActivityStatus Status { get; init; }
    public ActivityPriority Priority { get; init; }
    public Guid AssigneeId { get; init; }
    public Guid OwnerId { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public DateTimeOffset? DueAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? Result { get; init; }
    public Guid? CalendarEventId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public abstract record CreateActivityRequestBase
{
    public ActivityType Type { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public ActivityPriority Priority { get; init; } = ActivityPriority.Normal;
    public Guid AssigneeId { get; init; }
    public Guid OwnerId { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public DateTimeOffset? DueAt { get; init; }
}

public abstract record UpdateActivityRequestBase
{
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public ActivityPriority Priority { get; init; }
    public DateTimeOffset? DueAt { get; init; }
}

public abstract record CompleteActivityRequestBase { public string? Result { get; init; } }
```

> Наследник: `public sealed record ActivityDto : ActivityDtoBase { public string? CallOutcome { get; init; } }`
> и т.д. — ровно как `CustomerDto : CustomerDtoBase` в Customer.

---

## 6. Application — generic CQRS + фабрика/проектор

Generic-handler'ы закрываются конкретными типами наследника (как в Customer). Создание сущности — через
`IActivityFactory` (нельзя `new` абстракцию); проекция в DTO — через `IActivityProjector` (вместо
Mapster, чтобы доп. поля и возможные VO не требовали скрытой регистрации).

```csharp
public interface IActivityFactory<TActivity, TCreateRequest>
    where TActivity : ActivityBase
{
    TActivity Create(TCreateRequest request);
}

public interface IActivityProjector<TActivity, TDto>
    where TActivity : ActivityBase where TDto : ActivityDtoBase
{
    TDto ToDto(TActivity activity);
}
```

**Команды / запросы (generic по `TActivity`/`TDto`/`TRequest`):**

```csharp
CreateActivityCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>;
CompleteActivityCommand(Guid ActivityId, Guid CompletedBy, string? Result) : ICommand;
CancelActivityCommand(Guid ActivityId) : ICommand;
ReassignActivityCommand(Guid ActivityId, Guid NewAssigneeId) : ICommand;
ChangeActivityStatusCommand(Guid ActivityId, ActivityStatus To) : ICommand;  // через StateMachine-валидатор

GetActivityByIdQuery<TDto>(Guid ActivityId) : IQuery<TDto?>;
ListActivitiesQuery<TDto>(Guid? AssigneeId, ActivityStatus? Status, string? EntityType, Guid? EntityId,
    DateTimeOffset? DueBefore, int Page, int Size) : IQuery<IReadOnlyList<TDto>>;
BatchGetActivitiesQuery<TDto>(string EntityType, IReadOnlyList<Guid> EntityIds)
    : IQuery<IReadOnlyDictionary<Guid, IReadOnlyList<TDto>>>;  // анти-N+1 для списков сущностей
```

**Канон хендлера** (из `CLAUDE.md` + транзакционный Outbox, как в Deals): получить агрегат через
репозиторий → доменный мутатор → `SaveChangesAsync` (Outbox кладёт события в ту же транзакцию) →
`ClearDomainEvents`. Фильтрация — только спецификациями. `ValueTask<T>` + `CancellationToken` всюду.

**Регистрация (extension-метод, открытые generic нельзя через `[Export]`):**

```csharp
services.AddActivitiesApplication<Activity, CreateActivityRequest, UpdateActivityRequest,
    ActivityDto, ActivityFactory, ActivityProjector>();
```

---

## 7. Infrastructure — EF Core + фоновые задачи

### 7.1. Абстрактные базы (расширяемая схема)

```csharp
public abstract class ActivityConfigurationBase<TActivity> : IEntityTypeConfiguration<TActivity>
    where TActivity : ActivityBase
{
    public void Configure(EntityTypeBuilder<TActivity> b)
    {
        b.ToTable("Activities", "activities");
        b.Property(a => a.Title).HasMaxLength(300).IsRequired();
        b.Property(a => a.EntityType).HasMaxLength(64).IsRequired();
        b.Property(a => a.Type).HasConversion<int>();
        b.Property(a => a.Status).HasConversion<int>();
        b.Property(a => a.Priority).HasConversion<int>();
        b.Property(a => a.Attributes).HasColumnType("jsonb");
        b.HasMany(a => a.Reminders).WithOne()
            .HasForeignKey(r => r.ActivityId).OnDelete(DeleteBehavior.Cascade);

        // Горячие пути: «мои открытые», «активности сущности», скан просрочек.
        b.HasIndex(a => new { a.AssigneeId, a.Status });
        b.HasIndex(a => new { a.EntityType, a.EntityId });
        b.HasIndex(a => new { a.Status, a.DueAt });

        b.Ignore(a => a.DomainEvents);  // CRITICAL
        ConfigureCustom(b);             // hook наследника: индексы/колонки доп. полей
    }

    protected virtual void ConfigureCustom(EntityTypeBuilder<TActivity> b) { }
}

public abstract class ActivitiesDbContextBase<TContext, TActivity> : DbContext
    where TContext : DbContext where TActivity : ActivityBase
{
    public DbSet<TActivity> Activities => Set<TActivity>();
    protected ActivitiesDbContextBase(DbContextOptions<TContext> o) : base(o) { }

    protected override void OnModelCreating(ModelBuilder mb)
        => mb.ApplyConfiguration(CreateActivityConfiguration());

    protected abstract IEntityTypeConfiguration<TActivity> CreateActivityConfiguration();
}
```

> Из Customer: **абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`,
> `IDesignTimeDbContextFactory` и миграции принадлежат наследнику (или сборке `.Default`).

**Транзакционный Outbox.** По образцу Deals/Calendar `DbContext` реализует
`IOutboxDbContext`/`IDeadLetterDbContext`; `IEventBus.PublishAsync` кладёт сообщения в `OutboxMessages`
той же транзакции, что и агрегат (атомарность). Зависеть на `Cheetah.Core.Outbox.*`.

**Регистрация:** `services.AddActivitiesInfrastructure<ActivitiesDbContext, Activity>();` —
`AddDbContext` (`UseNpgsql`), `IRepository<Activity, Guid>`, фоновые задачи.

### 7.2. Фоновые задачи (один исполнитель в кластере)

```csharp
[Export(LifetimeType.Scoped, typeof(IBackgroundTask))]
public sealed class ScanOverdueActivitiesTask<TActivity> : IBackgroundTask
    where TActivity : ActivityBase
{
    // "*/5 * * * *" — каждые 5 минут (формат свериться с Cheetah.BackgroundTasks)
    // AcquireAsync("activities:scan-overdue") — DistributedLock; если null — лок у другого инстанса.
    // GetAllAsync(new OverdueActivitiesSpecification<TActivity>(now)) → TryMarkOverdue → SaveChanges
    //   → события уже в Outbox.
}
```

> `DispatchActivityRemindersTask` — выбирает активности с несработавшими `ActivityReminder`, у которых
> `DueAt - OffsetBeforeDue <= now`, публикует `ActivityDueIntegrationEvent` (→ Notification), помечает
> `MarkSent()`. Generic-задачи регистрируются вручную в `AddActivitiesInfrastructure<>` (открытый
> generic не берётся `[Export]`-генератором — то же ограничение, что в Customer).

---

## 8. Api (декларативные эндпоинты, расширяемые)

Эндпоинты — наследники базовых из `Cheetah.Backend.Endpoints` (`CreateCommandEndpoint`,
`CommandEndpoint`, `QueryOrNotFoundEndpoint`, `QueryCollectionEndpoint`), как в Deals; регистрация —
генератором `Cheetah.Generators.Endpoints`. Базовый `ActivityEndpointsBase<…>` с `virtual`-методами,
чтобы наследник переопределял маршруты/поведение.

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `/api/activities` | `CreateActivityCommand` |
| PUT/DELETE | `/api/activities/{id}` | update / delete (soft) |
| GET | `/api/activities/{id}` | `GetActivityByIdQuery` |
| GET | `/api/activities?assigneeId=&status=&entityType=&entityId=&dueBefore=&page=&size=` | `ListActivitiesQuery` |
| GET | `/api/activities/my?status=` | «мои задачи» (`AssigneeId` из контекста) |
| POST | `/api/activities/{id}/complete` `{ result? }` | `CompleteActivityCommand` |
| POST | `/api/activities/{id}/cancel` | `CancelActivityCommand` |
| POST | `/api/activities/{id}/reassign` `{ newAssigneeId }` | `ReassignActivityCommand` |
| POST | `/api/activities/batch-get` `{ entityType, entityIds[] }` | `BatchGetActivitiesQuery` (анти-N+1) |

`CheetahActivitiesApiModuleBase<…>` маппит эндпоинты в `OnApplicationInitialization`; модуль-классы
`partial` (для Source Generators). Авторизация — `Cheetah.Permissions`.

---

## 9. События (публикует Activities)

```csharp
namespace Cheetah.Modules.Activities.DomainEvents;

ActivityCreatedIntegrationEvent(Guid ActivityId, string EntityType, Guid EntityId,
    Guid AssigneeId, DateTimeOffset? DueAt) : EventBase;
ActivityCompletedIntegrationEvent(Guid ActivityId, Guid CompletedBy) : EventBase;
ActivityDueIntegrationEvent(Guid ActivityId, Guid AssigneeId) : EventBase;      // → Notification
ActivityOverdueIntegrationEvent(Guid ActivityId, Guid AssigneeId) : EventBase;
```

Потребители: Notification (`Due`/`Overdue`), Timeline, Search, аналитика.

**Целостность.** Подписка на общий `EntityDeletedIntegrationEvent(EntityType, EntityId)` — закрывать
(`Cancel`) или удалять висячие активности удалённой сущности (политика). Регистрация подписки в
`OnApplicationInitialization`; идемпотентность — через `Cheetah.Core.Inbox`.

---

## 10. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | инварианты `ActivityBase` (нельзя Reminder без `DueAt`; `Complete` идемпотентен; `TryMarkOverdue` только для Open+просрочка; переходы статусов); наследование (sealed-`Activity` с доп. полем корректно создаётся фабрикой) |
| `Application.Tests` | generic-хендлеры с моками `IRepository`/`IEventBus`/фабрики/проектора (Moq — в репозитории нет NSubstitute): публикация событий в Outbox, batch-get анти-N+1, StateMachine-валидатор переходов |
| `Client.Tests` | сериализация запросов/ответов HTTP-клиента, обработка ошибок (404/409) |
| (Default) | интеграционный smoke: создать → complete → списки/фильтры; миграция применяется |

---

## 11. План реализации (пошагово)

> Каждый шаг = отдельный коммит. После каждого слоя — `dotnet build` + `dotnet sln add` в
> `Cheetah.slnx`, папка `/Modules/Activities/`. Пакеты — через `Directory.Packages.props`
> (`PackageReference` без `Version`).

**Фаза 0 — каркас**
1. Создать проекты по §3 (8 шаблонных + опц. `.Default` + `Client` + 3 тестовых), ссылки строго по
   порядку зависимостей. Добавить в `Cheetah.slnx`.

**Фаза 1 — контракты**
2. `DomainEvents`: 4 интеграционных события (§9).
3. `Shared`: enums + `EntityRefKeys` (§4.2).
4. `Contracts`: абстрактные `ActivityDtoBase`, `…RequestBase` (§5).

**Фаза 2 — домен**
5. `Domain`: `ActivityBase`, `ActivityReminderBase` + `InitializeCore`/мутаторы (§4.3).
6. `Domain`: generic-спецификации (§4.5), при необходимости комбинаторы.
7. `Domain.Tests`: инварианты — **до** Infrastructure (домен без БД).

**Фаза 3 — инфраструктура**
8. `Infrastructure`: `ActivityConfigurationBase<>` (+`ConfigureCustom` hook), `ActivitiesDbContextBase<>`,
   реализация Outbox-интерфейсов.
9. `Infrastructure`: `AddActivitiesInfrastructure<>` (`AddDbContext`, репозиторий, фоновые задачи).
10. Фоновые задачи `ScanOverdueActivitiesTask<>` / `DispatchActivityRemindersTask<>` (§7.2).

**Фаза 4 — приложение**
11. `Application`: `IActivityFactory`/`IActivityProjector`, generic команды/запросы + хендлеры (§6).
12. `Application`: `AddStateMachine<ActivityStatus>` (§4.4) + `AddActivitiesApplication<>`.
13. `Application.Tests`: хендлеры (события в Outbox, batch-get, переходы).

**Фаза 5 — API + (Default) + клиент**
14. `Api`: `ActivityEndpointsBase<>` + `CheetahActivitiesApiModuleBase<>` (§8), декларативные эндпоинты.
15. (Опц.) `.Default`: sealed `Activity`, конкретные Contracts, фабрика/проектор, `ActivitiesDbContext`
    + `IDesignTimeDbContextFactory` + **миграция** `InitialActivities` (схема `activities`), готовые
    Api/Infrastructure/Application-регистрации.
16. `Client`: `IActivitiesClient` + реализация (Workflow/Booking создают активности), `Client.Tests`.

**Фаза 6 — интеграция**
17. Подписка на `EntityDeletedIntegrationEvent` (политика очистки) + идемпотентность через Inbox.
18. End-to-end: создать активность по `crm.deal` → напоминание → просрочка → проверить события/Outbox.

**Фаза 7 — финал**
19. README модуля (это **базовый/шаблонный** модуль → по чек-листу `CLAUDE.md` README обязателен:
    назначение, точки расширения, extension-методы, пример наследования — по образцу Customer README).
20. Обновить статус этого плана на «реализовано», добавить ссылку на код; обновить `MEMORY.md`.

---

## 12. Зависимости от инфраструктуры

| Инфраструктурный модуль | Использование в Activities |
|---|---|
| `Cheetah.Core.Domain` | `AggregateRoot`/`Entity`, аудит-интерфейсы (`DateTimeOffset?`) |
| `Cheetah.Core.StateMachine` | переходы `ActivityStatus` |
| `Cheetah.Core.DataAccess` (`IRepository<T>`) | репозитории + спецификации |
| `Cheetah.Core.Specification` | вся фильтрация (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` | `ICommand`/`IQuery`/`IDispatcher`/`IEventBus` |
| `Cheetah.Core.Outbox.*` | транзакционная публикация событий |
| `Cheetah.Core.Inbox` | идемпотентность подписки на `EntityDeleted` |
| `Cheetah.BackgroundTasks` + `Cheetah.DistributedLock(.Postgres)` | скан просрочек / диспетчер напоминаний |
| `CrmEntityFrameworkModule` + `…PostgreSqlModule` | EF Core + Npgsql |
| `Cheetah.Backend.Endpoints` + `Cheetah.Generators.Endpoints` | декларативные эндпоинты |
| `Cheetah.Permissions` | авторизация эндпоинтов |
| `Cheetah.Modules.Calendar` (Client) | связь `Meeting` ↔ `CalendarEventId` (опционально) |

---

## 13. Отличия от исходного эскиза плана (`docs/plans.md` §2/§A)

1. **Абстрактный шаблон вместо конкретного модуля** — по требованию расширяемости (§0). Эскиз §A давал
   `sealed Activity`; здесь — `ActivityBase` + generic-хелперы по образцу Customer.
2. **Аудит-поля — `DateTimeOffset?`** (сверено по `ICreateAtEntity`/`IUpdatedAtEntity` ядра), а не
   `DateTime` из эскиза.
3. **События — через транзакционный Outbox** (как реально сделано в Deals), а не «после SaveChanges».
4. **Эндпоинты — декларативные** наследники `Cheetah.Backend.Endpoints` + генератор, не raw Minimal API.
5. **Опциональная сборка `.Default`** — чтобы модуль работал «из коробки», оставаясь расширяемым
   (Customer такой не имеет — сознательное улучшение, §3.1).
6. **`Attributes (jsonb)`** как «быстрый» карман расширения на MVP до появления Custom Fields.

**Отложено (follow-up):** полноценная динамическая расширяемость через Custom Fields; справочник
типов/статусов вместо enum; gRPC для горячих списков; round-trip с Calendar (внешние участники).

---

## 14. Что реализовано (сверка с кодом)

Реализован **абстрактный шаблон** по образцу `Cheetah.Modules.Customer` (требование §0 —
расширяемость сущностей и ViewModel). 7 шаблонных проектов + 2 тестовых, всё собирается, 29 тестов
зелёные, полная солюшн `Cheetah.slnx` собирается без ошибок.

**Точки расширяемости (готовы):**

- сущность — `abstract ActivityBase` + `protected InitializeCore(...)` + `virtual`-мутаторы; наследник
  объявляет `sealed class Activity : ActivityBase` со своими полями;
- ViewModel — `abstract record ActivityDtoBase`; наследник — `sealed record ActivityDto : ActivityDtoBase`;
- запросы — `abstract record CreateActivityRequestBase`/`UpdateActivityRequestBase`;
- создание/проекция — `IActivityFactory`/`IActivityProjector` (наследник реализует);
- схема EF — `ActivityConfigurationBase<TActivity>` + `ConfigureCustom`-hook;
- регистрация — `AddActivitiesInfrastructure<TContext,TActivity>()` и
  `AddActivitiesApplication<TActivity,TCreateRequest,TUpdateRequest,TDto,TFactory,TProjector>()`;
- эндпоинты — `ActivityEndpointsBase<…>` (`virtual`-методы) + `CheetahActivitiesApiModuleBase<…>`;
- «быстрый» карман — колонка `Attributes (jsonb)` на `ActivityBase`.

**Сознательные отличия от §0–§13 (как у работающего шаблона Customer):**

1. **Без транзакционного Outbox.** Хендлеры публикуют события через `IEventBus` после
   `SaveChangesAsync` (канон `CLAUDE.md` + паттерн Customer), а не через Outbox, как в Deals.
   База `ActivitiesDbContextBase` — чистый `CrmDbContext<T>` без `IOutboxDbContext`. Апгрейд до
   Outbox — follow-up.
2. **`ActivityReminder` — конкретный тип** (не расширяемый), т.к. напоминания редко требуют расширения;
   сама активность остаётся расширяемой.
3. **Эндпоинты — Customer-стиль** (`EndpointsBase` + `IDispatcher` + `virtual`-методы), а не
   декларативный генератор Deals: генератор требует закрытых пар Request↔Command, что несовместимо с
   generic-CQRS шаблона.
4. **Lifecycle:** реализованы Create/Update/Start/Complete/Cancel/Reassign + Get/List. StateMachine
   (`ActivityStatus`) валидирует Start/Complete/Cancel.
5. **Не вошло в этот инкремент (follow-up):** сборка `.Default` (готовая `sealed`-реализация +
   миграция) и `Client` — оба требуют закрытых типов и создаются наследником/в следующем шаге; фоновые
   задачи `ScanOverdueActivitiesTask`/`DispatchActivityRemindersTask` (нужна сверка контрактов
   `Cheetah.BackgroundTasks`/`DistributedLock`); подписка на `EntityDeletedIntegrationEvent`.
   Доменные крючки уже готовы: `TryMarkOverdue`, `ActivityReminder.MarkSent`, спецификации
   `OverdueActivitiesSpecification`/`ActivitiesByEntitySpecification`.
