# Cheetah.Modules.Deals.Contracts

DTO (ответы, `ICrmResponse`) и Request-ы (`ICrmRequest`) HTTP-слоя Deals. Зависит на
`Cheetah.Contracts` + `Cheetah.Core` + `…Shared`.

## Состав

| Группа | Типы |
|---|---|
| Ответы | `DealDto`, `DealListItemDto`, `DealHistoryDto`, `PipelineDto`, `PipelineStageDto`, `BoardDto`, `BoardColumnDto` |
| Сделки (запросы) | `CreateDealRequest`, `GetDealByIdRequest`, `ListDealsRequest`, `ChangeDealStageRequest`, `WinDealRequest`, `LoseDealRequest`, `AssignDealOwnerRequest`, `GetDealHistoryRequest`, `GetDealBoardRequest` |
| Воронки (запросы) | `CreatePipelineRequest`, `GetPipelineByIdRequest`, `ListPipelinesRequest`, `AddPipelineStageRequest` |

Маршрутные поля помечены `[FromRoute]` (`DealId`, `PipelineId`). Маппинг Request → CQRS-команда/запрос
выполняет Mapster-профиль в слое Api.

## Зависимости

`Cheetah.Contracts`, `Cheetah.Core`, `Cheetah.Modules.Deals.Shared`.
