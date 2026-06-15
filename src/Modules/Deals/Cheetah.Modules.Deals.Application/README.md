# Cheetah.Modules.Deals.Application

Прикладной слой Deals: CQRS-команды/запросы, конфигурация конечного автомата статуса и проекция
в DTO. Зависит **только на Domain** (+ Contracts, DomainEvents, Core.CQRS/Events/StateMachine) —
не на Infrastructure.

## Команды

| Команда | Результат |
|---|---|
| `CreateDealCommand` | `Guid` (старт на первой Open-стадии, событие `DealCreated`) |
| `ChangeDealStageCommand` | void (доменный `MoveToStage`) |
| `WinDealCommand` / `LoseDealCommand` | void (валидация перехода через StateMachine) |
| `AssignDealOwnerCommand` | void |
| `CreatePipelineCommand` / `AddPipelineStageCommand` | `Guid` |

## Запросы

`GetDealByIdQuery`, `ListDealsQuery` (фильтр + пагинация), `GetDealHistoryQuery`,
`GetDealBoardQuery` (Kanban-агрегаты), `GetPipelineByIdQuery`, `ListPipelinesQuery`.

## StateMachine

Регистрируется в `ConfigureServices`:

```csharp
services.AddStateMachine<DealStatus>(sm => sm
    .From(DealStatus.Open).To(DealStatus.Won, DealStatus.Lost)
    .From(DealStatus.Won).To(DealStatus.Open)
    .From(DealStatus.Lost).To(DealStatus.Open));
```

## Конвенции

Хендлеры на `[Export(LifetimeType.Scoped, …)]`, `ValueTask` + `CancellationToken`. События
публикуются через `IEventBus` (Outbox) перед `SaveChangesAsync`. Проекция в DTO — `DealProjector`
(без Mapster: owned-`Money` и коллекции раскрываются явно).
