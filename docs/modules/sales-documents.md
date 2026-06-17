# Cheetah.Modules.SalesDocuments.* — модуль «Коммерческие документы: КП / заказы / счета» (расширяемый шаблон)

> Статус: **реализовано (MVP абстрактного шаблона).** Код — в `src/Modules/SalesDocuments/`. 7 шаблонных
> проектов + 2 тестовых собираются, полная солюшн `Cheetah.slnx` собирается без ошибок; тесты зелёные:
> Domain (15), Application (7) = 22. Эндпоинты — декларативные (`Cheetah.Backend.Endpoints` + генератор):
> абстрактные шаблоны CRUD+grid документа (host-closed) + конкретные эндпоинты операций (строки +
> жизненный цикл). Краткий гайд по расширению — `src/Modules/SalesDocuments/README.md`. Что вошло и
> сознательные отличия — в §14.
> Документ — пошаговый план сборки модуля по канону `CLAUDE.md`
> (Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client)),
> своя БД PostgreSQL, общение через REST/gRPC + события через шину.
>
> Источник: раздел [§5](../plans.md#5-sales-documents-кп-заказы-счета) общего плана. Это **п.4
> рекомендуемого порядка реализации** (после [Catalog](./catalog.md), строгий порядок: документы
> собираются из позиций каталога). Tier 1 (Deals → Activities → Leads) и Catalog уже реализованы.
>
> **Tier 2 — коммерческий контур.** Формирует документы трёх связанных типов — **Quote** (КП),
> **Order** (заказ), **Invoice** (счёт) — из строк-позиций (ссылка на `Product` + **снимок** цены и
> наименования + кол-во + скидка + налог). Считает итоги, ведёт статусы через `StateMachine`,
> генерирует PDF (`FileStorage`), привязывается к сделке (`Deal`).

---

## 0. Главное требование — расширяемость

> **Модуль обязан быть расширяемым: главный агрегат и его ViewModel должны допускать расширение
> полями приложения-наследника без форка модуля** — ровно как уже реализованные
> [`Cheetah.Modules.Customer`](../../src/Modules/Customer/README.md),
> [`Cheetah.Modules.Activities`](./activities.md) и [`Cheetah.Modules.Catalog`](./catalog.md).

Коммерческие документы по природе требуют доп. полей под конкретный бизнес: условия оплаты
(`PaymentTerms`), способ доставки и адрес, менеджер-подписант, банковские реквизиты, НДС-режим, ссылки
на договор, печатные комментарии, язык документа. Поэтому SalesDocuments строится как **абстрактный
шаблон-модуль**: модуль поставляет **абстрактные базовые типы и generic-хелперы**, а наследник
дописывает свои `sealed`-типы со своими полями.

**Что расширяемо (главный агрегат), а что — нет.** По образцу Catalog (расширяем `ProductBase`,
конкретен `PriceListItem`) и Activities (расширяем `ActivityBase`, конкретен `ActivityReminder`):

| Тип | Форма | Обоснование |
|---|---|---|
| **`SalesDocumentBase`** | **abstract** (расширяемый) | главный агрегат; почти всегда нужны доп. поля (условия оплаты, доставка, реквизиты) |
| **`SalesDocumentLine`** | concrete (`sealed`, child of документа) | строка-снимок (Product, Name, UnitPrice, Qty, Discount, TaxRate) — стабильна |

**Что это даёт наследнику:**

- `sealed class SalesDocument : SalesDocumentBase` со своими полями (`PaymentTerms`, `ShipTo`,
  `Notes`, `Language`…);
- `sealed record SalesDocumentDto : SalesDocumentDtoBase` со своими полями в ответе API;
- свои `Create/Update`-Request-ы (наследники `…RequestBase`);
- рабочий жизненный цикл (Draft → Sent → … через `StateMachine`), расчёт итогов, нумерация, PDF и
  события — «из коробки», дописав ~6–7 классов.

**Три уровня расширяемости** (как в Catalog §0 / Activities §0):

1. **Структурная (compile-time).** Наследование `SalesDocumentBase`/`SalesDocumentDtoBase` +
   `ConfigureCustom` hook в EF-конфигурации + фабрика/проектор наследника. Сильная типизация, индексы
   по доп. полям.
2. **Динамическая (runtime, без миграций).** «Быстрый» карман — опц. колонка `Attributes (jsonb)` на
   `SalesDocumentBase`; полноценно — будущий модуль [Custom Fields](../plans.md#7-custom-fields-кастомные-поля).
3. **Поведенческая.** `virtual`-мутаторы агрегата; `virtual`-методы эндпоинтов; `DocType`/`DocumentStatus`
   как данные-расширяемые точки (enum на MVP).

---

## 1. Назначение и границы

**Что делает:** ведёт коммерческие документы (Quote/Order/Invoice), их строки-позиции (со снимком
цены и наименования из каталога), статусы, итоги, нумерацию, PDF и связи со сделкой/клиентом. Отвечает
на вопрос «что, кому, на какую сумму выставлено и в каком состоянии этот документ».

**Чего НЕ делает:**

- не считает бухгалтерию/проводки/НДС-декларации — это ERP (модуль хранит коммерческие документы и их
  жизненный цикл, налог — справочно на строке);
- не ведёт справочник номенклатуры и цены — это [Catalog](./catalog.md) (документ **читает** цену
  через `ICatalogClient` и делает **снимок** в строку; последующие правки каталога документ не меняют);
- не управляет воронкой — [Deals](./deals.md) (документ ссылается на `DealId?`, событие
  `QuoteAccepted` потребляет Deals, чтобы двигать сделку в `Won`);
- не доставляет письма/PDF клиенту — публикует `DocumentSent`, доставку делает Notification.

**Связи (по `Id`, без FK через границу модуля):** `CustomerId`, `DealId?`, `ProductId` (в строке —
логическая ссылка на Catalog), `OwnerId`, `PdfFileId?` (логическая ссылка на FileStorage),
`SourceDocumentId?` (внутри модуля — настоящий FK на исходный документ при «создать на основе»).

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Форма модуля | **абстрактный шаблон** (как Customer/Activities/Catalog) — расширяемый `SalesDocumentBase`/`SalesDocumentDtoBase`. §0 |
| Идентификатор | `Guid` |
| Хранилище | PostgreSQL (EF Core); миграции — **у наследника** (или сборка `.Default`) |
| Один агрегат vs три | **один агрегат `SalesDocumentBase` с дискриминатором `DocType`** (Quote/Order/Invoice) — как §5 плана; общий жизненный цикл, статус-машина с guard'ами по `DocType` |
| Строки | concrete `SalesDocumentLine` (child агрегата) — **снимок** `Name`/`UnitPrice` на момент добавления |
| Статусы | enum `DocumentStatus` под `IStateMachineEntity<DocumentStatus>`; переходы — `StateMachine` + доменные guard'ы по `DocType` (паттерн Deals §4.4) |
| Деньги | value object `Money` (Amount + Currency); валюта одна на документ. Источник `Money` — §1.2 |
| Итоги | `Subtotal`/`DiscountTotal`/`TaxTotal`/`GrandTotal` пересчитываются агрегатом при изменении строк (доменный метод `Recalculate()`) |
| Цена строки | **снимок** из Catalog (`ICatalogClient.ResolvePriceAsync`) при добавлении; `PriceChanged` НЕ меняет уже выставленный документ, только метит `Draft`-КП «цена устарела» (§9) |
| Нумерация | последовательная per `DocType` per год; через `Cheetah.DistributedLock` или БД-секвенс. Дыры — §1.2 |
| PDF | рендер в Infrastructure (`IDocumentPdfRenderer`) → `Cheetah.FileStorage`; в документе только `PdfFileId` |
| Quote→Order→Invoice | операция «создать на основе» (копия строк + `SourceDocumentId`) |
| События | через `IEventBus` после `SaveChangesAsync` (паттерн Customer/Activities/Catalog; **не** Outbox на MVP) |
| Эндпоинты | **декларативные** `Cheetah.Backend.Endpoints` + генератор (стиль Identity/Catalog); grid через `IGridRepository` |
| «Быстрый» карман | опц. `Attributes (jsonb)` на `SalesDocumentBase` (MVP), полноценно — Custom Fields |
| Просрочка счёта | фоновый скан `ScanOverdueInvoicesTask` (`BackgroundTasks` + `DistributedLock`) → `InvoiceOverdue` — **follow-up** (как механика Activities) |
| Soft-delete | терминальные статусы запрещают правки; `Cancel()` вместо физического удаления |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Общий `Money`.** Сейчас `Money` живёт в `Cheetah.Modules.Deals.Shared` (самодостаточный `record`,
   зависит только на Core, помечен как кандидат на вынос). Сквозное решение №5 плана: ввести общий
   `Money` именно с Sales Documents. Варианты:
   - **(рекомендация)** вынести `Money` в `Cheetah.Core.Domain` (или новый `Cheetah.Core.Money`) и
     переиспользовать в Deals/Catalog/SalesDocuments — единый VO мультивалютности;
   - локальная копия `Money` в `SalesDocuments.Shared` (быстро, но дублирование);
   - ссылка на `Deals.Shared` (плохо — кросс-модульная зависимость Shared↔Shared).
   На MVP допустимо стартовать с локальной копии и вынести в Core отдельным рефактором.
2. **Сборка `.Default` «из коробки».** По образцу Catalog §1.2 / Activities — рекомендация **гибрид**:
   отдельная `Cheetah.Modules.SalesDocuments.Default` с `sealed SalesDocument`, конкретными Contracts,
   `DbContext` и миграциями (в Catalog/Activities `.Default` отложили в follow-up — здесь так же
   допустимо).
3. **Нумерация документов:** без дыр (БД-секвенс per DocType/год через `DistributedLock`, гарантия
   последовательности) vs допускаются дыры (проще, отдельный счётчик). Формат номера — конвенция
   `{Prefix}-{Year}-{Seq}` (напр. `INV-2026-000123`).
4. **PDF на MVP:** заглушка/простой HTML→PDF-рендер vs полноценный шаблонизатор. Рекомендация —
   абстракция `IDocumentPdfRenderer` + минимальный рендер на MVP, полноценные шаблоны — follow-up.
5. **Статус-модель:** единый enum `DocumentStatus` со всеми значениями + guard'ы по `DocType`
   (рекомендация, как Deal) vs отдельный enum на каждый `DocType`. Единый проще для generic-хендлеров.
6. **Скидка строки:** процент vs абсолютная сумма; налог — ставка на строке (`TaxRate`) vs внешний
   налоговый калькулятор. MVP — скидка процентом + ставка налога на строке.
7. **Мультитенантность** (`Cheetah.Core.Tenants`) — глобально vs per-tenant; нумерация per-tenant. MVP — глобально.

---

## 2. Архитектурная роль

```
   ┌────────────────────────────────────────────────────────────────────┐
   │                  SalesDocuments Module (шаблон)                     │
   │  ┌──────────────────────────────────────────┐  1──*  ┌───────────┐ │
   │  │ SalesDocumentBase (abstract)             │────────│ SalesDoc- │ │
   │  │  DocType, Number, Status, Money totals,  │        │ umentLine │ │
   │  │  CustomerId, DealId?, PdfFileId?,         │        │ (snapshot:│ │
   │  │  SourceDocumentId?                        │        │  Name,    │ │
   │  └──────────────────┬───────────────────────┘        │  UnitPrice│ │
   │                     │ наследует                       │  Qty,     │ │
   │     sealed SalesDocument (поля приложения)            │  Discount,│ │
   │                                                       │  TaxRate) │ │
   │  StateMachine<DocumentStatus> + guard'ы по DocType    └───────────┘ │
   └───────────────┬────────────────────────────────────────────────────┘
                   │ publish (IEventBus → Redis/Kafka)
                   ▼
   DocumentCreated / DocumentSent / QuoteAccepted / QuoteRejected /
   InvoiceIssued / InvoicePaid / InvoiceOverdue / DocumentCancelled
                   │
                   ├──▶ Deals      (QuoteAccepted → двигать сделку в Won)
                   ├──▶ Notification (DocumentSent → письмо клиенту с PDF)
                   └──◀ Catalog    (PriceChanged → метить Draft-КП «цена устарела»)
   ┌────────────────────────────────────────────────────────────────────┐
   │ Infrastructure: ICatalogClient (снимок цены), IDocumentPdfRenderer  │
   │   → FileStorage (PdfFileId), IDocumentNumberGenerator (DistLock)     │
   └────────────────────────────────────────────────────────────────────┘
```

---

## 3. Структура проектов

```
src/Modules/SalesDocuments/
├── Cheetah.Modules.SalesDocuments.DomainEvents/   # DocumentCreated/Sent/QuoteAccepted/InvoicePaid/Overdue/Cancelled (Core.Events)
├── Cheetah.Modules.SalesDocuments.Shared/          # enums (DocType, DocumentStatus), Money (или ссылка на Core), SalesDocumentsConstants
├── Cheetah.Modules.SalesDocuments.Contracts/       # ABSTRACT DTO/Request базы (SalesDocumentDtoBase, …RequestBase) + конкретные DTO строк/итогов
├── Cheetah.Modules.SalesDocuments.Domain/          # abstract SalesDocumentBase; sealed SalesDocumentLine; generic-спеки; StateMachine-маркер
├── Cheetah.Modules.SalesDocuments.Infrastructure/  # EF Core, abstract DbContextBase/ConfigBase, нумерация, PDF-рендер, AddSalesDocumentsInfrastructure<>
├── Cheetah.Modules.SalesDocuments.Application/      # generic handlers, фабрика/проектор, StateMachine-конфиг, AddSalesDocumentsApplication<>
├── Cheetah.Modules.SalesDocuments.Api/             # декларативные эндпоинты (Backend.Endpoints) + SalesDocumentsMappingProfile
├── (опц.) Cheetah.Modules.SalesDocuments.Default/  # sealed SalesDocument + конкретные Contracts + DbContext + миграции «из коробки»
├── Cheetah.Modules.SalesDocuments.Client/          # HTTP-клиент server-to-server (Deals → статус КП, и т.п.)
└── Tests/
    ├── Cheetah.Modules.SalesDocuments.Domain.Tests/
    ├── Cheetah.Modules.SalesDocuments.Application.Tests/
    └── Cheetah.Modules.SalesDocuments.Client.Tests/
```

**Порядок зависимостей (строго, как в Customer/Activities/Catalog):**

```
DomainEvents (Core.Events)
   ↓
Shared (Core)                                    ← enums + Money
   ↓
Contracts (Core + Shared)                        ← ABSTRACT базы документа + конкретные DTO строк/итогов
   ↓
Domain (DomainEvents + Specification + StateMachine) ← abstract SalesDocumentBase, sealed Line, generic-спеки
   ↓
Infrastructure (Domain + EF + EF.PostgreSql + FileStorage + DistributedLock + Catalog.Client)
Application (Domain + Contracts + CQRS + Events)  ← generic handlers, StateMachine-конфиг, AddSalesDocumentsApplication<>
Api (Application + Contracts + AspNetCore)        ← abstract …EndpointsBase<>, ApiModuleBase
   ↓
Default (наследует всё) + Client (Contracts)
```

> Канон `CLAUDE.md`: **Application зависит только на Domain** (не на Infrastructure); фильтрация —
> только через спецификации, не raw LINQ в хендлерах. `ICatalogClient` — внешний порт, инжектируется
> в хендлер из Catalog.Client (зарегистрирован хостом).

---

## 4. Доменная модель

### 4.1. Сводка

| Тип | Базовый | Роль |
|---|---|---|
| **`SalesDocumentBase`** | `AggregateRoot<Guid>` + `IStateMachineEntity<DocumentStatus>` + `ICreateAtEntity` + `IUpdatedAtEntity` | **abstract** агрегат документа (расширяемый) |
| **`SalesDocumentLine`** | `Entity<Guid>` (child of документа) | строка-снимок (`ProductId`, `Name`, `UnitPrice`, `Qty`, `Discount`, `TaxRate`, `LineTotal`) |
| generic `Specification<TDoc>` | Domain | `DocumentByNumber`, `DocumentsByCustomer`, `OpenDocumentsByProduct`, `OverdueInvoices` |

> Базовые классы ядра (сверено по Deals/Catalog): `Entity<TId>`/`AggregateRoot<TId>` (короткая форма
> `AggregateRoot` для `Guid`); аудит-интерфейсы `ICreateAtEntity`/`IUpdatedAtEntity` объявляют
> **`DateTimeOffset?`**. `IStateMachineEntity<TState>` объявляет `TState State` (см. `Deal.State`).
> `DbContext` наследует `CrmDbContext<TContext>` и помечается `[ConnectionStringName(...)]`.

### 4.2. Shared — enums, Money и конвенции

```csharp
namespace Cheetah.Modules.SalesDocuments.Shared;

public enum DocType { Quote = 0, Order = 1, Invoice = 2 }

// Единый набор статусов всех типов; допустимость перехода зависит от DocType (guard'ы домена).
public enum DocumentStatus
{
    Draft = 0,
    // Quote
    Sent = 1, Accepted = 2, Rejected = 3, Expired = 4,
    // Order
    Confirmed = 5, Fulfilled = 6,
    // Invoice
    Issued = 7, Paid = 8, Overdue = 9,
    // общий терминальный
    Cancelled = 10
}

public static class SalesDocumentsConstants
{
    public const string ConnectionStringName = "SalesDocuments";
    public const string DefaultRoutePrefix = "/api/sales-documents";
}
```

> `Money` — см. открытый вопрос §1.2. На MVP — самодостаточный `record Money` в этом `Shared`
> (копия из `Deals.Shared`), с задачей вынести в `Cheetah.Core.Domain` отдельным рефактором.

### 4.3. `SalesDocumentBase` — точки расширения

Принципы абстрактности (как в Catalog §4.3 / Activities §4.3): нельзя `new` абстракцию в
generic-handler → создание через `protected InitializeCore(...)` + `ISalesDocumentFactory`; мутаторы
`virtual`; статус — через `StateMachine` (Application) + доменные guard'ы; доп. поля наследника — в
`sealed SalesDocument : SalesDocumentBase` с `private set`.

```csharp
public abstract class SalesDocumentBase : AggregateRoot<Guid>,
    IStateMachineEntity<DocumentStatus>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<SalesDocumentLine> _lines = new();

    public DocType DocType { get; protected set; }
    public string Number { get; protected set; } = null!;     // присваивается при выпуске (§7.3)
    public DocumentStatus Status { get; protected set; }
    public Guid CustomerId { get; protected set; }
    public Guid? DealId { get; protected set; }
    public Guid OwnerId { get; protected set; }
    public string Currency { get; protected set; } = null!;   // ISO-4217, одна на документ

    public decimal Subtotal { get; protected set; }
    public decimal DiscountTotal { get; protected set; }
    public decimal TaxTotal { get; protected set; }
    public decimal GrandTotal { get; protected set; }

    public DateTimeOffset? ValidUntil { get; protected set; }  // для КП/счёта
    public Guid? PdfFileId { get; protected set; }
    public Guid? SourceDocumentId { get; protected set; }      // «создан на основе»
    public string? Attributes { get; protected set; }          // jsonb — «быстрый» карман

    public IReadOnlyList<SalesDocumentLine> Lines => _lines;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public DocumentStatus State => Status;

    protected SalesDocumentBase() { } // EF + наследник

    /// <summary>Инициализация ядра — вызывается фабрикой/Create наследника (замена new).</summary>
    protected void InitializeCore(Guid id, DocType docType, Guid customerId, Guid ownerId,
        string currency, Guid? dealId, Guid? sourceDocumentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        Id = id; DocType = docType; CustomerId = customerId; OwnerId = ownerId;
        Currency = currency.Trim().ToUpperInvariant();
        DealId = dealId; SourceDocumentId = sourceDocumentId;
        Status = DocumentStatus.Draft; Number = string.Empty;
        AddDomainEvent(new DocumentCreatedIntegrationEvent(Id, (int)docType, customerId, ownerId));
    }

    // --- строки (правки разрешены только в Draft) ---
    public virtual void AddLine(Guid productId, string name, decimal unitPrice, decimal qty,
        decimal discountPercent, decimal taxRate)
    {
        EnsureDraft();
        _lines.Add(SalesDocumentLine.Create(Id, productId, name, unitPrice, qty, discountPercent, taxRate));
        Recalculate();
    }

    public virtual void RemoveLine(Guid lineId)
    {
        EnsureDraft();
        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line is not null) { _lines.Remove(line); Recalculate(); }
    }

    protected void Recalculate()
    {
        Subtotal      = _lines.Sum(l => l.UnitPrice * l.Qty);
        DiscountTotal = _lines.Sum(l => l.DiscountAmount);
        TaxTotal      = _lines.Sum(l => l.TaxAmount);
        GrandTotal    = Subtotal - DiscountTotal + TaxTotal;
    }

    // --- жизненный цикл (переходы валидирует StateMachine в Application + guard'ы DocType) ---
    public virtual void Issue(string number)         // Draft → Sent (Quote) / Issued (Invoice) / Confirmed (Order)
    {
        EnsureDraft();
        if (_lines.Count == 0) throw new InvalidOperationException("Document must have at least one line.");
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        Number = number;
        Status = DocType switch
        {
            DocType.Quote   => DocumentStatus.Sent,
            DocType.Order   => DocumentStatus.Confirmed,
            DocType.Invoice => DocumentStatus.Issued,
            _ => throw new InvalidOperationException()
        };
        AddDomainEvent(new DocumentSentIntegrationEvent(Id, (int)DocType, Number, CustomerId, GrandTotal, Currency));
    }

    public virtual void Accept()   // Quote: Sent → Accepted
    {
        EnsureDocType(DocType.Quote);
        TransitionTo(DocumentStatus.Accepted);
        AddDomainEvent(new QuoteAcceptedIntegrationEvent(Id, DealId, CustomerId, GrandTotal, Currency));
    }

    public virtual void Reject(string? reason)   // Quote: Sent → Rejected
    {
        EnsureDocType(DocType.Quote);
        TransitionTo(DocumentStatus.Rejected);
        AddDomainEvent(new QuoteRejectedIntegrationEvent(Id, reason));
    }

    public virtual void MarkPaid()   // Invoice: Issued/Overdue → Paid
    {
        EnsureDocType(DocType.Invoice);
        TransitionTo(DocumentStatus.Paid);
        AddDomainEvent(new InvoicePaidIntegrationEvent(Id, CustomerId, GrandTotal, Currency));
    }

    public virtual void MarkOverdue() // Invoice: Issued → Overdue (фоновый скан)
    {
        if (DocType != DocType.Invoice || Status != DocumentStatus.Issued) return;
        Status = DocumentStatus.Overdue;
        AddDomainEvent(new InvoiceOverdueIntegrationEvent(Id, CustomerId, GrandTotal, Currency));
    }

    public virtual void Cancel(string? reason)
    {
        if (Status is DocumentStatus.Paid or DocumentStatus.Cancelled)
            throw new InvalidOperationException($"Document {Id} cannot be cancelled ({Status}).");
        Status = DocumentStatus.Cancelled;
        AddDomainEvent(new DocumentCancelledIntegrationEvent(Id, reason));
    }

    public void AttachPdf(Guid fileId) => PdfFileId = fileId;

    protected void TransitionTo(DocumentStatus next) => Status = next; // фактический guard — StateMachine
    private void EnsureDraft() { if (Status != DocumentStatus.Draft) throw new InvalidOperationException("Lines editable only in Draft."); }
    private void EnsureDocType(DocType t) { if (DocType != t) throw new InvalidOperationException($"Operation valid only for {t}."); }
}
```

> Наследник: `public sealed class SalesDocument : SalesDocumentBase { public string? PaymentTerms
> { get; private set; } public string? ShipTo { get; private set; } /* + методы записи */ }` — ровно
> как `Customer : CustomerBase` / `Product : ProductBase`.
>
> **StateMachine vs доменные guard'ы.** Терминальность и разрешённость переходов конфигурируется в
> Application через `AddStateMachine<DocumentStatus>` (§6.3) и проверяется `IStateMachineValidator`
> в хендлерах (паттерн Deals §4.4). Доменные методы держат инварианты, специфичные для `DocType`
> (нельзя `Accept` заказ, нельзя править строки вне `Draft`).

### 4.4. `SalesDocumentLine` — строка-снимок (конкретная сущность)

```csharp
public sealed class SalesDocumentLine : Entity<Guid>
{
    public Guid DocumentId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;     // снимок наименования
    public decimal UnitPrice { get; private set; }        // снимок цены из Catalog
    public decimal Qty { get; private set; }
    public decimal DiscountPercent { get; private set; }  // 0..100
    public decimal TaxRate { get; private set; }          // 0..100

    public decimal LineSubtotal  => UnitPrice * Qty;
    public decimal DiscountAmount => LineSubtotal * DiscountPercent / 100m;
    public decimal TaxAmount      => (LineSubtotal - DiscountAmount) * TaxRate / 100m;
    public decimal LineTotal      => LineSubtotal - DiscountAmount + TaxAmount;

    private SalesDocumentLine() { }

    internal static SalesDocumentLine Create(Guid documentId, Guid productId, string name,
        decimal unitPrice, decimal qty, decimal discountPercent, decimal taxRate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        return new SalesDocumentLine
        {
            Id = Guid.NewGuid(), DocumentId = documentId, ProductId = productId,
            Name = name.Trim(), UnitPrice = unitPrice, Qty = qty,
            DiscountPercent = discountPercent, TaxRate = taxRate
        };
    }
}
```

> Снимок (`Name`/`UnitPrice`) фиксирует документ относительно последующих правок каталога — счёт не
> меняется от `PriceChanged`. `LineTotal`/итоги — вычисляемые (можно материализовать колонками при
> необходимости отчётности).

### 4.5. Domain — спецификации (фильтрация только через них)

```csharp
public sealed class DocumentByNumberSpecification<TDoc>(string number) : Specification<TDoc>
    where TDoc : SalesDocumentBase
{
    public override Expression<Func<TDoc, bool>> ToExpression() => d => d.Number == number;
}

public sealed class DocumentsByCustomerSpecification<TDoc>(Guid customerId, DocType? type) : Specification<TDoc>
    where TDoc : SalesDocumentBase
{
    public override Expression<Func<TDoc, bool>> ToExpression()
        => d => d.CustomerId == customerId && (type == null || d.DocType == type);
}

// для пометки «цена устарела» по PriceChanged: открытые КП, содержащие товар
public sealed class DraftQuotesByProductSpecification<TDoc>(Guid productId) : Specification<TDoc>
    where TDoc : SalesDocumentBase
{
    public override Expression<Func<TDoc, bool>> ToExpression()
        => d => d.DocType == DocType.Quote && d.Status == DocumentStatus.Draft
                && d.Lines.Any(l => l.ProductId == productId);
}

// для фонового скана просрочки
public sealed class OverdueInvoicesSpecification<TDoc>(DateTimeOffset asOf) : Specification<TDoc>
    where TDoc : SalesDocumentBase
{
    public override Expression<Func<TDoc, bool>> ToExpression()
        => d => d.DocType == DocType.Invoice && d.Status == DocumentStatus.Issued
                && d.ValidUntil != null && d.ValidUntil < asOf;
}
```

> Комбинаторы `And`/`Or`/`Not` — проверить наличие в `Cheetah.Core.Specification` (тот же открытый
> вопрос, что в Deals/Catalog), нужны для `ListDocumentsQuery` с комбинацией фильтров.

---

## 5. Contracts — расширяемые ViewModel

Абстрактные `record`-базы для документа (наследник добавляет поля через `init`); для строк и итогов —
конкретные DTO. Все реализуют `ICrmResponse` (как в Deals/Activities/Catalog).

```csharp
public abstract record SalesDocumentDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public DocType DocType { get; init; }
    public string Number { get; init; } = null!;
    public DocumentStatus Status { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? DealId { get; init; }
    public Guid OwnerId { get; init; }
    public string Currency { get; init; } = null!;
    public decimal Subtotal { get; init; }
    public decimal DiscountTotal { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal GrandTotal { get; init; }
    public DateTimeOffset? ValidUntil { get; init; }
    public Guid? PdfFileId { get; init; }
    public Guid? SourceDocumentId { get; init; }
    public IReadOnlyList<SalesDocumentLineDto> Lines { get; init; } = [];
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public abstract record CreateDocumentRequestBase
{
    public DocType DocType { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? DealId { get; init; }
    public string Currency { get; init; } = null!;
    public DateTimeOffset? ValidUntil { get; init; }
    public IReadOnlyList<AddLineRequest> Lines { get; init; } = [];
}

public abstract record UpdateDocumentRequestBase
{
    public Guid? DealId { get; init; }
    public DateTimeOffset? ValidUntil { get; init; }
}

// Конкретные DTO/Request (не расширяемые):
public sealed record SalesDocumentLineDto(Guid Id, Guid ProductId, string Name, decimal UnitPrice,
    decimal Qty, decimal DiscountPercent, decimal TaxRate, decimal LineTotal);
public sealed record AddLineRequest(Guid ProductId, decimal Qty, decimal? UnitPriceOverride,
    decimal DiscountPercent, decimal TaxRate);   // UnitPrice по умолчанию — из Catalog
public sealed record RejectQuoteRequest(string? Reason);
public sealed record CancelDocumentRequest(string? Reason);
public sealed record ConvertDocumentRequest(DocType ToType);
```

> Наследник: `public sealed record SalesDocumentDto : SalesDocumentDtoBase { public string?
> PaymentTerms { get; init; } }` — ровно как `CustomerDto : CustomerDtoBase`.

---

## 6. Application — generic CQRS + фабрика/проектор + StateMachine

Generic-handler'ы закрываются конкретными типами наследника. Создание документа — через
`ISalesDocumentFactory` (нельзя `new` абстракцию); проекция в DTO — через `ISalesDocumentProjector`.
Сигнатуры — калька рабочих интерфейсов Activities/Catalog.

```csharp
public interface ISalesDocumentFactory<out TDoc, in TCreateRequest>
    where TDoc : SalesDocumentBase where TCreateRequest : CreateDocumentRequestBase
{
    TDoc Create(TCreateRequest request);
}

public interface ISalesDocumentProjector<in TDoc, out TDto>
    where TDoc : SalesDocumentBase where TDto : SalesDocumentDtoBase
{
    TDto ToDto(TDoc document);
}
```

### 6.1. Команды / запросы

```csharp
// документы (generic по TDoc/TDto/TRequest)
CreateDocumentCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>;
UpdateDocumentCommand<TUpdateRequest>(Guid DocumentId, TUpdateRequest Request) : ICommand;
AddLineCommand(Guid DocumentId, AddLineRequest Line) : ICommand;          // цена из Catalog → снимок
RemoveLineCommand(Guid DocumentId, Guid LineId) : ICommand;
IssueDocumentCommand(Guid DocumentId) : ICommand;                         // присвоить Number + выпустить
AcceptQuoteCommand(Guid DocumentId) : ICommand;
RejectQuoteCommand(Guid DocumentId, string? Reason) : ICommand;
MarkInvoicePaidCommand(Guid DocumentId) : ICommand;
CancelDocumentCommand(Guid DocumentId, string? Reason) : ICommand;
ConvertDocumentCommand(Guid DocumentId, DocType ToType) : ICommand<Guid>; // создать на основе
GenerateDocumentPdfCommand(Guid DocumentId) : ICommand<Guid>;            // рендер → FileStorage → PdfFileId
GetDocumentByIdQuery<TDto>(Guid DocumentId) : IQuery<TDto?>;
ListDocumentsQuery<TDto>(Guid? CustomerId, DocType? Type, DocumentStatus? Status, int Page, int Size)
    : IQuery<IReadOnlyList<TDto>>;
```

### 6.2. Канон хендлера

Из `CLAUDE.md` + паттерн Deals/Catalog: получить агрегат через репозиторий → (для переходов статуса)
проверить `IStateMachineValidator<DocumentStatus>.Validate(from, to)` → доменный мутатор →
`SaveChangesAsync` → опубликовать `DomainEvents` через `IEventBus` → `ClearDomainEvents`. Фильтрация —
только спецификациями. `ValueTask<T>` + `CancellationToken` всюду.

- **`AddLineCommand`** инжектит `ICatalogClient`: если `UnitPriceOverride` не задан — берёт цену через
  `ResolvePriceAsync(productId, qty)` и наименование товара, делает **снимок** в строку.
- **`IssueDocumentCommand`** инжектит `IDocumentNumberGenerator` → присваивает `Number` (§7.3) → `Issue`.
- **`ConvertDocumentCommand`** читает исходный документ, создаёт через фабрику новый `DocType` с копией
  строк и `SourceDocumentId` = исходный (Quote→Order→Invoice).

### 6.3. StateMachine — конфигурация (в Application-модуле)

```csharp
services.AddStateMachine<DocumentStatus>(sm => sm
    .From(DocumentStatus.Draft).To(DocumentStatus.Sent, DocumentStatus.Confirmed,
        DocumentStatus.Issued, DocumentStatus.Cancelled)
    .From(DocumentStatus.Sent).To(DocumentStatus.Accepted, DocumentStatus.Rejected,
        DocumentStatus.Expired, DocumentStatus.Cancelled)
    .From(DocumentStatus.Issued).To(DocumentStatus.Paid, DocumentStatus.Overdue, DocumentStatus.Cancelled)
    .From(DocumentStatus.Overdue).To(DocumentStatus.Paid, DocumentStatus.Cancelled)
    .From(DocumentStatus.Confirmed).To(DocumentStatus.Fulfilled, DocumentStatus.Cancelled));
```

> Имена API StateMachine свериться с `src/Cheetah.Core.StateMachine` при реализации (`AddStateMachine<TState>`,
> `IStateMachineValidator<TState>.Validate`, `IStateMachineEntity<TState>`, `InvalidStateTransitionException`) —
> ровно как сделано в Deals.

### 6.4. Регистрация (extension-метод, открытые generic нельзя через `[Export]`)

```csharp
services.AddSalesDocumentsApplication<SalesDocument, CreateDocumentRequest, UpdateDocumentRequest,
    SalesDocumentDto, SalesDocumentFactory, SalesDocumentProjector>();
// внутри: фабрика/проектор + закрытые ICommandHandler<…>/IQueryHandler<…> + AddStateMachine<DocumentStatus>.
// Хендлеры без generic-параметров (AddLine, Issue, Accept, …) — закрываются по TDoc.
```

---

## 7. Infrastructure — EF Core + нумерация + PDF

### 7.1. Абстрактные базы (расширяемая схема документа)

```csharp
public abstract class SalesDocumentConfigurationBase<TDoc> : IEntityTypeConfiguration<TDoc>
    where TDoc : SalesDocumentBase
{
    public void Configure(EntityTypeBuilder<TDoc> b)
    {
        b.ToTable("SalesDocuments", "sales");
        b.Property(d => d.DocType).HasConversion<int>();
        b.Property(d => d.Status).HasConversion<int>();
        b.Property(d => d.Number).HasMaxLength(64);
        b.Property(d => d.Currency).HasMaxLength(3).IsRequired();
        b.Property(d => d.Subtotal).HasPrecision(18, 2);
        b.Property(d => d.DiscountTotal).HasPrecision(18, 2);
        b.Property(d => d.TaxTotal).HasPrecision(18, 2);
        b.Property(d => d.GrandTotal).HasPrecision(18, 2);
        b.Property(d => d.Attributes).HasColumnType("jsonb");

        b.HasMany(d => d.Lines).WithOne().HasForeignKey(l => l.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(d => d.Lines).AutoInclude();

        b.HasIndex(d => new { d.DocType, d.Number }).IsUnique();
        b.HasIndex(d => new { d.CustomerId, d.DocType });
        b.HasIndex(d => new { d.DocType, d.Status });

        b.Ignore(d => d.DomainEvents);  // CRITICAL
        ConfigureCustom(b);             // hook наследника: индексы/колонки доп. полей
    }

    protected virtual void ConfigureCustom(EntityTypeBuilder<TDoc> b) { }
}

[ConnectionStringName(SalesDocumentsConstants.ConnectionStringName)]
public abstract class SalesDocumentsDbContextBase<TContext, TDoc> : CrmDbContext<TContext>
    where TContext : DbContext where TDoc : SalesDocumentBase
{
    public DbSet<TDoc> Documents => Set<TDoc>();
    public DbSet<SalesDocumentLine> Lines => Set<SalesDocumentLine>();

    protected SalesDocumentsDbContextBase(DbContextOptions<TContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.ApplyConfiguration(CreateDocumentConfiguration());
        mb.ApplyConfiguration(new SalesDocumentLineConfiguration());
    }

    protected abstract IEntityTypeConfiguration<TDoc> CreateDocumentConfiguration();
}
```

> Из Customer/Catalog: **абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`,
> `IDesignTimeDbContextFactory` и миграции принадлежат наследнику (или сборке `.Default`). Конфигурация
> `SalesDocumentLine` — конкретная (сущность `sealed`); `LineTotal`/`DiscountAmount`/… — `Ignore`
> (вычисляемые) либо `HasComputedColumnSql`/материализация — открытый вопрос отчётности.

**Регистрация:** `services.AddSalesDocumentsInfrastructure<SalesDocumentsDbContext, SalesDocument>();` —
`AddDbContext` (`UseNpgsql`), `IRepository<SalesDocument, Guid>`, `IGridRepository`,
`IDocumentNumberGenerator`, `IDocumentPdfRenderer`.

### 7.2. Снимок цены из Catalog

Порт `ICatalogClient` (из `Cheetah.Modules.Catalog.Client`) — `ResolvePriceAsync(productId, qty)` +
`GetProductAsync(productId)` (наименование). Инжектится в `AddLineCommandHandler`. Если Catalog.Client
ещё не реализован (см. catalog.md §14 — отложен в follow-up) — это **предпосылка**: реализовать
`ICatalogClient` либо временно принимать `UnitPriceOverride`/`Name` в запросе.

### 7.3. Нумерация (`IDocumentNumberGenerator`)

```csharp
public interface IDocumentNumberGenerator
{
    // Последовательный номер per DocType per год; формат {Prefix}-{Year}-{Seq:000000}.
    ValueTask<string> NextAsync(DocType docType, CancellationToken ct = default);
}
```

Реализация — БД-секвенс per (DocType, год) либо счётчик-таблица под `Cheetah.DistributedLock` (один
генератор номера в кластере → без гонок). Дыры/последовательность — §1.2.

### 7.4. PDF (`IDocumentPdfRenderer` → FileStorage)

```csharp
public interface IDocumentPdfRenderer
{
    ValueTask<byte[]> RenderAsync(SalesDocumentBase document, CancellationToken ct = default);
}
```

`GenerateDocumentPdfCommand` → `RenderAsync` → сохранить через `Cheetah.FileStorage`
(`IFileStorage.SaveAsync`) → `document.AttachPdf(fileId)`. MVP — простой рендер (см. §1.2 п.4);
полноценные шаблоны — follow-up.

---

## 8. Api (декларативные эндпоинты)

Декларативный стиль `Cheetah.Backend.Endpoints` (как Identity/Catalog): эндпоинты — наследники
`CreateCommandEndpoint`/`UpdateCommandEndpoint`/`DeleteCommandEndpoint`/`QueryOrNotFoundEndpoint`/
`QueryGridEndpoint`; маршруты регистрирует генератор `Cheetah.Generators.Endpoints` в
`OnApplicationInitialization`; маппинг Request↔Command/Query и Entity→ViewModel — Mapster
(`SalesDocumentsMappingProfile`); грид — `IGridRepository`.

**Документ — абстрактные шаблоны эндпоинтов** (агрегат расширяем) → закрывает наследник/хост (как
товары в Catalog): `CreateDocumentEndpoint<…>`, `GetDocumentByIdEndpoint<…>`, `UpdateDocumentEndpoint<…>`,
`DeleteDocumentEndpoint<…>`, `GetDocumentsGridEndpoint<…>`.

**Операции жизненного цикла / строк — конкретные эндпоинты:**

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `api/sales-documents` | `CreateDocumentCommand<…>` |
| GET | `api/sales-documents` | `GetDocumentsGridQuery` (грид) |
| GET | `api/sales-documents/{id}` | `GetDocumentByIdQuery<…>` (со строками) |
| PUT | `api/sales-documents/{id}` | `UpdateDocumentCommand<…>` |
| DELETE | `api/sales-documents/{id}` | `CancelDocumentCommand` (soft) |
| POST | `api/sales-documents/{id}/lines` | `AddLineCommand` (цена из Catalog → снимок) |
| DELETE | `api/sales-documents/{id}/lines/{lineId}` | `RemoveLineCommand` |
| POST | `api/sales-documents/{id}/issue` | `IssueDocumentCommand` (присвоить номер + выпустить) |
| POST | `api/sales-documents/{id}/accept` | `AcceptQuoteCommand` |
| POST | `api/sales-documents/{id}/reject` | `RejectQuoteCommand` |
| POST | `api/sales-documents/{id}/pay` | `MarkInvoicePaidCommand` |
| POST | `api/sales-documents/{id}/convert` | `ConvertDocumentCommand` `{ toType }` |
| POST | `api/sales-documents/{id}/pdf` | `GenerateDocumentPdfCommand` |
| GET | `api/sales-documents/{id}/pdf` | отдать файл по `PdfFileId` (FileStorage) |

Авторизация — `Cheetah.Permissions`.

---

## 9. События (публикует SalesDocuments)

```csharp
namespace Cheetah.Modules.SalesDocuments.DomainEvents;

DocumentCreatedIntegrationEvent(Guid DocumentId, int DocType, Guid CustomerId, Guid OwnerId) : EventBase;
DocumentSentIntegrationEvent(Guid DocumentId, int DocType, string Number, Guid CustomerId, decimal GrandTotal, string Currency) : EventBase;
QuoteAcceptedIntegrationEvent(Guid DocumentId, Guid? DealId, Guid CustomerId, decimal GrandTotal, string Currency) : EventBase;
QuoteRejectedIntegrationEvent(Guid DocumentId, string? Reason) : EventBase;
InvoicePaidIntegrationEvent(Guid DocumentId, Guid CustomerId, decimal GrandTotal, string Currency) : EventBase;
InvoiceOverdueIntegrationEvent(Guid DocumentId, Guid CustomerId, decimal GrandTotal, string Currency) : EventBase;
DocumentCancelledIntegrationEvent(Guid DocumentId, string? Reason) : EventBase;
```

Потребители: **Deals** (`QuoteAccepted` → двигать сделку в `Won`), **Notification** (`DocumentSent` →
письмо клиенту с PDF; `InvoiceOverdue` → напоминание), Timeline/Analytics.

**Подписка SalesDocuments (потребитель):** `PriceChangedIntegrationEvent` из Catalog (§9 catalog.md) →
найти `Draft`-КП с этим товаром (`DraftQuotesByProductSpecification`) и пометить «цена устарела»
(колонка-флаг или доменное событие) — реализация в `OnApplicationInitialization` Application-модуля.

---

## 10. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | расчёт итогов (`Recalculate`: subtotal/discount/tax/grand на наборе строк); правки строк только в `Draft`; `Issue` присваивает статус по `DocType` и требует ≥1 строки; guard'ы `Accept`/`MarkPaid` по `DocType`; `Cancel` запрещён для `Paid`/`Cancelled`; наследование (sealed `SalesDocument` с доп. полем создаётся фабрикой); снимок цены в `SalesDocumentLine` |
| `Application.Tests` | generic-хендлеры с моками `IRepository`/`IEventBus`/фабрики/проектора/`ICatalogClient`/`IDocumentNumberGenerator` (Moq, как в Catalog): публикация событий после `SaveChanges`; `AddLine` берёт цену из Catalog (снимок); `Issue` присваивает номер; StateMachine-валидатор переходов; `Convert` копирует строки + `SourceDocumentId` |
| `Client.Tests` | сериализация запросов/ответов HTTP-клиента, обработка ошибок (404/409) |
| (Default) | интеграционный smoke: создать КП → добавить строки → issue → accept → convert в Invoice → pay; миграция применяется |

---

## 11. План реализации (пошагово)

> Каждый шаг = отдельный коммит. После каждого слоя — `dotnet build` + `dotnet sln add` в
> `Cheetah.slnx`, папка `/Modules/SalesDocuments/`. Пакеты — через `Directory.Packages.props`
> (`PackageReference` без `Version`). Перед коммитом — удалять `nul`-файлы.

**Фаза 0 — каркас**
1. Создать проекты по §3 (7 шаблонных + опц. `.Default` + `Client` + 3 тестовых), ссылки строго по
   порядку зависимостей. Добавить в `Cheetah.slnx`.

**Фаза 1 — контракты**
2. `DomainEvents`: 7 интеграционных событий (§9).
3. `Shared`: enums `DocType`/`DocumentStatus` + `Money` (копия, §1.2) + `SalesDocumentsConstants` (§4.2).
4. `Contracts`: абстрактные `SalesDocumentDtoBase`/`…RequestBase` + конкретные DTO строк/итогов (§5).

**Фаза 2 — домен**
5. `Domain`: `SalesDocumentBase` (+`InitializeCore`/`Recalculate`/жизненный цикл/`IStateMachineEntity`) — §4.3.
6. `Domain`: `SalesDocumentLine` (снимок + вычисляемые суммы) — §4.4.
7. `Domain`: generic-спецификации (§4.5), при необходимости комбинаторы.
8. `Domain.Tests`: инварианты и расчёт итогов — **до** Infrastructure (домен без БД).

**Фаза 3 — инфраструктура**
9. `Infrastructure`: `SalesDocumentConfigurationBase<>` (+`ConfigureCustom`), `SalesDocumentLineConfiguration`,
   `SalesDocumentsDbContextBase<>` (§7.1).
10. `Infrastructure`: `AddSalesDocumentsInfrastructure<>` (`AddDbContext`, репозитории, `IGridRepository`).
11. `Infrastructure`: `IDocumentNumberGenerator` (DistributedLock/секвенс) + `IDocumentPdfRenderer`
    (FileStorage) — §7.3–7.4.

**Фаза 4 — приложение**
12. `Application`: `ISalesDocumentFactory`/`ISalesDocumentProjector`, generic команды/запросы + хендлеры (§6.1–6.2).
13. `Application`: `AddStateMachine<DocumentStatus>` (§6.3); `AddSalesDocumentsApplication<>` (§6.4);
    подписка на `PriceChanged` (§9).
14. `Application.Tests`: хендлеры (события, снимок цены, номер, StateMachine, convert).

**Фаза 5 — API + (Default) + клиент**
15. `Api`: `DocumentEndpointsBase<>` + конкретные эндпоинты строк/жизненного цикла +
    `CheetahSalesDocumentsApiModuleBase<>` (§8).
16. (Опц.) `.Default`: sealed `SalesDocument`, конкретные Contracts, фабрика/проектор, `DbContext` +
    `IDesignTimeDbContextFactory` + **миграция** `InitialSalesDocuments` (схема `sales`), готовые
    регистрации, опц. seed.
17. `Client`: `ISalesDocumentsClient` (`GetDocumentAsync`, `GetByDealAsync`) + реализация, `Client.Tests`.

**Фаза 6 — интеграция**
18. Подписка на `PriceChanged` (метить Draft-КП); проверить публикацию событий после `SaveChanges`.
19. End-to-end: КП → строки (цена из Catalog) → issue (номер) → accept → convert в Order/Invoice → pay → PDF.

**Фаза 7 — финал**
20. README модуля (по чек-листу `CLAUDE.md`: назначение, точки расширения, extension-методы, пример
    наследования — по образцу Customer/Catalog README).
21. Обновить статус этого плана на «реализовано», добавить ссылку на код; обновить `MEMORY.md`.

---

## 12. Зависимости от инфраструктуры

| Инфраструктурный модуль | Использование в SalesDocuments |
|---|---|
| `Cheetah.Core.Domain` | `AggregateRoot`/`Entity`, аудит-интерфейсы (`DateTimeOffset?`) |
| `Cheetah.Core.StateMachine` | переходы статуса документа (`IStateMachineEntity<DocumentStatus>`, валидатор) |
| `Cheetah.Core.DataAccess` (`IRepository<T>`) | репозитории + спецификации |
| `Cheetah.Core.Specification` | вся фильтрация (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` | `ICommand`/`IQuery`/`IDispatcher`/`IEventBus` |
| `CrmEntityFrameworkModule` + `…PostgreSqlModule` | EF Core + Npgsql; `CrmDbContext<T>` |
| `Cheetah.FileStorage` (+ `.Local`/`.S3`) | хранение PDF (`PdfFileId`) |
| `Cheetah.DistributedLock` (+ `.Postgres`) | нумерация без гонок; фоновый скан просрочки |
| `Cheetah.BackgroundTasks` | (follow-up) скан просрочки счетов → `InvoiceOverdue` |
| `Cheetah.Modules.Catalog.Client` (`ICatalogClient`) | снимок цены/наименования в строку |
| `Cheetah.Backend.Endpoints` + `Cheetah.Generators.Endpoints` | декларативные эндпоинты + генерация маршрутов |
| `Cheetah.Core.Grid` (`IGridRepository`) | грид: пагинация/сортировка/фильтрация + ProjectTo в ViewModel |
| `Cheetah.Mapping.Mapster` | Request↔Command/Query, Entity→ViewModel (`SalesDocumentsMappingProfile`) |
| `Cheetah.Permissions` | авторизация эндпоинтов |
| `Cheetah.Core.Tenants` | (опц.) мультитенантность + нумерация per-tenant — §1.2 |

**Не используется на MVP:** Outbox (события через `IEventBus` после `SaveChanges`, как
Customer/Activities/Catalog; апгрейд — follow-up); Saga (нет распределённых компенсаций на MVP).

---

## 13. Отличия от исходного эскиза плана (`docs/plans.md` §5)

1. **Абстрактный шаблон вместо конкретного модуля** — по требованию расширяемости (§0). Эскиз §5 давал
   `sealed`-сущности; здесь — `SalesDocumentBase` + generic-хелперы по образцу Customer/Activities/Catalog.
2. **Расширяем только `SalesDocumentBase`**, а `SalesDocumentLine` — конкретная (как `PriceListItem` в
   Catalog / `ActivityReminder` в Activities: расширяем главный агрегат, дочерние — фиксированы).
3. **Один агрегат с дискриминатором `DocType`** (как в §5 плана), единый `DocumentStatus` под
   `StateMachine` + доменные guard'ы по `DocType` (паттерн Deals для статуса).
4. **Аудит-поля — `DateTimeOffset?`** (сверено по ядру и Deals/Catalog).
5. **События — через `IEventBus` после `SaveChanges`** (паттерн Customer/Activities/Catalog), не Outbox.
6. **Эндпоинты — декларативные** наследники `Cheetah.Backend.Endpoints` + генератор (как Identity/Catalog):
   документ — абстрактные шаблоны (host-closed), операции строк/жизненного цикла — конкретные. Полный
   CRUD + grid (через `IGridRepository`); маппинг — Mapster.
7. **`Attributes (jsonb)`** как «быстрый» карман расширения на MVP до появления Custom Fields.
8. **`Money`** — общий VO (вынос из Deals.Shared в Core — рекомендация §1.2; на MVP локальная копия).
9. **Опциональная сборка `.Default`** — чтобы модуль работал «из коробки», оставаясь расширяемым
   (как Catalog; в Activities/Catalog `.Default` отложена в follow-up — здесь допустимо так же).

**Отложено (follow-up):** фоновый скан просрочки счетов (`InvoiceOverdue` через BackgroundTasks);
полноценный PDF-шаблонизатор; транзакционный Outbox; Saga для сложных конвертаций; gRPC для горячих
списков; вынос `Money` в Core; мультитенантность и нумерация per-tenant; полноценные скидочные правила.

---

## 14. Что реализовано (сверка с кодом)

Реализован **абстрактный шаблон** по образцу `Cheetah.Modules.Catalog`/`Cheetah.Modules.Activities`
(требование §0 — расширяемость документа и его ViewModel). 7 шаблонных проектов + 2 тестовых, всё
собирается, **22 теста зелёных** (Domain 15 + Application 7), полная солюшн `Cheetah.slnx` собирается
без ошибок. Эндпоинты — декларативные (`Cheetah.Backend.Endpoints` + генератор).

**Точки расширяемости (готовы):**

- сущность — `abstract SalesDocumentBase` + `protected InitializeCore(...)` + `virtual`-мутаторы;
  наследник объявляет `sealed class SalesDocument : SalesDocumentBase` со своими полями;
- ViewModel — `abstract record SalesDocumentDtoBase`/`SalesDocumentGridViewModelBase`;
- запросы — `abstract record CreateDocumentRequestBase`/`UpdateDocumentRequestBase` + `GetById/Delete/Grid` базы;
- создание/проекция — `ISalesDocumentFactory` (+ `CreateForConversion`) / `ISalesDocumentProjector`;
- схема EF — `SalesDocumentConfigurationBase<TDoc>` + `ConfigureCustom`-hook;
- регистрация — `AddSalesDocumentsInfrastructure<TContext,TDoc>()` и
  `AddSalesDocumentsApplication<TDoc,TCreateRequest,TUpdateRequest,TDto,TGridViewModel,TFactory,TProjector>()`;
- эндпоинты документа — абстрактные шаблоны `CreateDocumentEndpoint<…>`/`GetDocumentByIdEndpoint<…>`/
  `UpdateDocumentEndpoint<…>`/`GetDocumentsGridEndpoint<…>` (host-closed);
- «быстрый» карман — колонка `Attributes (jsonb)` на `SalesDocumentBase`.

**Сознательные отличия от §0–§13 (как у работающих шаблонов Catalog/Activities):**

1. **Один агрегат `SalesDocumentBase` с дискриминатором `DocType`** + единый `DocumentStatus` под
   `IStateMachineEntity`; переходы валидируются `IStateMachineValidator` в хендлерах + доменные guard'ы
   по `DocType`. Строка `SalesDocumentLine` — конкретная (расширяется только агрегат).
2. **Исходящие порты вынесены в Domain** (`IDocumentNumberGenerator`, `IDocumentPdfService`,
   `IProductPricingPort`) — чтобы Application зависел только на Domain (канон `CLAUDE.md`), а не на
   Infrastructure/FileStorage/Catalog.Client. Реализации — в Infrastructure.
3. **PDF-сервис рендерит плейсхолдер-текст** (`FileStorageDocumentPdfService` → FileStorage); реальный
   шаблонизатор — follow-up. Порт объединяет рендер+сохранение и возвращает `PdfFileId`.
4. **Нумерация — простая** (`SequentialDocumentNumberGenerator`: счёт по типу/году + уникальный индекс
   `(DocType, Number)`); без-дырочный секвенс/DistributedLock — follow-up.
5. **`IProductPricingPort` по умолчанию пустой** (`NullProductPricingPort`); до адаптера над
   Catalog.Client строки добавляются с явной ценой (`UnitPriceOverride`). Снимок цены/наименования
   фиксируется в строке при добавлении.
6. **Эндпоинты — декларативные** (`Cheetah.Backend.Endpoints` + генератор, как Identity/Catalog):
   операции строк/жизненного цикла — конкретные (self-wired); CRUD+grid документа — абстрактные
   шаблоны, закрываемые хостом. Маппинг операций Request→Command — `SalesDocumentsMappingProfile` (Mapster).
7. **Без транзакционного Outbox** — события через `IEventBus` после `SaveChangesAsync`.
8. **`Money`** — локальная копия в `Shared` (как у Deals); вынос в Core — follow-up.

**Не вошло в этот инкремент (follow-up):** сборки `.Default` (готовая `sealed`-реализация + миграция)
и `Client`; адаптер `IProductPricingPort` над Catalog.Client; подписка на `PriceChanged` (метить
Draft-КП «цена устарела»); фоновый скан просрочки счетов; PDF-GET эндпоинт (отдача файла); gRPC.
