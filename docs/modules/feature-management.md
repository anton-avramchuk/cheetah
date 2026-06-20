# Cheetah.FeatureManagement + Cheetah.Modules.FeatureManagement.* — управление фич-флагами (расширяемый шаблон)

> Статус: **проектный план (черновик).** Кода ещё нет. Документ — пошаговый план сборки по канону
> `CLAUDE.md` (Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client)),
> своя БД PostgreSQL, общение через REST/gRPC + события через шину.
>
> Источник: раздел [§11](../plans.md) общего плана. Это **первый из платформенных модулей Tier 3**
> (рекомендация плана, п.6 «Рекомендуемого порядка»): фич-флаги позволяют безопасно выкатывать сами
> новые модули через постепенный rollout, поэтому их разумно поднять одними из первых.
>
> **Tier 3 — платформенная возможность.** Включать/выключать функциональность в рантайме без
> передеплоя, выкатывать постепенно (percentage rollout), таргетировать на тенант/пользователя/роль,
> вести A/B-варианты.

---

## 0. Главное требование — расширяемость

> **Модуль обязан быть расширяемым: сущности и ViewModel должны допускать расширение полями
> приложения-наследника без форка модуля — ровно как уже сделанные
> [`Cheetah.Modules.Activities`](activities.md) и [`Cheetah.Modules.Customer`](../../src/Modules/Customer/README.md).**

У фич-флагов расширяемость **двухмерная**, и это её главное отличие от Activities:

1. **Структурная (как Activities/Customer) — compile-time.** Бизнес-модуль поставляет **абстрактные
   базовые типы** (`FeatureFlagBase`, `TargetingRuleBase`, `FeatureFlagDtoBase`, `…RequestBase`) и
   generic-хелперы; наследник дописывает `sealed`-типы со своими полями (например, `JiraTicket`,
   `RolloutOwnerTeam`, метки релиза). Плюс опциональная сборка `.Default` с рабочей реализацией «из
   коробки» — тот же приём, что в Activities (§3.1).
2. **Поведенческая (специфична для фич-флагов) — plugin-точка `IFeatureFilter`.** Движок вычисления
   флага открыт для **пользовательских стратегий таргетинга**: помимо встроенных (`Percentage`,
   `Users`, `Tenants`, `Roles`, `TimeWindow`, `JsonLogic`) приложение регистрирует свои фильтры
   (`IFeatureFilter`), не трогая ядро. Это главная и самая востребованная точка расширения для
   фич-флагов — таргетинг по произвольному бизнес-признаку (план тарифа, регион, версия клиента).
3. **Динамическая — данные без миграций.** Сам каталог флагов наполняется **в рантайме** (через
   `registry/sync` и админку), а условия таргетинга задаются **JsonLogic-выражением** над
   `FeatureContext.Attributes`. То есть «новый флаг» и «новое правило» не требуют ни кода, ни
   миграции — только структурное расширение полей самого определения требует наследника.

**Что даёт наследнику (по образцу Activities):**

- `sealed class FeatureFlag : FeatureFlagBase` со своими полями;
- `sealed record FeatureFlagDto : FeatureFlagDtoBase`;
- свои `Create/Update`-Request-ы (наследники `…RequestBase`);
- свои `IFeatureFilter` без изменения движка;
- рабочая админка/каталог/оценка — «из коробки» (`.Default`), дописав ~6–7 классов или вовсе ничего.

> **Почему не `Deals`-стиль (конкретный модуль).** Deals намеренно сделан `sealed` (одна фиксированная
> схема). Фич-флаги, как и активности, по природе платформенны и почти всегда обрастают полями под
> процесс конкретной команды (владелец фичи, тикет, дата-таргет ремувала, метки эксперимента) —
> поэтому шаблон-подход оправдан.

---

## 1. Назначение и границы

**Что делает:**

- единая точка ответа на вопрос «включена ли фича X в данном контексте?» (`IFeatureManager`);
- декларативное объявление флагов модулями при старте (catalog/registry), как у
  [`Permissions.Catalog`](../../src/Modules/Permissions) и Tags;
- таргетинг: kill-switch, процент аудитории (стабильный по hash), allow/deny-списки
  (пользователи/тенанты/роли), временные окна, условия через JsonLogic, **пользовательские фильтры**;
- варианты (A/B/n) — флаг отдаёт не только `bool`, но и выбранный вариант/значение;
- горячее чтение из кэша + инвалидация по событию при изменении флага.

**Чего НЕ делает:**

- **не управляет правами доступа** — это [`Cheetah.Permissions`](../../src/Modules/Permissions). Флаг ≠
  permission: флаг про «фича существует/раскатана», permission про «этому субъекту можно». Граница
  жёсткая: фильтрация выдачи флагов по правам — не задача модуля;
- **не хранит бизнес-конфигурацию** (таймауты, лимиты) — это отдельный Settings/Config; фич-флаги это
  вкл/выкл и rollout, а не «значение настройки»;
- **не доставляет уведомления** об изменении флага — публикует событие, доставку делает Notification.

**Связи (по `Id`/`Key`, без FK через границу модуля):** `OwnerService` (логическое имя сервиса),
`TenantId?` (Tenants), `UserId?`/`Roles` приходят в `FeatureContext` от потребителя.

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Форма модуля | **абстрактный шаблон** (как Activities/Customer) + опц. `.Default` «из коробки». См. §0/§3.1 |
| Разделение | **абстракция `src/Cheetah.FeatureManagement`** (движок, без БД) **+ бизнес-модуль** (хранилище/каталог/админка). См. §2 |
| Идентификатор | `Guid` для агрегата; **`Key`** (`"{service}.{feature}"`) — стабильный публичный контракт |
| Хранилище | PostgreSQL (EF Core); миграции — **у наследника / в `.Default`** |
| Своя абстракция vs MS.FeatureManagement | **свой `IFeatureManager`** (единый стиль Cheetah), §11.9-1 плана |
| Стратегия правил | **«первое сработавшее»** (deny → allow → percentage → default), §11.9-2 плана |
| Чтение в горячем пути | из кэша (`ICacheService`), инвалидация по событию; eventually consistent — допустимо |
| Точка расширения таргетинга | plugin `IFeatureFilter` (встроенные + пользовательские) |
| Условия | `Cheetah.Expressions.JsonLogic` над `FeatureContext.Attributes` |
| Мультитенантность | глобальное правило + `TenantOverride` (`Cheetah.Core.Tenants`), §11.9-4 плана |
| Регистрация флагов | реестр-каталог: сервисы шлют `FeatureDefinitionDescriptor` при старте (как Permissions.Catalog/Tags) |
| Аудит изменений | через `Cheetah.Audit` (кто/когда включил фичу) — да, §11.9-5 плана |
| StateMachine | **не используется** — у флага нет lifecycle-состояния, `Enabled` это простой kill-switch |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **`.Default` «из коробки».** По образцу Activities — рекомендация **да**: отдельная сборка
   `Cheetah.Modules.FeatureManagement.Default` с `sealed`-типами, `DbContext` и миграцией, чтобы модуль
   работал сразу. Наследник может не подключать `.Default`. Решение — §3.1.
