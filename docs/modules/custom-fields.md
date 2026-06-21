# Cheetah.Modules.CustomFields — кастомные поля сущностей (без миграций)

> Статус: **реализовано (MVP).** Код — в `src/Modules/CustomFields/` (8 сборок + 3 тест-проекта),
> добавлен в `Cheetah.slnx`; солюшн собирается, тесты зелёные: Domain 9 + Application 6 + Client 5 = **20**.
> Краткий гайд — `src/Modules/CustomFields/README.md`. Документ — пошаговый план сборки по канону
> `CLAUDE.md` (Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client)),
> своя БД PostgreSQL, общение через REST/gRPC + события через шину.
>
> Источник: раздел [§7](../plans.md) общего плана. Это **следующий модуль на реализацию** после
> завершённого [`FeatureManagement`](feature-management.md) — первый из «продуктовых» платформенных
> Tier 3 (рекомендация плана, п.6: «Далее Custom Fields, Workflow, Webhooks, Search»). К этому моменту
> уже существуют все сущности-потребители (Deal, Customer, Lead, Activity, SalesDocument…), которым
> нужны дополнительные поля, а инфраструктура (`Cheetah.Validation`, `Cheetah.Expressions.JsonLogic`,
> JSONB в PostgreSQL, `Cheetah.Core.Cache`) — зрелая и готова к переиспользованию.
>
> **Tier 3 — расширяемость.** Дать администратору добавлять поля зарегистрированным типам сущностей в
> рантайме, без миграций и без форка модулей-потребителей; хранить значения, валидировать их и
> управлять видимостью полей.

---

## 0. Главное архитектурное решение — расширяемость на уровне данных, а не типов

> **Custom Fields — это и есть механизм расширяемости платформы.** Поэтому, в отличие от
> [`Activities`](activities.md) / [`Customer`](../../src/Modules/Customer/README.md) /
> [`FeatureManagement`](feature-management.md), модуль **намеренно конкретный (`sealed`, как
> [`Deals`](deals.md))**, а не абстрактный шаблон.

Объяснение этого выбора — ключ ко всему плану:

- Расширяемость остальных модулей **структурна / compile-time**: наследник дописывает `sealed`-тип со
  своими полями (`ActivityDto : ActivityDtoBase`). Это нужно там, где у каждой команды свой набор полей
  процесса.
- Custom Fields решает ровно эту же задачу **данными, а не кодом**: «новое поле» — это строка
  `CustomFieldDefinition` в БД + ключ в JSONB-значениях, появляющаяся **в рантайме** через админку/реестр.
  Делать сам модуль ещё и абстрактным шаблоном — это «расширяемость над расширяемостью»: избыточно и
  запутанно.
- Собственные сущности модуля (`CustomFieldDefinition`, `CustomFieldValueSet`) уже полностью
  generic: определение описывает произвольное поле, значения лежат в `jsonb`. Прибавлять им
  compile-time-поля наследника нет смысла.

**Две оси расширяемости, которые модуль даёт потребителям (вместо наследования):**

1. **Динамическая (данные без миграций) — ядро.** Администратор объявляет поля для типа `crm.deal`
   через `POST /api/custom-fields/definitions`; значения хранятся в `jsonb`. Ни кода, ни миграции.
2. **Поведенческая (plugin-правила валидации) — `IValidationRule`.** Валидация значений переиспользует
   `Cheetah.Validation`; помимо встроенных правил (`Required`, `StringLength`, `NumberRange`,
   `Pattern`, `EnumValue`, `Expression`…) приложение регистрирует **свои** `IValidationRule`, не трогая
   ядро. Это прямой аналог `IFeatureFilter` в FeatureManagement.

> **Вывод для §15 «Отличия от эскиза».** Эскиз §7 не оговаривал форму модуля; здесь она зафиксирована
> явно: **конкретный модуль** (как Deals), потому что расширяемость у Custom Fields data-level.

---

## 1. Назначение и границы

**Что делает:**

- позволяет администратору объявлять **дополнительные поля** для зарегистрированных типов сущностей
  (`crm.deal`, `crm.customer`, `crm.lead`…): тип данных, обязательность, опции, правила валидации,
  условие видимости, порядок;
- хранит **значения** этих полей, привязанные к `(EntityType, EntityId)`, в собственном хранилище
  (`jsonb`), не трогая схему чужих таблиц;
- **валидирует** значения по декларативным правилам (`Cheetah.Validation`);
- управляет **видимостью** поля через условие `JsonLogic` (`Cheetah.Expressions.JsonLogic`);
- отдаёт значения батчем (анти-N+1) для списочных экранов; потребитель мёржит их со своей DTO на уровне
  API-композиции / BFF.

**Чего НЕ делает:**

- **не меняет схему чужих таблиц** — значения живут в БД самого модуля; сущности-потребители ссылаются
  по `(EntityType, EntityId)` (единая полиморфная конвенция, как в Tags/Activities/Notes);
- **не владеет бизнес-сущностью** — видит её как пару `(EntityType, EntityId)` и не знает её домена;
- **не рендерит формы** — отдаёт метаданные полей (тип/опции/порядок/видимость), UI строит фронт
  (Angular);
- **не доставляет уведомления** — публикует события, доставку делает Notification.

