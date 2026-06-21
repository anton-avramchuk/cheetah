# Cheetah.Modules.Workflow — автоматизация бизнес-процессов (no-code правила)

Модуль автоматизации Cheetah CRM. Правило = **триггер → условие (JsonLogic) → действия**. Workflow —
**реактивный консьюмер событий + диспетчер действий**: события приходят в него через шину (firehose), а
действия он исполняет наружу через расширяемые плагины `IWorkflowAction`.

Полное код-уровневое проектное описание — [`docs/modules/workflow.md`](../../../docs/modules/workflow.md).
Контракты расширения — в абстракции [`Cheetah.Workflow`](../../Cheetah.Workflow/README.md).

---

## Оглавление

1. [Модель интеграции (читать первым)](#1-модель-интеграции-читать-первым)
2. [Состав сборок](#2-состав-сборок)
3. [Внедрение в монолит](#3-внедрение-в-монолит)
4. [Внедрение в микросервисную топологию](#4-внедрение-в-микросервисную-топологию)
5. [Создание правил (API + JSON)](#5-создание-правил)
6. [Условия (JsonLogic)](#6-условия-jsonlogic)
7. [Шаблоны параметров действий](#7-шаблоны-параметров-действий)
8. [Кастомные действия (`IWorkflowAction`)](#8-кастомные-действия-iworkflowaction) ← как сделать свою «активити»
9. [Регистрация триггеров и действий в каталоге](#9-регистрация-триггеров-и-действий-в-каталоге)
10. [Кастомные триггеры (`IWorkflowTrigger`)](#10-кастомные-триггеры-iworkflowtrigger)
11. [Структурное расширение: своё правило с доп. полями](#11-структурное-расширение-своё-правило-с-доп-полями)
12. [Справочник REST API](#12-справочник-rest-api)
13. [Как работает движок (поток, дедуп, ошибки)](#13-как-работает-движок)
14. [Тестирование](#14-тестирование)
15. [Конфигурация и миграции](#15-конфигурация-и-миграции)
16. [Диагностика проблем](#16-диагностика-проблем)
17. [Ограничения и follow-up](#17-ограничения-и-follow-up)

---

## 1. Модель интеграции (читать первым)

Частая ошибка — считать Workflow «сервисом, который дёргают». Это не так. Связь идёт по трём каналам:

| Канал | Направление | Механизм |
|---|---|---|
| **Триггеры** | модуль → Workflow | модуль просто **публикует своё интеграционное событие** в шину; форвардер дублирует его в firehose-канал, Workflow подписан. Источник о Workflow **не знает** |
| **Действия** | Workflow → модуль | Workflow исполняет действие через `IWorkflowActionExecutor`: in-proc плагин (монолит) или команда-событие по шине (микросервис) |
| **Регистрация** | модуль → Workflow при старте | контрибутор декларирует свои триггеры/действия в каталоге (`registry/sync`) — чтобы админка могла строить правила |

> **Главное:** в рантайме никто не зовёт Workflow синхронно. Триггеры идут *в* него через шину, действия —
> *из* него. «Ходят к Workflow» только при регистрации на старте.

```
 Источник (Deals)                       WORKFLOW                          Цель (Activities)
   publish DealWonEvent ─┐            ┌───────────────┐              ┌─ IWorkflowAction ─┐
                         ▼  firehose  │ 1 match rules │   action     │  CreateActivity   │
        ШИНА ── WorkflowEventEnvelope ─▶ 2 condition  ├─ executor ──▶│  (in-proc/шина)   │
                                      │ 3 plan→exec   │              └───────────────────┘
                                      │ 4 AutomationRun (дедуп)      │
                                      └───────────────┘
```

---

## 2. Состав сборок

| Сборка | Назначение |
|---|---|
| `Cheetah.Workflow` (абстракция) | контракты расширения: `WorkflowEventEnvelope`, `IWorkflowAction`, `IWorkflowActionExecutor`, `IWorkflowTrigger`, дескрипторы |
| `…Workflow.DomainEvents` | события правил/срабатываний |
| `…Workflow.Shared` | enums (`TriggerType`/`RunStatus`/`ActionFailureMode`), `BuiltInActions`, `WorkflowConstants` |
| `…Workflow.Contracts` | DTO/Request базы, `ExecuteActionRequestedIntegrationEvent` |
| `…Workflow.Domain` | `AutomationRuleBase` + `TriggerBinding`/`RuleAction`/`AutomationRun`/`AutomationRunStep`, спецификации, репозиторий-интерфейс |
| `…Workflow.Application` | движок `WorkflowEngine<TRule>`, `ParameterRenderer`, CQRS, `AddWorkflowApplication<…>` |
| `…Workflow.Infrastructure` | EF, репозиторий, матчинг, `InProcActionExecutor`, firehose, `AddWorkflowInfrastructure<…>` |
| `…Workflow.Api` | `AutomationRuleEndpointsBase` + `CheetahWorkflowApiModuleBase` |
| `…Workflow.Default` | sealed `AutomationRule` + DbContext + миграция + эндпоинты + `LogAction` + готовый модуль |
| `…Workflow.Client` | `IWorkflowCatalogClient`, регистрация триггеров/действий, приём действий в микросервисе |

---

## 3. Внедрение в монолит

Самый быстрый путь — подключить готовый модуль `.Default`.

### Шаг 1. Подключить модуль

Добавьте зависимость от `CheetahWorkflowDefaultModule` в корневой модуль приложения:

```csharp
[DependsOn(
    typeof(CheetahWorkflowDefaultModule)
    /* … остальные модули приложения … */)]
public class AppModule : CrmModule
{
    // Firehose-форвардер подключается на уровне хоста — см. шаг 2.
    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        // Вызывать ПОСЛЕ регистрации шины (Redis/Kafka/InMemory/Outbox), поэтому в PostConfigureServices.
        context.Services.AddWorkflowFirehose();
    }
}
```

`CheetahWorkflowDefaultModule` сам:
- регистрирует `WorkflowDbContext` + репозитории + движок (`AddWorkflowInfrastructure` + `AddWorkflowApplication`);
- маппит REST-эндпоинты `/api/automation/*`;
- подписывает движок на firehose-поток (`WorkflowEnvelopeHandler`).

> **`AddWorkflowFirehose()` обязателен.** Без него никто не публикует firehose-конверты → движок не получает
> события, и правила не срабатывают. Метод декорирует зарегистрированный `IEventBus` форвардером,
> сохраняя его lifetime.

### Шаг 2. Строка подключения

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Workflow": "Host=localhost;Database=workflow;Username=postgres;Password=postgres"
  }
}
```

Имя строки — `Workflow` (`WorkflowConstants.ConnectionStringName`). Схема таблиц — `workflow`.

### Шаг 3. Применить миграцию

Миграция `InitialWorkflow` уже в `.Default`. Применяется штатным мигратором Cheetah при старте
(`AddDatabaseMigrator`), либо вручную:

```bash
dotnet ef database update \
  --project src/Modules/Workflow/Cheetah.Modules.Workflow.Default \
  --context WorkflowDbContext
```

### Шаг 4. Дать движку действия

Из коробки есть только действие `Log` (диагностическое). Реальные действия добавляются как плагины —
см. [§8](#8-кастомные-действия-iworkflowaction).

После этого создавайте правила через [API](#5-создание-правил).

---

## 4. Внедрение в микросервисную топологию

Workflow выносится в отдельный сервис; остальные сервисы — контрибуторы (источники событий и/или цели
действий).

**Сервис Workflow:** как монолит (§3) — `CheetahWorkflowDefaultModule` + `AddWorkflowFirehose()` + БД.

**Сервис-источник** (например, Deals): ничего особого — он просто публикует свои события в общую шину.
Опционально регистрирует свои триггеры в каталоге Workflow (§9), чтобы они были видны в админке.

**Сервис-цель** (например, Activities — где реально исполняются действия): подключает `Client` и свои
`IWorkflowAction`; приёмник `ExecuteActionRequestedHandler` подбирает команды-действия с шины и исполняет
локально:

```csharp
// модуль сервиса-цели
[DependsOn(typeof(CrmWorkflowClientModule))]   // подписывает ExecuteActionRequestedHandler на шину
public class ActivitiesServiceModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services
            .AddWorkflowClient(o => o.BaseUrl = "http://workflow")   // HTTP к сервису Workflow (registry/sync)
            .RegisterActions(a => a.Add("CreateActivity", "Activities", "Создать активность",
                ActionTransport.Bus,
                p => p.Param("title", "string", required: true)
                      .Param("assigneeId", "guid", required: true)));
    }
}
```

> **Транспорт.** Сейчас встроен `InProcActionExecutor` (монолит). Транспорт `Bus` (Workflow публикует
> `ExecuteActionRequestedIntegrationEvent`, цель ловит `ExecuteActionRequestedHandler`) — приёмная часть
> готова в `Client`; издающий `BusActionExecutor` помечен follow-up (§17). До его появления микросервисные
> действия можно исполнять, разместив `IWorkflowAction` прямо в сервисе Workflow.

---

## 5. Создание правил

Правила создаются через REST (или сидингом в БД). Пример: «когда выиграна крупная сделка — поставить
задачу и уведомить владельца».

```http
POST /api/automation/rules
Content-Type: application/json

{
  "name": "Крупная сделка выиграна → задача + уведомление",
  "ownerService": "Deals",
  "conditionExpression": "{ \">\": [ { \"var\": \"Amount\" }, 100000 ] }",
  "triggers": [
    { "triggerType": "Event", "triggerKey": "DealWonIntegrationEvent", "parameters": null }
  ],
  "actions": [
    {
      "order": 0,
      "actionType": "CreateActivity",
      "failureMode": "ContinueNext",
      "parameters": "{ \"title\": \"Поблагодарить за сделку\", \"assigneeId\": \"{{trigger.OwnerId}}\", \"entityType\": \"crm.deal\", \"entityId\": \"{{trigger.DealId}}\" }"
    },
    {
      "order": 1,
      "actionType": "SendNotification",
      "failureMode": "ContinueNext",
      "parameters": "{ \"userId\": \"{{trigger.OwnerId}}\", \"template\": \"deal-won\" }"
    }
  ]
}
```

Затем включить правило (по умолчанию создаётся выключенным):

```http
POST /api/automation/rules/{id}/enable
```

Перед включением можно прогнать **dry-run** (проверит условие и покажет план действий, ничего не исполняя):

```http
POST /api/automation/rules/{id}/test
Content-Type: application/json

{
  "eventName": "DealWonIntegrationEvent",
  "sourceEventId": "00000000-0000-0000-0000-000000000000",
  "payload": { "Amount": 150000, "OwnerId": "11111111-1111-1111-1111-111111111111", "DealId": "22222222-2222-2222-2222-222222222222" }
}
```

Поля правила:
- `triggerKey` для `Event` = **имя типа** интеграционного события (`@event.GetType().Name`), напр.
  `DealWonIntegrationEvent`; для `Schedule` = cron-выражение;
- `conditionExpression` — JsonLogic-строка над payload события (§6), `null` = всегда срабатывает;
- `actions[].parameters` — **JSON-строка** шаблона параметров (§7);
- `failureMode` — `StopRule` | `ContinueNext` | `Compensate` (§13).

---

## 6. Условия (JsonLogic)

Условие — выражение [JsonLogic](https://jsonlogic.com) над `payload` события (движок `Cheetah.Expressions.JsonLogic`).
Переменные — это поля payload (имена = имена свойств события, как в record).

```jsonc
// Amount > 100000
{ ">": [ { "var": "Amount" }, 100000 ] }

// Source == "Web" И есть Email
{ "and": [ { "==": [ { "var": "Source" }, "Web" ] }, { "!!": { "var": "Email" } } ] }

// Stage входит в список
{ "in": [ { "var": "StageId" }, ["s1", "s2"] ] }
```

Движок имеет защитные лимиты (`MaxExpressionLength`/`MaxNestingDepth`/`MaxEvaluationTime`), так что «тяжёлое»
условие не повесит горячий путь.

---

## 7. Шаблоны параметров действий

`parameters` действия — JSON-объект. Значение-строка вида `"{{trigger.Field}}"` подставляется из payload
триггера (`ParameterRenderer`).

```jsonc
{
  "title": "Задача по сделке",          // литерал
  "assigneeId": "{{trigger.OwnerId}}",   // → payload["OwnerId"]
  "entityId": "{{trigger.DealId}}",      // → payload["DealId"]
  "priority": 2                          // число-литерал
}
```

> **Важно (текущее ограничение):** подстановка работает **только для строки целиком**. Встроенный вариант
> `"Сделка {{trigger.DealId}}"` НЕ подставляется (останется литералом). Нужна вычислимая/встроенная
> подстановка — это follow-up (§17). Неизвестная переменная → `null`.

В действии параметры приходят как `context.Parameters` (`IReadOnlyDictionary<string, object?>`). Значения
из payload приходят как `JsonElement` (payload разбирается из JSON форвардером), литералы — как
`string`/`long`/`double`/`bool`. Читайте их защитно — см. хелпер в [§8](#8-кастомные-действия-iworkflowaction).

---

## 8. Кастомные действия (`IWorkflowAction`)

**Это главная точка расширения.** «Активити» (действие) — это класс, реализующий `IWorkflowAction`, с
уникальным `Name`. Движок находит его по `RuleAction.ActionType` и исполняет.

### 8.1. Минимальный пример

```csharp
using Cheetah.Core.DependencyInjection;
using Cheetah.Workflow;

[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]   // авто-регистрация в DI
public sealed class SlackNotifyAction : IWorkflowAction
{
    private readonly ISlackClient _slack;

    public SlackNotifyAction(ISlackClient slack) => _slack = slack;

    public string Name => "SlackNotify";   // = RuleAction.ActionType в правиле

    public async ValueTask ExecuteAsync(WorkflowActionContext context, CancellationToken ct)
    {
        var channel = context.GetString("channel") ?? "#general";
        var text = context.GetString("text") ?? $"Событие {context.Trigger.EventName}";
        await _slack.PostAsync(channel, text, ct);
    }
}
```

Зарегистрировав этот класс (через `[Export]`), вы можете в любом правиле указать
`"actionType": "SlackNotify"` с параметрами `{ "channel": "...", "text": "..." }`.

### 8.2. Что доступно в `WorkflowActionContext`

| Поле | Тип | Что это |
|---|---|---|
| `RuleId` | `Guid` | правило, которое сработало |
| `RunId` | `Guid` | запись журнала `AutomationRun` этого срабатывания |
| `Trigger` | `WorkflowEventEnvelope` | исходное событие: `EventName`, `SourceEventId`, `Payload`, `TenantId?` |
| `Parameters` | `IReadOnlyDictionary<string, object?>` | **отрендеренные** параметры действия (литералы + подставленные из payload) |

### 8.3. Безопасное чтение параметров (встроенный хелпер)

Значения параметров могут быть `JsonElement` (подставлены из payload) или примитивами (литералы шаблона).
Чтобы скрыть разницу, в абстракции `Cheetah.Workflow` есть готовые расширения
`WorkflowActionContextExtensions` — используйте их прямо в действии:

```csharp
using Cheetah.Workflow;   // GetString / GetGuid / GetInt / GetBool

string? channel = context.GetString("channel");
Guid?   assignee = context.GetGuid("assigneeId");
int?    hours = context.GetInt("dueInHours");
bool?   urgent = context.GetBool("urgent");
```

Сигнатуры:

```csharp
string? GetString(this WorkflowActionContext ctx, string key);
Guid?   GetGuid(this WorkflowActionContext ctx, string key);
int?    GetInt(this WorkflowActionContext ctx, string key);
bool?   GetBool(this WorkflowActionContext ctx, string key);
```

### 8.4. Действие, дёргающее другой модуль (монолит)

Самый частый случай — действие вызывает команду доменного модуля через `IDispatcher`:

```csharp
[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]
public sealed class CreateActivityAction : IWorkflowAction
{
    private readonly IDispatcher _dispatcher;

    public CreateActivityAction(IDispatcher dispatcher) => _dispatcher = dispatcher;

    public string Name => BuiltInActions.CreateActivity;   // "CreateActivity"

    public async ValueTask ExecuteAsync(WorkflowActionContext ctx, CancellationToken ct)
    {
        var title = ctx.GetString("title") ?? "Задача";
        var assignee = ctx.GetGuid("assigneeId")
            ?? throw new InvalidOperationException("assigneeId is required");
        var entityType = ctx.GetString("entityType") ?? "crm.deal";
        var entityId = ctx.GetGuid("entityId") ?? Guid.Empty;

        await _dispatcher.SendAsync(new CreateActivityCommand(
            Type: ActivityType.Task, Title: title,
            AssigneeId: assignee, OwnerId: assignee,
            EntityType: entityType, EntityId: entityId,
            DueAt: DateTime.UtcNow.AddHours(ctx.GetInt("dueInHours") ?? 24),
            Priority: ActivityPriority.Normal, Description: null), ct);
    }
}
```

> Это создаёт зависимость сборки действия от того модуля, чью команду оно вызывает (`Activities.Application`).
> Размещайте такие действия в сборке-контрибуторе (рядом с целевым модулем), а не в ядре Workflow.

### 8.5. Идемпотентность и ошибки

- Движок гарантирует **«одно событие → один запуск правила»** (уникальный индекс `(RuleId, EventId)`),
  но если действие делает внешний вызов, делайте его по возможности идемпотентным — at-least-once шины
  может доставить событие повторно при сбоях до записи журнала.
- Брошенное исключение фиксируется в `AutomationRun.Steps` и обрабатывается по `FailureMode` действия
  (`StopRule`/`ContinueNext`). Не глотайте исключения внутри действия, если хотите, чтобы сбой попал в журнал.

### 8.6. Где регистрируется действие

`[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]` достаточно, если сборка с действием — это
`CrmModule` с генератором (`RegisterServices`). `InProcActionExecutor` получает `IEnumerable<IWorkflowAction>`
и диспетчеризует по `Name`. Если двух действий с одинаковым `Name` — будет конфликт ключа словаря; имена
должны быть уникальны (конвенция — `BuiltInActions` для встроенных, `"{service}.{action}"` для своих).

---

## 9. Регистрация триггеров и действий в каталоге

Каталог нужен админке/UI, чтобы показывать доступные триггеры и действия при сборке правила. Контрибутор
декларирует их при старте через `Client`:

```csharp
context.Services
    .AddWorkflowClient(o => o.BaseUrl = "http://workflow")   // адрес сервиса Workflow (в монолите — свой же)
    .RegisterTriggers(t => t
        .Add("DealWonIntegrationEvent", "Deals", "Сделка выиграна", payload: ["DealId", "OwnerId", "Amount"])
        .Add("DealLostIntegrationEvent", "Deals", "Сделка проиграна", payload: ["DealId", "Reason"]))
    .RegisterActions(a => a
        .Add("CreateActivity", "Activities", "Создать активность", ActionTransport.InProc,
            p => p.Param("title", "string", required: true)
                  .Param("assigneeId", "guid", required: true)
                  .Param("entityType", "string")
                  .Param("entityId", "guid")));
```

`WorkflowRegistrationSyncService` (hosted) на старте отправит всё это в `POST /api/automation/registry/sync`.
Недоступность Workflow **не валит хост** (`ContinueOnFailure`).

> Серверный приём `registry/sync` + хранение каталога — follow-up (§17). Клиентская часть и контракты
> готовы; декларация действий полезна уже сейчас как самодокументирование.

---

## 10. Кастомные триггеры (`IWorkflowTrigger`)

Встроенные триггеры — `Event` (firehose) и `Schedule` (cron, follow-up). Нестандартный источник (например,
внешний приёмник вебхуков) реализуется через `IWorkflowTrigger`, который «толкает» нормализованные конверты
в движок:

```csharp
public interface IWorkflowTrigger
{
    string Name { get; }
    ValueTask ActivateAsync(WorkflowTriggerSink sink, CancellationToken ct);
}
```

Внутри `ActivateAsync` подпишитесь на свой источник и для каждого события вызовите
`sink.Push(new WorkflowEventEnvelope(eventName, sourceEventId, payload), ct)`. Для большинства задач
встроенного `Event`-триггера (любое интеграционное событие шины) достаточно — кастомный триггер нужен редко.

---

## 11. Структурное расширение: своё правило с доп. полями

Если нужно добавить правилу свои поля (владелец процесса, тикет, метки) — не форкайте модуль, а соберите
свою «реализацию» по образцу `.Default`. Шаблон даёт все базовые типы.

### 11.1. Сущность

```csharp
public sealed class AutomationRule : AutomationRuleBase
{
    public string? OwnerTeam { get; private set; }

    private AutomationRule() { }

    public static AutomationRule Create(string name, string ownerService, string? description,
        Guid? tenantId, string? ownerTeam)
    {
        var rule = new AutomationRule();
        rule.InitializeCore(Guid.NewGuid(), name, ownerService, description, tenantId);
        rule.OwnerTeam = ownerTeam;
        return rule;
    }
}
```

### 11.2. Contracts (DTO/Request с доп. полем)

```csharp
public sealed record CreateAutomationRuleRequest : CreateAutomationRuleRequestBase
{
    public string? OwnerTeam { get; init; }
}

public sealed record AutomationRuleDto : AutomationRuleDtoBase
{
    public string? OwnerTeam { get; init; }
}
```

### 11.3. Фабрика + проектор

```csharp
[Export(LifetimeType.Scoped, typeof(IAutomationRuleFactory<AutomationRule, CreateAutomationRuleRequest>))]
public sealed class RuleFactory : IAutomationRuleFactory<AutomationRule, CreateAutomationRuleRequest>
{
    public AutomationRule Create(CreateAutomationRuleRequest r)
    {
        var rule = AutomationRule.Create(r.Name, r.OwnerService, r.Description, null, r.OwnerTeam);
        rule.SetCondition(r.ConditionExpression);
        rule.SetTriggers(r.Triggers.Select(t => t.TriggerType == TriggerType.Schedule
            ? TriggerBinding.Schedule(rule.Id, t.TriggerKey)
            : TriggerBinding.Event(rule.Id, t.TriggerKey)));
        rule.SetActions(r.Actions.Select(a =>
            RuleAction.Create(rule.Id, a.Order, a.ActionType, a.Parameters, a.FailureMode)));
        return rule;
    }
}

[Export(LifetimeType.Scoped, typeof(IAutomationRuleProjector<AutomationRule, AutomationRuleDto>))]
public sealed class RuleProjector : IAutomationRuleProjector<AutomationRule, AutomationRuleDto>
{
    public AutomationRuleDto ToDto(AutomationRule r) => new()
    {
        Id = r.Id, Name = r.Name, OwnerService = r.OwnerService, Description = r.Description,
        ConditionExpression = r.ConditionExpression, IsActive = r.IsActive, OwnerTeam = r.OwnerTeam,
        Triggers = r.Triggers.Select(t => new TriggerBindingDto(t.TriggerType, t.TriggerKey, t.Parameters)).ToArray(),
        Actions = r.Actions.Select(a => new RuleActionDto(a.Order, a.ActionType, a.Parameters, a.FailureMode)).ToArray(),
        CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt
    };
}
```

### 11.4. EF: конфиг с доп. колонкой + DbContext + миграция

```csharp
public sealed class AutomationRuleConfiguration : AutomationRuleConfigurationBase<AutomationRule>
{
    protected override void ConfigureCustom(EntityTypeBuilder<AutomationRule> b)
        => b.Property(r => r.OwnerTeam).HasMaxLength(100);
}

public sealed class WorkflowDbContext : WorkflowDbContextBase<WorkflowDbContext, AutomationRule>
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options) { }
    protected override IEntityTypeConfiguration<AutomationRule> CreateRuleConfiguration()
        => new AutomationRuleConfiguration();
}
```

Затем `dotnet ef migrations add InitialWorkflow --context WorkflowDbContext` + `IDesignTimeDbContextFactory`.

### 11.5. Endpoints + модуль

```csharp
public sealed class AutomationRuleEndpoints
    : AutomationRuleEndpointsBase<CreateAutomationRuleRequest, AutomationRuleDto>;

public partial class AppWorkflowModule : CrmModule  // один модуль на сборку (требование генератора)
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services); // [Export]: RuleFactory, RuleProjector, ваши IWorkflowAction
        context.Services.AddWorkflowInfrastructure<WorkflowDbContext, AutomationRule>();
        context.Services.AddWorkflowApplication<AutomationRule, CreateAutomationRuleRequest,
            AutomationRuleDto, RuleFactory, RuleProjector>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new AutomationRuleEndpoints().Map(context.GetRouteBuilder());
        context.ServiceProvider.GetRequiredService<IEventBus>()
            .Subscribe<WorkflowEventEnvelope, WorkflowEnvelopeHandler>();
    }
}
```

> Не подключайте при этом `.Default` (иначе два конкурирующих набора регистраций/эндпоинтов).

---

## 12. Справочник REST API

База: `/api/automation` (за `Cheetah.Permissions`).

| Метод | Маршрут | Тело / параметры | Назначение |
|---|---|---|---|
| POST | `/rules` | `CreateAutomationRuleRequest` | создать правило (выключенным) → `201` + `id` |
| GET | `/rules?ownerService=&onlyActive=&page=&size=` | — | список правил |
| GET | `/rules/{id}` | — | правило по id (`404` если нет) |
| POST | `/rules/{id}/enable` | — | включить (бросит, если нет триггеров/действий) |
| POST | `/rules/{id}/disable` | — | выключить |
| DELETE | `/rules/{id}` | — | удалить |
| POST | `/rules/{id}/test` | `WorkflowEventEnvelope` | dry-run: `{ conditionMatched, plannedActions[] }` |
| GET | `/runs?ruleId=&status=&page=&size=` | — | журнал срабатываний |

`size` по умолчанию 50. `status` — `Pending`/`Succeeded`/`Failed`/`PartiallyFailed`.

---

## 13. Как работает движок

`WorkflowEngine<TRule>.HandleTriggerAsync(envelope)`:

1. **Матчинг** — `IRuleMatcher` отдаёт активные правила с `Event`-триггером на `envelope.EventName`
   (с учётом тенанта). Сейчас — прямой запрос в БД с `Include` детей; кэш-индекс — follow-up.
2. **Условие** — если у правила есть `ConditionExpression`, оно проверяется JsonLogic над `payload`;
   `false` → правило пропускается.
3. **Дедуп** — `ExistsAsync` по `(RuleId, EventId)`; если уже было — пропуск. Бэкстоп — уникальный индекс.
4. **Журнал** — создаётся `AutomationRun` (Pending).
5. **Действия по `Order`** — параметры рендерятся (`ParameterRenderer`), исполняются через
   `IWorkflowActionExecutor`. Успех/ошибка пишутся в `AutomationRunStep`.
6. **`FailureMode`** при ошибке действия:
   - `StopRule` — прервать оставшиеся действия;
   - `ContinueNext` — записать ошибку и продолжить;
   - `Compensate` — откат (через `Cheetah.Saga`) — follow-up.
7. **Завершение** — `AutomationRun.Complete()` выставляет статус (`Succeeded`/`Failed`/`PartiallyFailed`) и
   публикует `AutomationRun*`-событие.

---

## 14. Тестирование

Движок — чистая оркестрация над портами, тестируется без БД/шины (моки `IRuleMatcher`,
`IExpressionEvaluator`, `IWorkflowActionExecutor`, `IParameterRenderer`, `IRepository<AutomationRun, Guid>`,
`IEventBus`). Покрытие модуля: Domain (9) + Application (9, движок) + Client (4) = **22 теста**.

Свои действия тестируйте напрямую: соберите `WorkflowActionContext` с нужным `Parameters` и проверьте эффект.

---

## 15. Конфигурация и миграции

- **Строка подключения:** `ConnectionStrings:Workflow` (имя — `WorkflowConstants.ConnectionStringName`).
- **Схема БД:** `workflow` (`WorkflowConstants.Schema`). Таблицы: `AutomationRules`, `TriggerBindings`,
  `RuleActions`, `AutomationRuns`, `AutomationRunSteps`.
- **Миграции:** в `.Default` (или в вашей сборке-наследнике). Design-time фабрика — `WorkflowDbContextFactory`.
- `*.Parameters` и `ConditionExpression`/`PayloadSnapshot` — колонки `jsonb`.

---

## 16. Диагностика проблем

| Симптом | Причина / решение |
|---|---|
| Правило не срабатывает | Не вызван `AddWorkflowFirehose()` на хосте → конверты не публикуются. Проверьте, что метод вызван **после** регистрации шины (в `PostConfigureServices`). |
| Правило не срабатывает (firehose есть) | `triggerKey` ≠ `@event.GetType().Name`; правило выключено (`IsActive=false`); условие вернуло `false`. Прогоните `POST /rules/{id}/test`. |
| `No in-proc workflow action registered for 'X'` | Нет `IWorkflowAction` с `Name == "X"` в DI. Проверьте `[Export(..., typeof(IWorkflowAction))]` и уникальность `Name`. |
| Действие исполнилось дважды | At-least-once шины + действие неидемпотентно. Дедуп защищает запуск правила, но не внешний side-effect — делайте действие идемпотентным. |
| Параметр пустой/`null` в действии | Встроенная подстановка (`"text {{trigger.X}}"`) не поддерживается — используйте whole-string `"{{trigger.X}}"`; либо переменной нет в payload. |
| `Enable` бросает исключение | У правила нет триггеров или действий — добавьте их перед включением. |

---

## 17. Ограничения и follow-up

Текущая реализация — рабочее ядро (монолит). В follow-up:

- **Серверный `registry/sync`** + хранение каталога триггеров/действий (клиентская часть готова);
- **`BusActionExecutor`/`HttpActionExecutor`** — сейчас только `InProcActionExecutor` (монолит);
- **cron-триггеры** (`ScheduleTriggerTask` + `IDistributedLock`) — `Schedule`-привязка хранится, но не исполняется;
- **кэш-индекс** `eventName → ruleIds` (`ICacheService`) — сейчас прямой запрос в БД;
- **Saga для `Compensate`** — откат многошаговых действий;
- **вычислимые/встроенные шаблоны параметров** (сейчас whole-string);
- **gRPC** для registry/evaluate; транзакционный **Outbox** для публикации действий.
