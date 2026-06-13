# Cheetah.Core.Grid

Серверная поддержка табличных гридов: фильтрация, сортировка и пагинация по запросу с фронтенда, с проекцией прямо во ViewModel. Зависит от `Cheetah.Core.EntityFramework`, `Cheetah.Core.DataAccess`, `Cheetah.Contracts`, `Cheetah.Mapping.Core`.

## Состав

| Тип | Назначение |
|-----|------------|
| `IGridRepository<TEntity, TKey>` / `IGridRepository<TEntity>` (Guid) | Расширяет `IRepository<,>`: `GetGridAsync<TViewModel>(GridRequest)` и `GetByIdAsync<TViewModel>(id)` |
| `EfGridRepository<TDbContext, TEntity, TKey>` | EF-реализация: строит `Where`/`OrderBy` по `TEntity`, затем `ProjectTo<TViewModel>` |
| `CrmGridModule` | Модуль |

Запрос/ответ — `GridRequest` (`Filter`, `Sort`, `Page`, `PageSize`) и `GridResult<T>` (`Data`, `Total`) из `Cheetah.Contracts`.

## Как это работает

Имена полей фильтра/сортировки приходят из **ViewModel** (например, `patientName`), но EF не может транслировать выражения после проекции. Поэтому репозиторий достаёт проекционную лямбду Mapster и разворачивает имена ViewModel в выражения по `TEntity` (`src.Direction.Patient.FullName`), строя `Where`/`OrderBy` до проекции.

Порядок: фильтр → `Count` (total) → сортировка → пагинация → `ProjectTo<TViewModel>` → `ToList`.

Поддерживаемые операторы фильтра: `eq`, `neq`, `contains`, `startswith`, `endswith`, `gt`, `gte`, `lt`, `lte`, `isnull`, `isnotnull`, `isempty`, `isnotempty` (со строковыми — опция `ignoreCase`). Строковые методы используют одно-аргументные перегрузки, транслируемые EF в SQL.

## Использование

```csharp
public class GetPatientsGridQueryHandler(IGridRepository<Patient> repo)
{
    public ValueTask<GridResult<PatientViewModel>> HandleAsync(GridRequest request, CancellationToken ct)
        => repo.GetGridAsync<PatientViewModel>(request, ct);
}
```
