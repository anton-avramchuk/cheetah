# Cheetah.Modules.Notes.* — модуль «Заметки» (абстрактный шаблон-модуль)

> **Статус: реализовано (MVP), находки ревью исправлены.** Код — в `src/Modules/Notes/`:
> 9 шаблонных сборок + 3 тестовых, 58 тестов зелёных (Domain 24, Application 22, Client 12),
> решение собирается. Гайд по расширению —
> [`src/Modules/Notes/README.md`](../../src/Modules/Notes/README.md).
> Модуль выделен из [`NotesTimeline`](notes-timeline.md) как самостоятельный (без read-model «лента»);
> образец структуры и приёмов — [`Cheetah.Modules.Customer`](../../src/Modules/Customer/README.md).

> **Тип:** base/template (каркас), а не готовый микросервис. Модуль поставляет **только абстрактные
> базовые типы и generic-хелперы**; конкретное приложение наследует их, генерирует миграции у себя и
> получает рабочий CRUD заметок «из коробки», дописав ~6 классов.

---

## 1. Назначение и главное требование

**Заметка к любой сущности CRM.** Модуль предметно-нейтрален: привязка — полиморфная пара
`(EntityType: string, EntityId: Guid)`. `EntityType` — произвольная строка; известные ключи
(`crm.deal`, `crm.customer`, `crm.contact`, `crm.lead`, `crm.invoice`) — лишь конвенция `EntityRefKeys`,
общая с Tags/Activities/CustomFields. Модуль **не знает** и **не ссылается** на модули-владельцы
сущностей: ни project reference, ни FK через границу.

**Расширяемость (обязательна).** Сущность заметки и её ViewModel должны расширяться полями
приложения-наследника без форка модуля. Механизм — структурный (compile-time), как в Customer/Activities:

| Уровень | Механизм |
|---|---|
| структурный (compile-time) | `NoteBase`/`NoteDtoBase`/`…RequestBase` + `ConfigureCustom`-hook в EF-конфигурации + фабрика/проектор наследника |
| runtime без миграций | `(EntityType, EntityId)` + карман `Attributes (jsonb)`; полноценно — модуль [Custom Fields](custom-fields.md) |
| интеграционный | доменные события (`NoteCreated`/`NoteUpdated`/`NoteRemoved`/`UserMentioned`) — потребители подписываются, модуль о них не знает |

**Что даёт наследнику:** `sealed class Note : NoteBase` со своими полями (`Visibility`, `Category`, …),
`sealed record NoteDto : NoteDtoBase`, свои Request-ы — и рабочий CRUD, упоминания, пин, soft-delete.

---

## 2. Отношение к существующему NotesTimeline

**Решение владельца: `NotesTimeline` — отдельный сервис, он не трогается.** `Notes` строится рядом,
как самостоятельный модуль: свои сборки, своё пространство имён, своя БД (connection string `Notes`,
схема `notes`), свои маршруты. Пересечений на уровне кода, схемы и конфигурации нет.

Что при этом сознательно принимается:

- **два write-model заметок в решении** — `NotesTimeline` (заметка + лента в одной БД) и `Notes`
  (только заметки). Это независимые сервисы, а не два владельца одной таблицы: приложение включает
  один из них;
- **дублируются контракты и события** (`NoteCreatedIntegrationEvent` и др.) в двух пространствах имён.
  Потребитель (Notification, Workflow, лента) подписывается на события того сервиса, который включён;
- общими остаются только **конвенции**, а не код: полиморфная пара `(EntityType, EntityId)` и ключи
  `EntityRefKeys` (`crm.deal`, `crm.customer`, …), те же, что в Tags/Activities/CustomFields.

> **Взаимоисключаемость по маршрутам.** Оба модуля по умолчанию занимают префикс `api/notes`,
> поэтому включать `CheetahNotesDefaultModule` и модуль `NotesTimeline` в одном хосте нельзя:
> маршруты зарегистрируются дважды и каждый запрос упадёт с `AmbiguousMatchException` (500).
> Приложение включает **один** из них; если сосуществование всё же нужно — переопределите `Route`
> в наследниках шаблонов эндпоинтов (например на `api/entity-notes`).

`Notes` **не зависит** от `NotesTimeline` ни одной ссылкой. Если приложению нужна лента поверх
`Notes`, `Timeline`-сервис подписывается на `Cheetah.Modules.Notes.DomainEvents.NoteCreatedIntegrationEvent`
через уже существующий шов `AddTimelineProjector<TEvent, TProjector>()` — правок в `NotesTimeline` для
этого не требуется.

---

## 3. Состав сборок и граф зависимостей

