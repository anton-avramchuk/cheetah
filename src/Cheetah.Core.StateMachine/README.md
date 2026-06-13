# Cheetah.Core.StateMachine

Лёгкая валидация переходов состояний на основе enum'а. Не исполняет переходы — только проверяет, что переход «из A в B» разрешён. Зависит от `Cheetah.Core`.

## Состав

| Тип | Назначение |
|-----|------------|
| `StateMachineBuilder<TState>` | Fluent-описание разрешённых переходов: `From(...).To(...)` |
| `StateMachineOptions` | Хранит конфигурации для нескольких enum-типов; `For<TState>()` |
| `IStateMachineValidator<TState>` / `StateMachineValidator<TState>` | Проверка допустимости перехода |
| `IStateMachineEntity<TState>` | Маркер сущности с свойством `State` |
| `InvalidStateTransitionException` | Бросается при недопустимом переходе |
| `StateMachineExtensions` | `AddStateMachine()` и `AddStateMachine<TState>(configure)` |
| `CrmStateMachineModule` | Модуль (регистрирует open-generic валидатор) |

`TState` — всегда `struct, Enum`.

## Использование

Конфигурация при старте:

```csharp
services.AddStateMachine<OrderState>(sm => sm
    .From(OrderState.New).To(OrderState.Confirmed, OrderState.Cancelled)
    .From(OrderState.Confirmed).To(OrderState.Shipped, OrderState.Cancelled)
    .From(OrderState.Shipped).To(OrderState.Completed));
```

Проверка в домене/хендлере:

```csharp
public class OrderService(IStateMachineValidator<OrderState> validator)
{
    public void Confirm(Order order)
    {
        validator.Validate(order.State, OrderState.Confirmed); // бросит InvalidStateTransitionException, если нельзя
        order.SetState(OrderState.Confirmed);
    }
}
```

Несколько enum-типов настраиваются независимо — каждый `For<TState>()` возвращает свой билдер.
