# Cheetah.Modules.Leads.* — модуль «Лиды» (расширяемый шаблон)

> Статус: **реализовано (MVP абстрактного шаблона).** Код — в `src/Modules/Leads/`. 9 проектов
> (7 шаблонных + 2 тестовых) собираются; тесты зелёные: Domain (16), Application (11) = 27. Краткий
> гайд по расширению — `src/Modules/Leads/README.md`. Сознательные отличия от плана — те же, что у
> Activities: события через `IEventBus` после `SaveChangesAsync` (без Outbox); эндпоинты —
> `EndpointsBase`+`IDispatcher`. Конвертация — через порт `ILeadConversionOrchestrator` (реализацию,
> вкл. Saga/Client-вызовы, подключает наследник). Не вошло (follow-up): `.Default` + миграция, `Client`,
> публичный приём, политика слияния дублей, Outbox, коды 404/409.
>
> **Уточнение (отличие от §4.1):** `LeadStatus` и `LeadSource` сделаны **сущностями-справочниками**
> (lookup-таблицы, не enum и не абстрактные) со **seed** (`HasData`); лид ссылается по FK
> `StatusId`/`SourceId`. enum-автомат убран — переходы валидируются доменно по `LeadWellKnownIds`
> (как стадии в Deals). Добавлены lookup-эндпоинты `GET /api/leads/statuses|sources`.
> Документ — пошаговый план сборки модуля по канону `CLAUDE.md`
> (Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client)),
> своя БД PostgreSQL, общение через REST/gRPC + события через шину.
>
> Источник: разделы [§3](../plans.md) и [§L](../plans.md) общего плана. Это **п.3 рекомендуемого
> порядка реализации** (после [Deals](deals.md) и [Activities](activities.md)).
>
> **Tier 1.** Лиды — входная воронка маркетинга/обращений до квалификации в `Customer` (+ опц. `Deal`).
> Модуль обкатывает **конвертацию через границы модулей** (Lead → Customer → Deal).

---

## 0. Главное требование — расширяемость

> **Модуль обязан быть расширяемым: сущности и ViewModel должны допускать расширение полями
> приложения-наследника без форка модуля.**

Как и [Activities](activities.md) и `Customer`, Leads строится как **абстрактный шаблон-модуль**:
модуль поставляет **только абстрактные базовые типы и generic-хелперы**, наследник дописывает свои
`sealed`-типы со своими полями. Канон расширяемости — тот же, что уже реализован в Activities:

- сущность — `abstract LeadBase` + `protected InitializeCore(...)` + `virtual`-мутаторы; наследник
  объявляет `sealed class Lead : LeadBase` со своими полями (например, `Utm`, `Industry`);
- ViewModel — `abstract record LeadDtoBase`; наследник — `sealed record LeadDto : LeadDtoBase`;
- запросы — `abstract record Create/Update/ConvertLeadRequestBase`;
- создание/проекция — `ILeadFactory`/`ILeadProjector` (наследник реализует);
- схема EF — `LeadConfigurationBase<TLead>` + `ConfigureCustom`-hook;
- регистрация — `AddLeadsInfrastructure<>` / `AddLeadsApplication<>`;
- эндпоинты — `LeadEndpointsBase<…>` (`virtual`-методы) + `CheetahLeadsApiModuleBase<…>`;
- «быстрый» карман — колонка `Attributes (jsonb)` на `LeadBase`.

**Дополнительные точки расширения, специфичные для Leads:**

1. **Скоринг — `protected virtual int CalculateScore()`** на `LeadBase`. База даёт простое правило
   (вес по заполненности/источнику); наследник переопределяет (или позже — через
   [Custom Fields](../plans.md) + `Expressions.JsonLogic`).
2. **Конвертация — порт `ILeadConversionOrchestrator`** (см. §6.3). Это главная точка расширения:
   шаблон не знает, как именно создаются `Customer`/`Deal`, — наследник подключает свою реализацию
   (прямые Client-вызовы или `Cheetah.Saga`).

---

## 1. Назначение и границы

**Что делает:** принимает «сырые» контакты (лиды) из форм/импорта/рекламы; ведёт их квалификацию и
**конвертацию** в `Customer` (+ опционально `Deal`). У лида свой жизненный цикл:
`New → Working → Qualified → Converted | Disqualified`.

