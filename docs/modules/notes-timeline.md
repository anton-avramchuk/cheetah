# Cheetah.Modules.NotesTimeline.* — модуль «Заметки и хронология» (расширяемый шаблон)

> Статус: **реализовано (MVP абстрактного шаблона).** Код — в `src/Modules/NotesTimeline/`. 9 проектов
> (7 шаблонных + 2 тестовых) собираются; тесты зелёные: Domain (11), Application (14) = 25. Конкретные
> типы (`Note`/`NoteDto`/фабрика/проектор) живут в тестах — отдельный `…Default`-проект и живая подписка
> проекторов на шину вынесены в follow-up (как фоновые задачи в Activities). Краткий гайд по расширению —
> `src/Modules/NotesTimeline/README.md`.
> Документ — пошаговый план сборки модуля по канону `CLAUDE.md`
> (Events → Shared → Contracts → Domain → Infrastructure → Application → Api (+ Client)),
> своя БД PostgreSQL, общение через REST/gRPC + события через шину.
>
> Источник: раздел [§6](../plans.md) общего плана. Это **п.5 рекомендуемого порядка реализации**
> (после коммерческого контура Catalog → Sales Documents), «слой вовлечённости поверх уже
> существующих событий».
>
> **Tier 2.** Два связанных сервиса вокруг произвольной сущности CRM (`EntityType` + `EntityId`):
> **Notes** — текстовые заметки/комментарии пользователей; **Timeline** — единая хронология того, что
> происходило с сущностью, материализуемая из доменных/интеграционных событий и `Cheetah.Audit`.

---

## 0. Главное требование — расширяемость

> **Модуль обязан быть расширяемым: сущность заметки и её ViewModel должны допускать расширение полями
> приложения-наследника без форка модуля; набор «видов событий» хронологии должен пополняться
> потребителями без правок модуля.**

Поэтому NotesTimeline строится как **абстрактный шаблон-модуль** по образцу уже реализованных
[`Cheetah.Modules.Activities`](../../src/Modules/Activities/README.md) и
[`Cheetah.Modules.Customer`](../../src/Modules/Customer/README.md): модуль поставляет **только
абстрактные базовые типы и generic-хелперы**, а наследник дописывает свои `sealed`-типы и поля.

У модуля **две независимые точки расширяемости** — по числу под-доменов:

| Под-домен | Природа | Механизм расширения |
|---|---|---|
| **Notes** (write-model) | заметка часто требует доп. полей (категория заметки, видимость, источник) | **структурная (compile-time)** — `NoteBase`/`NoteDtoBase` + фабрика/проектор наследника, как в Activities |
| **Timeline** (read-model) | состав «видов событий» зависит от того, какие модули подключены | **динамическая (runtime)** — **реестр рендереров** `ITimelineProjector`, потребители декларируют свои виды (паттерн Tags-registry) |

**Что это даёт наследнику Notes:**

- `sealed class Note : NoteBase` со своими полями (например, `Visibility`, `Category`);
- `sealed record NoteDto : NoteDtoBase` со своими полями в ответе API;
- свои `Create/Update`-Request-ы (наследники `…RequestBase`);
- рабочий CRUD заметок, упоминания, пин — «из коробки», дописав ~6–7 классов.

**Что это даёт потребителю Timeline:**

- регистрирует `ITimelineProjector` для своего интеграционного события (`DealStageChanged`,
  `InvoicePaid`, …) — модуль сам подпишется на шину и материализует строку ленты;
- не нужно править NotesTimeline, чтобы научить ленту новому «виду».

**Три уровня расширяемости** (закрываются на этапе проектирования, см. §4):

1. **Структурная (compile-time).** Наследование `NoteBase`/`NoteDtoBase` + `ConfigureCustom`-hook в
   EF-конфигурации + фабрика/проектор наследника. Сильная типизация, индексы по доп. полям.
2. **Динамическая (runtime, без миграций).** Полиморфная привязка `(EntityType, EntityId)` делает
   модуль предметно-нейтральным. Реестр `ITimelineProjector`/`TimelineKindDescriptor` пополняется
   потребителями. `Payload (jsonb)` строки ленты хранит произвольную нагрузку вида без миграций.
   Доп. атрибуты заметки без схемы — через будущий [Custom Fields](../plans.md) или опц. `Attributes (jsonb)`.
3. **Поведенческая.** `virtual`-мутаторы `NoteBase`, `virtual`-методы эндпоинтов, политика видимости
   как точка переопределения.

> **Почему два разных механизма.** Заметка — это write-model с инвариантами (автор, тело, упоминания),
> её расширяют полями — поэтому абстрактные базы, как в Activities. Хронология — это read-model,
> которую наполняют **другие** модули; её расширяют не полями, а новыми «видами событий» — поэтому
> открытый реестр рендереров, как реестр применимых типов в Tags.

