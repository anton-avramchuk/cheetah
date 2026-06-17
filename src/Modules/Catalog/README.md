# Cheetah.Modules.Catalog.* — абстрактный шаблон-модуль «Каталог товаров и прайс-листы»

> **Тип:** base/template (каркас), а не готовый микросервис.
> Модуль поставляет **абстрактный расширяемый товар** + конкретные категории/прайс-листы и
> generic-хелперы. Конкретное приложение наследует тип товара, генерирует миграции у себя и получает
> рабочий CRUD товаров/категорий/прайсов «из коробки», дописав ~6 классов. Сделан по образцу
> [`Cheetah.Modules.Activities`](../Activities/README.md) / [`Cheetah.Modules.Customer`](../Customer/README.md).
>
> Полный план и решения — [`docs/modules/catalog.md`](../../../docs/modules/catalog.md).

## Назначение

Справочник того, что компания продаёт и почём: товары/услуги (расширяемый `ProductBase`), дерево
категорий (`ProductCategory`, материализованный путь), прайс-листы по валютам (`PriceList` + строки
`PriceListItem`). Отвечает на вопрос «какая цена у товара в прайс-листе при количестве N». Источник
истины по ценам для будущего модуля Sales Documents (документ делает снимок цены в свою строку).

**Главное требование — расширяемость:** товар и его ViewModel наследуемы; приложение добавляет свои
поля (бренд, штрихкод, вес, ставка НДС) без форка модуля. Категории/прайс-листы — конкретные (как
`ActivityReminder`): расширяется главный агрегат, дочерние/справочные фиксированы.

## Состав сборок и граф зависимостей

```
Catalog.DomainEvents   → Core.Events                          (ProductCreated/Updated/Deactivated/Activated, PriceChanged)
Catalog.Shared         → Core                                 (enum ProductType/UnitOfMeasure, CatalogConstants)
Catalog.Contracts      → Core + Contracts + Shared            (ABSTRACT ProductDtoBase/RequestBase + конкретные DTO категорий/прайсов)
Catalog.Domain         → DomainEvents + Specification         (abstract ProductBase; sealed ProductCategory/PriceList/PriceListItem; спеки)
Catalog.Infrastructure → Domain + EF + EF.PostgreSql          (abstract DbContextBase/ProductConfigBase, AddCatalogInfrastructure<>)
Catalog.Application     → Domain + Contracts + CQRS + Events   (generic CQRS товара, [Export]-хендлеры категорий/прайсов, AddCatalogApplication<>)
Catalog.Api            → Application + Contracts + Backend.Endpoints + Mapster  (декларативные эндпоинты + CatalogMappingProfile)
Tests: Domain.Tests (15), Application.Tests (24)
```

`Default` и `Client` отсутствуют — их создаёт наследник / следующий инкремент (нужны закрытые типы).

## Ключевые абстракции

| Тип | Сборка | Роль |
|---|---|---|
| `ProductBase : AggregateRoot<Guid>` | Domain | агрегат товара; `InitializeCore`, `Update`, `Deactivate`, `Activate`, `SetAttributes` |
| `ProductCategory : AggregateRoot<Guid>` | Domain | дерево категорий (`ParentId` + материализованный `Path`), `Create`/`Rename`/`Reorder` |
| `PriceList : AggregateRoot<Guid>` | Domain | прайс-лист + строки; `SetPrice`, `ResolvePrice(productId, qty)` |
| `PriceListItem : Entity<Guid>` | Domain | строка цены (дитя прайса): `ProductId`, `Price`, `MinQty?` |
| `Product*Specification<TProduct>` | Domain | generic-спеки: BySku, ActiveByCategory, Filter |
| `ProductDtoBase` + `Create/UpdateProductRequestBase` | Contracts | абстрактные record (точка расширения ViewModel/запросов) |
| `CatalogDbContextBase<TContext, TProduct>` | Infrastructure | `DbSet` товаров + категорий + прайсов |
| `ProductConfigurationBase<TProduct>` | Infrastructure | таблица/схема, уникальный Sku, индексы, `ConfigureCustom` hook |
| `IProductFactory`/`IProductProjector` | Application | `Create(...)`/`ToDto(...)` — замена `new`/Mapster |
| generic CQRS товара | Application | Create/Update/Deactivate/Activate/Delete + GetById/List/Grid |
| `[Export]`-хендлеры категорий/прайсов | Application | полный CRUD + Grid (категории), CRUD + SetPrice/ResolvePrice + Grid (прайсы) |
| декларативные эндпоинты | Api | наследники `Cheetah.Backend.Endpoints` + генератор; грид через `IGridRepository` |

