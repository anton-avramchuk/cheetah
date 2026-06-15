# Cheetah.Modules.Deals — модуль «Сделки / Воронка»

> Статус: **реализовано (MVP)**. Код — в `src/Modules/Deals/`. Покрытие тестами: Domain (22),
> Application (4), Client (3) — все зелёные; вся солюшн собирается. Реализация следует паттернам
> модуля Calendar: транзакционный Outbox для интеграционных событий и декларативные эндпоинты
> (`Cheetah.Backend.Endpoints` + генератор). Отличия от исходного эскиза плана отмечены в §13.
>
> Источник: разделы [§1](../plans.md) и [§D](../plans.md) общего плана `docs/plans.md`. Здесь они
> сведены в пошаговый план сборки модуля по канону `CLAUDE.md`:
> Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client),
> своя БД PostgreSQL, общение через REST/gRPC + события через шину.
>
> **Tier 1 — сердце CRM.** Без сущности «сделка» и воронки продукт остаётся адресной книгой.
> Модуль идёт первым в порядке реализации: он обкатывает инфраструктурный
> `Cheetah.Core.StateMachine` на реальном домене.

---

## 1. Назначение и границы

**Что делает:** ведёт сделки (opportunities) по стадиям воронки. Сделка — потенциальная продажа
с суммой, ответственным, ожидаемой датой закрытия и текущей стадией. Воронка (pipeline) —
упорядоченный набор стадий со своими вероятностями. Фиксирует историю переходов между стадиями
для метрик (win rate, средний цикл, время в стадии).

**Чего НЕ делает:**

- не хранит товарные позиции и расчёт цены — это [Catalog](../plans.md) и
  [Sales Documents](../plans.md) (сделка ссылается на КП/заказ по `Id`);
- не управляет задачами по сделке — это [Activities](../plans.md) (привязка полиморфная:
  `EntityType="crm.deal"`, `EntityId=DealId`);
- не управляет пользователями — `OwnerId` это логическая ссылка на пользователя `Identity`
  (без FK через границу модуля).

**Связи (по `Id`, без FK через границу модуля):** `CustomerId`, `ContactId?`, `OwnerId`.

### 1.1. Зафиксированные решения

| Вопрос | Решение |
|---|---|
| Форма модуля | **конкретный модуль** (готовые сущности, своя БД, свои миграции) |
| Идентификатор | `Guid` |
| Хранилище | PostgreSQL (EF Core) |
| Стадии воронки | данные (`PipelineStage`), а не enum — воронок несколько, стадии настраиваются |
| Статус сделки | enum `DealStatus` (Open/Won/Lost) под `IStateMachineEntity<DealStatus>` |
| Деньги | value object `Money(Amount, Currency)` как owned-type EF (2 колонки) |
| `Won`/`Lost` | терминальные; `Lost` требует `LostReason`; фиксируют `ClosedAt` |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Мультивалютность:** одна валюта на сделку + курс на момент закрытия **vs** единая валюта системы.
   → На MVP: одна валюта на сделку, без конвертации. Аналитика по валютам — follow-up.
2. **Связь со счётом/КП:** `Deal.QuoteId?` **vs** обратная ссылка из Sales Documents.
   → Решается при реализации Sales Documents; в MVP поля нет.
3. **Soft-delete сделок** и влияние на отчётность. → `IRemovedAtEntity` опционально, см. §6.
4. **Money — общий VO** в `Cheetah.Core.Domain` (сквозное решение №5 плана) **vs** локальный в `Shared`.
   → Стартуем с локального в `Shared`, при выделении общего — заменяем.
5. **Комбинаторы спецификаций** (`And`/`Or`/`Not`) — проверить наличие в `Cheetah.Core.Specification`;
   если нет — добавить.
6. **`PagedResult<T>` / `IQuery<T>` / `IQueryHandler<,>`** — использовать существующие из
   `Cheetah.Core.CQRS` (имена уточнить по `IDispatcher.cs`).

---

## 2. Архитектурная роль