**Чего НЕ делает:**

- не дублирует `Customer` — после конвертации источник истины по клиенту переходит в Customer; лид
  хранит `ConvertedCustomerId`/`ConvertedDealId`;
- не создаёт Customer/Deal сам — делегирует это порту `ILeadConversionOrchestrator` (реализует
  наследник), оставаясь развязанным с конкретными модулями;
- не управляет пользователями — `OwnerId` это логическая ссылка на Identity (без FK через границу).

**Связи (по `Id`, без FK через границу модуля):** `OwnerId?`, `ConvertedCustomerId?`, `ConvertedDealId?`.

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Форма модуля | **абстрактный шаблон** (как Activities/Customer) — расширяемые сущности/DTO |
| Идентификатор | `Guid` |
| Хранилище | PostgreSQL (EF Core); миграции — **у наследника** |
| Контакты | value objects `Email`/`Phone` (`Cheetah.Core.Domain.ValueObjects`); на границе API — строки |
| Статус лида | enum `LeadStatus` под `IStateMachineEntity<LeadStatus>` |
| Источник | enum `LeadSource` |
| Скоринг | `protected virtual int CalculateScore()` (переопределяемо) |
| Конвертация | порт `ILeadConversionOrchestrator` (реализует наследник); событие `LeadConverted` |
| Антидубль | спецификация по Email/Phone, мягкое предупреждение (`ActivityValidationException`-аналог) |
| «Быстрый» карман | колонка `Attributes (jsonb)` на `LeadBase` |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Кто создаёт Customer/Deal при конвертации.** Рекомендация: порт `ILeadConversionOrchestrator`,
   реализацию даёт наследник. Причина — `Customer` это шаблон-модуль **без** Client (его создаёт
   конкретное приложение), поэтому шаблон Leads не может ссылаться на конкретный Customer-клиент.
   Варианты реализации у наследника: (а) прямые Client-вызовы (`Customer.Client` + `Deals.Client`)
   с ручной компенсацией; (б) `Cheetah.Saga` (событийная оркестрация с компенсациями) — для
   устойчивости; (в) только публикация `LeadConvertedIntegrationEvent` → Workflow/downstream.
2. **Поставлять ли «готовую» реализацию `.Default`** (sealed `Lead` + Contracts + DbContext + миграция
   + дефолтный orchestrator) — как и в Activities, рекомендуется как follow-up.
3. **Публичный приём лидов** (анонимный endpoint за `Cheetah.RateLimit`/captcha) — в MVP или follow-up.
4. **`EntityDeletedIntegrationEvent`** — общий контракт (сквозное решение №2 плана) — нужен ли Leads.

---

## 2. Архитектурная роль

```
   ┌──────────────────────────────────────────────────────────┐
   │                     Leads Module (шаблон)                 │
   │  ┌────────────────────┐                                   │
   │  │  LeadBase          │  New → Working → Qualified         │
   │  │ (abstract,         │            ├──▶ Converted          │
   │  │  StateMachine)     │            └──▶ Disqualified        │
   │  │  Email?/Phone? VO  │                                    │
   │  └─────────┬──────────┘                                    │
   │            │ наследует                                     │
   │   sealed Lead (поля) + LeadDto (поля) + CalculateScore()   │
   │            │                                               │
   │   Convert  ▼  ILeadConversionOrchestrator (порт)           │
   └────────────┼──────────────────────────────────────────────┘
                │ реализация наследника
        ┌───────┴────────┐
        ▼                ▼
   Customer.Client   Deals.Client   (или Cheetah.Saga с компенсациями)
                │
                ▼ publish
   LeadCreated / Converted / Disqualified  ──▶ Activities / Notification / Analytics
```

---

## 3. Структура проектов

