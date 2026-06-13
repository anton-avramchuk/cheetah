# Cheetah.Generators.ApiClient

Incremental source generator: по request-типам с `[ApiRoute]` (из [Cheetah.Contracts](../Cheetah.Contracts/README.md)) генерирует типизированный HTTP-клиент для server-to-server интеграции между .NET-модулями.

Подключается как анализатор:
```xml
<ProjectReference Include="..\..\..\Cheetah.Generators.ApiClient\Cheetah.Generators.ApiClient.csproj"
                  ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
```

## Что генерирует

Для `partial`-класса-модуля с `[GenerateApiClient("Orders")]` собирает все request-типы с `[ApiRoute(route, ApiMethod, ...)]` из подключённых сборок и генерирует `IOrdersService` / `OrdersService` поверх `HttpClient`.

```csharp
[GenerateApiClient("Orders")]
public partial class OrdersClientModule : CrmModule { }

[ApiRoute("api/orders/{id:guid}", ApiMethod.Get, ResponseType = typeof(OrderResponse))]
public sealed record GetOrderRequest([property: FromRoute] Guid Id) : ICrmRequest;
// → IOrdersService.GetOrderAsync(Guid id) → GET api/orders/{id}
```

## Поведение

- HTTP-метод и форма результата выводятся из `ApiMethod`: `Get`/`GetOrNotFound`/`GetCollection`/`GetGrid`, `Post`/`PostWithResult`/`Create`, `Update`/`UpdateWithResult`/`Patch`, `Delete`.
- `[FromRoute]`-параметры подставляются в шаблон маршрута, остальные уходят в body/query.
- `GetGrid` принимает `GridRequest` и возвращает `GridResult<TResponse>`.
- `ServiceName` на `[ApiRoute]` позволяет разнести маршруты по нескольким сервисам (`IVacanciesService` и т.п.); иначе берётся имя из `[GenerateApiClient]`.
- `MethodName` снимает неоднозначность, когда два маршрута дают одно имя метода.

> Это server-to-server клиент для .NET-модулей. Angular-фронтенд общается с REST API напрямую и в этом клиенте не нуждается.

## Диагностики

- **`APIGEN001`** (warning) — не найдено ни одного типа с `[ApiRoute]` в подключённых сборках.