---

## 1. Назначение и границы

**Что делает (Notes):** хранит текстовые заметки/комментарии пользователей, привязанные к
`(EntityType, EntityId)`, с упоминаниями (`@user` → `Mentions[]`), вложениями (`AttachmentFileIds[]`
через `Cheetah.FileStorage`) и закреплением (`PinnedAt`).

**Что делает (Timeline):** строит и отдаёт агрегированную хронологию сущности — что с ней
происходило (создана, сменила стадию, добавлена активность, отправлен документ, написана заметка).
Лента — **материализованная проекция (read-model)**, наполняемая подписками на интеграционные события
других модулей и записями `Cheetah.Audit`.

**Чего НЕ делает:**

- не источник истины по доменным фактам — Timeline всегда eventually consistent относительно модулей-
  источников; при расхождении истина в модуле-владельце события;
- не шлёт уведомления сам — на `@mention` публикует `UserMentionedIntegrationEvent`, доставку делает
  Notification;
- не хранит файлы вложений — только их `FileId` (загрузка/выдача — `Cheetah.FileStorage`);
- не управляет пользователями — `AuthorId`/`ActorId`/`Mentions[]` это логические ссылки на Identity
  (без FK через границу модуля).

**Связи (по `Id`, без FK через границу модуля):** `AuthorId`, `Mentions[]`, `ActorId`,
`AttachmentFileIds[]`, `(EntityType, EntityId)`.

### 1.1. Решения, которые фиксируем

| Вопрос | Решение |
|---|---|
| Форма модуля | **абстрактный шаблон** (как Activities/Customer) — расширяемая сущность `Note` + реестр Timeline |
| Идентификатор | `Guid` |
| Хранилище | PostgreSQL (EF Core); миграции — **у наследника/хоста** |
| Привязка к сущности | полиморфная `(EntityType: string, EntityId: Guid)` — единая конвенция с Tags/Activities/CustomFields |
| Timeline | **материализованная проекция** (read-model), а не сборка из Audit на лету (рекомендация плана §6.4) |
| Реестр видов ленты | **регистрируемый потребителями** `ITimelineProjector` (паттерн Tags), а не жёсткий список в модуле |
| Упоминания | `Mentions[]` (Guid пользователей) на `NoteBase`; публикуют `UserMentionedIntegrationEvent` |
| Вложения | `AttachmentFileIds[]` (Guid из `Cheetah.FileStorage`); модуль хранит только ссылки |
| Пагинация ленты | **курсорная** (по `OccurredAt` + `Id`), не offset — лента длинная и растёт во времени |
| Эндпоинты | декларативные (`Cheetah.Backend.Endpoints` + генератор), грид заметок через `IGridRepository` |
| События-источники Timeline | подписка через шину; идемпотентность — `Cheetah.Core.Inbox` (одно событие — одна строка) |
| Soft-delete заметок | `IRemovedAtEntity`: `Remove()` ставит `RemovedAt` (как Customer/Activities) |

### 1.2. Открытые вопросы (зафиксировать до/во время реализации)

1. **Один модуль или два.** Notes и Timeline — связаны, но разделимы. Рекомендация: **один модуль
   `NotesTimeline` с двумя под-доменами** в общих сборках (общая привязка `(EntityType, EntityId)`,
   общая БД), но раздельные агрегаты/эндпоинты. Альтернатива — два модуля; на MVP избыточно. Решение — §3.
2. **Поставлять ли «готовую» реализацию Notes по умолчанию.** По образцу Activities — **гибрид**:
   отдельная сборка `…Default` с `sealed Note`, конкретными Contracts, `DbContext` и миграциями, чтобы
   модуль работал «из коробки», оставаясь расширяемым. Решение — §3.1.
3. **Timeline: материализовать vs собирать из Audit.** Зафиксировано — **материализовать** (быстрое
   чтение карточки сущности). Audit остаётся вторичным источником для бэкофилла/реиндексации.
4. **Реестр видов событий: jsonb-`Payload` vs типизированные строки.** На MVP — `Payload (jsonb)` +
   `Kind`-дискриминатор, рендер на фронте/в проекторе. Типизация — точка расширения позже.
5. **`EntityDeletedIntegrationEvent`** — общий контракт (сквозное решение плана №2) vs per-module.
   Нужен для очистки заметок и сворачивания ленты удалённой сущности.
6. **Право видеть заметку/строку ленты** — фильтрация через `Cheetah.Permissions` на запросе vs
   пост-фильтр. На MVP — на запросе (видимость по владельцу/привязанной сущности).
