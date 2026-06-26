# Cheetah.Modules.Catalog.Blazor

Blazor Server BFF UI-слой модуля **Catalog**: страницы, формы, пункты меню и CRUD-сервисы поверх
инфраструктуры `Cheetah.AspNetCore.Blazor.*`. Сборка не содержит хоста — хост подключает её страницы через
`AddAdditionalAssemblies(typeof(CheetahCatalogBlazorModule).Assembly)`.

## Что внутри

| Область | Файлы | Назначение |
|---|---|---|
| Страницы | `Pages/Categories.razor` (`/catalog/categories`), `Pages/PriceLists.razor` (`/catalog/price-lists`) | Грид + диалоги CRUD на базе `CrmGrid` |
| Формы | `Pages/Forms/*Form.razor` | Create/Edit формы для диалогов (`CrmTextBox`, `CrmSwitch`) |
| ViewModel'и | `ViewModels/*.cs` | Grid (`IHasId` + `[GridColumn]`), Details, Create |
| CRUD | `Services/*CrudService.cs` | `BaseCrudService<TEntity,…>` поверх `IGridRepository<TEntity>`; create/update — доменные фабрики |
| Меню | `Navigation/CatalogMenuContributor.cs` | Секция «Каталог» (`IMenuContributor`) |
| Товары | `Components/CatalogProductGrid.razor` | Generic-шаблон (см. ниже) |

CRUD-сервисы и contributor меню регистрируются генератором по `[Export]` — вручную в DI ничего добавлять
не нужно (вызов `RegisterServices` в `CheetahCatalogBlazorModule`).

## Категории и прайс-листы vs товары

`ProductCategory` и `PriceList` — **конкретные** агрегаты, поэтому их страницы готовы и маршрутизируемы.
`Catalog` же — **абстрактный шаблон**: товар расширяется наследником `ProductBase`, конкретного типа товара
в модуле нет. Поэтому routable-страницы товаров здесь нет — вместо неё поставляется generic-компонент
`CatalogProductGrid`, который конкретное приложение закрывает своими типами:

```razor
@page "/catalog/products"
@layout Cheetah.AspNetCore.Blazor.Layouts.MainLayout

<CatalogProductGrid TGridViewModel="AppProductGridVm"
                    TDetailsViewModel="AppProductDetailsVm"
                    TCreateViewModel="AppProductCreateVm"
                    Service="Service"
                    EditFormType="typeof(AppProductEditForm)"
                    CreateFormType="typeof(AppProductCreateForm)" />
```

## Требования к хосту

1. `AddAdditionalAssemblies(typeof(CheetahCatalogBlazorModule).Assembly)` в `Program.cs` (требование Blazor).
2. Зарегистрировать **однопараметрические** grid-репозитории, которых требует `BaseCrudService`:

   ```csharp
   services.AddScoped<IGridRepository<ProductCategory>>(sp =>
       new EfGridRepository<CatalogDbContext, ProductCategory>(/* … */));
   services.AddScoped<IGridRepository<PriceList>>(sp =>
       new EfGridRepository<CatalogDbContext, PriceList>(/* … */));
   ```

   Инфраструктура Catalog (`AddCatalogInfrastructure`) регистрирует двухпараметрический
   `IGridRepository<T, Guid>`; `BaseCrudService<T,…>` принимает однопараметрический `IGridRepository<T>`
   (его реализует `EfGridRepository<TContext, T>`). Без этой регистрации DI не построит CRUD-сервисы.
3. Маппинг сущность → ViewModel выполняется `IGridRepository.GetGridAsync<VM>`/`GetByIdAsync<VM>` через
   `IObjectMapper`. ViewModel'и названы как поля сущностей, поэтому работает конвенция Mapster; при
   необходимости добавьте явный маппинг.
4. Бренд (`IApplicationConfigurationProvider`), `IUserTokenStore`/`IBffAuthenticator` и реализацию
   `IMenuAccessEvaluator` поставляет приложение — см. README соответствующих базовых модулей.

## Зависимости

`Cheetah.Core`, `Cheetah.AspNetCore.Blazor.Grid` / `.Navigation` / `.Layouts` / `.Controls`,
`Cheetah.Modules.Catalog.Domain`. Модуль: `[DependsOn]` на `CoreModule`, `CrmBlazorGridModule`,
`CrmBlazorNavigationModule`, `CrmBlazorLayoutsModule`, `CrmBlazorControlsModule`, `CheetahCatalogDomainModule`.
