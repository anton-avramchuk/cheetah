using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Notes.Shared;

namespace Cheetah.Modules.Notes.Contracts;

/// <summary>
/// Базовый запрос на создание заметки. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateNoteRequest : CreateNoteRequestBase</c> и добавляет свои поля.
/// </summary>
public abstract record CreateNoteRequestBase : ICrmRequest
{
    /// <summary>Полиморфная привязка: произвольный ключ типа сущности (см. <see cref="EntityRefKeys"/>).</summary>
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }

    /// <summary>
    /// Автор заметки. На MVP приходит от клиента; после подключения Permissions заменяется
    /// значением из <c>ICurrentUser</c> — сигнатуры команд при этом не меняются.
    /// </summary>
    public Guid AuthorId { get; init; }

    public string Body { get; init; } = null!;
    public IReadOnlyList<Guid>? Mentions { get; init; }
    public IReadOnlyList<Guid>? AttachmentFileIds { get; init; }

    /// <summary>Ответ в треде: родительская заметка.</summary>
    public Guid? ParentNoteId { get; init; }
}

/// <summary>
/// Базовый запрос на редактирование заметки (Id — из маршрута). <see cref="Mentions"/> и
/// <see cref="AttachmentFileIds"/> опущенные (<c>null</c>) означают «не трогать»; чтобы очистить
/// набор — передайте пустой массив.
/// </summary>
public abstract record UpdateNoteRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
    public string Body { get; init; } = null!;
    public IReadOnlyList<Guid>? Mentions { get; init; }
    public IReadOnlyList<Guid>? AttachmentFileIds { get; init; }
}

/// <summary>Запрос «заметка по Id».</summary>
public abstract record GetNoteByIdRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
}

/// <summary>
/// Запрос «заметки сущности»: полиморфная привязка + фильтр закреплённых + постраничность.
/// Параметры страницы nullable — иначе minimal API считал бы их обязательными в query-строке;
/// опущенные значения подставляет обработчик (<see cref="NotesConstants.DefaultPageSize"/>,
/// с ограничением <see cref="NotesConstants.MaxPageSize"/>).
/// </summary>
public abstract record GetNotesByEntityRequestBase : ICrmRequest
{
    [FromQuery] public string EntityType { get; init; } = null!;
    [FromQuery] public Guid EntityId { get; init; }
    [FromQuery] public bool? PinnedOnly { get; init; }
    [FromQuery] public int? Skip { get; init; }
    [FromQuery] public int? Take { get; init; }
}

/// <summary>
/// Запрос «ветка треда» — ответы на заметку, постранично (старые сверху). Параметры страницы
/// nullable по той же причине, что и в <see cref="GetNotesByEntityRequestBase"/>.
/// </summary>
public abstract record GetNoteRepliesRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
    [FromQuery] public int? Skip { get; init; }
    [FromQuery] public int? Take { get; init; }
}

/// <summary>Базовый запрос lifecycle-операции над заметкой (закрепить/открепить/удалить).</summary>
public abstract record NoteLifecycleRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
}