```
src/Modules/Leads/
├── Cheetah.Modules.Leads.DomainEvents/   # LeadCreated/Qualified/Disqualified/Converted
├── Cheetah.Modules.Leads.Shared/          # enums (LeadStatus, LeadSource), константы
├── Cheetah.Modules.Leads.Contracts/       # ABSTRACT DTO/Request базы + ConvertLeadResult
├── Cheetah.Modules.Leads.Domain/          # abstract LeadBase, generic-спеки, ILeadConversionOrchestrator
├── Cheetah.Modules.Leads.Infrastructure/  # abstract DbContextBase/ConfigBase (VO-конвертеры), AddLeadsInfrastructure<>
├── Cheetah.Modules.Leads.Application/      # generic CQRS, ILeadFactory/Projector, AddLeadsApplication<>
├── Cheetah.Modules.Leads.Api/             # abstract LeadEndpointsBase<> (+ публичный приём), ApiModuleBase
├── (опц.) Cheetah.Modules.Leads.Default/  # sealed Lead + Contracts + DbContext + миграция + дефолтный orchestrator
├── (опц.) Cheetah.Modules.Leads.Client/   # HTTP-клиент server-to-server
└── Tests/
    ├── Cheetah.Modules.Leads.Domain.Tests/
    └── Cheetah.Modules.Leads.Application.Tests/
```

**Порядок зависимостей** — строго как в Activities/Customer:
`DomainEvents → Shared → Contracts → Domain → {Infrastructure, Application} → Api`.

> `ILeadConversionOrchestrator` живёт в **Domain** (порт), реализация — у наследника (`.Default` или
> приложение). Так Application зависит только на Domain (канон `CLAUDE.md`), а шаблон не тянет
> Customer/Deals.

---

## 4. Domain — абстрактная база

### 4.1. Shared — enums

```csharp
namespace Cheetah.Modules.Leads.Shared;

public enum LeadStatus { New = 0, Working = 1, Qualified = 2, Converted = 3, Disqualified = 4 }
public enum LeadSource { Web = 0, Import = 1, Ads = 2, Referral = 3, Manual = 4, Api = 5 }
```

### 4.2. `LeadBase` — точки расширения

```csharp
public abstract class LeadBase : AggregateRoot<Guid>,
    IStateMachineEntity<LeadStatus>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    public string FullName { get; private set; } = null!;
    public string? Company { get; private set; }
    public Email? Email { get; private set; }        // VO из Cheetah.Core.Domain.ValueObjects
    public Phone? Phone { get; private set; }        // VO из Cheetah.Core.Domain.ValueObjects
    public LeadSource Source { get; private set; }
    public LeadStatus Status { get; private set; }
    public int Score { get; private set; }           // 0..100
    public Guid? OwnerId { get; private set; }
    public Guid? ConvertedCustomerId { get; private set; }
    public Guid? ConvertedDealId { get; private set; }
    public string? DisqualifyReason { get; private set; }
    public string? Attributes { get; private set; }  // jsonb — карман расширения без миграций

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    public LeadStatus State => Status;               // IStateMachineEntity

    protected LeadBase() { } // EF + наследник

    /// <summary>Вызывается фабрикой наследника (замена new). Контакты — строками, валидируются VO.</summary>
    protected void InitializeCore(Guid id, string fullName, LeadSource source,
        string? email, string? phone, string? company, Guid? ownerId)
    {
        Id = id;
        SetFullName(fullName);
        Source = source;
        Company = company;
        Email = ParseEmail(email);
        Phone = ParsePhone(phone);
        OwnerId = ownerId;
        Status = LeadStatus.New;
        Score = CalculateScore();
        AddDomainEvent(new LeadCreatedIntegrationEvent(Id, source.ToString()));
    }

    public virtual void StartWorking() { if (Status == LeadStatus.New) Status = LeadStatus.Working; }

    public virtual void Qualify()
    {
        if (Status is not (LeadStatus.New or LeadStatus.Working))
            throw new InvalidOperationException("Only a new/working lead can be qualified.");
        Status = LeadStatus.Qualified;
    }

    public virtual void Disqualify(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason required.");
        Status = LeadStatus.Disqualified;
        DisqualifyReason = reason.Trim();
        AddDomainEvent(new LeadDisqualifiedIntegrationEvent(Id, DisqualifyReason));
    }

    /// <summary>Фиксация результата конвертации (после успеха orchestrator'а, см. §6.3).</summary>
    public virtual void MarkConverted(Guid customerId, Guid? dealId)
    {
        if (Status == LeadStatus.Converted) return;
        Status = LeadStatus.Converted;
        ConvertedCustomerId = customerId;
        ConvertedDealId = dealId;
        AddDomainEvent(new LeadConvertedIntegrationEvent(Id, customerId, dealId));
    }

    public virtual void AssignOwner(Guid? ownerId) { OwnerId = ownerId; }
    public virtual void Update(string fullName, string? company, string? email, string? phone)
    {
        SetFullName(fullName); Company = company;
        Email = ParseEmail(email); Phone = ParsePhone(phone);
        Score = CalculateScore();
    }

    public void SetAttributes(string? attributes) => Attributes = attributes;

    /// <summary>Переопределяемое правило скоринга — точка расширения.</summary>
    protected virtual int CalculateScore()
    {
        var s = 0;
        if (Email is not null) s += 30;
        if (Phone is not null) s += 30;
        if (!string.IsNullOrWhiteSpace(Company)) s += 20;
        s += Source switch { LeadSource.Referral => 20, LeadSource.Web => 10, _ => 0 };
        return Math.Min(s, 100);
    }

    private void SetFullName(string v) { ArgumentException.ThrowIfNullOrWhiteSpace(v); FullName = v.Trim(); }
    private static Email? ParseEmail(string? v) => string.IsNullOrWhiteSpace(v) ? null : Email.Create(v);
    private static Phone? ParsePhone(string? v) => string.IsNullOrWhiteSpace(v) ? null : Phone.Create(v);
}
```