```
   ┌──────────────────────────────────────────────┐
   │                 Deals Module                   │
   │  ┌──────────────┐      ┌────────────────────┐ │
   │  │  Pipeline    │ 1──* │  PipelineStage     │ │ ← настраиваемая воронка + стадии
   │  └──────────────┘      └────────────────────┘ │   (Open / Won / Lost)
   │  ┌──────────────┐      ┌────────────────────┐ │
   │  │  Deal        │ 1──* │  DealStageHistory  │ │ ← сделка + лента переходов
   │  │ (StateMachine│      └────────────────────┘ │   (для метрик)
   │  │  по статусу) │                              │
   │  └──────┬───────┘                              │
   └─────────┼─────────────────────────────────────┘
             │ publish (Outbox → Redis/Kafka)
             ▼
   DealCreated / StageChanged / Won / Lost / OwnerChanged
             │
             ├──▶ Workflow      (автозадачи при смене стадии)
             ├──▶ Notification  (письмо при DealWon)
             ├──▶ Activities    (задачи по сделке)
             └──▶ Analytics / Timeline / Search
```

Свой PostgreSQL, REST (+ gRPC для горячих списков и board), события через шину — всё по `CLAUDE.md`.

---

## 3. Структура проектов

```
src/Modules/Deals/
├── Cheetah.Modules.Deals.DomainEvents/    # DealCreated/StageChanged/Won/Lost/OwnerChanged
├── Cheetah.Modules.Deals.Shared/          # enums (DealStatus, StageType, TriggerSource), Money
├── Cheetah.Modules.Deals.Contracts/       # DTO, Create/Update/ChangeStage requests, ViewModels
├── Cheetah.Modules.Deals.Domain/          # Deal, Pipeline, спецификации, StateMachine-маркеры
├── Cheetah.Modules.Deals.Infrastructure/  # EF Core, миграции, репозитории, DbContext
├── Cheetah.Modules.Deals.Application/      # CQRS (commands/queries/handlers), StateMachine-конфиг
├── Cheetah.Modules.Deals.Api/             # Minimal API (+ gRPC для board/списков)
├── Cheetah.Modules.Deals.Client/          # HTTP-клиент для server-to-server (Leads → Deal)
└── Tests/
    ├── Cheetah.Modules.Deals.Domain.Tests/
    ├── Cheetah.Modules.Deals.Application.Tests/
    └── Cheetah.Modules.Deals.Client.Tests/
```

**Порядок зависимостей (строго):**

```
DomainEvents (нет зависимостей, кроме EventBase из Core.Events)
   ↓
Shared (только Core)
   ↓
Contracts (Core + Shared)
   ↓
Domain (DomainEvents) → Application (Domain) → Api (Application + Contracts + CrmMapsterModule)
   ↓
Infrastructure (Domain + CrmEntityFrameworkModule + CrmEntityFrameworkPostgreSqlModule)

Client (Contracts)
```

> Напоминание из `CLAUDE.md`: **Application зависит только на Domain**, не на Infrastructure;
> интерфейсы репозиториев живут в Domain (или используется общий `IRepository<TEntity>` из
> `Cheetah.Core.DataAccess`). Фильтрация — только через спецификации, не raw LINQ в хендлерах.

---

## 4. Доменная модель

### 4.1. Сводка

| Агрегат / сущность | Базовый | Назначение | Ключевые поля |
|---|---|---|---|
| **Pipeline** | `AggregateRoot<Guid>` | воронка | `Name`, `IsDefault`, `IsActive`, `Stages[]` |
| **PipelineStage** | `Entity<Guid>` (child) | стадия | `PipelineId`, `Name`, `Order`, `Probability` (0–100), `Type` (Open/Won/Lost) |
| **Deal** | `AggregateRoot<Guid>` + `IStateMachineEntity<DealStatus>` | сделка | `Title`, `PipelineId`, `StageId`, `Money Value`, `CustomerId`, `ContactId?`, `OwnerId`, `ExpectedCloseDate?`, `Status`, `LostReason?`, `ClosedAt?` |
| **DealStageHistory** | `Entity<Guid>` (child) | история переходов | `DealId`, `FromStageId`, `ToStageId`, `ChangedBy`, `CreatedAt` |

**Решения по границам агрегатов:**

- `PipelineStage` — дитя `Pipeline` (стадии не меняются без воронки, всегда в одной транзакции).
- `DealStageHistory` — дитя `Deal` (история переходов пишется вместе со сменой стадии).
- Внутристадийные перемещения валидируются **доменно** (`PipelineStage.Order/Type`), т.к. стадии —
  данные. StateMachine валидирует только переходы по `DealStatus` (терминализацию Open→Won|Lost
  и reopen Won|Lost→Open).

### 4.2. Shared — enums и Money

