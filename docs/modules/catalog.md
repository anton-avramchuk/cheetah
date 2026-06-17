# Cheetah.Modules.Catalog.* — модуль «Каталог товаров и прайс-листы» (расширяемый шаблон)

> Статус: **реализовано (MVP абстрактного шаблона).** Код — в `src/Modules/Catalog/`. 7 шаблонных
> проектов + 2 тестовых собираются; тесты зелёные: Domain (15), Application (24) = 39. Эндпоинты —
> **декларативные** (`Cheetah.Backend.Endpoints` + генератор), полный CRUD + grid для товаров,
> категорий и прайс-листов; грид через `IGridRepository`. Краткий гайд по расширению —
> `src/Modules/Catalog/README.md`. Что вошло и сознательные отличия — в §14.
> Документ — пошаговый план сборки модуля по канону `CLAUDE.md`
> (Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client)),
> своя БД PostgreSQL, общение через REST/gRPC + события через шину.
>
> Источник: раздел [§4](../plans.md#4-catalog-каталог-товаров-и-прайс-листы) общего плана. Это **п.4
> рекомендуемого порядка реализации** (после Tier 1: Deals → Activities → Leads), первый шаг
> коммерческого контура. Порядок строгий: **[Sales Documents](../plans.md#5-sales-documents-кп-заказы-счета)
> зависят от Catalog** (документы собираются из позиций каталога с ценами).
>
> **Tier 2.** Справочник того, что компания продаёт и почём: товары/услуги, категории (дерево),
> прайс-листы по валютам/сегментам, цены позиций. Источник истины по ценам для Sales Documents.

---

## 0. Главное требование — расширяемость

> **Модуль обязан быть расширяемым: сущности и ViewModel должны допускать расширение полями
> приложения-наследника без форка модуля** — ровно как уже реализованные
> [`Cheetah.Modules.Customer`](../../src/Modules/Customer/README.md) и
> [`Cheetah.Modules.Activities`](./activities.md).

Каталог по природе требует доп. полей под конкретный бизнес: бренд, артикул производителя, вес,
габариты, ставка НДС по умолчанию, штрихкод, страна происхождения, теги маркетплейса. Поэтому Catalog
строится как **абстрактный шаблон-модуль**: модуль поставляет **абстрактные базовые типы и
generic-хелперы**, а наследник дописывает свои `sealed`-типы со своими полями.

**Что расширяемо (главный агрегат), а что — нет.** По образцу Activities (расширяем `ActivityBase`,
конкретен `ActivityReminder`):

| Тип | Форма | Обоснование |
|---|---|---|
| **`ProductBase`** | **abstract** (расширяемый) | главный агрегат; почти всегда нужны доп. поля |
| **`ProductCategory`** | concrete (`sealed`) | дерево категорий редко требует доп. полей; задел — §4.6 |
| **`PriceList`** | concrete (`sealed`) | заголовок прайс-листа стабилен |
| **`PriceListItem`** | concrete (`sealed`, child of PriceList) | строка цены — пара (Product, Price, MinQty) |

**Что это даёт наследнику:**

- `sealed class Product : ProductBase` со своими полями (`Brand`, `Weight`, `Barcode`, `VatRate`…);
- `sealed record ProductDto : ProductDtoBase` со своими полями в ответе API;
- свои `Create/Update`-Request-ы (наследники `…RequestBase`);
- рабочий CRUD товаров/категорий/прайс-листов и события — «из коробки», дописав ~6–7 классов.

**Три уровня расширяемости** (как в Activities §0):

1. **Структурная (compile-time).** Наследование `ProductBase`/`ProductDtoBase` + `ConfigureCustom`
   hook в EF-конфигурации + фабрика/проектор наследника. Сильная типизация, индексы по доп. полям.
2. **Динамическая (runtime, без миграций).** «Быстрый» карман — опц. колонка `Attributes (jsonb)` на
   `ProductBase`; полноценно — будущий модуль [Custom Fields](../plans.md#7-custom-fields-кастомные-поля).
3. **Поведенческая.** `virtual`-мутаторы сущности, `virtual`-методы эндпоинтов; `ProductType`/`Unit`
   как данные-расширяемые точки (enum на MVP → справочник позже, см. §1.2).

---

## 1. Назначение и границы

**Что делает:** ведёт справочник номенклатуры (товары/услуги), дерево категорий, прайс-листы и цены
позиций. Отвечает на вопрос «какая цена у товара X в прайс-листе Y при количестве N».

**Чего НЕ делает:**

- не хранит товарные позиции документов и итоги — это
  [Sales Documents](../plans.md#5-sales-documents-кп-заказы-счета) (документ делает **снимок**
  цены/наименования в свою строку, цена в нём не зависит от последующих правок каталога);
- не считает склад/остатки/резервы — это будущий модуль Inventory (каталог — только номенклатура);
- не управляет сделками — Deals ссылается на товары/документы по `Id`.

**Связи (по `Id`, без FK через границу модуля):** `CategoryId?` (внутри модуля — настоящий FK),
`OwnerId?`/тенант — логические ссылки.

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Форма модуля | **абстрактный шаблон** (как Customer/Activities) — расширяемый `ProductBase`/`ProductDtoBase`. §0 |
| Идентификатор | `Guid` |
| Хранилище | PostgreSQL (EF Core); миграции — **у наследника** (или сборка `.Default`) |
| Цена | **не на товаре, а в `PriceListItem`** — одна номенклатура, разные цены для валют/сегментов |
| Валюта | на уровне `PriceList` (весь прайс в одной валюте); общий VO `Money` — открытый вопрос §1.2 |
| Категории | дерево через `ParentId`; материализованный путь `Path` для выборок поддерева (§4.4) |
| Кэш | прайс-листы меняются редко, читаются часто → `Cheetah.Core.Cache` на разрешение цены (§7.3) |
| `ProductType`/`Unit` | enum на MVP; справочник-таблица — точка расширения позже |
| События | через `IEventBus` после `SaveChangesAsync` (паттерн Customer/Activities; **не** Outbox на MVP) |
| Эндпоинты | **декларативные** `Cheetah.Backend.Endpoints` + генератор (стиль Identity); grid через `IGridRepository` |
| «Быстрый» карман | опц. `Attributes (jsonb)` на `ProductBase` (MVP), полноценно — Custom Fields |
| Soft-delete | `IsActive` (деактивация товара) вместо физического удаления; `Remove()` — follow-up |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Сборка `.Default` «из коробки».** По образцу плана Activities §3.1 — рекомендация **гибрид**:
   отдельная `Cheetah.Modules.Catalog.Default` с `sealed Product`, конкретными Contracts, `DbContext`
   и миграциями, чтобы модуль работал без дописывания (фактически в Activities `.Default` отложили в
   follow-up — здесь так же допустимо).
2. **`Money` как value object** (Amount + Currency) в `Shared` модуля vs `decimal Price` + `string
   Currency` на `PriceList`. Рекомендация: цена в `PriceListItem` — `decimal`, валюта — на `PriceList`
   (одна на прайс); общий `Money` ввести при появлении Sales Documents (сквозное решение №5 плана).
3. **Дерево категорий:** материализованный путь `Path` (рекомендация, простой и быстрый) vs PostgreSQL
   `ltree` (требует расширения БД) vs рекурсивный CTE.
4. **Скидочные правила** (`PriceListItem.MinQty` — пороги количества) — MVP: цена за единицу + опц.
   `MinQty`; полноценные правила скидок — follow-up.
5. **Мультитенантность** (`Cheetah.Core.Tenants`) — каталог глобальный vs per-tenant. MVP — глобальный.
6. **`ProductType`/`Unit`** — enum vs справочник (см. §1.1).

---

## 2. Архитектурная роль

```
   ┌──────────────────────────────────────────────────────────────┐
   │                      Catalog Module (шаблон)                  │
   │  ┌──────────────────┐        ┌──────────────────────────────┐ │
   │  │ ProductCategory  │ 1──*   │   ProductBase (abstract)     │ │
   │  │ (tree, Path)     │◀───────│   CategoryId?                │ │
   │  └──────────────────┘        └──────────────┬───────────────┘ │
   │                                             │ наследует        │
   │                              sealed Product (поля приложения)  │
   │  ┌──────────────────┐  1──* ┌──────────────────────────────┐  │
   │  │ PriceList        │───────│  PriceListItem               │  │
   │  │ (Currency,       │       │  (ProductId, Price, MinQty?) │  │
   │  │  IsDefault)      │       └──────────────────────────────┘  │
   │  └──────────────────┘                                         │
   └───────────────┬──────────────────────────────────────────────┘
                   │ publish (IEventBus → Redis/Kafka)
                   ▼
   ProductCreated / ProductUpdated / ProductDeactivated / PriceChanged
                   │
                   ├──▶ Sales Documents (PriceChanged → пометить открытые КП «цена устарела»)
                   ├──▶ Search / Timeline / Analytics
                   └──◀ (нет внешних зависимостей: каталог — источник, не потребитель)
   ┌──────────────────────────────────────────────────────────────┐
   │ Infrastructure: кэш разрешения цены (Cheetah.Core.Cache)      │
   │   PriceResolver(productId, priceListId, qty) → cached         │
   └──────────────────────────────────────────────────────────────┘
```

---

## 3. Структура проектов

```
src/Modules/Catalog/
├── Cheetah.Modules.Catalog.DomainEvents/   # ProductCreated/Updated/Deactivated, PriceChanged (Core.Events)
├── Cheetah.Modules.Catalog.Shared/          # enums (ProductType, UnitOfMeasure), CatalogConstants
├── Cheetah.Modules.Catalog.Contracts/       # ABSTRACT DTO/Request базы (ProductDtoBase, …RequestBase) + конкретные DTO категорий/прайсов
├── Cheetah.Modules.Catalog.Domain/          # abstract ProductBase; sealed ProductCategory/PriceList/PriceListItem; generic-спеки
├── Cheetah.Modules.Catalog.Infrastructure/  # abstract DbContextBase/ProductConfigBase, AddCatalogInfrastructure<>, кэш цен
├── Cheetah.Modules.Catalog.Application/      # generic handlers, IProductFactory/Projector, AddCatalogApplication<>
├── Cheetah.Modules.Catalog.Api/             # декларативные эндпоинты (Backend.Endpoints) + CatalogMappingProfile
├── (опц.) Cheetah.Modules.Catalog.Default/  # sealed Product + конкретные Contracts + DbContext + миграции «из коробки»
├── Cheetah.Modules.Catalog.Client/          # HTTP-клиент server-to-server (Sales Documents → цены/товары)
└── Tests/
    ├── Cheetah.Modules.Catalog.Domain.Tests/
    ├── Cheetah.Modules.Catalog.Application.Tests/
    └── Cheetah.Modules.Catalog.Client.Tests/
```

**Порядок зависимостей (строго, как в Customer/Activities):**

```
DomainEvents (Core.Events)
   ↓
Shared (Core)
   ↓
Contracts (Core + Shared)                       ← ABSTRACT базы Product + конкретные DTO Category/PriceList
   ↓
Domain (DomainEvents + Specification)           ← abstract ProductBase, sealed Category/PriceList, generic-спеки
   ↓
Infrastructure (Domain + EF + EF.PostgreSql)    ← abstract DbContextBase/ConfigBase, AddCatalogInfrastructure<>, кэш
Application (Domain + Contracts + CQRS + Events) ← generic handlers, AddCatalogApplication<>
Api (Application + Contracts + AspNetCore)       ← abstract ProductEndpointsBase<>, ApiModuleBase
   ↓
Default (наследует всё) + Client (Contracts)
```

> Канон `CLAUDE.md`: **Application зависит только на Domain** (не на Infrastructure); фильтрация —
> только через спецификации, не raw LINQ в хендлерах.

---

## 4. Доменная модель

### 4.1. Сводка

| Тип | Базовый | Роль |
|---|---|---|
| **`ProductBase`** | `AggregateRoot<Guid>` + `ICreateAtEntity` + `IUpdatedAtEntity` | **abstract** агрегат номенклатуры (расширяемый) |
| **`ProductCategory`** | `AggregateRoot<Guid>` | дерево категорий (`ParentId`, `Path`) |
| **`PriceList`** | `AggregateRoot<Guid>` + аудит | заголовок прайса (`Currency`, `IsDefault`, `ValidFrom/To`) |
| **`PriceListItem`** | `Entity<Guid>` (child of `PriceList`) | цена позиции (`ProductId`, `Price`, `MinQty?`) |
| generic `Specification<TProduct>` | Domain | `ProductBySku`, `ActiveProductsByCategory`, `ProductsSearch` |
| `Specification<…>` | Domain | категории/прайсы — конкретные спеки |

> Базовые классы ядра (сверено по Activities): `Entity<TId>`/`AggregateRoot<TId>` (короткая форма
> `AggregateRoot` для `Guid`); аудит-интерфейсы `ICreateAtEntity`/`IUpdatedAtEntity` объявляют
> **`DateTimeOffset?`** (не `DateTime`). `AddDomainEvent`/`DomainEvents`/`ClearDomainEvents` — на
> `AggregateRoot`. `DbContext` наследует `CrmDbContext<TContext>` и помечается
> `[ConnectionStringName(CatalogConstants.ConnectionStringName)]`.

### 4.2. Shared — enums и конвенции

```csharp
namespace Cheetah.Modules.Catalog.Shared;

public enum ProductType   { Goods = 0, Service = 1, DigitalGoods = 2, Bundle = 3 }
public enum UnitOfMeasure { Piece = 0, Hour = 1, Kilogram = 2, Liter = 3, Meter = 4, Pack = 5 }

public static class CatalogConstants
{
    public const string ConnectionStringName = "Catalog";
    public const string DefaultProductsRoutePrefix    = "/api/products";
    public const string DefaultCategoriesRoutePrefix  = "/api/categories";
    public const string DefaultPriceListsRoutePrefix  = "/api/price-lists";
}
```

### 4.3. `ProductBase` — точки расширения

Принципы абстрактности (как в Activities §4.3): нельзя `new` абстракцию в generic-handler → создание
через `protected InitializeCore(...)` + `IProductFactory`; мутаторы `virtual`; доп. поля наследника —
в `sealed Product : ProductBase` с `private set`.

```csharp
public abstract class ProductBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Sku { get; protected set; } = null!;
    public string Name { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public ProductType Type { get; protected set; }
    public UnitOfMeasure Unit { get; protected set; }
    public Guid? CategoryId { get; protected set; }
    public bool IsActive { get; protected set; }

    // «Быстрый» карман расширения без миграций (MVP; полноценно — Custom Fields).
    public string? Attributes { get; protected set; } // jsonb

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected ProductBase() { } // EF + наследник

    /// <summary>Инициализация ядра — вызывается фабрикой/Create наследника (замена new).</summary>
    protected void InitializeCore(Guid id, string sku, string name, ProductType type,
        UnitOfMeasure unit, Guid? categoryId, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id; Sku = sku.Trim(); Name = name.Trim(); Type = type; Unit = unit;
        CategoryId = categoryId; Description = description; IsActive = true;
        AddDomainEvent(new ProductCreatedIntegrationEvent(Id, Sku, Name, (int)Type));
    }

    public virtual void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        AddDomainEvent(new ProductUpdatedIntegrationEvent(Id));
    }

    public virtual void ChangeCategory(Guid? categoryId) => CategoryId = categoryId;

    public virtual void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new ProductDeactivatedIntegrationEvent(Id));
    }

    public virtual void Activate() => IsActive = true;
}
```

> Наследник: `public sealed class Product : ProductBase { public string? Brand { get; private set; }
> public decimal? WeightKg { get; private set; } /* + методы записи */ }` — ровно как
> `Customer : CustomerBase` / `Activity : ActivityBase`.

### 4.4. `ProductCategory` — дерево (конкретный агрегат)

```csharp
public sealed class ProductCategory : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public string Path { get; private set; } = null!;  // материализованный путь: "/root/electronics/phones"
    public int Order { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private ProductCategory() { }

    public static ProductCategory Create(string name, ProductCategory? parent)
    {
        var c = new ProductCategory { Id = Guid.NewGuid(), Name = name.Trim(), ParentId = parent?.Id };
        c.Path = parent is null ? $"/{c.Id}" : $"{parent.Path}/{c.Id}";
        return c;
    }
}
```

> `Path` хранит идентификаторы предков — выборка поддерева через `WHERE Path LIKE '{parent.Path}/%'`
> (индекс по `Path`). Альтернативы (`ltree`, рекурсивный CTE) — §1.2.

### 4.5. `PriceList` + `PriceListItem` (агрегат с дочерними строками)

```csharp
public sealed class PriceList : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<PriceListItem> _items = new();

    public string Name { get; private set; } = null!;
    public string Currency { get; private set; } = null!;   // ISO-4217, напр. "USD"
    public bool IsDefault { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public IReadOnlyList<PriceListItem> Items => _items;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private PriceList() { }

    public static PriceList Create(string name, string currency, bool isDefault) { /* … */ }

    public void SetPrice(Guid productId, decimal price, int? minQty = null)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.MinQty == minQty);
        if (item is null) _items.Add(PriceListItem.Create(Id, productId, price, minQty));
        else item.ChangePrice(price);
        AddDomainEvent(new PriceChangedIntegrationEvent(Id, productId, price, Currency));
    }

    public decimal? ResolvePrice(Guid productId, int qty)
        => _items.Where(i => i.ProductId == productId && (i.MinQty == null || qty >= i.MinQty))
                 .OrderByDescending(i => i.MinQty ?? 0)
                 .Select(i => (decimal?)i.Price).FirstOrDefault();
}

public sealed class PriceListItem : Entity<Guid>
{
    public Guid PriceListId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }
    public int? MinQty { get; private set; }   // порог количества для этой цены

    private PriceListItem() { }
    internal static PriceListItem Create(Guid priceListId, Guid productId, decimal price, int? minQty) { /* … */ }
    internal void ChangePrice(decimal price) => Price = price;
}
```

### 4.6. Domain — спецификации (фильтрация только через них)

```csharp
// generic по TProduct — расширяемый агрегат
public sealed class ProductBySkuSpecification<TProduct>(string sku) : Specification<TProduct>
    where TProduct : ProductBase
{
    public override Expression<Func<TProduct, bool>> ToExpression() => p => p.Sku == sku;
}

public sealed class ActiveProductsByCategorySpecification<TProduct>(Guid categoryId) : Specification<TProduct>
    where TProduct : ProductBase
{
    public override Expression<Func<TProduct, bool>> ToExpression()
        => p => p.IsActive && p.CategoryId == categoryId;
}

public sealed class ProductsSearchSpecification<TProduct>(string term, bool onlyActive) : Specification<TProduct>
    where TProduct : ProductBase
{
    public override Expression<Func<TProduct, bool>> ToExpression()
        => p => (!onlyActive || p.IsActive) &&
                (p.Name.Contains(term) || p.Sku.Contains(term));
}

// конкретные спеки для прайс-листов / категорий
public sealed class DefaultPriceListSpecification : Specification<PriceList>
{
    public override Expression<Func<PriceList, bool>> ToExpression() => pl => pl.IsDefault;
}
```

> Комбинаторы `And`/`Or`/`Not` — проверить наличие в `Cheetah.Core.Specification` (тот же открытый
> вопрос, что в Deals/Activities), нужны для `ListProductsQuery` с комбинацией фильтров.

---

## 5. Contracts — расширяемые ViewModel

Абстрактные `record`-базы для товара (наследник добавляет поля через `init`-свойства); для категорий
и прайс-листов — конкретные DTO. Все реализуют `ICrmResponse` (как в Deals/Activities).

```csharp
public abstract record ProductDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public ProductType Type { get; init; }
    public UnitOfMeasure Unit { get; init; }
    public Guid? CategoryId { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public abstract record CreateProductRequestBase
{
    public string Sku { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public ProductType Type { get; init; }
    public UnitOfMeasure Unit { get; init; } = UnitOfMeasure.Piece;
    public Guid? CategoryId { get; init; }
}

public abstract record UpdateProductRequestBase
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
}

// Конкретные DTO/Request (не расширяемые) для категорий и прайс-листов:
public sealed record ProductCategoryDto : ICrmResponse { /* Id, Name, ParentId, Path, Order */ }
public sealed record CreateCategoryRequest(string Name, Guid? ParentId);
public sealed record PriceListDto : ICrmResponse { /* Id, Name, Currency, IsDefault, Items[] */ }
public sealed record SetPriceRequest(Guid ProductId, decimal Price, int? MinQty);
public sealed record ResolvedPriceDto(Guid ProductId, decimal Price, string Currency);
```

> Наследник: `public sealed record ProductDto : ProductDtoBase { public string? Brand { get; init; } }`
> — ровно как `CustomerDto : CustomerDtoBase`.

---

## 6. Application — generic CQRS + фабрика/проектор

Generic-handler'ы закрываются конкретными типами наследника. Создание товара — через `IProductFactory`
(нельзя `new` абстракцию); проекция в DTO — через `IProductProjector` (вместо Mapster, чтобы доп. поля
не требовали скрытой регистрации). Сигнатуры — точная калька рабочих интерфейсов Activities
(`IActivityFactory<out TActivity, in TCreateRequest>` / `IActivityProjector<TActivity, TDto>`).

```csharp
public interface IProductFactory<out TProduct, in TCreateRequest>
    where TProduct : ProductBase where TCreateRequest : CreateProductRequestBase
{
    TProduct Create(TCreateRequest request);
}

public interface IProductProjector<in TProduct, out TDto>
    where TProduct : ProductBase where TDto : ProductDtoBase
{
    TDto ToDto(TProduct product);
}
```

**Команды / запросы:**

```csharp
// товары (generic по TProduct/TDto/TRequest)
CreateProductCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>;
UpdateProductCommand<TUpdateRequest>(Guid ProductId, TUpdateRequest Request) : ICommand;
DeactivateProductCommand(Guid ProductId) : ICommand;
ActivateProductCommand(Guid ProductId) : ICommand;
GetProductByIdQuery<TDto>(Guid ProductId) : IQuery<TDto?>;
ListProductsQuery<TDto>(string? Search, Guid? CategoryId, bool OnlyActive, int Page, int Size)
    : IQuery<IReadOnlyList<TDto>>;

// категории (конкретные)
CreateCategoryCommand(string Name, Guid? ParentId) : ICommand<Guid>;
ListCategoriesQuery(Guid? ParentId) : IQuery<IReadOnlyList<ProductCategoryDto>>;

// прайс-листы (конкретные)
CreatePriceListCommand(string Name, string Currency, bool IsDefault) : ICommand<Guid>;
SetPriceCommand(Guid PriceListId, Guid ProductId, decimal Price, int? MinQty) : ICommand;
GetPriceListByIdQuery(Guid PriceListId) : IQuery<PriceListDto?>;
ResolvePriceQuery(Guid? PriceListId, Guid ProductId, int Qty) : IQuery<ResolvedPriceDto?>; // default-прайс если null
```

**Канон хендлера** (из `CLAUDE.md` + паттерн Activities): получить агрегат через репозиторий → доменный
мутатор → `SaveChangesAsync` → опубликовать `DomainEvents` через `IEventBus` → `ClearDomainEvents`.
Фильтрация — только спецификациями. `ValueTask<T>` + `CancellationToken` всюду. Анти-дубль по `Sku` —
`ProductBySkuSpecification` + `ExistsAsync` перед созданием (бросать `CatalogValidationException`).

**Регистрация (extension-метод, открытые generic нельзя через `[Export]` — как в Activities):**

```csharp
services.AddCatalogApplication<Product, CreateProductRequest, UpdateProductRequest,
    ProductDto, ProductFactory, ProductProjector>();
// внутри: регистрирует фабрику/проектор + закрытые ICommandHandler<…>/IQueryHandler<…>
// для товаров; конкретные handler'ы категорий/прайсов берутся [Export]-генератором.
```

---

## 7. Infrastructure — EF Core + кэш цен

### 7.1. Абстрактные базы (расширяемая схема товара)

```csharp
public abstract class ProductConfigurationBase<TProduct> : IEntityTypeConfiguration<TProduct>
    where TProduct : ProductBase
{
    public void Configure(EntityTypeBuilder<TProduct> b)
    {
        b.ToTable("Products", "catalog");
        b.Property(p => p.Sku).HasMaxLength(64).IsRequired();
        b.Property(p => p.Name).HasMaxLength(300).IsRequired();
        b.Property(p => p.Type).HasConversion<int>();
        b.Property(p => p.Unit).HasConversion<int>();
        b.Property(p => p.Attributes).HasColumnType("jsonb");

        b.HasIndex(p => p.Sku).IsUnique();
        b.HasIndex(p => new { p.CategoryId, p.IsActive });

        b.Ignore(p => p.DomainEvents);  // CRITICAL
        ConfigureCustom(b);             // hook наследника: индексы/колонки доп. полей
    }

    protected virtual void ConfigureCustom(EntityTypeBuilder<TProduct> b) { }
}

[ConnectionStringName(CatalogConstants.ConnectionStringName)]
public abstract class CatalogDbContextBase<TContext, TProduct> : CrmDbContext<TContext>
    where TContext : DbContext where TProduct : ProductBase
{
    public DbSet<TProduct> Products => Set<TProduct>();
    public DbSet<ProductCategory> Categories => Set<ProductCategory>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();

    protected CatalogDbContextBase(DbContextOptions<TContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.ApplyConfiguration(CreateProductConfiguration());
        mb.ApplyConfiguration(new ProductCategoryConfiguration());
        mb.ApplyConfiguration(new PriceListConfiguration());
        mb.ApplyConfiguration(new PriceListItemConfiguration());
    }

    protected abstract IEntityTypeConfiguration<TProduct> CreateProductConfiguration();
}
```

> Из Customer/Activities: **абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`,
> `IDesignTimeDbContextFactory` и миграции принадлежат наследнику (или сборке `.Default`).
> Конфигурации `ProductCategory`/`PriceList`/`PriceListItem` — конкретные (не generic), т.к. сами
> сущности `sealed`.

**Регистрация:** `services.AddCatalogInfrastructure<CatalogDbContext, Product>();` — `AddDbContext`
(`UseNpgsql`), `IRepository<Product, Guid>`, `IRepository<ProductCategory, Guid>`,
`IRepository<PriceList, Guid>`, кэш-обёртка резолвера цен.

### 7.2. PriceList — конфигурация дочерних строк

`PriceListItem` — owned/child агрегата `PriceList`: `b.HasMany(pl => pl.Items).WithOne()
.HasForeignKey(i => i.PriceListId).OnDelete(DeleteBehavior.Cascade)`. Индекс
`(PriceListId, ProductId)` для быстрого резолва. Категория — self-reference по `ParentId`, индекс по
`Path`.

### 7.3. Кэш разрешения цены (горячий путь)

```csharp
// Цена читается часто (формирование документов), прайсы меняются редко → кэш.
[Export(LifetimeType.Scoped, typeof(IPriceResolver))]
public sealed class CachedPriceResolver : IPriceResolver
{
    // ключ "catalog:price:{priceListId}:{productId}:{qty}" → decimal? из Cheetah.Core.Cache;
    // инвалидация по PriceChangedIntegrationEvent (сброс ключей прайс-листа).
}
```

> Источник истины — БД (`PriceList.ResolvePrice`); кэш — поверх. Инвалидация по событию
> `PriceChanged` (подписка в `OnApplicationInitialization`, eventually consistent — допустимо).

---

## 8. Api (декларативные эндпоинты)

Декларативный стиль `Cheetah.Backend.Endpoints` (как модуль **Identity**): эндпоинты — наследники
`CreateCommandEndpoint`/`UpdateCommandEndpoint`/`DeleteCommandEndpoint`/`QueryOrNotFoundEndpoint`/
`QueryGridEndpoint`; маршруты регистрирует генератор `Cheetah.Generators.Endpoints` в
`OnApplicationInitialization` модуля; маппинг Request↔Command/Query и Entity→ViewModel — через Mapster
(`CatalogMappingProfile`); грид (пагинация/сортировка/фильтрация) — через `IGridRepository`.

**Категории и прайс-листы — конкретные эндпоинты (self-wired «из коробки»):**

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `api/categories` | `CreateCategoryCommand` |
| GET | `api/categories` | `GetCategoriesGridQuery` (грид) |
| GET | `api/categories/{id}` | `GetCategoryByIdQuery` |
| PUT | `api/categories/{id}` | `UpdateCategoryCommand` |
| DELETE | `api/categories/{id}` | `DeleteCategoryCommand` |
| POST | `api/price-lists` | `CreatePriceListCommand` |
| GET | `api/price-lists` | `GetPriceListsGridQuery` (грид) |
| GET | `api/price-lists/{id}` | `GetPriceListByIdQuery` (со строками) |
| PUT | `api/price-lists/{id}` | `UpdatePriceListCommand` |
| DELETE | `api/price-lists/{id}` | `DeletePriceListCommand` |
| POST | `api/price-lists/{id}/items` | `SetPriceCommand` |
| GET | `api/price-lists/{id}/price?productId=&qty=` | `ResolvePriceQuery` (default-прайс если id опущен) |

**Товары — абстрактные шаблоны эндпоинтов** (товар расширяем) → закрывает наследник/хост (как у
Identity): `CreateProductEndpoint<…>`, `GetProductByIdEndpoint<…>`, `UpdateProductEndpoint<…>`,
`DeleteProductEndpoint<…>`, `GetProductsGridEndpoint<…>`. После закрытия конкретными типами генератор
хоста регистрирует маршруты `api/products` (CRUD + grid). Авторизация — `Cheetah.Permissions`.

---

## 9. События (публикует Catalog)

```csharp
namespace Cheetah.Modules.Catalog.DomainEvents;

ProductCreatedIntegrationEvent(Guid ProductId, string Sku, string Name, int Type) : EventBase;
ProductUpdatedIntegrationEvent(Guid ProductId) : EventBase;
ProductDeactivatedIntegrationEvent(Guid ProductId) : EventBase;
PriceChangedIntegrationEvent(Guid PriceListId, Guid ProductId, decimal Price, string Currency) : EventBase;
```

Потребители: **Sales Documents** (`PriceChanged` → пометить открытые КП «цена устарела»; см. §5 плана
«снимок цены в строке»), Search (индексация товаров), Timeline/Analytics.

> Каталог — **источник** событий, не потребитель: внешних подписок на MVP нет (в отличие от Activities
> с `EntityDeleted`). Подписка на `PriceChanged` для инвалидации кэша — внутренняя (§7.3).

---

## 10. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | инварианты `ProductBase` (Sku/Name обязательны; `Deactivate` идемпотентен; событие при создании); `PriceList.SetPrice/ResolvePrice` (порог `MinQty`, выбор подходящей цены); `ProductCategory.Create` (построение `Path`); наследование (sealed `Product` с доп. полем создаётся фабрикой) |
| `Application.Tests` | generic-хендлеры с моками `IRepository`/`IEventBus`/фабрики/проектора (Moq — как в Activities): публикация событий, анти-дубль по `Sku`, `ResolvePriceQuery` (default-прайс), фильтры `ListProductsQuery` |
| `Client.Tests` | сериализация запросов/ответов HTTP-клиента, обработка ошибок (404/409) |
| (Default) | интеграционный smoke: создать товар → завести прайс → set price → resolve; миграция применяется |

---

## 11. План реализации (пошагово)

> Каждый шаг = отдельный коммит. После каждого слоя — `dotnet build` + `dotnet sln add` в
> `Cheetah.slnx`, папка `/Modules/Catalog/`. Пакеты — через `Directory.Packages.props`
> (`PackageReference` без `Version`). Перед коммитом — удалять `nul`-файлы.

**Фаза 0 — каркас**
1. Создать проекты по §3 (7 шаблонных + опц. `.Default` + `Client` + 3 тестовых), ссылки строго по
   порядку зависимостей. Добавить в `Cheetah.slnx`.

**Фаза 1 — контракты**
2. `DomainEvents`: 4 интеграционных события (§9).
3. `Shared`: enums `ProductType`/`UnitOfMeasure` + `CatalogConstants` (§4.2).
4. `Contracts`: абстрактные `ProductDtoBase`/`…RequestBase` + конкретные DTO категорий/прайсов (§5).

**Фаза 2 — домен**
5. `Domain`: `ProductBase` (+`InitializeCore`/мутаторы) — §4.3.
6. `Domain`: `ProductCategory` (дерево/`Path`), `PriceList` + `PriceListItem` (§4.4–4.5).
7. `Domain`: generic + конкретные спецификации (§4.6), при необходимости комбинаторы.
8. `Domain.Tests`: инварианты — **до** Infrastructure (домен без БД).

**Фаза 3 — инфраструктура**
9. `Infrastructure`: `ProductConfigurationBase<>` (+`ConfigureCustom` hook), конфигурации
   Category/PriceList/PriceListItem, `CatalogDbContextBase<>` (§7.1–7.2).
10. `Infrastructure`: `AddCatalogInfrastructure<>` (`AddDbContext`, репозитории).
11. `Infrastructure`: `IPriceResolver` + `CachedPriceResolver` (`Cheetah.Core.Cache`) + инвалидация
    по `PriceChanged` (§7.3).

**Фаза 4 — приложение**
12. `Application`: `IProductFactory`/`IProductProjector`, generic команды/запросы товаров + хендлеры (§6).
13. `Application`: конкретные хендлеры категорий/прайсов; `AddCatalogApplication<>`.
14. `Application.Tests`: хендлеры (события, анти-дубль Sku, resolve price, фильтры).

**Фаза 5 — API + (Default) + клиент**
15. `Api`: `ProductEndpointsBase<>` + `CategoryEndpoints`/`PriceListEndpoints` +
    `CheetahCatalogApiModuleBase<>` (§8).
16. (Опц.) `.Default`: sealed `Product`, конкретные Contracts, фабрика/проектор, `CatalogDbContext`
    + `IDesignTimeDbContextFactory` + **миграция** `InitialCatalog` (схема `catalog`), готовые
    Api/Infrastructure/Application-регистрации, опц. seed (default-прайс).
17. `Client`: `ICatalogClient` (`GetProductAsync`, `ResolvePriceAsync` — для Sales Documents) +
    реализация, `Client.Tests`.

**Фаза 6 — интеграция**
18. Подписка на `PriceChanged` для инвалидации кэша; проверить публикацию событий после `SaveChanges`.
19. End-to-end: создать товар → категория → прайс → set price → resolve (с `MinQty`) → деактивация.

**Фаза 7 — финал**
20. README модуля (базовый/шаблонный → по чек-листу `CLAUDE.md` README обязателен: назначение, точки
    расширения, extension-методы, пример наследования — по образцу Customer/Activities README).
21. Обновить статус этого плана на «реализовано», добавить ссылку на код; обновить `MEMORY.md`.

---

## 12. Зависимости от инфраструктуры

| Инфраструктурный модуль | Использование в Catalog |
|---|---|
| `Cheetah.Core.Domain` | `AggregateRoot`/`Entity`, аудит-интерфейсы (`DateTimeOffset?`) |
| `Cheetah.Core.DataAccess` (`IRepository<T>`) | репозитории + спецификации |
| `Cheetah.Core.Specification` | вся фильтрация (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` | `ICommand`/`IQuery`/`IDispatcher`/`IEventBus` |
| `Cheetah.Core.Cache` | кэш разрешения цены (горячий путь формирования документов) |
| `CrmEntityFrameworkModule` + `…PostgreSqlModule` | EF Core + Npgsql; `CrmDbContext<T>` |
| `Cheetah.Backend.Endpoints` + `Cheetah.Generators.Endpoints` | декларативные эндпоинты + генерация маршрутов |
| `Cheetah.Core.Grid` (`IGridRepository`) | грид: пагинация/сортировка/фильтрация + ProjectTo в ViewModel |
| `Cheetah.Mapping.Mapster` | Request↔Command/Query, Entity→ViewModel (`CatalogMappingProfile`) |
| `Cheetah.Permissions` | авторизация эндпоинтов |
| `Cheetah.Core.Tenants` | (опц.) мультитенантность каталога — §1.2 |

**Не используется (в отличие от других модулей):** StateMachine (у товара нет конечного автомата —
только `IsActive`), BackgroundTasks/DistributedLock (нет фоновых сканов), Outbox (события через
`IEventBus` после `SaveChanges`, как в Customer/Activities).

---

## 13. Отличия от исходного эскиза плана (`docs/plans.md` §4)

1. **Абстрактный шаблон вместо конкретного модуля** — по требованию расширяемости (§0). Эскиз §4 давал
   `sealed`-сущности; здесь — `ProductBase` + generic-хелперы по образцу Customer/Activities.
2. **Расширяем только `ProductBase`**, а `ProductCategory`/`PriceList`/`PriceListItem` — конкретные
   (как `ActivityReminder` в Activities: расширяем главный агрегат, дочерние/справочные — фиксированы).
3. **Аудит-поля — `DateTimeOffset?`** (сверено по `ICreateAtEntity`/`IUpdatedAtEntity` ядра).
4. **События — через `IEventBus` после `SaveChanges`** (паттерн Customer/Activities), а не Outbox.
5. **Эндпоинты — декларативные** наследники `Cheetah.Backend.Endpoints` + генератор (как Identity):
   категории/прайс-листы — конкретные (self-wired), товары — абстрактные шаблоны (host-closed). Полный
   CRUD + grid (через `IGridRepository`); маппинг — Mapster (`CatalogMappingProfile`).
6. **`Attributes (jsonb)`** как «быстрый» карман расширения на MVP до появления Custom Fields.
7. **Опциональная сборка `.Default`** — чтобы модуль работал «из коробки», оставаясь расширяемым
   (рекомендация; в Activities `.Default` была отложена в follow-up — здесь допустимо так же).

**Отложено (follow-up):** полноценная динамическая расширяемость через Custom Fields; справочник
типов/единиц вместо enum; полноценные скидочные правила; `ltree` для дерева категорий;
мультитенантность; gRPC для горячих списков/резолва цен; общий VO `Money` (вводится с Sales Documents).

---

## 14. Что реализовано (сверка с кодом)

Реализован **абстрактный шаблон** по образцу `Cheetah.Modules.Activities`/`Cheetah.Modules.Customer`
(требование §0 — расширяемость товара и его ViewModel). 7 шаблонных проектов + 2 тестовых, всё
собирается, **39 тестов зелёных** (Domain 15 + Application 24), полная солюшн `Cheetah.slnx` собирается
без ошибок. Эндпоинты — декларативные (`Cheetah.Backend.Endpoints` + генератор), полный CRUD + grid.

**Точки расширяемости (готовы):**

- сущность — `abstract ProductBase` + `protected InitializeCore(...)` + `virtual`-мутаторы; наследник
  объявляет `sealed class Product : ProductBase` со своими полями;
- ViewModel — `abstract record ProductDtoBase`; наследник — `sealed record ProductDto : ProductDtoBase`;
- запросы — `abstract record CreateProductRequestBase`/`UpdateProductRequestBase`;
- создание/проекция — `IProductFactory`/`IProductProjector` (наследник реализует);
- схема EF — `ProductConfigurationBase<TProduct>` + `ConfigureCustom`-hook;
- регистрация — `AddCatalogInfrastructure<TContext,TProduct>()` (вкл. `IGridRepository`) и
  `AddCatalogApplication<TProduct,TCreateRequest,TUpdateRequest,TDto,TGridViewModel,TFactory,TProjector>()`;
- эндпоинты товара — абстрактные шаблоны `CreateProductEndpoint<…>`/`GetProductByIdEndpoint<…>`/
  `UpdateProductEndpoint<…>`/`DeleteProductEndpoint<…>`/`GetProductsGridEndpoint<…>` (host-closed);
  grid-VM — `abstract ProductGridViewModelBase`;
- «быстрый» карман — колонка `Attributes (jsonb)` на `ProductBase`.

**Сознательные отличия от §0–§13 (как у работающего шаблона Activities):**

1. **Без транзакционного Outbox.** Хендлеры публикуют события через `IEventBus` после
   `SaveChangesAsync` (канон `CLAUDE.md` + паттерн Customer/Activities). Апгрейд до Outbox — follow-up.
2. **Категории/прайс-листы — конкретные типы** (не расширяемые), хендлеры регистрируются генератором
   по `[Export]`; расширяется только агрегат товара.
3. **Кэш разрешения цены не реализован** — `ResolvePriceQuery` читает прайс из БД напрямую
   (через `AutoInclude()` строк). `CachedPriceResolver` + инвалидация по `PriceChanged` — follow-up.
4. **Эндпоинты — декларативные** (`Cheetah.Backend.Endpoints` + генератор, как Identity): категории и
   прайс-листы конкретны и работают «из коробки»; товары — абстрактные шаблоны, закрываемые хостом.
   Grid (пагинация/сортировка/фильтрация) — через `IGridRepository`/`GetGridAsync`/`GetByIdAsync<VM>`.
5. **Lifecycle:** полный CRUD + grid для товаров (+ Deactivate/Activate), категорий и прайс-листов;
   у прайс-листов также SetPrice/ResolvePrice.
6. **Не вошло в этот инкремент (follow-up):** сборка `.Default` (готовая `sealed`-реализация +
   миграция) и `Client` (`ICatalogClient` для Sales Documents) — оба требуют закрытых типов и
   создаются наследником/в следующем шаге; подписка на `PriceChanged` для инвалидации кэша.