7. **Реакции/треды на заметках** (ответы, лайки) — вне MVP; `ParentNoteId?` зарезервировать в схеме.

---

## 2. Архитектурная роль

```
   ┌──────────────────────────────────────────────────────────────┐
   │                 NotesTimeline Module (шаблон)                │
   │                                                              │
   │  Write-model (Notes)            Read-model (Timeline)         │
   │  ┌────────────────────┐         ┌──────────────────────────┐ │
   │  │  NoteBase (abstract)│        │  TimelineEntry           │ │
   │  │  AuthorId, Body,    │        │  (Kind, Title, Payload,  │ │
   │  │  Mentions[],        │        │   ActorId, OccurredAt)   │ │
   │  │  AttachmentFileIds[],│       └─────────▲────────────────┘ │
   │  │  PinnedAt           │                  │ материализует     │
   │  └─────────┬───────────┘                  │                   │
   │            │ наследует           ┌─────────┴────────────────┐ │
   │  sealed Note + NoteDto (поля)    │ Реестр ITimelineProjector│ │
   │                                  │ (потребители регистрируют│ │
   │                                  │  свои виды событий)      │ │
   └────────────┼─────────────────────────────▲──────────────────┘
                │ publish (Outbox)             │ subscribe (Inbox, идемпотентно)
                ▼                              │
   NoteCreated / UserMentioned        DealStageChanged · ActivityCompleted ·
                │                      InvoicePaid · DocumentSent · NoteCreated · …
                ├──▶ Notification (UserMentioned → письмо/пуш)         (события др. модулей + Audit)
                ├──▶ Timeline (NoteCreated → строка ленты)
                └──◀ EntityDeleted (очистка заметок + сворачивание ленты)
```

Timeline — **и потребитель, и владелец read-model**: он подписан на широкий набор интеграционных
событий шины, и для каждого зарегистрированного `Kind` создаёт `TimelineEntry`. Notes — обычный
write-model-агрегат, публикующий свои события (которые в т.ч. потребляет сам Timeline).

---

## 3. Структура проектов

Стандартные 8 сборок канона + тесты, в `/src/Modules/NotesTimeline/`:

```
src/Modules/NotesTimeline/
├── Cheetah.Modules.NotesTimeline.DomainEvents/   # NoteCreated/Updated/Pinned/Removed, UserMentioned
├── Cheetah.Modules.NotesTimeline.Shared/          # enums (TimelineEntryKind?), EntityRefKeys, конвенции
├── Cheetah.Modules.NotesTimeline.Contracts/       # NoteDtoBase, TimelineEntryDto, Create/Update requests, дескрипторы
├── Cheetah.Modules.NotesTimeline.Domain/          # NoteBase, TimelineEntry, спецификации, реестр-порт
├── Cheetah.Modules.NotesTimeline.Infrastructure/  # EF Core, конфиги, подписки-проекторы, Audit-адаптер
├── Cheetah.Modules.NotesTimeline.Application/      # generic CQRS (Notes) + сервис записи ленты + проекторы
├── Cheetah.Modules.NotesTimeline.Api/             # декларативные эндпоинты (notes CRUD + timeline read)
├── Cheetah.Modules.NotesTimeline.Client/          # реестр-клиент: регистрация видов + (опц.) удалённое чтение
└── Tests/
    ├── Cheetah.Modules.NotesTimeline.Domain.Tests/
    ├── Cheetah.Modules.NotesTimeline.Application.Tests/
    └── Cheetah.Modules.NotesTimeline.Client.Tests/
```

> **Client нужен**, в отличие от Catalog: потребители (другие .NET-модули) регистрируют свои
> `TimelineKindDescriptor` при старте через Client — ровно как `TagsRegistrationSyncService`.

### 3.1. Решение по «готовой реализации»

По образцу Activities — **гибрид**: помимо абстрактного шаблона поставляем
`Cheetah.Modules.NotesTimeline.Default` с `sealed Note : NoteBase`, конкретными
`NoteDto`/`Create…Request`, `NotesTimelineDbContext` и миграциями. Хост может:

- подключить `…Default` → заметки и лента работают «из коробки»;
- **или** не подключать Default, объявить свой `sealed Note` со своими полями и собрать свой набор.

Timeline-часть в Default уже регистрирует базовые проекторы для известных событий ядра
(`NoteCreated`); проекторы доменных модулей (Deals/Activities/SalesDocuments) регистрируют **сами эти
модули** через Client при старте (см. §4.5).

---

## 4. Доменная модель

### 4.1. Сводка