### 4.3. StateMachine — конфигурация (в Application)

```csharp
services.AddStateMachine<LeadStatus>(sm => sm
    .From(LeadStatus.New).To(LeadStatus.Working, LeadStatus.Qualified, LeadStatus.Disqualified)
    .From(LeadStatus.Working).To(LeadStatus.Qualified, LeadStatus.Disqualified)
    .From(LeadStatus.Qualified).To(LeadStatus.Converted, LeadStatus.Disqualified));
```

### 4.4. Спецификации (generic)

```csharp
LeadByEmailSpecification<TLead>(string email)        // антидубль (c.Email == Email.Create(email))
LeadByPhoneSpecification<TLead>(string phone)        // антидубль
ActiveLeadsByOwnerSpecification<TLead>(Guid ownerId) // не Converted и не Disqualified
LeadsFilterSpecification<TLead>(LeadStatus?, LeadSource?, Guid? ownerId)  // список с фильтрами
```

---

## 5. Contracts — расширяемые ViewModel

```csharp
public abstract record LeadDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = null!;
    public string? Company { get; init; }
    public string? Email { get; init; }   // VO → строка
    public string? Phone { get; init; }
    public LeadSource Source { get; init; }
    public LeadStatus Status { get; init; }
    public int Score { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? ConvertedCustomerId { get; init; }
    public Guid? ConvertedDealId { get; init; }
    public string? DisqualifyReason { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}

public abstract record CreateLeadRequestBase
{
    public string FullName { get; init; } = null!;
    public LeadSource Source { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public Guid? OwnerId { get; init; }
}

public abstract record UpdateLeadRequestBase
{
    public string FullName { get; init; } = null!;
    public string? Company { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}

public abstract record ConvertLeadRequestBase
{
    public bool CreateDeal { get; init; }
    public string? DealTitle { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public Guid? PipelineId { get; init; }
}

public sealed record ConvertLeadResult(Guid CustomerId, Guid? DealId) : ICrmResponse;
```

---

## 6. Application — generic CQRS + конвертация

### 6.1. Фабрика/проектор (как в Activities)

```csharp
public interface ILeadFactory<out TLead, in TCreateRequest>
    where TLead : LeadBase where TCreateRequest : CreateLeadRequestBase
{ TLead Create(TCreateRequest request); }

public interface ILeadProjector<in TLead, out TDto>
    where TLead : LeadBase where TDto : LeadDtoBase
{ TDto ToDto(TLead lead); }
```

### 6.2. Команды / запросы