**Связи (по `Id`/ключам, без FK через границу модуля):** `EntityType` (ключ зарегистрированного типа),
`EntityId` (строкой, канонично), `TenantId?` (Tenants), `OwnerService` (кто зарегистрировал тип).

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Форма модуля | **конкретный (`sealed`, как Deals)** — расширяемость data-level, не наследование. См. §0 |
| Хранилище значений | **`jsonb` один набор на сущность** (`CustomFieldValueSet`: `(EntityType, EntityId)` → объект `{key: value}`), **не классический EAV** (меньше джойнов, GIN-индекс). §7.1 эскиза |
| Идентификатор сущности-потребителя | **`EntityId` строкой** (канонично, как `EntityId (string)` в Tags); `IdType` из регистрации валидирует вход |
| Хранилище правил валидации | `List<IValidationRule>` сериализуется в `jsonb` через `IValidationRuleSerializer` (полиморфный JSON ядра `Cheetah.Validation`) |
| Валидация значений | **`Cheetah.Validation`** (`IValidationEngine.ValidateAsync`) — не свой движок |
| Видимость поля | **`Cheetah.Expressions.JsonLogic`** (`IExpressionEvaluator.EvaluateBooleanAsync` над значениями + контекстом) |
| Регистрация типов | реестр-каталог: сервисы шлют `CustomFieldEntityTypeDescriptor` при старте (как Tags / Permissions.Catalog), идемпотентный upsert |
| Чтение метаданных в горячем пути | определения кэшируются (`ICacheService`), инвалидация по событию; меняются редко, читаются часто |
| Чтение значений | `GET .../values?entityType=&entityId=` → словарь; `batch-get` для списков (анти-N+1); композиция с основной DTO — на стороне потребителя/BFF |
| Мультитенантность | определения и значения **скоупятся по `TenantId`** (у тенантов свои поля); каталог типов — глобальный контракт платформы |
| StateMachine | **не используется** — ни у определения, ни у значения нет lifecycle-автомата |
| Celostность | подписка на `EntityDeletedIntegrationEvent(EntityType, EntityId)` → удаление набора значений (как Tags §8/Activities §2.6) |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Фильтрация по значению кастомного поля** (`WHERE values->>'priority' = 'high'`). MVP — фильтрация
   только по **определениям** (метаданным); фильтрация по значениям через `jsonb @>`/GIN — **follow-up**
   (нужна спец-спецификация, транслирующая в Npgsql JSON-операторы; raw LINQ запрещён каноном).
2. **Reference-поля** (`DataType.Reference` → ссылка на другую сущность). MVP хранит «сырой» `EntityId`
   строкой + `ReferenceEntityType` в опциях определения; разрешение/подгрузка ссылки — на стороне
   потребителя. Кросс-модульная валидация существования ссылки — follow-up (через Client цели).
3. **Версионирование определений** (изменили `DataType` у поля с данными). MVP — запрет смены `DataType`
   после создания (только soft-deactivate + новое поле); миграция значений — follow-up.
4. **Скоуп определений:** per-tenant (рекомендация — у тенантов свои поля) vs глобальные + tenant-override.
   Рекомендация — **per-tenant с опциональным глобальным шаблоном** от `OwnerService` (предопределённые
   поля из регистрации — глобальные, админ тенанта добавляет свои).
5. **Имена контрактов** `ICacheService` / `Cheetah.Core.Tenants` / событий шины — свериться при
   реализации (как делалось в FeatureManagement §1.2-5).

---

## 2. Архитектурная роль

> Custom Fields — **конечный бизнес-модуль со своей БД** (не «абстракция + модуль», как
> FeatureManagement). Переиспользует три инфраструктурных кирпича как сервисы: `Cheetah.Validation`
> (правила), `Cheetah.Expressions.JsonLogic` (видимость), `Cheetah.Core.Cache` (горячее чтение
> определений). Регистрация типов — тем же push-паттерном, что и Tags.

```
   startup register (push, идемпотентно)
  ┌────────────┐   CustomFieldEntityTypeDescriptor    ┌──────────────────────────────┐
  │ CRM-сервис │────────────────────────────────────▶│   CustomFields Service        │
  │ (Deals/…)  │                                      │  ┌────────────────────────┐  │
  └────────────┘                                      │  │ EntityType Catalog     │  │ ← какие типы можно расширять
        │ PUT values {entityType, entityId, values{}} │  ├────────────────────────┤  │
        │ ───────────────────────────────────────────▶│  │ FieldDefinitions       │  │ ← метаданные полей (per tenant)
        │                                              │  ├────────────────────────┤  │
        │ GET/batch-get values                         │  │ ValueSets (jsonb)      │  │ ← значения (EntityType,EntityId)→{}
        │ ◀───────────────────────────────────────────│  └────────────────────────┘  │
        │   мёрж с основной DTO на BFF/API-композиции  │   validate: Cheetah.Validation│
        ▼                                              │   visibility: JsonLogic       │
                                                       │   hot read: ICacheService     │
                                                       └───────────────┬───────────────┘
                                                                       │ events (Redis/Kafka)
                            DefinitionChanged (→ инвалидация кэша) / ValuesChanged
                                                                       ▼
                                              потребитель/UI обновляет форму и проекцию
```

Свой PostgreSQL; REST + (follow-up) gRPC для горячих `batch-get`; события через шину — всё по `CLAUDE.md`.

---

## 3. Структура проектов

По канону `CLAUDE.md`, как `Cheetah.Modules.Deals.*` (конкретный модуль, без абстрактных баз и без
`.Default`):

```
src/Modules/CustomFields/
├── Cheetah.Modules.CustomFields.DomainEvents/   # CustomFieldDefinitionCreated/Updated/Deactivated, ValuesChanged
├── Cheetah.Modules.CustomFields.Shared/          # enums (CustomFieldDataType, EntityIdType), конвенции ключей
├── Cheetah.Modules.CustomFields.Contracts/       # DTO/Request, CustomFieldEntityTypeDescriptor
├── Cheetah.Modules.CustomFields.Domain/          # CustomFieldDefinition, CustomFieldValueSet, спеки, репозитории
├── Cheetah.Modules.CustomFields.Infrastructure/  # EF Core, jsonb + GIN, миграции, реализации репозиториев
├── Cheetah.Modules.CustomFields.Application/      # CQRS: определения, значения, валидация, видимость, реестр
├── Cheetah.Modules.CustomFields.Api/             # Minimal API (декларативные эндпоинты)
├── Cheetah.Modules.CustomFields.Client/          # клиент: регистрация типов при старте + values API
└── Tests/
    ├── Cheetah.Modules.CustomFields.Domain.Tests/
    ├── Cheetah.Modules.CustomFields.Application.Tests/
    └── Cheetah.Modules.CustomFields.Client.Tests/
```