```csharp
namespace Cheetah.Modules.Deals.Shared;

public enum DealStatus { Open = 0, Won = 1, Lost = 2 }
public enum StageType  { Open = 0, Won = 1, Lost = 2 }
public enum TriggerSource { Manual = 0, Automation = 1, Import = 2 }

// Локальная версия Money, пока не выделен общий VO в Cheetah.Core.Domain (см. §1.2.4).
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; } // ISO-4217, 3 символа

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be ISO-4217.", nameof(currency));
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0m, currency);
}
```

### 4.3. Domain — агрегаты (ключевая логика)

Полные образцы — в [`docs/plans.md` §D.3](../plans.md). Главные доменные инварианты:

- `Pipeline.FirstStage()` — первая `Open`-стадия по `Order`; при создании сделки `StageId` = она.
- `Deal.Create(...)` — фабрика, ставит `Status=Open`, публикует `DealCreatedIntegrationEvent`.
- `Deal.MoveToStage(target, changedBy)`:
  - запрет смены стадии у закрытой сделки;
  - запрет стадии из другой воронки;
  - пишет `DealStageHistory`;
  - если `target.Type == Won` → `WinInternal`; если `Lost` → требует `Lose(reason)`; иначе
    публикует `DealStageChangedIntegrationEvent`.
- `Deal.Win` / `WinInternal` — инвариант: нельзя выиграть без `Value.Amount > 0`; ставит `ClosedAt`,
  публикует `DealWonIntegrationEvent`.
- `Deal.Lose(reason, changedBy)` — `reason` обязателен; ставит `ClosedAt`, публикует `DealLostIntegrationEvent`.
- `Deal.Reopen(stageId)` — снимает терминальный статус (Won|Lost → Open), чистит `ClosedAt`/`LostReason`.
- `Deal.AssignOwner(newOwnerId)` — публикует `DealOwnerChangedIntegrationEvent`.

### 4.4. StateMachine — конфигурация (в Application-модуле)

```csharp
services.AddStateMachine<DealStatus>(sm => sm
    .From(DealStatus.Open).To(DealStatus.Won, DealStatus.Lost)
    .From(DealStatus.Won).To(DealStatus.Open)    // reopen
    .From(DealStatus.Lost).To(DealStatus.Open));
```

> Имена API StateMachine свериться с `src/Cheetah.Core.StateMachine` при реализации
> (`AddStateMachine<TState>`, `IStateMachineValidator<TState>.Validate`, `IStateMachineEntity<TState>`,
> `InvalidStateTransitionException`).

### 4.5. Domain — спецификации (фильтрация только через них)

```csharp
OpenDealsByOwnerSpecification(Guid ownerId)            // OwnerId == x && Status == Open
DealsByPipelineStageSpecification(Guid pipelineId, Guid? stageId)
DealsByCustomerSpecification(Guid customerId)
```

Если в `Cheetah.Core.Specification` нет `And`/`Or`/`Not` комбинаторов — добавить как часть модуля
(нужны для `ListDealsQuery` с комбинацией фильтров).

---

## 5. Application — CQRS

### 5.1. Команды

```csharp
CreateDealCommand(string Title, Guid PipelineId, decimal Amount, string Currency,
    Guid CustomerId, Guid OwnerId, Guid? ContactId, DateTime? ExpectedCloseDate) : ICommand<Guid>;
ChangeDealStageCommand(Guid DealId, Guid ToStageId, Guid ChangedBy) : ICommand;
WinDealCommand(Guid DealId, Guid ChangedBy) : ICommand;
LoseDealCommand(Guid DealId, string Reason, Guid ChangedBy) : ICommand;
AssignDealOwnerCommand(Guid DealId, Guid NewOwnerId) : ICommand;
```

### 5.2. Запросы

```csharp
GetDealByIdQuery(Guid DealId) : IQuery<DealDto?>;
ListDealsQuery(Guid? OwnerId, Guid? PipelineId, Guid? StageId, DealStatus? Status, int Page, int Size)
    : IQuery<PagedResult<DealListItemDto>>;
GetPipelineBoardQuery(Guid PipelineId) : IQuery<BoardDto>;   // Kanban — агрегаты по стадиям
GetDealHistoryQuery(Guid DealId) : IQuery<IReadOnlyList<DealHistoryDto>>;
```

### 5.3. Канон хендлеров (из `CLAUDE.md`)