```csharp
CreateLeadCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>;
UpdateLeadCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand;
QualifyLeadCommand(Guid Id) : ICommand;
DisqualifyLeadCommand(Guid Id, string Reason) : ICommand;
ConvertLeadCommand<TConvertRequest>(Guid Id, TConvertRequest Request) : ICommand<ConvertLeadResult>;

GetLeadByIdQuery<TDto>(Guid Id) : IQuery<TDto?>;
ListLeadsQuery<TDto>(LeadStatus? Status, LeadSource? Source, Guid? OwnerId) : IQuery<IReadOnlyList<TDto>>;
```

Канон хендлера — как в Activities: репозиторий → доменный мутатор → `SaveChangesAsync` → публикация
`DomainEvents` через `IEventBus` → `ClearDomainEvents`. Антидубль в `CreateLeadCommandHandler`:
проверка `LeadByEmailSpecification`/`LeadByPhoneSpecification` до сохранения (мягкое предупреждение по
политике). Фильтрация — только спецификациями.

### 6.3. Конвертация — порт `ILeadConversionOrchestrator` (главная точка расширения)

```csharp
// Domain (порт). Реализацию даёт наследник: прямые Client-вызовы или Cheetah.Saga.
public interface ILeadConversionOrchestrator
{
    ValueTask<ConversionOutcome> ConvertAsync(LeadConversionRequest request, CancellationToken ct);
}

public sealed record LeadConversionRequest(
    Guid LeadId, string FullName, string? Email, string? Phone, string? Company,
    bool CreateDeal, string? DealTitle, decimal? Amount, string? Currency, Guid? PipelineId);

public sealed record ConversionOutcome(Guid CustomerId, Guid? DealId);
```

`ConvertLeadCommandHandler<TLead, TConvertRequest>`:

```
1. загрузить lead (Qualified — инвариант; иначе ActivityValidationException-аналог)
2. outcome = await _orchestrator.ConvertAsync(request, ct)      // создаёт Customer (+Deal) у наследника
3. lead.MarkConverted(outcome.CustomerId, outcome.DealId)
4. SaveChangesAsync → publish LeadConvertedIntegrationEvent → Clear
5. return new ConvertLeadResult(outcome.CustomerId, outcome.DealId)
```

**Реализации orchestrator'а у наследника (выбор за приложением):**

- **Прямой** (`DirectLeadConversionOrchestrator`): `Customer.Client.CreateAsync` → (опц.)
  `Deals.Client.CreateDealAsync`; при сбое Deal — компенсация (удалить/архивировать Customer).
- **Saga** (`Cheetah.Saga`): событийная оркестрация `LeadConversion` с компенсациями (см. §6.4) —
  для устойчивости к падениям между шагами.
- **Event-only**: публикует событие, фактическое создание делает Workflow/downstream (eventually
  consistent).

> Шаблон по умолчанию может поставлять `EventOnlyLeadConversionOrchestrator` (no-op + публикация),
> чтобы модуль работал без зависимости от Customer/Deals; «боевой» orchestrator подключает наследник.

### 6.4. Saga-вариант (опционально, на `Cheetah.Saga`)

`Cheetah.Saga` — событийная: сага стартует по событию (`[SagaStartedBy]`) и реагирует на последующие
(`[SagaHandles]`), управляя `Complete()`/`Compensate(reason)`/`Fail(reason)`. Схема конвертации:

```
[SagaStartedBy(LeadConversionStartedEvent)]  → вызвать Customer.Client.Create; ждать CustomerCreated
[SagaHandles(CustomerCreatedEvent)]          → если CreateDeal: Deals.Client.Create; иначе Complete
[SagaHandles(DealCreatedEvent)]              → MarkLeadConverted; Complete()
[SagaHandles(*Failed)]                       → Compensate(reason): удалить созданный Customer/Deal
```

Корреляция — по `LeadId` (`GetCorrelation`). Хранилище саги — `Cheetah.Saga.EntityFrameworkCore`
(`ISagaDbContext`) или Mongo. Это **follow-up**: для MVP достаточно прямого orchestrator'а.

### 6.5. Регистрация (extension-методы)

```csharp
services.AddLeadsApplication<Lead, CreateLeadRequest, UpdateLeadRequest, ConvertLeadRequest,
    LeadDto, LeadFactory, LeadProjector>();
// orchestrator регистрирует наследник:
services.AddScoped<ILeadConversionOrchestrator, DirectLeadConversionOrchestrator>();
```

