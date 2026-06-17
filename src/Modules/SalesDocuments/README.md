# Cheetah.Modules.SalesDocuments.* — абстрактный шаблон-модуль «Коммерческие документы (КП / заказы / счета)»

> **Тип:** base/template (каркас), а не готовый микросервис.
> Модуль поставляет **абстрактный расширяемый документ** + конкретную строку и generic-хелперы.
> Конкретное приложение наследует тип документа, генерирует миграции у себя и получает рабочий
> жизненный цикл (Draft → Sent/Confirmed/Issued → …), расчёт итогов, нумерацию, PDF и события
> «из коробки», дописав ~6 классов. Сделан по образцу
> [`Cheetah.Modules.Catalog`](../Catalog/README.md) / [`Cheetah.Modules.Activities`](../Activities/README.md).
>
> Полный план и решения — [`docs/modules/sales-documents.md`](../../../docs/modules/sales-documents.md).

## Назначение

Коммерческие документы трёх связанных типов (`DocType`): **Quote** (КП), **Order** (заказ),
**Invoice** (счёт) — из строк-позиций со **снимком** наименования и цены из каталога (последующие
правки каталога документ не меняют). Считает итоги (`Subtotal`/`DiscountTotal`/`TaxTotal`/`GrandTotal`),
ведёт статусы через конечный автомат, присваивает номер при выпуске, генерирует PDF (FileStorage),
привязывается к сделке (`DealId`). Источник коммерческих событий (`QuoteAccepted` → Deals,
`DocumentSent`/`InvoiceOverdue` → Notification).

**Главное требование — расширяемость:** документ и его ViewModel наследуемы; приложение добавляет свои
поля (условия оплаты, доставка, реквизиты) без форка модуля. Строка (`SalesDocumentLine`) — конкретная
(как `PriceListItem` в Catalog): расширяется главный агрегат, дочерняя сущность фиксирована.

## Состав сборок и граф зависимостей

```
SalesDocuments.DomainEvents   → Core.Events                       (DocumentCreated/Sent, QuoteAccepted/Rejected, InvoicePaid/Overdue, DocumentCancelled)
SalesDocuments.Shared         → Core                              (enum DocType/DocumentStatus, Money, SalesDocumentsConstants)
SalesDocuments.Contracts      → Core + Contracts + Shared         (ABSTRACT SalesDocumentDtoBase/…RequestBase + конкретные DTO строк/операций)
SalesDocuments.Domain         → DomainEvents + Specification + StateMachine
                                                                  (abstract SalesDocumentBase; sealed SalesDocumentLine; спеки; порты)
SalesDocuments.Infrastructure → Domain + EF + EF.PostgreSql + FileStorage
                                                                  (abstract DbContextBase/ConfigBase, AddSalesDocumentsInfrastructure<>,
                                                                   номер-генератор, PDF-сервис, NullProductPricingPort)
SalesDocuments.Application     → Domain + Contracts + CQRS + Events + StateMachine
                                                                  (generic CQRS документа, фабрика/проектор, AddStateMachine<DocumentStatus>,
                                                                   AddSalesDocumentsApplication<>)
SalesDocuments.Api            → Application + Contracts + Backend.Endpoints + Mapster
                                                                  (абстрактные шаблоны CRUD+grid документа + конкретные эндпоинты операций)
Tests: Domain.Tests (15), Application.Tests (7)
```

`Default` и `Client` отсутствуют — их создаёт наследник / следующий инкремент (нужны закрытые типы).

## Ключевые абстракции

| Тип | Сборка | Роль |
|---|---|---|
| `SalesDocumentBase : AggregateRoot<Guid>, IStateMachineEntity<DocumentStatus>` | Domain | агрегат документа; `InitializeCore`, `AddLine`/`RemoveLine`, `Recalculate`, `Issue`, `Accept`/`Reject`, `MarkPaid`/`MarkOverdue`, `Cancel`, `AttachPdf` |
| `SalesDocumentLine : Entity<Guid>` | Domain | строка-снимок (`ProductId`, `Name`, `UnitPrice`, `Qty`, `DiscountPercent`, `TaxRate`) + вычисляемые суммы |
| `*Specification<TDoc>` | Domain | generic-спеки: ByNumber, Filter, DraftQuotesByProduct, OverdueInvoices |
| `IDocumentNumberGenerator` / `IDocumentPdfService` / `IProductPricingPort` | Domain | исходящие порты (реализация — Infrastructure/адаптеры) |
| `SalesDocumentDtoBase` + `Create/UpdateDocumentRequestBase` + `…GridViewModelBase` | Contracts | абстрактные record (точка расширения ViewModel/запросов) |
| `SalesDocumentsDbContextBase<TContext, TDoc>` | Infrastructure | `DbSet` документов + строк |
| `SalesDocumentConfigurationBase<TDoc>` | Infrastructure | таблица/схема, уникальный `(DocType, Number)`, индексы, `ConfigureCustom` hook |
| `ISalesDocumentFactory`/`ISalesDocumentProjector` | Application | `Create(...)`/`CreateForConversion(...)`/`ToDto(...)` — замена `new`/Mapster |
| generic CQRS документа | Application | Create/Update + AddLine/RemoveLine + Issue/Accept/Reject/Pay/Cancel + Convert + Pdf + GetById/List/Grid |
| декларативные эндпоинты | Api | абстрактные шаблоны CRUD+grid (host-closed) + конкретные эндпоинты операций |

