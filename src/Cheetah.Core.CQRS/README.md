# Cheetah.Core.CQRS

Лёгкие контракты CQRS: команды, запросы, их обработчики и диспетчер. Только абстракции — реализация диспетчера живёт в [Cheetah.CQRS.Dispatcher](../Cheetah.CQRS.Dispatcher/README.md). Зависит только от `Cheetah.Core`.

## Состав

| Тип | Назначение |
|-----|------------|
| `ICommand` / `ICommand<TResult>` | Маркер команды (изменение состояния) |
| `IQuery<TResult>` | Маркер запроса (чтение) |
| `ICommandHandler<TCommand>` / `ICommandHandler<TCommand, TResult>` | Обработчик команды |
| `IQueryHandler<TQuery, TResult>` | Обработчик запроса |
| `IDispatcher` | `SendAsync` (команды) и `QueryAsync` (запросы) |

Все методы — `ValueTask` с `CancellationToken` (горячий путь CQRS).

## Соглашения

- **Команды** меняют состояние и возвращают `Guid`/`void`.
- **Запросы** только читают и возвращают ViewModel.

```csharp
public record CreateOrderCommand(string Number) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateOrderCommand, Guid>))]
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public async ValueTask<Guid> HandleAsync(CreateOrderCommand cmd, CancellationToken ct)
    {
        // ...
    }
}
```

Вызов из API-слоя:

```csharp
var id = await dispatcher.SendAsync<CreateOrderCommand, Guid>(command, ct);
var vm = await dispatcher.QueryAsync<GetOrderByIdQuery, OrderViewModel>(query, ct);
```
