# Cheetah.Modules.Notes.* — абстрактный шаблон-модуль «Заметки»

> **Тип:** base/template (каркас) + готовая реализация `Default`.
> Шаблонные сборки поставляют **только абстрактные базовые типы и generic-хелперы**. Приложение либо
> подключает `Cheetah.Modules.Notes.Default` и получает рабочий CRUD «из коробки», либо наследует
> типы, дописав ~6 классов и свою миграцию.

## Назначение

**Заметка к любой сущности CRM.** Модуль предметно-нейтрален: привязка — полиморфная пара
`(EntityType: string, EntityId: Guid)`. `EntityType` — произвольная строка; `EntityRefKeys`
(`crm.deal`, `crm.customer`, `crm.contact`, `crm.lead`, `crm.invoice`) — лишь конвенция, общая с
Tags/Activities/CustomFields. Модуль **не ссылается** на модули-владельцы сущностей: ни project
reference, ни FK через границу.

Возможности: текст с упоминаниями (`@user`), вложения, треды (ответы на заметку), закрепление,
soft-delete, схемо-независимый карман `Attributes (jsonb)`.

> Отдельный сервис [`NotesTimeline`](../NotesTimeline) (заметки + лента хронологии) существует
> независимо и этим модулем не затрагивается. См. `docs/modules/notes.md`, §2.

## Состав сборок и граф зависимостей

```
Notes.DomainEvents   → Core.Events                          (record-события)
Notes.Shared         → Core                                 (константы, EntityRefKeys)
Notes.Contracts      → Contracts + Core + Shared            (ABSTRACT NoteDtoBase / …RequestBase)
Notes.Domain         → DomainEvents + Specification         (abstract NoteBase, generic-спеки)
Notes.Infrastructure → Domain + EF + EF.PostgreSql          (abstract DbContextBase/ConfigBase, AddNotesInfrastructure<>)
Notes.Application    → Domain + Contracts + CQRS + Events   (generic handlers, AddNotesApplication<>)
Notes.Api            → Application + Contracts + Backend.Endpoints (ABSTRACT декларативные эндпоинты)
Notes.Default        → всё вышеперечисленное + Mapping.Mapster    (конкретные типы, DbContext, миграция)
Notes.Client         → Contracts                            (generic HTTP-клиент)
Tests: Domain.Tests (24), Application.Tests (22), Client.Tests (12) — 58 тестов
```

## Ключевые абстракции

| Тип | Сборка | Роль |
|---|---|---|
| `NoteBase : AggregateRoot<Guid>` | Domain | агрегат; `InitializeCore`, `Edit`, `ChangeAttachments`, `Pin`/`Unpin`, `Remove` (soft-delete), `SetAttributes` |
| `NotesByEntity` / `PinnedNotesByEntity` / `NotesByAuthor` / `NoteThread` `Specification<TNote>` | Domain | generic-спецификации (raw LINQ в Application запрещён) |
| `NoteDtoBase`, `Create`/`Update`/`GetNoteById`/`GetNotesByEntity`/`GetNoteReplies`/`NoteLifecycle` `…RequestBase` | Contracts | абстрактные `record` — точка расширения ViewModel и запросов |
| `NotesDbContextBase<TContext, TNote>` | Infrastructure | `DbSet<TNote>`, `[ConnectionStringName("Notes")]`, конфигурация через abstract-hook |
| `NoteConfigurationBase<TNote>` | Infrastructure | схема `notes`, длины, jsonb-коллекции, индексы, query-filter soft-delete, hook `ConfigureCustom` |
| `INoteFactory` / `INoteProjector` | Application | `Create(request)` / `ToDto(note)` — замена `new` абстрактной сущности и проекция вместо Mapster |
| `INoteReader<TNote>` | Domain (порт) → Infrastructure (`EfNoteReader`) | постраничное чтение списков: отбор, сортировка и срез — в БД, без трекинга |
| generic CQRS: `CreateNoteCommand<>`, `UpdateNoteCommand<>`, `Pin/Unpin/RemoveNoteCommand`, `GetNoteByIdQuery<>`, `GetNotesByEntityQuery<>`, `GetNoteRepliesQuery<>` | Application | handler'ы закрываются типами наследника |
| `CreateNoteEndpoint<>`, `GetNoteByIdEndpoint<>`, `GetNotesByEntityEndpoint<>`, `GetNoteRepliesEndpoint<>`, `UpdateNoteEndpoint<>`, `Pin`/`Unpin`/`RemoveNoteEndpoint<>` | Api | **абстрактные** декларативные эндпоинты; маршруты регистрирует генератор по закрытым наследникам |

