# Cheetah.Core.Cache

Core-абстракция кэширования. Единый интерфейс поверх in-memory и distributed-кэша. Зависит только от `Cheetah.Core`. Конкретная реализация (например, Redis) подключается отдельным backend-модулем.

## Состав

| Тип | Назначение |
|-----|------------|
| `ICacheService` | `GetAsync`, `SetAsync`, `GetOrSetAsync`, `RemoveAsync` |
| `CrmCacheCoreModule` | Core-модуль |

Все методы — `ValueTask` с `CancellationToken`.

## Использование

`GetOrSetAsync` реализует паттерн Cache-Aside: значение берётся из кэша, а при промахе вычисляется фабрикой и кладётся обратно.

```csharp
public class CatalogService(ICacheService cache, ICatalogRepository repo)
{
    public ValueTask<Catalog> GetAsync(Guid id, CancellationToken ct) =>
        cache.GetOrSetAsync(
            $"catalog:{id}",
            async () => await repo.LoadAsync(id, ct),
            TimeSpan.FromMinutes(10),
            ct);
}
```

`expiry` опционален: `null` означает поведение по умолчанию для подключённой реализации.
