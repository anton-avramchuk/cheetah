# Cheetah.Modules.Activities.* — абстрактный шаблон-модуль «Задачи и активности»

> **Тип:** base/template (каркас), а не готовый микросервис.
> Модуль поставляет **только абстрактные базовые типы и generic-хелперы**. Конкретное приложение
> наследует типы, генерирует миграции у себя и получает рабочий CRUD + lifecycle «из коробки»,
> дописав ~6–7 классов. Сделан по образцу [`Cheetah.Modules.Customer`](../Customer/README.md).
>
> Полный план и решения — [`docs/modules/activities.md`](../../../docs/modules/activities.md).

## Назначение

Активности — задачи (Task), звонки (Call), встречи (Meeting), письма (Email) — привязанные к
произвольной сущности CRM через полиморфную ссылку `(EntityType, EntityId)`. У активности есть тип,
статус (конечный автомат), приоритет, исполнитель, владелец, срок, результат и напоминания.

**Главное требование — расширяемость:** и сущность, и ViewModel наследуемы; приложение добавляет свои
поля без форка модуля.

## Состав сборок и граф зависимостей

```
Activities.DomainEvents   → Core.Events                          (интеграционные события, Guid id)
Activities.Shared         → Core                                 (enum Type/Status/Priority, EntityRefKeys, константы)
Activities.Contracts      → Core + Contracts + Shared            (ABSTRACT базовые DTO/Request)
Activities.Domain         → DomainEvents + Specification + SM     (abstract ActivityBase, ActivityReminder, generic-спеки)
Activities.Infrastructure → Domain + EF + EF.PostgreSql          (abstract DbContextBase/ConfigBase, AddActivitiesInfrastructure<>)
Activities.Application     → Domain + Contracts + CQRS + Events   (generic handlers, IActivityFactory/Projector, AddActivitiesApplication<>)
Activities.Api            → Application + Contracts + AspNetCore  (abstract ActivityEndpointsBase<>, ApiModuleBase)
Tests: Domain.Tests (18), Application.Tests (11)
```

`Default` и `Client` отсутствуют — их создаёт наследник (нужны закрытые типы).

## Ключевые абстракции

| Тип | Сборка | Роль |
|---|---|---|
| `ActivityBase : AggregateRoot<Guid>` | Domain | агрегат; `InitializeCore`, `Update`, `Start`, `Complete`, `Cancel`, `Reassign`, `AddReminder`, `TryMarkOverdue`, `LinkCalendarEvent`, `SetAttributes` |
| `ActivityReminder : Entity<Guid>` | Domain | напоминание (дитя активности), `MarkSent()` |
| `Activities*Specification<TActivity>` | Domain | generic-спеки: ByEntity, OpenByAssignee, Overdue, Filter |
| `ActivityDtoBase` + `Create/UpdateActivityRequestBase` | Contracts | абстрактные record (точка расширения ViewModel/запросов) |
| `ActivitiesDbContextBase<TContext, TActivity>` | Infrastructure | `DbSet` активностей + напоминаний |
| `ActivityConfigurationBase<TActivity>` | Infrastructure | таблица/схема, индексы, связь с напоминаниями, `ConfigureCustom` hook |
| `IActivityFactory`/`IActivityProjector` | Application | `Create(...)`/`ToDto(...)` — замена `new`/Mapster |
| generic CQRS | Application | Create/Update/Start/Complete/Cancel/Reassign + GetById/List |
| `ActivityEndpointsBase<…>` / `CheetahActivitiesApiModuleBase<…>` | Api | CRUD/lifecycle-маршруты, `virtual` |

### Архитектурные приёмы абстрактности

- **Нельзя `new` абстрактную сущность** в generic-handler → фабрика `IActivityFactory` + `InitializeCore`.
- **`[Export]` source-gen работает только по закрытым типам** → открытые generic-handler'ы
  регистрируются вручную в `AddActivitiesInfrastructure<>`/`AddActivitiesApplication<>`.
- **Абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`, design-time factory и
  миграции принадлежат наследнику; базовый `OnModelCreating` применяет конфигурацию через hook.
- **События** публикуются через `IEventBus` после `SaveChangesAsync` (как в Customer; Outbox — follow-up).

## Extension-методы (точки регистрации у наследника)

```csharp
// Infrastructure: DbContext (активности + напоминания), мигратор, PostgreSQL,
// IRepository<Activity, Guid> и IRepository<ActivityReminder, Guid>
services.AddActivitiesInfrastructure<AppActivitiesDbContext, Activity>();