| Агрегат / сущность | Под-домен | Назначение | Природа |
|---|---|---|---|
| **NoteBase** (AggregateRoot, abstract) | Notes | заметка/комментарий к сущности | расширяемая база (наследник → `sealed Note`) |
| **TimelineEntry** (Entity, read-model) | Timeline | строка хронологии сущности | конкретная (расширяется не полями, а видами) |

### 4.2. Shared — enums и конвенции

```csharp
namespace Cheetah.Modules.NotesTimeline.Shared;

// Привязка к произвольной сущности CRM — общая конвенция с Tags/Activities/CustomFields.
public static class EntityRefKeys
{
    public const string Deal = "crm.deal";
    public const string Customer = "crm.customer";
    public const string Contact = "crm.contact";
    public const string Lead = "crm.lead";
    public const string Invoice = "crm.invoice";
}

// «Виды» строк ленты — НЕ закрытый enum, а строковые ключи "{service}.{event}" (стабильный контракт,
// как featureKey/permissionKey). Здесь — лишь известные значения ядра; потребители добавляют свои.
public static class TimelineKinds
{
    public const string NoteAdded        = "notes.note-added";
    public const string DealStageChanged = "deals.stage-changed";
    public const string ActivityDone     = "activities.completed";
    public const string DocumentSent     = "sales.document-sent";
    public const string InvoicePaid      = "sales.invoice-paid";
}

public enum NoteRemovalMode { SoftDelete, Hard }   // политика удаления (наследник выбирает)
```

### 4.3. `NoteBase` — точки расширения

```csharp
public abstract class NoteBase : AggregateRoot<Guid>,
    ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    private readonly List<Guid> _mentions = new();
    private readonly List<Guid> _attachmentFileIds = new();

    public string EntityType { get; private set; } = null!;   // полиморфная привязка
    public Guid EntityId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Body { get; private set; } = null!;
    public IReadOnlyList<Guid> Mentions => _mentions;
    public IReadOnlyList<Guid> AttachmentFileIds => _attachmentFileIds;
    public Guid? ParentNoteId { get; private set; }           // зарезервировано под треды (вне MVP)
    public DateTimeOffset? PinnedAt { get; private set; }
    public string? Attributes { get; private set; }           // «быстрый» карман расширения (jsonb)

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }

    protected NoteBase() { }   // EF + наследник

    /// <summary>Заводит инварианты новой заметки + событие создания. Вызывает фабрика наследника.</summary>
    protected void InitializeCore(Guid id, string entityType, Guid entityId, Guid authorId,
        string body, IEnumerable<Guid>? mentions = null, IEnumerable<Guid>? attachmentFileIds = null,
        Guid? parentNoteId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        Id = id;
        EntityType = entityType.Trim();
        EntityId = entityId;
        AuthorId = authorId;
        Body = body.Trim();
        ParentNoteId = parentNoteId;
        if (mentions is not null) _mentions.AddRange(mentions.Distinct());
        if (attachmentFileIds is not null) _attachmentFileIds.AddRange(attachmentFileIds.Distinct());

        AddDomainEvent(new NoteCreatedIntegrationEvent(Id, EntityType, EntityId, AuthorId));
        RaiseMentions();   // по одному UserMentionedIntegrationEvent на упомянутого
    }

    public virtual void Edit(string body, IEnumerable<Guid>? mentions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        Body = body.Trim();
        var before = _mentions.ToHashSet();
        _mentions.Clear();
        if (mentions is not null) _mentions.AddRange(mentions.Distinct());
        AddDomainEvent(new NoteUpdatedIntegrationEvent(Id));
        foreach (var m in _mentions.Where(m => !before.Contains(m)))   // только НОВЫЕ упоминания
            AddDomainEvent(new UserMentionedIntegrationEvent(Id, EntityType, EntityId, m, AuthorId));
    }

    public void Pin() { if (PinnedAt is null) PinnedAt = DateTimeOffset.UtcNow; }
    public void Unpin() => PinnedAt = null;
    public void Remove() { if (RemovedAt is null) { RemovedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new NoteRemovedIntegrationEvent(Id, EntityType, EntityId)); } }
    public void SetAttributes(string? attributes) => Attributes = attributes;

    private void RaiseMentions()
    {
        foreach (var m in _mentions)
            AddDomainEvent(new UserMentionedIntegrationEvent(Id, EntityType, EntityId, m, AuthorId));
    }
}
```

> **Точки расширения:** наследник объявляет `sealed class Note : NoteBase` + свою фабрику (вызовет
> `InitializeCore`), переопределяет `Edit` для доп. полей, добавляет колонки через `ConfigureCustom` в
> EF-конфигурации. Mentions/attachments — коллекции в агрегате (value-конвертация в EF, §7).

