# Cheetah.Modules.Tags — проект микросервиса тэгов

> Статус: проектное описание (черновик). Кода в репозитории ещё нет.

## 1. Назначение и границы

**Что делает:** централизованный сервис, который хранит тэги и их привязки к произвольным
сущностям *других* сервисов. Источник истины (source of truth) по тэгам и по связям
«тэг ↔ сущность».

**Чего НЕ делает:**

- не хранит сами бизнес-сущности (контакты, сделки и т.п.) — только их идентификаторы;
- не знает доменной логики потребителей. Видит сущность как пару `(EntityType, EntityId)`.

**Ключевая идея:** сервис не знает заранее, что бывает «контакт» или «сделка». Каждый сервис
**на старте регистрирует** свои типы сущностей, пригодные для тэгирования (taggable entity
types), вместе с ограничениями. Tags хранит этот каталог и проверяет по нему все операции.

---

## 2. Архитектурная роль

```
                       ┌─────────────────────────┐
   startup register →  │      Tags Service        │
  ┌──────────┐  (push) │  ┌────────────────────┐  │
  │ CRM svc  │────────▶│  │ Taggable Catalog   │  │  ← каталог зарегистрированных типов
  └──────────┘         │  ├────────────────────┤  │
  ┌──────────┐  assign │  │ Tags + Groups      │  │  ← словарь тэгов (per tenant)
  │ Desk svc │────────▶│  ├────────────────────┤  │
  └──────────┘         │  │ Assignments        │  │  ← связи tag ↔ (type,id)
                       │  └────────────────────┘  │
                       └───────────┬──────────────┘
                                   │ events (Redis/Kafka)
              TagAssigned/Unassigned/Created… ▼
                       потребители обновляют свою денормализованную проекцию
```

Свой PostgreSQL, общение через REST + gRPC, события через шину (Redis/Kafka) — всё как в
`CLAUDE.md`.

---

## 3. Регистрация применимых сущностей (ядро задачи)

### 3.1. Что регистрируется — дескриптор типа

```csharp
public sealed record TaggableEntityTypeDescriptor
{
    public string Key { get; init; }            // стабильный ключ: "crm.contact", "desk.ticket"
    public string DisplayName { get; init; }     // "Контакт" (для UI каталога)
    public string OwnerService { get; init; }     // "crm-service" (кто владеет)
    public TagEntityIdType IdType { get; init; }   // Guid | Long | String — как кастовать EntityId

    // ограничения (политика тэгирования для этого типа):
    public int? MaxTagsPerEntity { get; init; }      // null = без лимита
    public bool AllowAdHocTags { get; init; }        // можно ли создавать тэги «на лету»
    public string[]? AllowedGroups { get; init; }     // если задано — только тэги из этих групп
}
```

`Key` — **контракт между сервисами**, неизменяемый строковый идентификатор. Рекомендуется
конвенция `"{service}.{entity}"`; держать константы в `Tags.Shared` или в `Shared` самого
потребителя.

### 3.2. Механизм — push при старте, идемпотентно

Рекомендуемый вариант (а не события): сервис при старте делает **upsert-вызов** в Tags через
клиентскую библиотеку. Почему push, а не событие:

- регистрация — это синхронный контракт «я обещаю, что такие типы есть», её удобно ретраить и
  видеть результат;
- идемпотентный upsert = self-healing: каталог восстанавливается при каждом рестарте, переживает
  потерю БД;
- порядок запуска сервисов не важен (ретраи с backoff, неблокирующий hosted service).

На стороне потребителя — декларативно:

```csharp
services.AddTagsClient(o => o.BaseUrl = cfg["Tags:Url"])
        .RegisterTaggableTypes(reg =>
        {
            reg.Add("crm.contact", "Контакт", x => { x.IdType = TagEntityIdType.Guid; x.MaxTagsPerEntity = 50; });
            reg.Add("crm.deal",    "Сделка",  x => { x.IdType = TagEntityIdType.Guid; x.AllowedGroups = ["stage", "priority"]; });
        });
```

