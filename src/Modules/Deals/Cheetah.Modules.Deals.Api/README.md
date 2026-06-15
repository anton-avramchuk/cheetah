# Cheetah.Modules.Deals.Api

HTTP-слой Deals. Эндпоинты описаны **декларативно** классами в `Endpoints/` (наследники
`Cheetah.Backend.Endpoints`); их регистрацию генерирует `Cheetah.Generators.Endpoints`.
Реквест → команда/запрос — через Mapster-профиль (`DealsMappingProfile`).

## Эндпоинты

| Метод | Маршрут | CQRS |
|---|---|---|
| POST | `api/deals` | `CreateDealCommand` |
| GET | `api/deals/{dealId}` | `GetDealByIdQuery` |
| GET | `api/deals` | `ListDealsQuery` |
| GET | `api/deals/board?pipelineId=` | `GetDealBoardQuery` |
| GET | `api/deals/{dealId}/history` | `GetDealHistoryQuery` |
| POST | `api/deals/{dealId}/stage` | `ChangeDealStageCommand` |
| POST | `api/deals/{dealId}/win` | `WinDealCommand` |
| POST | `api/deals/{dealId}/lose` | `LoseDealCommand` |
| POST | `api/deals/{dealId}/owner` | `AssignDealOwnerCommand` |
| POST | `api/pipelines` | `CreatePipelineCommand` |
| GET | `api/pipelines/{pipelineId}` | `GetPipelineByIdQuery` |
| GET | `api/pipelines` | `ListPipelinesQuery` |
| POST | `api/pipelines/{pipelineId}/stages` | `AddPipelineStageCommand` |

> Литеральный `api/deals/board` имеет приоритет над `api/deals/{dealId:guid}` за счёт constraint'а.

## Зависимости

`CrmAspNetCoreModule`, `CrmMapsterModule`, `CrmBackendEndpointsModule`, Application, Contracts +
генераторы модулей/эндпоинтов.