### Архитектурные приёмы абстрактности

- **Нельзя `new` абстрактную сущность** в generic-handler → фабрика `ISalesDocumentFactory` + `InitializeCore`.
- **Открытые generic-handler'ы** документа регистрируются вручную в `AddSalesDocumentsApplication<>`
  (атрибут `[Export]` работает только по закрытым типам).
- **Абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`, design-time factory и миграции
  принадлежат наследнику (или будущей сборке `.Default`).
- **Статус-машина единая** (`DocumentStatus`), допустимость перехода уточняют доменные guard'ы по `DocType`.
- **Исходящие порты в Domain** (номер/PDF/цена) — Application не зависит от Infrastructure/FileStorage/Catalog.Client.

## Как использовать (наследник)

```csharp
// 1. Сущность
public sealed class SalesDocument : SalesDocumentBase
{
    public string? PaymentTerms { get; private set; }
    private SalesDocument() { }
    public static SalesDocument Create(DocType type, Guid customerId, Guid ownerId, string currency,
        Guid? dealId, DateTimeOffset? validUntil, string? paymentTerms)
    {
        var d = new SalesDocument();
        d.InitializeCore(Guid.NewGuid(), type, customerId, ownerId, currency, dealId, validUntil);
        d.PaymentTerms = paymentTerms;
        return d;
    }
    public static SalesDocument CreateForConversion(SalesDocumentBase src, DocType toType)
    {
        var d = new SalesDocument();
        d.InitializeCore(Guid.NewGuid(), toType, src.CustomerId, src.OwnerId, src.Currency,
            src.DealId, src.ValidUntil, src.Id);
        return d;
    }
}

// 2. Contracts: sealed record SalesDocumentDto : SalesDocumentDtoBase { public string? PaymentTerms { get; init; } }
//    + CreateDocumentRequest : CreateDocumentRequestBase, UpdateDocumentRequest, GridViewModel, GetById/Delete/Grid requests.
// 3. Factory : ISalesDocumentFactory<SalesDocument, CreateDocumentRequest>, Projector : ISalesDocumentProjector<…>.
// 4. DbContext : SalesDocumentsDbContextBase<AppDbContext, SalesDocument> + конфигурация + миграция.
// 5. Регистрация в модулях наследника:
services.AddSalesDocumentsInfrastructure<AppDbContext, SalesDocument>();
services.AddSalesDocumentsApplication<SalesDocument, CreateDocumentRequest, UpdateDocumentRequest,
    SalesDocumentDto, SalesDocumentGridViewModel, SalesDocumentFactory, SalesDocumentProjector>();
// 6. Закрыть абстрактные эндпоинты документа конкретными Request/Command/Query/Dto (как у товара в Catalog).
//    Реальный адаптер каталога: зарегистрировать свой IProductPricingPort над Catalog.Client.
```

## Сознательные отличия / follow-up

- **Без транзакционного Outbox** — события через `IEventBus` после `SaveChanges` (как Catalog/Activities).
- **PDF — плейсхолдер** (`FileStorageDocumentPdfService` рендерит текст): полноценный шаблонизатор — follow-up.
- **Нумерация — простая** (счёт по типу/году + уникальный индекс): без-дырочный секвенс/DistributedLock — follow-up.
- **`IProductPricingPort` по умолчанию пустой** (`NullProductPricingPort`): адаптер над Catalog.Client — follow-up;
  до него строки добавляются с явной ценой (`UnitPriceOverride`).
- **Не вошли** сборки `.Default` (готовая `sealed`-реализация + миграция) и `Client`, фоновый скан
  просрочки счетов (`InvoiceOverdue`), вынос `Money` в Core, мультитенантность.
