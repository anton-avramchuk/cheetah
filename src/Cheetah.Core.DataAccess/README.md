# Cheetah.Core.DataAccess

Провайдеро-независимые абстракции доступа к данным: контракты репозиториев, строки подключения, data filters и сидинг. Конкретные реализации — EF Core ([Cheetah.Core.EntityFramework](../Cheetah.Core.EntityFramework/README.md)) и Dapper ([Cheetah.Core.Dapper](../Cheetah.Core.Dapper/README.md)). Зависит от `Cheetah.Core`, `Cheetah.Core.Domain`, `Cheetah.Core.Specification`.

## Состав

| Область | Типы | Назначение |
|---------|------|------------|
| Репозитории | `IReadOnlyRepository<TEntity, TKey>`, `IRepository<TEntity, TKey>` (+ Guid-перегрузки) | Контракт репозитория: `GetByIdAsync`, `GetBySpecAsync`, `GetAllAsync(spec)`, `ExistsAsync(spec)`, `Add/Update/Delete`, `SaveChangesAsync`, `AsQueryable`/`AsNoTrackingQueryable` |
| Connection strings | `ConnectionStrings`, `IConnectionStringResolver`, `IConnectionStringChecker`, `[ConnectionStringName]`, `CrmDbConnectionOptions`, `CrmDatabaseInfo` | Резолвинг и проверка строк подключения по модулям |
| Data filters | `IDataFilter`, `IDataFilter<TFilter>`, `DataFilter`, `CrmDataFilterOptions` | Включаемые/выключаемые глобальные фильтры (soft-delete, мультитенантность) |
| Seeding | `IDataSeedContributor`, `DataSeedContext`, `DataSeedContributorList` | Контрибьюторы начальных данных (авто-регистрируются) |
| Specs | `EntityByIdSpecification` | Базовые спецификации DataAccess |
| Module | `CrmDataAccessModule` | Авто-сбор data-seed-контрибьюторов, регистрация `IDataFilter<>` |

## Репозиторий и спецификации

Application-слой работает **только** через `IRepository<,>` и спецификации — без прямого `DbContext`:

```csharp
var spec = new ActiveOrdersSpecification().And(new OrdersByCustomerSpecification(id));
var orders = await _repository.GetAllAsync(spec, ct);
```

## Data filters

Глобальный фильтр (например, soft-delete) можно временно выключить:

```csharp
public class HardQueryService(IDataFilter dataFilter, IRepository<Order> repo)
{
    public async Task<List<Order>> IncludingDeletedAsync(CancellationToken ct)
    {
        using (dataFilter.Disable<ISoftDelete>())
            return await repo.GetAllAsync(ct: ct);
    }
}
```

## Сидинг

```csharp
[Export(LifetimeType.Transient, typeof(IDataSeedContributor))]
public class DefaultRolesSeedContributor : IDataSeedContributor
{
    public async Task SeedAsync(DataSeedContext context) { /* ... */ }
}
```

Контрибьюторы подхватываются `CrmDataAccessModule` автоматически — регистрировать список вручную не нужно.
