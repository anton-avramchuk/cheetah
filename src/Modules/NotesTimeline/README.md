# Cheetah.Modules.NotesTimeline — расширяемый шаблон «Заметки и хронология»

Модуль из двух под-доменов вокруг произвольной сущности CRM (`(EntityType, EntityId)`):

- **Notes** — заметки/комментарии (write-model), расширяемые полями приложения **по образцу Activities**;
- **Timeline** — материализованная лента истории (read-model), расширяемая **видами событий** через
  реестр проекторов.

Полное проектное описание — [`docs/modules/notes-timeline.md`](../../../docs/modules/notes-timeline.md).

## Состав (9 проектов)

`DomainEvents` → `Shared` → `Contracts` → `Domain` → `Infrastructure` → `Application` → `Api`
(+ `Domain.Tests`, `Application.Tests`). Конкретный `DbContext`, миграции и `…Default`-набор —
у наследника/хоста.

## Как расширить Notes (структурно, как Activities)

Наследник дописывает ~6 типов и одну строку регистрации:

```csharp
// 1. Сущность со своими полями
public sealed class Note : NoteBase
{
    public string? Visibility { get; private set; }
    private Note() { }
    public static Note Create(CreateNoteRequest r)
    {
        var n = new Note();
        n.InitializeCore(Guid.NewGuid(), r.EntityType, r.EntityId, r.AuthorId, r.Body,
            r.Mentions, r.AttachmentFileIds, r.ParentNoteId);
        return n;
    }
}

// 2. Contracts наследника
public sealed record CreateNoteRequest : CreateNoteRequestBase { public string? Visibility { get; init; } }
public sealed record UpdateNoteRequest : UpdateNoteRequestBase;
public sealed record NoteDto : NoteDtoBase { public string? Visibility { get; init; } }

// 3. Фабрика + проектор
public sealed class NoteFactory : INoteFactory<Note, CreateNoteRequest> { /* … */ }
public sealed class NoteProjector : INoteProjector<Note, NoteDto> { /* … */ }

// 4. EF-конфигурация (доп. колонки через ConfigureCustom)
public sealed class NoteConfiguration : NoteConfigurationBase<Note>
{
    protected override void ConfigureCustom(EntityTypeBuilder<Note> b)
        => b.Property("Visibility").HasMaxLength(32);
}

// 5. DbContext наследника
public sealed class NotesDbContext : NotesTimelineDbContextBase<NotesDbContext, Note>
{
    public NotesDbContext(DbContextOptions<NotesDbContext> o) : base(o) { }
    protected override IEntityTypeConfiguration<Note> CreateNoteConfiguration() => new NoteConfiguration();
}

// 6. Регистрация (инфраструктура + прикладной слой)
services.AddNotesTimelineInfrastructure<NotesDbContext, Note>();
services.AddNotesApplication<Note, CreateNoteRequest, UpdateNoteRequest, NoteDto, NoteFactory, NoteProjector>();

// 7. Api-модуль
public sealed class NoteEndpoints : NoteEndpointsBase<CreateNoteRequest, UpdateNoteRequest, NoteDto> { }
public sealed class AppNotesTimelineApiModule
    : CheetahNotesTimelineApiModuleBase<NoteEndpoints, CreateNoteRequest, UpdateNoteRequest, NoteDto> { }
```

## Как расширить Timeline (динамически, как Tags-registry)

Модуль-источник учит ленту новому «виду», регистрируя проектор своего события:

```csharp
public sealed class DealStageChangedTimelineProjector : ITimelineProjector<DealStageChangedIntegrationEvent>
{
    public string Kind => "deals.stage-changed";
    public bool CanProject(DealStageChangedIntegrationEvent e) => true;
    public TimelineEntry Project(DealStageChangedIntegrationEvent e)
        => TimelineEntry.Create("crm.deal", e.DealId, Kind, "Сделка сменила стадию",
            DateTimeOffset.UtcNow, sourceEventId: e.EventId.ToString());
}

services.AddTimelineProjector<DealStageChangedIntegrationEvent, DealStageChangedTimelineProjector>();
```

`TimelineProjectionService<TEvent>` прогоняет проекторы и пишет строку идемпотентно через
`ITimelineWriter` (анти-дубль по `SourceEventId`). Подписку проекторов на шину наследник/хост
выполняет в `OnApplicationInitialization` (follow-up — авто-подписка генератором).

## Ключевые точки

- **Soft-delete** заметок (`RemovedAt`, query-filter скрывает по умолчанию).
- **Упоминания/вложения** — `Guid`-коллекции в `jsonb`; упоминания публикуют
  `UserMentionedIntegrationEvent` (только новые при `Edit`).
- **Лента append-only**, курсорная пагинация по `OccurredAt`.
- Фильтрация — только через спецификации; чтение ленты сортирует/пагинирует в памяти (MVP).