- Команда: получить агрегат через репозиторий → доменный метод → `SaveChangesAsync` →
  **затем** опубликовать `DomainEvents` через `IEventBus` → `ClearDomainEvents`.
- `CreateDealCommandHandler` грузит `Pipeline` (для `FirstStage`), вызывает `Deal.Create`.
- `ChangeDealStageCommandHandler` грузит `Deal` + `Pipeline`, берёт `target` стадию из `Pipeline.Stages`,
  вызывает `deal.MoveToStage(target, changedBy)`.
- `GetPipelineBoardQueryHandler` — **анти-N+1**: одна агрегатная выборка `AsNoTrackingQueryable()`
  + `GroupBy(StageId)` + `Select(Count, Sum)` без загрузки сущностей.

Полные образцы — [`docs/plans.md` §D.6](../plans.md).

---

## 6. Infrastructure — EF Core

- Схема БД: `deals`. Таблицы: `Deals`, `Pipelines`, `PipelineStages`, `DealStageHistory`.
- `Money` → owned-type (`b.OwnsOne(d => d.Value, ...)`), колонки `Amount numeric(18,2)` + `Currency (3)`.
- Все enum → `HasConversion<int>()`.
- `builder.Ignore(d => d.DomainEvents)` во **всех** конфигурациях агрегатов (CRITICAL).
- Каскад: `Pipeline → Stages`, `Deal → History` (`OnDelete(Cascade)`).
- `DealsDbContext` : `ApplyConfigurationsFromAssembly`.
- `Infrastructure`-модуль зависит на `CrmEntityFrameworkModule` + `CrmEntityFrameworkPostgreSqlModule`,
  регистрирует `AddDbContext<DealsDbContext>(UseNpgsql(...))`, репозитории через `[Export(Scoped)]`.

### 6.1. Индексы (горячие пути, цель 10k RPS)

```csharp
b.HasIndex(d => new { d.OwnerId, d.Status });      // «мои открытые сделки»
b.HasIndex(d => new { d.PipelineId, d.StageId });  // board / списки по стадии
b.HasIndex(d => d.CustomerId);                      // сделки клиента
b.HasIndex(d => d.ExpectedCloseDate);              // прогноз/закрытия по дате
b.HasIndex(s => new { s.PipelineId, s.Order });    // стадии воронки
b.HasIndex(h => h.DealId);                          // история сделки
```

Полные образцы конфигураций — [`docs/plans.md` §D.7](../plans.md).

---

## 7. Api (Minimal API)

Эндпоинты регистрируются в `OnApplicationInitialization`, маппинг через `IObjectMapper`
(`CrmMapsterModule`), параметры через `[FromBody]`/`[FromRoute]`/`[FromQuery]`/`[FromServices]`.

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `/api/deals` | `CreateDealCommand` |
| PUT/DELETE | `/api/deals/{id}` | update / delete |
| GET | `/api/deals?stageId=&ownerId=&status=&customerId=&page=&size=` | `ListDealsQuery` |
| POST | `/api/deals/{id}/stage` `{ toStageId, comment? }` | `ChangeDealStageCommand` |
| POST | `/api/deals/{id}/win` | `WinDealCommand` |
| POST | `/api/deals/{id}/lose` `{ reason }` | `LoseDealCommand` |
| GET | `/api/deals/{id}/history` | `GetDealHistoryQuery` |
| GET | `/api/deals/board?pipelineId=` | `GetPipelineBoardQuery` |
| GET/POST/PUT | `/api/pipelines`, `.../stages` | CRUD воронок/стадий |

> gRPC дублирует `board` и `list` — горячий межсервисный путь (см. §9).

---

## 8. События (публикует Deals)

```csharp
namespace Cheetah.Modules.Deals.DomainEvents;

DealCreatedIntegrationEvent(Guid DealId, Guid CustomerId, Guid PipelineId, Guid StageId,
    decimal Amount, string Currency, Guid OwnerId) : EventBase;
DealStageChangedIntegrationEvent(Guid DealId, Guid FromStageId, Guid ToStageId, Guid ChangedBy) : EventBase;
DealWonIntegrationEvent(Guid DealId, Guid CustomerId, decimal Amount, string Currency, DateTime ClosedAt) : EventBase;
DealLostIntegrationEvent(Guid DealId, Guid CustomerId, string Reason, DateTime ClosedAt) : EventBase;
DealOwnerChangedIntegrationEvent(Guid DealId, Guid OldOwnerId, Guid NewOwnerId) : EventBase;
```

