# Cheetah.Core.Diagnostic

Модуль для измерения времени выполнения методов через атрибут `[MeasureTime]`.

Пакет содержит два модуля:

| Модуль | Назначение |
|---|---|
| `CrmCoreDiagnosticModule` | Замер времени **DI-сервисов** через `DispatchProxy` |
| `CrmCoreDiagnosticAspNetCoreModule` | Замер времени **HTTP-эндпоинтов** через `IEndpointFilter` |

Оба используют один и тот же атрибут `[MeasureTime]` и секцию `Diagnostics` в конфиге.

---

## Подключение

### Только сервисы

Добавь зависимость на `CrmCoreDiagnosticModule` в bootstrapper-модуль своего сервиса:

```csharp
[DependsOn(
    typeof(CrmCoreDiagnosticModule),
    typeof(MyApplicationModule)
)]
public partial class MyBootstrapperModule : CrmModule { }
```

### Сервисы + эндпоинты (ASP.NET Core)

```csharp
[DependsOn(
    typeof(CrmCoreDiagnosticAspNetCoreModule), // включает CrmCoreDiagnosticModule транзитивно
    typeof(MyApplicationModule),
    typeof(MyApiModule)
)]
public partial class MyBootstrapperModule : CrmModule { }
```

> `CrmCoreDiagnosticAspNetCoreModule` уже транзитивно зависит от `CrmCoreDiagnosticModule`, добавлять оба не нужно.

## Конфигурация

В `appsettings.json` добавь секцию `Diagnostics`:

```json
{
  "Diagnostics": {
    "Enabled": true
  }
}
```

| Поле      | Тип    | По умолчанию | Описание                                       |
|-----------|--------|:------------:|------------------------------------------------|
| `Enabled` | `bool` | `true`       | Глобальный включатель/выключатель логирования  |

При `Enabled: false` атрибуты `[MeasureTime]` игнорируются и замеры не производятся.

## Использование атрибута

### На сервисах

Пометь нужные методы атрибутом `[MeasureTime]` на классе-реализации:

```csharp
public class OrderService : IOrderService
{
    [MeasureTime]
    public async ValueTask<OrderViewModel> GetByIdAsync(Guid id, CancellationToken ct)
    {
        // ...
    }

    [MeasureTime("создание заказа")]
    public async ValueTask<Guid> CreateAsync(CreateOrderCommand command, CancellationToken ct)
    {
        // ...
    }
}
```

Опциональный параметр `label` добавляется в лог-сообщение для удобной идентификации:

```
[Diagnostic] (создание заказа) OrderService.CreateAsync completed in 42ms
[Diagnostic] OrderService.GetByIdAsync completed in 7ms
```

При выбросе исключения логируется `Warning`:

```
[Diagnostic] (создание заказа) OrderService.CreateAsync threw after 13ms
```

### На эндпоинтах

Атрибут вешается непосредственно на лямбду или метод-хендлер:

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

ASP.NET Core автоматически помещает атрибуты лямбды в метаданные эндпоинта, откуда `TimingEndpointFilter` их читает.

Лог-сообщения используют `DisplayName` эндпоинта (заданный через `.WithName()`):

```
[Diagnostic] (создание заказа) HTTP POST /api/orders completed in 38ms
[Diagnostic] HTTP GET /api/orders/{id} completed in 5ms
```

---

## Требования к сервису

Модуль использует `DispatchProxy` для перехвата вызовов, поэтому:

- Сервис **должен быть зарегистрирован через интерфейс** — `[Export(LifetimeType.Scoped, typeof(IOrderService))]`
- Атрибут `[MeasureTime]` ставится на метод **класса-реализации** (или на метод интерфейса — оба варианта поддерживаются)
- Поддерживаемые типы возвращаемых значений: `void`, `T`, `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`

```csharp
// ✅ Правильно — зарегистрирован через интерфейс
[Export(LifetimeType.Scoped, typeof(IOrderService))]
public class OrderService : IOrderService
{
    [MeasureTime]
    public async ValueTask<Guid> CreateAsync(...) { ... }
}

// ❌ Не будет проксирован — нет интерфейса в регистрации
[Export(LifetimeType.Scoped)]
public class OrderService
{
    [MeasureTime]
    public async ValueTask<Guid> CreateAsync(...) { ... }
}
```

## Как работает

### Сервисы — DispatchProxy

1. При старте приложения (`PostConfigureServices`) модуль сканирует все зарегистрированные сервисы.
2. Для каждого сервиса с реализацией, у которой есть хотя бы один метод с `[MeasureTime]`, исходный дескриптор заменяется на фабрику, создающую `DispatchProxy`-обёртку.
3. Proxy при каждом вызове метода:
   - проверяет наличие `[MeasureTime]` и флаг `Enabled`;
   - запускает `Stopwatch`;
   - для async-методов ждёт завершения задачи, затем останавливает таймер;
   - пишет `Information`-лог при успехе или `Warning`-лог при исключении.

### Эндпоинты — IEndpointFilter

1. В `OnApplicationInitialization` модуль получает текущий `IEndpointRouteBuilder` из `ObjectAccessor`.
2. Создаёт `MapGroup("")` (пустой префикс — пути не меняются) с навешанным `TimingEndpointFilter`.
3. Заменяет значение в `ObjectAccessor` на этот группированный билдер.
4. Все последующие модули, вызывающие `context.GetRouteBuilder()`, получают обёрнутый билдер и их эндпоинты автоматически наследуют фильтр.
5. `TimingEndpointFilter` при каждом запросе читает `MeasureTimeAttribute` из метаданных эндпоинта — если атрибута нет, пропускает без замера.

Логи пишутся через стандартный `ILogger<T>`, поэтому фильтрация настраивается обычным способом через `Logging` в `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Cheetah.Core.Diagnostic": "Information"
    }
  }
}
```