---

## 7. Infrastructure — EF Core (VO-конвертеры + ConfigureCustom)

```csharp
public abstract class LeadConfigurationBase<TLead> : IEntityTypeConfiguration<TLead>
    where TLead : LeadBase
{
    protected virtual string TableName => LeadsConstants.DefaultTableName;
    protected virtual string Schema => LeadsConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TLead> b)
    {
        b.ToTable(TableName, Schema);
        b.Ignore(e => e.DomainEvents);
        b.HasKey(x => x.Id);

        b.Property(x => x.FullName).HasMaxLength(LeadsConstants.MaxNameLength).IsRequired();
        b.Property(x => x.Company).HasMaxLength(LeadsConstants.MaxNameLength);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.Source).HasConversion<int>();
        b.Property(x => x.DisqualifyReason).HasMaxLength(LeadsConstants.MaxReasonLength);
        b.Property(x => x.Attributes).HasColumnType("jsonb");

        // VO → строковые колонки (как в Customer): для null конвертер не вызывается.
        b.Property(x => x.Email).HasConversion(e => e!.Value, v => Email.Create(v))
            .HasMaxLength(LeadsConstants.MaxEmailLength);
        b.Property(x => x.Phone).HasConversion(p => p!.Value, v => Phone.Create(v))
            .HasMaxLength(LeadsConstants.MaxPhoneLength);

        b.HasIndex(x => x.Email);
        b.HasIndex(x => new { x.OwnerId, x.Status });

        ConfigureCustom(b);
    }

    protected virtual void ConfigureCustom(EntityTypeBuilder<TLead> b) { }
}
```

`LeadsDbContextBase<TContext, TLead> : CrmDbContext<TContext>` (DbSet лидов, `OnModelCreating` →
`CreateLeadConfiguration()`). `AddLeadsInfrastructure<TContext, TLead>()` — `AddApplicationDbContext`,
`AddDatabaseMigrator`, `Configure UseNpgsql`, `IRepository<TLead, Guid>`. Миграции — у наследника.

> Сознательно (как в Activities/Customer): события — через `IEventBus` после `SaveChangesAsync`,
> без транзакционного Outbox. Апгрейд до Outbox — follow-up.

---

## 8. Api (декларативно-расширяемые эндпоинты)

`LeadEndpointsBase<TCreateRequest, TUpdateRequest, TConvertRequest, TDto>` — `virtual`-методы +
`IDispatcher` (стиль Customer/Activities). `CheetahLeadsApiModuleBase<…>` маппит в
`OnApplicationInitialization`.

| Метод | Маршрут |
|---|---|
| POST | `/api/leads` (+ опц. публичный `/api/public/leads` за `RateLimit`) |
| GET | `/api/leads?status=&source=&ownerId=` |
| GET | `/api/leads/{id}` |
| PUT | `/api/leads/{id}` |
| POST | `/api/leads/{id}/qualify` |
| POST | `/api/leads/{id}/disqualify` `{ reason }` |
| POST | `/api/leads/{id}/convert` `{ createDeal, dealTitle?, amount?, currency?, pipelineId? }` → `ConvertLeadResult` |

---

## 9. События (публикует Leads)

```csharp
namespace Cheetah.Modules.Leads.DomainEvents;

LeadCreatedIntegrationEvent(Guid LeadId, string Source) : EventBase;
LeadQualifiedIntegrationEvent(Guid LeadId) : EventBase;
LeadDisqualifiedIntegrationEvent(Guid LeadId, string Reason) : EventBase;
LeadConvertedIntegrationEvent(Guid LeadId, Guid CustomerId, Guid? DealId) : EventBase;
```

Потребители: Activities (автозадача по новому лиду), Notification, аналитика; Saga-вариант конвертации
потребляет `CustomerCreated`/`DealCreated`.

---