2. **Локальная реплика определений у потребителя** (push при старте + события) vs удалённый `evaluate`
   на каждый запрос. **Решено: локальная реплика** (in-proc-БД для монолита, `RemoteFeatureDefinitionProvider`
   + gRPC/HTTP-push для отдельного сервиса) — см. **§2.1 «Микросервисный режим»**. §11.9-3 плана.
3. **Scope флагов:** глобальные + tenant-override (рекомендация) vs обязательно per-tenant. §11.9-4.
4. **Хранилище вариантов A/B** — отдельная child-entity `FeatureVariantDef` (вес для распределения) vs
   inline в `Parameters(json)` правила. Рекомендация — отдельная сущность (типобезопасные веса).
5. **Имена контрактов кэша/событий/Tenants** — свериться с `Cheetah.Core.Cache` (`ICacheService`),
   `Cheetah.Core.Tenants`, `Cheetah.Expressions.JsonLogic` при реализации.

---

## 2. Архитектурная роль — абстракция + бизнес-модуль

> **Ключевое архитектурное решение (§11.2 плана).** Как `RateLimit` (абстракция) + `RateLimit.Redis`
> (провайдер) и `Permissions` + `Permissions.Catalog`: лёгкая инфраструктурная сборка с `IFeatureManager`
> для **всех потребителей** (без БД) + полноценный доменный модуль со своей БД, реализующий порт
> `IFeatureDefinitionProvider`.

```
   Потребитель (любой модуль: Deals/Activities/…)
        │  IFeatureManager.IsEnabledAsync("deals.kanban-v2", ctx)
        ▼
   ┌──────────────────────────────────────────────────────────────┐
   │  src/Cheetah.FeatureManagement   (АБСТРАКЦИЯ, без БД)        │
   │  • IFeatureManager (движок)   • FeatureContext              │
   │  • IFeatureFilter (plugin) ───┐  встроенные: Percentage/    │
   │  • [FeatureGate]/RequireFeature│  Users/Tenants/Roles/       │
   │  • IFeatureDefinitionProvider ─┼─ TimeWindow/JsonLogic       │
   │    (ПОРТ к хранилищу)          │  + пользовательские фильтры │
   └───────────────┬───────────────┴──────────────────────────────┘
                   │ реализует порт
                   ▼
   ┌──────────────────────────────────────────────────────────────┐
   │  src/Modules/FeatureManagement   (БИЗНЕС-МОДУЛЬ, своя БД)    │
   │  abstract FeatureFlagBase 1──* TargetingRuleBase            │
   │                           1──* FeatureVariantDef            │
   │                           1──* TenantOverride               │
   │  Infrastructure: store + cache (ICacheService) → реализация │
   │  IFeatureDefinitionProvider; Application: каталог/таргетинг │
   │  Api: админка + registry/sync + evaluate                   │
   └───────────────┬──────────────────────────────────────────────┘
                   │ publish (события)
                   ▼
   FeatureFlagCreated / Changed (→ инвалидация кэша) / Toggled
                   │
                   └──▶ все инстансы сбрасывают локальный кэш ключа (Redis/Kafka шина)
```

**Почему так.** `IFeatureManager` нужен **каждому** модулю-потребителю → он в лёгкой сборке без
зависимости на EF/БД. А определения/таргетинг/админка — это домен со своей БД → отдельный бизнес-модуль
реализует порт `IFeatureDefinitionProvider`. Потребитель зависит **только на абстракцию**, не на
бизнес-модуль (как на `Events` у обычных модулей).

### 2.1. Микросервисный режим (развёртывание и реплики)

> Модуль рассчитан и на **монолит**, и на **микросервисную** топологию. Разница инкапсулирована в одной
> точке — **какая реализация порта `IFeatureDefinitionProvider` подключена**. Сам движок
> (`IFeatureManager` + фильтры) и потребительский код (`IsEnabledAsync`) при смене топологии **не
> меняются**.

#### Две реализации одного порта

Порт `IFeatureDefinitionProvider` (§4.3) для того и введён, чтобы его источник можно было подменить под
топологию. Реализаций **две**:

| Где исполняется | Реализация | Источник определений | Сеть в горячем пути |
|---|---|---|---|
| Сервис FeatureManagement / монолит | `CachedFeatureDefinitionProvider` (Infrastructure, §8.2) | локальная БД + `ICacheService` | нет (БД только на cache-miss) |
| **Любой другой микросервис-потребитель** | **`RemoteFeatureDefinitionProvider`** (в `Client`) | **локальная in-memory реплика**, наполняемая по gRPC/HTTP при старте + обновляемая по событиям | **нет** (чистое вычисление в памяти) |

```
  ┌─────────────────────────────┐         ┌─────────────────────────────┐
  │  Сервис FeatureManagement   │         │   Микросервис "Billing"     │
  │  IFeatureManager            │         │   IFeatureManager           │
  │    └ CachedFeatureDef…Prov  │         │     └ RemoteFeatureDef…Prov │
  │        └ БД + ICacheService │         │         └ локальная реплика │ ← in-memory, без I/O на запрос
  └───────────┬─────────────────┘         └──────────▲──────────┬───────┘
              │ владеет БД, админка                  │ pull@старт │ subscribe
              │ publish Changed/Toggled              │ (gRPC/HTTP)│ Changed/Toggled
              ▼                                       │            ▼
        ┌─────────────────────  ОБЩАЯ ШИНА (Redis/Kafka)  ─────────────────────┐
        │  FeatureFlagChangedIntegrationEvent → каждый сервис обновляет реплику │
        └──────────────────────────────────────────────────────────────────────┘
```

`RemoteFeatureDefinitionProvider` **не имеет БД** удалённого сервиса — это его суть: `IsEnabledAsync` в
`Billing` считается над локальной репликой, **без сетевого хопа на каждый запрос** (цель 10k RPS). Сеть
задействована только: (1) один раз при старте — `pull` всего набора определений; (2) при изменении флага
— `Changed`-событие по шине триггерит точечное обновление реплики.

#### Три механизма, делающие это микросервис-безопасным

