# Cheetah CRM — план развития бизнес-модулей

> Статус: проектное описание (черновик). Кода в репозитории ещё нет.
>
> Документ описывает **новые бизнес-модули**, которых не хватает Cheetah как продукту-CRM.
> Инфраструктурный слой (`src/Cheetah.*`) уже зрелый и переиспользуется; задача модулей —
> «вертикальная» доменная логика поверх него. Каждый модуль следует канону `CLAUDE.md`:
> Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client при
> server-to-server), своя БД PostgreSQL, общение через REST/gRPC + события через шину.

## Оглавление

| # | Модуль | Tier | Краткое назначение | Ключевая инфраструктура |
|---|---|---|---|---|
| 1 | [Deals / Pipeline](#1-deals--pipeline-сделки-и-воронка) | 🔴 1 | Сделки, стадии, воронка продаж | StateMachine, Audit, Events |
| 2 | [Activities / Tasks](#2-activities--tasks-задачи-и-активности) | 🔴 1 | Задачи, звонки, встречи по сущностям | BackgroundTasks, Notification |
| 3 | [Leads](#3-leads-лиды) | 🔴 1 | Захват и квалификация лидов | Events, Customer/Deal |
| 4 | [Catalog (Products & Price Lists)](#4-catalog-каталог-товаров-и-прайс-листы) | 🟠 2 | Товары/услуги, цены, валюты | EF, Cache |
| 5 | [Sales Documents (Quotes / Orders / Invoices)](#5-sales-documents-кп-заказы-счета) | 🟠 2 | КП, заказы, счета из позиций | FileStorage, StateMachine |
| 6 | [Notes & Timeline](#6-notes--timeline-заметки-и-хронология) | 🟠 2 | Заметки, комментарии, лента событий | Audit, Events |
| 7 | [Custom Fields](#7-custom-fields-кастомные-поля) | 🟡 3 | Доп. поля сущностей без миграций | Expressions.JsonLogic, Validation |
| 8 | [Workflow / Automation](#8-workflow--automation-бизнес-процессы) | 🟡 3 | Триггеры и автодействия | StateMachine, Events, BackgroundTasks |
| 9 | [Webhooks](#9-webhooks-исходящие-интеграции) | 🟡 3 | Доставка событий наружу | Outbox, RateLimit |
| 10 | [Search](#10-search-сквозной-поиск) | 🟡 3 | Полнотекстовый поиск по сущностям | Events, проекции |
| 11 | [Feature Management](#11-feature-management-управление-фич-флагами) ✅ *(MVP реализован)* | 🟡 3 | Фич-флаги, таргетинг, постепенный rollout | Expressions.JsonLogic, Cache, Tenants |
| 12 | [Scheduling / Booking](#12-scheduling--booking-calendly-внутри-crm) | 🟠 2 | Публичные страницы записи, слоты, бронирование (Calendly) | Calendar, Notification, DistributedLock, RateLimit |

---

## 1. Deals / Pipeline (сделки и воронка)

> **Tier 1 — сердце CRM.** Без сущности «сделка» и воронки продукт остаётся адресной книгой.

### 1.1. Назначение и границы

**Что делает:** ведёт сделки (opportunities) по стадиям воронки. Сделка — это потенциальная
продажа с суммой, ответственным, ожидаемой датой закрытия и текущей стадией. Воронка (pipeline) —
упорядоченный набор стадий со своими вероятностями.

**Чего НЕ делает:**

- не хранит товарные позиции и расчёт цены — это [Catalog](#4-catalog-каталог-товаров-и-прайс-листы)
  и [Sales Documents](#5-sales-documents-кп-заказы-счета) (сделка ссылается на КП/заказ по Id);
- не управляет задачами по сделке — это [Activities](#2-activities--tasks-задачи-и-активности)
  (привязка полиморфная: `EntityType="crm.deal"`).

**Связи (по Id, без FK через границу модуля):** `CustomerId`, `ContactId?`, `OwnerId` (пользователь
Identity).

### 1.2. Доменная модель

| Агрегат / сущность | Назначение | Ключевые поля |
|---|---|---|
| **Pipeline** (AggregateRoot) | воронка | `Id`, `Name`, `IsDefault`, `IsActive` |
| **PipelineStage** (Entity, child of Pipeline) | стадия | `Id`, `Name`, `Order`, `Probability` (0–100), `Type` (Open/Won/Lost) |
| **Deal** (AggregateRoot) | сделка | `Id`, `Title`, `PipelineId`, `StageId`, `Amount` + `Currency`, `CustomerId`, `ContactId?`, `OwnerId`, `ExpectedCloseDate?`, `Status` (Open/Won/Lost), `LostReason?`, audit-поля |
| **DealStageHistory** (Entity) | история переходов | `DealId`, `FromStageId?`, `ToStageId`, `ChangedBy`, `ChangedAt`, `DurationInPrevStage` |

**Ключевые решения:**

- **Стадии на `Cheetah.Core.StateMachine`.** Переходы между стадиями — конечный автомат: разрешённые
  переходы, guard-условия (нельзя в `Won` без `Amount`), побочные действия (публикация события). Это
  главная причина, по которой модуль идёт первым — он валидирует инфраструктурный StateMachine на
  реальном домене.
- **`Won`/`Lost` — терминальные стадии.** Закрытие фиксирует `ClosedAt`, для `Lost` обязателен
  `LostReason`. Метрики (win rate, средний цикл) считаются из `DealStageHistory`.
- **Деньги — value object `Money(Amount, Currency)`** (в `Cheetah.Core.Domain` или в `Shared` модуля).
  Мультивалютность — открытый вопрос §1.7.
- **Воронок может быть несколько** (продажи, партнёрка). У сделки ровно одна активная воронка; смена
  воронки сбрасывает стадию на первую.

### 1.3. Структура проектов

```
src/Modules/Deals/
├── Cheetah.Modules.Deals.DomainEvents/   # DealCreated/StageChanged/Won/Lost
├── Cheetah.Modules.Deals.Shared/          # enums (DealStatus, StageType), Money
├── Cheetah.Modules.Deals.Contracts/       # DTO, Create/Update/ChangeStage requests
├── Cheetah.Modules.Deals.Domain/          # Deal, Pipeline, спецификации, StateMachine-конфиг
├── Cheetah.Modules.Deals.Infrastructure/  # EF Core, миграции, репозитории
├── Cheetah.Modules.Deals.Application/      # CQRS
├── Cheetah.Modules.Deals.Api/             # Minimal API (+ gRPC для горячих списков)
├── Cheetah.Modules.Deals.Client/          # HTTP-клиент для server-to-server
└── Tests/ Domain.Tests, Application.Tests, Client.Tests
```

### 1.4. API (REST)

- `POST/PUT/DELETE /api/deals`, `GET /api/deals?stageId=&ownerId=&status=&customerId=` (пагинация)
- `POST /api/deals/{id}/stage` — сменить стадию `{ toStageId, comment? }` (через StateMachine)
- `POST /api/deals/{id}/win`, `POST /api/deals/{id}/lose` `{ reason }`
- `GET  /api/deals/{id}/history` — лента переходов
- `GET  /api/pipelines`, `POST/PUT /api/pipelines`, `.../stages` (CRUD стадий)
- `GET  /api/deals/board?pipelineId=` — данные для Kanban-доски (сгруппировано по стадиям, суммы)

### 1.5. События (публикует Deals)

```csharp
public record DealCreatedIntegrationEvent(Guid DealId, Guid CustomerId, decimal Amount, string Currency, Guid OwnerId) : EventBase;
public record DealStageChangedIntegrationEvent(Guid DealId, Guid FromStageId, Guid ToStageId) : EventBase;
public record DealWonIntegrationEvent(Guid DealId, Guid CustomerId, decimal Amount, string Currency) : EventBase;
public record DealLostIntegrationEvent(Guid DealId, string Reason) : EventBase;
```

Потребители: [Workflow](#8-workflow--automation-бизнес-процессы) (автозадачи при смене стадии),
[Notification](подписка на `DealWon`), аналитика.

### 1.6. Производительность (цель 10k RPS)

- Индексы: `(OwnerId, Status)`, `(PipelineId, StageId)`, `(CustomerId)`, `(ExpectedCloseDate)`.
- Kanban-доска и списки — проекции (`Select` → DTO) с `AsNoTracking`; суммы по стадиям — агрегатным
  запросом, не загрузкой сущностей.
- gRPC для board/списков (межсервисные горячие пути).

### 1.7. Решения, которые нужно зафиксировать

1. **Мультивалютность:** одна валюта на сделку + курс на момент закрытия vs единая валюта системы.
2. **Воронка как шаблон-модуль** (абстрактный, как Customer) vs готовый модуль с фиксированной схемой.
3. **Связь со счётом/КП:** `Deal.QuoteId?` vs обратная ссылка из Sales Documents.
4. **Soft-delete сделок** и влияние на отчётность.

---

## 2. Activities / Tasks (задачи и активности)

> **Tier 1.** Задачи цепляются ко всем сущностям — это «что нужно сделать» в CRM.
> Calendar ≠ задачник: Calendar про события с RRULE и расписание; Activities про to-do, звонки,
> дедлайны и привязку к бизнес-сущностям.

### 2.1. Назначение и границы

**Что делает:** хранит активности — задачи, звонки, встречи, письма-напоминания — привязанные к
произвольной сущности (`EntityType` + `EntityId`, как в Tags). У активности есть тип, статус,
исполнитель, срок, результат.

**Чего НЕ делает:**

- не управляет календарной раскладкой и RRULE (для повторяющихся — ссылка на Calendar-событие);
- не шлёт уведомления сам — публикует `ActivityDue`, доставку делает Notification.

### 2.2. Доменная модель

| Агрегат | Назначение | Ключевые поля |
|---|---|---|
| **Activity** (AggregateRoot) | задача/звонок/встреча | `Id`, `Type` (Task/Call/Meeting/Email), `Title`, `Description?`, `Status` (Open/InProgress/Done/Canceled), `Priority`, `AssigneeId`, `DueAt?`, `CompletedAt?`, `Result?`, привязка `EntityType`+`EntityId`, `OwnerId` |
| **ActivityReminder** (Entity) | напоминание | `ActivityId`, `OffsetBeforeDue`, `Channel`, `Sent` |

**Ключевые решения:**

- **Полиморфная привязка** `(EntityType, EntityId)` — единый паттерн с Tags. Активность можно повесить
  на deal, customer, contact, lead без знания этих модулей.
- **Дедлайны и напоминания через `Cheetah.BackgroundTasks`** + `DistributedLock.Postgres` (один
  исполнитель скана в кластере) — переиспользуем механику, отлаженную в Calendar.
- **Просрочка:** фоновая задача переводит просроченные `Open` в признак overdue и публикует событие.
- **Связь с Calendar:** активность типа Meeting может ссылаться на `CalendarEventId?` (опционально),
  чтобы встреча попадала в расписание.

### 2.3. Структура проектов

Стандартная (8 сборок + тесты), как у Deals. `Infrastructure` содержит фоновые задачи
`ScanDueActivitiesTask` / `DispatchActivityRemindersTask`.

### 2.4. API

- `POST/PUT/DELETE /api/activities`, `GET /api/activities?assigneeId=&status=&entityType=&entityId=&dueBefore=`
- `POST /api/activities/{id}/complete` `{ result? }`, `/cancel`
- `GET  /api/activities/my?status=` — «мои задачи» (по `AssigneeId` из контекста)
- `POST /api/activities/batch-get` — активности для списка сущностей (анти-N+1)

### 2.5. События

```csharp
public record ActivityCreatedIntegrationEvent(Guid ActivityId, string EntityType, Guid EntityId, Guid AssigneeId, DateTime? DueAt) : EventBase;
public record ActivityCompletedIntegrationEvent(Guid ActivityId, Guid CompletedBy) : EventBase;
public record ActivityDueIntegrationEvent(Guid ActivityId, Guid AssigneeId) : EventBase; // → Notification
public record ActivityOverdueIntegrationEvent(Guid ActivityId, Guid AssigneeId) : EventBase;
```

### 2.6. Целостность

Подписка на общий `EntityDeletedIntegrationEvent(EntityType, EntityId)` — закрывать/удалять висячие
активности удалённой сущности (тот же подход, что в Tags §8).

---

## 3. Leads (лиды)

> **Tier 1.** Воронка маркетинга/входящих обращений до квалификации в Customer + Deal.

### 3.1. Назначение и границы

**Что делает:** принимает «сырые» контакты (лиды) из форм, импорта, рекламных каналов; ведёт их
квалификацию и **конвертацию** в `Customer` (+ опционально `Deal`). У лида свой жизненный цикл,
отличный от клиента: `New → Working → Qualified → Converted | Disqualified`.

**Чего НЕ делает:** не дублирует Customer — после конвертации источник истины по клиенту переходит в
Customer; лид хранит `ConvertedCustomerId`.

### 3.2. Доменная модель

| Агрегат | Ключевые поля |
|---|---|
| **Lead** (AggregateRoot) | `Id`, `FullName`, `Company?`, `Email?`, `Phone?`, `Source` (Web/Import/Ads/Referral…), `Status`, `Score` (0–100), `OwnerId?`, `ConvertedCustomerId?`, `ConvertedDealId?`, `DisqualifyReason?` |

**Ключевые решения:**

- **Конвертация — доменная операция** `Lead.Convert()`: публикует `LeadConvertedIntegrationEvent`;
  фактическое создание Customer/Deal делает либо хендлер-оркестратор в Application, либо
  [Workflow](#8-workflow--automation-бизнес-процессы). Рекомендация — оркестратор через **Saga**
  (`Cheetah.Saga`), т.к. это распределённая операция через границы модулей (Lead → Customer → Deal),
  требующая компенсаций.
- **Скоринг** — простое правило (вес по заполненности/источнику) либо позже через
  [Custom Fields](#7-custom-fields-кастомные-поля) + `Expressions.JsonLogic`.
- **Антидубли:** проверка по Email/Phone при создании (спецификация), мягкое предупреждение.

### 3.3. Структура проектов

Стандартная. `Application` содержит `ConvertLeadCommand` + Saga-оркестрацию (если выбран этот путь).

### 3.4. API

- `POST /api/leads` (в т.ч. публичный endpoint для веб-форм — за RateLimit), `GET /api/leads?status=&source=&ownerId=`
- `POST /api/leads/{id}/qualify`, `/disqualify` `{ reason }`
- `POST /api/leads/{id}/convert` `{ createDeal: bool, dealTitle?, amount? }`

### 3.5. События

```csharp
public record LeadCreatedIntegrationEvent(Guid LeadId, string Source) : EventBase;
public record LeadConvertedIntegrationEvent(Guid LeadId, Guid CustomerId, Guid? DealId) : EventBase;
public record LeadDisqualifiedIntegrationEvent(Guid LeadId, string Reason) : EventBase;
```

### 3.6. Решения

1. **Кто создаёт Customer/Deal при конвертации:** Saga-оркестратор (рекомендация) vs прямой вызов
   Client-библиотек vs Workflow-правило.
2. **Публичный приём лидов:** отдельный анонимный endpoint + капча/RateLimit vs только через интеграции.

---

## 4. Catalog (каталог товаров и прайс-листы)

> **Tier 2 — коммерческий контур.** Что именно продаём и почём.

### 4.1. Назначение и границы

**Что делает:** справочник товаров/услуг, категории, единицы измерения, прайс-листы (цены по
валютам/сегментам), скидочные правила. Источник истины по ценам для
[Sales Documents](#5-sales-documents-кп-заказы-счета).

### 4.2. Доменная модель

| Агрегат | Ключевые поля |
|---|---|
| **Product** (AggregateRoot) | `Id`, `Sku`, `Name`, `Type` (Goods/Service), `CategoryId?`, `Unit`, `IsActive`, `Description?` |
| **ProductCategory** (AggregateRoot) | `Id`, `Name`, `ParentId?` (дерево) |
| **PriceList** (AggregateRoot) | `Id`, `Name`, `Currency`, `IsDefault`, `ValidFrom?`, `ValidTo?` |
| **PriceListItem** (Entity) | `PriceListId`, `ProductId`, `Price`, `MinQty?` |

**Ключевые решения:**

- **Цена — не на товаре, а в прайс-листе** (одна номенклатура, разные цены для сегментов/валют).
- **Категории — дерево** (`ParentId`); материализованный путь или `ltree` (PostgreSQL) для быстрых
  выборок поддерева.
- **Кэш горячих прайсов** (`Cheetah.Core.Cache`): прайс-листы меняются редко, читаются часто при
  формировании документов.

### 4.3. Структура / API / события

Стандартная структура. API: CRUD `/api/products`, `/api/categories`, `/api/price-lists`,
поиск `GET /api/products?search=&categoryId=&active=`, `GET /api/price-lists/{id}/price?productId=&qty=`.
События: `ProductCreated/Updated/Deactivated`, `PriceChanged` (потребитель — пересчёт открытых КП).

---

## 5. Sales Documents (КП, заказы, счета)

> **Tier 2.** Коммерческие документы из позиций каталога. Опирается на готовый `FileStorage`.

### 5.1. Назначение и границы

**Что делает:** формирует документы трёх связанных типов — **Quote** (КП), **Order** (заказ),
**Invoice** (счёт) — из строк-позиций (ссылки на `Product` + цена + кол-во + скидка). Считает итоги,
ведёт статусы, генерирует PDF, привязывается к сделке.

**Чего НЕ делает:** не считает бухгалтерию/проводки (это ERP); хранит коммерческие документы и их
жизненный цикл.

### 5.2. Доменная модель

| Агрегат | Ключевые поля |
|---|---|
| **SalesDocument** (AggregateRoot) | `Id`, `DocType` (Quote/Order/Invoice), `Number`, `DealId?`, `CustomerId`, `Currency`, `Status`, `Subtotal`, `DiscountTotal`, `TaxTotal`, `GrandTotal`, `ValidUntil?`, `PdfFileId?` |
| **SalesDocumentLine** (Entity) | `DocumentId`, `ProductId`, `Name` (снимок), `UnitPrice` (снимок), `Qty`, `Discount`, `TaxRate`, `LineTotal` |

**Ключевые решения:**

- **Снимок цены/наименования в строке** — документ неизменяем относительно последующих правок
  каталога (PriceChanged не меняет уже выставленный счёт).
- **Статусы через StateMachine** (`Draft → Sent → Accepted/Rejected` для КП; `Issued → Paid/Overdue`
  для счёта). Терминальные статусы запрещают редактирование позиций.
- **Нумерация** — последовательная per `DocType` per год; генерация через
  `Cheetah.DistributedLock` или БД-секвенс (без дыр — открытый вопрос).
- **PDF** генерируется в Infrastructure (шаблон + рендер), сохраняется через `Cheetah.FileStorage`
  (S3/Local), в документе хранится только `PdfFileId`.
- **Quote → Order → Invoice** — операции «создать на основе» (копия позиций со связью `SourceDocId`).

### 5.3. Структура / API / события

Стандартная структура. API: CRUD `/api/sales-documents`, `/lines`, `POST /{id}/send`,
`POST /{id}/accept`, `POST /{id}/convert` `{ toType }`, `GET /{id}/pdf`.
События: `DocumentSent`, `QuoteAccepted` (→ можно двигать Deal в Won), `InvoicePaid`, `InvoiceOverdue`.

---

## 6. Notes & Timeline (заметки и хронология)

> **Tier 2.** Заметки/комментарии к любой сущности + единая лента истории. Использует `Audit`.

### 6.1. Назначение и границы

**Что делает:** два связанных сервиса вокруг сущностей:

1. **Notes** — текстовые заметки/комментарии пользователей, привязанные к `(EntityType, EntityId)`,
   с упоминаниями (`@user`) и вложениями (через FileStorage).
2. **Timeline** — агрегированная хронология того, что происходило с сущностью (создание, смена
   стадии, добавление активности, письмо, заметка) — строится из доменных/интеграционных событий и
   из `Cheetah.Audit`.

### 6.2. Доменная модель

| Агрегат | Ключевые поля |
|---|---|
| **Note** (AggregateRoot) | `Id`, `EntityType`, `EntityId`, `AuthorId`, `Body`, `Mentions[]`, `AttachmentFileIds[]`, `PinnedAt?` |
| **TimelineEntry** (Entity, read-model) | `EntityType`, `EntityId`, `Kind`, `Title`, `Payload(json)`, `ActorId`, `OccurredAt` |

**Ключевые решения:**

- **Timeline — это проекция (read-model)**, наполняемая подписками на интеграционные события других
  модулей (`DealStageChanged`, `ActivityCompleted`, `NoteCreated`, `DocumentSent`…). Модуль держит
  реестр «известных видов событий → как отрендерить в строку ленты».
- **Mentions** публикуют событие `UserMentionedIntegrationEvent` → Notification.
- Альтернатива хранению Timeline — собирать на лету из `Cheetah.Audit`; рекомендация —
  материализованная лента (быстрое чтение на карточке сущности), §6.4.

### 6.3. API

- `POST/PUT/DELETE /api/notes`, `GET /api/notes?entityType=&entityId=`, `POST /{id}/pin`
- `GET /api/timeline?entityType=&entityId=&kinds=&before=` (курсорная пагинация)

### 6.4. Решения

1. **Timeline:** материализованная проекция (рекомендация) vs сборка из Audit на лету.
2. **Реестр видов событий** в ленте: жёсткий в модуле vs регистрируемый потребителями (как Tags).

---

## 7. Custom Fields (кастомные поля)

> **Tier 3 — расширяемость.** Для CRM критично: дать добавлять поля сущностям без миграций.

### 7.1. Назначение и границы

**Что делает:** позволяет администратору объявлять дополнительные поля для зарегистрированных типов
сущностей (`crm.deal`, `crm.customer`…) и хранит их значения. Поддерживает типы (string/number/
date/bool/enum/reference), обязательность, валидацию, видимость.

**Чего НЕ делает:** не меняет схемы чужих таблиц — значения хранятся в собственном хранилище модуля
(EAV/JSONB), сущности-потребители ссылаются по `(EntityType, EntityId)`.

### 7.2. Доменная модель

| Агрегат | Ключевые поля |
|---|---|
| **CustomFieldDefinition** (AggregateRoot) | `Id`, `EntityType`, `Key`, `Label`, `DataType`, `Required`, `Options[]?`, `ValidationRule(json)`, `Order`, `IsActive` |
| **CustomFieldValue** (Entity) | `EntityType`, `EntityId`, `FieldId`, `Value(jsonb)` |

**Ключевые решения:**

- **Хранилище — JSONB** в PostgreSQL (`jsonb` колонка `values` на `(EntityType, EntityId)`) вместо
  классического EAV: меньше джойнов, индексация через GIN, фильтрация по значениям.
- **Валидация через `Cheetah.Expressions.JsonLogic`** — правило валидации и видимости поля задаётся
  JsonLogic-выражением, исполняется на сервере (`Cheetah.Validation`).
- **Регистрация применимых типов** — тот же паттерн, что в Tags: потребители декларируют свои
  `EntityType` и (опц.) предопределённые поля при старте через Client.
- **Чтение** — `GET /api/custom-fields/values?entityType=&entityId=` отдаёт словарь; потребитель
  мёржит с основной DTO на уровне API-композиции (или BFF).

### 7.3. API

- `POST/PUT/DELETE /api/custom-fields/definitions`, `GET .../definitions?entityType=`
- `PUT /api/custom-fields/values` `{ entityType, entityId, values{} }` (upsert), `GET .../values`
- `POST /api/custom-fields/values/batch-get` (анти-N+1 для списков)

---

## 8. Workflow / Automation (бизнес-процессы)

> **Tier 3.** «Если случилось X — сделать Y». Связывает StateMachine + Events + BackgroundTasks.

### 8.1. Назначение и границы

**Что делает:** no-code правила автоматизации: триггер (доменное событие или расписание) → условие
(JsonLogic) → действия (создать задачу, сменить стадию, отправить уведомление, вызвать webhook).
Это «клей» между модулями без хардкода зависимостей.

**Чего НЕ делает:** не заменяет код доменных инвариантов; выполняет декларативные побочные действия.

### 8.2. Доменная модель

| Агрегат | Ключевые поля |
|---|---|
| **AutomationRule** (AggregateRoot) | `Id`, `Name`, `TriggerType` (Event/Schedule), `TriggerKey` (имя события или cron), `Condition(jsonlogic)`, `Actions[]` (тип + параметры), `IsActive` |
| **AutomationRun** (Entity) | `RuleId`, `TriggeredAt`, `Status`, `Error?`, `Context(json)` — журнал срабатываний |

**Ключевые решения:**

- **Триггеры-события:** модуль подписывается на широкий набор интеграционных событий шины; имя события
  = `TriggerKey`. Условие фильтрует по payload через JsonLogic.
- **Действия как плагины** (`IAutomationAction`): `CreateActivity`, `ChangeDealStage`, `SendNotification`,
  `CallWebhook`, `AssignOwner`. Каждое действие дергает соответствующий Client-модуль.
- **Идемпотентность** через `Cheetah.Core.Inbox` (одно событие — одно срабатывание правила).
- **Расписание** через `Cheetah.BackgroundTasks` + `DistributedLock`.
- **Длинные/многошаговые сценарии** с компенсацией — через `Cheetah.Saga` (опционально, для сложных
  правил).

### 8.3. API

- CRUD `/api/automation/rules`, `POST /{id}/enable|disable`, `POST /{id}/test` `{ sampleEvent }`
- `GET /api/automation/runs?ruleId=&status=` — аудит срабатываний

---

## 9. Webhooks (исходящие интеграции)

> **Tier 3.** Доставка событий платформы во внешние системы. Outbox уже готов.

### 9.1. Назначение и границы

**Что делает:** позволяет внешним системам подписаться (URL + секрет + фильтр событий) и надёжно
доставляет им HTTP-вебхуки по интеграционным событиям, с ретраями, подписью и журналом доставок.

### 9.2. Доменная модель

| Агрегат | Ключевые поля |
|---|---|
| **WebhookSubscription** (AggregateRoot) | `Id`, `Url`, `Secret`, `EventTypes[]`, `IsActive`, `Headers?` |
| **WebhookDelivery** (Entity) | `SubscriptionId`, `EventType`, `Payload`, `Status`, `Attempts`, `LastAttemptAt`, `ResponseCode?` |

**Ключевые решения:**

- **Надёжность через `Cheetah.Core.Outbox`** — событие → запись доставки в outbox → фоновый
  диспетчер шлёт HTTP с экспоненциальными ретраями и backoff; `DistributedLock` для единичного
  исполнителя.
- **Подпись** payload HMAC-SHA256 секретом (`Cheetah.Core.Security`), заголовок `X-Cheetah-Signature`.
- **Защита потребителя:** `Cheetah.RateLimit` на отправку, circuit breaker на «мёртвые» endpoints
  (после N фейлов — авто-disable + уведомление владельцу).
- **Dead-letter** недоставленных после max-retry; ручной replay через API.

### 9.3. API

- CRUD `/api/webhooks/subscriptions`, `GET /{id}/deliveries`, `POST /deliveries/{id}/replay`

---

## 10. Search (сквозной поиск)

> **Tier 3.** Единый быстрый поиск по сущностям всех модулей.

### 10.1. Назначение и границы

**Что делает:** глобальный поиск (global search bar) по клиентам, сделкам, контактам, лидам,
документам — единый индекс с релевантностью и фасетами по типу.

**Чего НЕ делает:** не источник истины — индекс, наполняемый из событий; всегда eventually consistent.

### 10.2. Архитектура

- **Индекс наполняется проекциями по событиям** (`*CreatedIntegrationEvent`, `*UpdatedIntegrationEvent`,
  `*DeletedIntegrationEvent`) — тот же реактивный паттерн, что Timeline/Tags.
- **Бэкенд индекса — открытый вопрос §10.4:**
  - **PostgreSQL full-text** (`tsvector` + GIN) — без новой инфраструктуры, ок до средних объёмов;
  - **OpenSearch/Elasticsearch** — для больших объёмов и сложной релевантности (требует нового
    инфраструктурного модуля `Cheetah.Search.Elastic`).
  - Рекомендация: стартовать на PostgreSQL FTS (переиспользуем имеющийся стек), абстрагировать за
    `ISearchIndex`, чтобы заменить бэкенд позже.

### 10.3. Доменная модель / API

| Агрегат (read-model) | Ключевые поля |
|---|---|
| **SearchDocument** | `EntityType`, `EntityId`, `Title`, `Subtitle`, `Body` (tsvector), `OwnerId`, `UpdatedAt`, `Url` |

- `GET /api/search?q=&types=&limit=` — единый поиск, группировка по типу, подсветка
- `POST /api/search/reindex?entityType=` — переиндексация (админ; через BackgroundTasks)

### 10.4. Решения

1. **Бэкенд:** PostgreSQL FTS (рекомендация для старта) vs OpenSearch.
2. **Права в выдаче:** фильтрация по доступу (`Cheetah.Permissions`) на этапе запроса vs пост-фильтр.

---

---

## 11. Feature Management (управление фич-флагами)

> **✅ MVP реализован.** Код: `src/Cheetah.FeatureManagement/` + `src/Modules/FeatureManagement/`;
> детальный план с отметками о выполнении — [`docs/modules/feature-management.md`](modules/feature-management.md)
> (§0–§15 + §2.1 «Микросервисный режим»). Первый из платформенных Tier 3. Уточнения относительно эскиза:
> модуль сделан **расширяемым шаблоном** (как Activities/Customer) + `.Default`; вторая ось расширяемости —
> plugin `IFeatureFilter`; микросервисный режим (`RemoteFeatureDefinitionProvider` + локальная реплика) —
> **основное решение**. Solution собирается, 31 тест зелёный.

> **Tier 3 — платформенная возможность.** Включать/выключать функциональность в рантайме без
> передеплоя, выкатывать постепенно (percentage rollout), таргетировать на тенант/пользователя/роль,
> вести A/B-варианты. Архитектурно повторяет связку `Cheetah.Permissions` + `Cheetah.Permissions.Catalog`:
> **инфраструктурная абстракция** (вычисление флагов) + **бизнес-модуль** (хранилище, таргетинг,
> админка) + **реестр** (модули декларируют свои флаги при старте).

### 11.1. Назначение и границы

**Что делает:**

- единая точка ответа на вопрос «включена ли фича X в данном контексте?»;
- декларативное объявление флагов модулями при старте (catalog), как у Permissions;
- таргетинг: вкл/выкл, процент аудитории (стабильный по hash), allow/deny-списки
  (пользователи/тенанты/роли), временные окна, условия через JsonLogic;
- варианты (A/B/n) — флаг отдаёт не только bool, но и выбранный вариант/значение;
- горячее чтение из кэша + инвалидация по событию при изменении флага.

**Чего НЕ делает:**

- не управляет правами доступа (это `Cheetah.Permissions`; фич-флаг ≠ permission: флаг про «фича
  существует/раскатана», permission про «этому субъекту можно»);
- не хранит бизнес-конфигурацию приложения (для произвольных настроек — отдельный Settings/Config,
  фич-флаги это вкл/выкл и rollout, а не «значение таймаута»).

### 11.2. Разделение на сборки (абстракция + модуль)

Как `RateLimit` (абстракция) + `RateLimit.Redis` (провайдер) и `Permissions` + `Permissions.Catalog`:

```
Инфраструктура (src/Cheetah.*):
  Cheetah.FeatureManagement            # IFeatureManager, IFeatureFilter, FeatureContext,
                                        # движок вычисления, [FeatureGate] endpoint-filter, middleware.
                                        # Зависит ТОЛЬКО на Core (+ Expressions.JsonLogic для условий).
                                        # НЕ знает, откуда берутся определения (порт IFeatureDefinitionProvider).

Бизнес-модуль (src/Modules/FeatureManagement):
  Cheetah.Modules.FeatureManagement.DomainEvents   # FeatureToggled/Created/RolloutChanged
  Cheetah.Modules.FeatureManagement.Shared          # enums (FeatureValueType, RolloutType), конвенции ключей
  Cheetah.Modules.FeatureManagement.Contracts       # DTO/Request, FeatureDefinitionDescriptor
  Cheetah.Modules.FeatureManagement.Domain          # FeatureFlag, TargetingRule, Variant, спецификации
  Cheetah.Modules.FeatureManagement.Infrastructure  # EF Core, реализация IFeatureDefinitionProvider (store+cache)
  Cheetah.Modules.FeatureManagement.Application      # CQRS: каталог, таргетинг, оценка
  Cheetah.Modules.FeatureManagement.Api             # Minimal API (админка флагов + evaluate)
  Cheetah.Modules.FeatureManagement.Client          # клиент: регистрация флагов при старте + удалённая оценка
  Tests: Domain.Tests, Application.Tests, Client.Tests
```

> **Почему так.** `IFeatureManager` нужен всем модулям (потребители), поэтому он в лёгкой
> инфраструктурной сборке без БД. А определения/таргетинг/админка — это полноценный домен со своей
> БД, поэтому отдельный бизнес-модуль реализует порт `IFeatureDefinitionProvider`.

### 11.3. Абстракция — контракты (Cheetah.FeatureManagement)

```csharp
namespace Cheetah.FeatureManagement;

// Контекст оценки: кто/где спрашивает. Заполняется из HTTP-контекста или вручную.
public sealed record FeatureContext
{
    public Guid? UserId { get; init; }
    public Guid? TenantId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, object?> Attributes { get; init; }
        = new Dictionary<string, object?>();  // произвольные атрибуты для JsonLogic-условий
}

public interface IFeatureManager
{
    ValueTask<bool> IsEnabledAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default);
    ValueTask<FeatureVariant?> GetVariantAsync(string featureKey, FeatureContext? context = null, CancellationToken ct = default);
}

public sealed record FeatureVariant(string Name, string? Value);

// Фильтр-стратегия (расширяемость движка). Встроенные: Percentage, Users, Tenants, Roles, TimeWindow, JsonLogic.
public interface IFeatureFilter
{
    string Name { get; }
    ValueTask<bool> EvaluateAsync(FeatureFilterContext context, CancellationToken ct);
}

// Порт к хранилищу определений. Реализует Infrastructure бизнес-модуля.
public interface IFeatureDefinitionProvider
{
    ValueTask<FeatureDefinition?> GetAsync(string featureKey, Guid? tenantId, CancellationToken ct);
    ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct);
}
```

Использование потребителем — три способа:

```csharp
// 1. Императивно в хендлере/сервисе
if (await _features.IsEnabledAsync("deals.kanban-v2", ctx, ct)) { /* новый путь */ }

// 2. Декларативно на эндпоинте (endpoint-filter из абстракции)
routes.MapGet("/api/deals/board", Handler).RequireFeature("deals.kanban-v2");

// 3. Вариант A/B
var variant = await _features.GetVariantAsync("pricing.experiment", ctx, ct);
```

### 11.4. Доменная модель (бизнес-модуль)

| Агрегат / сущность | Назначение | Ключевые поля |
|---|---|---|
| **FeatureFlag** (AggregateRoot) | определение флага | `Id`, `Key`, `Name`, `Description?`, `OwnerService`, `Enabled` (kill-switch), `ValueType` (Bool/Variant), `IsActive` |
| **TargetingRule** (Entity) | правило таргетинга | `FlagId`, `Order`, `FilterName` (Percentage/Users/…), `Parameters(json)`, `ResultVariant?` |
| **FeatureVariantDef** (Entity) | вариант A/B | `FlagId`, `Name`, `Value`, `Weight` (для распределения) |
| **TenantOverride** (Entity) | переопределение для тенанта | `FlagId`, `TenantId`, `Enabled`, `Rules(json)?` |

**Ключевые решения:**

- **Регистрация флагов как в Permissions.Catalog/Tags.** Каждый сервис при старте декларирует свои
  ключи флагов (`FeatureDefinitionDescriptor`) через Client → идемпотентный upsert в каталог. Так
  central store знает обо всех флагах, а ключ — стабильный контракт `"{service}.{feature}"`.
- **Percentage rollout — стабильный** по `hash(featureKey + userId/tenantId) % 100 < percent`, чтобы
  один пользователь не «мигал» между включено/выключено между запросами.
- **Условия через `Cheetah.Expressions.JsonLogic`** — фильтр `JsonLogic` исполняет правило над
  `FeatureContext.Attributes` (переиспользуем тот же движок, что в Custom Fields/Workflow).
- **Порядок правил.** `TargetingRule.Order`: первое сработавшее правило определяет результат
  (deny-список → allow-список → percentage → default). Явная стратегия разрешения конфликтов.
- **Kill-switch `FeatureFlag.Enabled`** — мгновенно выключает фичу глобально, минуя весь таргетинг.
- **Мультитенантность** — `TenantOverride`: глобальное правило + переопределение на уровне тенанта
  (`Cheetah.Core.Tenants`). При оценке сначала смотрится override тенанта, потом глобальное правило.

### 11.5. Производительность и согласованность (цель 10k RPS)

- **Чтение — из кэша** (`Cheetah.Core.Cache`): определения флагов меняются редко, читаются на каждый
  запрос. `IFeatureDefinitionProvider` отдаёт из кэша, БД — только miss/refresh.
- **Инвалидация по событию.** При изменении флага модуль публикует `FeatureFlagChangedIntegrationEvent`;
  все инстансы сбрасывают локальный кэш ключа (через шину Redis/Kafka). Eventually consistent — это
  допустимо для фич-флагов.
- **Оценка — без I/O в горячем пути.** Движок `IFeatureManager` работает над уже закэшированными
  определениями; percentage/JsonLogic — чистые вычисления в памяти.
- **In-proc для монолита, gRPC для отдельного сервиса.** Если FeatureManagement вынесен в свой сервис,
  потребители держат локальную реплику определений (push при старте + события), а не дёргают сеть на
  каждый `IsEnabledAsync`.

### 11.6. API

**Каталог (registry) — для сервисов:**

- `POST /api/features/registry/sync` — батч-upsert дескрипторов флагов (Client при старте)
- `GET  /api/features/registry?ownerService=` — список зарегистрированных флагов

**Админка флагов:**

- `GET/POST/PUT/DELETE /api/features` — CRUD определений
- `POST /api/features/{key}/enable` · `/disable` — kill-switch
- `PUT  /api/features/{key}/targeting` — правила таргетинга (`rules[]`, варианты, веса)
- `PUT  /api/features/{key}/tenants/{tenantId}` — override для тенанта

**Оценка (для потребителей без локальной реплики):**

- `POST /api/features/evaluate` — `{ keys[], context }` → `{ key: { enabled, variant? } }` (батч, анти-N+1)

> gRPC дублирует `evaluate` и `registry/sync` — горячий межсервисный путь.

### 11.7. События

```csharp
public record FeatureFlagCreatedIntegrationEvent(string Key, string OwnerService) : EventBase;
public record FeatureFlagChangedIntegrationEvent(string Key, Guid? TenantId) : EventBase; // → инвалидация кэша
public record FeatureFlagToggledIntegrationEvent(string Key, bool Enabled) : EventBase;
```

### 11.8. Регистрация флагов потребителем (паттерн как Permissions.Catalog)

```csharp
// в bootstrap сервиса-потребителя
services.AddFeatureManagement()                 // регистрирует IFeatureManager + встроенные фильтры
        .AddFeatureCatalogClient(o => o.BaseUrl = cfg["Features:Url"])
        .RegisterFeatures(reg =>
        {
            reg.Add("deals.kanban-v2", "Kanban-доска v2", x => x.ValueType = FeatureValueType.Bool);
            reg.Add("pricing.experiment", "A/B цены", x =>
            {
                x.ValueType = FeatureValueType.Variant;
                x.Variants = ["control", "treatment"];
            });
        });
```

`FeatureRegistrationSyncService` (hosted) при старте отправляет дескрипторы в `registry/sync`
(идемпотентно, с ретраями) — ровно как `TagsRegistrationSyncService`. Регистрация не валит хост при
недоступности каталога (`ContinueOnFailure`), флаги доступны со значениями по умолчанию.

### 11.9. Решения, которые нужно зафиксировать

1. **Совместимость с `Microsoft.FeatureManagement`:** свой `IFeatureManager` (рекомендация — единый
   стиль с остальным Cheetah) vs обёртка над MS.FeatureManagement.
2. **Стратегия разрешения правил:** «первое сработавшее» (рекомендация) vs «all must pass» vs приоритеты.
3. **Локальная реплика определений у потребителя** обязательна (рекомендация для 10k RPS) vs удалённый
   `evaluate` на каждый запрос.
4. **Scope флагов:** глобальные + tenant-override (рекомендация) vs обязательно per-tenant.
5. **Аудит изменений флагов** — через `Cheetah.Audit` (кто/когда включил фичу) — да/нет.

---

## 12. Scheduling / Booking (Calendly внутри CRM)

> **Tier 2.** Публичные страницы записи: внешний человек (часто лид/клиент) сам выбирает свободный
> слот у сотрудника и бронирует встречу. CRM-нативный аналог Calendly. Надстройка над модулем
> **Calendar** (источник занятости и итоговых событий) + **Notification** (подтверждения/напоминания)
> + опционально **Leads/Activities** (создать лид/встречу из брони).

### 12.1. Назначение и границы

**Что делает:**

- сотрудник (**host**) заводит **тип встречи** (booking page): «Вводный звонок 30 мин» — длительность,
  тип (видео/телефон/офис), буферы до/после, минимальный запас по времени, горизонт планирования,
  intake-вопросы;
- **расписание доступности** host'а (рабочие часы по дням недели + исключения/выходные, таймзона);
- публичная ссылка → внешний invitee видит **свободные слоты** (доступность минус занятость из
  Calendar) в своей таймзоне и бронирует;
- при подтверждении создаётся `CalendarEvent` (через `Calendar.Client`), invitee и host получают
  подтверждение, ставятся напоминания; опционально создаётся `Lead`/`Activity`;
- self-service перенос/отмена по токену из письма.

**Чего НЕ делает:**

- не хранит занятость сам — берёт free/busy из Calendar (источник истины по событиям);
- не доставляет письма/SMS — публикует события, доставка в Notification;
- не управляет RRULE-сериями — итоговая встреча создаётся как разовое событие Calendar.

### 12.2. Доменная модель

| Агрегат / сущность | Назначение | Ключевые поля |
|---|---|---|
| **BookingType** (AggregateRoot) | тип встречи / публичная страница | `Id`, `HostUserId`, `Slug` (уникальный URL), `Name`, `DurationMinutes`, `LocationKind` (Video/Phone/InPerson), `LocationDetails?`, `BufferBefore`, `BufferAfter`, `MinNotice`, `MaxAdvance` (горизонт), `Color`, `IsActive` |
| **AvailabilitySchedule** (AggregateRoot) | недельная доступность host'а | `Id`, `HostUserId`, `TimeZoneId`, `WeeklyRules[]`, `DateOverrides[]` |
| **WeeklyAvailabilityRule** (Entity) | окно в дне недели | `DayOfWeek`, `StartTime`, `EndTime` |
| **AvailabilityDateOverride** (Entity) | исключение на дату | `Date`, `IsUnavailable`, `Windows[]?` |
| **Booking** (AggregateRoot) | сама бронь | `Id`, `BookingTypeId`, `HostUserId`, `InviteeName`, `InviteeEmail`, `InviteePhone?`, `InviteeTimeZone`, `StartUtc`, `EndUtc`, `Status` (Confirmed/Cancelled/Rescheduled/NoShow/Completed), `Answers[]`, `CalendarEventId?`, `CreatedLeadId?`, `ManageToken`, `CancelReason?` |
| **BookingAnswer** (Entity) | ответ на intake-вопрос | `Question`, `Value` |

**Ключевые решения:**

- **Слот-движок — ядро модуля** (§12.3). Чистая функция: доступность ∩ горизонт ∩ min-notice −
  занятость − буферы → список слотов. Тестируется юнит-тестами без БД.
- **Занятость — из Calendar.** Booking дергает `Calendar.Client` (`GetUserBusyAsync(hostUserId,
  fromUtc, toUtc)` — это follow-up `GetUserBusyQuery`, отмеченный в плане Calendar) и вычитает
  занятые интервалы. Так нет двойного источника истины.
- **Защита от двойной брони.** На подтверждении — `DistributedLock` по ключу
  `booking:{hostUserId}:{slotStartUtc}` + повторная проверка занятости внутри лока + уникальный
  индекс `(HostUserId, StartUtc)` среди активных броней. Гонка двух invitee на один слот исключается.
- **Invitee без аккаунта Identity.** Контакты invitee хранит сам Booking; в `CalendarEvent`
  организатор — host, данные invitee идут в описание/локацию (внешние участники Calendar — отдельный
  отложенный слайс). Это снимает зависимость от Identity для внешних гостей.
- **Публичные эндпоинты анонимны** и защищены `RateLimit` + (опц.) captcha — как публичный приём
  Leads. `ManageToken` (одноразовый, неугадываемый) даёт invitee перенос/отмену без логина.
- **Связь с воронкой.** На `BookingConfirmed` опционально создаётся `Lead` (source = Booking) и/или
  `Activity` типа Meeting — через подписку Workflow или прямой оркестратор.

### 12.3. Слот-движок (псевдоалгоритм)

```
ComputeSlots(bookingType, schedule, busyIntervals, rangeFrom, rangeTo, inviteeTz):
  now ← UtcNow
  earliest ← now + bookingType.MinNotice
  latest   ← now + bookingType.MaxAdvance
  window   ← [max(rangeFrom, earliest) .. min(rangeTo, latest)]

  slots ← []
  для каждого дня D в window (в TZ расписания):
     дневные_окна ← DateOverride(D) ?? WeeklyRules(D.DayOfWeek)   // [StartTime..EndTime] в TZ host'а
     для каждого окна W:
        t ← W.Start
        пока t + Duration ≤ W.End:
           slotUtc ← [t .. t+Duration] → в UTC
           expanded ← slotUtc, расширенный на BufferBefore/BufferAfter
           если slotUtc ⊂ window  и  expanded ∩ busyIntervals = ∅:
              slots.add(slotUtc)
           t ← t + SlotStep            // шаг = Duration или фикс. сетка (15 мин)
  вернуть slots, отрендеренные в inviteeTz
```

> Буферы расширяют проверяемый интервал, но не сам слот (invitee видит чистые 30 минут, а занятыми
> считаются 30 + буферы). `busyIntervals` приходят из Calendar уже в UTC.

### 12.4. Domain — фрагмент агрегата `Booking`

```csharp
public sealed class Booking : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<BookingAnswer> _answers = new();

    public Guid BookingTypeId { get; private set; }
    public Guid HostUserId { get; private set; }
    public string InviteeName { get; private set; } = null!;
    public string InviteeEmail { get; private set; } = null!;
    public string? InviteePhone { get; private set; }
    public string InviteeTimeZone { get; private set; } = "UTC";
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public BookingStatus Status { get; private set; }
    public Guid? CalendarEventId { get; private set; }
    public Guid? CreatedLeadId { get; private set; }
    public string ManageToken { get; private set; } = null!;
    public string? CancelReason { get; private set; }
    public IReadOnlyList<BookingAnswer> Answers => _answers;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Booking() { }

    public static Booking Reserve(BookingType type, DateTime startUtc,
        string inviteeName, string inviteeEmail, string inviteeTimeZone,
        string? inviteePhone, IEnumerable<BookingAnswer> answers)
    {
        var b = new Booking
        {
            Id = Guid.NewGuid(), BookingTypeId = type.Id, HostUserId = type.HostUserId,
            StartUtc = startUtc, EndUtc = startUtc.AddMinutes(type.DurationMinutes),
            InviteeName = inviteeName, InviteeEmail = inviteeEmail,
            InviteePhone = inviteePhone, InviteeTimeZone = inviteeTimeZone,
            Status = BookingStatus.Confirmed,
            ManageToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16))
        };
        b._answers.AddRange(answers);
        b.AddDomainEvent(new BookingConfirmedIntegrationEvent(
            b.Id, type.Id, type.HostUserId, b.StartUtc, b.EndUtc, inviteeName, inviteeEmail));
        return b;
    }

    // Привязка к созданному событию Calendar (после успешного Calendar.Client.Create).
    public void AttachCalendarEvent(Guid calendarEventId) => CalendarEventId = calendarEventId;
    public void AttachLead(Guid leadId) => CreatedLeadId = leadId;

    public void Reschedule(DateTime newStartUtc, int durationMinutes)
    {
        EnsureActive();
        StartUtc = newStartUtc; EndUtc = newStartUtc.AddMinutes(durationMinutes);
        Status = BookingStatus.Rescheduled;
        AddDomainEvent(new BookingRescheduledIntegrationEvent(Id, StartUtc, EndUtc));
    }

    public void Cancel(string reason, bool byInvitee)
    {
        EnsureActive();
        Status = BookingStatus.Cancelled; CancelReason = reason;
        AddDomainEvent(new BookingCancelledIntegrationEvent(Id, reason, byInvitee));
    }

    public void MarkNoShow() { if (Status == BookingStatus.Confirmed) Status = BookingStatus.NoShow; }

    private void EnsureActive()
    {
        if (Status is BookingStatus.Cancelled or BookingStatus.NoShow or BookingStatus.Completed)
            throw new InvalidOperationException($"Booking {Id} is not active ({Status}).");
    }
}
```

### 12.5. Application — оркестрация подтверждения брони

```csharp
public record CreateBookingCommand(Guid BookingTypeId, DateTime StartUtc,
    string InviteeName, string InviteeEmail, string? InviteePhone, string InviteeTimeZone,
    IReadOnlyList<BookingAnswerDto> Answers) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateBookingCommand, Guid>))]
public sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, Guid>
{
    private readonly IRepository<BookingType> _types;
    private readonly IRepository<Booking> _bookings;
    private readonly ICalendarClient _calendar;       // Calendar.Client
    private readonly IDistributedLock _lock;
    private readonly IEventBus _eventBus;

    public async ValueTask<Guid> HandleAsync(CreateBookingCommand cmd, CancellationToken ct)
    {
        var type = await _types.GetByIdAsync(cmd.BookingTypeId, ct)
            ?? throw new NotFoundException(nameof(BookingType), cmd.BookingTypeId);

        // Анти-дабл-букинг: лок на слот + повторная проверка занятости внутри лока.
        await using var handle = await _lock.AcquireAsync($"booking:{type.HostUserId}:{cmd.StartUtc:O}", ct)
            ?? throw new ConflictException("Slot is being booked by someone else.");

        var busy = await _calendar.GetUserBusyAsync(type.HostUserId, cmd.StartUtc,
            cmd.StartUtc.AddMinutes(type.DurationMinutes), ct);
        if (busy.Count > 0) throw new ConflictException("Slot is no longer available.");

        var booking = Booking.Reserve(type, cmd.StartUtc, cmd.InviteeName, cmd.InviteeEmail,
            cmd.InviteeTimeZone, cmd.InviteePhone, cmd.Answers.Select(a => a.ToEntity()));
        _bookings.Add(booking);
        await _bookings.SaveChangesAsync(ct);

        // Создаём событие в Calendar (host — организатор, invitee — в описании).
        var eventId = await _calendar.CreateEventAsync(new CreateCalendarEventRequest
        {
            Title = $"{type.Name} — {cmd.InviteeName}",
            StartUtc = booking.StartUtc, EndUtc = booking.EndUtc,
            OrganizerUserId = type.HostUserId,
            Description = $"{cmd.InviteeName} <{cmd.InviteeEmail}>",
            EntityType = "crm.booking", EntityId = booking.Id
        }, ct);
        booking.AttachCalendarEvent(eventId);
        await _bookings.SaveChangesAsync(ct);

        foreach (var e in booking.DomainEvents) await _eventBus.PublishAsync(e, ct);
        booking.ClearDomainEvents();
        return booking.Id;
    }
}
```

> Если нужны компенсации (создали бронь, но Calendar упал) — обернуть в `Cheetah.Saga`, как
> конвертацию Lead. Для MVP достаточно лока + проверки + идемпотентного создания события.

### 12.6. Слот-запрос (публичный)

```csharp
public record GetAvailableSlotsQuery(string Slug, DateOnly From, DateOnly To, string InviteeTimeZone)
    : IQuery<IReadOnlyList<SlotDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<SlotDto>>))]
public sealed class GetAvailableSlotsQueryHandler : IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<SlotDto>>
{
    private readonly IRepository<BookingType> _types;
    private readonly IRepository<AvailabilitySchedule> _schedules;
    private readonly ICalendarClient _calendar;
    private readonly ISlotEngine _engine;   // чистый движок §12.3, юнит-тестируемый

    public async ValueTask<IReadOnlyList<SlotDto>> HandleAsync(GetAvailableSlotsQuery q, CancellationToken ct)
    {
        var type = await _types.GetBySpecAsync(new BookingTypeBySlugSpecification(q.Slug), ct)
            ?? throw new NotFoundException(nameof(BookingType), q.Slug);
        var schedule = await _schedules.GetBySpecAsync(new ScheduleByHostSpecification(type.HostUserId), ct)!;

        var (fromUtc, toUtc) = (q.From.ToUtcStart(), q.To.ToUtcEnd());
        var busy = await _calendar.GetUserBusyAsync(type.HostUserId, fromUtc, toUtc, ct);

        return _engine.ComputeSlots(type, schedule!, busy, fromUtc, toUtc, q.InviteeTimeZone);
    }
}
```

### 12.7. Структура проектов / API / события

Стандартная (8 сборок + тесты; `Domain.Tests` обязательно покрывает `ISlotEngine`).

**Публичные эндпоинты** (анонимные, за RateLimit):

- `GET  /api/public/booking/{slug}` — описание страницы записи
- `GET  /api/public/booking/{slug}/slots?from=&to=&tz=` — свободные слоты
- `POST /api/public/booking/{slug}` — создать бронь `{ start, invitee*, answers[] }`
- `GET/POST /api/public/booking/manage/{token}` — перенос/отмена invitee по токену

**Приватные эндпоинты** (host, за авторизацией):

- CRUD `/api/booking-types`, `/api/availability`
- `GET /api/bookings?hostUserId=&status=&from=&to=`, `POST /api/bookings/{id}/no-show`

**События:**

```csharp
public record BookingConfirmedIntegrationEvent(Guid BookingId, Guid BookingTypeId, Guid HostUserId,
    DateTime StartUtc, DateTime EndUtc, string InviteeName, string InviteeEmail) : EventBase;
public record BookingRescheduledIntegrationEvent(Guid BookingId, DateTime StartUtc, DateTime EndUtc) : EventBase;
public record BookingCancelledIntegrationEvent(Guid BookingId, string Reason, bool ByInvitee) : EventBase;
```

Потребители: Notification (письма invitee+host, напоминания через тот же механизм, что Calendar),
Leads (создать лид из брони), Activities (встреча по сделке/контакту).

### 12.8. Решения, которые нужно зафиксировать

1. **Free/busy из Calendar:** реализовать `GetUserBusyQuery`/`ICalendarClient.GetUserBusyAsync` в
   Calendar (его follow-up) — предпосылка для Booking.
2. **Создание Lead/Activity из брони:** через Workflow-правило vs прямой оркестратор в Booking.
3. **Round-robin / коллективные встречи** (несколько host'ов на тип) — MVP single-host, расширение позже.
4. **Шаг сетки слотов** (`SlotStep`): = длительности vs фиксированная сетка (15/30 мин).
5. **Интеграция внешних календарей** (Google/Outlook free-busy) — вне MVP, через будущий коннектор.

---

## Сводка зависимостей от инфраструктуры

| Инфраструктурный модуль | Где задействован |
|---|---|
| `Cheetah.Core.StateMachine` | Deals (стадии), Sales Documents (статусы), Workflow |
| `Cheetah.BackgroundTasks` + `DistributedLock` | Activities (дедлайны), Workflow (cron), Search (reindex), Webhooks |
| `Cheetah.Core.Outbox` | Webhooks (надёжная доставка), все события модулей |
| `Cheetah.Core.Inbox` | Workflow (идемпотентность срабатываний), Activities (удаление сущностей) |
| `Cheetah.Saga` | Leads (конвертация Lead→Customer→Deal), сложные Workflow |
| `Cheetah.Audit` | Deals (история), Notes & Timeline |
| `Cheetah.FileStorage` | Sales Documents (PDF), Notes (вложения) |
| `Cheetah.Expressions.JsonLogic` + `Validation` | Custom Fields, Workflow (условия), Leads (скоринг), Feature Management (условия таргетинга) |
| `Cheetah.Core.Cache` | Catalog (прайсы), Search (горячие запросы), Feature Management (определения флагов) |
| `Cheetah.RateLimit` + `Security` | Webhooks (подпись/лимиты), Leads (публичный приём) |
| `Cheetah.Permissions` | сквозная авторизация во всех модулях, фильтрация Search |
| `Cheetah.Core.Tenants` | мультитенантные модули; Feature Management (tenant-override флагов) |
| `Cheetah.Modules.Calendar` (Client) | Scheduling/Booking (free/busy host'а + создание итогового события) |
| `Cheetah.DistributedLock` | Activities, Webhooks, Workflow; Scheduling/Booking (анти-дабл-букинг слота) |

## Рекомендуемый порядок реализации

1. **Deals / Pipeline** — задействует StateMachine на реальном домене, даёт продукту смысл.
2. **Activities / Tasks** — цепляется ко всем сущностям, переиспользует механику напоминаний Calendar.
3. **Leads** — замыкает входную воронку, обкатывает Saga-оркестрацию.
4. **Catalog** → **Sales Documents** — коммерческий контур (порядок строгий: документы зависят от каталога).
5. **Notes & Timeline** — слой вовлечённости поверх уже существующих событий.
   **Scheduling / Booking** — после Activities + готового free/busy в Calendar (зависит от обоих).
6. **Feature Management** — ✅ **MVP реализован**
   ([`docs/modules/feature-management.md`](modules/feature-management.md)). Поднят первым среди
   платформенных: фич-флаги позволяют безопасно выкатывать сами новые модули (Deals/Activities/…) через
   постепенный rollout. Далее **Custom Fields**, **Workflow**, **Webhooks**, **Search** — внедряются по
   мере появления интеграционных событий из п.1–5.

## Сквозные решения, которые стоит зафиксировать до старта

1. **Полиморфная привязка** `(EntityType, EntityId)` — единая конвенция ключей (как в Tags) для
   Activities, Notes, Custom Fields, Search. Завести общий `Cheetah.Modules.Shared`/реестр типов.
2. **Общее событие `EntityDeletedIntegrationEvent(EntityType, EntityId)`** — обязать модули
   публиковать при удалении (нужно Activities, Notes, Tags, Search для очистки).
3. **Шаблон-модуль vs готовый модуль:** какие из новых модулей делать абстрактными (как Customer),
   а какие — конечными.
4. **Мультитенантность:** какие модули тенант-скоупятся (`Cheetah.Core.Tenants`), какие глобальны.
5. **Money / валюты** — общий value object и стратегия мультивалютности для Deals/Catalog/Documents.

---
---

# Часть II — Детализация Tier 1 (код-уровень)

> Ниже — проработка трёх приоритетных модулей до уровня доменных сущностей, спецификаций,
> CQRS-сигнатур, EF-конфигураций, StateMachine-настройки и эндпоинтов. Код приведён как образец
> и сверен с реальными контрактами репозитория:
>
> - `Specification<T>` (`Cheetah.Core.Specification`) — абстрактный, переопределяется `ToExpression()`;
>   есть неявная конвертация в `Expression<Func<T,bool>>`.
> - `IRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>` (`Cheetah.Core.DataAccess`):
>   `GetByIdAsync`, `GetBySpecAsync`, `GetAllAsync(spec?)`, `ExistsAsync`, `AsQueryable`,
>   `AsNoTrackingQueryable`, `Add/Update/Delete`, `ValueTask<int> SaveChangesAsync`. Есть короткая форма
>   `IRepository<TEntity>` для `Guid`-ключа — её и используем.
> - StateMachine (`Cheetah.Core.StateMachine`): `AddStateMachine<TState>(sm => sm.From(x).To(y,z))`,
>   `IStateMachineValidator<TState>.Validate(from, to)` (бросает `InvalidStateTransitionException`),
>   маркер `IStateMachineEntity<TState>`.
> - CQRS: `ICommand<TResult>`, `ICommandHandler<TCommand,TResult>.HandleAsync`, `IQuery<TResult>`,
>   `[Export(LifetimeType.Scoped, typeof(...))]`, `IDispatcher.SendAsync`, `IEventBus.PublishAsync`.
> - Сущности: `private set`, приватный `ctor` для EF, статичная фабрика, `AddDomainEvent` /
>   `DomainEvents` / `ClearDomainEvents`, в EF-конфиге обязателен `builder.Ignore(e => e.DomainEvents)`.

## D. Deals / Pipeline — детальная проработка

### D.1. Shared — enums и value object

```csharp
namespace Cheetah.Modules.Deals.Shared;

public enum DealStatus { Open = 0, Won = 1, Lost = 2 }

public enum StageType { Open = 0, Won = 1, Lost = 2 }

public enum TriggerSource { Manual = 0, Automation = 1, Import = 2 }

// Money — кандидат в Cheetah.Core.Domain как общий VO (см. сквозное решение №5).
// Здесь — локальная версия на случай, если общий ещё не выделен.
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

### D.2. DomainEvents

```csharp
namespace Cheetah.Modules.Deals.DomainEvents;

public record DealCreatedIntegrationEvent(
    Guid DealId, Guid CustomerId, Guid PipelineId, Guid StageId,
    decimal Amount, string Currency, Guid OwnerId) : EventBase;

public record DealStageChangedIntegrationEvent(
    Guid DealId, Guid FromStageId, Guid ToStageId, Guid ChangedBy) : EventBase;

public record DealWonIntegrationEvent(
    Guid DealId, Guid CustomerId, decimal Amount, string Currency, DateTime ClosedAt) : EventBase;

public record DealLostIntegrationEvent(
    Guid DealId, Guid CustomerId, string Reason, DateTime ClosedAt) : EventBase;

public record DealOwnerChangedIntegrationEvent(Guid DealId, Guid OldOwnerId, Guid NewOwnerId) : EventBase;
```

### D.3. Domain — агрегаты

```csharp
namespace Cheetah.Modules.Deals.Domain;

// Воронка — отдельный агрегат; стадии живут внутри него (child-entities).
public sealed class Pipeline : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<PipelineStage> _stages = new();

    public string Name { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<PipelineStage> Stages => _stages;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Pipeline() { }

    public static Pipeline Create(string name, bool isDefault = false)
    {
        var p = new Pipeline { Id = Guid.NewGuid(), Name = name, IsDefault = isDefault, IsActive = true };
        return p;
    }

    public PipelineStage AddStage(string name, int order, int probability, StageType type)
    {
        var stage = PipelineStage.Create(Id, name, order, probability, type);
        _stages.Add(stage);
        return stage;
    }

    public PipelineStage FirstStage()
        => _stages.OrderBy(s => s.Order).First(s => s.Type == StageType.Open);
}

public sealed class PipelineStage : Entity<Guid>
{
    public Guid PipelineId { get; private set; }
    public string Name { get; private set; } = null!;
    public int Order { get; private set; }
    public int Probability { get; private set; } // 0..100
    public StageType Type { get; private set; }

    private PipelineStage() { }

    internal static PipelineStage Create(Guid pipelineId, string name, int order, int probability, StageType type)
    {
        if (probability is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(probability));
        return new PipelineStage
        {
            Id = Guid.NewGuid(), PipelineId = pipelineId,
            Name = name, Order = order, Probability = probability, Type = type
        };
    }
}

// Сделка — агрегат + IStateMachineEntity по статусу (Open/Won/Lost — терминальные).
public sealed class Deal : AggregateRoot<Guid>, IStateMachineEntity<DealStatus>,
    ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<DealStageHistory> _history = new();

    public string Title { get; private set; } = null!;
    public Guid PipelineId { get; private set; }
    public Guid StageId { get; private set; }
    public Money Value { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public Guid? ContactId { get; private set; }
    public Guid OwnerId { get; private set; }
    public DateTime? ExpectedCloseDate { get; private set; }
    public DealStatus Status { get; private set; }
    public string? LostReason { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public IReadOnlyList<DealStageHistory> History => _history;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // IStateMachineEntity<DealStatus>
    public DealStatus State => Status;

    private Deal() { }

    public static Deal Create(string title, Pipeline pipeline, Money value,
        Guid customerId, Guid ownerId, Guid? contactId = null, DateTime? expectedCloseDate = null)
    {
        var firstStage = pipeline.FirstStage();
        var deal = new Deal
        {
            Id = Guid.NewGuid(), Title = title,
            PipelineId = pipeline.Id, StageId = firstStage.Id,
            Value = value, CustomerId = customerId, ContactId = contactId,
            OwnerId = ownerId, ExpectedCloseDate = expectedCloseDate, Status = DealStatus.Open
        };
        deal.AddDomainEvent(new DealCreatedIntegrationEvent(
            deal.Id, customerId, pipeline.Id, firstStage.Id, value.Amount, value.Currency, ownerId));
        return deal;
    }

    // Перемещение по стадиям внутри Open. Терминализация — отдельными методами Win/Lose.
    public void MoveToStage(PipelineStage target, Guid changedBy)
    {
        if (Status != DealStatus.Open)
            throw new InvalidOperationException("Closed deal cannot change stage.");
        if (target.PipelineId != PipelineId)
            throw new InvalidOperationException("Stage belongs to another pipeline.");
        if (target.Id == StageId) return;

        var from = StageId;
        _history.Add(DealStageHistory.Create(Id, from, target.Id, changedBy));
        StageId = target.Id;

        if (target.Type == StageType.Won) WinInternal(changedBy);
        else if (target.Type == StageType.Lost) throw new InvalidOperationException("Use Lose(reason).");
        else AddDomainEvent(new DealStageChangedIntegrationEvent(Id, from, target.Id, changedBy));
    }

    public void Win(Guid changedBy) => WinInternal(changedBy);

    private void WinInternal(Guid changedBy)
    {
        // Доменный инвариант: нельзя выиграть без суммы.
        if (Value.Amount <= 0) throw new InvalidOperationException("Won deal requires positive amount.");
        Status = DealStatus.Won;
        ClosedAt = DateTime.UtcNow;
        AddDomainEvent(new DealWonIntegrationEvent(Id, CustomerId, Value.Amount, Value.Currency, ClosedAt.Value));
    }

    public void Lose(string reason, Guid changedBy)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Lost reason is required.");
        Status = DealStatus.Lost;
        LostReason = reason;
        ClosedAt = DateTime.UtcNow;
        AddDomainEvent(new DealLostIntegrationEvent(Id, CustomerId, reason, ClosedAt.Value));
    }

    public void Reopen(Guid stageId)
    {
        if (Status == DealStatus.Open) return;
        Status = DealStatus.Open; ClosedAt = null; LostReason = null; StageId = stageId;
    }

    public void AssignOwner(Guid newOwnerId)
    {
        if (newOwnerId == OwnerId) return;
        var old = OwnerId; OwnerId = newOwnerId;
        AddDomainEvent(new DealOwnerChangedIntegrationEvent(Id, old, newOwnerId));
    }

    public void ChangeValue(Money value) => Value = value;
}

public sealed class DealStageHistory : Entity<Guid>, ICreateAtEntity
{
    public Guid DealId { get; private set; }
    public Guid FromStageId { get; private set; }
    public Guid ToStageId { get; private set; }
    public Guid ChangedBy { get; private set; }
    public DateTime CreatedAt { get; set; }

    private DealStageHistory() { }

    internal static DealStageHistory Create(Guid dealId, Guid fromStageId, Guid toStageId, Guid changedBy)
        => new() { Id = Guid.NewGuid(), DealId = dealId, FromStageId = fromStageId, ToStageId = toStageId, ChangedBy = changedBy };
}
```

> **Замечание про StateMachine.** Валидатор проверяет переходы по `DealStatus`
> (`Open → Won|Lost`, `Won|Lost → Open` при reopen). Внутристадийные перемещения (`MoveToStage`)
> валидируются доменно по `PipelineStage.Order/Type`, т.к. стадии — данные, а не enum.

### D.4. StateMachine — конфигурация (в Application-модуле)

```csharp
services.AddStateMachine<DealStatus>(sm => sm
    .From(DealStatus.Open).To(DealStatus.Won, DealStatus.Lost)
    .From(DealStatus.Won).To(DealStatus.Open)   // reopen
    .From(DealStatus.Lost).To(DealStatus.Open));
```

### D.5. Domain — спецификации

```csharp
public sealed class OpenDealsByOwnerSpecification : Specification<Deal>
{
    private readonly Guid _ownerId;
    public OpenDealsByOwnerSpecification(Guid ownerId) => _ownerId = ownerId;
    public override Expression<Func<Deal, bool>> ToExpression()
        => d => d.OwnerId == _ownerId && d.Status == DealStatus.Open;
}

public sealed class DealsByPipelineStageSpecification : Specification<Deal>
{
    private readonly Guid _pipelineId; private readonly Guid? _stageId;
    public DealsByPipelineStageSpecification(Guid pipelineId, Guid? stageId = null)
        => (_pipelineId, _stageId) = (pipelineId, stageId);
    public override Expression<Func<Deal, bool>> ToExpression()
        => d => d.PipelineId == _pipelineId && (_stageId == null || d.StageId == _stageId);
}

public sealed class DealsByCustomerSpecification : Specification<Deal>
{
    private readonly Guid _customerId;
    public DealsByCustomerSpecification(Guid customerId) => _customerId = customerId;
    public override Expression<Func<Deal, bool>> ToExpression() => d => d.CustomerId == _customerId;
}

// Спецификации можно комбинировать через And/Or-хелперы Cheetah.Core.Specification
// (если их нет — добавить AndSpecification/OrSpecification как расширение).
```

### D.6. Application — CQRS-сигнатуры

```csharp
// --- Commands ---
public record CreateDealCommand(
    string Title, Guid PipelineId, decimal Amount, string Currency,
    Guid CustomerId, Guid OwnerId, Guid? ContactId, DateTime? ExpectedCloseDate) : ICommand<Guid>;

public record ChangeDealStageCommand(Guid DealId, Guid ToStageId, Guid ChangedBy) : ICommand;
public record WinDealCommand(Guid DealId, Guid ChangedBy) : ICommand;
public record LoseDealCommand(Guid DealId, string Reason, Guid ChangedBy) : ICommand;
public record AssignDealOwnerCommand(Guid DealId, Guid NewOwnerId) : ICommand;

// --- Queries ---
public record GetDealByIdQuery(Guid DealId) : IQuery<DealDto?>;
public record ListDealsQuery(Guid? OwnerId, Guid? PipelineId, Guid? StageId, DealStatus? Status,
    int Page, int Size) : IQuery<PagedResult<DealListItemDto>>;
public record GetPipelineBoardQuery(Guid PipelineId) : IQuery<BoardDto>;
public record GetDealHistoryQuery(Guid DealId) : IQuery<IReadOnlyList<DealHistoryDto>>;
```

Пример хендлера команды (создание сделки) — канон из `CLAUDE.md`:

```csharp
[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateDealCommand, Guid>))]
public sealed class CreateDealCommandHandler : ICommandHandler<CreateDealCommand, Guid>
{
    private readonly IRepository<Deal> _deals;
    private readonly IRepository<Pipeline> _pipelines;
    private readonly IEventBus _eventBus;

    public CreateDealCommandHandler(IRepository<Deal> deals, IRepository<Pipeline> pipelines, IEventBus eventBus)
        => (_deals, _pipelines, _eventBus) = (deals, pipelines, eventBus);

    public async ValueTask<Guid> HandleAsync(CreateDealCommand cmd, CancellationToken ct)
    {
        var pipeline = await _pipelines.GetByIdAsync(cmd.PipelineId, ct)
            ?? throw new NotFoundException(nameof(Pipeline), cmd.PipelineId);

        var deal = Deal.Create(cmd.Title, pipeline, new Money(cmd.Amount, cmd.Currency),
            cmd.CustomerId, cmd.OwnerId, cmd.ContactId, cmd.ExpectedCloseDate);

        _deals.Add(deal);
        await _deals.SaveChangesAsync(ct);          // события публикуем ПОСЛЕ сохранения

        foreach (var e in deal.DomainEvents) await _eventBus.PublishAsync(e, ct);
        deal.ClearDomainEvents();
        return deal.Id;
    }
}
```

Пример хендлера смены стадии (использует StateMachine только для терминализации, стадии — доменно):

```csharp
[Export(LifetimeType.Scoped, typeof(ICommandHandler<ChangeDealStageCommand>))]
public sealed class ChangeDealStageCommandHandler : ICommandHandler<ChangeDealStageCommand>
{
    private readonly IRepository<Deal> _deals;
    private readonly IRepository<Pipeline> _pipelines;
    private readonly IEventBus _eventBus;

    public ChangeDealStageCommandHandler(IRepository<Deal> deals, IRepository<Pipeline> pipelines, IEventBus eventBus)
        => (_deals, _pipelines, _eventBus) = (deals, pipelines, eventBus);

    public async ValueTask HandleAsync(ChangeDealStageCommand cmd, CancellationToken ct)
    {
        var deal = await _deals.GetByIdAsync(cmd.DealId, ct)
            ?? throw new NotFoundException(nameof(Deal), cmd.DealId);
        var pipeline = await _pipelines.GetByIdAsync(deal.PipelineId, ct)!;
        var target = pipeline!.Stages.First(s => s.Id == cmd.ToStageId);

        deal.MoveToStage(target, cmd.ChangedBy);    // доменные инварианты внутри
        await _deals.SaveChangesAsync(ct);

        foreach (var e in deal.DomainEvents) await _eventBus.PublishAsync(e, ct);
        deal.ClearDomainEvents();
    }
}
```

Board-запрос (анти-N+1, проекция + агрегаты, без загрузки сущностей):

```csharp
[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetPipelineBoardQuery, BoardDto>))]
public sealed class GetPipelineBoardQueryHandler : IQueryHandler<GetPipelineBoardQuery, BoardDto>
{
    private readonly IRepository<Deal> _deals;
    private readonly IRepository<Pipeline> _pipelines;

    public async ValueTask<BoardDto> HandleAsync(GetPipelineBoardQuery q, CancellationToken ct)
    {
        var pipeline = await _pipelines.GetByIdAsync(q.PipelineId, ct)!;

        // одна агрегатная выборка по стадиям вместо N запросов
        var columns = await _deals.AsNoTrackingQueryable()
            .Where(d => d.PipelineId == q.PipelineId && d.Status == DealStatus.Open)
            .GroupBy(d => d.StageId)
            .Select(g => new BoardColumnDto
            {
                StageId = g.Key,
                Count = g.Count(),
                Sum = g.Sum(d => d.Value.Amount)
            })
            .ToListAsync(ct);

        return new BoardDto { PipelineId = q.PipelineId, Columns = columns };
    }
}
```

### D.7. Infrastructure — EF-конфигурации

```csharp
public sealed class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> b)
    {
        b.ToTable("Deals", "deals");
        b.HasKey(d => d.Id);
        b.Property(d => d.Title).HasMaxLength(300).IsRequired();
        b.Property(d => d.Status).HasConversion<int>();

        // Money как owned-type (две колонки Amount/Currency)
        b.OwnsOne(d => d.Value, v =>
        {
            v.Property(p => p.Amount).HasColumnName("Amount").HasColumnType("numeric(18,2)");
            v.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        b.Property(d => d.LostReason).HasMaxLength(1000);

        b.HasMany(d => d.History)
            .WithOne().HasForeignKey(h => h.DealId).OnDelete(DeleteBehavior.Cascade);

        // Индексы под горячие пути (см. §1.6)
        b.HasIndex(d => new { d.OwnerId, d.Status });
        b.HasIndex(d => new { d.PipelineId, d.StageId });
        b.HasIndex(d => d.CustomerId);
        b.HasIndex(d => d.ExpectedCloseDate);

        b.Ignore(d => d.DomainEvents); // CRITICAL
    }
}

public sealed class PipelineConfiguration : IEntityTypeConfiguration<Pipeline>
{
    public void Configure(EntityTypeBuilder<Pipeline> b)
    {
        b.ToTable("Pipelines", "deals");
        b.Property(p => p.Name).HasMaxLength(200).IsRequired();
        b.HasMany(p => p.Stages).WithOne().HasForeignKey(s => s.PipelineId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(p => p.IsDefault);
        b.Ignore(p => p.DomainEvents);
    }
}

public sealed class PipelineStageConfiguration : IEntityTypeConfiguration<PipelineStage>
{
    public void Configure(EntityTypeBuilder<PipelineStage> b)
    {
        b.ToTable("PipelineStages", "deals");
        b.Property(s => s.Name).HasMaxLength(200).IsRequired();
        b.Property(s => s.Type).HasConversion<int>();
        b.HasIndex(s => new { s.PipelineId, s.Order });
    }
}

public sealed class DealStageHistoryConfiguration : IEntityTypeConfiguration<DealStageHistory>
{
    public void Configure(EntityTypeBuilder<DealStageHistory> b)
    {
        b.ToTable("DealStageHistory", "deals");
        b.HasIndex(h => h.DealId);
    }
}

public sealed class DealsDbContext : DbContext
{
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Pipeline> Pipelines => Set<Pipeline>();
    public DbSet<PipelineStage> PipelineStages => Set<PipelineStage>();

    public DealsDbContext(DbContextOptions<DealsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
        => mb.ApplyConfigurationsFromAssembly(typeof(DealsDbContext).Assembly);
}
```

### D.8. Api — эндпоинты (фрагмент)

```csharp
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var routes = context.GetRouteBuilder();
    var mapper = context.ServiceProvider.GetRequiredService<IObjectMapper>();

    routes.MapPost("/api/deals", async (
        [FromBody] CreateDealRequest req,
        [FromServices] IDispatcher dispatcher,
        CancellationToken ct) =>
    {
        var id = await dispatcher.SendAsync(mapper.Map<CreateDealCommand>(req), ct);
        return Results.Created($"/api/deals/{id}", id);
    }).WithName("CreateDeal").WithOpenApi();

    routes.MapPost("/api/deals/{id:guid}/stage", async (
        [FromRoute] Guid id, [FromBody] ChangeStageRequest req,
        [FromServices] IDispatcher dispatcher, CancellationToken ct) =>
    {
        await dispatcher.SendAsync(new ChangeDealStageCommand(id, req.ToStageId, req.ChangedBy), ct);
        return Results.NoContent();
    }).WithName("ChangeDealStage").WithOpenApi();

    routes.MapGet("/api/deals/board", async (
        [FromQuery] Guid pipelineId, [FromServices] IDispatcher dispatcher, CancellationToken ct) =>
        Results.Ok(await dispatcher.SendAsync(new GetPipelineBoardQuery(pipelineId), ct)))
        .WithName("GetDealBoard").WithOpenApi();
}
```

---

## A. Activities / Tasks — детальная проработка

### A.1. Shared

```csharp
namespace Cheetah.Modules.Activities.Shared;

public enum ActivityType { Task = 0, Call = 1, Meeting = 2, Email = 3 }
public enum ActivityStatus { Open = 0, InProgress = 1, Done = 2, Canceled = 3 }
public enum ActivityPriority { Low = 0, Normal = 1, High = 2, Urgent = 3 }

// Конвенция полиморфной привязки — общая с Tags/Notes/CustomFields (сквозное решение №1).
public static class EntityRefKeys
{
    public const string Deal = "crm.deal";
    public const string Customer = "crm.customer";
    public const string Contact = "crm.contact";
    public const string Lead = "crm.lead";
}
```

### A.2. Domain — агрегат

```csharp
public sealed class Activity : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<ActivityReminder> _reminders = new();

    public ActivityType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public ActivityStatus Status { get; private set; }
    public ActivityPriority Priority { get; private set; }
    public Guid AssigneeId { get; private set; }
    public Guid OwnerId { get; private set; }

    // полиморфная привязка к произвольной сущности
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }

    public DateTime? DueAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Result { get; private set; }
    public Guid? CalendarEventId { get; private set; }  // если Meeting вынесена в Calendar
    public IReadOnlyList<ActivityReminder> Reminders => _reminders;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Activity() { }

    public static Activity Create(ActivityType type, string title, Guid assigneeId, Guid ownerId,
        string entityType, Guid entityId, DateTime? dueAt = null,
        ActivityPriority priority = ActivityPriority.Normal, string? description = null)
    {
        var a = new Activity
        {
            Id = Guid.NewGuid(), Type = type, Title = title, Status = ActivityStatus.Open,
            Priority = priority, AssigneeId = assigneeId, OwnerId = ownerId,
            EntityType = entityType, EntityId = entityId, DueAt = dueAt, Description = description
        };
        a.AddDomainEvent(new ActivityCreatedIntegrationEvent(a.Id, entityType, entityId, assigneeId, dueAt));
        return a;
    }

    public void AddReminder(TimeSpan offsetBeforeDue, string channel)
    {
        if (DueAt is null) throw new InvalidOperationException("Reminder requires DueAt.");
        _reminders.Add(ActivityReminder.Create(Id, offsetBeforeDue, channel));
    }

    public void Start() { if (Status == ActivityStatus.Open) Status = ActivityStatus.InProgress; }

    public void Complete(Guid completedBy, string? result = null)
    {
        if (Status is ActivityStatus.Done or ActivityStatus.Canceled) return;
        Status = ActivityStatus.Done; CompletedAt = DateTime.UtcNow; Result = result;
        AddDomainEvent(new ActivityCompletedIntegrationEvent(Id, completedBy));
    }

    public void Cancel() { Status = ActivityStatus.Canceled; }

    public void Reassign(Guid newAssigneeId) { AssigneeId = newAssigneeId; }

    // Вызывается фоновой задачей скана просрочек.
    public bool TryMarkOverdue(DateTime now, out ActivityOverdueIntegrationEvent? evt)
    {
        evt = null;
        if (Status == ActivityStatus.Open && DueAt is { } due && due < now)
        {
            evt = new ActivityOverdueIntegrationEvent(Id, AssigneeId);
            AddDomainEvent(evt);
            return true;
        }
        return false;
    }
}

public sealed class ActivityReminder : Entity<Guid>
{
    public Guid ActivityId { get; private set; }
    public TimeSpan OffsetBeforeDue { get; private set; }
    public string Channel { get; private set; } = null!;
    public bool Sent { get; private set; }
    public DateTime? SentAt { get; private set; }

    private ActivityReminder() { }
    internal static ActivityReminder Create(Guid activityId, TimeSpan offset, string channel)
        => new() { Id = Guid.NewGuid(), ActivityId = activityId, OffsetBeforeDue = offset, Channel = channel };

    public void MarkSent() { Sent = true; SentAt = DateTime.UtcNow; }
}
```

### A.3. Domain — спецификации

```csharp
public sealed class ActivitiesByEntitySpecification : Specification<Activity>
{
    private readonly string _entityType; private readonly Guid _entityId;
    public ActivitiesByEntitySpecification(string entityType, Guid entityId)
        => (_entityType, _entityId) = (entityType, entityId);
    public override Expression<Func<Activity, bool>> ToExpression()
        => a => a.EntityType == _entityType && a.EntityId == _entityId;
}

public sealed class OpenActivitiesByAssigneeSpecification : Specification<Activity>
{
    private readonly Guid _assigneeId;
    public OpenActivitiesByAssigneeSpecification(Guid assigneeId) => _assigneeId = assigneeId;
    public override Expression<Func<Activity, bool>> ToExpression()
        => a => a.AssigneeId == _assigneeId &&
                (a.Status == ActivityStatus.Open || a.Status == ActivityStatus.InProgress);
}

// Для фоновых задач: «открытые с DueAt < now» и «с несработавшими напоминаниями к сроку».
public sealed class OverdueActivitiesSpecification : Specification<Activity>
{
    private readonly DateTime _now;
    public OverdueActivitiesSpecification(DateTime now) => _now = now;
    public override Expression<Func<Activity, bool>> ToExpression()
        => a => a.Status == ActivityStatus.Open && a.DueAt != null && a.DueAt < _now;
}
```

### A.4. Application — CQRS-сигнатуры

```csharp
public record CreateActivityCommand(ActivityType Type, string Title, Guid AssigneeId, Guid OwnerId,
    string EntityType, Guid EntityId, DateTime? DueAt, ActivityPriority Priority, string? Description) : ICommand<Guid>;
public record CompleteActivityCommand(Guid ActivityId, Guid CompletedBy, string? Result) : ICommand;
public record CancelActivityCommand(Guid ActivityId) : ICommand;
public record ReassignActivityCommand(Guid ActivityId, Guid NewAssigneeId) : ICommand;

public record GetActivityByIdQuery(Guid ActivityId) : IQuery<ActivityDto?>;
public record ListActivitiesQuery(Guid? AssigneeId, ActivityStatus? Status,
    string? EntityType, Guid? EntityId, DateTime? DueBefore, int Page, int Size) : IQuery<PagedResult<ActivityDto>>;
public record BatchGetActivitiesQuery(string EntityType, IReadOnlyList<Guid> EntityIds)
    : IQuery<IReadOnlyDictionary<Guid, IReadOnlyList<ActivityDto>>>; // анти-N+1 для списков
```

### A.5. Infrastructure — фоновые задачи (как в Calendar)

```csharp
// Скан просрочек: один исполнитель в кластере (DistributedLock), периодичность через BackgroundTasks.
[Export(LifetimeType.Scoped, typeof(IBackgroundTask))]
public sealed class ScanOverdueActivitiesTask : IBackgroundTask
{
    private readonly IRepository<Activity> _repo;
    private readonly IEventBus _eventBus;
    private readonly IDistributedLock _lock;

    public string Schedule => "*/5 * * * *"; // каждые 5 минут (cron)

    public async ValueTask ExecuteAsync(CancellationToken ct)
    {
        await using var handle = await _lock.AcquireAsync("activities:scan-overdue", ct);
        if (handle is null) return; // лок держит другой инстанс

        var now = DateTime.UtcNow;
        var overdue = await _repo.GetAllAsync(new OverdueActivitiesSpecification(now), ct);
        foreach (var a in overdue)
            if (a.TryMarkOverdue(now, out _)) { /* событие в a.DomainEvents */ }

        await _repo.SaveChangesAsync(ct);
        foreach (var a in overdue)
        {
            foreach (var e in a.DomainEvents) await _eventBus.PublishAsync(e, ct);
            a.ClearDomainEvents();
        }
    }
}
```

> `DispatchActivityRemindersTask` — аналогично: выбирает активности с несработавшими `ActivityReminder`,
> у которых `DueAt - OffsetBeforeDue <= now`, публикует `ActivityDueIntegrationEvent` (→ Notification),
> помечает напоминания `MarkSent()`. Точные имена `IBackgroundTask`/`IDistributedLock` свериться с
> `Cheetah.BackgroundTasks` и `Cheetah.DistributedLock.Postgres` при реализации.

### A.6. EF-конфигурация (ключевое)

```csharp
public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> b)
    {
        b.ToTable("Activities", "activities");
        b.Property(a => a.Title).HasMaxLength(300).IsRequired();
        b.Property(a => a.EntityType).HasMaxLength(64).IsRequired();
        b.Property(a => a.Type).HasConversion<int>();
        b.Property(a => a.Status).HasConversion<int>();
        b.Property(a => a.Priority).HasConversion<int>();
        b.HasMany(a => a.Reminders).WithOne().HasForeignKey(r => r.ActivityId).OnDelete(DeleteBehavior.Cascade);

        // Горячие пути: «мои открытые», «активности сущности», скан просрочек.
        b.HasIndex(a => new { a.AssigneeId, a.Status });
        b.HasIndex(a => new { a.EntityType, a.EntityId });
        b.HasIndex(a => new { a.Status, a.DueAt });
        b.Ignore(a => a.DomainEvents);
    }
}
```

### A.7. Целостность — подписка на удаление сущности

```csharp
[Export(LifetimeType.Scoped, typeof(IEventHandler<EntityDeletedIntegrationEvent>))]
public sealed class CleanupActivitiesOnEntityDeletedHandler : IEventHandler<EntityDeletedIntegrationEvent>
{
    private readonly IRepository<Activity> _repo;
    public async ValueTask HandleAsync(EntityDeletedIntegrationEvent e, CancellationToken ct)
    {
        var orphans = await _repo.GetAllAsync(new ActivitiesByEntitySpecification(e.EntityType, e.EntityId), ct);
        foreach (var a in orphans) a.Cancel();   // или Delete — по политике
        await _repo.SaveChangesAsync(ct);
    }
}
```

> Подписка регистрируется в `OnApplicationInitialization`:
> `eventBus.Subscribe<EntityDeletedIntegrationEvent, CleanupActivitiesOnEntityDeletedHandler>();`

---

## L. Leads — детальная проработка

### L.1. Shared

```csharp
namespace Cheetah.Modules.Leads.Shared;

public enum LeadStatus { New = 0, Working = 1, Qualified = 2, Converted = 3, Disqualified = 4 }
public enum LeadSource { Web = 0, Import = 1, Ads = 2, Referral = 3, Manual = 4, Api = 5 }
```

### L.2. Domain — агрегат + StateMachine

```csharp
public sealed class Lead : AggregateRoot<Guid>, IStateMachineEntity<LeadStatus>,
    ICreateAtEntity, IUpdatedAtEntity
{
    public string FullName { get; private set; } = null!;
    public string? Company { get; private set; }
    public Email? Email { get; private set; }      // VO из Cheetah.Core.Domain
    public Phone? Phone { get; private set; }      // VO из Cheetah.Core.Domain
    public LeadSource Source { get; private set; }
    public LeadStatus Status { get; private set; }
    public int Score { get; private set; }         // 0..100
    public Guid? OwnerId { get; private set; }
    public Guid? ConvertedCustomerId { get; private set; }
    public Guid? ConvertedDealId { get; private set; }
    public string? DisqualifyReason { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LeadStatus State => Status;

    private Lead() { }

    public static Lead Create(string fullName, LeadSource source,
        string? email = null, string? phone = null, string? company = null, Guid? ownerId = null)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(), FullName = fullName, Source = source, Company = company,
            Email = email is null ? null : Email.Create(email),
            Phone = phone is null ? null : Phone.Create(phone),
            Status = LeadStatus.New, OwnerId = ownerId
        };
        lead.Score = lead.CalculateScore();
        lead.AddDomainEvent(new LeadCreatedIntegrationEvent(lead.Id, source.ToString()));
        return lead;
    }

    public void StartWorking() { if (Status == LeadStatus.New) Status = LeadStatus.Working; }

    public void Qualify()
    {
        if (Status is not (LeadStatus.New or LeadStatus.Working))
            throw new InvalidOperationException("Only new/working lead can be qualified.");
        Status = LeadStatus.Qualified;
    }

    public void Disqualify(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason required.");
        Status = LeadStatus.Disqualified; DisqualifyReason = reason;
        AddDomainEvent(new LeadDisqualifiedIntegrationEvent(Id, reason));
    }

    // Конвертация: фактическое создание Customer/Deal делает Saga-оркестратор (см. L.4).
    // Здесь — только перевод состояния и фиксация ссылок после успеха саги.
    public void MarkConverted(Guid customerId, Guid? dealId)
    {
        if (Status == LeadStatus.Converted) return;
        Status = LeadStatus.Converted;
        ConvertedCustomerId = customerId; ConvertedDealId = dealId;
        AddDomainEvent(new LeadConvertedIntegrationEvent(Id, customerId, dealId));
    }

    private int CalculateScore()
    {
        // Простое правило; позже — через Custom Fields + Expressions.JsonLogic.
        var s = 0;
        if (Email is not null) s += 30;
        if (Phone is not null) s += 30;
        if (!string.IsNullOrWhiteSpace(Company)) s += 20;
        s += Source switch { LeadSource.Referral => 20, LeadSource.Web => 10, _ => 0 };
        return Math.Min(s, 100);
    }
}
```

StateMachine-конфигурация:

```csharp
services.AddStateMachine<LeadStatus>(sm => sm
    .From(LeadStatus.New).To(LeadStatus.Working, LeadStatus.Qualified, LeadStatus.Disqualified)
    .From(LeadStatus.Working).To(LeadStatus.Qualified, LeadStatus.Disqualified)
    .From(LeadStatus.Qualified).To(LeadStatus.Converted, LeadStatus.Disqualified));
```

### L.3. Спецификации (в т.ч. антидубль)

```csharp
public sealed class LeadByEmailSpecification : Specification<Lead>
{
    private readonly string _email;
    public LeadByEmailSpecification(string email) => _email = email.ToLowerInvariant();
    public override Expression<Func<Lead, bool>> ToExpression()
        => l => l.Email != null && l.Email.Value == _email;
}

public sealed class ActiveLeadsByOwnerSpecification : Specification<Lead>
{
    private readonly Guid _ownerId;
    public ActiveLeadsByOwnerSpecification(Guid ownerId) => _ownerId = ownerId;
    public override Expression<Func<Lead, bool>> ToExpression()
        => l => l.OwnerId == _ownerId &&
                l.Status != LeadStatus.Converted && l.Status != LeadStatus.Disqualified;
}
```

### L.4. Application — CQRS + Saga-конвертация

```csharp
public record CreateLeadCommand(string FullName, LeadSource Source,
    string? Email, string? Phone, string? Company, Guid? OwnerId) : ICommand<Guid>;
public record QualifyLeadCommand(Guid LeadId) : ICommand;
public record DisqualifyLeadCommand(Guid LeadId, string Reason) : ICommand;
public record ConvertLeadCommand(Guid LeadId, bool CreateDeal,
    string? DealTitle, decimal? Amount, string? Currency, Guid PipelineId) : ICommand<ConvertLeadResult>;

public record ConvertLeadResult(Guid CustomerId, Guid? DealId);
```

Конвертация пересекает границы трёх модулей (Lead → Customer → Deal) и требует компенсаций, поэтому
оркестрируется через `Cheetah.Saga`:

```
ConvertLeadSaga:
  step 1  CreateCustomer       (Customer.Client.CreateAsync)        comp: DeleteCustomer
  step 2  CreateDeal (если CreateDeal=true, Deal.Client.CreateAsync) comp: DeleteDeal
  step 3  MarkLeadConverted    (локальная команда + публикация LeadConvertedIntegrationEvent)
```

При сбое любого шага сага выполняет компенсации предыдущих. Антидубль (`LeadByEmailSpecification`)
проверяется в `CreateLeadCommandHandler` до сохранения и даёт мягкое предупреждение/слияние по
политике.

### L.5. EF-конфигурация (VO Email/Phone → строки)

```csharp
public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> b)
    {
        b.ToTable("Leads", "leads");
        b.Property(l => l.FullName).HasMaxLength(300).IsRequired();
        b.Property(l => l.Company).HasMaxLength(300);
        b.Property(l => l.Status).HasConversion<int>();
        b.Property(l => l.Source).HasConversion<int>();

        // VO → строковые колонки (как в Customer)
        b.Property(l => l.Email).HasConversion(
            v => v == null ? null : v.Value, s => s == null ? null : Email.Create(s)).HasMaxLength(320);
        b.Property(l => l.Phone).HasConversion(
            v => v == null ? null : v.Value, s => s == null ? null : Phone.Create(s)).HasMaxLength(40);

        b.HasIndex(l => l.Email);
        b.HasIndex(l => new { l.OwnerId, l.Status });
        b.Ignore(l => l.DomainEvents);
    }
}
```

### L.6. Api

- `POST /api/leads` — приём (вкл. публичный анонимный вариант за `Cheetah.RateLimit` для веб-форм)
- `POST /api/leads/{id}/qualify` · `/disqualify` `{ reason }`
- `POST /api/leads/{id}/convert` `{ createDeal, dealTitle?, amount?, currency?, pipelineId }` → `ConvertLeadResult`
- `GET  /api/leads?status=&source=&ownerId=&page=&size=`

---

## Открытые технические уточнения для всех трёх модулей

1. **Имена контрактов фоновых задач/локов** (`IBackgroundTask`, `IDistributedLock`, формат расписания) —
   свериться с `Cheetah.BackgroundTasks` и `Cheetah.DistributedLock.Postgres` (в примерах — предполагаемые).
2. **Комбинаторы спецификаций** (`And`/`Or`/`Not`) — проверить наличие в `Cheetah.Core.Specification`;
   если нет — добавить как часть этих модулей.
3. **`PagedResult<T>` / `IQuery<T>` / `IQueryHandler<,>`** — использовать существующие из `Cheetah.Core.CQRS`
   (имена уточнить по `IDispatcher.cs`).
4. **`EntityDeletedIntegrationEvent`** — где живёт контракт: общий `Cheetah.Contracts` vs per-module.
   Рекомендация — общий, т.к. его публикуют и потребляют многие модули (сквозное решение №2).
5. **`Email`/`Phone` VO** — подтвердить фабрики (`Email.Create` vs конструктор) по `Cheetah.Core.Domain`.

---
---

# Часть III — Детализация Tier 2 (код-уровень)

> Проработка коммерческого контура (Catalog → Sales Documents) и слоя вовлечённости
> (Notes & Timeline) до уровня сущностей, спецификаций, CQRS и EF — в той же дисциплине, что Часть II.

## C. Catalog — детальная проработка

### C.1. Shared

```csharp
namespace Cheetah.Modules.Catalog.Shared;

public enum ProductType { Goods = 0, Service = 1 }
public enum UnitOfMeasure { Piece = 0, Hour = 1, Day = 2, Kilogram = 3, Liter = 4, Month = 5 }
```

### C.2. Domain — агрегаты

```csharp
public sealed class Product : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public ProductType Type { get; private set; }
    public Guid? CategoryId { get; private set; }
    public UnitOfMeasure Unit { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Product() { }

    public static Product Create(string sku, string name, ProductType type, UnitOfMeasure unit,
        Guid? categoryId = null, string? description = null)
    {
        var p = new Product
        {
            Id = Guid.NewGuid(), Sku = sku, Name = name, Type = type, Unit = unit,
            CategoryId = categoryId, Description = description, IsActive = true
        };
        p.AddDomainEvent(new ProductCreatedIntegrationEvent(p.Id, sku, name));
        return p;
    }

    public void Rename(string name) => Name = name;
    public void MoveToCategory(Guid? categoryId) => CategoryId = categoryId;
    public void Deactivate() { if (IsActive) { IsActive = false; AddDomainEvent(new ProductDeactivatedIntegrationEvent(Id)); } }
    public void Activate() => IsActive = true;
}

public sealed class ProductCategory : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public string Path { get; private set; } = null!;  // материализованный путь "/root/sub" для выборок поддерева

    private ProductCategory() { }

    public static ProductCategory Create(string name, ProductCategory? parent)
    {
        var c = new ProductCategory { Id = Guid.NewGuid(), Name = name, ParentId = parent?.Id };
        c.Path = parent is null ? $"/{c.Id}" : $"{parent.Path}/{c.Id}";
        return c;
    }
}

// Прайс-лист — агрегат; позиции (цены) — child-entities внутри его границы.
public sealed class PriceList : AggregateRoot<Guid>, ICreateAtEntity
{
    private readonly List<PriceListItem> _items = new();

    public string Name { get; private set; } = null!;
    public string Currency { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public IReadOnlyList<PriceListItem> Items => _items;
    public DateTime CreatedAt { get; set; }

    private PriceList() { }

    public static PriceList Create(string name, string currency, bool isDefault = false)
        => new() { Id = Guid.NewGuid(), Name = name, Currency = currency, IsDefault = isDefault };

    public void SetPrice(Guid productId, decimal price, decimal? minQty = null)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == productId && i.MinQty == minQty);
        if (existing is not null) { existing.ChangePrice(price); }
        else _items.Add(PriceListItem.Create(Id, productId, price, minQty));
        AddDomainEvent(new PriceChangedIntegrationEvent(Id, productId, price, Currency));
    }

    public decimal? ResolvePrice(Guid productId, decimal qty)
        => _items.Where(i => i.ProductId == productId && (i.MinQty == null || qty >= i.MinQty))
                 .OrderByDescending(i => i.MinQty ?? 0)
                 .Select(i => (decimal?)i.Price)
                 .FirstOrDefault();
}

public sealed class PriceListItem : Entity<Guid>
{
    public Guid PriceListId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Price { get; private set; }
    public decimal? MinQty { get; private set; }

    private PriceListItem() { }
    internal static PriceListItem Create(Guid priceListId, Guid productId, decimal price, decimal? minQty)
        => new() { Id = Guid.NewGuid(), PriceListId = priceListId, ProductId = productId, Price = price, MinQty = minQty };
    internal void ChangePrice(decimal price) => Price = price;
}
```

### C.3. Спецификации

```csharp
public sealed class ActiveProductsSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression() => p => p.IsActive;
}

public sealed class ProductsByCategorySubtreeSpecification : Specification<Product>
{
    private readonly IReadOnlyCollection<Guid> _categoryIds;  // id поддерева, вычислены по Path
    public ProductsByCategorySubtreeSpecification(IReadOnlyCollection<Guid> categoryIds) => _categoryIds = categoryIds;
    public override Expression<Func<Product, bool>> ToExpression()
        => p => p.CategoryId != null && _categoryIds.Contains(p.CategoryId.Value);
}

public sealed class ProductBySkuSpecification : Specification<Product>
{
    private readonly string _sku;
    public ProductBySkuSpecification(string sku) => _sku = sku;
    public override Expression<Func<Product, bool>> ToExpression() => p => p.Sku == _sku;
}
```

### C.4. Application — CQRS + кэш цен

```csharp
public record CreateProductCommand(string Sku, string Name, ProductType Type, UnitOfMeasure Unit,
    Guid? CategoryId, string? Description) : ICommand<Guid>;
public record SetPriceCommand(Guid PriceListId, Guid ProductId, decimal Price, decimal? MinQty) : ICommand;
public record ListProductsQuery(string? Search, Guid? CategoryId, bool? Active, int Page, int Size)
    : IQuery<PagedResult<ProductDto>>;
public record ResolvePriceQuery(Guid PriceListId, Guid ProductId, decimal Qty) : IQuery<decimal?>;
```

`ResolvePriceQuery` обслуживает горячий путь формирования документов → кэш `Cheetah.Core.Cache`,
инвалидируемый по `PriceChangedIntegrationEvent`:

```csharp
[Export(LifetimeType.Scoped, typeof(IQueryHandler<ResolvePriceQuery, decimal?>))]
public sealed class ResolvePriceQueryHandler : IQueryHandler<ResolvePriceQuery, decimal?>
{
    private readonly IRepository<PriceList> _priceLists;
    private readonly ICacheService _cache;   // Cheetah.Core.Cache

    public async ValueTask<decimal?> HandleAsync(ResolvePriceQuery q, CancellationToken ct)
    {
        var pl = await _cache.GetOrCreateAsync($"pricelist:{q.PriceListId}",
            async () => await _priceLists.GetByIdAsync(q.PriceListId, ct), TimeSpan.FromMinutes(30), ct);
        return pl?.ResolvePrice(q.ProductId, q.Qty);
    }
}
```

### C.5. EF-конфигурация (фрагмент)

```csharp
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products", "catalog");
        b.Property(p => p.Sku).HasMaxLength(64).IsRequired();
        b.Property(p => p.Name).HasMaxLength(300).IsRequired();
        b.Property(p => p.Type).HasConversion<int>();
        b.Property(p => p.Unit).HasConversion<int>();
        b.HasIndex(p => p.Sku).IsUnique();
        b.HasIndex(p => new { p.CategoryId, p.IsActive });
        b.Ignore(p => p.DomainEvents);
    }
}

public sealed class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> b)
    {
        b.ToTable("PriceLists", "catalog");
        b.Property(p => p.Currency).HasMaxLength(3);
        b.HasMany(p => p.Items).WithOne().HasForeignKey(i => i.PriceListId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(p => p.IsDefault);
        b.Ignore(p => p.DomainEvents);
    }
}

public sealed class PriceListItemConfiguration : IEntityTypeConfiguration<PriceListItem>
{
    public void Configure(EntityTypeBuilder<PriceListItem> b)
    {
        b.ToTable("PriceListItems", "catalog");
        b.Property(i => i.Price).HasColumnType("numeric(18,2)");
        b.HasIndex(i => new { i.PriceListId, i.ProductId });
    }
}

// ProductCategory.Path — индекс с text_pattern_ops (PostgreSQL) для префиксных запросов поддерева
// (b.HasIndex(c => c.Path)); либо тип ltree, если выбран он (см. §4.2).
```

---

## S. Sales Documents — детальная проработка

### S.1. Shared

```csharp
namespace Cheetah.Modules.SalesDocuments.Shared;

public enum SalesDocType { Quote = 0, Order = 1, Invoice = 2 }

// Статусы зависят от типа; единый enum с группировкой по DocType.
public enum SalesDocStatus
{
    Draft = 0,
    // Quote: Sent → Accepted | Rejected
    Sent = 1, Accepted = 2, Rejected = 3,
    // Invoice: Issued → Paid | Overdue
    Issued = 10, Paid = 11, Overdue = 12,
    Cancelled = 99
}
```

### S.2. Domain — агрегат с расчётом итогов

```csharp
public sealed class SalesDocument : AggregateRoot<Guid>, IStateMachineEntity<SalesDocStatus>,
    ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<SalesDocumentLine> _lines = new();

    public SalesDocType DocType { get; private set; }
    public string Number { get; private set; } = null!;
    public Guid? DealId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Currency { get; private set; } = null!;
    public SalesDocStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public Guid? PdfFileId { get; private set; }          // ссылка в Cheetah.FileStorage
    public Guid? SourceDocId { get; private set; }        // Quote→Order→Invoice происхождение
    public IReadOnlyList<SalesDocumentLine> Lines => _lines;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public SalesDocStatus State => Status;

    private SalesDocument() { }

    public static SalesDocument Create(SalesDocType type, string number, Guid customerId,
        string currency, Guid? dealId = null)
        => new()
        {
            Id = Guid.NewGuid(), DocType = type, Number = number,
            CustomerId = customerId, Currency = currency, DealId = dealId, Status = SalesDocStatus.Draft
        };

    public void AddLine(Guid productId, string name, decimal unitPrice, decimal qty,
        decimal discount, decimal taxRate)
    {
        EnsureDraft();
        _lines.Add(SalesDocumentLine.Create(Id, productId, name, unitPrice, qty, discount, taxRate));
        Recalculate();
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureDraft();
        _lines.RemoveAll(l => l.Id == lineId);
        Recalculate();
    }

    private void Recalculate()
    {
        Subtotal = _lines.Sum(l => l.UnitPrice * l.Qty);
        DiscountTotal = _lines.Sum(l => l.DiscountAmount);
        TaxTotal = _lines.Sum(l => l.TaxAmount);
        GrandTotal = Subtotal - DiscountTotal + TaxTotal;
    }

    public void Send()
    {
        if (DocType != SalesDocType.Quote) throw new InvalidOperationException("Only quotes are sent.");
        TransitionTo(SalesDocStatus.Sent);
        AddDomainEvent(new SalesDocumentSentIntegrationEvent(Id, CustomerId, DocType));
    }

    public void Accept()
    {
        TransitionTo(SalesDocStatus.Accepted);
        AddDomainEvent(new QuoteAcceptedIntegrationEvent(Id, DealId, GrandTotal, Currency));
    }

    public void MarkPaid()
    {
        if (DocType != SalesDocType.Invoice) throw new InvalidOperationException("Only invoices are paid.");
        TransitionTo(SalesDocStatus.Paid);
        AddDomainEvent(new InvoicePaidIntegrationEvent(Id, CustomerId, GrandTotal, Currency));
    }

    public void AttachPdf(Guid fileId) => PdfFileId = fileId;

    // Создать документ следующего типа «на основе» (копия позиций).
    public SalesDocument ConvertTo(SalesDocType target, string number)
    {
        var doc = Create(target, number, CustomerId, Currency, DealId);
        doc.SourceDocId = Id;
        foreach (var l in _lines)
            doc.AddLine(l.ProductId, l.Name, l.UnitPrice, l.Qty, l.Discount, l.TaxRate);
        return doc;
    }

    private void TransitionTo(SalesDocStatus to)
    {
        // переход валидируется StateMachine на уровне хендлера (IStateMachineValidator);
        // здесь — применяем уже разрешённый переход.
        Status = to;
    }

    private void EnsureDraft()
    {
        if (Status != SalesDocStatus.Draft)
            throw new InvalidOperationException("Lines are editable only in Draft.");
    }
}

public sealed class SalesDocumentLine : Entity<Guid>
{
    public Guid DocumentId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;     // снимок на момент добавления
    public decimal UnitPrice { get; private set; }         // снимок цены
    public decimal Qty { get; private set; }
    public decimal Discount { get; private set; }          // доля 0..1
    public decimal TaxRate { get; private set; }           // доля 0..1

    public decimal DiscountAmount => UnitPrice * Qty * Discount;
    public decimal TaxAmount => (UnitPrice * Qty - DiscountAmount) * TaxRate;
    public decimal LineTotal => UnitPrice * Qty - DiscountAmount + TaxAmount;

    private SalesDocumentLine() { }
    internal static SalesDocumentLine Create(Guid documentId, Guid productId, string name,
        decimal unitPrice, decimal qty, decimal discount, decimal taxRate)
        => new()
        {
            Id = Guid.NewGuid(), DocumentId = documentId, ProductId = productId, Name = name,
            UnitPrice = unitPrice, Qty = qty, Discount = discount, TaxRate = taxRate
        };
}
```

### S.3. StateMachine-конфигурация

```csharp
services.AddStateMachine<SalesDocStatus>(sm => sm
    // КП
    .From(SalesDocStatus.Draft).To(SalesDocStatus.Sent, SalesDocStatus.Cancelled)
    .From(SalesDocStatus.Sent).To(SalesDocStatus.Accepted, SalesDocStatus.Rejected, SalesDocStatus.Cancelled)
    // Счёт
    .From(SalesDocStatus.Draft).To(SalesDocStatus.Issued)
    .From(SalesDocStatus.Issued).To(SalesDocStatus.Paid, SalesDocStatus.Overdue, SalesDocStatus.Cancelled)
    .From(SalesDocStatus.Overdue).To(SalesDocStatus.Paid));
```

### S.4. Application — нумерация и PDF

```csharp
public record CreateSalesDocumentCommand(SalesDocType DocType, Guid CustomerId, string Currency, Guid? DealId)
    : ICommand<Guid>;
public record AddLineCommand(Guid DocumentId, Guid ProductId, decimal Qty, decimal Discount, decimal TaxRate)
    : ICommand;  // UnitPrice/Name резолвятся из Catalog по ProductId (снимок)
public record SendDocumentCommand(Guid DocumentId) : ICommand;
public record ConvertDocumentCommand(Guid DocumentId, SalesDocType Target) : ICommand<Guid>;
public record GeneratePdfCommand(Guid DocumentId) : ICommand<Guid>; // → fileId
```

- **Нумерация** — последовательная per `DocType` per год без дыр: захват через
  `Cheetah.DistributedLock` либо БД-секвенс/таблица счётчиков с `UPDATE ... RETURNING`.
- **`AddLineCommand`** резолвит цену/имя из Catalog (`ResolvePriceQuery` + `Product`) и фиксирует
  **снимок** в строке — последующее `PriceChanged` не меняет выставленный документ.
- **`GeneratePdfCommand`** рендерит шаблон в Infrastructure, сохраняет байты через
  `Cheetah.FileStorage` (`IFileStorage.SaveAsync`), кладёт `PdfFileId` в документ.

### S.5. EF-конфигурация (фрагмент)

```csharp
public sealed class SalesDocumentConfiguration : IEntityTypeConfiguration<SalesDocument>
{
    public void Configure(EntityTypeBuilder<SalesDocument> b)
    {
        b.ToTable("SalesDocuments", "sales");
        b.Property(d => d.Number).HasMaxLength(32).IsRequired();
        b.Property(d => d.Currency).HasMaxLength(3);
        b.Property(d => d.DocType).HasConversion<int>();
        b.Property(d => d.Status).HasConversion<int>();
        foreach (var p in new[] { nameof(SalesDocument.Subtotal), nameof(SalesDocument.DiscountTotal),
                 nameof(SalesDocument.TaxTotal), nameof(SalesDocument.GrandTotal) })
            b.Property(p).HasColumnType("numeric(18,2)");

        b.HasMany(d => d.Lines).WithOne().HasForeignKey(l => l.DocumentId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(d => new { d.DocType, d.Number }).IsUnique();
        b.HasIndex(d => d.CustomerId);
        b.HasIndex(d => d.DealId);
        b.Ignore(d => d.DomainEvents);
    }
}
```

> `QuoteAcceptedIntegrationEvent` → подписчик в Deals может двигать сделку в `Won`;
> `InvoiceOverdue` генерится фоновой задачей скана по `ValidUntil`/срокам (как Activities).

---

## N. Notes & Timeline — детальная проработка

### N.1. Domain — Note (агрегат) и TimelineEntry (read-model)

```csharp
public sealed class Note : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string EntityType { get; private set; } = null!;   // полиморфная привязка
    public Guid EntityId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Body { get; private set; } = null!;
    public DateTime? PinnedAt { get; private set; }

    private readonly List<Guid> _mentions = new();
    private readonly List<Guid> _attachmentFileIds = new();
    public IReadOnlyList<Guid> Mentions => _mentions;
    public IReadOnlyList<Guid> AttachmentFileIds => _attachmentFileIds;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private Note() { }

    public static Note Create(string entityType, Guid entityId, Guid authorId, string body,
        IEnumerable<Guid>? mentions = null, IEnumerable<Guid>? attachmentFileIds = null)
    {
        var n = new Note
        {
            Id = Guid.NewGuid(), EntityType = entityType, EntityId = entityId,
            AuthorId = authorId, Body = body
        };
        if (mentions is not null) n._mentions.AddRange(mentions);
        if (attachmentFileIds is not null) n._attachmentFileIds.AddRange(attachmentFileIds);

        n.AddDomainEvent(new NoteCreatedIntegrationEvent(n.Id, entityType, entityId, authorId));
        foreach (var userId in n._mentions)
            n.AddDomainEvent(new UserMentionedIntegrationEvent(n.Id, userId, entityType, entityId));
        return n;
    }

    public void Edit(string body) => Body = body;
    public void Pin() => PinnedAt = DateTime.UtcNow;
    public void Unpin() => PinnedAt = null;
}

// Read-model ленты: не агрегат, наполняется проекцией из событий других модулей.
public sealed class TimelineEntry : Entity<Guid>
{
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Kind { get; private set; } = null!;     // "deal.stage_changed", "activity.completed", "note.created"…
    public string Title { get; private set; } = null!;
    public string? Payload { get; private set; }           // jsonb
    public Guid? ActorId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private TimelineEntry() { }
    public static TimelineEntry Record(string entityType, Guid entityId, string kind,
        string title, Guid? actorId, DateTime occurredAt, string? payload = null)
        => new()
        {
            Id = Guid.NewGuid(), EntityType = entityType, EntityId = entityId,
            Kind = kind, Title = title, ActorId = actorId, OccurredAt = occurredAt, Payload = payload
        };
}
```

### N.2. Application — проекция Timeline из событий

Timeline — **материализованная проекция** (см. §6.4): модуль подписывается на интеграционные события
других модулей и пишет строки ленты. Реестр «событие → как отрендерить» делает рендеринг
расширяемым.

```csharp
public interface ITimelineProjector<in TEvent>   // одна реализация на тип события
{
    string Kind { get; }
    TimelineEntry Project(TEvent @event);
}

// Пример: проекция смены стадии сделки.
[Export(LifetimeType.Scoped, typeof(IEventHandler<DealStageChangedIntegrationEvent>))]
public sealed class DealStageChangedTimelineHandler : IEventHandler<DealStageChangedIntegrationEvent>
{
    private readonly IRepository<TimelineEntry> _timeline;
    public async ValueTask HandleAsync(DealStageChangedIntegrationEvent e, CancellationToken ct)
    {
        _timeline.Add(TimelineEntry.Record("crm.deal", e.DealId, "deal.stage_changed",
            "Сделка перемещена по воронке", e.ChangedBy, DateTime.UtcNow,
            payload: JsonSerializer.Serialize(new { e.FromStageId, e.ToStageId })));
        await _timeline.SaveChangesAsync(ct);
    }
}
```

> Подписки регистрируются в `OnApplicationInitialization` модуля Notes&Timeline. Зависимость —
> только на `*.DomainEvents` соответствующих модулей (правило «другие модули зависят только от Events»).

### N.3. CQRS / спецификации / EF

```csharp
public record CreateNoteCommand(string EntityType, Guid EntityId, Guid AuthorId, string Body,
    IReadOnlyList<Guid> Mentions, IReadOnlyList<Guid> AttachmentFileIds) : ICommand<Guid>;
public record PinNoteCommand(Guid NoteId) : ICommand;
public record ListNotesQuery(string EntityType, Guid EntityId) : IQuery<IReadOnlyList<NoteDto>>;
public record GetTimelineQuery(string EntityType, Guid EntityId,
    IReadOnlyCollection<string>? Kinds, DateTime? Before, int Size) : IQuery<IReadOnlyList<TimelineDto>>;

public sealed class NotesByEntitySpecification : Specification<Note>
{
    private readonly string _type; private readonly Guid _id;
    public NotesByEntitySpecification(string type, Guid id) => (_type, _id) = (type, id);
    public override Expression<Func<Note, bool>> ToExpression()
        => n => n.EntityType == _type && n.EntityId == _id;
}
```

```csharp
public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> b)
    {
        b.ToTable("Notes", "notes");
        b.Property(n => n.EntityType).HasMaxLength(64).IsRequired();
        b.Property(n => n.Body).IsRequired();
        // коллекции id — jsonb-колонки (PostgreSQL)
        b.Property(n => n.Mentions).HasColumnType("jsonb");
        b.Property(n => n.AttachmentFileIds).HasColumnType("jsonb");
        b.HasIndex(n => new { n.EntityType, n.EntityId });
        b.Ignore(n => n.DomainEvents);
    }
}

public sealed class TimelineEntryConfiguration : IEntityTypeConfiguration<TimelineEntry>
{
    public void Configure(EntityTypeBuilder<TimelineEntry> b)
    {
        b.ToTable("TimelineEntries", "notes");
        b.Property(t => t.Kind).HasMaxLength(64);
        b.Property(t => t.Payload).HasColumnType("jsonb");
        // курсорная пагination по (EntityType, EntityId, OccurredAt desc)
        b.HasIndex(t => new { t.EntityType, t.EntityId, t.OccurredAt });
    }
}
```

---

## Технические уточнения для Tier 2 (свериться при реализации)

1. **`ICacheService` / `IFileStorage`** — точные имена и сигнатуры по `Cheetah.Core.Cache` и
   `Cheetah.FileStorage` (в примерах — предполагаемые).
2. **jsonb-маппинг коллекций** (`List<Guid>`) — через `HasConversion` + value-comparer или нативный
   Npgsql jsonb; выбрать единый подход (общий с Custom Fields).
3. **Нумерация документов без дыр** — DistributedLock vs БД-секвенс; зафиксировать (влияет на учётные
   требования).
4. **Рендер PDF** — библиотека шаблонов (QuestPDF/иное) добавляется в `Directory.Packages.props`.
5. **StateMachine с одним enum на два типа документов** (Quote/Invoice) — проверить, что пересечение
   `Draft` корректно разрешается; при необходимости развести на два enum.