`RegisterTaggableTypes` собирает дескрипторы в память, а фоновый `IHostedService` при старте шлёт
их батчем в Tags (`POST /api/tags/registry/sync` или gRPC `Registry.Sync`) с ретраями. Tags делает
upsert по `Key` в рамках `OwnerService`.

> **Открытый вопрос:** альтернатива — регистрация через событие
> `TaggableTypeRegisteredIntegrationEvent` (полностью асинхронно, без знания адреса Tags). Минус —
> нет немедленного ответа об ошибке валидации. Рекомендация: push основной, событие — опционально.

---

## 4. Доменная модель

| Агрегат / сущность | Назначение | Ключевые поля |
|---|---|---|
| **TaggableEntityType** (AggregateRoot) | зарегистрированный тип сущности | `Key`, `DisplayName`, `OwnerService`, `IdType`, ограничения |
| **Tag** (AggregateRoot) | сам тэг | `Id`, `TenantId`, `Name`, `Slug`, `Color`, `Description`, `GroupId?` |
| **TagGroup** (AggregateRoot) | категория тэгов (Priority, Stage…) | `Id`, `Name`, `Mode` (single/multi выбор) |
| **TagAssignment** (Entity) | связь тэг ↔ сущность | `TagId`, `EntityTypeKey`, `EntityId (string)`, `AssignedBy`, `AssignedAt` |

Замечания по решениям:

- **EntityId хранится строкой** (канонично), а `IdType` из регистрации используется для
  валидации/каста на входе. Так один сервис покрывает Guid-, long- и string-идентификаторы без
  зоопарка колонок.
- **Scope тэгов.** Рекомендация: тэги — глобальные в рамках тенанта; ограничение применимости
  делается на уровне типа (`AllowedGroups`) и группы. Это проще, чем «тэг привязан к одному типу
  сущности», и переиспользуемо («VIP» применим и к контакту, и к сделке).
- **TagGroup.Mode** = single/multi: например, группа «Стадия» допускает только один тэг на
  сущность (single), «Метки» — много.

---

## 5. Структура проектов

По канону `CLAUDE.md`, как `Cheetah.Modules.Identity.*`:

```
src/Modules/Tags/
├── Cheetah.Modules.Tags.DomainEvents/   # интеграционные события (TagAssigned…)
├── Cheetah.Modules.Tags.Shared/          # enums (TagEntityIdType, TagGroupMode), конвенции ключей
├── Cheetah.Modules.Tags.Contracts/       # DTO, Request/Response, TaggableEntityTypeDescriptor
├── Cheetah.Modules.Tags.Domain/          # агрегаты, спецификации, интерфейсы репозиториев
├── Cheetah.Modules.Tags.Infrastructure/  # EF Core, миграции, реализации репозиториев, интеграции (PostgreSQL)
├── Cheetah.Modules.Tags.Application/      # CQRS-хендлеры
├── Cheetah.Modules.Tags.Api/             # Minimal API + gRPC endpoints
├── Cheetah.Modules.Tags.Client/          # клиент для потребителей (регистрация + тэгирование)
└── Tests/
    ├── Cheetah.Modules.Tags.Domain.Tests
    ├── Cheetah.Modules.Tags.Application.Tests
    └── Cheetah.Modules.Tags.Client.Tests
```

---

## 6. API (REST + gRPC)

**Реестр (registry):**

- `POST /api/tags/registry/sync` — батч-upsert дескрипторов (вызывает клиент при старте)
- `GET  /api/tags/registry` — список зарегистрированных типов (для UI/админки)

**Тэги и группы:**

- `POST/PUT/DELETE /api/tags`, `GET /api/tags?group=…&search=…`
- `POST/PUT/DELETE /api/tags/groups`

**Привязки:**

- `POST   /api/tags/assignments` — назначить тэг(и): `{ entityType, entityId, tagIds[] }`
- `DELETE /api/tags/assignments` — снять
- `GET    /api/tags/assignments?entityType=…&entityId=…` — тэги конкретной сущности
- `POST   /api/tags/assignments/batch-get` — тэги для **списка** сущностей (анти-N+1, для
  списочных экранов)