```
Notes.DomainEvents   → Core.Events                          (record-события, без прочих зависимостей)
Notes.Shared         → Core                                 (константы, EntityRefKeys)
Notes.Contracts      → Core + Shared                        (ABSTRACT NoteDtoBase / …RequestBase)
Notes.Domain         → DomainEvents + Core.Specification    (abstract NoteBase, generic-спецификации)
Notes.Infrastructure → Domain + EF + EF.PostgreSql          (abstract DbContextBase/ConfigBase, AddNotesInfrastructure<>)
Notes.Application    → Domain + Contracts + CQRS + Events   (generic handlers, AddNotesApplication<>)
Notes.Api            → Application + Contracts + Backend.Endpoints (ABSTRACT декларативные эндпоинты)
Notes.Default        → всё выше + Mapping.Mapster           (конкретные типы, эндпоинты, DbContext, миграция)
Notes.Client         → Contracts                            (generic HTTP-клиент, см. §7)
Tests: Notes.Domain.Tests, Notes.Application.Tests, Notes.Client.Tests
```

Все проекты — в `Cheetah.slnx`, папка `/Modules/Notes/`. Каталог — `src/Modules/Notes/` + `README.md`.

---

## 4. Ключевые абстракции

| Тип | Сборка | Роль |
|---|---|---|
| `NoteBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity` | Domain | агрегат; `InitializeCore`, `Edit`, `Pin`/`Unpin`, `Remove` (soft-delete), `SetAttributes` |
| `NotesByEntitySpecification<TNote>`, `PinnedNotesByEntitySpecification<TNote>`, `NotesByAuthorSpecification<TNote>`, `NoteThreadSpecification<TNote>` | Domain | generic-спецификации (raw LINQ в Application запрещён) |
| `NoteDtoBase`, `Create`/`Update`/`GetNoteById`/`GetNotesByEntity`/`GetNoteReplies`/`NoteLifecycle` `…RequestBase` | Contracts | абстрактные `record` — точка расширения ViewModel и запросов |
| `NotesDbContextBase<TContext, TNote>` | Infrastructure | `DbSet<TNote> Notes`, `[ConnectionStringName("Notes")]`, конфигурация через abstract-hook |
| `NoteConfigurationBase<TNote>` | Infrastructure | таблица/схема `notes`, длины, jsonb-коллекции `Mentions`/`AttachmentFileIds`/`Attributes`, индексы, query-filter `RemovedAt == null`, hook `ConfigureCustom` |
| `INoteFactory<TNote, TCreateRequest>` | Application | `Create(request)` — замена `new` для абстрактной сущности |
| `INoteProjector<TNote, TDto>` | Application | `ToDto(note)` — проекция вместо Mapster (расширенные поля без скрытой конфигурации) |
| generic CQRS: `CreateNoteCommand<TCreate>`, `UpdateNoteCommand<TUpdate>`, `Pin/Unpin/RemoveNoteCommand`, `GetNoteByIdQuery<TDto>`, `GetNotesByEntityQuery<TDto>` | Application | handler'ы закрываются типами наследника |
| `CreateNoteEndpoint<>`, `GetNoteByIdEndpoint<>`, `GetNotesByEntityEndpoint<>`, `GetNoteRepliesEndpoint<>`, `UpdateNoteEndpoint<>`, `Pin`/`Unpin`/`RemoveNoteEndpoint<>` | Api | **абстрактные** декларативные эндпоинты; маршруты регистрирует генератор по закрытым наследникам |
| `CheetahNotesApiModule` | Api | носитель зависимостей Api-слоя (маршрутов сам не объявляет) |

### Архитектурные приёмы абстрактности (наследуются от Customer/NotesTimeline)

- **Нельзя `new` абстрактную сущность** в generic-handler → фабрика `INoteFactory`.
- **`[Export]` source-gen работает только по закрытым типам** → открытые generic-handler'ы
  регистрируются вручную в `AddNotesApplication<…>` / `AddNotesInfrastructure<…>`.
- **Абстрактный DbContext не мигрируется** → конкретный `DbContext`, design-time factory и миграции — у наследника.
- **События публикуются строго после `SaveChangesAsync`**, затем `ClearDomainEvents()`.

---

## 5. Модель данных (таблица `notes."Notes"`)