### 4.4. `TimelineEntry` — read-model (конкретная сущность)

```csharp
public sealed class TimelineEntry : Entity<Guid>
{
    public string EntityType { get; private set; } = null!;   // к какой сущности относится строка
    public Guid EntityId { get; private set; }
    public string Kind { get; private set; } = null!;          // "deals.stage-changed" и т.п. (TimelineKinds)
    public string Title { get; private set; } = null!;         // готовая краткая строка для карточки
    public string? Payload { get; private set; }               // jsonb: произвольная нагрузка вида
    public Guid? ActorId { get; private set; }                 // кто инициировал (Identity), null = система
    public DateTimeOffset OccurredAt { get; private set; }     // время факта (из события), не вставки
    public string? SourceEventId { get; private set; }         // для идемпотентности (Inbox) и дедупликации

    private TimelineEntry() { }

    public static TimelineEntry Create(string entityType, Guid entityId, string kind, string title,
        DateTimeOffset occurredAt, Guid? actorId = null, string? payload = null, string? sourceEventId = null)
        => new()
        {
            Id = Guid.NewGuid(), EntityType = entityType, EntityId = entityId, Kind = kind,
            Title = title, OccurredAt = occurredAt, ActorId = actorId, Payload = payload,
            SourceEventId = sourceEventId
        };
}
```

> Строка ленты **неизменяема** (append-only): хронология — журнал фактов. Правки/удаления родительской
> сущности добавляют новые строки (`*-updated`, `*-removed`), а не редактируют старые.

### 4.5. Реестр видов событий — точка расширяемости Timeline

Потребитель учит ленту новому виду, реализовав **проектор события → строка** и зарегистрировав его.
Это и есть динамическая расширяемость Timeline (паттерн Tags-registry).

```csharp
// Domain/Application — порт проектора конкретного интеграционного события в строку ленты.
public interface ITimelineProjector<in TEvent> where TEvent : EventBase
{
    string Kind { get; }                                  // TimelineKinds.* — стабильный ключ вида
    bool CanProject(TEvent @event);                       // напр. только DealWon, не любой StageChanged
    TimelineEntry Project(TEvent @event);                 // материализация строки
}

// Contracts — дескриптор вида для регистрации в каталоге (как TagsTypeDescriptor/FeatureDescriptor).
public sealed record TimelineKindDescriptor(string Kind, string OwnerService, string TitleTemplate);
```

Регистрация потребителем при старте (как `TagsRegistrationSyncService`):

```csharp
// в bootstrap модуля Deals
services.AddTimelineProjectors(reg =>
{
    reg.Add<DealStageChangedIntegrationEvent>(TimelineKinds.DealStageChanged,
        e => TimelineEntry.Create(EntityRefKeys.Deal, e.DealId, TimelineKinds.DealStageChanged,
            title: "Сделка сменила стадию", occurredAt: e.OccurredAt, sourceEventId: e.EventId.ToString()));
});
```

Инфраструктура Timeline (§7) подписывает зарегистрированные проекторы на шину и пишет строки
идемпотентно (`Cheetah.Core.Inbox` по `SourceEventId`).

### 4.6. Domain — спецификации (фильтрация только через них)

```csharp
// Notes
public sealed class NotesByEntitySpecification(string entityType, Guid entityId) : Specification<NoteBase> {
    public override Expression<Func<NoteBase, bool>> ToExpression()
        => n => n.EntityType == entityType && n.EntityId == entityId && n.RemovedAt == null;
}
public sealed class PinnedNotesSpecification(string entityType, Guid entityId) : Specification<NoteBase> { /* + PinnedAt != null */ }

// Timeline (курсорная пагинация: строки старше курсора)
public sealed class TimelineByEntitySpecification(string entityType, Guid entityId,
    DateTimeOffset? before, IReadOnlyCollection<string>? kinds) : Specification<TimelineEntry> {
    public override Expression<Func<TimelineEntry, bool>> ToExpression()
        => e => e.EntityType == entityType && e.EntityId == entityId
             && (before == null || e.OccurredAt < before)
             && (kinds == null || kinds.Count == 0 || kinds.Contains(e.Kind));
}
public sealed class TimelineEntryBySourceEventSpecification(string sourceEventId) : Specification<TimelineEntry> { /* идемпотентность */ }
```

---

## 5. Contracts — расширяемые ViewModel