1. **`pull` при старте.** `RemoteFeatureDefinitionProvider` при инициализации тянет полный снимок
   определений (`GET /api/features/registry` / gRPC) и кладёт в локальную реплику. Дальше горячий путь —
   только память.
2. **`push` обновлений по шине.** Потребитель **подписывается на `.DomainEvents`** бизнес-модуля
   (`FeatureFlagChangedIntegrationEvent`/`Toggled`) и при событии обновляет/инвалидирует ключ в реплике.
   Это требует у потребителя ссылки на `Cheetah.Modules.FeatureManagement.DomainEvents` (контракты, без
   домена) — единственная дополнительная зависимость для микросервисного режима.
3. **Деградация при недоступности каталога.** Старт сервиса **не падает**, если FeatureManagement
   недоступен (`ContinueOnFailure`): провайдер поднимается с пустой/устаревшей (stale) репликой, движок
   отдаёт **значения по умолчанию** (флаг выключен). Реплика догоняется, как только каталог снова
   доступен (старт-`pull` с ретраями + ближайшее `Changed`-событие). Фич-флаг **никогда** не должен
   ронять чужой сервис.

#### Подключение в микросервисе-потребителе

```csharp
services.AddFeatureManagement()                 // абстракция: IFeatureManager + встроенные фильтры
        .AddFeatureCatalogClient(o => o.BaseUrl = cfg["Features:Url"])   // gRPC/HTTP к сервису FeatureManagement
        .UseRemoteReplica()                     // подключает RemoteFeatureDefinitionProvider + подписку на шину
        .RegisterFeatures(reg => reg.Add("billing.new-dunning-flow", "Новый flow напоминаний",
                                          x => x.ValueType = FeatureValueType.Bool));
```

Монолит вместо `.UseRemoteReplica()` подключает `AddFeatureManagementInfrastructure<>` (§8.2) — тот же
порт, но БД-реализация. **Потребительский код от выбора не зависит.**

#### Что это меняет в плане сборки

- `RemoteFeatureDefinitionProvider` и `UseRemoteReplica()` — **часть `Client`** (а не follow-up): для
  настоящей микросервисной топологии это основной механизм, см. шаг 16 в §12.
- **gRPC-контракт** на `evaluate` + `registry` (снимок определений) — горячий межсервисный путь;
  поднимается вместе с `Client` (в §12/§15 помечено follow-up только для случая «пока монолит»).
- Согласованность — **eventually consistent** между сервисами (лаг = время доставки события по шине).
  Для фич-флагов это допустимо и заложено (§8.2): kill-switch и rollout не требуют строгой согласованности.

---

## 3. Структура проектов

```
src/Cheetah.FeatureManagement/                    # АБСТРАКЦИЯ (зависит ТОЛЬКО на Core + Expressions.JsonLogic)
                                                  #   IFeatureManager, IFeatureFilter, FeatureContext,
                                                  #   FeatureDefinition (read-DTO движка), движок оценки,
                                                  #   IFeatureDefinitionProvider (порт), [FeatureGate]/RequireFeature

src/Modules/FeatureManagement/                    # БИЗНЕС-МОДУЛЬ
├── Cheetah.Modules.FeatureManagement.DomainEvents/  # FeatureFlagCreated/Changed/Toggled (Core.Events)
├── Cheetah.Modules.FeatureManagement.Shared/         # enums (FeatureValueType, RolloutType), конвенции ключей
├── Cheetah.Modules.FeatureManagement.Contracts/      # ABSTRACT DTO/Request базы + FeatureDefinitionDescriptor
├── Cheetah.Modules.FeatureManagement.Domain/         # abstract FeatureFlagBase/TargetingRuleBase, generic-спеки
├── Cheetah.Modules.FeatureManagement.Infrastructure/ # abstract DbContextBase/ConfigBase, реализация порта (store+cache)
├── Cheetah.Modules.FeatureManagement.Application/     # generic CQRS: каталог, таргетинг, оценка, фабрика/проектор
├── Cheetah.Modules.FeatureManagement.Api/            # abstract EndpointsBase<>, ApiModuleBase (админка + registry + evaluate)
├── (опц.) Cheetah.Modules.FeatureManagement.Default/ # sealed FeatureFlag + DbContext + миграции «из коробки»
├── Cheetah.Modules.FeatureManagement.Client/         # клиент: registry/sync при старте + удалённая оценка
└── Tests/
    ├── Cheetah.Modules.FeatureManagement.Domain.Tests/
    ├── Cheetah.Modules.FeatureManagement.Application.Tests/
    └── Cheetah.Modules.FeatureManagement.Client.Tests/
```

**Порядок зависимостей (строго):**

```
Cheetah.FeatureManagement (АБСТРАКЦИЯ; Core + Expressions.JsonLogic)  ← потребители зависят сюда
   ↑ реализует IFeatureDefinitionProvider
DomainEvents (Core.Events)
   ↓
Shared (Core)
   ↓
Contracts (Core + Shared + FeatureManagement-абстракция)   ← ABSTRACT DTO/Request + FeatureDefinitionDescriptor
   ↓
Domain (DomainEvents + Specification + FeatureManagement)  ← abstract FeatureFlagBase, generic-спеки
   ↓
Infrastructure (Domain + EF + EF.PostgreSql + Cache)       ← DbContextBase/ConfigBase, реализация порта (store+cache)
Application (Domain + Contracts + CQRS + Events)            ← generic handlers, движок-оценка, AddFeatureManagementApplication<>
Api (Application + Contracts + AspNetCore + Endpoints)      ← abstract EndpointsBase<>, ApiModuleBase
   ↓
Default (наследует всё, миграции) + Client (Contracts + FeatureManagement-абстракция)
```

> Канон `CLAUDE.md`: **Application зависит только на Domain** (не на Infrastructure); фильтрация —
> только через спецификации, не raw LINQ. Абстракция `Cheetah.FeatureManagement` — без EF/БД (порт).

### 3.1. Решение по «готовой реализации» (как в Activities)

Рекомендация — **гибрид**: шаблон + сборка `Cheetah.Modules.FeatureManagement.Default` с рабочей
реализацией «из коробки» (`sealed FeatureFlag`, конкретные DTO/Request, `FeatureManagementDbContext` +
миграция, готовые Api/Infrastructure/Application-регистрации). Приложение либо подключает `.Default`,
либо собирает свой набор по образцу §11. Снимает минус Customer/«ничего не работает, пока не допишешь»,
сохраняя расширяемость.

---

## 4. Абстракция `Cheetah.FeatureManagement` (контракты движка)

> Сверено с эскизом §11.3 плана; типы — в стиле остального Cheetah (`ValueTask`, `CancellationToken`).