| Колонка | Тип | Примечание |
|---|---|---|
| `Id` | uuid PK | |
| `EntityType` | varchar(64) NOT NULL | полиморфная привязка |
| `EntityId` | uuid NOT NULL | |
| `AuthorId` | uuid NOT NULL | ссылка на Identity по Id, без FK |
| `Body` | varchar(8000) NOT NULL | |
| `Mentions` | jsonb | `Guid[]`, value converter + comparer |
| `AttachmentFileIds` | jsonb | `Guid[]` — id из файлового хранилища |
| `ParentNoteId` | uuid NULL | треды/ответы |
| `PinnedAt` | timestamptz NULL | |
| `Attributes` | jsonb NULL | карман расширения без миграций |
| `CreatedAt`/`UpdatedAt`/`RemovedAt` | timestamptz NULL | аудит + soft-delete |

Индексы: `(EntityType, EntityId)`, `PinnedAt`, `AuthorId`, `ParentNoteId`.
Global query filter: `RemovedAt IS NULL`.

---

## 6. API (`api/notes`, префикс переопределяем)

| Метод | Маршрут | Назначение |
|---|---|---|
| `POST` | `/api/notes` | создать заметку (`201 Created` + `Guid`) |
| `GET` | `/api/notes/{id:guid}` | заметка по id (**новое** — в NotesTimeline отсутствует) |
| `GET` | `/api/notes?entityType=&entityId=[&pinnedOnly=][&skip=][&take=]` | заметки сущности; закреплённые первыми, затем новые сверху (**пагинация — новое**; параметры страницы необязательны) |
| `GET` | `/api/notes/{id:guid}/replies` | ветка треда (**новое**, `ParentNoteId` уже в модели) |
| `PUT` | `/api/notes/{id:guid}` | правка тела + упоминаний |
| `POST` | `/api/notes/{id:guid}/pin` \| `/unpin` | закрепление |
| `DELETE` | `/api/notes/{id:guid}` | soft-delete |

**Эндпоинты — декларативные** (`Cheetah.Backend.Endpoints` + генератор, как в Identity/Catalog/Deals),
а не императивный `MapPost` — это отличие от текущего `NoteEndpointsBase`; при переезде базы
переписываются на декларативную форму, generic-контракт наследования сохраняется.

`NoteValidationException` → `400 BadRequest` с `{ error }`.

---

## 7. Client

`Cheetah.Modules.Notes.Client` — generic HTTP-клиент над закрытыми типами наследника:

```csharp
public interface INotesClient<TCreateRequest, TUpdateRequest, TDto>
    where TCreateRequest : CreateNoteRequestBase
    where TUpdateRequest : UpdateNoteRequestBase
    where TDto : NoteDtoBase
{
    ValueTask<Guid> CreateAsync(TCreateRequest request, CancellationToken ct = default);
    ValueTask<TDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<IReadOnlyList<TDto>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
    ValueTask UpdateAsync(Guid id, TUpdateRequest request, CancellationToken ct = default);
    ValueTask PinAsync(Guid id, CancellationToken ct = default);
    ValueTask RemoveAsync(Guid id, CancellationToken ct = default);
}
```

Регистрация — `services.AddNotesClient<CreateNoteRequest, UpdateNoteRequest, NoteDto>()`, named-клиент
`CheetahAPI`. Аутентификация server-to-server — `Cheetah.Backend.ServiceAuth` (M2M) либо
`UserTokenForwardingHandler` (on-behalf-of), по образцу `Deals.Client`/`Catalog.Client`.
Тесты — `Notes.Client.Tests` (обязательны по канону).

---

## 8. События (Notes.DomainEvents)

```csharp
public record NoteCreatedIntegrationEvent(Guid NoteId, string EntityType, Guid EntityId, Guid AuthorId) : EventBase;
public record NoteUpdatedIntegrationEvent(Guid NoteId) : EventBase;
public record NoteRemovedIntegrationEvent(Guid NoteId, string EntityType, Guid EntityId) : EventBase;
public record UserMentionedIntegrationEvent(Guid NoteId, string EntityType, Guid EntityId, Guid MentionedUserId, Guid ByUserId) : EventBase;
```

Потребители: **Timeline** (строка ленты по `NoteCreated`), **Notification** (уведомление по
`UserMentioned`), **Workflow** (триггеры). Ни один из них не зависит от `Notes.Domain` — только от
`Notes.DomainEvents`. `UserMentioned` публикуется по одному на **каждое новое** упоминание
(при `Edit` — только для добавившихся).

---

## 9. Точки регистрации у наследника

```csharp
// Infrastructure: DbContext, мигратор, PostgreSQL, IRepository<Note, Guid>
services.AddNotesInfrastructure<AppNotesDbContext, Note>();

// Application: фабрика, проектор, закрытые CQRS-handler'ы
services.AddNotesApplication<Note, CreateNoteRequest, UpdateNoteRequest,
    NoteDto, NoteFactory, NoteProjector>();

// Api
public sealed class AppNotesApiModule
    : CheetahNotesApiModuleBase<AppNoteEndpoints, CreateNoteRequest, UpdateNoteRequest, NoteDto>;
```