**Порядок зависимостей (строго, по `CLAUDE.md`):**

```
DomainEvents (Core.Events)
   ↓
Shared (Core)
   ↓
Contracts (Core + Shared + Validation — для сериализуемых правил в Request)
   ↓
Domain (DomainEvents + Specification + Validation)         ← CustomFieldDefinition/ValueSet, спеки, репо-интерфейсы
   ↓
Infrastructure (Domain + EF + EF.PostgreSql + Cache)       ← jsonb-конфиг, GIN, репозитории, кэш-провайдер
Application (Domain + Contracts + CQRS + Events
            + Validation + Expressions.JsonLogic)           ← CQRS, валидация значений, оценка видимости
Api (Application + Contracts + AspNetCore + Endpoints + Mapster)
   ↓
Client (Contracts)                                          ← регистрация типов + values API для server-to-server
```

> Канон: **Application зависит только на Domain** (не на Infrastructure); фильтрация — **только через
> спецификации**, не raw LINQ. `IValidationRule`-контракты живут в `Cheetah.Validation` и используются
> в Domain/Contracts как сериализуемые декларативные правила.

---

## 4. Доменная модель

### 4.1. Сводка

| Агрегат / сущность | Базовый | Роль | Ключевые поля |
|---|---|---|---|
| **`CustomFieldEntityType`** | `AggregateRoot<Guid>` | зарегистрированный тип (каталог, **глобальный**) | `Key` (`"crm.deal"`), `DisplayName`, `OwnerService`, `IdType`, `IsActive` |
| **`CustomFieldDefinition`** | `AggregateRoot<Guid>` + аудит | определение поля (**per tenant**) | `Id`, `TenantId?`, `EntityType`, `Key`, `Label`, `DataType`, `Required`, `Options[]?`, `ValidationRules` (jsonb), `VisibilityRule` (jsonlogic), `Order`, `IsActive` |
| **`CustomFieldValueSet`** | `AggregateRoot<Guid>` + аудит | набор значений сущности | `Id`, `TenantId?`, `EntityType`, `EntityId` (string), `Values` (jsonb `{fieldKey: value}`) |

> Аудит-интерфейсы ядра объявляют **`DateTimeOffset?`** (`ICreateAtEntity`/`IUpdatedAtEntity`) — как в
> Activities/FeatureManagement, использовать его. `AddDomainEvent`/`ClearDomainEvents` — на
> `AggregateRoot`. StateMachine **не** подключается.

**Почему `CustomFieldValueSet` — агрегат, а не строка-на-поле (EAV):** один `jsonb`-набор на сущность
даёт атомарный upsert значений, отсутствие N джойнов при чтении карточки и GIN-индексацию для будущей
фильтрации. Эскиз §7.1 прямо рекомендует JSONB вместо EAV.

### 4.2. `CustomFieldDefinition` — инварианты

```csharp
public sealed class CustomFieldDefinition : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid? TenantId { get; private set; }
    public string EntityType { get; private set; } = null!;   // "crm.deal" — зарегистрированный тип
    public string Key { get; private set; } = null!;          // "delivery_terms" — уникален в (Tenant, EntityType)
    public string Label { get; private set; } = null!;
    public CustomFieldDataType DataType { get; private set; }
    public bool Required { get; private set; }
    public IReadOnlyList<string>? Options { get; private set; }       // для Enum/MultiEnum
    public string? ValidationRulesJson { get; private set; }          // List<IValidationRule> (jsonb)
    public string? VisibilityRule { get; private set; }               // JsonLogic над значениями+контекстом
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private CustomFieldDefinition() { }   // EF

    public static CustomFieldDefinition Create(Guid? tenantId, string entityType, string key,
        string label, CustomFieldDataType dataType, bool required,
        IReadOnlyList<string>? options, string? validationRulesJson, string? visibilityRule, int order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (dataType is CustomFieldDataType.Enum or CustomFieldDataType.MultiEnum
            && (options is null || options.Count == 0))
            throw new InvalidOperationException("Enum field requires non-empty Options.");

        var d = new CustomFieldDefinition
        {
            Id = Guid.NewGuid(), TenantId = tenantId, EntityType = entityType.Trim(),
            Key = key.Trim(), Label = label, DataType = dataType, Required = required,
            Options = options, ValidationRulesJson = validationRulesJson,
            VisibilityRule = visibilityRule, Order = order
        };
        d.AddDomainEvent(new CustomFieldDefinitionCreatedIntegrationEvent(d.Id, d.TenantId, d.EntityType, d.Key));
        return d;
    }

    public void UpdateMetadata(string label, bool required, IReadOnlyList<string>? options,
        string? validationRulesJson, string? visibilityRule, int order)
    {
        // DataType неизменяем после создания (см. §1.2-3): защита значений в jsonb.
        Label = label; Required = required; Options = options;
        ValidationRulesJson = validationRulesJson; VisibilityRule = visibilityRule; Order = order;
        AddDomainEvent(new CustomFieldDefinitionChangedIntegrationEvent(Id, TenantId, EntityType, Key)); // → инвалидация кэша
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new CustomFieldDefinitionDeactivatedIntegrationEvent(Id, TenantId, EntityType, Key));
    }
}
```

### 4.3. `CustomFieldValueSet` — upsert значений

