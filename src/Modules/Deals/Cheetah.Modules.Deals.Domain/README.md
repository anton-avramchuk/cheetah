# Cheetah.Modules.Deals.Domain

Доменный слой Deals: агрегаты, value-логика, спецификации и интерфейсы репозиториев. Зависит на
`Core.Domain`, `Core.DataAccess`, `Core.Specification`, `Core.StateMachine`, `…Shared`, `…DomainEvents`.

## Агрегаты и сущности

| Тип | Базовый | Роль |
|---|---|---|
| `Pipeline` | `AggregateRoot<Guid>` | Воронка; стадии — child-entities в её границе |
| `PipelineStage` | `Entity<Guid>` | Стадия: `Order`, `Probability` (0..100), `Type` |
| `Deal` | `AggregateRoot<Guid>`, `IStateMachineEntity<DealStatus>` | Сделка: `Money Value`, привязки `CustomerId`/`OwnerId`, статус |
| `DealStageHistory` | `Entity<Guid>` | Запись перехода между стадиями (для метрик) |

**Инварианты `Deal`:** старт на первой Open-стадии; нельзя менять стадию у закрытой; нельзя выиграть
без положительной суммы; `Lose` требует причину; `Reopen` сбрасывает `ClosedAt`/`LostReason`.
Терминализация по статусу — через StateMachine; перемещение по стадиям — доменно (`MoveToStage`).

## Репозитории (интерфейсы)

| Интерфейс | Назначение |
|---|---|
| `IPipelineRepository` | `GetWithStagesAsync`/`GetDefaultWithStagesAsync`/`ListWithStagesAsync` — загрузка стадий (базовый `GetByIdAsync` их не тянет) |
| `IDealRepository` | `GetOpenBoardAsync` (агрегаты доски), `ListAsync` (пагинация) |

Реализация — в Infrastructure. Фильтрация — только через спецификации
(`DealsFilterSpecification`, `OpenDealsByOwnerSpecification`, …); raw LINQ в хендлерах запрещён.