### 4.1. Контекст и менеджер

```csharp
namespace Cheetah.FeatureManagement;

// Контекст оценки: кто/где спрашивает. Заполняется из HTTP-контекста или вручную.
public sealed record FeatureContext
{
    public Guid? UserId { get; init; }
    public Guid? TenantId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    // произвольные атрибуты для JsonLogic-условий и пользовательских фильтров
    public IReadOnlyDictionary<string, object?> Attributes { get; init; }
        = new Dictionary<string, object?>();
}

public interface IFeatureManager
{
    ValueTask<bool> IsEnabledAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default);
    ValueTask<FeatureVariant?> GetVariantAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default);
}

public sealed record FeatureVariant(string Name, string? Value);
```

### 4.2. Точка расширения — `IFeatureFilter` (plugin-стратегии таргетинга)

> Это **поведенческая расширяемость §0.2** — главная для фич-флагов. Встроенные фильтры (`Percentage`,
> `Users`, `Tenants`, `Roles`, `TimeWindow`, `JsonLogic`) регистрируются ядром; приложение добавляет
> свои, не трогая движок.

```csharp
public interface IFeatureFilter
{
    string Name { get; }   // совпадает с TargetingRule.FilterName
    ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct);
}

public sealed record FeatureFilterContext(
    string FeatureKey,
    FeatureContext Subject,
    IReadOnlyDictionary<string, object?> Parameters);  // из TargetingRule.Parameters(json)

// Регистрация пользовательского фильтра (открытость движка):
[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]
public sealed class PlanTierFilter : IFeatureFilter
{
    public string Name => "PlanTier";
    public ValueTask<bool> EvaluateAsync(FeatureFilterContext ctx, CancellationToken ct)
        => ValueTask.FromResult(
            ctx.Subject.Attributes.TryGetValue("planTier", out var v) &&
            ctx.Parameters.TryGetValue("allowed", out var a) && /* v ⊂ a */ true);
}
```

### 4.3. Порт к хранилищу — `IFeatureDefinitionProvider`

Абстракция **не знает**, откуда берутся определения; их поставляет Infrastructure бизнес-модуля
(store + cache). `FeatureDefinition` — плоский read-DTO движка (не доменная сущность).

```csharp
public interface IFeatureDefinitionProvider
{
    ValueTask<FeatureDefinition?> GetAsync(string featureKey, Guid? tenantId, CancellationToken ct);
    ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct);
}

public sealed record FeatureDefinition(
    string Key,
    bool Enabled,                            // kill-switch
    FeatureValueType ValueType,              // Bool | Variant
    IReadOnlyList<TargetingRuleDefinition> Rules,    // упорядочены по Order
    IReadOnlyList<VariantDefinition> Variants);

public sealed record TargetingRuleDefinition(int Order, string FilterName,
    IReadOnlyDictionary<string, object?> Parameters, string? ResultVariant);
public sealed record VariantDefinition(string Name, string? Value, int Weight);
```

### 4.4. Движок оценки — стратегия «первое сработавшее»

Чистая логика без I/O (определения уже в кэше). Порядок разрешения (§11.4 плана): `kill-switch` →
правила по `Order` (deny → allow → percentage → default) → значение по умолчанию.

```csharp
// псевдокод IFeatureManager.IsEnabledAsync
def = await provider.GetAsync(key, ctx.TenantId)            // из кэша
if def is null:           return false                       // незарегистрированный флаг → выкл
if not def.Enabled:       return false                       // kill-switch минует весь таргетинг
foreach rule in def.Rules (по Order):                        // первое сработавшее определяет результат
    filter = filters[rule.FilterName]                        // встроенный или пользовательский
    if await filter.EvaluateAsync(ctx, rule.Parameters):
        return rule.ResultVariant is null ? true : true      // вариант → см. GetVariantAsync
return false                                                 // default
```

**Percentage rollout — стабильный** по `hash(featureKey + (userId ?? tenantId)) % 100 < percent`, чтобы
один субъект не «мигал» между включено/выключено между запросами.

### 4.5. Декларативное использование на эндпоинте

```csharp
routes.MapGet("/api/deals/board", Handler).RequireFeature("deals.kanban-v2");  // endpoint-filter из абстракции
// или [FeatureGate("deals.kanban-v2")] на minimal-api делегате
```

---

## 5. Доменная модель бизнес-модуля (абстрактные базы)

### 5.1. Сводка

| Тип | Базовый | Роль |
|---|---|---|
| **`FeatureFlagBase`** | `AggregateRoot<Guid>` + `ICreateAtEntity` + `IUpdatedAtEntity` | определение флага (агрегат) |
| **`TargetingRuleBase`** | `Entity<Guid>` (child) | правило таргетинга (`Order`, `FilterName`, `Parameters`, `ResultVariant`) |
| **`FeatureVariantDef`** | `Entity<Guid>` (child) | вариант A/B (`Name`, `Value`, `Weight`) — конкретный, как `ActivityReminder` |
| **`TenantOverride`** | `Entity<Guid>` (child) | переопределение для тенанта (`TenantId`, `Enabled`, `Rules(json)?`) |
| generic `Specification<TFlag>` | Domain | `FlagByKey`, `FlagsByOwnerService`, `ActiveFlags` |

> Аудит-интерфейсы ядра объявляют **`DateTimeOffset?`** (`ICreateAtEntity`/`IUpdatedAtEntity`) — как в
> Activities, использовать его, не `DateTime`. `AddDomainEvent`/`DomainEvents`/`ClearDomainEvents` — на
> `AggregateRoot`. StateMachine **не** подключается (у флага нет состояния-автомата).

### 5.2. `FeatureFlagBase` — точки расширения

Принципы абстрактности — те же, что в Activities/Customer:

- нельзя `new` абстракцию в generic-handler → создание через `protected InitializeCore(...)` +
  `IFeatureFlagFactory`;
- мутаторы — `virtual` (наследник дополняет поведение);
- доп. поля наследника — в `sealed FeatureFlag : FeatureFlagBase` с `private set`.