```csharp
// Notes — абстрактная база ViewModel (граница API), как ActivityDtoBase.
public abstract record NoteDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public Guid AuthorId { get; init; }
    public string Body { get; init; } = null!;
    public IReadOnlyList<Guid> Mentions { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<Guid> AttachmentFileIds { get; init; } = Array.Empty<Guid>();
    public DateTimeOffset? PinnedAt { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public abstract record CreateNoteRequestBase
{
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public string Body { get; init; } = null!;
    public IReadOnlyList<Guid>? Mentions { get; init; }
    public IReadOnlyList<Guid>? AttachmentFileIds { get; init; }
}
public abstract record UpdateNoteRequestBase { public string Body { get; init; } = null!;
    public IReadOnlyList<Guid>? Mentions { get; init; } }

// Timeline — конкретный DTO (read-model не расширяют полями; гибкость — в Payload).
public sealed record TimelineEntryDto : ICrmResponse
{
    public Guid Id { get; init; }
    public string Kind { get; init; } = null!;
    public string Title { get; init; } = null!;
    public JsonElement? Payload { get; init; }
    public Guid? ActorId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}
public sealed record TimelinePageDto(IReadOnlyList<TimelineEntryDto> Items, DateTimeOffset? NextCursor);
```

---

## 6. Application — generic CQRS + фабрика/проектор

**Notes** — generic-хендлеры, закрываемые конкретными типами наследника (точно как Activities §6):

```csharp
public interface INoteFactory<out TNote, in TCreateRequest>
    where TNote : NoteBase where TCreateRequest : CreateNoteRequestBase { TNote Create(TCreateRequest request); }

public interface INoteProjector<in TNote, out TDto>
    where TNote : NoteBase where TDto : NoteDtoBase { TDto ToDto(TNote note); }

// Команды/запросы (generic по Request/Dto)
public record CreateNoteCommand<TReq>(TReq Request) : ICommand<Guid> where TReq : CreateNoteRequestBase;
public record UpdateNoteCommand<TReq>(Guid Id, TReq Request) : ICommand where TReq : UpdateNoteRequestBase;
public record PinNoteCommand(Guid Id) : ICommand;
public record UnpinNoteCommand(Guid Id) : ICommand;
public record RemoveNoteCommand(Guid Id) : ICommand;
public record GetNotesByEntityQuery<TDto>(string EntityType, Guid EntityId) : IQuery<IReadOnlyList<TDto>> where TDto : NoteDtoBase;
```

Регистрация — extension по образцу `AddActivitiesApplication<…>`:

```csharp
services.AddNotesApplication<TNote, TCreateRequest, TUpdateRequest, TDto, TFactory, TProjector>();
// внутри: AddScoped фабрики/проектора + закрытые generic CommandHandler/QueryHandler.
```

Хендлеры публикуют доменные события заметки **после** `SaveChangesAsync` (Outbox).

**Timeline** — не CQRS-расширяемый, а **сервис записи ленты** + чтение:

```csharp
// Запись: вызывается подписчиком шины (§7) для зарегистрированного проектора.
public interface ITimelineWriter {
    ValueTask AppendAsync(TimelineEntry entry, CancellationToken ct);   // идемпотентно по SourceEventId
}

// Чтение: курсорная страница.
public record GetTimelineQuery(string EntityType, Guid EntityId,
    DateTimeOffset? Before, IReadOnlyCollection<string>? Kinds, int Limit) : IQuery<TimelinePageDto>;
```

---

## 7. Infrastructure — EF Core + подписки-проекторы + Audit

### 7.1. Абстрактные базы конфигураций (расширяемая схема заметки)

```csharp
public abstract class NoteConfigurationBase<TNote> : IEntityTypeConfiguration<TNote> where TNote : NoteBase
{
    public void Configure(EntityTypeBuilder<TNote> b)
    {
        b.ToTable("Notes");
        b.HasKey(x => x.Id);
        b.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        b.Property(x => x.Body).IsRequired();
        // Mentions/AttachmentFileIds — Guid[] → jsonb (или отдельная таблица; для MVP jsonb)
        b.Property(x => x.Mentions).HasColumnType("jsonb").HasConversion(/* Guid[] <-> json */);
        b.Property(x => x.AttachmentFileIds).HasColumnType("jsonb").HasConversion(/* … */);
        b.Property(x => x.Attributes).HasColumnType("jsonb");
        b.HasIndex(x => new { x.EntityType, x.EntityId });
        b.HasIndex(x => x.PinnedAt);
        b.HasQueryFilter(x => x.RemovedAt == null);   // soft-delete по умолчанию скрыт
        b.Ignore(x => x.DomainEvents);                // CRITICAL
        ConfigureCustom(b);                            // hook расширения наследника
    }
    protected virtual void ConfigureCustom(EntityTypeBuilder<TNote> b) { }
}

public sealed class TimelineEntryConfiguration : IEntityTypeConfiguration<TimelineEntry>
{
    public void Configure(EntityTypeBuilder<TimelineEntry> b)
    {
        b.ToTable("TimelineEntries");
        b.Property(x => x.Payload).HasColumnType("jsonb");
        b.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt });   // курсорная пагинация
        b.HasIndex(x => x.SourceEventId).IsUnique();                        // идемпотентность ленты
    }
}
```