Потребители: Workflow (автозадачи при смене стадии), Notification (письмо при `DealWon`),
Activities, Timeline, Search, аналитика.

**Целостность:** подписаться на общий `EntityDeletedIntegrationEvent(EntityType, EntityId)` —
если удалён `Customer`/`Contact`, к которому привязаны сделки, применить политику (запрет удаления /
снятие ссылки / архивирование). Контракт события — общий (сквозное решение №2 плана).

---

## 9. Производительность (цель 10k RPS)

- Board и списки — проекции (`Select` → DTO) с `AsNoTracking`; суммы по стадиям — агрегатным запросом
  (`GroupBy` + `Sum`), не загрузкой сущностей.
- Индексы под все фильтры (§6.1).
- gRPC для `board`/списков (межсервисные горячие пути).
- Курсорная/страничная пагинация в `ListDealsQuery`.
- `ValueTask<T>` в хендлерах, `CancellationToken` всюду.

---

## 10. Тесты

| Проект | Покрытие |
|---|---|
| `Domain.Tests` | инварианты `Deal` (нельзя Won без суммы, нельзя сменить стадию у закрытой, Lose требует reason, Reopen чистит поля), `Pipeline.FirstStage`, `MoveToStage` (чужая воронка, запись history), `Money` (валидация валюты/суммы) |
| `Application.Tests` | хендлеры команд/запросов с моками репозиториев и `IEventBus`: публикация событий **после** `SaveChanges`, board-агрегаты, StateMachine-валидатор переходов |
| `Client.Tests` | сериализация запросов/ответов HTTP-клиента, обработка ошибок (404/409) |

---

## 11. План реализации (пошагово)

> Каждый шаг = отдельный коммит. После каждого слоя — `dotnet build` + добавление проектов в
> `Cheetah.slnx` (`dotnet sln add ...`) в папку `/Modules/Deals/`.

**Фаза 0 — каркас**
1. Создать 8 проектов + 3 тестовых по структуре §3, выставить ссылки строго по порядку зависимостей.
2. Добавить все проекты в `Cheetah.slnx` в `/Modules/Deals/`.
3. Проверить `Directory.Packages.props` — нужные пакеты (EF, Npgsql, Mapster) уже централизованы;
   `PackageReference` без `Version`.

**Фаза 1 — контракты**
4. `DomainEvents`: 5 интеграционных событий (§8).
5. `Shared`: enums + `Money` (§4.2).
6. `Contracts`: `CreateDealRequest`, `ChangeStageRequest`, `DealDto`, `DealListItemDto`, `BoardDto`,
   `BoardColumnDto`, `DealHistoryDto`, `PagedResult<T>` (если своего нет — взять из Core).

**Фаза 2 — домен**
7. `Domain`: `Pipeline`, `PipelineStage`, `Deal`, `DealStageHistory` + доменные методы/инварианты (§4.3).
8. `Domain`: спецификации (§4.5), при необходимости комбинаторы.
9. `Domain.Tests`: покрыть инварианты — **до** Infrastructure (домен тестируется без БД).

**Фаза 3 — инфраструктура**
10. `Infrastructure`: EF-конфигурации (owned `Money`, индексы, `Ignore(DomainEvents)`), `DealsDbContext`.
11. `Infrastructure`: реализации репозиториев (`[Export(Scoped)]`), `AddDbContext` в модуле.
12. Первая EF-миграция (`InitialDeals`), схема `deals`.

**Фаза 4 — приложение**
13. `Application`: команды/запросы (§5.1–5.2) + хендлеры (канон §5.3).
14. `Application`: конфигурация `AddStateMachine<DealStatus>` (§4.4).
15. `Application.Tests`: хендлеры (события после SaveChanges, board-агрегаты, переходы статусов).

**Фаза 5 — API + клиент**
16. `Api`: Minimal API эндпоинты (§7), маппинг через `IObjectMapper`, модуль-класс `partial`.
17. `Client`: `IDealsClient` + реализация (нужен для Leads-конвертации Lead→Deal), `Client.Tests`.
18. (Опц.) gRPC для `board`/`list` через реестр транспортов.

**Фаза 6 — интеграция и сидинг**
19. Подписка на `EntityDeletedIntegrationEvent` (политика по `Customer`/`Contact`).
20. Сид воронки по умолчанию (`IsDefault`) при инициализации модуля (стадии Open→…→Won/Lost).
21. Прогон end-to-end: создать сделку → сменить стадию → win/lose → проверить события и историю.

