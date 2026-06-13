# Cheetah.Backend.CQRS

Реализация `IDispatcher` из [Cheetah.Core.CQRS](../Cheetah.Core.CQRS/README.md). Резолвит обработчик команды/запроса из DI и вызывает его. Зависит от `Cheetah.Core`, `CrmCQRSCoreModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `Dispatcher` (`IDispatcher`, Scoped) | `SendAsync` → `ICommandHandler<,>`; `QueryAsync` → `IQueryHandler<,>` |
| `CrmBackendCQRSModule` | Модуль; регистрирует диспетчер через `[Export]` |

Диспетчер тонкий: получает `ICommandHandler<TCommand[,TResult]>` / `IQueryHandler<TQuery,TResult>` из `IServiceProvider` и делегирует `HandleAsync`. Сами хендлеры регистрируются в своих модулях через `[Export]`.

## Подключение

```csharp
[DependsOn(typeof(CrmBackendCQRSModule))]
public partial class MyApiModule : CrmModule { }
```

```csharp
var id = await dispatcher.SendAsync<CreateOrderCommand, Guid>(command, ct);
var vm = await dispatcher.QueryAsync<GetOrderByIdQuery, OrderViewModel>(query, ct);
```