- `POST   /api/tags/query/entities` — **обратный поиск**: вернуть `entityId[]` по фильтру тэгов
  (any/all/none), с пагинацией

gRPC дублирует горячие пути (`BatchGetTags`, `QueryEntities`, `Registry.Sync`) — это межсервисное
взаимодействие, где gRPC уместен.

---

## 7. События (публикует Tags)

```csharp
public record TagAssignedIntegrationEvent(Guid TagId, string EntityType, string EntityId, Guid TenantId) : EventBase;
public record TagUnassignedIntegrationEvent(Guid TagId, string EntityType, string EntityId, Guid TenantId) : EventBase;
public record TagCreatedIntegrationEvent(Guid TagId, Guid TenantId, string Name);
public record TagRenamedIntegrationEvent(Guid TagId, string NewName);
public record TagDeletedIntegrationEvent(Guid TagId);
```

**Зачем потребителю эти события.** Чтобы не дёргать Tags на каждый показ/фильтр, потребитель держит
**денормализованную проекцию** тэгов на своих сущностях (например, массив `tag_ids` в своей таблице
или отдельная локальная таблица), обновляя её по событиям. Tags остаётся источником истины, а чтение
и фильтрация у потребителя локальны и быстры. Этот паттерн стоит явно зафиксировать в README модуля.

---

## 8. Целостность данных

**Удаление сущности у потребителя → висячие привязки.** Два механизма (рекомендуются оба):

1. **Реактивно:** Tags подписывается на общий `EntityDeletedIntegrationEvent(EntityType, EntityId)`,
   который сервисы публикуют при удалении, и чистит привязки. Требует, чтобы потребители публиковали
   это событие.
2. **Лениво/верифицирующе:** `QueryEntities` возвращает только id; потребитель сам отсекает уже
   несуществующие. Плюс периодический фоновый reconciliation (через `BackgroundTasks`/Cron).

**Снятие регистрации типа.** Если `Key` исчезает из sync-набора сервиса — не удалять данные сразу
(могут быть привязки). Помечать тип `Deprecated`, чистить по политике/вручную.

---

## 9. Производительность (цель 10k RPS)

Таблица `TagAssignments`:

- PK/уникальный индекс: `(TenantId, EntityTypeKey, EntityId, TagId)`
- индекс для тэгов сущности: `(TenantId, EntityTypeKey, EntityId)`
- индекс для обратного поиска: `(TenantId, EntityTypeKey, TagId)`

Обратный поиск «по всем тэгам [a,b,c]» (AND) — пересечение через
`GROUP BY entityId HAVING count(distinct tagId)=N` либо `INTERSECT`. Кэш горячих словарей тэгов
(`Core.Cache`) — список тэгов/групп меняется редко, читается часто. `BatchGet` обязателен, чтобы
списочные экраны не порождали N+1.

---

## 10. Мультитенантность

Тэги, группы и привязки скоупятся по `TenantId` (есть `Cheetah.Core.Tenants` /
`EntityFramework.Tenants`). Каталог `TaggableEntityType` — **глобальный** (это контракт платформы, не
тенант-данные).

---

## 11. Решения, которые нужно зафиксировать

1. **Регистрация:** push-upsert через клиент (рекомендация) vs событие vs оба.
2. **Scope тэгов:** глобальные в тенанте (рекомендация) vs привязанные к типу сущности.
3. **Денормализация у потребителя:** делаем паттерн обязательным/опциональным, кладём ли в Client
   готовый «projection-helper».
4. **EntityId:** строкой канонически (рекомендация) vs полиморфные колонки.
5. **Очистка orphan’ов:** требуем ли от потребителей `EntityDeletedIntegrationEvent`.

---

## 12. Следующий шаг

После фиксации решений из §11 — детализация: точные сигнатуры команд/запросов CQRS, спецификации,
EF-конфигурации с индексами и API-эндпоинты в `OnApplicationInitialization`.
