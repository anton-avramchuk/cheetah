using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.NotesTimeline.Contracts;

/// <summary>
/// Базовый ViewModel заметки (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record NoteDto : NoteDtoBase</c> и при необходимости добавляет свои поля
/// (например, <c>Visibility</c>, <c>Category</c>). Это и есть точка расширяемости ViewModel.
/// </summary>
public abstract record NoteDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public Guid AuthorId { get; init; }
    public string Body { get; init; } = null!;
    public IReadOnlyList<Guid> Mentions { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<Guid> AttachmentFileIds { get; init; } = Array.Empty<Guid>();
    public Guid? ParentNoteId { get; init; }
    public DateTimeOffset? PinnedAt { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
