# Cheetah.AspNetCore.Contracts

ASP.NET-привязка контрактов из [Cheetah.Contracts](../Cheetah.Contracts/README.md): model binder для `GridRequest` из query string и хелперы смешанной привязки route + body. Зависит от `Cheetah.Core`, `CrmContractsModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `GridRequestModelBinder` / `GridRequestModelBinderProvider` | Парсит `GridRequest` из query string |
| `GridRequestExtensions` | Хелперы для работы с `GridRequest` в ASP.NET |
| `RouteBodyBindingExtensions` | `BindBodyWithRouteAsync<T>` / `MergeRouteValuesInto<T>` — склейка JSON-body с route-значениями |
| `CrmAspNetCoreContractsModule` | Модуль; вставляет grid-binder в начало `MvcOptions.ModelBinderProviders` |

## Grid из query string

Поддерживаются форматы Kendo UI (`sort[0][field]=Name`) и PrimeNG (`sort[0].field=Name`), включая вложенные фильтры:

```
?page=1&pageSize=10
&sort[0][field]=Name&sort[0][dir]=asc
&filter[logic]=and&filter[filters][0][field]=Name&filter[filters][0][operator]=contains&filter[filters][0][value]=test
```

Значения фильтров типизируются автоматически (int/long/decimal/double/bool/DateTime/Guid → иначе string).

## Route + Body

Генерируемые update/patch-endpoint'ы используют request-record, где часть полей приходит из маршрута (`[FromRoute] Id`), а часть — из JSON-body:

```csharp
var request = await context.BindBodyWithRouteAsync<UpdateOrderRequest>(ct);
// или, если тело уже связано [FromBody] (ради примера в OpenAPI/Scalar):
var merged = context.MergeRouteValuesInto(bodyRequest);
```
