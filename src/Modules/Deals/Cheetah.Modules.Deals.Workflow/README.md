# Cheetah.Modules.Deals.Workflow — адаптер действий Deals для Workflow

**Образец модуля-контрибутора:** показывает, как любой бизнес-модуль приносит свои «активити» (действия)
в [Workflow](../../Workflow/README.md), **не трогая ядро Workflow и не создавая обратных зависимостей**.

## Принцип

Адаптер зависит **только на абстракцию** `Cheetah.Workflow` и на собственный прикладной слой
`Cheetah.Modules.Deals.Application`. Направление зависимостей всегда:

```
Cheetah.Modules.Deals.Workflow ──▶ Cheetah.Workflow (абстракция)
                               └──▶ Cheetah.Modules.Deals.Application (свои команды)
```

Ядро Workflow о Deals ничего не знает — оно видит действия как `IWorkflowAction` через DI.

## Что внутри

**Действия** (`Actions/DealWorkflowActions.cs`) — каждое помечено `[Export(LifetimeType.Scoped, typeof(IWorkflowAction))]`
и вызывает доменную команду Deals через `IDispatcher`:

| `ActionType` | Параметры | Команда |
|---|---|---|
| `ChangeDealStage` | `dealId`, `toStageId`, `changedBy?` | `ChangeDealStageCommand` |
| `WinDeal` | `dealId`, `changedBy?` | `WinDealCommand` |
| `AssignDealOwner` | `dealId`, `newOwnerId` | `AssignDealOwnerCommand` |

**Дескрипторы** (`DealsWorkflowDescriptors`) — триггеры (события Deals) и действия для каталога Workflow
(чтобы админка/UI знали, из чего собирать правила).

## Подключение

### 1. Действия (обязательно)

Добавьте зависимость от модуля адаптера в граф модулей хоста:

```csharp
[DependsOn(
    typeof(CheetahWorkflowDefaultModule),   // сам Workflow
    typeof(CheetahDealsWorkflowModule)      // действия Deals → доступны правилам
)]
public class AppModule : CrmModule { /* … AddWorkflowFirehose() в PostConfigureServices … */ }
```

Всё — теперь в правиле можно указывать `"actionType": "ChangeDealStage"`. `InProcActionExecutor`
подхватит действие из DI автоматически.

### 2. Каталог для админки (опционально)

Если нужен видимый в UI список триггеров/действий Deals — задекларируйте их в каталоге через клиент
Workflow (на стороне хоста / контрибутора):

```csharp
services
    .AddWorkflowClient(o => o.BaseUrl = cfg["Workflow:Url"])   // в монолите — собственный адрес
    .RegisterTriggers(DealsWorkflowDescriptors.Triggers)
    .RegisterActions(DealsWorkflowDescriptors.Actions);
```

> Серверный приём `registry/sync` — follow-up Workflow; до него декларация безопасна (`ContinueOnFailure`)
> и полезна как самодокументирование. На исполнение действий каталог не влияет — оно идёт через `[Export]`.

## Пример правила

«Когда КП принято (`QuoteAcceptedIntegrationEvent` от SalesDocuments) — выиграть связанную сделку»:

```jsonc
{
  "name": "КП принято → сделка выиграна",
  "ownerService": "Deals",
  "triggers": [ { "triggerType": "Event", "triggerKey": "QuoteAcceptedIntegrationEvent" } ],
  "actions": [
    { "order": 0, "actionType": "WinDeal", "failureMode": "StopRule",
      "parameters": "{ \"dealId\": \"{{trigger.DealId}}\" }" }
  ]
}
```

## Как повторить для своего модуля

1. Создайте сборку `Cheetah.Modules.{Имя}.Workflow`, сославшись на `Cheetah.Workflow` + свой `*.Application`.
2. Реализуйте `IWorkflowAction` (+ `[Export(..., typeof(IWorkflowAction))]`), вызывая свои команды через `IDispatcher`.
   Параметры читайте через `context.GetString/GetGuid/GetInt/GetBool`.
3. (Опц.) Объявите `…WorkflowDescriptors` с `TriggerDescriptor`/`ActionDescriptor`.
4. Модуль-класс `Cheetah{Имя}WorkflowModule : CrmModule` с `[DependsOn(CoreModule, CrmCQRSCoreModule, CrmWorkflowModule, {Имя}ApplicationModule)]` и `RegisterServices(...)`.
