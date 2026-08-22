using Cheetah.Modules.Notes.Domain.Entities;

namespace Cheetah.Modules.Notes.Domain.Tests;

/// <summary>
/// Конкретный наследник <see cref="NoteBase"/> для проверки базового поведения. Доп. поле
/// <see cref="Visibility"/> демонстрирует расширяемость сущности.
/// </summary>
public sealed class TestNote : NoteBase
{
    public string? Visibility { get; private set; }

    private TestNote() { }

    public static TestNote Create(
        string entityType, Guid entityId, Guid authorId, string body,
        IEnumerable<Guid>? mentions = null, IEnumerable<Guid>? attachmentFileIds = null,
        Guid? parentNoteId = null)
    {
        var note = new TestNote();
        note.InitializeCore(Guid.NewGuid(), entityType, entityId, authorId, body,
            mentions, attachmentFileIds, parentNoteId);
        return note;
    }

    public void SetVisibility(string visibility) => Visibility = visibility;
}