```csharp
public sealed class CustomFieldValueSet : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid? TenantId { get; private set; }
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;   // строкой, канонично
    public string ValuesJson { get; private set; } = "{}";   // jsonb {fieldKey: value}
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private CustomFieldValueSet() { }

    public static CustomFieldValueSet Create(Guid? tenantId, string entityType, string entityId, string valuesJson)
        => new() { Id = Guid.NewGuid(), TenantId = tenantId, EntityType = entityType,
                   EntityId = entityId, ValuesJson = valuesJson };

    public void Replace(string valuesJson)   // upsert — целиком заменяем набор (валидация — в Application)
    {
        ValuesJson = valuesJson;
        AddDomainEvent(new CustomFieldValuesChangedIntegrationEvent(TenantId, EntityType, EntityId));
    }
}
```

### 4.4. Domain — спецификации (фильтрация только через них)

```csharp
public sealed class DefinitionsByEntityTypeSpecification(Guid? tenantId, string entityType, bool onlyActive)
    : Specification<CustomFieldDefinition>
{
    public override Expression<Func<CustomFieldDefinition, bool>> ToExpression()
        => d => d.EntityType == entityType
             && (d.TenantId == tenantId || d.TenantId == null)        // глобальные шаблоны + поля тенанта
             && (!onlyActive || d.IsActive);
}

public sealed class DefinitionByKeySpecification(Guid? tenantId, string entityType, string key)
    : Specification<CustomFieldDefinition>
{
    public override Expression<Func<CustomFieldDefinition, bool>> ToExpression()
        => d => d.EntityType == entityType && d.Key == key && d.TenantId == tenantId;
}

public sealed class ValueSetByEntitySpecification(Guid? tenantId, string entityType, string entityId)
    : Specification<CustomFieldValueSet>
{
    public override Expression<Func<CustomFieldValueSet, bool>> ToExpression()
        => v => v.EntityType == entityType && v.EntityId == entityId && v.TenantId == tenantId;
}

// Для batch-get (анти-N+1): значения для списка id одного типа.
public sealed class ValueSetsByEntityIdsSpecification(Guid? tenantId, string entityType, IReadOnlyCollection<string> ids)
    : Specification<CustomFieldValueSet>
{
    public override Expression<Func<CustomFieldValueSet, bool>> ToExpression()
        => v => v.EntityType == entityType && v.TenantId == tenantId && ids.Contains(v.EntityId);
}
```

**Репозитории (Domain-интерфейсы):** `ICustomFieldDefinitionRepository : IRepository<CustomFieldDefinition, Guid>`,
`ICustomFieldValueSetRepository : IRepository<CustomFieldValueSet, Guid>` (реализации — в Infrastructure).

---

## 5. Shared — enums и конвенции ключей

```csharp
namespace Cheetah.Modules.CustomFields.Shared;

public enum CustomFieldDataType
{
    String = 0, Text = 1, Number = 2, Date = 3, DateTime = 4,
    Bool = 5, Enum = 6, MultiEnum = 7, Reference = 8
}

public enum CustomFieldEntityIdType { Guid = 0, Long = 1, String = 2 }  // как кастовать/валидировать EntityId

// Ключ типа — стабильный контракт "{service}.{entity}" (как в Tags).
public static class CustomFieldEntityTypes
{
    public static string Compose(string service, string entity) => $"{service}.{entity}";
}

public static class CustomFieldsConstants
{
    public const string Schema = "custom_fields";
    public const string DefinitionsCacheKeyPrefix = "cf:def:";  // cf:def:{tenant}:{entityType}
}
```

---

## 6. Contracts — DTO + дескриптор реестра

Все DTO — `ICrmResponse` (как в Deals/Activities). Правила валидации в Request — это
`IReadOnlyList<IValidationRule>` (полиморфный JSON ядра `Cheetah.Validation`, сериализуется через
`IValidationRuleSerializer`).

```csharp
public sealed record CustomFieldDefinitionDto : ICrmResponse
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = null!;
    public string Key { get; init; } = null!;
    public string Label { get; init; } = null!;
    public CustomFieldDataType DataType { get; init; }
    public bool Required { get; init; }
    public IReadOnlyList<string>? Options { get; init; }
    public IReadOnlyList<ValidationRuleDto> ValidationRules { get; init; } = [];
    public string? VisibilityRule { get; init; }
    public int Order { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed record CreateCustomFieldDefinitionRequest
{
    public string EntityType { get; init; } = null!;
    public string Key { get; init; } = null!;
    public string Label { get; init; } = null!;
    public CustomFieldDataType DataType { get; init; } = CustomFieldDataType.String;
    public bool Required { get; init; }
    public IReadOnlyList<string>? Options { get; init; }
    public IReadOnlyList<IValidationRule>? ValidationRules { get; init; }   // полиморфный JSON Cheetah.Validation
    public string? VisibilityRule { get; init; }                            // JsonLogic
    public int Order { get; init; }
}

// upsert значений сущности
public sealed record SetCustomFieldValuesRequest
{
    public string EntityType { get; init; } = null!;
    public string EntityId { get; init; } = null!;
    public IReadOnlyDictionary<string, object?> Values { get; init; } = new Dictionary<string, object?>();
}

public sealed record CustomFieldValuesDto : ICrmResponse
{
    public string EntityType { get; init; } = null!;
    public string EntityId { get; init; } = null!;
    public IReadOnlyDictionary<string, object?> Values { get; init; } = new Dictionary<string, object?>();
}

// Дескриптор реестра — сервисы декларируют расширяемые типы при старте (как Tags / Permissions.Catalog).
public sealed record CustomFieldEntityTypeDescriptor(
    string Key,                                   // "crm.deal"
    string DisplayName,                           // "Сделка"
    string OwnerService,                          // "deals-service"
    CustomFieldEntityIdType IdType,
    IReadOnlyList<PredefinedFieldDescriptor>? PredefinedFields = null);  // опц. поля «из коробки» (глобальные)

public sealed record PredefinedFieldDescriptor(
    string Key, string Label, CustomFieldDataType DataType, bool Required = false,
    IReadOnlyList<string>? Options = null);
```

