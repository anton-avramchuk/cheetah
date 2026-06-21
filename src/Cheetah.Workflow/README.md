# Cheetah.Workflow (абстракция)

Лёгкая инфраструктурная сборка с **контрактами расширения** модуля автоматизации Workflow. Без БД и без
движка — движок исполняется централизованно в бизнес-модуле `Cheetah.Modules.Workflow.*`. Эту сборку
подключает любой модуль-**контрибутор**, который поставляет действия/триггеры Workflow или зависит от
конверта событий.

## Что внутри

| Тип | Назначение |
|---|---|
| `WorkflowEventEnvelope` | нормализованный «конверт» любого интеграционного события для движка правил (firehose). `SourceEventId` — id исходного события (дедуп) |
| `IWorkflowAction` / `WorkflowActionContext` | plugin-действие (главная точка расширения). Контрибутор регистрирует через `[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]` |
| `WorkflowActionContextExtensions` | хелперы чтения параметров действия: `GetString`/`GetGuid`/`GetInt`/`GetBool` (скрывают `JsonElement` vs литерал) |
| `IWorkflowActionExecutor` | порт-диспетчер: по `actionType` выбирает транспорт (in-proc / шина / HTTP) и исполняет действие. Движок зависит только от этого порта |
| `IWorkflowTrigger` / `WorkflowTriggerSink` | plugin-триггер (опциональная ось) для нестандартных источников сверх Event/Schedule |
| `TriggerDescriptor` / `ActionDescriptor` / `ActionParameterDescriptor` / `ActionTransport` | дескрипторы реестра — контрибутор декларирует свои события и операции при старте (как Permissions.Catalog/Tags) |
| `CrmWorkflowModule` | модуль абстракции (`DependsOn` Core, Core.Events, Expressions.JsonLogic) |

## Модель интеграции (важно)

Workflow — **не RPC-сервис**, а центральный реактивный консьюмер событий + диспетчер действий:

- **Триггеры** идут *в* Workflow через шину (источник публикует событие, форвардер дублирует его в
  firehose-канал как `WorkflowEventEnvelope`; источник о Workflow не знает);
- **Действия** идут *из* Workflow наружу через `IWorkflowActionExecutor` (in-proc плагин в монолите /
  команда-событие по шине в микросервисе);
- «Ходят к Workflow» только при **регистрации** триггеров/действий на старте (`registry/sync`).

## Зависимости

`Cheetah.Core`, `Cheetah.Core.Events`, `Cheetah.Expressions.JsonLogic`.

Полный план и код-уровневое описание — [`docs/modules/workflow.md`](../../docs/modules/workflow.md).