// Application: фабрика, проектор, закрытые generic CQRS-handler'ы
services.AddActivitiesApplication<Activity, CreateActivityRequest, UpdateActivityRequest,
    ActivityDto, ActivityFactory, ActivityProjector>();
```

Конечный автомат `ActivityStatus` регистрируется самим `CheetahActivitiesApplicationModule`
(не зависит от конкретного типа).

## Пример наследования («быстрая реализация»)

```csharp
// 1. Сущность с доп. полем
public sealed class Activity : ActivityBase
{
    public string? CallOutcome { get; private set; }
    private Activity() { }
    public static Activity Create(CreateActivityRequest r)
    {
        var a = new Activity();
        a.InitializeCore(Guid.NewGuid(), r.Type, r.Title, r.AssigneeId, r.OwnerId,
            r.EntityType, r.EntityId, r.DueAt, r.Priority, r.Description);
        return a;
    }
}

// 2. Contracts с доп. полем
public sealed record ActivityDto : ActivityDtoBase { public string? CallOutcome { get; init; } }
public sealed record CreateActivityRequest : CreateActivityRequestBase { public string? CallOutcome { get; init; } }
public sealed record UpdateActivityRequest : UpdateActivityRequestBase;

// 3. Фабрика + проектор
public sealed class ActivityFactory : IActivityFactory<Activity, CreateActivityRequest>
{ public Activity Create(CreateActivityRequest r) => Activity.Create(r); }

public sealed class ActivityProjector : IActivityProjector<Activity, ActivityDto>
{
    public ActivityDto ToDto(Activity a) => new()
    {
        Id = a.Id, Type = a.Type, Title = a.Title, Status = a.Status, Priority = a.Priority,
        AssigneeId = a.AssigneeId, OwnerId = a.OwnerId, EntityType = a.EntityType, EntityId = a.EntityId,
        DueAt = a.DueAt, CompletedAt = a.CompletedAt, Result = a.Result, CreatedAt = a.CreatedAt
        // + CallOutcome = a.CallOutcome
    };
}

// 4. EF-конфигурация (доп. поле через ConfigureCustom) + DbContext (миграции — у наследника)
public sealed class ActivityConfiguration : ActivityConfigurationBase<Activity>
{ protected override void ConfigureCustom(EntityTypeBuilder<Activity> b) => b.Property(x => x.CallOutcome).HasMaxLength(256); }

public sealed class AppActivitiesDbContext(DbContextOptions<AppActivitiesDbContext> o)
    : ActivitiesDbContextBase<AppActivitiesDbContext, Activity>(o)
{
    protected override IEntityTypeConfiguration<Activity> CreateActivityConfiguration() => new ActivityConfiguration();
}

// 5. Api-модуль наследника
public sealed class AppActivityEndpoints
    : ActivityEndpointsBase<CreateActivityRequest, UpdateActivityRequest, ActivityDto> { }
public sealed class AppActivitiesApiModule
    : CheetahActivitiesApiModuleBase<AppActivityEndpoints, CreateActivityRequest, UpdateActivityRequest, ActivityDto> { }

// 6. dotnet ef migrations add Initial  (+ IDesignTimeDbContextFactory) — в проекте наследника
```

## Эндпоинты (по умолчанию, `api/activities`)

| Метод | Маршрут |
|---|---|
| POST | `/api/activities` |
| GET | `/api/activities?assigneeId=&status=&entityType=&entityId=&dueBefore=` |
| GET | `/api/activities/{id}` |
| PUT | `/api/activities/{id}` |
| POST | `/api/activities/{id}/start` · `/complete` · `/cancel` · `/reassign` |

## События

`ActivityCreated/Completed/Canceled/Reassigned/Due/Overdue` (`*IntegrationEvent`, `Cheetah.Core.Events`).
Потребители: Notification (`Due`/`Overdue`), Timeline, Search.

## Ограничения / follow-up

- Миграции и `IDesignTimeDbContextFactory` — только у наследника (в модуле их нет).
- `Client` и готовая сборка `.Default` — создаёт наследник / следующий инкремент.
- Фоновые задачи скана просрочек и диспетчера напоминаний — follow-up (доменные крючки готовы:
  `TryMarkOverdue`, `ActivityReminder.MarkSent`, `OverdueActivitiesSpecification`).
- Подписка на `EntityDeletedIntegrationEvent` (очистка висячих активностей) — follow-up.
- Транзакционный Outbox — follow-up (сейчас публикация после `SaveChangesAsync`).
- Коды ответов: not-found сейчас маппится в 400 (`ActivityValidationException`) — уточнить до 404.