```csharp
public abstract class FeatureFlagBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<TargetingRuleBase> _rules = new();
    private readonly List<FeatureVariantDef> _variants = new();
    private readonly List<TenantOverride> _overrides = new();

    public string Key { get; protected set; } = null!;          // "{service}.{feature}" — стабильный контракт
    public string Name { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public string OwnerService { get; protected set; } = null!;  // кто зарегистрировал
    public bool Enabled { get; protected set; }                  // kill-switch
    public FeatureValueType ValueType { get; protected set; }    // Bool | Variant
    public bool IsActive { get; protected set; } = true;

    public IReadOnlyList<TargetingRuleBase> Rules => _rules;
    public IReadOnlyList<FeatureVariantDef> Variants => _variants;
    public IReadOnlyList<TenantOverride> Overrides => _overrides;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected FeatureFlagBase() { } // EF + наследник

    protected void InitializeCore(Guid id, string key, string name, string ownerService,
        FeatureValueType valueType, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerService);
        Id = id; Key = key.Trim(); Name = name; OwnerService = ownerService;
        ValueType = valueType; Description = description; Enabled = false;  // по умолчанию выкл
        AddDomainEvent(new FeatureFlagCreatedIntegrationEvent(Key, OwnerService));
    }

    public virtual void Enable()  { if (!Enabled) { Enabled = true;  Toggle(true);  } }
    public virtual void Disable() { if (Enabled)  { Enabled = false; Toggle(false); } }
    private void Toggle(bool on) => AddDomainEvent(new FeatureFlagToggledIntegrationEvent(Key, on));

    public virtual void SetTargeting(IEnumerable<TargetingRuleBase> rules, Guid? tenantId = null)
    {
        _rules.Clear(); _rules.AddRange(rules.OrderBy(r => r.Order));
        AddDomainEvent(new FeatureFlagChangedIntegrationEvent(Key, tenantId));   // → инвалидация кэша
    }

    public void SetTenantOverride(TenantOverride ov)
    {
        _overrides.RemoveAll(o => o.TenantId == ov.TenantId);
        _overrides.Add(ov);
        AddDomainEvent(new FeatureFlagChangedIntegrationEvent(Key, ov.TenantId));
    }
}
```

### 5.3. Shared — enums и конвенции ключей

```csharp
namespace Cheetah.Modules.FeatureManagement.Shared;

public enum FeatureValueType { Bool = 0, Variant = 1 }
public enum RolloutType      { Off = 0, On = 1, Percentage = 2, Targeted = 3 }

// Ключ — стабильный публичный контракт "{service}.{feature}".
public static class FeatureKeys
{
    public static string Compose(string service, string feature) => $"{service}.{feature}";
}

// Имена встроенных фильтров (совпадают с IFeatureFilter.Name и TargetingRule.FilterName).
public static class BuiltInFilters
{
    public const string Percentage = "Percentage";
    public const string Users      = "Users";
    public const string Tenants    = "Tenants";
    public const string Roles      = "Roles";
    public const string TimeWindow = "TimeWindow";
    public const string JsonLogic  = "JsonLogic";
}
```

### 5.4. Domain — спецификации (generic, фильтрация только через них)

```csharp
public sealed class FlagByKeySpecification<TFlag>(string key)
    : Specification<TFlag> where TFlag : FeatureFlagBase
{
    public override Expression<Func<TFlag, bool>> ToExpression() => f => f.Key == key;
}

public sealed class FlagsByOwnerServiceSpecification<TFlag>(string ownerService)
    : Specification<TFlag> where TFlag : FeatureFlagBase
{
    public override Expression<Func<TFlag, bool>> ToExpression() => f => f.OwnerService == ownerService;
}

public sealed class ActiveFlagsSpecification<TFlag>()
    : Specification<TFlag> where TFlag : FeatureFlagBase
{
    public override Expression<Func<TFlag, bool>> ToExpression() => f => f.IsActive;
}
```

---

## 6. Contracts — расширяемые ViewModel + дескриптор реестра

Абстрактные `record`-базы (наследник добавляет поля через `init`-свойства), все DTO — `ICrmResponse`
(как в Activities/Deals).

```csharp
public abstract record FeatureFlagDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Key { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string OwnerService { get; init; } = null!;
    public bool Enabled { get; init; }
    public FeatureValueType ValueType { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<TargetingRuleDto> Rules { get; init; } = [];
    public IReadOnlyList<FeatureVariantDto> Variants { get; init; } = [];
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public abstract record CreateFeatureFlagRequestBase
{
    public string Key { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string OwnerService { get; init; } = null!;
    public FeatureValueType ValueType { get; init; } = FeatureValueType.Bool;
}

public abstract record SetTargetingRequestBase
{
    public IReadOnlyList<TargetingRuleDto> Rules { get; init; } = [];
}

// Дескриптор для реестра — сервисы декларируют свои флаги при старте (как Permissions.Catalog/Tags).
public sealed record FeatureDefinitionDescriptor(
    string Key, string Name, string OwnerService, FeatureValueType ValueType,
    string? Description = null, IReadOnlyList<string>? Variants = null);
```

> Наследник: `public sealed record FeatureFlagDto : FeatureFlagDtoBase { public string? JiraTicket { get; init; } }`
> — ровно как `ActivityDto : ActivityDtoBase`.

---

## 7. Application — generic CQRS + фабрика/проектор + оценка

Generic-handler'ы закрываются конкретными типами наследника (как Activities/Customer). Создание — через
`IFeatureFlagFactory` (нельзя `new` абстракцию); проекция в DTO — через `IFeatureFlagProjector`.

```csharp
public interface IFeatureFlagFactory<TFlag, TCreateRequest> where TFlag : FeatureFlagBase
{ TFlag Create(TCreateRequest request); }

public interface IFeatureFlagProjector<TFlag, TDto> where TFlag : FeatureFlagBase where TDto : FeatureFlagDtoBase
{ TDto ToDto(TFlag flag); }
```

**Команды / запросы (generic по `TFlag`/`TDto`/`TRequest`):**

```csharp
CreateFeatureFlagCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>;
EnableFeatureFlagCommand(string Key) : ICommand;            // kill-switch on
DisableFeatureFlagCommand(string Key) : ICommand;           // kill-switch off
SetTargetingCommand(string Key, IReadOnlyList<TargetingRuleDto> Rules) : ICommand;
SetTenantOverrideCommand(string Key, Guid TenantId, bool Enabled, string? RulesJson) : ICommand;
SyncFeatureRegistryCommand(IReadOnlyList<FeatureDefinitionDescriptor> Descriptors) : ICommand;  // идемпотентный upsert

GetFeatureFlagByKeyQuery<TDto>(string Key) : IQuery<TDto?>;
ListFeatureFlagsQuery<TDto>(string? OwnerService, bool? OnlyActive, int Page, int Size) : IQuery<IReadOnlyList<TDto>>;
// Батч-оценка для потребителей без локальной реплики (анти-N+1):
EvaluateFeaturesQuery(IReadOnlyList<string> Keys, FeatureContext Context)
    : IQuery<IReadOnlyDictionary<string, FeatureEvaluationDto>>;
```