---

## 7. Application — CQRS (определения, значения, валидация, видимость, реестр)

**Команды / запросы:**

```csharp
// Определения (админка)
CreateCustomFieldDefinitionCommand(CreateCustomFieldDefinitionRequest Request) : ICommand<Guid>;
UpdateCustomFieldDefinitionCommand(Guid Id, UpdateCustomFieldDefinitionRequest Request) : ICommand;
DeactivateCustomFieldDefinitionCommand(Guid Id) : ICommand;

// Значения
SetCustomFieldValuesCommand(SetCustomFieldValuesRequest Request) : ICommand;   // upsert + ВАЛИДАЦИЯ

// Реестр (сервисы при старте)
SyncEntityTypeRegistryCommand(IReadOnlyList<CustomFieldEntityTypeDescriptor> Descriptors) : ICommand; // идемпотентный upsert

// Запросы
ListDefinitionsQuery(string EntityType, bool OnlyActive) : IQuery<IReadOnlyList<CustomFieldDefinitionDto>>;
GetValuesQuery(string EntityType, string EntityId) : IQuery<CustomFieldValuesDto>;
// Батч-чтение значений для списка сущностей (анти-N+1):
BatchGetValuesQuery(string EntityType, IReadOnlyList<string> EntityIds)
    : IQuery<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>>;
// Метаданные формы с учётом видимости (какие поля показать в данном контексте):
GetVisibleFieldsQuery(string EntityType, string EntityId, IReadOnlyDictionary<string, object?> Context)
    : IQuery<IReadOnlyList<CustomFieldDefinitionDto>>;
```

### 7.1. Валидация значений — ядро `SetCustomFieldValuesCommand`

Переиспользуем `Cheetah.Validation` (не свой движок):

```csharp
public async ValueTask HandleAsync(SetCustomFieldValuesCommand cmd, CancellationToken ct)
{
    var req = cmd.Request;
    var tenantId = _tenant.CurrentTenantId;

    // 1. Активные определения типа (через спецификацию + кэш).
    var defs = await _definitions.GetAllAsync(
        new DefinitionsByEntityTypeSpecification(tenantId, req.EntityType, onlyActive: true), ct);

    // 2. Собираем FieldValidation: путь + значение + правила (Required + ValidationRules из определения).
    var fields = defs.Select(d => new FieldValidation(
        FieldPath: d.Key,
        Value: req.Values.GetValueOrDefault(d.Key),
        Rules: BuildRules(d))).ToList();      // BuildRules: RequiredRule(если d.Required) + десериализованные ValidationRulesJson

    // 3. Кросс-полевая валидация видит весь словарь.
    var result = await _validation.ValidateAsync(fields, req.Values, _services, ct);
    if (!result.IsValid)
        throw new ValidationException(result.Errors);   // → 400 с подсветкой полей

    // 4. Отсекаем значения незарегистрированных ключей, upsert набора.
    var allowed = defs.Select(d => d.Key).ToHashSet();
    var clean = req.Values.Where(kv => allowed.Contains(kv.Key)).ToDictionary(k => k.Key, v => v.Value);

    var set = await _valueSets.GetBySpecAsync(
        new ValueSetByEntitySpecification(tenantId, req.EntityType, req.EntityId), ct);
    if (set is null)
    {
        set = CustomFieldValueSet.Create(tenantId, req.EntityType, req.EntityId, Serialize(clean));
        _valueSets.Add(set);
    }
    else set.Replace(Serialize(clean));

    await _valueSets.SaveChangesAsync(ct);
    foreach (var e in set.DomainEvents) await _eventBus.PublishAsync(e, ct);
    set.ClearDomainEvents();
}
```

> `IValidationEngine` **собирает все ошибки** (не падает на первой) и даёт кросс-полевым правилам весь
> словарь — ровно то, что нужно форме (подсветить все поля разом). `UniqueRule`/`ExpressionRule`
> получают `IServiceProvider` для I/O-лукапов.

### 7.2. Видимость поля — `GetVisibleFieldsQuery`

```csharp
// Для каждого определения с VisibilityRule — оцениваем JsonLogic над {значения сущности + context}.
foreach (var d in defs)
{
    if (string.IsNullOrEmpty(d.VisibilityRule)) { visible.Add(d); continue; }
    var data = Merge(currentValues, ctx.Context);                  // var-источник для JsonLogic
    if (await _expr.EvaluateBooleanAsync(d.VisibilityRule, data, ct))
        visible.Add(d);
}
```

**Канон хендлера** (`CLAUDE.md`): репозиторий + спецификации (raw LINQ запрещён) → доменный мутатор →
`SaveChangesAsync` → публикация событий через `IEventBus` → `ClearDomainEvents`. `ValueTask<T>` +
`CancellationToken` всюду. Чтение определений — `AsNoTracking` + кэш.

**`SyncEntityTypeRegistryCommand` — идемпотентный upsert** типов по `Key` в рамках `OwnerService`:
новый тип создаётся; существующий обновляет `DisplayName`/`IdType`; предопределённые поля апсёртятся как
**глобальные** определения (`TenantId == null`), **не затирая** поля, добавленные администраторами
тенантов.

---

## 8. Infrastructure — EF Core + JSONB + GIN

### 8.1. EF-конфигурация

