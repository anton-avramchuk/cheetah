# Cheetah.Core.Domain

Базовые строительные блоки DDD-слоя Domain: сущности, агрегаты, value objects. Зависит от `Cheetah.Core`, `Cheetah.Core.Events` (для доменных событий) и `Cheetah.Core.Specification`.

## Состав

| Тип | Назначение |
|-----|------------|
| `Entity<TId>` / `Entity` | Базовая сущность; равенство по `Id`, `GetKeys()` |
| `AggregateRoot<TId>` / `AggregateRoot` (Guid) | Корень агрегата; копит доменные события (`AddDomainEvent`, `DomainEvents`, `ClearDomainEvents`) |
| `ValueObject` | Базовый value object; равенство по `GetEqualityComponents()` |
| `IEntity` / `IEntity<TKey>` | Контракты сущности |
| `ICreateAtEntity`, `IUpdatedAtEntity`, `IRemovedAtEntity` | Аудит-маркеры (`CreatedAt`/`UpdatedAt`/`RemovedAt` как `DateTimeOffset?`) |
| `EntityByIdSpecification<T>` | Готовая спецификация «сущность по Guid Id» |
| `EntityNotFoundException` | Бросается при отсутствии сущности |
| Value objects: `Email`, `Phone`, `Color` | Готовые VO (нормализация + валидация через `Create`) |

## Сущность и агрегат

```csharp
public class Order : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Number { get; private set; } = null!;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private Order() { } // для EF Core

    public static Order Create(string number)
    {
        var order = new Order { Id = Guid.NewGuid(), Number = number };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, number));
        return order;
    }
}
```

Доменные события копятся внутри агрегата и публикуются в `IEventBus` **после** `SaveChangesAsync()`, затем очищаются через `ClearDomainEvents()`.

> В EF-конфигурации обязательно `builder.Ignore(e => e.DomainEvents)`.

## Value Object

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

Равенство и `GetHashCode()` выводятся из компонентов автоматически.