## 10. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | `TestLead : LeadBase` (+ доп. поле): жизненный цикл (Qualify только из New/Working; Disqualify требует reason; MarkConverted идемпотентен), скоринг (включая переопределение `CalculateScore`), VO Email/Phone (валидность/парсинг), антидубль-спеки |
| `Application.Tests` | generic-хендлеры с моками `IRepository`/`IEventBus`/`ILeadConversionOrchestrator` (Moq): Create (+ антидубль), Qualify/Disqualify, Convert (orchestrator вызван, `MarkConverted`, событие, результат), queries |

---

## 11. План реализации (пошагово)

> Каждый шаг = коммит. После слоя — `dotnet build` + `dotnet sln add` в `Cheetah.slnx`,
> папка `/Modules/Leads/`. Отдельная ветка, мерж в `dev` — как делали для Activities.

**Фаза 0** — каркас: 7 шаблонных + 2 тестовых проекта, ссылки по порядку зависимостей, в `Cheetah.slnx`.
**Фаза 1** — `DomainEvents` (4 события), `Shared` (enums + константы), `Contracts` (абстрактные DTO/Request + `ConvertLeadResult`).
**Фаза 2** — `Domain`: `LeadBase` (+ `CalculateScore` virtual), спецификации, порт `ILeadConversionOrchestrator`; `Domain.Tests` (включая переопределение скоринга).
**Фаза 3** — `Infrastructure`: `LeadConfigurationBase<>` (VO-конвертеры, `ConfigureCustom`), `LeadsDbContextBase<>`, `AddLeadsInfrastructure<>`.
**Фаза 4** — `Application`: `ILeadFactory`/`ILeadProjector`, generic команды/запросы + хендлеры, `ConvertLeadCommandHandler` через порт, `AddStateMachine<LeadStatus>`, `AddLeadsApplication<>`; `Application.Tests`.
**Фаза 5** — `Api`: `LeadEndpointsBase<>` + `CheetahLeadsApiModuleBase<>` (вкл. публичный приём).
**Фаза 6** — сборка солюшн, прогон тестов, README модуля (как у Activities), обновление этого плана и `MEMORY.md`.

**Follow-up:** сборка `.Default` (sealed `Lead` + миграция + `DirectLeadConversionOrchestrator`);
`Client`; Saga-вариант конвертации; публичный приём с captcha; антидубль-слияние; Outbox; not-found → 404.

---

## 12. Зависимости от инфраструктуры

| Модуль | Использование |
|---|---|
| `Cheetah.Core.Domain` (+ `ValueObjects`) | `AggregateRoot`, аудит (`DateTimeOffset?`), `Email`/`Phone` VO |
| `Cheetah.Core.StateMachine` | жизненный цикл `LeadStatus` |
| `Cheetah.Core.DataAccess` / `Specification` | репозитории + спецификации (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` / `Events` | команды/запросы, `IEventBus` |
| `CrmEntityFrameworkModule` + `…PostgreSqlModule` | EF Core + Npgsql |
| `Cheetah.Backend.Endpoints` / `Cheetah.AspNetCore` | эндпоинты |
| `Cheetah.RateLimit` | публичный приём лидов |
| `Cheetah.Saga` (+ `.EntityFrameworkCore`) | устойчивая конвертация (follow-up, у наследника) |
| `Cheetah.Modules.Deals.Client` | конвертация Lead → Deal (у наследника, в orchestrator'е) |
| `Cheetah.Permissions` | авторизация эндпоинтов |

---

## 13. Отличия от исходного эскиза плана (`docs/plans.md` §3/§L)

1. **Абстрактный шаблон вместо конкретного модуля** — по требованию расширяемости (§0): `LeadBase` +
   generic-хелперы (эскиз §L давал `sealed Lead`).
2. **Конвертация через порт `ILeadConversionOrchestrator`** в Domain, а не прямой Saga/Client в
   Application — чтобы шаблон не зависел от `Customer`/`Deals` (Customer — шаблон без Client). Saga и
   прямые Client-вызовы — это **реализации порта** у наследника.
3. **Скоринг — переопределяемый** `protected virtual int CalculateScore()` (точка расширения).
4. **Аудит-поля — `DateTimeOffset?`**; **события — после `SaveChangesAsync`** (как в Activities/Customer),
   без Outbox.
5. **`Attributes (jsonb)`** — карман расширения без миграций до появления Custom Fields.
