# Cheetah.Workflow + Cheetah.Modules.Workflow.* — автоматизация бизнес-процессов (no-code правила)

> Статус: **проектный план (черновик), кода ещё нет.** Документ — исчерпывающий, код-уровневый план
> сборки по канону `CLAUDE.md` (Events → Shared → Contracts → Domain → Infrastructure → Application →
> Api (+ Client)), своя БД PostgreSQL, общение через REST/gRPC + события через шину. Код в документе —
> образец, сверенный с реальными контрактами репозитория (`IEventBus`, `IInboxStore`,
> `OutboxEventBus`, `CronBackgroundTask`, `Saga<TData>`, `IExpressionEvaluator`, паттерны Api/Client из
> [`FeatureManagement`](feature-management.md)).
>
> Источник: раздел [§8](../plans.md#8-workflow--automation-бизнес-процессы) общего плана. Это **второй из
> платформенных модулей Tier 3** (после Feature Management), следующий по рекомендованному порядку
> (`plans.md`, п.6): к этому моменту все модули Tier 1–2 уже публикуют интеграционные события — есть из
> чего строить триггеры.

## Оглавление

| § | Раздел |
|---|---|
| 0 | [Модель интеграции — ответ на «единый микросервис»](#0-главное--модель-интеграции-ответ-на-единый-микросервис-к-которому-ходят) |
| 1 | [Назначение и границы](#1-назначение-и-границы) |
| 2 | [Архитектурная роль](#2-архитектурная-роль) |
| 3 | [Структура проектов](#3-структура-проектов) |
| 4 | [Абстракция `Cheetah.Workflow`](#4-абстракция-cheetahworkflow-полные-контракты) |
| 5 | [Доменная модель (полный код)](#5-доменная-модель-бизнес-модуля-полный-код) |
| 6 | [Shared](#6-shared--enums-и-конвенции) |
| 7 | [Contracts](#7-contracts--расширяемые-viewmodel--дескрипторы) |
| 8 | [Domain — спецификации](#8-domain--спецификации) |
| 9 | [Application — движок + CQRS (полный код)](#9-application--движок--cqrs-полный-код) |
| 10 | [Infrastructure (полный код)](#10-infrastructure--полный-код) |
| 11 | [Api](#11-api--декларативные-эндпоинты--реестр) |
| 12 | [Client + регистрация](#12-client--регистрация-контрибутора) |
| 13 | [Тесты](#13-тесты) |
| 14 | [План реализации (пошагово)](#14-план-реализации-пошагово) |
| 15 | [События](#15-события-публикует-workflow) |
| 16 | [Сквозные примеры (3 правила)](#16-сквозные-примеры-end-to-end) |
| 17 | [Производительность, конкурентность, отказоустойчивость](#17-производительность-конкурентность-отказоустойчивость) |
| 18 | [Безопасность](#18-безопасность) |
| 19 | [Зависимости от инфраструктуры](#19-зависимости-от-инфраструктуры) |
| 20 | [Отличия от эскиза §8](#20-отличия-от-исходного-эскиза-плана-docsplansmd-8) |
| 21 | [Глоссарий](#21-глоссарий) |

---

## 0. Главное — модель интеграции (ответ на «единый микросервис, к которому ходят»)

Интуиция «есть один сервис Workflow, к которому остальные ходят» **наполовину верна и наполовину
инвертирована** — это надо зафиксировать до старта, иначе модуль спроектируется неправильно.

Workflow — **не** RPC-сервис, который дёргают синхронно. Это **центральный реактивный потребитель
событий + диспетчер действий**. Связь с остальными модулями идёт по трём независимым каналам:

| Канал | Направление | Кто инициирует | Механизм |
|---|---|---|---|
| **Триггеры** | модуль → Workflow | модуль-источник | публикация интеграционного события в **шину**; Workflow подписан. Источник **не знает** о Workflow |
| **Действия** | Workflow → модуль | Workflow | вызов **Client** модуля-цели (in-proc) ИЛИ команда-событие по шине ИЛИ шаблонный HTTP/gRPC |
| **Регистрация** | модуль → Workflow | модуль при старте | `registry/sync`: модуль декларирует, **какие события он шлёт** (trigger-дескрипторы) и **какие операции открывает** (action-дескрипторы) — как Feature Management регистрирует флаги |

> **Вывод.** «Ходят к Workflow» — это только **регистрация при старте** (как в Feature Management). В
> рантайме модули **ничего не зовут** у Workflow: они публикуют события (а Workflow слушает), а действия
> исходят **из** Workflow наружу. Это полностью развязывает источники триггеров от Workflow и снимает
> циклические зависимости.

```
   ┌─ Модуль-источник (Deals) ─┐                  ┌──── WORKFLOW (центральный сервис) ─────┐
   │  publish DealWonEvent ─────┼──┐               │  1. firehose-подписка (1 канал)        │
   └────────────────────────────┘  │   trigger     │  2. подбор активных правил по EventName│
                                    ▼  (конверт)    │  3. условие (JsonLogic над payload)    │
              ┌──── ШИНА (Redis/Kafka) ────┐ ─────▶ │  4. план действий (Order)              │
              │ WorkflowEventEnvelope       │        │  5. журнал AutomationRun + идемпотент. │
              └─────────────────────────────┘        └──────────────────┬─────────────────────┘
   ┌─ Модуль-цель (Activities) ─┐                                       │ action
   │  CreateActivityCommand ◀───┼───── IWorkflowActionExecutor ─────────┘
   └────────────────────────────┘     (in-proc plugin | bus-команда | HTTP)
```

### 0.1. Главное требование — расширяемость

> **Модуль обязан быть расширяемым** — как [`Activities`](activities.md)/[`Customer`](../../src/Modules/Customer/README.md)
> и [`FeatureManagement`](feature-management.md). У Workflow, как у фич-флагов, расширяемость
> **многомерная**, и поведенческая ось здесь — главная:

1. **Поведенческая (главная) — plugin-точки `IWorkflowAction` и `IWorkflowTrigger`.** Прямой аналог
   `IFeatureFilter` в Feature Management. Каталог действий и триггеров **открыт**: каждый модуль
   контрибутит свои действия (`CreateActivityAction`, `ChangeDealStageAction`, `SendNotificationAction`,
   `CallWebhookAction`, `AssignOwnerAction`) и (опц.) триггеры, **не трогая ядро Workflow**. Это и есть
   механизм, которым Workflow «знает» обо всех модулях, не завися от их кода.
2. **Динамическая — данные без миграций.** Правило (триггер-ключ + JsonLogic-условие + список действий
   с параметрами) — это **данные**. «Новое правило» не требует ни кода, ни миграции.
3. **Структурная (как Activities) — compile-time.** Бизнес-модуль поставляет абстрактный
   `AutomationRuleBase`; наследник дописывает `sealed`-тип со своими полями (владелец процесса, тикет,
   метки команды) + опциональная сборка `.Default` «из коробки».

---

## 1. Назначение и границы

**Что делает:**

- хранит **правила автоматизации**: `Trigger` (событие/расписание) → `Condition` (JsonLogic) →
  `Actions[]` (упорядоченный список действий с параметрами);
- слушает шину событий (firehose), сопоставляет каждое событие с активными правилами по `TriggerKey`,
  проверяет условие над payload, исполняет действия;
- декларативный реестр: модули при старте регистрируют **триггеры** (какие события шлют + схема payload)
  и **действия** (какие операции открывают + параметры) — как Permissions.Catalog/Tags;
- ведёт **журнал срабатываний** `AutomationRun` (когда, по какому событию, какие действия, успех/ошибка);
- **идемпотентность** (одно событие — одно срабатывание правила) через уникальный индекс на журнале +
  опц. `Cheetah.Core.Inbox`;
- **расписание** (cron-триггеры) через `Cheetah.BackgroundTasks` + `Cheetah.DistributedLock`;
- **надёжная доставка** действий через `Cheetah.Core.Outbox`; многошаговые действия с компенсацией —
  через `Cheetah.Saga`.

**Чего НЕ делает:**

- **не заменяет доменные инварианты** — исполняет *декларативные побочные действия*, а не бизнес-правила,
  которые обязаны жить в домене;
- **не доставляет уведомления сам** — действие `SendNotification` дёргает `Notification`-модуль;
- **не источник истины** по сущностям — только реагирует и оркестрирует;
- **не хранит каталог фич** — это `FeatureManagement` (флаг ≠ правило).

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Роль модуля | **реактивный центральный консьюмер + диспетчер действий**, не RPC-сервис (§0/§2) |
| Форма модуля | **абстрактный шаблон** (`AutomationRuleBase`, как Activities) + опц. `.Default` «из коробки» |
| Разделение | **абстракция `src/Cheetah.Workflow`** (plugin-контракты + конверт + порты, без БД) **+ бизнес-модуль** (хранилище/движок/админка) |
| Как Workflow получает чужие события | **firehose-конверт `WorkflowEventEnvelope`** на отдельном канале шины (§2.2) |
| Как Workflow исполняет действия | port `IWorkflowActionExecutor`; стратегии: in-proc `IWorkflowAction` / шина-команда / HTTP (§2.3/§10.5) |
| Точка расширения | plugin `IWorkflowAction` + `IWorkflowTrigger` — главная ось |
| Условия | `Cheetah.Expressions.JsonLogic` (`IExpressionEvaluator`) над payload + атрибутами контекста |
| Идемпотентность | уникальный индекс `(RuleId, EventId)` на `AutomationRun` (журнал = дедуп) + опц. Inbox-декоратор |
| Надёжность действий | `Cheetah.Core.Outbox` (публикация через `OutboxEventBus`); компенсации — `Cheetah.Saga` |
| Расписание | `CronBackgroundTask` + `IDistributedLock` (один исполнитель в кластере) |
| StateMachine | **не используется** (`IsActive` — флаг; `AutomationRun.Status` — журнальный enum) |
| Аудит | журнал `AutomationRun` (встроенный) + опц. `Cheetah.Audit` на изменения правил |
| Идентификатор | `Guid` для агрегатов; `Name` — человекочитаемый, не обязан быть глобально уникальным |
| Мультитенантность | опц. `TenantId?` на правиле (`Cheetah.Core.Tenants`); матчинг учитывает tenant конверта |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Firehose vs типизированные подписки** (§2.2). Решено: **firehose-конверт**. Альтернатива —
   типизированные `Subscribe<TEvent,…>` со ссылкой на `DomainEvents` каждого модуля — fallback для
   монолита, где firehose-shim ещё не включён.
2. **Кто публикует конверт.** Вариант A — декоратор `IEventBus` (`WorkflowEventForwarder`), прозрачно
   перехватывает все `EventBase` и дублирует в firehose-канал. Вариант B — источники сами шлют конверт.
   Рекомендация — **A** (ноль изменений в существующих модулях). Уточнить, что декоратор `IEventBus`
   совместим с уже существующим `OutboxEventBus`-декоратором (порядок декорации в DI).
3. **Шаблонизация параметров действий** (`{{trigger.dealId}}`): свой мини-рендер vs `Cheetah.Expressions`.
   Рекомендация — **гибрид**: простая подстановка `{{path}}` для строк + опционально JsonLogic-выражение
   для вычислимых параметров (§9.4).
4. **`.Default` «из коробки»** — да (как Activities/FM): `sealed AutomationRule` + DbContext + миграция +
   встроенные действия.
5. **Имена контрактов** — `IInboxStore`/`CronBackgroundTask`/`IDistributedLock`/`IExpressionEvaluator`/
   `IOutboxStore` сверены; `IDispatcher.SendAsync` и `Cheetah.Backend.Endpoints`-базы уточнить по факту.

---

## 2. Архитектурная роль

> Как Feature Management: лёгкая **абстракция** (`Cheetah.Workflow`) с plugin-контрактами для всех
> модулей-контрибуторов (без БД) + полноценный **бизнес-модуль** со своей БД и движком. Отличие от FM:
> движок Workflow исполняется **централизованно** (в сервисе Workflow), поэтому он живёт в бизнес-модуле,
> а в абстракции — только контракты расширения, конверт и порты.

```
   Любой модуль                          ┌────────────────────────────────────────────────────┐
   • публикует события  ───firehose────▶ │  src/Modules/Workflow (БИЗНЕС-МОДУЛЬ, своя БД)     │
   • (опц.) контрибутит IWorkflowAction  │  AutomationRuleBase 1──* TriggerBinding            │
        │ зависит на                     │                     1──* RuleAction                │
        ▼                                │  WorkflowEngine: ingest→match→condition→plan→exec  │
   ┌──────────────────────────────────┐  │  Журнал: AutomationRun (дедуп по (RuleId,EventId)) │
   │ src/Cheetah.Workflow (АБСТРАКЦИЯ) │◀─┤  Inbox · Outbox · Saga · Cron · Cache-индекс       │
   │ • IWorkflowAction (plugin)        │  │  Api: админка + registry/sync + test                │
   │ • IWorkflowTrigger (plugin)       │  └───────────────────────┬────────────────────────────┘
   │ • WorkflowEventEnvelope (конверт) │                          │ action
   │ • WorkflowActionContext           │     ┌────────────────────┴───────────────────┐
   │ • IWorkflowActionExecutor (порт)  │     ▼ InProc            ▼ Bus                 ▼ Http
   │ • TriggerDescriptor/ActionDescr.  │  IWorkflowAction   ExecuteActionRequested   шаблонный вызов
   └──────────────────────────────────┘  (монолит)         (микросервис)            REST/gRPC цели
```

### 2.1. Почему так

- **`IWorkflowAction`/`IWorkflowTrigger`** нужны модулям-контрибуторам → в лёгкой сборке без EF. Модуль
  зависит **только на абстракцию** (как на `Events`).
- **Движок, хранилище, админка, журнал** — домен со своей БД → отдельный бизнес-модуль.
- **`WorkflowEventEnvelope`** в абстракции — чтобы и forwarder (источники), и движок (Workflow)
  ссылались на один контракт без взаимных зависимостей.

### 2.2. Вход — firehose-конверт

**Проблема.** Шина (`IEventBus.Subscribe<TEvent, THandler>`) типизирована: канал = `{prefix}{EventType.Name}`,
подписка на конкретный `TEvent`. Чтобы реагировать на `DealWonIntegrationEvent` типизированно, Workflow
должен ссылаться на `Cheetah.Modules.Deals.DomainEvents`. Делать так для **всех** модулей — значит
пересобирать Workflow при каждом новом событии. Для микросервисного Workflow это неприемлемо.

**Решение — firehose.** Единый конверт + один общий канал. Декоратор `IEventBus` прозрачно дублирует
каждое `EventBase` в firehose-канал как `WorkflowEventEnvelope`; Workflow подписан на **один** тип.
`Payload` — плоский разбор полей события, сразу пригоден как контекст JsonLogic. Полный код forwarder'а —
§10.3, конверт — §4.1.

> Аналогия с реальным `OutboxEventBus` (`src/Cheetah.Core.Outbox/OutboxEventBus.cs`): он тоже декоратор
> `IEventBus`, перехватывающий `PublishAsync`. `WorkflowEventForwarder` строится тем же приёмом, поверх
> (или внутри) цепочки декораторов.

### 2.3. Выход — port `IWorkflowActionExecutor`

Действие — **данные**: `{ ActionType, Parameters(json) }`, где `ActionType` — имя из каталога. Исполнение
абстрагировано портом, **три транспорта**:

| Транспорт | Реализация executor | Когда | Как исполняется |
|---|---|---|---|
| **InProc** | `InProcActionExecutor` | монолит | резолвит `IWorkflowAction` по `Name` из DI → `action.ExecuteAsync` → внутри `IDispatcher.Send(command)` |
| **Bus** | `BusActionExecutor` | микросервис | публикует `ExecuteActionRequestedIntegrationEvent` (через `OutboxEventBus`) → целевой сервис подбирает у себя |
| **Http** | `HttpActionExecutor` | микросервис без шины-команд | по `ActionDescriptor` рендерит запрос и зовёт REST/gRPC Client цели |

Движок всегда зовёт `IWorkflowActionExecutor.ExecuteAsync(actionType, ctx, ct)` — от транспорта **не
зависит**. Композитный executor выбирает транспорт по `ActionDescriptor.Transport` (§10.5).

> **Это ответ на «как один сервис исполняет действия в чужих модулях».** Workflow не вызывает методы
> чужого кода — он либо исполняет зарегистрированный in-proc плагин (монолит), либо публикует
> команду-событие «исполни действие X», которую целевой модуль подбирает (микросервис). Outbox делает
> доставку надёжной, Saga — компенсируемой.

### 2.4. Микросервисный режим

- **Вход** развязан firehose-конвертом по шине; источникам про Workflow знать не нужно.
- **Выход** развязан портом: `BusActionExecutor` шлёт команды-события (целевой сервис их подбирает) или
  `HttpActionExecutor` зовёт Client цели по дескриптору.
- **Реестр** наполняется при старте контрибуторов (`registry/sync`).
- **Деградация:** недоступность Workflow не должна ронять источники (они просто публикуют в шину) и
  контрибуторов (`registry/sync` — `ContinueOnFailure`). Пропуски при простое — вопрос гарантий шины
  (persistent stream Kafka / Redis Streams); идемпотентность по `(RuleId, EventId)` защищает от повторов
  at-least-once.

---

## 3. Структура проектов

```
src/Cheetah.Workflow/                              # АБСТРАКЦИЯ (Core + Core.Events + Expressions; без БД)
src/Modules/Workflow/
├── Cheetah.Modules.Workflow.DomainEvents/         # AutomationRuleCreated/Enabled/Disabled, AutomationRun*
├── Cheetah.Modules.Workflow.Shared/               # enums (TriggerType, RunStatus, ActionFailureMode), ключи
├── Cheetah.Modules.Workflow.Contracts/            # ABSTRACT DTO/Request + дескрипторы + ExecuteActionRequested
├── Cheetah.Modules.Workflow.Domain/               # abstract AutomationRuleBase + TriggerBinding/RuleAction + AutomationRun + спеки
├── Cheetah.Modules.Workflow.Infrastructure/       # EF, firehose, Inbox, Outbox-executor, cron, executors
├── Cheetah.Modules.Workflow.Application/           # generic CQRS + WorkflowEngine + parameter renderer
├── Cheetah.Modules.Workflow.Api/                  # abstract EndpointsBase<>, ApiModuleBase
├── (опц.) Cheetah.Modules.Workflow.Default/       # sealed AutomationRule + DbContext + миграции + встроенные действия
├── Cheetah.Modules.Workflow.Client/               # registry/sync + приём ExecuteActionRequested
└── Tests/ Domain.Tests, Application.Tests, Client.Tests
```

**Зависимости проектов (`.csproj` references):**

| Проект | Зависит на |
|---|---|
| `Cheetah.Workflow` (абстракция) | `Cheetah.Core`, `Cheetah.Core.Events`, `Cheetah.Expressions` |
| `…Workflow.DomainEvents` | `Cheetah.Core.Events` |
| `…Workflow.Shared` | `Cheetah.Core` |
| `…Workflow.Contracts` | `Cheetah.Core`, `…Shared`, `Cheetah.Workflow` |
| `…Workflow.Domain` | `…DomainEvents`, `…Shared`, `Cheetah.Core.Domain`, `Cheetah.Core.Specification`, `Cheetah.Workflow` |
| `…Workflow.Infrastructure` | `…Domain`, `…Contracts`, `CrmEntityFrameworkModule`+`…PostgreSql`, `Cheetah.Core.Inbox`, `Cheetah.Core.Outbox`, `Cheetah.BackgroundTasks`, `Cheetah.DistributedLock`, `Cheetah.Core.Cache` |
| `…Workflow.Application` | `…Domain`, `…Contracts`, `Cheetah.Core.CQRS`, `Cheetah.Core.Events`, `Cheetah.Expressions.JsonLogic`, `Cheetah.Saga` |
| `…Workflow.Api` | `…Application`, `…Contracts`, `Cheetah.AspNetCore`, `Cheetah.Backend.Endpoints`, `Cheetah.Permissions` |
| `…Workflow.Default` | `…Api`, `…Infrastructure` (+ встроенные действия), миграции |
| `…Workflow.Client` | `…Contracts`, `Cheetah.Workflow`, `Cheetah.AspNetCore.Contracts` |

> Application зависит **только на Domain** (не на Infrastructure); фильтрация — только через спецификации.
> Абстракция `Cheetah.Workflow` — без EF/БД.

### 3.1. Решение по «готовой реализации» (как в Activities/FM)

Рекомендация — **гибрид**: шаблон + сборка `Cheetah.Modules.Workflow.Default` с рабочей реализацией «из
коробки» (`sealed AutomationRule`, конкретные DTO/Request, `WorkflowDbContext` + миграция, регистрации
Infrastructure/Application/Api, набор встроенных действий §10.6).

---

## 4. Абстракция `Cheetah.Workflow` (полные контракты)

### 4.1. Конверт события

```csharp
namespace Cheetah.Workflow;

using Cheetah.Core.Events;

/// <summary>Нормализованный «конверт» любого интеграционного события для движка правил.</summary>
public sealed record WorkflowEventEnvelope(
    string EventName,                                  // = тип события, напр. "DealWonIntegrationEvent"
    Guid SourceEventId,                                // из EventBase — для идемпотентности
    DateTimeOffset OccurredAt,
    IReadOnlyDictionary<string, object?> Payload,      // плоский разбор полей события (контекст JsonLogic)
    string? SourceService = null,
    Guid? TenantId = null) : EventBase;                // сам конверт — тоже EventBase (едет по шине)
```

### 4.2. Действие — plugin + контекст + порт

```csharp
namespace Cheetah.Workflow;

/// <summary>Plugin-действие. Реализуется модулем-контрибутором (in-proc) ИЛИ синтезируется из дескриптора.</summary>
public interface IWorkflowAction
{
    string Name { get; }                               // = RuleAction.ActionType и ActionDescriptor.Name
    ValueTask ExecuteAsync(WorkflowActionContext context, CancellationToken ct);
}

public sealed record WorkflowActionContext(
    Guid RuleId,
    Guid RunId,
    WorkflowEventEnvelope Trigger,                      // событие, вызвавшее правило
    IReadOnlyDictionary<string, object?> Parameters);  // отрендеренные параметры (из RuleAction.Parameters)

/// <summary>Порт-диспетчер: по ActionType выбирает транспорт/реализацию и исполняет.</summary>
public interface IWorkflowActionExecutor
{
    ValueTask ExecuteAsync(string actionType, WorkflowActionContext context, CancellationToken ct);
}

/// <summary>Режим обработки сбоя действия (зеркало RuleAction.FailureMode).</summary>
public enum WorkflowActionFailureMode { StopRule = 0, ContinueNext = 1, Compensate = 2 }
```

### 4.3. Триггер — plugin (опциональная ось)

```csharp
namespace Cheetah.Workflow;

public interface IWorkflowTrigger
{
    string Name { get; }                               // = TriggerBinding.TriggerType custom-имя
    ValueTask ActivateAsync(WorkflowTriggerSink sink, CancellationToken ct);
}

/// <summary>Канал, в который триггер «толкает» нормализованные конверты движку правил.</summary>
public sealed record WorkflowTriggerSink(Func<WorkflowEventEnvelope, CancellationToken, ValueTask> Push);
```

> Для MVP достаточно встроенных `EventTrigger` (firehose) + `ScheduleTrigger` (cron). `IWorkflowTrigger`
> — точка роста (например, внешний приёмник вебхуков), как пользовательские `IFeatureFilter` в FM.

### 4.4. Дескрипторы реестра

```csharp
namespace Cheetah.Workflow;

public sealed record TriggerDescriptor(
    string EventName, string OwnerService, string Title,
    IReadOnlyList<string> PayloadFields);              // имена полей payload (подсказки условий в UI)

public sealed record ActionDescriptor(
    string Name, string OwnerService, string Title,
    IReadOnlyList<ActionParameterDescriptor> Parameters,
    ActionTransport Transport);                        // InProc | Bus | Http

public sealed record ActionParameterDescriptor(string Key, string Type, bool Required, string? Default = null);

public enum ActionTransport { InProc = 0, Bus = 1, Http = 2 }
```

### 4.5. Регистрация ядра (extension)

```csharp
namespace Cheetah.Workflow;

public static class WorkflowServiceCollectionExtensions
{
    /// <summary>Регистрирует диспетчер действий + встроенные триггеры. Пользовательские IWorkflowAction
    /// подхватываются через [Export(..., typeof(IWorkflowAction))].</summary>
    public static IServiceCollection AddWorkflow(this IServiceCollection services)
    {
        services.TryAddScoped<IWorkflowActionExecutor, CompositeActionExecutor>(); // выбор транспорта (§10.5)
        return services;
    }
}
```

---

## 5. Доменная модель бизнес-модуля (полный код)

### 5.1. Сводка

| Тип | Базовый | Роль |
|---|---|---|
| **`AutomationRuleBase`** | `AggregateRoot<Guid>` + `ICreateAtEntity` + `IUpdatedAtEntity` | правило (агрегат): триггеры + условие + действия |
| **`TriggerBinding`** | `Entity<Guid>` (child) | привязка к триггеру (`TriggerType`, `TriggerKey`, `Parameters`) |
| **`RuleAction`** | `Entity<Guid>` (child) | действие (`Order`, `ActionType`, `Parameters`, `FailureMode`) |
| **`AutomationRun`** | `AggregateRoot<Guid>` + `ICreateAtEntity` | журнал срабатывания (`RuleId`, `EventId`, `Status`, шаги) |
| **`AutomationRunStep`** | `Entity<Guid>` (child of Run) | результат одного действия (`ActionType`, `Status`, `Error?`) |

> Аудит-интерфейсы ядра — **`DateTimeOffset?`** (как в Activities/FM). StateMachine **не** подключается.

### 5.2. `AutomationRuleBase` (полностью)

```csharp
namespace Cheetah.Modules.Workflow.Domain;

using Cheetah.Core.Domain;
using Cheetah.Modules.Workflow.DomainEvents;
using Cheetah.Modules.Workflow.Shared;

public abstract class AutomationRuleBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<TriggerBinding> _triggers = new();
    private readonly List<RuleAction> _actions = new();

    public string Name { get; protected set; } = null!;
    public string OwnerService { get; protected set; } = null!;   // кто зарегистрировал/владеет
    public string? Description { get; protected set; }
    public string? ConditionExpression { get; protected set; }    // JsonLogic над payload; null = всегда true
    public bool IsActive { get; protected set; }                  // выкл по умолчанию
    public Guid? TenantId { get; protected set; }                 // опц. tenant-scope
    public IReadOnlyList<TriggerBinding> Triggers => _triggers;
    public IReadOnlyList<RuleAction> Actions => _actions;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected AutomationRuleBase() { } // EF + наследник

    /// <summary>Инициализация ядра — вызывается из конкретного фабричного метода наследника.</summary>
    protected void InitializeCore(Guid id, string name, string ownerService, string? description, Guid? tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerService);
        Id = id;
        Name = name.Trim();
        OwnerService = ownerService.Trim();
        Description = description;
        TenantId = tenantId;
        IsActive = false;
        AddDomainEvent(new AutomationRuleCreatedIntegrationEvent(Id, Name, OwnerService));
    }

    public virtual void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public virtual void Enable()
    {
        if (IsActive) return;
        EnsureExecutable();                              // нельзя включить пустое правило
        IsActive = true;
        AddDomainEvent(new AutomationRuleEnabledIntegrationEvent(Id));   // → инвалидация кэш-индекса
    }

    public virtual void Disable()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new AutomationRuleDisabledIntegrationEvent(Id));  // → инвалидация кэш-индекса
    }

    public virtual void SetCondition(string? jsonLogic) => ConditionExpression = jsonLogic;

    public virtual void SetTriggers(IEnumerable<TriggerBinding> bindings)
    {
        _triggers.Clear();
        _triggers.AddRange(bindings);
        // изменение набора триггеров меняет индекс event→rule → инвалидация
        AddDomainEvent(new AutomationRuleChangedIntegrationEvent(Id, TriggerKeys()));
    }

    public virtual void SetActions(IEnumerable<RuleAction> actions)
    {
        _actions.Clear();
        _actions.AddRange(actions.OrderBy(a => a.Order));
    }

    /// <summary>Имена событий-триггеров — для построения кэш-индекса и инвалидации.</summary>
    public IReadOnlyList<string> EventTriggerKeys()
        => _triggers.Where(t => t.TriggerType == TriggerType.Event).Select(t => t.TriggerKey).ToArray();

    private IReadOnlyList<string> TriggerKeys() => _triggers.Select(t => t.TriggerKey).ToArray();

    private void EnsureExecutable()
    {
        if (_triggers.Count == 0)
            throw new InvalidOperationException("Rule has no triggers.");
        if (_actions.Count == 0)
            throw new InvalidOperationException("Rule has no actions.");
    }
}
```

### 5.3. `TriggerBinding` и `RuleAction` (полностью)

```csharp
public sealed class TriggerBinding : Entity<Guid>
{
    public Guid RuleId { get; private set; }
    public TriggerType TriggerType { get; private set; }    // Event | Schedule
    public string TriggerKey { get; private set; } = null!; // имя события ИЛИ cron-выражение
    public string? Parameters { get; private set; }          // jsonb (доп. настройки триггера)

    private TriggerBinding() { }

    public static TriggerBinding Event(Guid ruleId, string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        return new TriggerBinding { Id = Guid.NewGuid(), RuleId = ruleId, TriggerType = TriggerType.Event, TriggerKey = eventName };
    }

    public static TriggerBinding Schedule(Guid ruleId, string cron)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cron);
        return new TriggerBinding { Id = Guid.NewGuid(), RuleId = ruleId, TriggerType = TriggerType.Schedule, TriggerKey = cron };
    }
}

public sealed class RuleAction : Entity<Guid>
{
    public Guid RuleId { get; private set; }
    public int Order { get; private set; }
    public string ActionType { get; private set; } = null!;  // = IWorkflowAction.Name / ActionDescriptor.Name
    public string Parameters { get; private set; } = "{}";    // jsonb (шаблоны параметров)
    public ActionFailureMode FailureMode { get; private set; }

    private RuleAction() { }

    public static RuleAction Create(Guid ruleId, int order, string actionType, string parametersJson,
        ActionFailureMode failureMode = ActionFailureMode.StopRule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);
        return new RuleAction
        {
            Id = Guid.NewGuid(), RuleId = ruleId, Order = order,
            ActionType = actionType, Parameters = parametersJson ?? "{}", FailureMode = failureMode
        };
    }
}
```

### 5.4. `AutomationRun` — журнал + дедуп (полностью)

```csharp
namespace Cheetah.Modules.Workflow.Domain;

using Cheetah.Workflow;

public sealed class AutomationRun : AggregateRoot<Guid>, ICreateAtEntity
{
    private readonly List<AutomationRunStep> _steps = new();

    public Guid RuleId { get; private set; }
    public Guid EventId { get; private set; }            // SourceEventId конверта — часть уникального ключа дедупа
    public string EventName { get; private set; } = null!;
    public RunStatus Status { get; private set; }
    public string? PayloadSnapshot { get; private set; } // jsonb — payload триггера (для аудита/повтора)
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyList<AutomationRunStep> Steps => _steps;
    public DateTimeOffset? CreatedAt { get; set; }

    private AutomationRun() { }

    public static AutomationRun Start(Guid ruleId, WorkflowEventEnvelope trigger, string payloadJson)
        => new()
        {
            Id = Guid.NewGuid(), RuleId = ruleId,
            EventId = trigger.SourceEventId, EventName = trigger.EventName,
            Status = RunStatus.Pending, PayloadSnapshot = payloadJson
        };

    public void RecordSuccess(string actionType)
        => _steps.Add(AutomationRunStep.Success(Id, _steps.Count, actionType));

    public void RecordFailure(string actionType, string error)
        => _steps.Add(AutomationRunStep.Failure(Id, _steps.Count, actionType, error));

    public void Complete()
    {
        var anyFailed = _steps.Any(s => s.Status == RunStatus.Failed);
        var anySucceeded = _steps.Any(s => s.Status == RunStatus.Succeeded);
        Status = (anyFailed, anySucceeded) switch
        {
            (true, true)  => RunStatus.PartiallyFailed,
            (true, false) => RunStatus.Failed,
            _             => RunStatus.Succeeded
        };
        CompletedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(Status == RunStatus.Failed
            ? new AutomationRunFailedIntegrationEvent(Id, RuleId, _steps.LastOrDefault()?.Error ?? "unknown")
            : new AutomationRunCompletedIntegrationEvent(Id, RuleId, Status));
    }
}

public sealed class AutomationRunStep : Entity<Guid>
{
    public Guid RunId { get; private set; }
    public int Order { get; private set; }
    public string ActionType { get; private set; } = null!;
    public RunStatus Status { get; private set; }
    public string? Error { get; private set; }

    private AutomationRunStep() { }

    internal static AutomationRunStep Success(Guid runId, int order, string actionType)
        => new() { Id = Guid.NewGuid(), RunId = runId, Order = order, ActionType = actionType, Status = RunStatus.Succeeded };

    internal static AutomationRunStep Failure(Guid runId, int order, string actionType, string error)
        => new() { Id = Guid.NewGuid(), RunId = runId, Order = order, ActionType = actionType, Status = RunStatus.Failed, Error = error };
}
```

> **Дедуп.** Уникальный индекс `(RuleId, EventId)` на `AutomationRun` (§10.1) делает повторный запуск
> одного правила тем же событием невозможным: вторая вставка падает на `DbUpdateException` (unique
> violation) → движок трактует как «уже обработано» и тихо пропускает (§9.1).

---

## 6. Shared — enums и конвенции

```csharp
namespace Cheetah.Modules.Workflow.Shared;

public enum TriggerType        { Event = 0, Schedule = 1 }
public enum RunStatus          { Pending = 0, Succeeded = 1, Failed = 2, PartiallyFailed = 3 }
public enum ActionFailureMode  { StopRule = 0, ContinueNext = 1, Compensate = 2 }

/// <summary>Имена встроенных действий (совпадают с IWorkflowAction.Name и ActionDescriptor.Name).</summary>
public static class BuiltInActions
{
    public const string CreateActivity   = "CreateActivity";
    public const string ChangeDealStage  = "ChangeDealStage";
    public const string SendNotification = "SendNotification";
    public const string CallWebhook      = "CallWebhook";
    public const string AssignOwner      = "AssignOwner";
}

/// <summary>Ключ расписания → синтетическое имя события cron-триггера.</summary>
public static class WorkflowKeys
{
    public static string ScheduleEventName(Guid ruleId) => $"schedule:{ruleId}";
}
```

---

## 7. Contracts — расширяемые ViewModel + дескрипторы

```csharp
namespace Cheetah.Modules.Workflow.Contracts;

using Cheetah.Workflow;
using Cheetah.Modules.Workflow.Shared;

public abstract record AutomationRuleDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string OwnerService { get; init; } = null!;
    public string? Description { get; init; }
    public string? ConditionExpression { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<TriggerBindingDto> Triggers { get; init; } = [];
    public IReadOnlyList<RuleActionDto> Actions { get; init; } = [];
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public abstract record CreateAutomationRuleRequestBase
{
    public string Name { get; init; } = null!;
    public string OwnerService { get; init; } = null!;
    public string? Description { get; init; }
    public string? ConditionExpression { get; init; }
    public IReadOnlyList<TriggerBindingDto> Triggers { get; init; } = [];
    public IReadOnlyList<RuleActionDto> Actions { get; init; } = [];
}

public abstract record UpdateAutomationRuleRequestBase : CreateAutomationRuleRequestBase;

public sealed record TriggerBindingDto(TriggerType TriggerType, string TriggerKey, string? Parameters);
public sealed record RuleActionDto(int Order, string ActionType, string Parameters, ActionFailureMode FailureMode);
public sealed record AutomationRunDto(Guid Id, Guid RuleId, Guid EventId, string EventName,
    RunStatus Status, IReadOnlyList<AutomationRunStepDto> Steps, DateTimeOffset OccurredAt);
public sealed record AutomationRunStepDto(int Order, string ActionType, RunStatus Status, string? Error);

/// <summary>Результат dry-run «теста» правила (TestRuleCommand): сработало ли условие и какой план действий.</summary>
public sealed record TestRunResult(bool ConditionMatched, IReadOnlyList<PlannedActionDto> PlannedActions);
public sealed record PlannedActionDto(int Order, string ActionType, IReadOnlyDictionary<string, object?> RenderedParameters);

/// <summary>Команда-событие для микросервисного executor'а: целевой модуль её подбирает.</summary>
public sealed record ExecuteActionRequestedIntegrationEvent(
    Guid RuleId, Guid RunId, string ActionType,
    IReadOnlyDictionary<string, object?> Parameters,
    WorkflowEventEnvelope Trigger) : EventBase;
```

> Наследник: `public sealed record AutomationRuleDto : AutomationRuleDtoBase { public string? OwnerTeam { get; init; } }`
> — ровно как `ActivityDto : ActivityDtoBase`.

---

## 8. Domain — спецификации

```csharp
namespace Cheetah.Modules.Workflow.Domain;

using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Workflow.Shared;

public sealed class RuleByIdSpecification<TRule>(Guid id)
    : Specification<TRule> where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression() => r => r.Id == id;
}

/// <summary>Горячий путь матчинга: активные правила с Event-триггером на данное имя события.</summary>
public sealed class ActiveRulesByEventSpecification<TRule>(string eventName, Guid? tenantId)
    : Specification<TRule> where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression()
        => r => r.IsActive
             && (r.TenantId == null || r.TenantId == tenantId)
             && r.Triggers.Any(t => t.TriggerType == TriggerType.Event && t.TriggerKey == eventName);
}

public sealed class RulesByOwnerServiceSpecification<TRule>(string ownerService)
    : Specification<TRule> where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression() => r => r.OwnerService == ownerService;
}

public sealed class ScheduledRulesSpecification<TRule>()
    : Specification<TRule> where TRule : AutomationRuleBase
{
    public override Expression<Func<TRule, bool>> ToExpression()
        => r => r.IsActive && r.Triggers.Any(t => t.TriggerType == TriggerType.Schedule);
}
```

> Матчинг event→rule в горячем пути держим в **кэше** (`ICacheService`): индекс `eventName → ruleIds`,
> инвалидируемый при изменении правила (как кэш определений в FM §8.2). БД-запрос по
> `ActiveRulesByEventSpecification` — только на miss/refresh. См. `IRuleMatcher` (§10.4).

---

## 9. Application — движок + CQRS (полный код)

### 9.1. Движок правил (ядро модуля)

```csharp
namespace Cheetah.Modules.Workflow.Application;

using Cheetah.Core.DependencyInjection;
using Cheetah.Core.DataAccess;
using Cheetah.Expressions;          // IExpressionEvaluator
using Cheetah.Workflow;
using Cheetah.Modules.Workflow.Domain;
using Cheetah.Modules.Workflow.Shared;

public interface IWorkflowEngine
{
    ValueTask HandleTriggerAsync(WorkflowEventEnvelope envelope, CancellationToken ct);
}

[Export(LifetimeType.Scoped, typeof(IWorkflowEngine))]
public sealed class WorkflowEngine<TRule> : IWorkflowEngine where TRule : AutomationRuleBase
{
    private readonly IRuleMatcher<TRule> _matcher;        // event → активные правила (через кэш-индекс §10.4)
    private readonly IExpressionEvaluator _expressions;   // Cheetah.Expressions.JsonLogic
    private readonly IWorkflowActionExecutor _executor;   // §2.3
    private readonly IParameterRenderer _renderer;        // §9.4
    private readonly IRepository<AutomationRun, Guid> _runs;
    private readonly ILogger<WorkflowEngine<TRule>> _logger;

    public WorkflowEngine(IRuleMatcher<TRule> matcher, IExpressionEvaluator expressions,
        IWorkflowActionExecutor executor, IParameterRenderer renderer,
        IRepository<AutomationRun, Guid> runs, ILogger<WorkflowEngine<TRule>> logger)
        => (_matcher, _expressions, _executor, _renderer, _runs, _logger)
         = (matcher, expressions, executor, renderer, runs, logger);

    public async ValueTask HandleTriggerAsync(WorkflowEventEnvelope envelope, CancellationToken ct)
    {
        var rules = await _matcher.MatchAsync(envelope.EventName, envelope.TenantId, ct);
        if (rules.Count == 0) return;

        foreach (var rule in rules)
        {
            // 1. Условие (JsonLogic над payload). null condition → true.
            if (rule.ConditionExpression is { Length: > 0 } expr &&
                !await _expressions.EvaluateBooleanAsync(expr, envelope.Payload, ct))
                continue;

            // 2. Журнал срабатывания + дедуп. Уникальный индекс (RuleId, EventId) ловит повтор.
            var run = AutomationRun.Start(rule.Id, envelope, Serialize(envelope.Payload));
            _runs.Add(run);
            try
            {
                await _runs.SaveChangesAsync(ct);
            }
            catch (Exception ex) when (IsUniqueViolation(ex))
            {
                _logger.LogDebug("Rule {RuleId} already ran for event {EventId} — skipping (dedup).",
                    rule.Id, envelope.SourceEventId);
                continue;                                 // идемпотентность: правило уже отработало это событие
            }

            // 3. Действия по Order. Параметры рендерятся из payload.
            foreach (var action in rule.Actions)
            {
                try
                {
                    var prms = _renderer.Render(action.Parameters, envelope.Payload);
                    await _executor.ExecuteAsync(action.ActionType,
                        new WorkflowActionContext(rule.Id, run.Id, envelope, prms), ct);
                    run.RecordSuccess(action.ActionType);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Action {Action} of rule {RuleId} failed.", action.ActionType, rule.Id);
                    run.RecordFailure(action.ActionType, ex.Message);
                    if (action.FailureMode == ActionFailureMode.StopRule) break;
                    if (action.FailureMode == ActionFailureMode.Compensate) { /* → Cheetah.Saga, §10.7 */ }
                    // ContinueNext → продолжаем со следующего действия
                }
            }

            // 4. Завершение журнала + публикация AutomationRun*-события (через Outbox).
            run.Complete();
            await _runs.SaveChangesAsync(ct);
            foreach (var e in run.DomainEvents) await PublishAsync(e, ct);
            run.ClearDomainEvents();
        }
    }

    private static string Serialize(IReadOnlyDictionary<string, object?> payload)
        => System.Text.Json.JsonSerializer.Serialize(payload);
    // IsUniqueViolation / PublishAsync — детали Infrastructure (Npgsql 23505; IEventBus через Outbox).
}
```

> **Почему дедуп — через БД, а не Inbox.** Уникальный индекс `(RuleId, EventId)` самодостаточен: журнал
> одновременно и аудит, и дедуп. `Cheetah.Core.Inbox`-декоратор (§10.3) — дополнительная защита на
> уровне подписки (если один и тот же конверт доставлен дважды до движка). Оба слоя совместимы.

### 9.2. Фабрика и проектор (нельзя `new` абстракцию)

```csharp
public interface IAutomationRuleFactory<TRule, TCreateRequest> where TRule : AutomationRuleBase
{
    TRule Create(TCreateRequest request);
}

public interface IAutomationRuleProjector<TRule, TDto>
    where TRule : AutomationRuleBase where TDto : AutomationRuleDtoBase
{
    TDto ToDto(TRule rule);
}
```

### 9.3. CQRS — команды/запросы + ключевые хендлеры

```csharp
// --- Commands ---
public sealed record CreateAutomationRuleCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>;
public sealed record UpdateAutomationRuleCommand<TUpdateRequest>(Guid RuleId, TUpdateRequest Request) : ICommand;
public sealed record EnableRuleCommand(Guid RuleId) : ICommand;
public sealed record DisableRuleCommand(Guid RuleId) : ICommand;
public sealed record DeleteRuleCommand(Guid RuleId) : ICommand;
public sealed record SyncWorkflowRegistryCommand(
    IReadOnlyList<TriggerDescriptor> Triggers, IReadOnlyList<ActionDescriptor> Actions) : ICommand;
public sealed record TestRuleCommand(Guid RuleId, WorkflowEventEnvelope SampleEvent) : ICommand<TestRunResult>;

// --- Queries ---
public sealed record GetRuleByIdQuery<TDto>(Guid RuleId) : IQuery<TDto?>;
public sealed record ListRulesQuery<TDto>(string? OwnerService, bool? OnlyActive, int Page, int Size)
    : IQuery<IReadOnlyList<TDto>>;
public sealed record ListRunsQuery(Guid? RuleId, RunStatus? Status, int Page, int Size)
    : IQuery<IReadOnlyList<AutomationRunDto>>;
```

Хендлер создания (канон `CLAUDE.md` — фабрика → репозиторий → SaveChanges → события):

```csharp
[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateAutomationRuleCommand<CreateAutomationRuleRequest>, Guid>))]
public sealed class CreateAutomationRuleCommandHandler<TRule, TCreateRequest>
    : ICommandHandler<CreateAutomationRuleCommand<TCreateRequest>, Guid>
    where TRule : AutomationRuleBase
    where TCreateRequest : CreateAutomationRuleRequestBase
{
    private readonly IAutomationRuleFactory<TRule, TCreateRequest> _factory;
    private readonly IRepository<TRule, Guid> _rules;
    private readonly IEventBus _eventBus;

    public CreateAutomationRuleCommandHandler(IAutomationRuleFactory<TRule, TCreateRequest> factory,
        IRepository<TRule, Guid> rules, IEventBus eventBus)
        => (_factory, _rules, _eventBus) = (factory, rules, eventBus);

    public async ValueTask<Guid> HandleAsync(CreateAutomationRuleCommand<TCreateRequest> cmd, CancellationToken ct)
    {
        var rule = _factory.Create(cmd.Request);        // InitializeCore + SetTriggers/SetActions/SetCondition
        _rules.Add(rule);
        await _rules.SaveChangesAsync(ct);
        foreach (var e in rule.DomainEvents) await _eventBus.PublishAsync(e, ct);
        rule.ClearDomainEvents();
        return rule.Id;
    }
}
```

Хендлер `SyncWorkflowRegistry` (идемпотентный upsert каталога; **не трогает правила**):

```csharp
[Export(LifetimeType.Scoped, typeof(ICommandHandler<SyncWorkflowRegistryCommand>))]
public sealed class SyncWorkflowRegistryCommandHandler : ICommandHandler<SyncWorkflowRegistryCommand>
{
    private readonly ITriggerCatalogRepository _triggers;   // отдельная таблица каталога (не правила!)
    private readonly IActionCatalogRepository _actions;

    public async ValueTask HandleAsync(SyncWorkflowRegistryCommand cmd, CancellationToken ct)
    {
        foreach (var t in cmd.Triggers) await _triggers.UpsertAsync(t, ct);  // по (EventName)
        foreach (var a in cmd.Actions)  await _actions.UpsertAsync(a, ct);   // по (Name)
        await _triggers.SaveChangesAsync(ct);
    }
}
```

Хендлер `TestRule` (dry-run — условие + план, **без исполнения**):

```csharp
[Export(LifetimeType.Scoped, typeof(ICommandHandler<TestRuleCommand, TestRunResult>))]
public sealed class TestRuleCommandHandler<TRule> : ICommandHandler<TestRuleCommand, TestRunResult>
    where TRule : AutomationRuleBase
{
    private readonly IRepository<TRule, Guid> _rules;
    private readonly IExpressionEvaluator _expressions;
    private readonly IParameterRenderer _renderer;

    public async ValueTask<TestRunResult> HandleAsync(TestRuleCommand cmd, CancellationToken ct)
    {
        var rule = await _rules.GetBySpecAsync(new RuleByIdSpecification<TRule>(cmd.RuleId), ct)
            ?? throw new NotFoundException(nameof(AutomationRuleBase), cmd.RuleId);

        var matched = rule.ConditionExpression is not { Length: > 0 } expr
            || await _expressions.EvaluateBooleanAsync(expr, cmd.SampleEvent.Payload, ct);

        var planned = matched
            ? rule.Actions.Select(a => new PlannedActionDto(a.Order, a.ActionType,
                  _renderer.Render(a.Parameters, cmd.SampleEvent.Payload))).ToArray()
            : [];

        return new TestRunResult(matched, planned);
    }
}
```

Регистрация (открытые generic — через extension, не `[Export]`):

```csharp
services.AddWorkflowApplication<AutomationRule, CreateAutomationRuleRequest,
    AutomationRuleDto, RuleFactory, RuleProjector>();
```

### 9.4. `IParameterRenderer` — подстановка параметров действий

```csharp
public interface IParameterRenderer
{
    /// <summary>Рендерит JSON-шаблон параметров действия, подставляя значения из payload триггера.</summary>
    IReadOnlyDictionary<string, object?> Render(string parametersTemplateJson,
        IReadOnlyDictionary<string, object?> payload);
}

// Реализация (Infrastructure): {{trigger.dealId}} → payload["dealId"]; опц. JsonLogic для вычислимых.
// Поддерживает плоские пути и литералы. Неизвестная переменная → null + предупреждение в лог.
```

Пример шаблона параметров действия `CreateActivity`:

```json
{
  "title": "Позвонить по выигранной сделке {{trigger.DealId}}",
  "assigneeId": "{{trigger.OwnerId}}",
  "entityType": "crm.deal",
  "entityId": "{{trigger.DealId}}",
  "dueInHours": 24
}
```

---

## 10. Infrastructure — полный код

### 10.1. EF Core — конфигурации (абстрактные базы, расширяемая схема)

```csharp
namespace Cheetah.Modules.Workflow.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cheetah.Modules.Workflow.Domain;

public abstract class AutomationRuleConfigurationBase<TRule> : IEntityTypeConfiguration<TRule>
    where TRule : AutomationRuleBase
{
    public void Configure(EntityTypeBuilder<TRule> b)
    {
        b.ToTable("AutomationRules", "workflow");
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasMaxLength(300).IsRequired();
        b.Property(r => r.OwnerService).HasMaxLength(100).IsRequired();
        b.Property(r => r.Description).HasMaxLength(2000);
        b.Property(r => r.ConditionExpression).HasColumnType("jsonb");

        b.HasMany(r => r.Triggers).WithOne().HasForeignKey(t => t.RuleId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(r => r.Actions).WithOne().HasForeignKey(a => a.RuleId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(r => r.OwnerService);
        b.HasIndex(r => r.IsActive);

        b.Ignore(r => r.DomainEvents);   // CRITICAL
        ConfigureCustom(b);              // hook наследника: индексы/колонки доп. полей
    }

    protected virtual void ConfigureCustom(EntityTypeBuilder<TRule> b) { }
}

public sealed class TriggerBindingConfiguration : IEntityTypeConfiguration<TriggerBinding>
{
    public void Configure(EntityTypeBuilder<TriggerBinding> b)
    {
        b.ToTable("TriggerBindings", "workflow");
        b.Property(t => t.TriggerType).HasConversion<int>();
        b.Property(t => t.TriggerKey).HasMaxLength(200).IsRequired();
        b.Property(t => t.Parameters).HasColumnType("jsonb");
        // ключевой индекс матчинга: «дай правила по имени события»
        b.HasIndex(t => new { t.TriggerType, t.TriggerKey });
    }
}

public sealed class RuleActionConfiguration : IEntityTypeConfiguration<RuleAction>
{
    public void Configure(EntityTypeBuilder<RuleAction> b)
    {
        b.ToTable("RuleActions", "workflow");
        b.Property(a => a.ActionType).HasMaxLength(100).IsRequired();
        b.Property(a => a.Parameters).HasColumnType("jsonb").IsRequired();
        b.Property(a => a.FailureMode).HasConversion<int>();
        b.HasIndex(a => new { a.RuleId, a.Order });
    }
}

public sealed class AutomationRunConfiguration : IEntityTypeConfiguration<AutomationRun>
{
    public void Configure(EntityTypeBuilder<AutomationRun> b)
    {
        b.ToTable("AutomationRuns", "workflow");
        b.Property(r => r.EventName).HasMaxLength(200).IsRequired();
        b.Property(r => r.Status).HasConversion<int>();
        b.Property(r => r.PayloadSnapshot).HasColumnType("jsonb");
        b.HasMany(r => r.Steps).WithOne().HasForeignKey(s => s.RunId).OnDelete(DeleteBehavior.Cascade);

        // ДЕДУП + горячий аудит:
        b.HasIndex(r => new { r.RuleId, r.EventId }).IsUnique();   // одно событие → один запуск правила
        b.HasIndex(r => new { r.RuleId, r.CreatedAt });
        b.HasIndex(r => r.Status);

        b.Ignore(r => r.DomainEvents);
    }
}

public sealed class AutomationRunStepConfiguration : IEntityTypeConfiguration<AutomationRunStep>
{
    public void Configure(EntityTypeBuilder<AutomationRunStep> b)
    {
        b.ToTable("AutomationRunSteps", "workflow");
        b.Property(s => s.ActionType).HasMaxLength(100).IsRequired();
        b.Property(s => s.Status).HasConversion<int>();
        b.Property(s => s.Error).HasMaxLength(4000);
    }
}
```

`WorkflowDbContextBase<TContext, TRule>` — по образцу `ActivitiesDbContextBase` (абстрактный DbContext
**нельзя мигрировать** → конкретный + `IDesignTimeDbContextFactory` + миграции у наследника / в `.Default`).

```csharp
public abstract class WorkflowDbContextBase<TContext, TRule> : DbContext
    where TContext : DbContext where TRule : AutomationRuleBase
{
    protected WorkflowDbContextBase(DbContextOptions<TContext> options) : base(options) { }

    public DbSet<TRule> Rules => Set<TRule>();
    public DbSet<AutomationRun> Runs => Set<AutomationRun>();
    public DbSet<TriggerCatalogEntry> TriggerCatalog => Set<TriggerCatalogEntry>();  // реестр §10.8
    public DbSet<ActionCatalogEntry> ActionCatalog => Set<ActionCatalogEntry>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfiguration(new TriggerBindingConfiguration());
        mb.ApplyConfiguration(new RuleActionConfiguration());
        mb.ApplyConfiguration(new AutomationRunConfiguration());
        mb.ApplyConfiguration(new AutomationRunStepConfiguration();
        // конфиг конкретного TRule (AutomationRuleConfigurationBase<TRule>) — добавляет наследник/.Default
        ConfigureRule(mb);
    }

    protected abstract void ConfigureRule(ModelBuilder mb);
}
```

### 10.2. `IRuleMatcher` — кэш-индекс event → rules

```csharp
public interface IRuleMatcher<TRule> where TRule : AutomationRuleBase
{
    ValueTask<IReadOnlyList<TRule>> MatchAsync(string eventName, Guid? tenantId, CancellationToken ct);
}

[Export(LifetimeType.Scoped, typeof(IRuleMatcher<>))]
public sealed class CachedRuleMatcher<TRule> : IRuleMatcher<TRule> where TRule : AutomationRuleBase
{
    private readonly IRepository<TRule, Guid> _rules;
    private readonly ICacheService _cache;   // Cheetah.Core.Cache

    public async ValueTask<IReadOnlyList<TRule>> MatchAsync(string eventName, Guid? tenantId, CancellationToken ct)
    {
        // Кэшируем ИНДЕКС ruleIds по eventName (а не сами сущности — их грузим с детьми из БД/трекинга).
        var ids = await _cache.GetOrSetAsync($"wf:idx:{eventName}",
            async () => (await _rules.GetAllAsync(new ActiveRulesByEventSpecification<TRule>(eventName, tenantId), ct))
                        .Select(r => r.Id).ToArray(),
            expiry: TimeSpan.FromMinutes(10), ct: ct);

        if (ids.Length == 0) return [];
        // Загружаем активные правила с детьми (Include Triggers+Actions) — для исполнения.
        return await _rules.GetAllAsync(new RulesByIdsSpecification<TRule>(ids).WithIncludes(), ct);
    }
}
```

> Инвалидация индекса — подписка на `AutomationRuleChanged/Enabled/Disabled` → `_cache.RemoveAsync("wf:idx:*")`
> на всех инстансах (через шину). Eventually consistent — для автоматизаций допустимо.

### 10.3. Firehose — forwarder (источники) + подписчик (Workflow) + идемпотентность

```csharp
// Источники: декоратор IEventBus, прозрачно дублирующий каждое EventBase в firehose-канал.
// Регистрируется как декоратор поверх (или рядом с) OutboxEventBus — порядок уточнить в DI (§1.2-2).
public sealed class WorkflowEventForwarder : IEventBus
{
    private readonly IEventBus _inner;

    public WorkflowEventForwarder(IEventBus inner) => _inner = inner;

    public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IEvent
    {
        await _inner.PublishAsync(@event, ct);                         // обычная доставка
        if (@event is EventBase eb && @event is not WorkflowEventEnvelope)
            await _inner.PublishAsync(ToEnvelope(eb), ct);             // + firehose-копия
    }

    public ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken ct = default)
        where TEvent : IEvent => _inner.PublishManyAsync(events, ct);  // (+ envelope в цикле — аналогично)

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent where THandler : IEventHandler<TEvent> => _inner.Subscribe<TEvent, THandler>();

    private static WorkflowEventEnvelope ToEnvelope(EventBase e)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(e, e.GetType());
        var payload = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, object?>>(json) ?? new();
        return new WorkflowEventEnvelope(e.GetType().Name, e.EventId, e.OccurredAt, payload);
    }
}
```

```csharp
// Workflow: один подписчик на весь поток.
[Export(LifetimeType.Scoped, typeof(IEventHandler<WorkflowEventEnvelope>))]
public sealed class WorkflowEnvelopeHandler : IEventHandler<WorkflowEventEnvelope>
{
    private readonly IWorkflowEngine _engine;
    public WorkflowEnvelopeHandler(IWorkflowEngine engine) => _engine = engine;
    public ValueTask HandleAsync(WorkflowEventEnvelope e, CancellationToken ct) => _engine.HandleTriggerAsync(e, ct);
}
// регистрация (OnApplicationInitialization): eventBus.Subscribe<WorkflowEventEnvelope, WorkflowEnvelopeHandler>();
```

Опциональная идемпотентность на уровне подписки — через `Cheetah.Core.Inbox` (`InboxIdempotentEventHandler`,
`src/Cheetah.Core.Inbox/InboxIdempotentEventHandler.cs`): обернуть `WorkflowEnvelopeHandler`, consumer-name
= `"WorkflowEnvelopeHandler"`, ключ — `WorkflowEventEnvelope.EventId`. Защищает от повторной доставки
**одного конверта**; дедуп на уровне правил — уникальный индекс `(RuleId, EventId)` (§10.1).

### 10.4. Композитный executor + транспорты

```csharp
// Выбор транспорта по ActionDescriptor.Transport (из реестра).
[Export(LifetimeType.Scoped, typeof(IWorkflowActionExecutor))]
public sealed class CompositeActionExecutor : IWorkflowActionExecutor
{
    private readonly IActionCatalogRepository _catalog;
    private readonly InProcActionExecutor _inproc;
    private readonly BusActionExecutor _bus;
    private readonly HttpActionExecutor _http;

    public async ValueTask ExecuteAsync(string actionType, WorkflowActionContext ctx, CancellationToken ct)
    {
        var descriptor = await _catalog.GetAsync(actionType, ct)
            ?? throw new InvalidOperationException($"Action '{actionType}' is not registered.");
        IWorkflowActionExecutor target = descriptor.Transport switch
        {
            ActionTransport.InProc => _inproc,
            ActionTransport.Bus    => _bus,
            ActionTransport.Http   => _http,
            _ => throw new InvalidOperationException($"Unknown transport {descriptor.Transport}.")
        };
        await target.ExecuteAsync(actionType, ctx, ct);
    }
}
```

```csharp
// Монолит: диспетчер по Name среди зарегистрированных IWorkflowAction.
public sealed class InProcActionExecutor : IWorkflowActionExecutor
{
    private readonly IReadOnlyDictionary<string, IWorkflowAction> _actions;
    public InProcActionExecutor(IEnumerable<IWorkflowAction> actions)
        => _actions = actions.ToDictionary(a => a.Name, StringComparer.Ordinal);
    public ValueTask ExecuteAsync(string type, WorkflowActionContext ctx, CancellationToken ct)
        => _actions.TryGetValue(type, out var a)
            ? a.ExecuteAsync(ctx, ct)
            : throw new InvalidOperationException($"No in-proc action '{type}'.");
}

// Микросервис: публикует команду-событие (через OutboxEventBus → надёжно), целевой сервис подбирает.
public sealed class BusActionExecutor : IWorkflowActionExecutor
{
    private readonly IEventBus _bus;   // в DI это OutboxEventBus — запись в той же транзакции, что и Run
    public BusActionExecutor(IEventBus bus) => _bus = bus;
    public ValueTask ExecuteAsync(string type, WorkflowActionContext ctx, CancellationToken ct)
        => _bus.PublishAsync(new ExecuteActionRequestedIntegrationEvent(
               ctx.RuleId, ctx.RunId, type, ctx.Parameters, ctx.Trigger), ct);
}

// Микросервис без шины-команд: шаблонный HTTP/gRPC вызов цели по дескриптору (url/method/body).
public sealed class HttpActionExecutor : IWorkflowActionExecutor
{
    private readonly IHttpClientFactory _factory;
    private readonly IActionCatalogRepository _catalog;
    public async ValueTask ExecuteAsync(string type, WorkflowActionContext ctx, CancellationToken ct)
    {
        // descriptor содержит шаблон endpoint+body; рендерим из ctx.Parameters/ctx.Trigger и шлём.
        // (детали шаблона endpoint — follow-up; для MVP основной микросервисный путь — BusActionExecutor.)
    }
}
```

### 10.5. Встроенные действия (`.Default` / контрибуторы)

```csharp
[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]
public sealed class CreateActivityAction : IWorkflowAction
{
    private readonly IDispatcher _dispatcher;   // монолит: прямая команда Activities
    public CreateActivityAction(IDispatcher dispatcher) => _dispatcher = dispatcher;
    public string Name => BuiltInActions.CreateActivity;

    public async ValueTask ExecuteAsync(WorkflowActionContext ctx, CancellationToken ct)
    {
        var p = ctx.Parameters;
        await _dispatcher.SendAsync(new CreateActivityCommand(
            Type: ActivityType.Task,
            Title: (string)p["title"]!,
            AssigneeId: Guid.Parse((string)p["assigneeId"]!),
            OwnerId: Guid.Parse((string)p["assigneeId"]!),
            EntityType: (string)p["entityType"]!,
            EntityId: Guid.Parse((string)p["entityId"]!),
            DueAt: p.TryGetValue("dueInHours", out var h) && h is not null
                   ? DateTime.UtcNow.AddHours(Convert.ToDouble(h)) : null,
            Priority: ActivityPriority.Normal, Description: null), ct);
    }
}
// Аналогично: ChangeDealStageAction, SendNotificationAction, CallWebhookAction, AssignOwnerAction.
```

### 10.6. Cron-планировщик (расписание)

```csharp
[Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
public sealed class ScheduleTriggerTask<TRule> : CronBackgroundTask where TRule : AutomationRuleBase
{
    private readonly IServiceProvider _sp;
    private readonly IDistributedLock _lock;

    public override string Name => "workflow.schedule-trigger";
    public override string CronExpression => "* * * * *";   // ежеминутно сверяем due-правила

    public override async Task ExecuteAsync(CancellationToken ct)
    {
        await using var handle = await _lock.AcquireAsync("workflow:schedule", ct);
        if (handle is null) return;                          // лок держит другой инстанс кластера

        using var scope = _sp.CreateScope();
        var rules = scope.ServiceProvider.GetRequiredService<IRepository<TRule, Guid>>();
        var engine = scope.ServiceProvider.GetRequiredService<IWorkflowEngine>();

        var scheduled = await rules.GetAllAsync(new ScheduledRulesSpecification<TRule>(), ct);
        var now = DateTimeOffset.UtcNow;
        foreach (var rule in scheduled)
            foreach (var trigger in rule.Triggers.Where(t => t.TriggerType == TriggerType.Schedule))
                if (CronDue(trigger.TriggerKey, now))        // Cronos: следующий запуск попал в текущую минуту
                {
                    var envelope = new WorkflowEventEnvelope(
                        WorkflowKeys.ScheduleEventName(rule.Id), Guid.NewGuid(), now,
                        new Dictionary<string, object?> { ["ruleId"] = rule.Id, ["firedAt"] = now });
                    await engine.HandleTriggerAsync(envelope, ct);
                }
    }
}
```

> Для cron-триггеров дедуп по `(RuleId, EventId)` не нужен (каждый запуск — новый `EventId`); защита от
> двойного запуска в кластере — `IDistributedLock`. Точность «попадания в минуту» считаем через `Cronos`
> (как в `CronBackgroundTask`).

### 10.7. Компенсации — Saga (для `FailureMode = Compensate`)

```csharp
// Многошаговое действие с откатом оборачиваем в Cheetah.Saga (Saga<TData> + [SagaStartedBy]).
public sealed class CompensatingActionSagaData
{
    public Guid RunId { get; set; }
    public List<string> CompletedActions { get; set; } = new();
}

[SagaStartedBy(typeof(ExecuteActionRequestedIntegrationEvent))]
public sealed class CompensatingActionSaga : Saga<CompensatingActionSagaData>
{
    public override string GetCorrelation(IEvent e)
        => e switch { ExecuteActionRequestedIntegrationEvent x => x.RunId.ToString(), _ => "" };

    public ValueTask On(ExecuteActionRequestedIntegrationEvent e, CancellationToken ct)
    {
        // выполнить действие; при сбое — Compensate(reason) → откат CompletedActions в обратном порядке
        return ValueTask.CompletedTask;
    }
}
```

> Saga — follow-up для MVP: достаточно `FailureMode = StopRule|ContinueNext`. `Compensate` включается,
> когда появятся действия с реальными побочными эффектами, требующими отката.

### 10.8. Реестр-каталог (отдельные таблицы, не правила)

`TriggerCatalogEntry`/`ActionCatalogEntry` — простые сущности (`EventName`/`Name` уникальны), наполняются
`SyncWorkflowRegistryCommand`. Используются: (1) админкой/UI для построения правил; (2)
`CompositeActionExecutor` для выбора транспорта (`ActionTransport`). Хранятся в той же БД `workflow`.

### 10.9. Регистрация инфраструктуры

```csharp
public static IServiceCollection AddWorkflowInfrastructure<TContext, TRule>(
    this IServiceCollection services, string connectionString)
    where TContext : WorkflowDbContextBase<TContext, TRule> where TRule : AutomationRuleBase
{
    services.AddDbContext<TContext>(o => o.UseNpgsql(connectionString));
    // IRepository<TRule>, IRepository<AutomationRun>, IRuleMatcher<TRule>, каталог-репозитории,
    // InProc/Bus/Http/Composite executors, ScheduleTriggerTask<TRule>, инвалидатор кэша.
    return services;
}
```

---

## 11. Api — декларативные эндпоинты + реестр

Эндпоинты — наследники базовых из `Cheetah.Backend.Endpoints` с `virtual`-методами (как Activities/FM);
`partial` модуль-классы для Source Generators; маппинг в `OnApplicationInitialization` (по образцу
`CheetahFeatureManagementApiModuleBase`, `src/Modules/FeatureManagement/.../CheetahFeatureManagementApiModuleBase.cs`).

```csharp
[DependsOn(typeof(CoreModule), typeof(CrmCQRSCoreModule), typeof(CrmAspNetCoreModule),
          typeof(CrmWorkflowModule), typeof(CheetahWorkflowApplicationModule),
          typeof(CheetahWorkflowContractsModule))]
public abstract class CheetahWorkflowApiModuleBase<TEndpoints, TCreateRequest, TDto> : CrmModule
    where TEndpoints : AutomationRuleEndpointsBase<TCreateRequest, TDto>, new()
    where TCreateRequest : CreateAutomationRuleRequestBase
    where TDto : AutomationRuleDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new TEndpoints().Map(context.GetRouteBuilder());
        // firehose-подписка движка:
        var bus = context.ServiceProvider.GetRequiredService<IEventBus>();
        bus.Subscribe<WorkflowEventEnvelope, WorkflowEnvelopeHandler>();
    }
}
```

**Админка правил** (за `Cheetah.Permissions`):

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| GET/POST | `/api/automation/rules`, `/api/automation/rules/{id}` | CRUD / `GetRuleByIdQuery` |
| PUT | `/api/automation/rules/{id}` | `UpdateAutomationRuleCommand` |
| POST | `/api/automation/rules/{id}/enable` · `/disable` | `Enable`/`DisableRuleCommand` |
| DELETE | `/api/automation/rules/{id}` | `DeleteRuleCommand` |
| POST | `/api/automation/rules/{id}/test` | `TestRuleCommand` (dry-run, `{ sampleEvent }`) |
| GET | `/api/automation/runs?ruleId=&status=&page=&size=` | `ListRunsQuery` (журнал срабатываний) |

**Реестр** (для сервисов-контрибуторов):

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `/api/automation/registry/sync` | `SyncWorkflowRegistryCommand` (батч триггеров+действий) |
| GET | `/api/automation/registry/triggers` · `/actions` | каталог для UI |

> gRPC дублирует `registry/sync` и приём действий — горячий межсервисный путь (follow-up, как у
> Deals/Calendar/FM).

---

## 12. Client + регистрация контрибутора

```csharp
namespace Cheetah.Modules.Workflow.Client;

public interface IWorkflowCatalogClient
{
    ValueTask SyncAsync(IReadOnlyList<TriggerDescriptor> triggers,
        IReadOnlyList<ActionDescriptor> actions, CancellationToken ct = default);
}

[Export(LifetimeType.Scoped, typeof(IWorkflowCatalogClient))]
public sealed class HttpWorkflowCatalogClient : IWorkflowCatalogClient
{
    private readonly HttpClient _http;
    public HttpWorkflowCatalogClient(IHttpClientFactory f) => _http = f.CreateClient("CheetahAPI");

    public async ValueTask SyncAsync(IReadOnlyList<TriggerDescriptor> triggers,
        IReadOnlyList<ActionDescriptor> actions, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("/api/automation/registry/sync",
            new { triggers, actions }, ct);
        resp.EnsureSuccessStatusCode();
    }
}
```

Hosted-сервис регистрации при старте — по образцу `FeatureClientHostedService`
(`src/Modules/FeatureManagement/.../FeatureClientHostedService.cs`), **`ContinueOnFailure`**:

```csharp
public sealed class WorkflowRegistrationSyncService : IHostedService
{
    private readonly IEnumerable<WorkflowRegistrationContribution> _contributions;
    private readonly IWorkflowCatalogClient _client;
    private readonly ILogger<WorkflowRegistrationSyncService> _logger;

    public async Task StartAsync(CancellationToken ct)
    {
        var triggers = _contributions.SelectMany(c => c.Triggers).ToArray();
        var actions  = _contributions.SelectMany(c => c.Actions).ToArray();
        if (triggers.Length == 0 && actions.Length == 0) return;
        try
        {
            await _client.SyncAsync(triggers, actions, ct);
            _logger.LogInformation("Registered {T} triggers and {A} actions in Workflow catalog.",
                triggers.Length, actions.Length);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Workflow registry sync failed; rules referencing them stay inert until catalog is up.");
        }
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

Подключение в сервисе-контрибуторе (например, Activities):

```csharp
services.AddWorkflow()                              // абстракция: IWorkflowActionExecutor + плагины
        .AddWorkflowCatalogClient(o => o.BaseUrl = cfg["Workflow:Url"])
        .RegisterTriggers(reg => reg.Add("ActivityCompletedIntegrationEvent", "Activities",
                                         "Активность завершена", payload: ["ActivityId", "CompletedBy"]))
        .RegisterActions(reg => reg.Add("CreateActivity", "Activities", "Создать активность",
                                        ActionTransport.InProc,
                                        p => p.Param("title", "string", required: true)
                                              .Param("assigneeId", "guid", required: true)
                                              .Param("entityType", "string", required: true)
                                              .Param("entityId", "guid", required: true)));
```

**Микросервисный приём действий.** В микросервисе-цели `Client`/модуль подписывается на
`ExecuteActionRequestedIntegrationEvent` и маршрутизирует на свой `IWorkflowAction`/команду по
`ActionType` — так Workflow-сервис исполняет действия, не завися от кода цели:

```csharp
[Export(LifetimeType.Scoped, typeof(IEventHandler<ExecuteActionRequestedIntegrationEvent>))]
public sealed class ExecuteActionRequestedHandler : IEventHandler<ExecuteActionRequestedIntegrationEvent>
{
    private readonly IReadOnlyDictionary<string, IWorkflowAction> _local;
    public ExecuteActionRequestedHandler(IEnumerable<IWorkflowAction> actions)
        => _local = actions.ToDictionary(a => a.Name, StringComparer.Ordinal);
    public ValueTask HandleAsync(ExecuteActionRequestedIntegrationEvent e, CancellationToken ct)
        => _local.TryGetValue(e.ActionType, out var a)
            ? a.ExecuteAsync(new WorkflowActionContext(e.RuleId, e.RunId, e.Trigger, e.Parameters), ct)
            : ValueTask.CompletedTask;     // действие не наше — игнор (другой сервис подберёт)
}
```

**MUST have `Client.Tests`** (сериализация registry/sync; 404/409; `ContinueOnFailure`; маршрутизация
`ExecuteActionRequested` на локальное действие).

---

## 13. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | инварианты `AutomationRuleBase` (Enable бросает при пустом правиле; Enable/Disable идемпотентны и публикуют события; `SetActions` сортирует по `Order`; `SetTriggers` публикует `Changed`; `EventTriggerKeys` корректен); наследование (`sealed AutomationRule` с доп. полем создаётся фабрикой); `AutomationRun` (Start → RecordSuccess/Failure → Complete даёт верный `Status`: Succeeded/Failed/PartiallyFailed; событие Failed vs Completed) |
| `Application.Tests` | **движок** (моки `IRuleMatcher`/`IExpressionEvaluator`/`IWorkflowActionExecutor`/`IParameterRenderer`/`IRepository`): условие true→исполняет, false→пропускает; порядок действий по `Order`; `FailureMode` (StopRule прерывает, ContinueNext продолжает, фиксируется в `Run.Steps`); **дедуп** (unique-violation → пропуск без повторного исполнения); `TestRuleCommand` ничего не исполняет (executor не вызван); идемпотентный `SyncWorkflowRegistry` (upsert каталога, правила не тронуты); `ParameterRenderer` (подстановка `{{trigger.x}}`, неизвестная переменная → null) |
| `Client.Tests` | сериализация registry/sync; 404/409; `ContinueOnFailure` при недоступном Workflow; `ExecuteActionRequestedHandler` маршрутизирует на локальное действие и игнорирует чужое |
| (Default) | интеграционный smoke: register trigger/action → create rule (`DealWon → CreateActivity`) → publish событие → проверить `AutomationRun` + созданную активность; повторная доставка того же события не создаёт второй activity (дедуп); миграция применяется |

> Движок — чистая оркестрация над портами, **юнит-тестируется без БД/шины** (как `ISlotEngine` в Booking,
> движок FM). Это ядро модуля и главный приоритет покрытия.

Пример теста движка (порядок + FailureMode):

```csharp
[Fact]
public async Task StopRule_failure_halts_subsequent_actions()
{
    var rule = TestRule.With(actions: [
        Action("A", ActionFailureMode.ContinueNext),
        Action("B", ActionFailureMode.StopRule),   // упадёт
        Action("C", ActionFailureMode.ContinueNext)]);
    matcher.Setup(m => m.MatchAsync("E", null, ct)).ReturnsAsync([rule]);
    executor.Setup(e => e.ExecuteAsync("B", It.IsAny<WorkflowActionContext>(), ct))
            .ThrowsAsync(new Exception("boom"));

    await engine.HandleTriggerAsync(Envelope("E"), ct);

    executor.Verify(e => e.ExecuteAsync("A", It.IsAny<WorkflowActionContext>(), ct), Times.Once);
    executor.Verify(e => e.ExecuteAsync("B", It.IsAny<WorkflowActionContext>(), ct), Times.Once);
    executor.Verify(e => e.ExecuteAsync("C", It.IsAny<WorkflowActionContext>(), ct), Times.Never); // прервано
}
```

---

## 14. План реализации (пошагово)

> Каждый шаг = отдельный коммит. После каждого слоя — `dotnet build` + `dotnet sln add` в `Cheetah.slnx`.
> Пакеты — через `Directory.Packages.props` (`PackageReference` без `Version`).

**Фаза 0 — абстракция**
1. `src/Cheetah.Workflow`: `WorkflowEventEnvelope`, `IWorkflowAction`/`WorkflowActionContext`,
   `IWorkflowActionExecutor`, `IWorkflowTrigger`/`WorkflowTriggerSink`, `TriggerDescriptor`/`ActionDescriptor`,
   `AddWorkflow()`, `CrmWorkflowModule`. Юнит-тесты диспетчера. В `Cheetah.slnx`.

**Фаза 1 — контракты**
2. `DomainEvents`: `AutomationRuleCreated/Enabled/Disabled/Changed`, `AutomationRunCompleted/Failed`.
3. `Shared`: `TriggerType`/`RunStatus`/`ActionFailureMode`, `BuiltInActions`, `WorkflowKeys`.
4. `Contracts`: абстрактные DTO/Request + `TriggerBindingDto`/`RuleActionDto`/`AutomationRunDto`/
   `TestRunResult` + `ExecuteActionRequestedIntegrationEvent`.

**Фаза 2 — домен**
5. `Domain`: `AutomationRuleBase` + `TriggerBinding`/`RuleAction` + `AutomationRun`/`AutomationRunStep`.
6. `Domain`: спецификации (§8).
7. `Domain.Tests`: инварианты + наследование + жизненный цикл `AutomationRun`.

**Фаза 3 — инфраструктура (хранилище)**
8. `Infrastructure`: EF-конфиги (§10.1) + `WorkflowDbContextBase<>` + каталог-сущности (§10.8).
9. `Infrastructure`: `IRuleMatcher` (кэш-индекс + инвалидация, §10.2), `ParameterRenderer` (§9.4).

**Фаза 4 — приложение (движок)**
10. `Application`: `IAutomationRuleFactory`/`Projector`; `WorkflowEngine<>` (§9.1).
11. `Application`: команды/запросы + хендлеры (Create/Update/Enable/Disable/Delete/SyncRegistry/TestRule).
12. `AddWorkflowApplication<,,,,>`.
13. `Application.Tests`: движок (условие/порядок/FailureMode/дедуп), dry-run, идемпотентный Sync, renderer.

**Фаза 5 — интеграция шины**
14. `Infrastructure`: `WorkflowEventForwarder` (firehose, §10.3) + `WorkflowEnvelopeHandler` + опц. Inbox.
15. `Infrastructure`: executors (`InProc`/`Bus`/`Http`/`Composite`, §10.4) + cron `ScheduleTriggerTask` (§10.6).
16. `AddWorkflowInfrastructure<,>` (§10.9).

**Фаза 6 — API + Default + клиент**
17. `Api`: `AutomationRuleEndpointsBase<,>` + `CheetahWorkflowApiModuleBase<>` (§11).
18. `.Default`: `sealed AutomationRule` + DTO/Request + Factory/Projector + `WorkflowDbContext` +
    `IDesignTimeDbContextFactory` + миграция (схема `workflow`) + встроенные действия (§10.5) + единый модуль.
19. `Client`: `IWorkflowCatalogClient`/`HttpWorkflowCatalogClient`, `WorkflowRegistrationSyncService`,
    `.AddWorkflowClient(...)`/`.RegisterTriggers(...)`/`.RegisterActions(...)`, `ExecuteActionRequestedHandler`.
    `Client.Tests`.

**Фаза 7 — финал**
20. Follow-up: транзакционный Outbox для действий; Saga для `Compensate`; gRPC; шаблонный `HttpActionExecutor`;
    внешние `IWorkflowTrigger`; аудит изменений правил через `Cheetah.Audit`; end-to-end пилот.
21. README обеих сборок (`src/Cheetah.Workflow/README.md` + `src/Modules/Workflow/README.md`); статус →
    «реализовано»; обновить `plans.md` и `MEMORY.md`.

---

## 15. События (публикует Workflow)

```csharp
namespace Cheetah.Modules.Workflow.DomainEvents;

AutomationRuleCreatedIntegrationEvent(Guid RuleId, string Name, string OwnerService) : EventBase;
AutomationRuleEnabledIntegrationEvent(Guid RuleId) : EventBase;     // → инвалидация кэш-индекса
AutomationRuleDisabledIntegrationEvent(Guid RuleId) : EventBase;    // → инвалидация кэш-индекса
AutomationRuleChangedIntegrationEvent(Guid RuleId, IReadOnlyList<string> TriggerKeys) : EventBase; // → инвалидация
AutomationRunCompletedIntegrationEvent(Guid RunId, Guid RuleId, RunStatus Status) : EventBase;
AutomationRunFailedIntegrationEvent(Guid RunId, Guid RuleId, string Error) : EventBase;  // → Notification (опц.)
```

Плюс `ExecuteActionRequestedIntegrationEvent` (Contracts, §7) — для микросервисного исполнения действий.

---

## 16. Сквозные примеры (end-to-end)

### Пример 1 — «Выиграна сделка → поставить задачу-благодарность» (event + condition + action)

Правило (создаётся через `POST /api/automation/rules`):

```jsonc
{
  "name": "Сделка выиграна → задача менеджеру",
  "ownerService": "Deals",
  "conditionExpression": "{ \">\": [ { \"var\": \"Amount\" }, 100000 ] }",   // только крупные
  "triggers": [ { "triggerType": "Event", "triggerKey": "DealWonIntegrationEvent" } ],
  "actions": [
    { "order": 0, "actionType": "CreateActivity", "failureMode": "ContinueNext",
      "parameters": "{ \"title\": \"Поблагодарить за сделку {{trigger.DealId}}\", \"assigneeId\": \"{{trigger.OwnerId}}\", \"entityType\": \"crm.deal\", \"entityId\": \"{{trigger.DealId}}\", \"dueInHours\": 24 }" },
    { "order": 1, "actionType": "SendNotification", "failureMode": "ContinueNext",
      "parameters": "{ \"userId\": \"{{trigger.OwnerId}}\", \"template\": \"deal-won\", \"dealId\": \"{{trigger.DealId}}\" }" }
  ]
}
```

Поток: Deals публикует `DealWonIntegrationEvent` → forwarder кладёт конверт в firehose → `WorkflowEngine`
матчит правило → условие `Amount > 100000` → действие 0 (`IDispatcher.Send(CreateActivityCommand)`) →
действие 1 (`SendNotification`) → `AutomationRun(Status=Succeeded)`.

### Пример 2 — «Каждый понедельник 9:00 → дайджест просроченных» (schedule)

```jsonc
{
  "name": "Понедельничный дайджест",
  "ownerService": "Activities",
  "triggers": [ { "triggerType": "Schedule", "triggerKey": "0 9 * * 1" } ],   // cron
  "actions": [ { "order": 0, "actionType": "SendNotification", "failureMode": "StopRule",
                 "parameters": "{ \"audience\": \"managers\", \"template\": \"weekly-overdue\" }" } ]
}
```

Поток: `ScheduleTriggerTask` (под `IDistributedLock`) видит due-правило → синтезирует конверт
`schedule:{ruleId}` → `WorkflowEngine` (условия нет) → действие.

### Пример 3 — микросервис: действие исполняется в другом сервисе

Workflow-сервис: `BusActionExecutor` публикует `ExecuteActionRequestedIntegrationEvent("CreateActivity", …)`
→ шина → сервис Activities: `ExecuteActionRequestedHandler` видит `ActionType="CreateActivity"` в своём
DI → `CreateActivityAction.ExecuteAsync`. Workflow-сервис **не ссылается** на код Activities.

---

## 17. Производительность, конкурентность, отказоустойчивость

**Горячий путь (цель 10k RPS событий):**

- **Матчинг — из кэша** (`IRuleMatcher`, §10.2): индекс `eventName → ruleIds` в `ICacheService`, БД
  только на miss/refresh. События без правил отсекаются за один lookup (пустой индекс).
- **Условие — чистое вычисление** (`IExpressionEvaluator`/JsonLogic) над уже разобранным `Payload`; без
  I/O. JsonLogic-движок имеет лимиты (`MaxExpressionLength`/`MaxNestingDepth`/`MaxEvaluationTime`) — DoS
  по «тяжёлому условию» исключён на уровне `Cheetah.Expressions`.
- **Действия — асинхронно и надёжно:** в микросервисе через `OutboxEventBus` (запись в транзакции Run,
  фоновый `OutboxProcessor` доставляет с ретраями) — горячий путь движка не блокируется сетью цели.

**Конкурентность:**

- **Дедуп** — уникальный индекс `(RuleId, EventId)`: при гонке двух доставок одного события вторая
  вставка `AutomationRun` падает (23505) → пропуск. Без распределённых локов на горячем пути.
- **Cron** — `IDistributedLock` гарантирует один исполнитель скана в кластере.
- **Инвалидация кэша** — по событиям `AutomationRule*Changed` на всех инстансах (через шину).

**Отказоустойчивость:**

- недоступность Workflow не роняет источники (публикуют в шину) и контрибуторов (`ContinueOnFailure`);
- at-least-once шины + дедуп = эффективно exactly-once на уровне срабатывания правила;
- сбой отдельного действия фиксируется в `AutomationRun.Steps` (`PartiallyFailed`) и не теряется;
- `AutomationRunFailedIntegrationEvent` → Notification владельцу правила (опц.).

**Ретеншн журнала:** `AutomationRun`/`Steps` растут быстро — фоновая чистка старше N дней (как
`InboxCleanupService`/`OutboxCleanupService`).

---

## 18. Безопасность

- **Админка и реестр** — за `Cheetah.Permissions` (создавать/включать правила и менять каталог может
  только уполномоченный). Правило — мощный примитив (может слать webhook, менять данные), поэтому права
  на запись правил — отдельный permission.
- **`CallWebhook`-действие** — исходящий вызов наружу: за `Cheetah.RateLimit`, URL — из allow-list, тело
  подписывается (HMAC) — переиспользуем подход модуля Webhooks (§9 плана).
- **JsonLogic-условия** исполняются в песочнице `Cheetah.Expressions` с лимитами времени/глубины — нет
  произвольного кода, только декларативные выражения над `Payload`.
- **Параметры действий** не должны позволять обращаться к произвольным внутренним API без дескриптора:
  `CompositeActionExecutor` исполняет только зарегистрированные `ActionType` (есть в каталоге).
- **Мультитенантность:** правило с `TenantId` матчится только к событиям своего тенанта (§8); действие
  исполняется в контексте тенанта триггера.

---

## 19. Зависимости от инфраструктуры

| Инфраструктурный модуль | Использование |
|---|---|
| `Cheetah.Core.Domain` | `AggregateRoot`/`Entity`, аудит-интерфейсы (`DateTimeOffset?`) |
| `Cheetah.Core.Events` (`IEventBus`/`EventBase`) | firehose-вход + публикация действий/событий правил |
| `Cheetah.Core.DataAccess` (`IRepository<T>`) + `Specification` | репозитории + фильтрация (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` (`IDispatcher`) | команды/запросы + in-proc действия (монолит) |
| `Cheetah.Expressions.JsonLogic` (`IExpressionEvaluator`) | условие правила + вычислимые параметры действий |
| `Cheetah.Core.Inbox` (`IInboxStore`) | опц. идемпотентность подписки firehose |
| `Cheetah.Core.Outbox` (`OutboxEventBus`/`IOutboxStore`) | надёжная доставка действий и событий правил |
| `Cheetah.Saga` (`Saga<TData>`) | компенсируемые многошаговые действия (`FailureMode = Compensate`) |
| `Cheetah.BackgroundTasks` (`CronBackgroundTask`) + `Cheetah.DistributedLock` | расписание (cron-триггеры) |
| `Cheetah.Core.Cache` (`ICacheService`) | кэш-индекс `eventName → ruleIds` (горячий путь матчинга) |
| `Cheetah.Core.Tenants` (опц.) | tenant-scope правил |
| `CrmEntityFrameworkModule` + `…PostgreSqlModule` | EF Core + Npgsql (`jsonb` для условий/параметров) |
| `Cheetah.Backend.Endpoints` + `Cheetah.Generators.Endpoints` | декларативные эндпоинты |
| `Cheetah.Permissions` | авторизация админки/реестра |
| `Cheetah.RateLimit` + `Security` | защита `CallWebhook`-действия |
| `Cheetah.Audit` (опц.) | журнал изменений правил (кто/когда) |
| Client'ы целевых модулей (Activities/Deals/Notification/Webhooks) | in-proc действия (монолит) |

---

## 20. Отличия от исходного эскиза плана (`docs/plans.md` §8)

1. **Явно зафиксирована модель интеграции** (§0): Workflow — реактивный консьюмер событий + диспетчер
   действий, а не RPC-сервис. Триггеры идут **в** Workflow через шину (firehose), действия — **из**
   Workflow наружу; «ходят к Workflow» только при регистрации (как FM).
2. **Firehose-конверт `WorkflowEventEnvelope`** (§2.2/§10.3) — Workflow не зависит от `DomainEvents`-сборок
   всех модулей (критично для микросервиса). Эскиз подразумевал «широкий набор подписок».
3. **Port `IWorkflowActionExecutor` с тремя транспортами** (§2.3/§10.4): in-proc плагины (монолит),
   команда-событие по шине и шаблонный HTTP (микросервис) — единый ответ на «как один сервис исполняет
   действия в чужих модулях». Эскиз давал только in-proc `IAutomationAction`.
4. **Абстрактный шаблон + `.Default`** (как Activities/FM) вместо `sealed`-эскиза — по требованию
   расширяемости §0.1. Главная ось — поведенческие плагины `IWorkflowAction`/`IWorkflowTrigger`.
5. **Идемпотентность — уникальный индекс `(RuleId, EventId)` на `AutomationRun`** (журнал = дедуп) как
   основной механизм; `Cheetah.Core.Inbox`-декоратор — дополнение (§9.1/§10.3). Эскиз называл только Inbox.
6. **Журнал расширен** до `AutomationRun` + `AutomationRunStep` (по-действийный результат, статусы
   `Succeeded/Failed/PartiallyFailed`) — точнее, чем `AutomationRun(Status, Error)` эскиза.
7. **StateMachine не используется** — у правила нет состояния-автомата; `AutomationRun.Status` —
   журнальный enum (эскиз StateMachine и не требовал).

**Отложено (follow-up):** транзакционный Outbox для действий; Saga для компенсируемых действий; gRPC
реестра/приёма действий; шаблонный `HttpActionExecutor`; внешние пользовательские триггеры
(`IWorkflowTrigger`) сверх Event/Schedule; аудит изменений правил через `Cheetah.Audit`.

---

## 21. Глоссарий

| Термин | Значение |
|---|---|
| **Правило (AutomationRule)** | агрегат: набор триггеров + условие + упорядоченные действия |
| **Триггер (TriggerBinding)** | что запускает правило: имя события (`Event`) или cron (`Schedule`) |
| **Условие (Condition)** | JsonLogic-выражение над `Payload` события; `null` = всегда истина |
| **Действие (RuleAction)** | операция с параметрами; `ActionType` — имя из каталога; `FailureMode` |
| **Firehose** | единый канал шины, куда forwarder дублирует все события как `WorkflowEventEnvelope` |
| **Конверт (WorkflowEventEnvelope)** | нормализованное событие: `EventName` + `Payload` + `SourceEventId` |
| **Executor** | порт `IWorkflowActionExecutor`; выбирает транспорт (InProc/Bus/Http) и исполняет действие |
| **Run (AutomationRun)** | журнал одного срабатывания правила; уникален по `(RuleId, EventId)` (дедуп) |
| **Реестр (registry)** | каталог триггеров/действий, который контрибуторы наполняют при старте |
| **Контрибутор** | модуль, который регистрирует свои триггеры/действия и (in-proc) поставляет `IWorkflowAction` |
