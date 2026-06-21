# Cheetah.AspNetCore.Blazor.Grid

Generic CRUD-грид для Blazor-хоста (BFF): компонент `CrmGrid`, базовый CRUD-сервис над `IGridRepository<>`
(`Cheetah.Core.Grid`) и скоупинг видимости по владельцу. Колонки описываются атрибутами на ViewModel —
отдельный UI на каждую сущность писать не нужно.

> Не путать с `Cheetah.Core.Grid` — там серверный `IGridRepository<>`/`GridRequest`. Этот модуль — Blazor-UI поверх него.

## Что предоставляет

- **`CrmGrid<TGrid, TDetails, TCreate>`** (Razor-компонент) — таблица, тулбар «Создать», кнопки edit/delete,
  диалоги (через `IDialogService`), тосты (через `IToastService`), пагинация (`CrmPager`). Колонки строит
  рефлексией по `[GridColumn]` на `TGrid`.
- **`ICrudService<TGrid, TDetails, TCreate>`** — контракт прикладного CRUD: `GetGridAsync`, `GetByIdAsync`,
  `CreateAsync`, `UpdateAsync`, `DeleteAsync`.
- **`BaseCrudService<TEntity, TGrid, TDetails, TCreate>`** — базовая реализация над `IGridRepository<TEntity>`
  (`GetGrid`/`GetById`/`Delete` готовы; `Create`/`Update` — абстрактные, вызывают доменные фабрики).
- **`[GridColumn(label, show = true, order = 0)]`** — разметка колонок на свойствах `TGrid`.
- **`IHasId`** (`Guid Id`) — `TGrid` обязан реализовать (для edit/delete по строке).
- **`CrmPageRequest`** (`Page`, `PageSize`) / **`CrmGridResult<T>`** (`Data`, `Total`) — DTO пагинации UI.
- **Скоупинг видимости**: `DataScope(Guid UserId, bool CanViewAll)`, шов `IDataScopeAccessor.ResolveAsync(...)`
  (реализуется приложением/Identity) и `IGridRepository<T>.GetScopedGridAsync<TEntity,TViewModel>(..., params ownerFields)`
  — если у пользователя нет права «видеть всё», добавляет фильтр `owner == userId` (несколько полей по OR).

## Параметры `CrmGrid`

| Параметр | Тип | Описание |
|---|---|---|
| `Title` | `string` | Заголовок над таблицей |
| `PageSize` | `int` | Размер страницы (по умолч. 20) |
| `RowActions` | `RenderFragment<TGrid>?` | Доп. кнопки в строке (слева от edit/delete) |
| `CreateModelFactory` | `Func<TCreate>?` | Фабрика модели для формы создания |
| `AutoOpenCreate` | `bool` | Сразу открыть диалог создания |
| `DialogWidthPx` | `int?` | Ширина диалогов create/edit |

`TGrid : class, IHasId`, `TDetails : class, new()`, `TCreate : class, new()`.

## Как добавить грид сущности

```csharp
// ViewModel грида
public class SkillGridViewModel : IHasId
{
    public Guid Id { get; set; }
    [GridColumn("Название", order: 0)] public string Name { get; set; } = "";
    [GridColumn("Активен", order: 1)]  public bool   IsActive { get; set; }
}

// CRUD-сервис
[Export(LifetimeType.Scoped, typeof(ICrudService<SkillGridViewModel, SkillDetailsViewModel, SkillCreateViewModel>))]
public class SkillCrudService(IGridRepository<Skill> repo)
    : BaseCrudService<Skill, SkillGridViewModel, SkillDetailsViewModel, SkillCreateViewModel>(repo)
{
    public override async Task CreateAsync(SkillCreateViewModel m, CancellationToken ct)
    { repo.Add(Skill.Create(m.Name)); await repo.SaveChangesAsync(ct); }

    public override async Task UpdateAsync(Guid id, SkillDetailsViewModel m, CancellationToken ct)
    { /* GetById → domain Update → SaveChanges */ }
}
```

```razor
@* страница *@
<CrmGrid TGridViewModel="SkillGridViewModel"
         TDetailsViewModel="SkillDetailsViewModel"
         TCreateViewModel="SkillCreateViewModel"
         Title="Навыки" />
```

## Зависимости

- `Cheetah.Core`, `Cheetah.Core.Grid`, `Cheetah.AspNetCore.Blazor.Dialogs`, `Cheetah.AspNetCore.Blazor.Toast`
  (`CrmBlazorGridModule` → `[DependsOn(CoreModule, CrmGridModule, CrmBlazorDialogsModule, CrmBlazorToastModule)]`).
- `Microsoft.AspNetCore.App` (Razor-компоненты `CrmGrid`/`CrmPager`).
- `BaseCrudService` требует `IGridRepository<TEntity>` (регистрируется Infrastructure-модулем сущности).
- Шов `IDataScopeAccessor` реализуется приложением (Identity) — нужен только при использовании `GetScopedGridAsync`.

## Подключение

```csharp
[DependsOn(typeof(CrmBlazorGridModule))]
public partial class MyModule : CrmModule { ... }
```