Минимальный набор классов наследника: `Note`, `NoteDto`, `CreateNoteRequest`, `UpdateNoteRequest`,
`NoteFactory`, `NoteProjector`, `AppNotesDbContext` + `NoteConfiguration` (+ миграция).

---

## 10. Проект `Notes.Default` (закрывает пробел NotesTimeline)

Отдельная сборка `Cheetah.Modules.Notes.Default` с **готовой конкретной реализацией** —
`Note`/`NoteDto`/`NoteFactory`/`NoteProjector`/`NotesDbContext`/`NoteConfiguration` + миграция
PostgreSQL. Даёт «включил и работает» без наследования и служит эталонным примером расширения
(как `Default`-проекты в Catalog/Deals). В NotesTimeline такого нет — конкретные типы живут только
в тестах, миграции не существует.

---

## 11. Что сделано

1. ✅ 12 проектов в `src/Modules/Notes/`, добавлены в `Cheetah.slnx` (папка `/Modules/Notes/`).
2. ✅ `DomainEvents` → `Shared` → `Contracts`.
3. ✅ `Domain`: `NoteBase` + 4 спецификации; `Domain.Tests` — 18 тестов.
4. ✅ `Infrastructure`: `NotesDbContextBase<TContext, TNote>`, `NoteConfigurationBase`,
   `GuidListJsonConverters`, `AddNotesInfrastructure<>`.
5. ✅ `Application`: фабрика/проектор, generic CQRS + `GetNoteByIdQuery`, пагинация, треды,
   `AddNotesApplication<>`; `Application.Tests` — 13 тестов.
6. ✅ `Api`: **абстрактные** декларативные эндпоинты (генератор игнорирует abstract-классы, поэтому
   маршруты появляются в сборке с закрытыми наследниками) + `CheetahNotesApiModule` как носитель
   зависимостей Api-слоя.
7. ✅ `Client` + `Client.Tests` — 10 тестов.
8. ✅ `Notes.Default`: конкретные типы, эндпоинты, Mapster-профиль `Request → Command/Query`,
   `NotesDbContext` + миграция `InitialNotes`. Генератор регистрирует все 8 маршрутов.
9. ✅ `src/Modules/Notes/README.md`.

`NotesTimeline` не затронут (§2).

**По итогам код-ревью дополнительно сделано:**

- `NoteValidationException : CrmException` (400) и `EntityNotFoundException` (404) вместо голого
  `Exception`, который глобальный обработчик не разбирал → был 500 на любой неизвестный id;
- `Edit`/`ChangeAttachments`: `null` = «не трогать» — правка одного текста больше не стирает
  упоминания; вложения стало можно менять после создания;
- страницы списков читает порт `INoteReader` (Domain) с EF-реализацией: сортировка и срез в БД,
  без трекинга, вместо материализации всей выборки в память;
- валидация длин `EntityType`/`Body` в домене (было 500 из Npgsql на границе varchar);
- события `NotePinned`/`NoteUnpinned`; проверка родителя треда (существует, той же сущности,
  треды плоские); пагинация ветки треда; копии коллекций в DTO; имена эндпоинтов разведены
  с `NotesTimeline` префиксом `Notes…`; клиент бросает ошибку вместо `Guid.Empty`.

**Follow-up:** авторизация через Permissions (§12), Blazor-UI сборка, gRPC-транспорт, грид
(`IGridRepository`), если понадобится серверная сортировка/фильтрация по произвольным полям.

---

## 12. Риски и открытые вопросы

- **Сосуществование с NotesTimeline** (§2): в решении два независимых сервиса заметок, контракты и
  события частично дублируются. Принято сознательно — `NotesTimeline` не трогается.
- **Авторизация — follow-up.** Решение владельца: правка/удаление чужой заметки закрывается **позже,
  через модуль Permissions**. MVP авторизацию не проверяет; шов закладывается заранее — мутирующие
  handler'ы получают `AuthorId` из команды, чтобы позже добавить проверку
  «автор либо право `notes.manage`» без смены сигнатур.
- **Видимость заметки** (`private`/`team`/`public`): остаётся **полем наследника** — базовый модуль
  её не фиксирует (не всем нужна, а её семантика завязана на Teams/Permissions конкретного приложения).
- **Вложения**: `AttachmentFileIds` — просто `Guid[]`, целостность с файловым хранилищем не проверяется.
- **Мульти-тенантность**: подключение резолвится `IConnectionStringResolver` — специальных действий
  в модуле не требуется.