### Архитектурные приёмы абстрактности

- **Нельзя `new` абстрактную сущность** в generic-handler → фабрика `INoteFactory`.
- **`[Export]` source-gen работает только по закрытым типам** → открытые generic-handler'ы
  регистрируются вручную в `AddNotesInfrastructure<>` / `AddNotesApplication<>`.
- **Абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`, design-time factory и
  миграции принадлежат наследнику (`Notes.Default` — готовый пример).
- **Генератор эндпоинтов игнорирует абстрактные классы** → шаблоны живут в `Api`, а маршруты
  появляются в сборке, где объявлены их закрытые наследники (`Notes.Default` или хост).
- **События публикуются строго после `SaveChangesAsync`**, затем `ClearDomainEvents()`.
- **Списки не читаются через `IRepository.GetAllAsync`** — он материализует весь результат
  спецификации в память. Для страниц есть порт `INoteReader<TNote>`: реализация в Infrastructure
  применяет ту же доменную спецификацию, но сортирует и режет на стороне БД.
- **Ошибки на границе:** «не найдена» → `EntityNotFoundException` (404), нарушение правил →
  `NoteValidationException : CrmException` (400). Оба разбирает глобальный
  `ValidationExceptionHandler`; обычный `Exception` дал бы 500.

## API (`api/notes`)

| Метод | Маршрут | Назначение |
|---|---|---|
| `POST` | `/api/notes` | создать заметку → `201` + `{ id }` |
| `GET` | `/api/notes/{id:guid}` | заметка по id (`404`, если нет/удалена) |
| `GET` | `/api/notes?entityType=&entityId=[&pinnedOnly=][&skip=][&take=]` | заметки сущности: закреплённые первыми, затем новые сверху (параметры страницы необязательны) |
| `GET` | `/api/notes/{id:guid}/replies[?skip=][&take=]` | ветка треда (старые сверху), постранично |
| `PUT` | `/api/notes/{id:guid}` | правка тела, упоминаний и вложений (`null` = «не трогать») |
| `POST` | `/api/notes/{id:guid}/pin` \| `/unpin` | закрепление |
| `DELETE` | `/api/notes/{id:guid}` | soft-delete |

## Подключение «из коробки»

```csharp
[DependsOn(typeof(CheetahNotesDefaultModule))]
public partial class MyAppModule : CrmModule;
```

`CheetahNotesDefaultModule` сам поднимает DbContext, репозиторий, фабрику/проектор, CQRS-handler'ы и
маршруты. Нужна строка подключения `Notes` и применение миграции `InitialNotes`.

## Расширение своими полями

```csharp
// 1. Сущность
public sealed class Note : NoteBase
{
    public string? Visibility { get; private set; }
    private Note() { }
    public static Note Create(CreateNoteRequest r)
    {
        var n = new Note();
        n.InitializeCore(Guid.NewGuid(), r.EntityType, r.EntityId, r.AuthorId, r.Body,
            r.Mentions, r.AttachmentFileIds, r.ParentNoteId);
        n.Visibility = r.Visibility;
        return n;
    }
}