### Архитектурные приёмы абстрактности

- **Нельзя `new` абстрактную сущность** в generic-handler → фабрика `IProductFactory` + `InitializeCore`.
- **`[Export]` source-gen работает только по закрытым типам** → открытые generic-handler'ы товара
  регистрируются вручную в `AddCatalogApplication<>`; конкретные хендлеры категорий/прайсов берёт
  генератор по `[Export]`.
- **Абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`, design-time factory и
  миграции принадлежат наследнику; базовый `OnModelCreating` применяет конфигурацию товара через hook.
- **Прайс-лист грузится со строками** через `AutoInclude()` на навигации `Items` (хендлеры читают
  агрегат спецификацией `PriceListByIdSpecification`, без EF-зависимости в Application).
- **События** публикуются через `IEventBus` после `SaveChangesAsync` (как в Customer/Activities;
  Outbox — follow-up).

## Extension-методы (точки регистрации у наследника)

```csharp
// Infrastructure: DbContext (товары + категории + прайсы), мигратор, PostgreSQL,
// IRepository<Product, Guid>, IRepository<ProductCategory, Guid>, IRepository<PriceList, Guid>
services.AddCatalogInfrastructure<AppCatalogDbContext, Product>();

// Application: фабрика, проектор, закрытые generic CQRS-handler'ы товара (включая Delete и Grid)
services.AddCatalogApplication<Product, CreateProductRequest, UpdateProductRequest,
    ProductDto, ProductGridViewModel, ProductFactory, ProductProjector>();
```

## Пример наследования («быстрая реализация»)

```csharp
// 1. Сущность с доп. полем
public sealed class Product : ProductBase
{
    public string? Brand { get; private set; }
    private Product() { }
    public static Product Create(CreateProductRequest r)
    {
        var p = new Product();
        p.InitializeCore(Guid.NewGuid(), r.Sku, r.Name, r.Type, r.Unit, r.CategoryId, r.Description);
        return p;
    }
}

// 2. Contracts с доп. полем
public sealed record ProductDto : ProductDtoBase { public string? Brand { get; init; } }
public sealed record CreateProductRequest : CreateProductRequestBase { public string? Brand { get; init; } }
public sealed record UpdateProductRequest : UpdateProductRequestBase;

// 3. Фабрика + проектор
public sealed class ProductFactory : IProductFactory<Product, CreateProductRequest>
{ public Product Create(CreateProductRequest r) => Product.Create(r); }

public sealed class ProductProjector : IProductProjector<Product, ProductDto>
{
    public ProductDto ToDto(Product p) => new()
    {
        Id = p.Id, Sku = p.Sku, Name = p.Name, Description = p.Description, Type = p.Type, Unit = p.Unit,
        CategoryId = p.CategoryId, IsActive = p.IsActive, CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
        // + Brand = p.Brand
    };
}

// 4. EF-конфигурация (доп. поле через ConfigureCustom) + DbContext (миграции — у наследника)
public sealed class ProductConfiguration : ProductConfigurationBase<Product>
{ protected override void ConfigureCustom(EntityTypeBuilder<Product> b) => b.Property(x => x.Brand).HasMaxLength(128); }

public sealed class AppCatalogDbContext(DbContextOptions<AppCatalogDbContext> o)
    : CatalogDbContextBase<AppCatalogDbContext, Product>(o)
{
    protected override IEntityTypeConfiguration<Product> CreateProductConfiguration() => new ProductConfiguration();
}

// 5. Request-DTO/ViewModel/команды товара наследника (закрывают шаблоны) + Mapster-маппинги.
//    Категории и прайс-листы уже работают «из коробки» в Cheetah.Modules.Catalog.Api.
public sealed record CreateProductRequest(/* поля + Brand */) : CreateProductRequestBase;
public sealed record ProductGridViewModel : ProductGridViewModelBase { public string? Brand { get; init; } }

// Закрытие абстрактных эндпоинтов товара (генератор хоста зарегистрирует маршруты):
public sealed class CreateProductEndpointImpl
    : CreateProductEndpoint<CreateProductRequest, CreateProductCommand<CreateProductRequest>> { }
public sealed class GetProductsGridEndpointImpl
    : GetProductsGridEndpoint<GetProductsGridRequest, GetProductsGridQuery<ProductGridViewModel>, ProductGridViewModel> { }