**`SyncFeatureRegistryCommand` — идемпотентный upsert** дескрипторов в каталог (по `Key`): новый ключ
создаётся (выкл по умолчанию), существующий обновляет метаданные, но **не трогает таргетинг/Enabled**
(чтобы рестарт сервиса не сбрасывал ручные настройки админа).

**Регистрация (extension-метод, открытые generic нельзя через `[Export]`):**

```csharp
services.AddFeatureManagementApplication<FeatureFlag, CreateFeatureFlagRequest,
    FeatureFlagDto, FeatureFlagFactory, FeatureFlagProjector>();
```

**Канон хендлера** (`CLAUDE.md`): получить агрегат через репозиторий → доменный мутатор →
`SaveChangesAsync` → публикация событий через `IEventBus` (по образцу Customer/Activities — без Outbox в
MVP; апгрейд до транзакционного Outbox — follow-up) → `ClearDomainEvents`. Фильтрация — только
спецификациями. `ValueTask<T>` + `CancellationToken` всюду.

---

## 8. Infrastructure — EF Core + реализация порта (store + cache)

### 8.1. Абстрактные базы (расширяемая схема) — как в Activities

```csharp
public abstract class FeatureFlagConfigurationBase<TFlag> : IEntityTypeConfiguration<TFlag>
    where TFlag : FeatureFlagBase
{
    public void Configure(EntityTypeBuilder<TFlag> b)
    {
        b.ToTable("FeatureFlags", "features");
        b.HasIndex(f => f.Key).IsUnique();                  // ключ — стабильный контракт
        b.Property(f => f.Key).HasMaxLength(200).IsRequired();
        b.Property(f => f.OwnerService).HasMaxLength(100).IsRequired();
        b.Property(f => f.ValueType).HasConversion<int>();
        b.HasMany(f => f.Rules).WithOne().HasForeignKey("FlagId").OnDelete(DeleteBehavior.Cascade);
        b.HasMany(f => f.Variants).WithOne().HasForeignKey("FlagId").OnDelete(DeleteBehavior.Cascade);
        b.HasMany(f => f.Overrides).WithOne().HasForeignKey("FlagId").OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(f => f.OwnerService);

        b.Ignore(f => f.DomainEvents);  // CRITICAL
        ConfigureCustom(b);             // hook наследника: индексы/колонки доп. полей
    }
    protected virtual void ConfigureCustom(EntityTypeBuilder<TFlag> b) { }
}
```

`FeatureManagementDbContextBase<TContext, TFlag>` — по образцу `ActivitiesDbContextBase` (абстрактный
DbContext **нельзя мигрировать** → конкретный DbContext + `IDesignTimeDbContextFactory` + миграции у
наследника / в `.Default`). `TargetingRule.Parameters` и `TenantOverride.Rules` — колонки `jsonb`.

### 8.2. Реализация порта `IFeatureDefinitionProvider` — кэш-aside + инвалидация

> Это горячий путь (читается на каждый `IsEnabledAsync`). Чтение — из `ICacheService`, БД только на miss.

```csharp
[Export(LifetimeType.Scoped, typeof(IFeatureDefinitionProvider))]
public sealed class CachedFeatureDefinitionProvider<TFlag> : IFeatureDefinitionProvider
    where TFlag : FeatureFlagBase
{
    private readonly IRepository<TFlag, Guid> _flags;
    private readonly ICacheService _cache;     // Cheetah.Core.Cache

    public ValueTask<FeatureDefinition?> GetAsync(string key, Guid? tenantId, CancellationToken ct)
        => _cache.GetOrSetAsync($"feature:{tenantId?.ToString() ?? "global"}:{key}",
               async () => await LoadAndProjectAsync(key, tenantId, ct),
               expiry: TimeSpan.FromMinutes(10), ct: ct);
    // LoadAndProject: GetBySpecAsync(FlagByKeySpecification) → применить TenantOverride → спроецировать в FeatureDefinition
}
```

**Инвалидация по событию.** Подписка на `FeatureFlagChangedIntegrationEvent`/`Toggled` →
`_cache.RemoveAsync($"feature:*:{key}")` на **всех инстансах** (событие идёт через Redis/Kafka-шину).
Eventually consistent — для фич-флагов допустимо (§11.5 плана).

**Регистрация:** `services.AddFeatureManagementInfrastructure<FeatureManagementDbContext, FeatureFlag>();`
— `AddDbContext` (`UseNpgsql`), `IRepository<FeatureFlag, Guid>`, `CachedFeatureDefinitionProvider<>`,
подписка-инвалидатор.

### 8.3. Абстракция движка — регистрация встроенных фильтров

`services.AddFeatureManagement()` (в сборке-абстракции) регистрирует `IFeatureManager` + встроенные
`IFeatureFilter` (`Percentage`/`Users`/`Tenants`/`Roles`/`TimeWindow`/`JsonLogic`). Пользовательские
фильтры подхватываются через `[Export(..., typeof(IFeatureFilter))]` (§4.2). `JsonLogic`-фильтр зависит
на `Cheetah.Expressions.JsonLogic`.

---

## 9. Api (декларативные эндпоинты, расширяемые) + реестр

Эндпоинты — наследники базовых из `Cheetah.Backend.Endpoints` с `virtual`-методами (как Activities);
`partial` модуль-классы для Source Generators; маппинг в `OnApplicationInitialization`.

**Каталог/реестр (для сервисов):**

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `/api/features/registry/sync` | `SyncFeatureRegistryCommand` (батч-upsert дескрипторов; Client при старте) |
| GET | `/api/features/registry?ownerService=` | `ListFeatureFlagsQuery` |

**Админка флагов:**

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| GET/POST | `/api/features`, `/api/features/{key}` | CRUD / `GetFeatureFlagByKeyQuery` |
| POST | `/api/features/{key}/enable` · `/disable` | `Enable`/`DisableFeatureFlagCommand` (kill-switch) |
| PUT | `/api/features/{key}/targeting` | `SetTargetingCommand` (`rules[]`, варианты, веса) |
| PUT | `/api/features/{key}/tenants/{tenantId}` | `SetTenantOverrideCommand` |

**Оценка (для потребителей без локальной реплики):**

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `/api/features/evaluate` | `EvaluateFeaturesQuery` → `{ key: { enabled, variant? } }` (батч, анти-N+1) |

Админка и реестр — за `Cheetah.Permissions`; `evaluate` — внутренний (между сервисами).
> gRPC дублирует `evaluate` и `registry/sync` — горячий межсервисный путь (follow-up, как у Deals/Calendar).

---

## 10. Client + регистрация флагов потребителем (паттерн Permissions.Catalog)