// 2. Contracts
public sealed record CreateNoteRequest : CreateNoteRequestBase { public string? Visibility { get; init; } }
public sealed record UpdateNoteRequest : UpdateNoteRequestBase;
public sealed record NoteDto : NoteDtoBase { public string? Visibility { get; init; } }
// + GetNoteByIdRequest, GetNotesByEntityRequest, GetNoteRepliesRequest, Pin/Unpin/RemoveNoteRequest

// 3. Фабрика + проектор
public sealed class NoteFactory : INoteFactory<Note, CreateNoteRequest>
{ public Note Create(CreateNoteRequest r) => Note.Create(r); }

public sealed class NoteProjector : INoteProjector<Note, NoteDto> { /* ToDto */ }

// 4. Схема
public sealed class NoteConfiguration : NoteConfigurationBase<Note>
{
    protected override void ConfigureCustom(EntityTypeBuilder<Note> b)
        => b.Property(x => x.Visibility).HasMaxLength(32);
}

public sealed class AppNotesDbContext : NotesDbContextBase<AppNotesDbContext, Note>
{
    public AppNotesDbContext(DbContextOptions<AppNotesDbContext> o) : base(o) { }
    protected override IEntityTypeConfiguration<Note> CreateNoteConfiguration() => new NoteConfiguration();
}

// 5. Эндпоинты (закрывают шаблоны — генератор зарегистрирует маршруты)
public sealed class CreateNoteEndpoint
    : Api.Endpoints.CreateNoteEndpoint<CreateNoteRequest, CreateNoteCommand<CreateNoteRequest>>;

// 6. Регистрация
services.AddNotesInfrastructure<AppNotesDbContext, Note>();
services.AddNotesApplication<Note, CreateNoteRequest, UpdateNoteRequest, NoteDto, NoteFactory, NoteProjector>();
```

Плюс Mapster-профиль `Request → Command/Query` (образец — `Notes.Default/Mapping/NotesMappingProfile.cs`)
и своя миграция.

## Client

```csharp
services.AddNotesClient<CreateNoteRequest, UpdateNoteRequest, NoteDto>();
```

Опции — секция `Notes:Client` (`BaseUrl` обязателен, `Timeout` по умолчанию 5 с), валидируются на
старте. Аутентификация server-to-server — `Cheetah.Backend.ServiceAuth` (M2M) либо
`UserTokenForwardingHandler` (on-behalf-of).

## События

`NoteCreatedIntegrationEvent`, `NoteUpdatedIntegrationEvent`, `NotePinnedIntegrationEvent`,
`NoteUnpinnedIntegrationEvent`, `NoteRemovedIntegrationEvent`, `UserMentionedIntegrationEvent`
(по одному на **каждое новое** упоминание; при `Edit` — только для добавившихся). Pin/unpin
публикуются только при фактической смене состояния. Потребители зависят только от сборки
`Notes.DomainEvents`.

## Известные ограничения

- **Авторизации нет.** Кто угодно может править/удалять чужую заметку — закрывается позже через
  модуль Permissions (проверка «автор либо право `notes.manage`»). `AuthorId` приходит в запросе;
  после подключения Permissions он будет браться из `ICurrentUser` без смены сигнатур команд.
- **Видимость заметки** (`private`/`team`/`public`) базовым модулем не фиксируется — это поле
  наследника.
- `AttachmentFileIds` — просто `Guid[]`; целостность с файловым хранилищем не проверяется.
- **Маршруты `api/notes` совпадают с `NotesTimeline`** — включать оба модуля в одном хосте нельзя:
  имена эндпоинтов разведены префиксом `Notes…`, но пути дублируются, и каждый запрос к `api/notes`
  упадёт с `AmbiguousMatchException` (500). Если понадобится сосуществование, переопределите `Route`
  в наследниках шаблонов эндпоинтов (например на `api/entity-notes`).
- Треды **плоские**: ответ на ответ отвергается (`NoteValidationException`), родитель обязан
  существовать и принадлежать той же сущности.