### 7.2. Подписки-проекторы (наполнение ленты)

- При инициализации модуль берёт зарегистрированные `ITimelineProjector<TEvent>` и **подписывает** их
  на шину (`IEventBus.Subscribe<TEvent, …>`), как подписки в `OnApplicationInitialization`.
- Обработчик: `CanProject` → `Project` → `ITimelineWriter.AppendAsync` (идемпотентно по
  `SourceEventId` через `Cheetah.Core.Inbox` / уникальный индекс — двойная защита от повторной доставки).
- **Очистка:** подписка на `EntityDeletedIntegrationEvent(EntityType, EntityId)` — soft-remove заметок
  + (опц.) добавление финальной строки `*.entity-removed` в ленту.

### 7.3. Бэкофилл из Audit

`Cheetah.Audit`-адаптер `IAuditTimelineBackfill` — фоновая переиндексация истории сущности из аудита
(`POST /api/timeline/rebuild` для админа через `Cheetah.BackgroundTasks`). Audit — вторичный источник;
основной поток — события в реальном времени (§7.2).

---

## 8. Api (декларативные эндпоинты)

Декларативный стиль `Cheetah.Backend.Endpoints` (как Identity/Catalog): эндпоинты — наследники
`CreateCommandEndpoint`/`UpdateCommandEndpoint`/`DeleteCommandEndpoint`/`QueryOrNotFoundEndpoint`/
`QueryGridEndpoint`; маршруты регистрирует генератор; маппинг Request↔Command, Entity→ViewModel — Mapster;
грид заметок — `IGridRepository`.

**Notes — абстрактные шаблоны эндпоинтов** (заметка расширяема) → закрывает наследник/хост:
`CreateNoteEndpoint<…>`, `UpdateNoteEndpoint<…>`, `GetNotesByEntityEndpoint<…>`, `DeleteNoteEndpoint<…>`.

| Метод | Маршрут | Команда/запрос |
|---|---|---|
| POST | `api/notes` | `CreateNoteCommand<…>` |
| GET | `api/notes?entityType=&entityId=` | `GetNotesByEntityQuery<…>` |
| PUT | `api/notes/{id}` | `UpdateNoteCommand<…>` |
| POST | `api/notes/{id}/pin` · `/unpin` | `PinNoteCommand` / `UnpinNoteCommand` |
| DELETE | `api/notes/{id}` | `RemoveNoteCommand` |

**Timeline — конкретные эндпоинты (read-only, курсорная пагинация):**

| Метод | Маршрут | Запрос |
|---|---|---|
| GET | `api/timeline?entityType=&entityId=&kinds=&before=&limit=` | `GetTimelineQuery` |
| POST | `api/timeline/rebuild?entityType=&entityId=` | бэкофилл из Audit (админ) |

Авторизация — `Cheetah.Permissions`. Вложения загружаются/выдаются отдельными эндпоинтами
`Cheetah.FileStorage`; заметка хранит только `AttachmentFileIds`.

---

## 9. События

**Публикует NotesTimeline:**

```csharp
namespace Cheetah.Modules.NotesTimeline.DomainEvents;

public record NoteCreatedIntegrationEvent(Guid NoteId, string EntityType, Guid EntityId, Guid AuthorId) : EventBase;
public record NoteUpdatedIntegrationEvent(Guid NoteId) : EventBase;
public record NoteRemovedIntegrationEvent(Guid NoteId, string EntityType, Guid EntityId) : EventBase;
public record UserMentionedIntegrationEvent(Guid NoteId, string EntityType, Guid EntityId, Guid MentionedUserId, Guid ByUserId) : EventBase; // → Notification
```

**Потребляет (наполнение Timeline):** `NoteCreated`, и через реестр проекторов — события других
модулей (`DealStageChangedIntegrationEvent`, `ActivityCompletedIntegrationEvent`,
`DocumentSentIntegrationEvent`, `InvoicePaidIntegrationEvent`, …) + `EntityDeletedIntegrationEvent`
(очистка). Потребитель `UserMentioned` — Notification.

---

## 10. Тесты

- **Domain.Tests:** инварианты `NoteBase` (создание заводит `NoteCreated` + по событию на упоминание;
  `Edit` поднимает событие только на **новые** упоминания; `Pin/Unpin`, `Remove` → soft-delete +
  событие); неизменяемость `TimelineEntry`. Тестовый `sealed class TestNote : NoteBase` с доп. полем.