```csharp
public sealed class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> b)
    {
        b.ToTable("CustomFieldDefinitions", CustomFieldsConstants.Schema);
        b.Property(d => d.EntityType).HasMaxLength(100).IsRequired();
        b.Property(d => d.Key).HasMaxLength(100).IsRequired();
        b.Property(d => d.DataType).HasConversion<int>();
        b.Property(d => d.Options).HasColumnType("jsonb");           // string[] → jsonb
        b.Property(d => d.ValidationRulesJson).HasColumnType("jsonb");
        b.HasIndex(d => new { d.TenantId, d.EntityType, d.Key }).IsUnique();   // ключ уникален в (tenant, type)
        b.HasIndex(d => new { d.TenantId, d.EntityType });
        b.Ignore(d => d.DomainEvents);   // CRITICAL
    }
}

public sealed class CustomFieldValueSetConfiguration : IEntityTypeConfiguration<CustomFieldValueSet>
{
    public void Configure(EntityTypeBuilder<CustomFieldValueSet> b)
    {
        b.ToTable("CustomFieldValueSets", CustomFieldsConstants.Schema);
        b.Property(v => v.EntityType).HasMaxLength(100).IsRequired();
        b.Property(v => v.EntityId).HasMaxLength(200).IsRequired();
        b.Property(v => v.ValuesJson).HasColumnType("jsonb").IsRequired();
        b.HasIndex(v => new { v.TenantId, v.EntityType, v.EntityId }).IsUnique();  // один набор на сущность
        // GIN-индекс по jsonb для будущей фильтрации по значениям (§1.2-1) — добавить в миграции вручную:
        //   CREATE INDEX ix_cfvs_values_gin ON custom_fields."CustomFieldValueSets" USING gin ("ValuesJson" jsonb_path_ops);
        b.Ignore(v => v.DomainEvents);
    }
}
```

`CustomFieldsDbContext` (конкретный, плюрал в имени по неймингу) + `IDesignTimeDbContextFactory` +
миграция `InitialCustomFields` (схема `custom_fields`, GIN-индекс через `migrationBuilder.Sql(...)`).
Модуль `DependsOn` `CrmEntityFrameworkModule` + `CrmEntityFrameworkPostgreSqlModule`.

### 8.2. Кэш определений (горячее чтение)

Определения читаются на каждый показ/валидацию формы и **меняются редко** → кэш-aside через
`ICacheService`, ключ `cf:def:{tenant}:{entityType}`, инвалидация по
`CustomFieldDefinitionChanged/Deactivated`-событию на всех инстансах (через Redis/Kafka-шину).
`CustomFieldDefinitionCacheInvalidator` — обработчик события (как `FeatureCacheInvalidator`).

### 8.3. Репозитории

Реализации `ICustomFieldDefinitionRepository`/`ICustomFieldValueSetRepository` через `[Export(Scoped)]`
поверх `CustomFieldsDbContext`. Опционально — `IGridRepository<CustomFieldDefinition>` для админ-грида
определений (как в Catalog).

---

## 9. Api (декларативные эндпоинты)

Эндпоинты в `OnApplicationInitialization` (Minimal API, контроллеры запрещены); `partial`-модуль для
Source Generators; `IObjectMapper` (Mapster) для маппинга. Стиль — как Catalog/Identity (декларативные
эндпоинты `Cheetah.Backend.Endpoints` + генератор).

**Реестр (для сервисов):**

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `/api/custom-fields/registry/sync` | `SyncEntityTypeRegistryCommand` (батч-upsert дескрипторов; Client при старте) |
| GET | `/api/custom-fields/registry` | список зарегистрированных типов (для UI/админки) |

**Определения (админка):**

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| GET | `/api/custom-fields/definitions?entityType=&onlyActive=` | `ListDefinitionsQuery` |
| POST | `/api/custom-fields/definitions` | `CreateCustomFieldDefinitionCommand` |
| PUT | `/api/custom-fields/definitions/{id}` | `UpdateCustomFieldDefinitionCommand` |
| DELETE | `/api/custom-fields/definitions/{id}` | `DeactivateCustomFieldDefinitionCommand` (soft) |

**Значения:**

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| GET | `/api/custom-fields/values?entityType=&entityId=` | `GetValuesQuery` |
| PUT | `/api/custom-fields/values` | `SetCustomFieldValuesCommand` (upsert + валидация) |
| POST | `/api/custom-fields/values/batch-get` | `BatchGetValuesQuery` (анти-N+1 для списков) |
| POST | `/api/custom-fields/fields/visible` | `GetVisibleFieldsQuery` (форма с учётом видимости) |

Админка и реестр — за `Cheetah.Permissions`; `values`/`batch-get` — внутренний межсервисный путь.
> gRPC дублирует `batch-get` и `registry/sync` — горячий межсервисный путь (follow-up, как у Tags/Deals).

---

## 10. Client + регистрация типов потребителем (паттерн Tags / Permissions.Catalog)

```csharp
// в bootstrap сервиса-потребителя (например, Deals)
services.AddCustomFieldsClient(o => o.BaseUrl = cfg["CustomFields:Url"])
        .RegisterCustomFieldTypes(reg =>
        {
            reg.Add("crm.deal", "Сделка", x => x.IdType = CustomFieldEntityIdType.Guid);
            reg.Add("crm.customer", "Клиент", x =>
            {
                x.IdType = CustomFieldEntityIdType.Guid;
                x.PredefinedFields =
                [
                    new("industry", "Отрасль", CustomFieldDataType.Enum, Options: ["IT", "Retail", "Manufacturing"])
                ];
            });
        });
```

`CustomFieldsRegistrationSyncService` (hosted) при старте шлёт дескрипторы в `registry/sync`
(идемпотентно, с ретраями) — ровно как `TagsRegistrationSyncService`. **Не валит хост** при недоступности
каталога (`ContinueOnFailure`).

`ICustomFieldsClient`: `SyncTypesAsync(descriptors)`, `GetValuesAsync(entityType, entityId)`,
`BatchGetValuesAsync(entityType, ids)`, `SetValuesAsync(request)`, `ListDefinitionsAsync(entityType)`.
**MUST have `Client.Tests`** (сериализация значений и правил, 400 при невалидных значениях, поведение
при недоступном каталоге).

