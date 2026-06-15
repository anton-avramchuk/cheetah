# Cheetah.Modules.Deals

Сердце CRM: **сделки** (opportunities) и **воронки продаж**. Сделка движется по настраиваемым
стадиям воронки, имеет сумму (`Money`), ответственного и статус с конечным автоматом
(`Open → Won|Lost`); все переходы фиксируются в истории для метрик.

Полное проектное описание: [`docs/modules/deals.md`](../../../docs/modules/deals.md).

## Сборки

| Сборка | Назначение |
|---|---|
| [`…DomainEvents`](Cheetah.Modules.Deals.DomainEvents/README.md) | События `DealCreated/StageChanged/Won/Lost/OwnerChanged` |
| [`…Shared`](Cheetah.Modules.Deals.Shared/README.md) | Enum'ы (`DealStatus`, `StageType`, `TriggerSource`), value object `Money`, константы |
| [`…Contracts`](Cheetah.Modules.Deals.Contracts/README.md) | DTO/Request; полиморфная привязка сделки — ключ `crm.deal` |
| [`…Domain`](Cheetah.Modules.Deals.Domain/README.md) | Агрегаты (`Pipeline`+`PipelineStage`, `Deal`+`DealStageHistory`), спецификации, репозитории-интерфейсы |
| [`…Infrastructure`](Cheetah.Modules.Deals.Infrastructure/README.md) | EF Core `DealsDbContext`, конфигурации (owned `Money`), репозитории, миграции, Outbox |
| [`…Application`](Cheetah.Modules.Deals.Application/README.md) | CQRS (сделки/воронки/доска), конфиг StateMachine `DealStatus` |
| [`…Api`](Cheetah.Modules.Deals.Api/README.md) | Minimal API на декларативных эндпоинтах + генератор (`/api/deals/**`, `/api/pipelines/**`) |
| [`…Client`](Cheetah.Modules.Deals.Client/README.md) | HTTP-клиент для server-to-server (например, конвертация Lead → Deal) |

## Жизненный цикл сделки

1. `Deal.Create(pipeline, …)` ставит сделку на первую **Open**-стадию воронки и публикует
   `DealCreatedIntegrationEvent`.
2. `MoveToStage(target)` перемещает по Open-стадиям (пишет `DealStageHistory`). Попадание на стадию
   типа **Won** терминализует сделку; для **Lost** нужен явный `Lose(reason)`.
3. Терминальные переходы (`Open → Won|Lost`, reopen `Won|Lost → Open`) валидируются через
   `Cheetah.Core.StateMachine` (`IStateMachineValidator<DealStatus>`); внутристадийные перемещения —
   доменно (стадии это данные, не enum).
4. Интеграционные события публикуются через **Outbox** того же `DealsDbContext` (атомарно с
   сохранением агрегата).

## Производительность

- Kanban-доска и списки — проекции `AsNoTracking`; суммы по стадиям — агрегатным запросом
  (`GroupBy` + `Sum`) в `IDealRepository.GetOpenBoardAsync`.
- Индексы под горячие пути: `(OwnerId, Status)`, `(PipelineId, StageId)`, `(CustomerId)`,
  `(ExpectedCloseDate)`.

## Тесты

`Domain.Tests` (инварианты `Deal`/`Pipeline`/`Money`), `Application.Tests` (хендлеры на Moq:
публикация событий, board-агрегаты, StateMachine), `Client.Tests` (HTTP-клиент).
