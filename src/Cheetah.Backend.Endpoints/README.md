# Cheetah.Backend.Endpoints

Декларативные типобезопасные endpoint'ы поверх Minimal API. Endpoint — это **чистый контейнер метаданных** (метод, маршрут, авторизация, кэш, rate-limit); код регистрации (`MapGet/MapPost` → маппинг → `dispatcher` → результат) генерирует Source Generator [Cheetah.Generators.Endpoints](../Cheetah.Generators.Endpoints/README.md). Зависит от `Cheetah.Contracts`, `Cheetah.Core.CQRS`.

## Состав

| Тип | Назначение |
|-----|------------|
| `ICrmEndpoint<TRequest, TResponse>`, `IEndpointDefinition` | Контракты endpoint'а |
| `EndpointBase<TRequest, TResponse>` | База: метаданные (`Method`, `Route`, теги, авторизация, кэш, rate-limit) |
| `CommandEndpoint<TRequest, TCommand>` | POST, команда без результата → 204 |
| `CommandWithResultEndpoint<...>` | POST, команда с результатом → 200 |
| `CreateCommandEndpoint<TRequest, TCommand>` | POST создание → 201 + Location (`GetByIdRouteName`) |
| `UpdateEndpoint`, `DeleteEndpoint` | PUT/DELETE |
| `QueryEndpoint<...>` | GET → 200 |
| `QueryOrNotFoundEndpoint<...>` | GET → 404, если null |
| `QueryCollectionEndpoint<...>` | GET коллекции |
| `QueryGridEndpoint<...>` | GET грида (фильтр/сортировка/пагинация, см. [Cheetah.Core.Grid](../Cheetah.Core.Grid/README.md)) |
| `EndpointConfiguration` | Fluent-настройка: `WithName`, `WithTags`, `RequireAuthorization`, `RequirePermissions`, `AllowAnonymousAccess`, `WithCacheControl`, `WithRateLimit`, `MarkAsDeprecated` |
| `RateLimitFilter`, `RateLimitSettings`, `BrowserCacheSettings` | Per-endpoint rate-limit и Cache-Control |
| `EmptyResponse`, `GuidResponse` | Стандартные ответы |

## Описание endpoint'а

```csharp
public sealed class CreateOrderEndpoint : CreateCommandEndpoint<CreateOrderRequest, CreateOrderCommand>
{
    public override string Route => "/api/orders";
    public override string GetByIdRouteName => "GetOrderById";

    protected override void Configure(EndpointConfiguration config) => config
        .WithName("CreateOrder")
        .WithTags("Orders")
        .RequirePermissions("orders.create")
        .WithRateLimit(limit: 10, window: TimeSpan.FromMinutes(1));
}
```

Поведение endpoint'а полностью определяется типом CQRS-команды/запроса — метода `HandleAsync` писать не нужно. Source Generator по этим метаданным генерирует регистрацию маршрута.

## Привязка маршрута и тела

- **GET/DELETE** — `[AsParameters]`: поля DTO биндятся из маршрута (по имени) и query-строки.
- **POST/PUT/PATCH** — `[FromBody]` + слияние маршрутных значений: параметры конструктора DTO, помеченные `[FromRoute]` (`Cheetah.Contracts.Attributes`), подставляются из пути. Тело их не несёт, поэтому один DTO покрывает и `{id}` в пути, и поля тела.
- **POST-команды** (`CommandEndpoint`, `CommandWithResultEndpoint`) допускают **пустое тело** (`EmptyBodyBehavior.Allow`) — подэкшены вида `POST /events/{id}/cancel` без тела не падают с 400.

```csharp
// POST api/calendar-events/{eventId}/reschedule — id из пути, остальное из тела
public sealed record RescheduleEventRequest(
    [property: FromRoute] Guid EventId,
    DateTime StartUtc, DateTime EndUtc) : ICrmRequest;
```

## Rate-limit

- **Named-policy** (`WithRateLimit("policyName")`) — лимиты резолвятся из `RateLimit:Policies:<policyName>` в конфигурации (ops крутят без передеплоя).
- **Inline** (`WithRateLimit(limit, window)`) — лимит как бизнес-правило прямо в коде.