> **Композиция чтения.** Потребитель/BFF получает значения батчем и мёржит со своей DTO на уровне
> API-композиции (модуль значения не «вклеивает» в чужие сущности). Этот паттерн — обязательная часть
> README модуля.

---

## 11. Валидация и видимость — переиспользование инфраструктуры (ядро ценности)

> Главное отличие модуля от «просто JSONB-хранилки» — **декларативная валидация и видимость поверх
> готовых движков**. Свой движок не пишем.

| Возможность | Контракт | Откуда |
|---|---|---|
| Правила валидации | `IValidationRule` (полиморфный JSON), `IValidationEngine.ValidateAsync` | `Cheetah.Validation` |
| Встроенные правила | `Required`, `StringLength`, `NumberRange`, `DateRange`, `Pattern`, `EnumValue`, `Compare`, `RequiredIf`, `Expression`, `Unique` | `Cheetah.Validation.Rules` |
| Свои правила (plugin) | `IValidationRule` + регистрация в DI | приложение-потребитель (ось §0.2) |
| Сериализация правил в `jsonb` | `IValidationRuleSerializer` | `Cheetah.Validation` |
| Видимость поля | `IExpressionEvaluator.EvaluateBooleanAsync(rule, data, ct)` | `Cheetah.Expressions.JsonLogic` |

- **Required** маппится из `CustomFieldDefinition.Required` в `RequiredRule` автоматически; остальные
  правила — из `ValidationRulesJson` определения.
- **Кросс-полевые правила** (`Compare`, `RequiredIf`, `Expression`) видят весь словарь значений
  (`IValidationEngine` передаёт `allValues`).
- **Видимость** — `JsonLogic`-выражение над объединением {текущие значения + переданный `Context`};
  движок уже ограничивает длину/глубину/время выполнения (защита от злоупотреблений).

---

## 12. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | инварианты `CustomFieldDefinition` (Enum требует Options; `UpdateMetadata` не меняет `DataType`; Deactivate идемпотентен и публикует событие); `CustomFieldValueSet.Replace` публикует `ValuesChanged`; спецификации (глобальные + tenant, batch по ids) |
| `Application.Tests` | `SetCustomFieldValues`: валидный набор сохраняется, невалидный → `ValidationException` со **всеми** ошибками, значения незарегистрированных ключей отсекаются; идемпотентный `SyncEntityTypeRegistry` (не затирает поля тенанта); `GetVisibleFields` фильтрует по JsonLogic; `BatchGetValues` анти-N+1 (один запрос на список). Моки `IRepository`/`IValidationEngine`/`IExpressionEvaluator`/`IEventBus` (Moq) |
| `Client.Tests` | сериализация значений/правил (полиморфный JSON), 400 при невалидных значениях, `ContinueOnFailure` при недоступном каталоге |

> Валидация/видимость — **чистая логика поверх готовых движков**; основной приоритет покрытия —
> хендлер `SetCustomFieldValues` (сбор правил из определений + отсев чужих ключей) и оценка видимости.

---

## 13. План реализации (пошагово)

> Каждый шаг = отдельный коммит. После каждого слоя — `dotnet build` + `dotnet sln add` в
> `Cheetah.slnx`. Пакеты — через `Directory.Packages.props` (`PackageReference` без `Version`). Перед
> коммитом — удалять `nul`-файлы; сообщения — Conventional Commits **без** строки «🤖 Generated…».

**Фаза 1 — контракты**
1. `DomainEvents`: `CustomFieldDefinitionCreated/Changed/Deactivated`, `CustomFieldValuesChanged` (§14).
2. `Shared`: `CustomFieldDataType`, `CustomFieldEntityIdType`, `CustomFieldEntityTypes.Compose`,
   `CustomFieldsConstants` (схема, префикс кэша).
3. `Contracts`: `CustomFieldDefinitionDto`/`Create`/`Update`-Request, `SetCustomFieldValuesRequest`,
   `CustomFieldValuesDto`, `CustomFieldEntityTypeDescriptor`/`PredefinedFieldDescriptor`,
   `ValidationRuleDto` (или прямое использование `IValidationRule`).

**Фаза 2 — домен**
4. `Domain`: `CustomFieldEntityType`, `CustomFieldDefinition`, `CustomFieldValueSet` + инварианты
   (§4.2/§4.3).
5. `Domain`: спецификации (§4.4) + репозиторий-интерфейсы.
6. `Domain.Tests`: инварианты + спецификации (зелёные).

**Фаза 3 — инфраструктура**
7. `Infrastructure`: EF-конфиги (jsonb для `Options`/`ValidationRulesJson`/`ValuesJson`, уникальные
   индексы), `CustomFieldsDbContext` + `IDesignTimeDbContextFactory`.
8. `Infrastructure`: миграция `InitialCustomFields` (схема + GIN-индекс через `Sql`); репозитории.
9. `Infrastructure`: кэш определений (`ICacheService`) + `CustomFieldDefinitionCacheInvalidator`;
   `AddCustomFieldsInfrastructure(...)`.

**Фаза 4 — приложение**
10. `Application`: команды определений (`Create`/`Update`/`Deactivate`) + `SyncEntityTypeRegistry`
    (идемпотентный upsert).
11. `Application`: `SetCustomFieldValuesCommand` с валидацией через `IValidationEngine` (§7.1) +
    маппер правил из `ValidationRulesJson` + `RequiredRule`.
12. `Application`: запросы `ListDefinitions`/`GetValues`/`BatchGetValues`/`GetVisibleFields`
    (видимость через `IExpressionEvaluator`, §7.2); `AddCustomFieldsApplication(...)`.
13. `Application.Tests`: валидация (все ошибки, отсев чужих ключей), идемпотентный Sync, видимость,
    batch анти-N+1 (зелёные).

**Фаза 5 — API + клиент**
14. `Api`: декларативные эндпоинты (§9) — реестр, определения, значения, видимость; `partial`-модуль;
    авторизация админки через `Cheetah.Permissions`.