```csharp
// в bootstrap сервиса-потребителя
services.AddFeatureManagement()                 // абстракция: IFeatureManager + встроенные фильтры
        .AddFeatureCatalogClient(o => o.BaseUrl = cfg["Features:Url"])
        .RegisterFeatures(reg =>
        {
            reg.Add("deals.kanban-v2", "Kanban-доска v2", x => x.ValueType = FeatureValueType.Bool);
            reg.Add("pricing.experiment", "A/B цены", x =>
            {
                x.ValueType = FeatureValueType.Variant;
                x.Variants = ["control", "treatment"];
            });
        });
```

`FeatureRegistrationSyncService` (hosted) при старте шлёт дескрипторы в `registry/sync` (идемпотентно, с
ретраями) — ровно как `TagsRegistrationSyncService`/`Permissions.Catalog`. **Не валит хост** при
недоступности каталога (`ContinueOnFailure`) — флаги доступны со значениями по умолчанию (выкл).

`IFeatureCatalogClient`: `SyncAsync(descriptors)`, `EvaluateAsync(keys, context)` — для потребителей без
локальной реплики, и `PullDefinitionsAsync(tenantId?)` — снимок определений для `RemoteFeatureDefinitionProvider`.
**MUST have `Client.Tests`** (сериализация, 404/409, поведение при недоступности).

> **Микросервисный режим — `.UseRemoteReplica()`.** В микросервисе-потребителе вместо удалённого
> `evaluate` на каждый запрос подключается `RemoteFeatureDefinitionProvider` (локальная in-memory
> реплика: `pull` при старте + `push`-обновление по `FeatureFlagChangedIntegrationEvent` с шины). Это
> та же сборка `Client`. Полностью — в **§2.1 «Микросервисный режим»**.

---

## 11. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | инварианты `FeatureFlagBase` (Enable/Disable идемпотентны и публикуют `Toggled`; `SetTargeting` сортирует по `Order` и публикует `Changed`; tenant-override заменяется, а не дублируется); наследование (sealed-`FeatureFlag` с доп. полем создаётся фабрикой); **движок оценки** (kill-switch минует таргетинг; «первое сработавшее»; стабильность percentage по hash; вариант по весам) |
| `Application.Tests` | generic-хендлеры с моками `IRepository`/`IEventBus`/фабрики/проектора (Moq): идемпотентный `SyncFeatureRegistry` (не сбрасывает Enabled/таргетинг существующего), batch-`evaluate` анти-N+1, инвалидация кэша по событию |
| `Client.Tests` | сериализация registry/evaluate; обработка 404/409; `ContinueOnFailure` при недоступном каталоге |
| (Default) | интеграционный smoke: register → enable → targeting → evaluate в разных контекстах; миграция применяется |

> Движок (`IFeatureManager` + фильтры) — **чистая логика, юнит-тестируется без БД** (как `ISlotEngine` в
> Booking). Это ядро модуля и главный приоритет покрытия.

---

## 12. План реализации (пошагово)

> Каждый шаг = отдельный коммит. После каждого слоя — `dotnet build` + `dotnet sln add` в
> `Cheetah.slnx`. Пакеты — через `Directory.Packages.props` (`PackageReference` без `Version`).

**Фаза 0 — абстракция (ядро движка)**
1. ✅ `src/Cheetah.FeatureManagement`: `FeatureContext`, `IFeatureManager`, `FeatureVariant`,
   `IFeatureFilter`/`FeatureFilterContext`, `IFeatureDefinitionProvider`/`FeatureDefinition`,
   `RequireFeature` (endpoint-filter), движок `FeatureManager` (стратегия «первое сработавшее» + deny
   через `Negate`, стабильный percentage по SHA-256, веса вариантов) + 6 встроенных фильтров
   (Percentage/Users/Tenants/Roles/TimeWindow/JsonLogic). `CrmFeatureManagementModule`. Тесты движка и
   фильтров — `Cheetah.FeatureManagement.Tests` (16 зелёных). Добавлено в `Cheetah.slnx`.

**Фаза 1 — контракты бизнес-модуля**
2. ✅ `DomainEvents`: `FeatureFlagCreated/Changed/Toggled` (§5.2/§13).
3. ✅ `Shared`: `RolloutType` + `FeatureKeys.Compose`. (`BuiltInFilterNames` живёт в абстракции — не дублируем; `FeatureValueType` тоже в абстракции, т.к. на него опирается движок.)
4. ✅ `Contracts`: абстрактные `FeatureFlagDtoBase`/`CreateFeatureFlagRequestBase`/`SetTargetingRequestBase`/`SetTenantOverrideRequestBase` + `TargetingRuleDto`/`FeatureVariantDto`/`FeatureEvaluationDto` + `FeatureDefinitionDescriptor` (§6). Все собираются, добавлены в `Cheetah.slnx`.

**Фаза 2 — домен**
5. ✅ `Domain`: `FeatureFlagBase` (abstract) + child-сущности `TargetingRule`/`FeatureVariantDef`/
   `TenantOverride` (**конкретные** — расширяемость таргетинга идёт через plugin-фильтры, не наследование
   правила; так же `ActivityReminder` конкретен в реальном Activities) + `InitializeCore`/мутаторы
   (`Enable`/`Disable`/`SetTargeting`/`SetVariants`/`SetTenantOverride`/`RegisterMetadata` —
   идемпотентный upsert метаданных без сброса Enabled/таргетинга).
6. ✅ `Domain`: generic-спецификации `FlagByKey`/`FlagsByOwnerService`/`ActiveFlags` (§5.4).
7. ✅ `Domain.Tests`: инварианты + наследование (`TestFeatureFlag : FeatureFlagBase` с доп. полем) — 6 зелёных.

**Фаза 3 — инфраструктура**
8. ✅ `Infrastructure`: `FeatureFlagConfigurationBase<>` (+`ConfigureCustom`, дети через `_rules`/`_variants`/
   `_overrides`, `jsonb`-параметры, уникальный индекс по `Key`) + child-конфиги + `FeatureManagementDbContextBase<>`.
9. ✅ `Infrastructure`: `CachedFeatureDefinitionProvider<>` (реализация порта, кэш-aside через `ICacheService`
   + `Include` детей + `FeatureDefinitionMapper` с разбором JSON и tenant-override) + `FeatureCacheInvalidator`
   (обработчик `Changed`/`Toggled`). Константы — в `Shared` (`FeatureManagementConstants`).
10. ✅ `AddFeatureManagementInfrastructure<TContext,TFlag>` (DbContext, мигратор, репозиторий, провайдер,
    инвалидатор). Подписка инвалидатора на шину — Фаза 6 (шаг 17).