**Фаза 7 — финал**
22. README модуля (`src/Modules/Deals/README.md`) — если это базовый модуль; для бизнес-модуля
    README не обязателен (см. чек-лист `CLAUDE.md`), но этот план-док обновить по факту реализации.
23. Перевести статус документа на «реализовано», добавить ссылку на код.

---

## 12. Зависимости от инфраструктуры

| Инфраструктурный модуль | Использование в Deals |
|---|---|
| `Cheetah.Core.StateMachine` | переходы статуса сделки (Open↔Won/Lost) |
| `Cheetah.Core.DataAccess` (`IRepository<T>`) | репозитории + спецификации |
| `Cheetah.Core.Specification` | вся фильтрация (raw LINQ запрещён) |
| `Cheetah.Core.CQRS` | `ICommand`/`IQuery`/`IDispatcher`/`IEventBus` |
| `CrmEntityFrameworkModule` + `...PostgreSqlModule` | EF Core + Npgsql |
| `CrmMapsterModule` | маппинг Request↔Command, Entity↔ViewModel |
| `Cheetah.Core.Outbox` | надёжная публикация интеграционных событий |
| `Cheetah.Audit` | история/аудит (опционально, поверх `DealStageHistory`) |
| `Cheetah.Permissions` | авторизация эндпоинтов |

---

## 13. Отличия реализации от исходного эскиза

Сверка эскиза плана (`docs/plans.md` §D) с реальными контрактами репозитория дала ряд правок:

1. **Аудит-поля — `DateTimeOffset?`.** Интерфейсы `ICreateAtEntity`/`IUpdatedAtEntity` в
   `Cheetah.Core.Domain` объявляют `DateTimeOffset?`, а не `DateTime`. Соответственно `ClosedAt`,
   `ExpectedCloseDate` и события (`ClosedAt`) переведены на `DateTimeOffset`.
2. **Money — в `Shared`, без наследования `ValueObject`.** Самодостаточный `sealed record` (Shared
   зависит только на `Core`, а `ValueObject` живёт в `Core.Domain`). Маппится как EF owned-type.
3. **Загрузка стадий и агрегаты — через доменные репозитории.** Базовый `IRepository.GetByIdAsync`
   (FindAsync) не тянет `Pipeline.Stages` и не делает серверную агрегацию. Добавлены
   `IPipelineRepository` (`GetWithStagesAsync`/`ListWithStagesAsync`) и `IDealRepository`
   (`GetOpenBoardAsync` — `GroupBy`+`Sum` в Infrastructure, `ListAsync` — пагинация) — паттерн как у
   `ICalendarEventRepository`.
4. **Публикация событий — до `SaveChangesAsync` (транзакционный Outbox).** Модуль реализует
   `IOutboxDbContext`/`IDeadLetterDbContext`; `IEventBus.PublishAsync` кладёт сообщения в
   `OutboxMessages` той же транзакции, что и агрегат. Это согласовано с модулем Calendar (отличается
   от формулировки «публиковать после SaveChanges» в `CLAUDE.md`, но даёт атомарность).
5. **Эндпоинты — декларативные, не raw Minimal API.** Наследники `Cheetah.Backend.Endpoints`
   (`CreateCommandEndpoint`/`CommandEndpoint`/`QueryEndpoint`/`QueryOrNotFoundEndpoint`/
   `QueryCollectionEndpoint`) + генератор `Cheetah.Generators.Endpoints`; реквест→команда через
   Mapster-профиль. Регистрация в `OnApplicationInitialization` — генерируется.
6. **Список сделок — `QueryCollectionEndpoint` + page/size**, а не `PagedResult<T>` (в репозитории
   есть `GridRequest`/`GridResult`, но для MVP выбран простой постраничный список без total — апгрейд
   до грида позже).
7. **StateMachine-валидатор** (`IStateMachineValidator<DealStatus>`) вызывается в хендлерах
   `Win`/`Lose` перед доменным методом; внутристадийные перемещения валидируются доменно.
8. **Тесты на моках — Moq** (в репозитории нет NSubstitute).

**Отложено (follow-up):** подписка на `EntityDeletedIntegrationEvent`; сид воронки по умолчанию;
gRPC для board/списков; интеграция с Sales Documents (`Deal.QuoteId?`); мультивалютность.
