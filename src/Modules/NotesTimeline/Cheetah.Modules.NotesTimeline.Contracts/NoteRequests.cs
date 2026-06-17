namespace Cheetah.Modules.NotesTimeline.Contracts;

/// <summary>
/// Базовый запрос на создание заметки. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateNoteRequest : CreateNoteRequestBase</c> и добавляет свои поля.
/// </summary>
public abstract record CreateNoteRequestBase
{
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public Guid AuthorId { get; init; }
    public string Body { get; init; } = null!;
    public IReadOnlyList<Guid>? Mentions { get; init; }
    public IReadOnlyList<Guid>? AttachmentFileIds { get; init; }
    public Guid? ParentNoteId { get; init; }
}

/// <summary>Базовый запрос на редактирование тела заметки и набора упоминаний.</summary>
public abstract record UpdateNoteRequestBase
{
    public string Body { get; init; } = null!;
    public IReadOnlyList<Guid>? Mentions { get; init; }
}