// аналогично GetById/Update/Delete + Mapster: Request→CreateProductCommand<…>, Product→ProductGridViewModel/ProductDto

// 6. dotnet ef migrations add InitialCatalog  (+ IDesignTimeDbContextFactory) — в проекте наследника
```

## Эндпоинты

Декларативный стиль `Cheetah.Backend.Endpoints` (как модуль Identity): эндпоинты — наследники
`CreateCommandEndpoint`/`UpdateCommandEndpoint`/`DeleteCommandEndpoint`/`QueryOrNotFoundEndpoint`/
`QueryGridEndpoint`; маршруты регистрирует генератор `Cheetah.Generators.Endpoints` в
`OnApplicationInitialization`; маппинг Request↔Command/Query и Entity→ViewModel — через Mapster
(`CatalogMappingProfile`); грид (пагинация/сортировка/фильтрация) — через `IGridRepository`.

**Категории и прайс-листы — конкретные эндпоинты, работают «из коробки»:**

| Метод | Маршрут | Назначение |
|---|---|---|
| POST | `api/categories` | создать категорию |
| GET | `api/categories` | грид категорий (`?page=&pageSize=&sort=&filter=`) |
| GET | `api/categories/{id}` | категория по Id |
| PUT | `api/categories/{id}` | обновить категорию |
| DELETE | `api/categories/{id}` | удалить категорию |
| POST | `api/price-lists` | создать прайс-лист |
| GET | `api/price-lists` | грид прайс-листов |
| GET | `api/price-lists/{id}` | прайс-лист со строками |
| PUT | `api/price-lists/{id}` | обновить заголовок прайса |
| DELETE | `api/price-lists/{id}` | удалить прайс-лист |
| POST | `api/price-lists/{id}/items` | установить/изменить цену `{ productId, price, minQty? }` |
| GET | `api/price-lists/{id}/price?productId=&qty=` | разрешить цену |

**Товары — абстрактные шаблоны эндпоинтов** (товар расширяем) → закрывает наследник/хост, тогда
генератор регистрирует маршруты (как абстрактные эндпоинты Identity):

| Шаблон | База | Маршрут (по умолчанию) |
|---|---|---|
| `CreateProductEndpoint<TRequest,TCommand>` | CreateCommandEndpoint | POST `api/products` |
| `GetProductByIdEndpoint<TRequest,TQuery,TDto>` | QueryOrNotFoundEndpoint | GET `api/products/{id}` |
| `UpdateProductEndpoint<TRequest,TCommand>` | UpdateCommandEndpoint | PUT `api/products/{id}` |
| `DeleteProductEndpoint<TRequest,TCommand>` | DeleteCommandEndpoint | DELETE `api/products/{id}` |
| `GetProductsGridEndpoint<TRequest,TQuery,TGridVm>` | QueryGridEndpoint | GET `api/products` |

Плюс доменные команды активации/деактивации (`Deactivate/ActivateProductCommand`) и список
`ListProductsQuery` доступны через CQRS (эндпоинты при необходимости добавляет наследник).

> Хост закрывает шаблоны товара своими конкретными Request/Command/Query/Dto/GridViewModel и
> объявляет их Mapster-маппинги (Request→`CreateProductCommand<…>`, `Product`→ViewModel). См. Identity
> как образец host-закрытия абстрактных эндпоинтов.

## События

`ProductCreated/Updated/Deactivated/Activated`, `PriceChanged` (`*IntegrationEvent`, `Cheetah.Core.Events`).
Потребители: Sales Documents (`PriceChanged` → пометить открытые КП), Search, Timeline, аналитика.

## Ограничения / follow-up

- Миграции и `IDesignTimeDbContextFactory` — только у наследника (в модуле их нет).
- `Client` (`ICatalogClient` для Sales Documents) и готовая сборка `.Default` — следующий инкремент.
- Кэш разрешения цены (`Cheetah.Core.Cache`) — follow-up (сейчас `ResolvePrice` читает прайс из БД).
- Транзакционный Outbox — follow-up (сейчас публикация после `SaveChangesAsync`).
- Справочник `ProductType`/`UnitOfMeasure` вместо enum, полноценные скидочные правила, `ltree` для
  дерева категорий, мультитенантность, общий VO `Money` — follow-up.
- Коды ответов: not-found маппится в 400 (`CatalogValidationException`) — уточнить до 404.
