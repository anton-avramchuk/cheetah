# Cheetah.Core.Diagnostic.AspNetCore

ASP.NET Core расширение для [`Cheetah.Core.Diagnostic`](../Cheetah.Core.Diagnostic/README.md).

Добавляет два инструмента:

| Инструмент | Механизм | Что измеряет |
|---|---|---|
| `TimingEndpointFilter` | `IEndpointFilter` | Время выполнения эндпоинтов, помеченных `[MeasureTime]` |
| `RequestLoggingMiddleware` | Middleware | Все входящие запросы и статусы ответов |

---

## Подключение

`CrmCoreDiagnosticAspNetCoreModule` транзитивно включает `CrmCoreDiagnosticModule` — добавлять оба не нужно:

```csharp
[DependsOn(
    typeof(CrmCoreDiagnosticAspNetCoreModule),
    typeof(MyApplicationModule),
    typeof(MyApiModule)
)]
public partial class MyBootstrapperModule : CrmModule { }
```

## Конфигурация

```json
{
  "Diagnostics": {
    "Enabled": true,
    "RequestLogging": {
      "Enabled": true,
      "LogBody": true,
      "MaxBodyBytes": 4096,
      "LogQueryString": true,
      "LogResponseStatus": true,
      "ExcludePaths": ["/health", "/metrics", "/swagger"]
    }
  }
}
```

`Diagnostics.Enabled` — мастер-выключатель из базового модуля. При `false` отключается всё, включая `RequestLogging`.

### Поля RequestLogging

| Поле                | Тип        | По умолчанию | Описание                                                                                    |
|---------------------|------------|:------------:|---------------------------------------------------------------------------------------------|
| `Enabled`           | `bool`     | `true`       | Включить/выключить логирование запросов независимо от таймингов эндпоинтов.                 |
| `LogBody`           | `bool`     | `true`       | Логировать тело запроса. Отключи для бинарных загрузок или чувствительных данных.           |
| `MaxBodyBytes`      | `int`      | `4096`       | Максимальный размер тела в байтах. Превышение усекается с пометкой `[truncated]`.           |
| `LogQueryString`    | `bool`     | `true`       | Включать query-параметры в лог. Отключи если в параметрах могут быть токены или пароли.    |
| `LogResponseStatus` | `bool`     | `true`       | Логировать статус-код ответа после выполнения хендлера.                                     |
| `ExcludePaths`      | `string[]` | `[]`         | Префиксы путей для исключения. `/health` подавляет также `/health/live`, `/health/ready`.  |

---

## TimingEndpointFilter

Атрибут `[MeasureTime]` вешается на лямбду или метод-хендлер:

```csharp
routeBuilder.MapPost("/api/orders",
    [MeasureTime("создание заказа")]
    async ([FromBody] CreateOrderRequest request,
           [FromServices] IDispatcher dispatcher,
           CancellationToken ct) =>
    {
        var id = await dispatcher.SendAsync(mapper.Map<CreateOrderCommand>(request), ct);
        return Results.Created($"/api/orders/{id}", id);
    })
    .WithName("CreateOrder")
    .WithOpenApi();

routeBuilder.MapGet("/api/orders/{id:guid}",
    [MeasureTime]
    async ([FromRoute] Guid id, [FromServices] IDispatcher dispatcher, CancellationToken ct) =>
    {
        var vm = await dispatcher.QueryAsync(new GetOrderByIdQuery(id), ct);
        return vm is null ? Results.NotFound() : Results.Ok(vm);
    });
```

ASP.NET Core автоматически помещает атрибуты лямбды в метаданные эндпоинта. `TimingEndpointFilter` проверяет их при каждом запросе — эндпоинты без `[MeasureTime]` пропускаются без оверхеда.

Лог-сообщения используют `DisplayName` эндпоинта (задаётся через `.WithName()`):

```
[Diagnostic] (создание заказа) HTTP POST /api/orders completed in 38ms
[Diagnostic] HTTP GET /api/orders/{id} completed in 5ms
[Diagnostic] (создание заказа) HTTP POST /api/orders threw after 12ms
```

### Как работает

1. В `OnApplicationInitialization` модуль получает `IEndpointRouteBuilder` из `ObjectAccessor`.
2. Создаёт `MapGroup("")` (пустой префикс — пути не меняются) с навешанным `TimingEndpointFilter`.
3. Заменяет значение в `ObjectAccessor` на этот группированный билдер.
4. Все последующие модули получают обёрнутый билдер — их эндпоинты автоматически наследуют фильтр.

---

## RequestLoggingMiddleware

Логирует каждый HTTP-запрос и ответ. Работает независимо от `[MeasureTime]`.

Пример вывода:

```
info: RequestLoggingMiddleware
      HTTP POST /api/orders?source=web | Body: {"productId":"...","qty":2}
info: RequestLoggingMiddleware
      HTTP POST /api/orders => 201

info: RequestLoggingMiddleware
      HTTP GET /api/orders/abc | Body: (empty)
info: RequestLoggingMiddleware
      HTTP GET /api/orders/abc => 200
```

Пример усечённого тела (`MaxBodyBytes: 4096`):

```
info: RequestLoggingMiddleware
      HTTP POST /api/import | Body: [{"id":1,"name":"foo"},{"id":2,... [truncated — 98432 bytes total]
```

### Как работает

1. Middleware регистрируется после `UseRouting()`.
2. При каждом запросе проверяет `Diagnostics.Enabled`, `RequestLogging.Enabled` и `ExcludePaths`.
3. Вызывает `EnableBuffering()` и читает не более `MaxBodyBytes` байт тела; сбрасывает позицию стрима — хендлер получает полное тело как обычно.
4. После выполнения хендлера логирует статус-код ответа (если `LogResponseStatus: true`).

---

## Фильтрация логов

```json
{
  "Logging": {
    "LogLevel": {
      "Cheetah.Core.Diagnostic.AspNetCore": "Information"
    }
  }
}
```
