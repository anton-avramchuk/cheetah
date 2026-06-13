# Cheetah.Core.Specification

Реализация паттерна **Specification**. По правилам проекта всё фильтрование в Application-слое идёт через спецификации — сырой LINQ (`.Where(x => ...)`) в хендлерах запрещён. Зависит только от `Cheetah.Core`.

## Состав

| Тип | Назначение |
|-----|------------|
| `ISpecification<T>` | `IsSatisfiedBy(obj)` + `ToExpression()` |
| `Specification<T>` | Базовый класс; кэширует скомпилированное выражение, неявно кастится в `Expression<Func<T,bool>>` |
| `CompositeSpecification<T>` / `ICompositeSpecification<T>` | Основа для составных спецификаций |
| `AndSpecification`, `OrSpecification`, `NotSpecification`, `AndNotSpecification` | Логические комбинаторы |
| `AnySpecification<T>`, `NoneSpecification<T>` | Всегда true / всегда false |
| `ExpressionSpecification<T>` | Спецификация из готового выражения |
| `SpecificationExtensions` | Fluent `.And()`, `.Or()`, `.Not()`, `.AndNot()` |
| `ISpecificationParser<TCriteria>` | Контракт парсинга спецификации в специфичный критерий (напр. SQL — см. Dapper DAL) |
| `ParameterRebinder`, `ExpressionFuncExtender` | Внутренняя склейка выражений при комбинировании |

## Использование

```csharp
public class ActiveOrdersSpecification : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression()
        => o => o.IsActive;
}

public class OrdersByCustomerSpecification(Guid customerId) : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression()
        => o => o.CustomerId == customerId;
}
```

Комбинирование:

```csharp
var spec = new ActiveOrdersSpecification()
    .And(new OrdersByCustomerSpecification(customerId));

var orders = await _repository.GetAllAsync(spec, ct);
```

`ToExpression()` транслируется EF Core в SQL; `IsSatisfiedBy()` проверяет объект в памяти (компилирует выражение лениво и кэширует).
