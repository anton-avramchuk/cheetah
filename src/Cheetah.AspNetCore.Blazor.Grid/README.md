# Cheetah.AspNetCore.Blazor.Grid

Generic CRUD-грид для Blazor-хоста (BFF): компонент `CrmGrid`, базовый CRUD-сервис над `IGridRepository<>`
(`Cheetah.Core.Grid`) и скоупинг видимости по владельцу. Колонки описываются атрибутами на ViewModel —
отдельный UI на каждую сущность писать не нужно.

> Не путать с `Cheetah.Core.Grid` — там серверный `IGridRepository<>`/`GridRequest`. Этот модуль — Blazor-UI поверх него.

## Что предоставляет

- **`CrmGrid<TGrid, TDetails, TCreate>`** (Razor-компонент) — таблица, тулбар «Создать», кнопки edit/delete,
  диалоги (через `IDialogService`), тосты (через `IToastService`), пагинация (`CrmPager`). Колонки строит
  рефлексией по `[GridColumn]` на `TGrid`. **Сортировка по клику на заголовок** (цикл: нет → asc → desc → нет;
  одна колонка за раз) — отправляется на сервер через `CrmPageRequest.Sort`. Создание/редактирование работают
  **в диалоге** (по умолчанию) либо
  **переходом на страницу** (`CreateUrl`/`EditUrl`); наружу отдаются события `OnCreated`/`OnUpdated`/`OnDeleted`
  с `Id` затронутой записи.
- **`ICrudService<TGrid, TDetails, TCreate>`** — контракт прикладного CRUD: `GetGridAsync`, `GetByIdAsync`,
  `CreateAsync` (возвращает `Guid` созданной записи), `UpdateAsync`, `DeleteAsync`.
- **`BaseCrudService<TEntity, TGrid, TDetails, TCreate>`** — базовая реализация над `IGridRepository<TEntity>`
  (`GetGrid`/`GetById`/`Delete` готовы; `Create`/`Update` — абстрактные, вызывают доменные фабрики).
- **`[GridColumn(label, show = true, order = 0, sortable = true)]`** — разметка колонок на свойствах `TGrid`.
  `sortable = false` отключает сортировку по конкретной колонке.
- **`IHasId`** (`Guid Id`) — `TGrid` обязан реализовать (для edit/delete по строке).
- **`CrmPageRequest`** (`Page`, `PageSize`, `Sort`, `Filter`) / **`CrmGridResult<T>`** (`Data`, `Total`) — DTO пагинации UI.
  `Sort` (`List<SortDescriptor>`) и `Filter` (`FilterDescriptor?`) — те же дескрипторы, что и серверный
  `GridRequest` (`Cheetah.Contracts.Requests`); `BaseCrudService` прокидывает их в репозиторий без перекладки.
  Имена полей в фильтре/сортировке берутся из ViewModel-грида (репозиторий разворачивает их по проекции Mapster).
- **Скоупинг видимости**: `DataScope(Guid UserId, bool CanViewAll)`, шов `IDataScopeAccessor.ResolveAsync(...)`
  (реализуется приложением/Identity) и `IGridRepository<T>.GetScopedGridAsync<TEntity,TViewModel>(..., params ownerFields)`
  — если у пользователя нет права «видеть всё», добавляет фильтр `owner == userId` (несколько полей по OR).

## Параметры `CrmGrid`

| Параметр | Тип | Описание |
|---|---|---|
| `Service` | `ICrudService<…>` | CRUD-сервис (обязателен) |
| `EditFormType` / `CreateFormType` | `Type?` | Формы для диалогов; не нужны в режиме перехода на страницу |
| `Title` | `string` | Заголовок над таблицей |
| `PageSize` | `int` | Размер страницы (по умолч. 20) |
| `RowActions` | `RenderFragment<TGrid>?` | Доп. кнопки в строке (слева от edit/delete) |
| `ToolbarActions` | `RenderFragment?` | Доп. контент тулбара (справа от «Создать») |
| `CreateModelFactory` | `Func<TCreate>?` | Фабрика модели для формы создания |
| `AutoOpenCreate` | `bool` | Сразу открыть диалог создания (игнорируется при `CreateUrl`) |
| `DialogWidthPx` | `int?` | Ширина диалогов create/edit |
| `Sortable` | `bool` | Сортировка по клику на заголовок (по умолч. `true`); пер-колонку отключается `[GridColumn(sortable: false)]` |
| `DefaultFilter` | `FilterDescriptor?` | Базовый фильтр, применяемый к каждому запросу (скоуп по родителю/статусу); при смене значения грид перезагружается с 1-й страницы |
| `ShowCreate` / `ShowEdit` / `ShowDelete` | `bool` | Показ кнопок (по умолч. `true`); если ни одной строковой кнопки и нет `RowActions` — колонка действий скрывается |
| `CreateButtonLabel` | `string` | Подпись кнопки создания |
| `CreateDialogTitle` / `EditDialogTitle` | `string` | Заголовки диалогов |
| `CreateUrl` | `string?` | Если задан — «Создать» переходит по URL вместо диалога |
| `EditUrl` | `Func<Guid,string>?` | Если задан — edit переходит на `EditUrl(id)` вместо диалога |
| `OnCreated` / `OnUpdated` / `OnDeleted` | `EventCallback<Guid>` | События наружу с `Id` затронутой записи |
| `OnRowClick` | `EventCallback<TGrid>` | Клик по строке (кнопки действий не всплывают) |

Метод `RefreshAsync()` (через `@ref`) перезагружает текущую страницу после внешних изменений.

`TGrid : class, IHasId`, `TDetails : class, new()`, `TCreate : class, new()`.

### Создание/редактирование на отдельной странице

```razor
@* Диалог по умолчанию; здесь — переход на страницы, событие создания наружу *@
<CrmGrid TGridViewModel="SkillGridViewModel"
         TDetailsViewModel="SkillDetailsViewModel"
         TCreateViewModel="SkillCreateViewModel"
         Title="Навыки"
         CreateUrl="/skills/new"
         EditUrl="@(id => $"/skills/{id}")"
         OnCreated="OnSkillCreated" />

@code {
    // например: открыть карточку только что созданной записи
    private void OnSkillCreated(Guid id) => Nav.NavigateTo($"/skills/{id}");
}
```

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
    public override async Task<Guid> CreateAsync(SkillCreateViewModel m, CancellationToken ct)
    { var s = Skill.Create(m.Name); repo.Add(s); await repo.SaveChangesAsync(ct); return s.Id; }

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