15. `Client`: `ICustomFieldsClient`/`HttpCustomFieldsClient`, `CustomFieldsRegistrationSyncService`
    (`ContinueOnFailure`), `.AddCustomFieldsClient(...)`/`.RegisterCustomFieldTypes(...)`;
    `Client.Tests` (зелёные).

**Фаза 6 — интеграция**
16. Подписка кэш-инвалидатора через шину; подписка на `EntityDeletedIntegrationEvent` → удаление набора
    значений (§16). Добавить все проекты в `Cheetah.slnx`.
17. Follow-up: фильтрация по значениям (`jsonb @>`/GIN-спецификация); gRPC для `batch-get`/`registry`;
    кросс-модульная валидация `Reference`-полей; аудит изменений определений (`Cheetah.Audit`);
    идемпотентность подписок через `Cheetah.Core.Inbox`; транзакционный Outbox.

**Фаза 7 — финал**
18. README модуля (назначение, регистрация типов, валидация/видимость, паттерн композиции чтения,
    ограничения). Статус плана → «реализовано»; обновить `plans.md` (отметка §7) и `MEMORY.md`.

---

## 14. События (публикует CustomFields)

```csharp
namespace Cheetah.Modules.CustomFields.DomainEvents;

public record CustomFieldDefinitionCreatedIntegrationEvent(Guid Id, Guid? TenantId, string EntityType, string Key) : EventBase;
public record CustomFieldDefinitionChangedIntegrationEvent(Guid Id, Guid? TenantId, string EntityType, string Key) : EventBase;     // → инвалидация кэша
public record CustomFieldDefinitionDeactivatedIntegrationEvent(Guid Id, Guid? TenantId, string EntityType, string Key) : EventBase; // → инвалидация кэша
public record CustomFieldValuesChangedIntegrationEvent(Guid? TenantId, string EntityType, string EntityId) : EventBase;
```

Потребители: **сам модуль** (инвалидация кэша определений на всех инстансах), Notes & Timeline/Search
(опц. — индексировать изменения значений), Audit (журнал изменений определений).

---

## 15. Зависимости от инфраструктуры

| Инфраструктурный модуль | Использование |
|---|---|
| `Cheetah.Core.Domain` | `AggregateRoot`/`Entity`, аудит-интерфейсы (`DateTimeOffset?`) |
| `Cheetah.Core.DataAccess` (`IRepository<T>`) | репозитории + спецификации |
| `Cheetah.Core.Specification` | вся фильтрация (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` | `ICommand`/`IQuery`/`IDispatcher`/`IEventBus` |
| **`Cheetah.Validation`** | `IValidationEngine`/`IValidationRule`/`IValidationRuleSerializer` + встроенные правила |
| **`Cheetah.Expressions.JsonLogic`** | видимость поля (`IExpressionEvaluator.EvaluateBooleanAsync`) |
| `Cheetah.Core.Cache` (`ICacheService`) | горячее чтение определений + инвалидация |
| `Cheetah.Core.Tenants` | tenant-scope определений и значений |
| `CrmEntityFrameworkModule` + `…PostgreSqlModule` | EF Core + Npgsql + `jsonb`/GIN |
| `Cheetah.Backend.Endpoints` + `Cheetah.Generators.Endpoints` | декларативные эндпоинты |
| `CrmMapsterModule` | маппинг Request↔Command, Entity→DTO |
| `Cheetah.Permissions` | авторизация админки/реестра |
| `Cheetah.Core.Grid` (опц.) | админ-грид определений (`IGridRepository`) |
| `Cheetah.Audit` (follow-up) | журнал изменений определений |
| `Cheetah.Core.Inbox` (follow-up) | идемпотентность подписок |

---

## 16. Производительность и целостность (цель 10k RPS)

- **Определения — из кэша** (меняются редко, читаются на каждый показ/валидацию формы); инвалидация по
  событию (eventually consistent — допустимо).
- **Значения — один `jsonb`-набор на сущность** (`(TenantId, EntityType, EntityId)` unique): чтение
  карточки без джойнов; `batch-get` обязателен для списков (анти-N+1).
- **GIN-индекс** по `ValuesJson` — задел под фильтрацию по значениям (`@>`), §1.2-1.
- **Удаление сущности у потребителя → висячие значения.** Подписка на
  `EntityDeletedIntegrationEvent(EntityType, EntityId)` (как Tags §8/Activities §2.6) удаляет набор
  значений; снятие регистрации типа — мягко (`Deprecated`), без немедленного удаления данных.

---

## 17. Отличия от исходного эскиза плана (`docs/plans.md` §7)

1. **Форма модуля зафиксирована — конкретный (`sealed`, как Deals)**, не абстрактный шаблон: Custom
   Fields сам по себе и есть механизм расширяемости (data-level), наследование типов избыточно (§0).
2. **Валидация и видимость — на готовых движках** `Cheetah.Validation` + `Cheetah.Expressions.JsonLogic`
   (а не «свой»), что снимает риск дублирования и сразу даёт богатый набор правил.
3. **Хранилище значений — один `jsonb`-набор на сущность** (`CustomFieldValueSet`), а не строка-на-поле
   (EAV) и не отдельная сущность `CustomFieldValue` из эскиза §7.2 — атомарный upsert + GIN.
4. **Реестр применимых типов — push-upsert при старте** (как Tags), с опциональными предопределёнными
   глобальными полями; per-tenant поля добавляет администратор.
5. **Аудит-поля — `DateTimeOffset?`**, события — через `IEventBus` после `SaveChanges`; Outbox/gRPC/
   фильтрация-по-значениям — **follow-up** (как у Activities/FeatureManagement).
6. **`DataType` неизменяем после создания** — защита данных в `jsonb` (миграция значений — follow-up).