- **Application.Tests:** generic CQRS заметок (create/update/pin/remove, проекция в `TestNoteDto`);
  `ITimelineWriter` — идемпотентность по `SourceEventId` (повторная доставка не дублирует строку);
  проектор события → корректная строка ленты; курсорная пагинация `GetTimelineQuery`.
- **Client.Tests:** регистрация `TimelineKindDescriptor`/проекторов (sync при старте, ретраи,
  `ContinueOnFailure` — не валит хост при недоступности каталога).

---

## 11. План реализации (пошагово)

1. **DomainEvents** — `NoteCreated/Updated/Removed`, `UserMentioned` (без зависимостей, кроме `EventBase`).
2. **Shared** — `EntityRefKeys`, `TimelineKinds`, enums.
3. **Contracts** — `NoteDtoBase`, `Create/UpdateNoteRequestBase`, `TimelineEntryDto`/`TimelinePageDto`,
   `TimelineKindDescriptor`.
4. **Domain** — `NoteBase` (абстракт + `InitializeCore`/мутаторы/упоминания), `TimelineEntry`
   (read-model), спецификации, порт `ITimelineProjector<TEvent>`.
5. **Infrastructure** — `NoteConfigurationBase<TNote>` (jsonb-коллекции, query-filter, `ConfigureCustom`),
   `TimelineEntryConfiguration`, `ITimelineWriter` (идемпотентный), подписчик-диспетчер проекторов,
   Audit-бэкофилл, `EntityDeleted`-очистка.
6. **Application** — фабрика/проектор-порты Notes, generic CQRS-хендлеры, `AddNotesApplication<…>`,
   `GetTimelineQuery`-хендлер, реестр `AddTimelineProjectors`.
7. **Api** — абстрактные эндпоинты Notes (`Create/Update/GetByEntity/Pin/Delete`), конкретные Timeline
   (`GET /api/timeline`, `POST /api/timeline/rebuild`); маппинг-профиль Mapster.
8. **Client** — регистрация `TimelineKindDescriptor`/проекторов при старте (как Tags); (опц.) удалённое чтение.
9. **Default** (гибрид, §3.1) — `sealed Note`, конкретные Contracts, `NotesTimelineDbContext` + миграции,
   базовые проекторы ядра.
10. **Тесты** Domain/Application/Client; добавить все проекты в `Cheetah.slnx` в `/Modules/NotesTimeline/`.
11. **Подписать проекторы из уже готовых модулей** (Deals/Activities/SalesDocuments) — по одному
    `ITimelineProjector` на ключевое событие, чтобы лента наполнялась с первого дня.

---

## 12. Зависимости от инфраструктуры

| Инфраструктурный модуль | Где задействован |
|---|---|
| `Cheetah.Audit` | Timeline — вторичный источник/бэкофилл истории |
| `Cheetah.FileStorage` | Notes — вложения (`AttachmentFileIds`) |
| `Cheetah.Core.Outbox` | надёжная публикация событий заметок |
| `Cheetah.Core.Inbox` | Timeline — идемпотентность материализации (одно событие — одна строка) |
| `Cheetah.Core.Cache` | (опц.) кэш «горячих» лент карточек активных сущностей |
| `Cheetah.Permissions` | видимость заметок и строк ленты |
| `Cheetah.Backend.Endpoints` + генератор | декларативные эндпоинты + грид (`IGridRepository`) |
| `Cheetah.Mapping` (Mapster) | Request↔Command, Entity→ViewModel |
| `Cheetah.BackgroundTasks` | `rebuild` ленты из Audit (админ) |

---

## 13. Отличия от исходного эскиза плана (`docs/plans.md` §6)

| План §6 | Здесь | Почему |
|---|---|---|
| `Note` как конкретный агрегат | `NoteBase` (абстрактная база) + `sealed Note` наследника | требование расширяемости (как Activities/Customer) |
| «материализованная проекция vs Audit на лету» (открытый вопрос) | зафиксировано: **материализованная**, Audit — вторичен/бэкофилл | быстрое чтение карточки (цель 10k RPS) |
| «реестр видов: жёсткий vs регистрируемый» (открытый вопрос) | зафиксировано: **регистрируемый** через `ITimelineProjector`/Client (паттерн Tags) | расширяемость Timeline без форка модуля |
| offset-пагинация ленты не оговорена | **курсорная** (`OccurredAt`+`Id`) | лента append-only и растёт; offset деградирует |
| Mentions/Attachments — массивы в модели | `jsonb`-коллекции на MVP (отд. таблицы — позже) | меньше джойнов на карточке; GIN-индексация |
```
