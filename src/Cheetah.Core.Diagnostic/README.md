# Cheetah.Core.Diagnostic

Базовый модуль диагностики. Предоставляет атрибут `[MeasureTime]` и замер времени выполнения **DI-сервисов** через `DispatchProxy`.

> Для HTTP-эндпоинтов и логирования запросов используй
> [`Cheetah.Core.Diagnostic.AspNetCore`](../Cheetah.Core.Diagnostic.AspNetCore/README.md).

---

## Подключение

```csharp
[DependsOn(
    typeof(CrmCoreDiagnosticModule),
    typeof(MyApplicationModule)
)]
public partial class MyBootstrapperModule : CrmModule { }
```

## Конфигурация

```json
{
  "Diagnostics": {
    "Enabled": true
  }
}
```

| Поле      | Тип    | По умолчанию | Описание                                                               |
|-----------|--------|:------------:|------------------------------------------------------------------------|
| `Enabled` | `bool` | `true`       | Мастер-выключатель. При `false` все замеры отключаются.                |

## Использование

Пометь нужные методы атрибутом `[MeasureTime]` на классе-реализации:

```csharp
[Export(LifetimeType.Scoped, typeof(IOrderService))]
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

Опциональный параметр `label` добавляется в лог-сообщение:

```
[Diagnostic] (создание заказа) OrderService.CreateAsync completed in 42ms
[Diagnostic] OrderService.GetByIdAsync completed in 7ms
```

При выбросе исключения логируется `Warning`:

```
[Diagnostic] (создание заказа) OrderService.CreateAsync threw after 13ms
```

## Требования

Модуль использует `DispatchProxy`, поэтому:

- Сервис **должен быть зарегистрирован через интерфейс** — `[Export(LifetimeType.Scoped, typeof(IOrderService))]`
- Атрибут ставится на метод **класса-реализации** (или на метод интерфейса — оба варианта поддерживаются)
- Поддерживаемые типы возвращаемых значений: `void`, `T`, `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`

```csharp
// ✅ Правильно
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

1. В `PostConfigureServices` модуль сканирует все зарегистрированные сервисы.
2. Для каждого сервиса, у реализации которого есть хотя бы один метод с `[MeasureTime]`, дескриптор заменяется на фабрику, создающую `DispatchProxy`-обёртку.
3. Proxy при каждом вызове:
   - проверяет наличие `[MeasureTime]` и флаг `Enabled`;
   - запускает `Stopwatch`;
   - для async-методов ждёт завершения задачи, затем останавливает таймер;
   - пишет `Information`-лог при успехе или `Warning`-лог при исключении.

## Фильтрация логов

```json
{
  "Logging": {
    "LogLevel": {
      "Cheetah.Core.Diagnostic": "Information"
    }
  }
}
```
