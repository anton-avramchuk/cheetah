# Cheetah.Contracts

Общие контракты транспортного слоя: маркеры запросов/ответов, модель грида (фильтр/сортировка/пагинация) и атрибуты для генерации API-клиентов и endpoint'ов. Зависит только от `Cheetah.Core`.

## Состав

| Область | Типы | Назначение |
|---------|------|------------|
| Маркеры | `ICrmRequest`, `ICrmResponse` | Базовые маркеры DTO запроса/ответа |
| Grid | `GridRequest` (`Page`, `PageSize`, `Sort`, `Filter`), `FilterDescriptor`, `SortDescriptor`, `GridResult<T>` (`Data`, `Total`) | Серверная фильтрация/сортировка/пагинация (см. [Cheetah.Core.Grid](../Cheetah.Core.Grid/README.md)) |
| Grid helpers | `GridRequestUrlBuilder` | Сборка query string грида (для клиентов/тестов) |
| Codegen | `[GenerateApiClient(serviceName)]`, `[ApiRoute(route, ApiMethod)]`, `ApiMethod` (enum), `[FromRoute]`/`[FromBody]`/`[FromQuery]` | Метаданные для генераторов API-клиента и endpoint'ов |

## Grid

`FilterDescriptor` — рекурсивная структура (AND/OR через `Logic` + `Filters`, либо лист `Field`/`Operator`/`Value`). Операторы: `eq`, `neq`, `contains`, `startswith`, `endswith`, `gt`, `gte`, `lt`, `lte`, `isnull`, `isnotnull`, `isempty`, `isnotempty`. Привязка из query string — в [Cheetah.AspNetCore.Contracts](../Cheetah.AspNetCore.Contracts/README.md).

## Атрибуты генерации

Помечают request-типы маршрутом и HTTP-семантикой; по ним [Cheetah.Generators.ApiClient](../Cheetah.Generators.ApiClient/README.md) и [Cheetah.Generators.Endpoints](../Cheetah.Generators.Endpoints/README.md) генерируют клиент и регистрацию endpoint'ов.

```csharp
[GenerateApiClient("Orders")]
public partial class OrdersApiModule : CrmModule { }

[ApiRoute("api/orders/{id:guid}", ApiMethod.Get, ResponseType = typeof(OrderResponse))]
public sealed record GetOrderRequest([property: FromRoute] Guid Id) : ICrmRequest;
```

`ApiMethod` различает семантику: `Get`/`GetOrNotFound`/`GetCollection`/`GetGrid`, `Post`/`PostWithResult`/`Create`, `Update`/`UpdateWithResult`/`Patch`, `Delete`.