**Фаза 4 — приложение**
11. ✅ `Application`: `IFeatureFlagFactory` (+`CreateFromDescriptor`)/`IFeatureFlagProjector`; команды
    `CreateFeatureFlag`/`Enable`/`Disable`/`SetTargeting`/`SetTenantOverride`/`SyncFeatureRegistry`
    (идемпотентный upsert метаданных, не сбрасывает Enabled/таргетинг) + запросы `GetFeatureFlagByKey`/
    `ListFeatureFlags`/`EvaluateFeatures` (через `IFeatureManager`, дедуп ключей). Дочерне-осведомлённый
    `IFeatureFlagRepository<TFlag>` (Domain) + реализация в Infrastructure — чтобы заменять правила без EF
    в Application. `TargetingRuleMapper` (DTO↔домен, JSON-параметры).
12. ✅ `AddFeatureManagementApplication<TFlag,TCreateRequest,TDto,TFactory,TProjector>`.
13. ✅ `Application.Tests`: Enable→Toggled, SetTargeting→Changed, идемпотентный Sync (создание + сохранение
    настроек), проекция, батч-Evaluate с дедупом — 5 зелёных.

**Фаза 5 — API + (Default) + клиент**
14. `Api`: `FeatureEndpointsBase<>` + `ApiModuleBase` (§9): админка + registry + evaluate.
15. (Опц.) `.Default`: sealed `FeatureFlag`, конкретные Contracts, фабрика/проектор,
    `FeatureManagementDbContext` + `IDesignTimeDbContextFactory` + миграция `InitialFeatures` (схема
    `features`), готовые регистрации.
16. `Client`: `IFeatureCatalogClient` + `FeatureRegistrationSyncService` + `.RegisterFeatures(...)` +
    **`RemoteFeatureDefinitionProvider` / `.UseRemoteReplica()`** (микросервисный режим, §2.1),
    `Client.Tests`.

**Фаза 6 — интеграция**
17. Подписка-инвалидатор кэша через шину (все инстансы); идемпотентность — `Cheetah.Core.Inbox` (опц.).
18. Аудит изменений флагов через `Cheetah.Audit`. Пилот: завести флаг `deals.kanban-v2`, выкатить
    percentage-rollout, проверить стабильность и инвалидацию.

**Фаза 7 — финал**
19. README обеих сборок (это **базовые/шаблонные** модули → README обязателен по чек-листу `CLAUDE.md`:
    назначение, точки расширения — `IFeatureFilter` + наследование, extension-методы, пример).
20. Обновить статус этого плана на «реализовано» + ссылка на код; обновить `MEMORY.md`.

---

## 13. События (публикует FeatureManagement)

```csharp
namespace Cheetah.Modules.FeatureManagement.DomainEvents;

FeatureFlagCreatedIntegrationEvent(string Key, string OwnerService) : EventBase;
FeatureFlagChangedIntegrationEvent(string Key, Guid? TenantId) : EventBase;   // → инвалидация кэша
FeatureFlagToggledIntegrationEvent(string Key, bool Enabled) : EventBase;
```

Потребители: **сам модуль** (инвалидация локального кэша на всех инстансах), Notification (опц. — оповестить
владельца сервиса), Audit (журнал изменений).

---

## 14. Зависимости от инфраструктуры

| Инфраструктурный модуль | Использование |
|---|---|
| `Cheetah.Core.Domain` | `AggregateRoot`/`Entity`, аудит-интерфейсы (`DateTimeOffset?`) |
| `Cheetah.Core.DataAccess` (`IRepository<T>`) | репозитории + спецификации |
| `Cheetah.Core.Specification` | вся фильтрация (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` | `ICommand`/`IQuery`/`IDispatcher`/`IEventBus` |
| `Cheetah.Core.Cache` (`ICacheService`) | горячее чтение определений флагов + инвалидация |
| `Cheetah.Expressions.JsonLogic` | фильтр `JsonLogic` (условия таргетинга над `Attributes`) |
| `Cheetah.Core.Tenants` | `TenantOverride` (tenant-scope флагов) |
| `Cheetah.Core.Inbox` (опц.) | идемпотентность подписки-инвалидатора |
| `CrmEntityFrameworkModule` + `…PostgreSqlModule` | EF Core + Npgsql |
| `Cheetah.Backend.Endpoints` + `Cheetah.Generators.Endpoints` | декларативные эндпоинты |
| `Cheetah.Permissions` | авторизация админки/реестра |
| `Cheetah.Audit` | журнал изменений флагов (кто/когда) |
| `Cheetah.RateLimit` (опц.) | защита публичного `evaluate`, если выставлен наружу |

---

## 15. Отличия от исходного эскиза плана (`docs/plans.md` §11)

1. **Абстрактный шаблон вместо конкретного модуля** — по требованию расширяемости (§0): бизнес-модуль
   даёт `FeatureFlagBase` + generic-хелперы (как Activities/Customer), эскиз §11.4 давал `sealed`.
2. **Двойная расширяемость явно разведена** (§0): структурная (наследование, как Activities) +
   поведенческая (`IFeatureFilter` plugin) + динамическая (реестр/JsonLogic без миграций).
3. **Аудит-поля — `DateTimeOffset?`** (сверено с `ICreateAtEntity`/`IUpdatedAtEntity` ядра).
4. **События — через `IEventBus` после `SaveChanges`** (паттерн Customer/Activities), Outbox — follow-up
   (в отличие от Deals). База DbContext — чистый `CrmDbContext<T>`.
5. **Опциональная сборка `.Default`** — модуль работает «из коробки», оставаясь расширяемым (как
   рекомендовано для Activities; Permissions.Catalog/Customer такой не имеют).
6. **`SyncFeatureRegistryCommand` не сбрасывает** ручные `Enabled`/таргетинг существующего флага при
   рестарте сервиса (идемпотентный upsert только метаданных) — явная защита от затирания настроек админа.
7. **StateMachine не используется** — у флага нет состояния-автомата (эскиз его и не требовал).

**Микросервисный режим** (§2.1) — **основное решение, не follow-up**: `RemoteFeatureDefinitionProvider`
+ локальная реплика (`pull` при старте + `push` по шине) в `Client`; gRPC-снимок определений —
горячий межсервисный путь.

**Отложено (follow-up):** транзакционный Outbox; круг вариантов A/B/n с детерминированным распределением
по весам; совместимость-обёртка над `Microsoft.FeatureManagement` (если потребуется). gRPC помечается
follow-up только для сценария «пока монолит» — в микросервисной топологии он входит в MVP `Client` (§2.1).
